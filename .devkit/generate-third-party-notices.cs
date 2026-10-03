using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// Regenerates THIRD-PARTY-NOTICES.md from what CashPrism.Shell actually
// publishes.
//
// A PackageReference scan cannot tell shipped packages from build-time-only
// ones, and neither can a plain `dotnet build`'s own deps.json: measured,
// `dotnet build` names 38 packages for CashPrism.Shell and `dotnet publish`
// 20 — the extra 18 are Microsoft.EntityFrameworkCore.Design's own Roslyn and
// MSBuild dependency chain, which `build` keeps around but a publish trims.
// Only `dotnet publish`'s deps.json is the ground truth for what a build
// actually ships, so this always publishes to a scratch directory and reads
// that. `nuget-license` (pinned in .config/dotnet-tools.json) supplies the
// licence and copyright metadata for the packages deps.json names.
//
// The publish below deliberately passes nothing that could change the resolved
// package graph — no runtime identifier, no --self-contained. The shape of the
// shipped build lives in CashPrism.Shell.csproj, so this publish resolves
// whatever the release publish resolves and cannot drift from it by carrying
// its own flags. ReadShippedPackages backs that up: it rejects any deps.json
// library type it was not written for, so making the project self-contained
// fails here — loudly — instead of silently leaving the .NET runtime's own
// licence out of a file that claims to be complete.
//
// Not everything shipped is a package. The fonts and the icon font under the
// web project's wwwroot are plain files no deps.json names, so they are listed
// by hand in the 'assets' section of the overrides file and rendered in a
// table of their own. Each entry names the files it covers, and a listed file
// that does not exist fails the run: a notice for something no longer shipped
// is as wrong as a missing one.
//
// The licence texts themselves are committed under .devkit/licenses/, one file
// per SPDX identifier, named exactly as nuget-license reports it. They are the
// canonical texts and are not edited, with one exception: MIT.txt and ISC.txt
// carry a placeholder where the canonical template puts the copyright line,
// because one text covers several entries and each entry's own notice sits in
// the table above it. A shipped package or asset under a licence with no file
// there fails rather than producing a notices file that claims to reproduce a
// licence it does not carry.
//
// Usage: dotnet run --file .devkit/generate-third-party-notices.cs [--output <path>]
//
// --output defaults to THIRD-PARTY-NOTICES.md at the repository root, i.e. this
// overwrites the real, committed file — the maintenance command to run by hand
// after a package change. `gates.sh` passes a temporary path instead, to check
// the committed file is still current without touching it.

internal static class Program
{
    private const string ShellProject = "src/CashPrism.Shell/CashPrism.Shell.csproj";
    private const string NoticesFileName = "THIRD-PARTY-NOTICES.md";
    private const string OverridesFileName = ".devkit/third-party-notices.overrides.json";
    private const string LicenseTextDirectory = ".devkit/licenses";
    private const string NotDeclared = "— not declared by the package";

    private static async Task<int> Main(string[] args)
    {
        // A maintenance command that fails says why in one line. A stack trace
        // through an async pipeline buries the sentence that names the problem.
        try
        {
            return await GenerateAsync(args);
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static async Task<int> GenerateAsync(string[] args)
    {
        if (!TryParseArgs(args, out var outputArg, out var argumentError))
        {
            Console.Error.WriteLine(argumentError);
            return 1;
        }

        var repoRoot = (await RunAsync("git", "rev-parse", "--show-toplevel")).Trim();
        var outputPath = outputArg is null ? Path.Combine(repoRoot, NoticesFileName) : Path.GetFullPath(outputArg);
        Directory.SetCurrentDirectory(repoRoot);

        await RunAsync("dotnet", "tool", "restore");

        var publishDir = Directory.CreateTempSubdirectory("cashprism-notices-");
        try
        {
            await RunAsync("dotnet", "publish", ShellProject, "-c", "Release", "-o", publishDir.FullName, "--nologo");

            var shipped = ReadShippedPackages(Path.Combine(publishDir.FullName, "CashPrism.Shell.deps.json"));
            var metadata = await ReadLicenseMetadataAsync();
            var overrides = ReadCopyrightOverrides();
            var assets = ReadAssets();

            var problems = new List<string>();

            foreach (var file in assets.SelectMany(asset => asset.Files).Where(file => !File.Exists(file)))
            {
                problems.Add($"{OverridesFileName} lists the asset file {file}, which does not exist.");
            }

            foreach (var package in shipped.Where(package => !metadata.ContainsKey(package)))
            {
                problems.Add($"No licence metadata for {package.Id} {package.Version}.");
            }

            foreach (var package in shipped.Where(package => metadata.TryGetValue(package, out var notice) && notice.License.Length == 0))
            {
                problems.Add(
                    $"{package.Id} {package.Version} declares no licence. A package whose licence is unknown "
                    + "cannot be shipped under it: establish the licence and feed it in with nuget-license's "
                    + "-override option, or drop the package.");
            }

            foreach (var id in overrides.Keys.Where(id => shipped.All(package => package.Id != id)))
            {
                problems.Add($"{OverridesFileName} overrides the copyright of {id}, which CashPrism.Shell does not ship.");
            }

            var rows = shipped
                .Where(metadata.ContainsKey)
                .Select(package => metadata[package])
                .Select(notice => notice with
                {
                    Copyright = overrides.TryGetValue(notice.Id, out var recovered) ? recovered : notice.Copyright,
                })
                .OrderBy(notice => notice.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var licenseTexts = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var licenses = rows.Select(row => row.License).Concat(assets.Select(asset => asset.License));
            foreach (var license in licenses.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var path = LicenseTextPath(license);
                if (path is null || !File.Exists(path))
                {
                    problems.Add(
                        $"No licence text for '{license}'. Add it as {LicenseTextDirectory}/{license}.txt: the "
                        + "notices file states that it reproduces every licence it names, and MIT and Apache-2.0 "
                        + "both require the text to travel with the software.");
                    continue;
                }

                licenseTexts[license] = File.ReadAllText(path).ReplaceLineEndings("\n").TrimEnd('\n');
            }

            if (problems.Count > 0)
            {
                problems.ForEach(Console.Error.WriteLine);
                return 1;
            }

            // Written through a StreamWriter with an explicit NewLine, because the
            // default is Environment.NewLine and that is CRLF on Windows, while
            // .gitattributes normalises every text file to LF. The committed file
            // would then differ from the generated one on a fresh clone, and the
            // notices gate would be red with a whole-file diff and no hint why.
            using (var writer = new StreamWriter(outputPath) { NewLine = "\n" })
            {
                Render(writer, rows, assets, licenseTexts);
            }

            Console.WriteLine(
                $"Wrote {rows.Count} packages, {assets.Count} assets and {licenseTexts.Count} licence texts to {outputPath}.");
            return 0;
        }
        finally
        {
            publishDir.Delete(recursive: true);
        }
    }

    private static bool TryParseArgs(string[] args, out string? output, out string? error)
    {
        output = null;
        error = null;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--output":
                    if (i + 1 == args.Length)
                    {
                        error = "--output needs a path.";
                        return false;
                    }

                    output = args[++i];
                    break;
                default:
                    error = $"Unknown argument: {args[i]}";
                    return false;
            }
        }

        return true;
    }

    // Accepts only the library types a framework-dependent publish produces and
    // rejects every other one rather than skipping what it does not know: a
    // self-contained publish adds the runtime pack, and its licence would
    // otherwise go unnamed with nothing saying so.
    private static IReadOnlyList<PackageIdentity> ReadShippedPackages(string depsJsonPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(depsJsonPath));

        var packages = new List<PackageIdentity>();
        foreach (var library in document.RootElement.GetProperty("libraries").EnumerateObject())
        {
            var type = library.Value.TryGetProperty("type", out var typeProperty) ? typeProperty.GetString() : null;
            if (type == "project")
            {
                continue;
            }

            if (type != "package")
            {
                throw new InvalidOperationException(
                    $"deps.json library '{library.Name}' has the unhandled type '{type ?? "(none)"}'. The publish "
                    + "shape changed; establish what that type ships, and whose licence covers it, before "
                    + "extending this generator.");
            }

            var separator = library.Name.LastIndexOf('/');
            if (separator < 0)
            {
                throw new InvalidOperationException(
                    $"deps.json library '{library.Name}' is not in the expected '<id>/<version>' form.");
            }

            packages.Add(new PackageIdentity(library.Name[..separator], library.Name[(separator + 1)..]));
        }

        return packages;
    }

    private static async Task<Dictionary<PackageIdentity, PackageNotice>> ReadLicenseMetadataAsync()
    {
        var json = await RunAsync("dotnet", "tool", "run", "nuget-license", "--", "-i", ShellProject, "-t", "-o", "Json");

        using var document = JsonDocument.Parse(json);

        var metadata = new Dictionary<PackageIdentity, PackageNotice>();
        foreach (var entry in document.RootElement.EnumerateArray())
        {
            var identity = new PackageIdentity(
                entry.GetProperty("PackageId").GetString()!,
                entry.GetProperty("PackageVersion").GetString()!);

            metadata[identity] = new PackageNotice(
                identity.Id,
                identity.Version,
                ReadString(entry, "License") ?? "",
                ReadString(entry, "Copyright") ?? NotDeclared);
        }

        return metadata;
    }

    // Copyright notices for packages that declare none. Deliberately not derived
    // from <authors>: a notice assembled out of a field that is not one reads
    // exactly like a real one, and this file exists to reproduce the real one.
    private static Dictionary<string, string> ReadCopyrightOverrides()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(OverridesFileName));

        return document.RootElement.GetProperty("copyright").EnumerateObject()
            .ToDictionary(entry => entry.Name, entry => entry.Value.GetProperty("notice").GetString()!);
    }

    private static IReadOnlyList<AssetNotice> ReadAssets()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(OverridesFileName));

        return document.RootElement.GetProperty("assets").EnumerateArray()
            .Select(entry => new AssetNotice(
                entry.GetProperty("name").GetString()!,
                entry.GetProperty("version").GetString()!,
                entry.GetProperty("license").GetString()!,
                entry.GetProperty("copyright").GetString()!,
                entry.GetProperty("files").EnumerateArray().Select(file => file.GetString()!).ToList()))
            .OrderBy(asset => asset.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? LicenseTextPath(string license) =>
        license.Length == 0 || license.AsSpan().IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            ? null
            : Path.Combine(LicenseTextDirectory, license + ".txt");

    private static void Render(
        TextWriter writer,
        IReadOnlyList<PackageNotice> rows,
        IReadOnlyList<AssetNotice> assets,
        IReadOnlyDictionary<string, string> licenseTexts)
    {
        writer.WriteLine("# Third-party notices");
        writer.WriteLine();
        writer.WriteLine("CashPrism ships the third-party packages and assets listed below. The tables");
        writer.WriteLine("name the licence each one is used under and the copyright notice it carries;");
        writer.WriteLine("the full text of every licence named is reproduced further down, as those");
        writer.WriteLine("licences require.");
        writer.WriteLine();
        writer.WriteLine("This file is generated — see `.devkit/generate-third-party-notices.cs` — and");
        writer.WriteLine("ships next to the executable produced by `dotnet publish`. Regenerate it after");
        writer.WriteLine("a package or asset change; the `notices` gate fails while it is stale.");
        writer.WriteLine();
        writer.WriteLine("## Packages");
        writer.WriteLine();
        writer.WriteLine("| Package | Version | Licence | Copyright |");
        writer.WriteLine("|---|---|---|---|");
        foreach (var row in rows)
        {
            writer.WriteLine($"| {row.Id} | {row.Version} | {row.License} | {Cell(row.Copyright)} |");
        }

        if (rows.Any(row => row.Copyright == NotDeclared))
        {
            writer.WriteLine();
            writer.WriteLine($"A copyright reading \"{NotDeclared}\" is declared neither in that package's own");
            writer.WriteLine("metadata nor in a licence file inside it, and no notice for it has been found");
            writer.WriteLine("elsewhere yet. The gap is deliberate: an approximation a reader cannot tell");
            writer.WriteLine("apart from a real notice is worse than a missing one. Notices recovered from a");
            writer.WriteLine($"project's own repository are recorded in `{OverridesFileName}`, each");
            writer.WriteLine("with the source it was copied from.");
        }

        writer.WriteLine();
        writer.WriteLine("## Assets");
        writer.WriteLine();
        writer.WriteLine("Fonts and icons served to the browser as files. Each one's own licence file");
        writer.WriteLine("ships beside it as well.");
        writer.WriteLine();
        writer.WriteLine("| Asset | Version | Licence | Copyright |");
        writer.WriteLine("|---|---|---|---|");
        foreach (var asset in assets)
        {
            writer.WriteLine($"| {asset.Name} | {asset.Version} | {asset.License} | {Cell(asset.Copyright)} |");
        }

        writer.WriteLine();
        writer.WriteLine("## Licence texts");

        foreach (var (license, text) in licenseTexts)
        {
            writer.WriteLine();
            writer.WriteLine($"### {license}");
            writer.WriteLine();
            writer.WriteLine($"Applies to every package and asset marked `{license}` in the tables above.");
            writer.WriteLine("Each of them keeps its own copyright notice, as given in those tables.");
            writer.WriteLine();
            writer.WriteLine("```text");
            foreach (var line in text.Split('\n'))
            {
                writer.WriteLine(line);
            }

            writer.WriteLine("```");
        }
    }

    // A pipe inside a value ends the table cell, so one copyright line carrying
    // one would silently break the row it sits in.
    private static string Cell(string value) => value.Replace("|", "\\|");

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;

    // Both streams are read concurrently: reading stdout to the end first
    // deadlocks as soon as the child fills the stderr pipe while this process is
    // still blocked on stdout.
    private static async Task<string> RunAsync(string fileName, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(stdout, stderr);
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'{fileName} {string.Join(' ', arguments)}' exited with {process.ExitCode}."
                + $"{Environment.NewLine}{stdout.Result}{Environment.NewLine}{stderr.Result}");
        }

        Console.Error.Write(stderr.Result);
        return stdout.Result;
    }

    private sealed record PackageIdentity(string Id, string Version);

    private sealed record PackageNotice(string Id, string Version, string License, string Copyright);

    private sealed record AssetNotice(
        string Name,
        string Version,
        string License,
        string Copyright,
        IReadOnlyList<string> Files);
}

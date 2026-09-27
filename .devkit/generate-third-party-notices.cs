using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

// Regenerates THIRD-PARTY-NOTICES.md from what CashPrism.Shell actually
// publishes.
//
// A PackageReference scan cannot tell shipped packages from build-time-only
// ones: Microsoft.EntityFrameworkCore.Design alone pulls in Roslyn and MSBuild
// packages that never leave the build machine. `dotnet publish`'s own
// deps.json is the ground truth for what ends up next to the executable, so
// this reads that instead of the project files. `nuget-license` (pinned in
// .config/dotnet-tools.json) supplies the licence and copyright metadata for
// the packages deps.json names.
//
// Usage: dotnet run .devkit/generate-third-party-notices.cs [<output file>]
//
// With no argument, overwrites THIRD-PARTY-NOTICES.md at the repository root
// — the maintenance command to run after a package changes. `gates.sh` passes
// a temporary path instead, to check the committed file is still current
// without touching it.

internal static class Program
{
    private const string ShellProject = "src/CashPrism.Shell/CashPrism.Shell.csproj";
    private const string NoticesFileName = "THIRD-PARTY-NOTICES.md";

    private static int Main(string[] args)
    {
        var repoRoot = Run("git", "rev-parse --show-toplevel").Trim();
        var outputPath = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(repoRoot, NoticesFileName);
        Directory.SetCurrentDirectory(repoRoot);

        Run("dotnet", "tool restore");

        var publishDir = Directory.CreateTempSubdirectory("cashprism-notices-");
        try
        {
            Run("dotnet", $"publish \"{ShellProject}\" -c Release -o \"{publishDir.FullName}\" --nologo");

            var depsJsonPath = Directory
                .EnumerateFiles(publishDir.FullName, "*.deps.json")
                .Single();
            var shipped = ReadShippedPackages(depsJsonPath);

            var metadata = ReadLicenseMetadata();

            var missing = shipped.Where(id => !metadata.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                Console.Error.WriteLine("No licence metadata for: " + string.Join(", ", missing.Select(m => $"{m.Id} {m.Version}")));
                return 1;
            }

            var rows = shipped
                .Select(id => metadata[id])
                .OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            File.WriteAllText(outputPath, Render(rows));
            Console.WriteLine($"Wrote {rows.Count} packages to {outputPath}.");
            return 0;
        }
        finally
        {
            publishDir.Delete(recursive: true);
        }
    }

    private static IReadOnlyList<(string Id, string Version)> ReadShippedPackages(string depsJsonPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(depsJsonPath));
        var libraries = document.RootElement.GetProperty("libraries");

        var packages = new List<(string Id, string Version)>();
        foreach (var library in libraries.EnumerateObject())
        {
            if (library.Value.GetProperty("type").GetString() != "package")
            {
                continue;
            }

            var separator = library.Name.LastIndexOf('/');
            packages.Add((library.Name[..separator], library.Name[(separator + 1)..]));
        }

        return packages;
    }

    private static Dictionary<(string Id, string Version), PackageNotice> ReadLicenseMetadata()
    {
        var json = Run(
            "dotnet",
            $"tool run nuget-license -- -i \"{ShellProject}\" -t -o Json");

        using var document = JsonDocument.Parse(json);

        var metadata = new Dictionary<(string Id, string Version), PackageNotice>();
        foreach (var entry in document.RootElement.EnumerateArray())
        {
            var id = entry.GetProperty("PackageId").GetString()!;
            var version = entry.GetProperty("PackageVersion").GetString()!;
            var license = entry.TryGetProperty("License", out var licenseProperty)
                ? licenseProperty.GetString() ?? "unknown"
                : "unknown";
            var copyright = entry.TryGetProperty("Copyright", out var copyrightProperty)
                ? copyrightProperty.GetString()
                : null;
            var authors = entry.TryGetProperty("Authors", out var authorsProperty)
                ? authorsProperty.GetString()
                : null;

            metadata[(id, version)] = new PackageNotice(
                id,
                version,
                license,
                copyright ?? (authors is null ? "" : $"Copyright (c) {authors}"));
        }

        return metadata;
    }

    private static string Render(IReadOnlyList<PackageNotice> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Third-party notices");
        builder.AppendLine();
        builder.AppendLine("CashPrism is built with the following third-party packages. Each is used");
        builder.AppendLine("under its own licence, reproduced here as that licence requires. This file is");
        builder.AppendLine("generated — see `.devkit/generate-third-party-notices.cs` — and ships next to");
        builder.AppendLine("the executable produced by `dotnet publish`.");
        builder.AppendLine();
        builder.AppendLine("| Package | Version | Licence | Copyright |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var row in rows)
        {
            builder.AppendLine($"| {row.Id} | {row.Version} | {row.License} | {row.Copyright} |");
        }

        return builder.ToString();
    }

    private static string Run(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'{fileName} {arguments}' exited with {process.ExitCode}.\n{stdout}\n{stderr}");
        }

        Console.Error.Write(stderr);
        return stdout;
    }

    private sealed record PackageNotice(string Id, string Version, string License, string Copyright);
}

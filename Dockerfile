# CashPrism as a container image, for a home server or a NAS. See
# docs/hosting.md for how to run it.
#
# The build stage always runs on the build machine's own architecture: the
# publish is framework-dependent and portable — the same `dotnet publish -c
# Release` as the release download, carrying the native SQLite library for
# every platform — so one output serves every target architecture and no
# emulation is needed. Only the runtime stage differs per architecture.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /source

COPY .editorconfig THIRD-PARTY-NOTICES.md ./
COPY src/ src/

RUN dotnet publish src/CashPrism.Shell/CashPrism.Shell.csproj -c Release -o /app

# The volume's mount point, created here because the runtime image has no
# shell. Copied with the non-root user as owner, so a fresh named volume
# inherits a directory the application can write to.
RUN mkdir /data

# -extra rather than plain chiseled: it carries ICU and the time zone database.
# Without ICU, globalisation runs invariant and the German culture the host
# pins does not exist; without the time zone database, TZ is ignored and every
# date is UTC.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra

ENV Hosting__DataDirectory=/data \
    Hosting__LaunchBrowser=false \
    ASPNETCORE_HTTP_PORTS=

COPY --from=build --chown=$APP_UID:$APP_UID /data /data
COPY --from=build /app /app

USER $APP_UID
WORKDIR /app
VOLUME /data
EXPOSE 5080

# Through the runtime, not the apphost next to the assembly: the apphost is a
# native executable for the build machine's architecture, so it would not start
# on any other.
ENTRYPOINT ["dotnet", "CashPrism.Shell.dll"]

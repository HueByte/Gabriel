# syntax=docker/dockerfile:1.7
# Build context is `src/api/` so this Dockerfile can `COPY` every project
# referenced by Gabriel.API.csproj (Core, Engine, Infrastructure) in one go.

# ---- Build stage ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj/sln first so `dotnet restore` can be cached independently
# of source changes.
COPY Gabriel.slnx ./
COPY Gabriel.API/Gabriel.API.csproj            Gabriel.API/
COPY Gabriel.Core/Gabriel.Core.csproj          Gabriel.Core/
COPY Gabriel.Engine/Gabriel.Engine.csproj      Gabriel.Engine/
COPY Gabriel.Infrastructure/Gabriel.Infrastructure.csproj Gabriel.Infrastructure/

RUN dotnet restore Gabriel.API/Gabriel.API.csproj

# Now bring in the full source and publish.
COPY . .

# SkipOpenAPI=true skips the post-Build target that emits swagger.json into
# the webapp tree; the webapp isn't part of this build context.
RUN dotnet publish Gabriel.API/Gabriel.API.csproj \
        -c Release \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false \
        /p:SkipOpenAPI=true

# ---- Runtime stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Persisted state — mounted by compose so the SQLite DB, uploaded project
# files, and rolling log files survive container rebuilds.
RUN mkdir -p /app/data /app/logs /app/projects-data \
    && useradd --uid 1000 --no-create-home --shell /usr/sbin/nologin gabriel \
    && chown -R gabriel:gabriel /app

COPY --from=build --chown=gabriel:gabriel /app/publish ./

# Bake Gabriel's docs into the image so the docs_list / docs_read tools work
# without depending on an external host mount or the GitHub-raw fallback.
# `docs` is a BuildKit named context wired up in docker-compose.yml; passing
# `--build-context docs=./docs` works for plain `docker build` invocations.
# LocalDocsLookup's default path "docs/gabriel-self-docs" then resolves
# directly under WORKDIR=/app with no further config.
COPY --from=docs --chown=gabriel:gabriel . ./docs

USER gabriel

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080
ENTRYPOINT ["dotnet", "Gabriel.API.dll"]

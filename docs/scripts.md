# Scripts

Every repeatable operation that is more than one command lives in `scripts/`. If you've typed a multi-step incantation twice, it becomes a script; once a script exists, docs reference the script, not the steps.

## `dev.ps1` — the dev environment

One command for the entire dev loop.

```powershell
scripts/dev.ps1 up                  # API (dotnet watch) + webapp (vite), each in its own window
scripts/dev.ps1 up -SkipWeb         # API only
scripts/dev.ps1 up -SkipApi         # webapp only
scripts/dev.ps1 up -Docker          # containerized stack (docker/docker-compose.yml)
scripts/dev.ps1 up -Docker -Build   # force image rebuild
scripts/dev.ps1 down                # stop whatever `up` started
scripts/dev.ps1 down -Docker        # docker compose down
scripts/dev.ps1 down -Docker -RemoveVolumes   # also wipe volumes: DB, logs, uploaded files
```

Things the script knows so you don't have to:

- The compose file lives in `docker/` but `.env` stays at the repo root, so compose is always invoked with `--env-file` (without it, `${VAR}` substitution silently falls back to defaults).
- The webapp image build `COPY`s the generated OpenAPI client, which is gitignored — run a host `dotnet build` before `up -Docker -Build` so it exists.
- `dotnet watch` holds DLL locks; run `down` before a manual `dotnet build`.

## `add-migration.ps1` — EF Core migrations

```powershell
scripts/add-migration.ps1 AddWidgetTable
```

Wraps `dotnet ef migrations add` with the `--project Gabriel.Infrastructure --startup-project Gabriel.API` pair the repo always needs. Uses the pinned `dotnet-ef` from `src/api/dotnet-tools.json` — the repo's single tool manifest, shared with the swagger CLI the OpenAPI build target uses — restored automatically, no global install needed.

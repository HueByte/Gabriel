# Gabriel

Gabriel is a chat application paired with an animated pixel-grid avatar. It combines a .NET 10 backend with a React 19 + Three.js webapp, and a standalone JavaScript prototype used to author the avatar's pulse animations.

## Repository layout

```text
Gabriel/
├── prototype/              Vanilla JS sandbox for designing 16×16 pulse animations
├── src/
│   ├── api/                .NET 10 backend solution (Gabriel.slnx)
│   │   ├── Gabriel.API/            ASP.NET Core minimal API + Swagger
│   │   ├── Gabriel.Core/           Domain entities, services, abstractions
│   │   └── Gabriel.Infrastructure/ EF Core (SQLite) + chat provider impls
│   └── webapp/             React 19 + Vite + R3F frontend (gabriel-webapp)
```

## Stack

**Backend** — .NET 10, ASP.NET Core, Entity Framework Core 10 (SQLite), Swashbuckle for OpenAPI.

**Frontend** — React 19, TypeScript 5, Vite 7, `@react-three/fiber`, `@react-three/drei`, Three.js, Axios. The API client is generated from `swagger.json` via `openapi-typescript-codegen`.

**Prototype** — Node.js scripts that generate and play back palettized 16×16 animation frames (`frames.json`).

## Getting started

### Backend

```sh
cd src/api
dotnet restore
dotnet tool restore
dotnet run --project Gabriel.API
```

The API listens on `http://localhost:5080`. On every build, `Gabriel.API.csproj` runs an `OpenAPI` MSBuild target that:

1. emits `src/webapp/src/api/swagger.json`, then
2. runs `npm run gen-api` in the webapp to regenerate the typed Axios client.

### Webapp

```sh
cd src/webapp
npm install
npm run dev
```

Available scripts:

| Script              | Purpose                                          |
| ------------------- | ------------------------------------------------ |
| `npm run dev`       | Start Vite dev server                            |
| `npm run build`     | Type-check (`tsc -b`) and produce a prod bundle  |
| `npm run preview`   | Preview the production build                     |
| `npm run typecheck` | Type-check only                                  |
| `npm run gen-api`   | Regenerate the typed API client from swagger.json|

### Prototype

```sh
cd prototype
node generate.js            # writes frames.json (random pattern)
node generate.js <pattern>  # pick a specific pattern from patterns.js
```

Open `prototype/index.html` in a browser to preview the generated animation.

## License

[MIT](LICENSE) © Gabriel contributors.

# Wolverine Report Studio

Angular frontend for managing tenant-scoped report configurations, validating Liquid templates and exporting PDF/HTML reports from Wolverine API.

## Local development

```powershell
pnpm install
pnpm start
```

Open `http://localhost:4200`. The app starts in demo mode, so the workspace can be explored without a running database or backend.

To use live data, open **API connection**, enter the Wolverine API base URL and paste a bearer token issued by the configured identity provider. The token is held in memory by the current browser session and is sent only to the configured API.

## Features

- Overview of recent report activity and export health.
- Semantic dataset catalog with tenant-safe fields.
- Report builder for identity, dataset, selected fields and filter inputs.
- Liquid template editor with safe-mode validation.
- PDF or HTML export with a demo HTML fallback when the API is not connected.
- API adapter for the report endpoints documented in the root integration guide.

## API calls

When connected, the app uses:

- `GET /api/reports/semantic-datasets`
- `POST /api/reports/configurations`
- `POST /api/reports/templates/validate`
- `POST /api/reports/configurations/{code}/execute`

The API base URL defaults to `http://localhost:5000`. For endpoint payloads and authorization requirements, see [`../docs/integration/api-integration-guide.md`](../docs/integration/api-integration-guide.md).

## Production build

```powershell
pnpm build
```

The optimized output is written to `dist/WolverineFrontend`. Configure the deployed API base URL through the connection panel or replace the default in `src/app/app.ts` as part of the deployment configuration process.

## Container image

```powershell
docker build -t wolverine-report-studio .
docker run --rm -p 8080:80 wolverine-report-studio
```

The image serves the SPA through Nginx and exposes `/healthz` for container probes.

# Repository Guidelines

## Project Structure & Module Organization

This repository is a full-stack AMR fleet operations demo. The backend lives in `backend/AmrFleet.Api` and contains ASP.NET Core controllers, SignalR hubs, models, services, and telemetry simulators. The frontend lives in `frontend/amr-fleet-ui` and uses Angular standalone components under `src/app/features`, shared models and API clients under `src/app/core`, and global styles in `src/styles.scss`. Operational documentation lives in `docs/`. Docker and CI entry points are at the repository root in `docker-compose.yml` and `.github/workflows/ci.yml`.

## Build, Test, and Development Commands

Run the API locally:

```bash
dotnet run --project backend/AmrFleet.Api/AmrFleet.Api.csproj --urls http://localhost:5000
```

Run the UI locally:

```bash
cd frontend/amr-fleet-ui
npm install
npm start
```

Build everything:

```bash
dotnet build backend/AmrFleet.Api/AmrFleet.Api.csproj
cd frontend/amr-fleet-ui && npm run build
```

Run the Docker stack:

```bash
docker compose up --build
```

The UI is served on `http://localhost:4200`; the API is on `http://localhost:5000`.

## Coding Style & Naming Conventions

Use C# file-scoped namespaces, PascalCase for public types and methods, and camelCase for locals and private fields. Keep backend behavior in services rather than controllers. Angular code should use standalone components, TypeScript interfaces for API contracts, kebab-case component folders, and concise SCSS scoped to each component. Keep UI copy operational and manufacturing-console oriented.

## Testing Guidelines

There are no dedicated test projects yet. Before submitting changes, run the backend and frontend build commands above. For safety-related changes, manually verify that `POST /api/robots/{id}/teleop-command` returns `409 Conflict` unless the robot is in `RecoveryMode`. Add focused tests when introducing shared service logic or changing recovery, mission, or incident behavior.

## Commit & Pull Request Guidelines

The current history uses concise imperative commits, for example `Create AMR fleet ops console`. Follow that style. Pull requests should include a short summary, validation commands run, screenshots for UI changes, and notes for any API, Docker, or safety-interlock changes.

## Security & Configuration Tips

Do not commit secrets, tokens, or machine-specific configuration. Keep CORS, ports, and Docker settings explicit. Recovery and tele-op controls must remain enforced in the backend, even when the UI disables unsafe actions.

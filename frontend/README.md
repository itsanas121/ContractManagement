# Contract Management Frontend

## Requirements

- Node.js 24.21.0 (24.x, 24.15.0 or later) with the bundled npm 11.19.1.
- Angular 22.2.2 (use the project-local CLI through `npx ng`).
- Angular Material 22.2.2 is the shared UI library.
- Unit tests run with Vitest, through `ng test`.

Exact package versions are recorded in `package-lock.json`.

## Install and run

From the repository root:

```bash
cd frontend
npm install
npm start
```

Open http://localhost:4200.

For a clean, lockfile-based installation, use `npm ci`.

## Build and test

```bash
npm run build
npx ng test --watch=false
```

## API configuration

Set `apiUrl` in `src/environments/environment.development.ts` to the
ASP.NET Core API address shown by the selected launch profile.

For local development, run the API with the HTTPS profile so it matches `apiUrl`
(`https://localhost:7287`):

```bash
dotnet run --project src/ContractManagement.Api --launch-profile https
```

`apiUrl` is the server origin. Endpoint paths must match Swagger.
Do not guess an `/api` prefix.

Production currently assumes a same-origin API; confirm deployment
configuration before release.

Environment files are public frontend configuration. Never put secrets,
passwords, credentials, or JWT signing keys in them.

Authentication and CORS are described in the next section.

## Authentication and CORS

- The API accepts browser requests only from the origins listed in `Cors:AllowedOrigins`
  (`src/ContractManagement.Api/appsettings.Development.json`). In development this is
  `http://localhost:4200`. Open the app through `localhost`, not `127.0.0.1`.
- Login: `POST /api/auth/login` with `{ email, password }`; the response is `{ token }`.
- `AuthService` (`core/auth`) stores the JWT in `sessionStorage`; it is cleared when the tab closes.
- `authInterceptor` adds `Authorization: Bearer <token>` only to requests sent to
  `environment.apiUrl`. On HTTP 401 it clears the session and redirects to
  `/login?returnUrl=...`. HTTP 403 keeps the session.
- `authGuard` protects a route when added to its `canActivate`. Guards are a usability
  feature only; the API enforces authorization.
- There is no login page yet. `/login` is the route the guard and interceptor redirect to.

## Structure

- `core/`: application-wide infrastructure.
- `shared/`: genuinely reusable components and utilities.
- `features/`: screens, API services, DTO types, and routes grouped by feature.

## Adding a feature

1. Work on a feature branch from updated develop.
2. Add code inside the appropriate `features/<feature>/` folder.
3. Add a feature route file when the feature has a screen.
4. Register its lazy route in `app.routes.ts`.
5. Import Angular Material components locally where needed.
6. Import `environment` for the API origin; do not hard-code it in services.
7. Keep unrelated feature files unchanged.
8. Build, test, and open a PR into develop.

Coordinate edits to app.routes.ts and app.config.ts in the team group.
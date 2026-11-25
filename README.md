# SitesLend

IT asset borrowing and inventory system (OJT project GT-OJT-01). Training project: use synthetic data only.

Status: runnable skeleton (task A03). There is no login and there are no features yet. The home page calls `GET /api/health` and shows whether the API and its database are reachable.

## Folders

| Folder | What it holds |
|---|---|
| `api/` | ASP.NET Core Web API (.NET 10), EF Core with Npgsql |
| `api.Tests/` | xUnit tests using WebApplicationFactory |
| `client/` | Angular 22 app (strict mode) with Tailwind CSS 4 |
| `docs/` | Design docs, `db-setup.sql` (optional limited database user), AI use log |
| `SitesLend.slnx` | Solution file for `api/` and `api.Tests/` |

Rules for AI coding agents are in `AGENTS.md` and `CLAUDE.md`.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` should start with `10.`)
- Node.js `^22.22.3`, `^24.15.0` or `>=26` with npm 8 or newer (Angular 22 requirement)
- PostgreSQL 15 or newer, and pgAdmin (the setup was tested on PostgreSQL 18)
- The HTTPS dev certificate trusted on your machine: run `dotnet dev-certs https --trust` once

## Setup from a fresh clone

### 1. Create the database

In pgAdmin, connected to your local PostgreSQL server as `postgres`, create an empty database named `siteslend`. In the Query Tool you can run:

```sql
CREATE DATABASE siteslend;
```

This setup connects as the `postgres` superuser, which is fine for a local training machine and never for a shared or deployed server. For a safer setup with a limited `siteslend_app` user that has rights only on `siteslend`, follow `docs/db-setup.sql` instead and use `Username=siteslend_app` (with its password) in step 2.

### 2. Give the API its connection string (user-secrets)

The connection string is never stored in a committed file. Store it in .NET user-secrets, which keeps it in your user profile outside the repository:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=siteslend;Username=postgres" --project api
```

If your `postgres` user has a password, add `;Password=YOUR_PASSWORD` to the end of the connection string. Do not commit the password.

`api/appsettings.Example.json` shows the shape of the setting with placeholder values. It is documentation only and is never loaded.

### 3. Install client packages

```bash
cd client
npm ci
```

## Run

Use two terminals.

```bash
# Terminal 1: the API (https://localhost:7029)
dotnet run --project api --launch-profile https
```

```bash
# Terminal 2: the client (http://localhost:4200)
cd client
npm start
```

Open <http://localhost:4200>. The Angular dev server forwards every `/api` request to the API (`client/proxy.conf.json`), so the browser only talks to one origin. This is what lets us use cookie login later without CORS.

What you should see on the home page:

| Result | Meaning |
|---|---|
| API Running, Database Connected | Everything works. |
| API Running, Database Not connected | The API is up but cannot reach PostgreSQL. Check that PostgreSQL is running and the user-secret is set (step 2). |
| "Can't reach the API" | The API is not running. Start terminal 1. |

You can also check the API directly: `https://localhost:7029/api/health` returns `{"status":"ok","database":"up"}`.

## Test, lint and format

| Purpose | Command (run from the repository root unless noted) |
|---|---|
| Build the API | `dotnet build` |
| Test the API | `dotnet test` |
| Check API formatting | `dotnet format --verify-no-changes` |
| Fix API formatting | `dotnet format` |
| Build the client | `cd client && npm run build` |
| Test the client | `cd client && npm test` |
| Lint the client | `cd client && npm run lint` |

`dotnet test` needs the database from steps 1 and 2 for one test (`Get_health_returns_ok_and_database_up`). It uses the same user-secret as the API. The other API test and all client tests need no database.

## Troubleshooting

- **Home page says "Can't reach the API", but the API is running.** Run the API with `--launch-profile https`. The dev proxy targets `https://localhost:7029`.
- **Browser warns about the certificate.** Run `dotnet dev-certs https --trust` and restart the browser.
- **`database: "down"` although PostgreSQL is running.** Check that the user-secret exists and matches the user (`postgres`) and password of your PostgreSQL server: `dotnet user-secrets list --project api` (this prints the value, so don't share your screen).
- **`role "…" does not exist`.** The `Username` in the user-secret is misspelled or not a real role. It should be `postgres`. Set the secret again (step 2).
- **"password authentication failed".** The password in the user-secret differs from the one your `postgres` user has, or your server needs a password and the secret has none. Set the secret again (step 2).
- **`database "siteslend" does not exist`.** Create it (step 1).
- **To see the exact reason a connection failed,** run `dotnet test --filter "FullyQualifiedName~database_up" --logger "console;verbosity=detailed"` and look for the `Npgsql.PostgresException` line. It never contains a password.

## Rules for contributors

- Never commit secrets, connection strings or real personal data.
- Schema changes happen only through EF Core migrations.
- Commits and pushes are done by a human after reviewing the diff.

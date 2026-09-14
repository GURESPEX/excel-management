# Infra — Compose Orchestration

Brings up the full stack (frontend + backend + PostgreSQL) from the images published to GHCR by the project's CI pipelines. No application source code lives here.

## Run it

```bash
docker compose up
```

Then open **http://localhost:5173** in a browser. The API listens on **http://localhost:5289**.

The images are tagged `ghcr.io/gurespex/excel-management-api:latest` and `ghcr.io/gurespex/excel-management-web:latest`, published by `.github/workflows/backend-ci.yml` and `frontend-ci.yml` on every push. If the GHCR packages are private, authenticate first:

```bash
docker login ghcr.io -u <your-github-username>
```

## What it does

- **postgres** — Postgres 17, a named volume for durable storage, seeded via the backend's own EF Core migrations (which run automatically on API startup).
- **api** — the backend image, wired to `postgres` via `Database__Provider=Postgres` and `ConnectionStrings__Default` (both standard ASP.NET Core config-key env vars), waits for Postgres to report healthy before starting.
- **web** — the frontend image (nginx-served static build), published at port 5173 to match the backend's CORS allow-list, which is currently fixed to `http://localhost:5173`.

Seeded accounts (from the backend's `SeedData`): `admin`/`admin123` (Admin), `viewer`/`viewer123` (Viewer).

To stop and remove everything, including the Postgres volume:

```bash
docker compose down -v
```

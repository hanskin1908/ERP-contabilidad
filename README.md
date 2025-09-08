# ERP Contable (MVP)

Este repositorio contiene el diseño técnico inicial para un MVP de un sistema contable orientado al PUC colombiano, con backend en .NET 8 + PostgreSQL y frontend en React.

- Objetivo: Registrar transacciones, manejar PUC, terceros e informes básicos (PyG y Balance General).
- Alcance: MVP funcional con autenticación, catálogo de cuentas, asientos contables, terceros, facturas básicas y reportes.
- Stack sugerido: .NET 8 (Web API) + EF Core + PostgreSQL; React + Tailwind; Auth con JWT.

Estructura principal de documentación:

- `docs/01-requerimientos.md`
- `docs/02-arquitectura.md`
- `docs/03-modelo-datos.md`
- `docs/04-api-spec.md`
- `docs/05-backlog-mvp.md`
- `docs/06-devops.md`
- `docs/07-reportes.md`
- `docs/08-glosario.md`

Consulta el backlog (`docs/05-backlog-mvp.md`) para el plan de entrega por iteraciones.

## Despliegue con un clic (gratuito)

Backend (API + Postgres) en Render:

- Botón “Deploy to Render” (usa `render.yaml`). Antes, sube/forkea este repo a GitHub y reemplaza `<REPO_URL>` con la URL pública de tu fork.

  - https://render.com/deploy?repo=<REPO_URL>

  Variables ya definidas en `render.yaml`:
  - `DB__CONNECTION` se inyecta desde la base gratuita creada por Render.
  - `JWT__KEY` se autogenera. Endpoint de health: `/health`.

Frontend (Vite) en Netlify o Vercel:

- Netlify (usa `web/netlify.toml`):
  1) Conecta el repo → base “web” → Netlify detecta Vite.
  2) Variable: `VITE_API_BASE = https://<TU-API-RENDER>.onrender.com/api`

- Vercel (usa `vercel.json`):
  1) Importa el repo en Vercel.
  2) Variables del proyecto: `VITE_API_BASE = https://<TU-API-RENDER>.onrender.com/api`
  3) Build estático vía `@vercel/static-build` (salida en `web/dist`).

Notas
- Si quieres probar rápido desde tu máquina: `npx localtunnel --port 5000` para API y `VITE_API_BASE=<url del túnel>/api npm run dev` en `web/`.

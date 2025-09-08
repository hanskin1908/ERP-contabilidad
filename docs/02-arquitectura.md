# Arquitectura Propuesta (MVP)

## Stack

- Backend: C# .NET 7 (ASP.NET Core Web API), EF Core, Npgsql.
- Base de datos: PostgreSQL.
- Frontend: React + Vite + TypeScript; Tailwind CSS.
- Auth: JWT; contraseñas con bcrypt/ASP.NET Identity PasswordHasher.
- Validación: FluentValidation (o validadores de ASP.NET Core).
- Logging: Serilog (archivo/console) y correlación de requests.

## Capas y módulos

- API: controladores REST, DTOs, validación de entrada, mapeo (AutoMapper).
- Aplicación: casos de uso/servicios (p. ej., PostAsiento, AprobarFactura).
- Dominio: entidades (Cuenta, Asiento, Movimiento, Tercero, Factura), reglas (doble partida, naturaleza).
- Infraestructura: DbContext, repositorios, migraciones, proveedores (PDF/Excel), email (opcional).

Módulos funcionales (en una sola solución para el MVP):
- Contabilidad (PUC, asientos, reportes).
- Terceros.
- Facturación básica (ventas/proveedores) con contabilización.
- Seguridad (usuarios/roles, JWT).

## Principios

- Regla de negocio en la capa de dominio/aplicación, no en controladores.
- Idempotencia para comandos sensibles (p. ej., aprobar factura).
- Validaciones de integridad fuertes en BD (FK, unique, checks cuando aplique) + validación en app.
- Soft-delete/inactivación donde aplique; auditoría mínima (created_at/updated_at/created_by).

## Estructura propuesta (backend)

- `src/Api` (controladores, DTOs, mapeos)
- `src/Application` (servicios/casos de uso, validadores)
- `src/Domain` (entidades, enums, contratos)
- `src/Infrastructure` (EF DbContext, repos, migraciones, Npgsql)
- `tests/` (unitarios de dominio/aplicación)

## Estructura propuesta (frontend)

- `src/app` (routing, layouts, providers)
- `src/features` (auth, cuentas, asientos, terceros, facturas, reportes)
- `src/shared` (ui, hooks, utils, api-client)

## Seguridad

- JWT con expiración razonable y refresh tokens opcional (post-MVP).
- Roles básicos: `admin`, `operador`.
- Registros de auditoría de acciones críticas (post-MVP: logs a tabla dedicada).

## Escalabilidad futura

- Separación por módulos para migrar a microservicios si es necesario (facturación electrónica, nómina, inventario como servicios aparte).
- Cache de catálogos «read-mostly» (p. ej. cuentas) en capa de aplicación si se requiere.

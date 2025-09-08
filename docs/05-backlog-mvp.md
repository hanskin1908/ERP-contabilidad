# Backlog MVP y Plan de Entrega

## Hitos (2–3 semanas cada uno)

1) Fundaciones (Sprint 0)
- Repo, linters, plantillas PR, CI (build/test) y Docker Compose (Postgres).
- API .NET esqueleto + Healthcheck; Vite React esqueleto + Auth layout.
- Entorno `.env` y variables seguras.

2) Autenticación y seguridad
- Registro/login JWT, roles `admin`/`operador`.
- Gestión mínima de usuarios.
- Criterios: no se accede a endpoints sin JWT; pruebas unitarias básicas.

3) PUC (Catálogo de Cuentas)
- CRUD de cuentas con jerarquía, naturaleza y `is_postable`.
- Semilla PUC base (clases 1–9) y ejemplo de subcuentas.
- Criterios: unicidad `code` por empresa; no borrar si tiene movimientos (solo inactivar).

4) Terceros
- CRUD de clientes/proveedores; validación de NIT.
- Criterios: unicidad NIT por empresa; inactivación.

5) Asientos contables
- Crear/editar asientos en borrador; publicar (`POSTED`).
- Reglas: doble partida, cuentas posteables, consecutivos por tipo.
- Diario y Mayor (consultas + CSV/Excel).

6) Facturas básicas
- Ventas y compras en borrador → aprobar = contabilizar.
- Criterios: control de estados; vínculo con `journal_entry`.

7) Estados financieros
- PyG y Balance General (fechas/rango; layout estándar simple).
- Exportaciones: CSV/Excel (PDF opcional si hay tiempo).

## Historias de usuario (selección)

- Como operador, puedo autenticarme para acceder a la aplicación.
  - Aceptación: login correcto devuelve JWT; accesos sin token devuelven 401.
- Como contador, puedo crear cuentas PUC y marcarlas posteables o no.
  - Aceptación: no permite duplicar código; no permite mover a no posteable si tiene movimientos.
- Como usuario, puedo crear un asiento con múltiples líneas y publicarlo.
  - Aceptación: no publica si débitos != créditos; asigna consecutivo por tipo.
- Como analista, puedo ver el Libro Mayor y exportarlo a CSV.
  - Aceptación: columnas: fecha, número, tercero, detalle, débito, crédito, saldo.
- Como facturador, puedo aprobar una factura de venta para que quede contabilizada.
  - Aceptación: al aprobar se genera `journal_entry` balanceado; estados consistentes.

## Criterios de calidad

- Cobertura de pruebas de dominio (reglas contables) y servicios clave.
- Validación y manejo de errores coherente (400/409/422).
- Migraciones reproducibles; datos semilla mínimos.
- Observabilidad básica (logs estructurados).

## Riesgos y mitigaciones

- PUC: complejidad de niveles → restringir MVP a 4–6 niveles prácticos.
- Reportes: performance → usar consultas agregadas con índices; paginación.
- Facturas: impuestos → mantener simple (sin IVA) en MVP y dejar extensiones.

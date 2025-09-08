# API REST (MVP)

Base: `/api`

## Autenticación

- POST `/auth/register` (solo admin inicial)
  - body: `{ email, fullName, password, role }`
  - resp: `{ id, email, role, token }`
- POST `/auth/login`
  - body: `{ email, password }`
  - resp: `{ token, user: { id, email, role, fullName } }`
- GET `/auth/me` (JWT)
  - resp: `{ id, email, role, fullName }`

## Catálogo de cuentas (PUC)

- GET `/accounts?search=&onlyPostable=&active=`
  - resp: `[{ id, code, name, level, nature, parentId, isPostable, isActive }]`
- GET `/accounts/{id}`
- POST `/accounts`
  - body: `{ code, name, level, nature, parentId?, isPostable }`
- PUT `/accounts/{id}`
- PATCH `/accounts/{id}/activate` | `/deactivate`

## Terceros

- GET `/third-parties?search=&type=&active=`
- GET `/third-parties/{id}`
- POST `/third-parties`
  - body: `{ nit, dv?, tipo, razonSocial, direccion?, email?, telefono? }`
- PUT `/third-parties/{id}`
- PATCH `/third-parties/{id}/activate` | `/deactivate`

## Asientos contables

- GET `/journal-entries?from=&to=&type=&thirdPartyId=&q=`
  - resp: `[{ id, number, type, date, status, totalDebit, totalCredit }]`
- GET `/journal-entries/{id}`
  - resp: `{ id, number, type, date, description, status, lines: [{ id, accountId, description, debit, credit, thirdPartyId? }] }`
- POST `/journal-entries`
  - body:
    ```json
    {
      "type": "DIARIO",
      "date": "2025-01-31",
      "description": "Ajuste fin de mes",
      "thirdPartyId": null,
      "lines": [
        {"accountId": 1001, "debit": 100000, "credit": 0, "description": "Caja"},
        {"accountId": 4135, "debit": 0, "credit": 100000, "description": "Ingresos"}
      ]
    }
    ```
  - reglas: suma(debit) = suma(credit); cuentas posteables; no permitir 0 en ambas columnas.
- PUT `/journal-entries/{id}` (solo si `status=DRAFT`)
- POST `/journal-entries/{id}/post` (cambiar a `POSTED`)
- POST `/journal-entries/{id}/void` (anulación con contrapartida o marca, definido por política)

## Facturas (básicas)

- GET `/invoices?type=&status=&from=&to=&q=`
- GET `/invoices/{id}`
- POST `/invoices`
  - body:
    ```json
    {
      "type": "SALE",
      "thirdPartyId": 200,
      "issueDate": "2025-01-31",
      "dueDate": "2025-02-15",
      "currency": "COP",
      "lines": [
        {"itemName": "Servicio X", "quantity": 1, "unitPrice": 100000, "taxRate": 0, "accountId": 4135}
      ]
    }
    ```
- PUT `/invoices/{id}` (si `status=DRAFT`)
- POST `/invoices/{id}/approve` → genera `journal_entry` asociado
- POST `/invoices/{id}/cancel` (si no contabilizada o según política)

## Reportes

- GET `/reports/trial-balance?from=&to=` (Balance de comprobación)
- GET `/reports/ledger?accountId=&from=&to=` (Libro Mayor)
- GET `/reports/income-statement?from=&to=` (PyG)
- GET `/reports/balance-sheet?asOf=` (Balance General)
- Descargas: `Accept: text/csv` o `application/pdf` (si se implementa PDF en MVP)

## Adjuntos

- POST `/attachments` (multipart) `{ entityType, entityId, file }`
- GET `/attachments?entityType=&entityId=`

## Errores (estándar)

- 400: Validaciones.
- 401/403: Autenticación/permiso.
- 404: No encontrado.
- 409: Conflicto (duplicados, estados no válidos).
- 422: Reglas contables (doble partida, cuenta no posteable, etc.).

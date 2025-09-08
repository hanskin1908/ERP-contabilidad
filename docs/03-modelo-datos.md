# Modelo de Datos (MVP)

## Entidades principales

- Empresa (`company`): opcional en MVP (pensado para multi‑empresa).
  - `id`, `nombre`, `nit`, `is_active`.
- Usuario (`app_user`): auth básica por empresa.
  - `id`, `company_id`, `email` (unique), `nombre`, `role`, `password_hash`, `is_active`.
- Tercero (`third_party`): clientes y proveedores.
  - `id`, `company_id`, `nit`, `dv`, `tipo` (cliente|proveedor|ambos|otro), `razon_social`, `direccion`, `email`, `telefono`, `is_active`.
- Cuenta (`account`): PUC.
  - `id`, `company_id`, `code`, `name`, `level`, `nature` (D|C), `parent_id`, `is_postable`, `is_active`.
- Asiento (`journal_entry`): comprobante contable.
  - `id`, `company_id`, `number`, `type` (INGRESO|EGRESO|AJUSTE|DIARIO), `date`, `description`, `third_party_id?`, `document_type?`, `document_number?`, `status` (DRAFT|POSTED|VOID), `total_debit`, `total_credit`, `created_by`.
- Movimiento (`journal_line`): detalle del asiento.
  - `id`, `journal_entry_id`, `account_id`, `description?`, `debit`, `credit`, `third_party_id?`, `category?`.
- Factura (`invoice`): ventas o compras (proveedores).
  - `id`, `company_id`, `type` (SALE|PURCHASE), `number`, `third_party_id`, `issue_date`, `due_date?`, `currency` (COP), `subtotal`, `tax_total`, `total`, `status` (DRAFT|APPROVED|CANCELLED), `journal_entry_id?`.
- Línea de factura (`invoice_line`):
  - `id`, `invoice_id`, `item_name`, `quantity`, `unit_price`, `discount?`, `tax_rate?`, `account_id`, `total`.
- Adjunto (`attachment`): archivos asociados a comprobantes/facturas.
  - `id`, `entity_type`, `entity_id`, `file_name`, `content_type`, `url`.

Notas:
- Montos en `numeric(18,2)`.
- Naturaleza de cuenta `D` (Débito) o `C` (Crédito).
- Solo cuentas con `is_postable=true` admiten movimientos.
- Doble partida verificada en la aplicación (opcional: trigger en BD en fases futuras).

## Índices y unicidad

- `account(code)` único por `company_id`.
- `journal_entry(number,type)` único por `company_id`.
- `third_party(nit)` único por `company_id`.
- `invoice(number,type)` único por `company_id`.

## Esquema SQL (ver `db/schema.sql`)

Incluye DDL para PostgreSQL con claves foráneas y restricciones básicas.

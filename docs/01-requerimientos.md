# Requerimientos

## MVP (Producto Mínimo Viable)

- Registro de transacciones: creación/edición de asientos contables (ingresos, egresos, ajustes) con doble partida.
- Comprobantes básicos: consecutivos por tipo (ingreso, egreso, diario/ajuste).
- Catálogo de cuentas (PUC): alta/baja/modificación, niveles, naturaleza (Débito/Crédito), inactivación.
- Control de ingresos/egresos por categorías: asociar a cuentas del PUC o etiquetas operativas.
- Terceros (clientes/proveedores): NIT, razón social, dirección y datos mínimos; asociar a transacciones.
- Facturas: registro básico de ventas y proveedores; estado (borrador, aprobado, anulado) y contabilización al aprobar.
- Estados financieros: PyG y Balance General; Libro Diario y Libro Mayor.
- Exportaciones: visualización y export a PDF/Excel (al menos CSV/Excel en MVP; PDF opcional si hay tiempo).
- Accesibilidad y usabilidad: interfaz simple; filtros por fecha y categoría.
- Autenticación/autorización: JWT, roles básicos (admin, operador).
- Moneda local COP: montos con `numeric(18,2)` y redondeos coherentes.

## Requerimientos avanzados (Roadmap a empresa)

- Tributario: IVA, retención en la fuente, ICA, reteIVA; certificados; FE DIAN.
- Nómina: cálculo completo (salud, pensión, ARL), liquidaciones, PILA.
- Inventarios: existencias, costos, kardex; integración con ventas/OC.
- Presupuestos y proyecciones: por período o proyecto; comparativos real vs presupuesto.
- Analítica/BI: KPIs (EBITDA, ROI, márgenes), tableros.
- Automatización: cargas masivas (CSV/Excel), plantillas de asientos recurrentes.
- Integraciones: API REST/GraphQL; webhooks/colas para eventos contables; bancos/CRM/e‑commerce.
- Seguridad/Auditoría: roles finos, trazabilidad, logs de auditoría, cifrado de datos sensibles.
- Arquitectura escalable: modular/microservicios; despliegue cloud; Docker/Kubernetes.
- Multi‑empresa y multi‑moneda.
- UX avanzada: responsive, paneles personalizables.
- Cumplimiento normativo: formatos exigidos por DIAN/supersociedades; actualizaciones normativas.

## Reglas clave de negocio (MVP)

- Doble partida: la suma de débitos debe igualar la suma de créditos por asiento.
- Naturaleza de cuentas: validaciones de débito/crédito según naturaleza; no permitir movimientos en cuentas no posteables (no hoja).
- Consecutivos por tipo de comprobante y por empresa.
- Periodicidad: soporte para filtros por período; opción de cerrar periodos (bloqueo de edición) en roadmap cercano.
- Integridad: no permitir anular/eliminar asientos aprobados sin contrapartida; soft-delete donde aplique (inactivar cuentas/terceros).
- Montos: usar `numeric(18,2)`; evitar flotantes; definición central de reglas de redondeo.

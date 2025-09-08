-- PostgreSQL DDL para MVP contable (PUC colombiano)
-- Nota: ajustar nombres de esquema si se requiere (por defecto: public)

CREATE TABLE IF NOT EXISTS company (
  id           BIGSERIAL PRIMARY KEY,
  name         VARCHAR(200) NOT NULL,
  nit          VARCHAR(30),
  is_active    BOOLEAN NOT NULL DEFAULT TRUE,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS app_user (
  id            BIGSERIAL PRIMARY KEY,
  company_id    BIGINT NOT NULL REFERENCES company(id),
  email         VARCHAR(200) NOT NULL,
  full_name     VARCHAR(200),
  role          VARCHAR(50) NOT NULL DEFAULT 'operador',
  password_hash TEXT NOT NULL,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(company_id, email)
);

CREATE TABLE IF NOT EXISTS third_party (
  id            BIGSERIAL PRIMARY KEY,
  company_id    BIGINT NOT NULL REFERENCES company(id),
  nit           VARCHAR(30) NOT NULL,
  dv            VARCHAR(2),
  tipo          VARCHAR(20) NOT NULL DEFAULT 'otro', -- cliente|proveedor|ambos|otro
  razon_social  VARCHAR(200) NOT NULL,
  direccion     VARCHAR(200),
  email         VARCHAR(200),
  telefono      VARCHAR(50),
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(company_id, nit)
);

CREATE TABLE IF NOT EXISTS account (
  id            BIGSERIAL PRIMARY KEY,
  company_id    BIGINT NOT NULL REFERENCES company(id),
  code          VARCHAR(20) NOT NULL,
  name          VARCHAR(200) NOT NULL,
  level         SMALLINT NOT NULL,
  nature        CHAR(1) NOT NULL CHECK (nature IN ('D','C')),
  parent_id     BIGINT REFERENCES account(id),
  is_postable   BOOLEAN NOT NULL DEFAULT FALSE,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(company_id, code)
);

CREATE TABLE IF NOT EXISTS journal_entry (
  id              BIGSERIAL PRIMARY KEY,
  company_id      BIGINT NOT NULL REFERENCES company(id),
  number          BIGINT NOT NULL,
  type            VARCHAR(20) NOT NULL, -- INGRESO|EGRESO|AJUSTE|DIARIO
  date            DATE NOT NULL,
  description     VARCHAR(500),
  third_party_id  BIGINT REFERENCES third_party(id),
  document_type   VARCHAR(50),
  document_number VARCHAR(50),
  status          VARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT|POSTED|VOID
  total_debit     NUMERIC(18,2) NOT NULL DEFAULT 0,
  total_credit    NUMERIC(18,2) NOT NULL DEFAULT 0,
  created_by      BIGINT REFERENCES app_user(id),
  created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(company_id, type, number)
);

CREATE TABLE IF NOT EXISTS journal_line (
  id                BIGSERIAL PRIMARY KEY,
  journal_entry_id  BIGINT NOT NULL REFERENCES journal_entry(id) ON DELETE CASCADE,
  account_id        BIGINT NOT NULL REFERENCES account(id),
  description       VARCHAR(500),
  debit             NUMERIC(18,2) NOT NULL DEFAULT 0,
  credit            NUMERIC(18,2) NOT NULL DEFAULT 0,
  third_party_id    BIGINT REFERENCES third_party(id),
  created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CHECK (debit >= 0 AND credit >= 0),
  CHECK (NOT (debit = 0 AND credit = 0))
);

CREATE INDEX IF NOT EXISTS ix_journal_line_account ON journal_line(account_id);
CREATE INDEX IF NOT EXISTS ix_journal_line_journal ON journal_line(journal_entry_id);

CREATE TABLE IF NOT EXISTS invoice (
  id              BIGSERIAL PRIMARY KEY,
  company_id      BIGINT NOT NULL REFERENCES company(id),
  type            VARCHAR(20) NOT NULL, -- SALE|PURCHASE
  number          BIGINT NOT NULL,
  third_party_id  BIGINT NOT NULL REFERENCES third_party(id),
  issue_date      DATE NOT NULL,
  due_date        DATE,
  currency        VARCHAR(10) NOT NULL DEFAULT 'COP',
  subtotal        NUMERIC(18,2) NOT NULL DEFAULT 0,
  tax_total       NUMERIC(18,2) NOT NULL DEFAULT 0,
  total           NUMERIC(18,2) NOT NULL DEFAULT 0,
  status          VARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- DRAFT|APPROVED|CANCELLED
  journal_entry_id BIGINT REFERENCES journal_entry(id),
  notes           VARCHAR(500),
  created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(company_id, type, number)
);

CREATE TABLE IF NOT EXISTS invoice_line (
  id           BIGSERIAL PRIMARY KEY,
  invoice_id   BIGINT NOT NULL REFERENCES invoice(id) ON DELETE CASCADE,
  item_name    VARCHAR(200) NOT NULL,
  quantity     NUMERIC(18,2) NOT NULL DEFAULT 1,
  unit_price   NUMERIC(18,2) NOT NULL DEFAULT 0,
  discount     NUMERIC(18,2) NOT NULL DEFAULT 0,
  tax_rate     NUMERIC(5,2) NOT NULL DEFAULT 0, -- porcentaje
  account_id   BIGINT NOT NULL REFERENCES account(id),
  total        NUMERIC(18,2) NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS attachment (
  id            BIGSERIAL PRIMARY KEY,
  entity_type   VARCHAR(50) NOT NULL, -- e.g., 'journal_entry','invoice'
  entity_id     BIGINT NOT NULL,
  file_name     VARCHAR(255) NOT NULL,
  content_type  VARCHAR(100),
  url           VARCHAR(500) NOT NULL,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Índices útiles
CREATE INDEX IF NOT EXISTS ix_account_parent ON account(parent_id);
CREATE INDEX IF NOT EXISTS ix_journal_entry_date ON journal_entry(date);
CREATE INDEX IF NOT EXISTS ix_invoice_third_party ON invoice(third_party_id);


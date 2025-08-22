CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
CREATE TABLE email_records (
    id uuid NOT NULL,
    "from" text NOT NULL,
    "to" text NOT NULL,
    cc text[] NOT NULL,
    bcc text[] NOT NULL,
    body text NOT NULL,
    subject text NOT NULL,
    date_created timestamp with time zone NOT NULL,
    date_sent timestamp with time zone,
    failure_reason text,
    CONSTRAINT pk_email_records PRIMARY KEY (id)
);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20250820000113_Initial', '9.0.7');

COMMIT;


CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'JobService') THEN
        CREATE SCHEMA "JobService";
    END IF;
END $EF$;

CREATE TABLE "JobService"."Jobs" (
    id uuid NOT NULL,
    tenant_id uuid NOT NULL,
    customer_id uuid,
    customer_name character varying(200) NOT NULL,
    service_address text NOT NULL,
    status character varying(50) NOT NULL,
    priority character varying(50) NOT NULL,
    description character varying(2000) NOT NULL,
    special_instructions character varying(1000),
    estimated_duration bigint NOT NULL,
    estimated_cost text NOT NULL,
    actual_cost text,
    requested_date timestamp with time zone NOT NULL,
    scheduled_date timestamp with time zone,
    completed_date timestamp with time zone,
    assigned_technician_id uuid,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT pk_jobs PRIMARY KEY (id),
    CONSTRAINT "CK_Jobs_CompletedDate_Future" CHECK ("completed_date" IS NULL OR "completed_date" >= "created_at"),
    CONSTRAINT "CK_Jobs_ScheduledDate_Future" CHECK ("scheduled_date" IS NULL OR "scheduled_date" >= "created_at")
);
COMMENT ON COLUMN "JobService"."Jobs".customer_name IS 'Name of the customer for this job';
COMMENT ON COLUMN "JobService"."Jobs".description IS 'Detailed description of the work to be performed';
COMMENT ON COLUMN "JobService"."Jobs".special_instructions IS 'Special instructions from customer or management';
COMMENT ON COLUMN "JobService"."Jobs".requested_date IS 'Date requested by customer';
COMMENT ON COLUMN "JobService"."Jobs".scheduled_date IS 'Date scheduled for technician';
COMMENT ON COLUMN "JobService"."Jobs".completed_date IS 'Date job was completed';
COMMENT ON COLUMN "JobService"."Jobs".created_at IS 'Timestamp when job was created';
COMMENT ON COLUMN "JobService"."Jobs".updated_at IS 'Timestamp when job was last updated';

CREATE TABLE "JobService"."JobNotes" (
    id uuid NOT NULL,
    author character varying(200) NOT NULL,
    content character varying(2000) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    job_id uuid NOT NULL,
    CONSTRAINT pk_job_notes PRIMARY KEY (id),
    CONSTRAINT "FK_JobNotes_Job" FOREIGN KEY (job_id) REFERENCES "JobService"."Jobs" (id) ON DELETE CASCADE
);
COMMENT ON COLUMN "JobService"."JobNotes".author IS 'Author of the note (technician, customer, system, etc.)';
COMMENT ON COLUMN "JobService"."JobNotes".content IS 'Content of the note';
COMMENT ON COLUMN "JobService"."JobNotes".created_at IS 'Timestamp when note was created';
COMMENT ON COLUMN "JobService"."JobNotes".job_id IS 'Foreign key to the Job';

CREATE TABLE "JobService"."JobServiceItems" (
    id uuid NOT NULL,
    service_name character varying(200) NOT NULL,
    quantity integer NOT NULL DEFAULT 1,
    comment character varying(500),
    price numeric(7,2) NOT NULL,
    is_fulfilled boolean NOT NULL DEFAULT FALSE,
    job_id uuid NOT NULL,
    CONSTRAINT pk_job_service_items PRIMARY KEY (id),
    CONSTRAINT "FK_JobRequirements_Job" FOREIGN KEY (job_id) REFERENCES "JobService"."Jobs" (id) ON DELETE CASCADE
);
COMMENT ON COLUMN "JobService"."JobServiceItems".service_name IS 'Name of the service provided';
COMMENT ON COLUMN "JobService"."JobServiceItems".quantity IS 'Quantity of service provided';
COMMENT ON COLUMN "JobService"."JobServiceItems".comment IS 'Optional comment about the service';
COMMENT ON COLUMN "JobService"."JobServiceItems".price IS 'Price per unit of service';
COMMENT ON COLUMN "JobService"."JobServiceItems".is_fulfilled IS 'Whether this service has been fulfilled';
COMMENT ON COLUMN "JobService"."JobServiceItems".job_id IS 'Foreign key to the Job';

CREATE INDEX "IX_JobNotes_Author" ON "JobService"."JobNotes" (author);

CREATE INDEX "IX_JobNotes_CreatedAt" ON "JobService"."JobNotes" (created_at);

CREATE INDEX "IX_JobNotes_JobId" ON "JobService"."JobNotes" (job_id);

CREATE INDEX "IX_Jobs_AssignedTechnicianId" ON "JobService"."Jobs" (assigned_technician_id);

CREATE INDEX "IX_Jobs_CreatedAt" ON "JobService"."Jobs" (created_at);

CREATE INDEX "IX_Jobs_CustomerId" ON "JobService"."Jobs" (customer_id);

CREATE INDEX "IX_Jobs_Priority" ON "JobService"."Jobs" (priority);

CREATE INDEX "IX_Jobs_RequestedDate" ON "JobService"."Jobs" (requested_date);

CREATE INDEX "IX_Jobs_ScheduledDate" ON "JobService"."Jobs" (scheduled_date);

CREATE INDEX "IX_Jobs_Status" ON "JobService"."Jobs" (status);

CREATE INDEX "IX_Jobs_TenantId" ON "JobService"."Jobs" (tenant_id);

CREATE INDEX "IX_Jobs_TenantId_CustomerId" ON "JobService"."Jobs" (tenant_id, customer_id);

CREATE INDEX "IX_Jobs_TenantId_Status" ON "JobService"."Jobs" (tenant_id, status);

CREATE INDEX "IX_Jobs_TenantId_TechnicianId_ScheduledDate" ON "JobService"."Jobs" (tenant_id, assigned_technician_id, scheduled_date);

CREATE INDEX "IX_JobServiceItems_IsFulfilled" ON "JobService"."JobServiceItems" (is_fulfilled);

CREATE INDEX "IX_JobServiceItems_JobId" ON "JobService"."JobServiceItems" (job_id);

CREATE INDEX "IX_JobServiceItems_ServiceName" ON "JobService"."JobServiceItems" (service_name);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20250807203923_Initial', '9.0.7');

COMMIT;


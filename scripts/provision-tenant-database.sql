-- Provision database schema for tenant: testt
-- Run this script against the mechanicbuddy database to fix the missing schema

-- Create helper function if not exists
CREATE OR REPLACE FUNCTION f_concat_ws(text, VARIADIC text[])
RETURNS text LANGUAGE sql IMMUTABLE AS 'SELECT array_to_string($2, $1)';

-- Create the main schema for tenant_testt
CREATE SCHEMA IF NOT EXISTS "tenant_testt";

-- Employee table
CREATE TABLE IF NOT EXISTS "tenant_testt".employee (
    id uuid primary key,
    firstname varchar NOT NULL,
    lastname varchar NOT NULL,
    email varchar,
    phone varchar,
    address varchar,
    proffession varchar,
    description varchar,
    introducedat timestamp without time zone not null
);

-- Vehicle table
CREATE TABLE IF NOT EXISTS "tenant_testt".vehicle (
    id uuid primary key,
    producer VARCHAR,
    model VARCHAR,
    regnr VARCHAR not null,
    vin VARCHAR,
    odo INT,
    body varchar,
    drivingside varchar,
    engine varchar,
    productiondate date,
    region varchar,
    series varchar,
    transmission varchar,
    description varchar,
    introducedat timestamp with time zone not null
);

-- Client table
CREATE TABLE IF NOT EXISTS "tenant_testt".client (
    id uuid primary key,
    address varchar,
    country varchar,
    region varchar,
    city varchar,
    postalcode varchar,
    phone varchar,
    description varchar,
    isasshole boolean default false NOT NULL,
    introducedat timestamp with time zone not null
);

-- VehicleRegistration table
CREATE TABLE IF NOT EXISTS "tenant_testt".vehicleregistration (
    ownerid uuid NOT NULL references "tenant_testt".client,
    vehicleid uuid not null references "tenant_testt".vehicle,
    datetimefrom timestamp with time zone not null,
    datetimeto timestamp with time zone null,
    primary key (ownerid, vehicleid, datetimefrom)
);

-- ClientEmail table
CREATE TABLE IF NOT EXISTS "tenant_testt".clientemail (
    address varchar not null,
    clientid uuid references "tenant_testt".client,
    isactive boolean not null,
    primary key (address, clientid)
);

-- PrivateClient table
CREATE TABLE IF NOT EXISTS "tenant_testt".privateclient (
    id uuid PRIMARY KEY references "tenant_testt".client,
    firstname varchar NOT NULL,
    lastname varchar,
    personalcode varchar
);

-- LegalClient table
CREATE TABLE IF NOT EXISTS "tenant_testt".legalclient (
    id uuid PRIMARY KEY references "tenant_testt".client,
    name varchar NOT NULL,
    regnr varchar
);

-- Pricing table
CREATE TABLE IF NOT EXISTS "tenant_testt".pricing (
    id uuid primary key,
    senton timestamp with time zone,
    printedon timestamp with time zone,
    email varchar,
    partyname varchar not null,
    partyaddress varchar,
    partycode varchar,
    vehicleline1 varchar,
    vehicleline2 varchar,
    vehicleline3 varchar,
    vehicleline4 varchar,
    issuedon timestamp with time zone not null,
    issuerid uuid not null references "tenant_testt".employee
);

-- Estimate table
CREATE TABLE IF NOT EXISTS "tenant_testt".estimate (
    id uuid primary key references "tenant_testt".pricing,
    number varchar not null unique
);

-- Invoice table
CREATE TABLE IF NOT EXISTS "tenant_testt".invoice (
    id uuid primary key references "tenant_testt".pricing,
    number int not null unique,
    paymenttype smallint not null,
    duedays smallint not null,
    ispaid boolean default false not null,
    iscredited boolean default false NULL
);

-- Work table
CREATE TABLE IF NOT EXISTS "tenant_testt".work (
    id uuid primary key,
    number int not null,
    invoiceid uuid references "tenant_testt".invoice,
    clientid uuid references "tenant_testt".client,
    vehicleid uuid null references "tenant_testt".vehicle,
    startedon timestamp with time zone not null,
    changedon timestamp with time zone not null unique,
    starterid uuid not null references "tenant_testt".employee,
    notes varchar,
    odo int,
    userstatus varchar default 'Default' NOT NULL,
    completedon timestamp with time zone,
    completerid uuid references "tenant_testt".employee
);

-- Offer table
CREATE TABLE IF NOT EXISTS "tenant_testt".offer (
    id uuid primary key,
    workid uuid not null references "tenant_testt".work(id),
    ordernr smallint not null,
    notes varchar,
    estimateid uuid references "tenant_testt".estimate,
    isvehilelesonestimate boolean default false not null,
    startedon timestamp with time zone not null,
    starterid uuid not null references "tenant_testt".employee,
    acceptedon timestamp with time zone,
    acceptorid uuid references "tenant_testt".employee,
    unique(workid, ordernr)
);

-- RepairJob table
CREATE TABLE IF NOT EXISTS "tenant_testt".repairjob (
    id uuid primary key,
    workid uuid not null references "tenant_testt".work(id),
    ordernr smallint not null,
    notes varchar,
    startedon timestamp with time zone not null,
    starterid uuid not null references "tenant_testt".employee,
    unique(workid, ordernr)
);

-- Assignment table
CREATE TABLE IF NOT EXISTS "tenant_testt".assignment (
    workid uuid not null references "tenant_testt".work(id),
    mechanicid uuid not null references "tenant_testt".employee,
    primary key (workid, mechanicid)
);

-- Saleable table
CREATE TABLE IF NOT EXISTS "tenant_testt".saleable (
    id uuid primary key,
    name varchar not null,
    quantity double precision not null,
    unit varchar not null,
    price double precision not null,
    discount smallint
);

-- ServiceOffered table
CREATE TABLE IF NOT EXISTS "tenant_testt".serviceoffered (
    id uuid primary key references "tenant_testt".saleable,
    offerid uuid not null references "tenant_testt".offer(id)
);

-- ProductOffered table
CREATE TABLE IF NOT EXISTS "tenant_testt".productoffered (
    id uuid primary key references "tenant_testt".saleable,
    offerid uuid not null references "tenant_testt".offer(id),
    code varchar not null,
    jnr smallint not null,
    serviceid uuid references "tenant_testt".serviceoffered
);

-- ServicePerformed table
CREATE TABLE IF NOT EXISTS "tenant_testt".serviceperformed (
    id uuid primary key references "tenant_testt".saleable,
    repairjobid uuid not null references "tenant_testt".repairjob,
    notes varchar,
    mechanicid uuid references "tenant_testt".employee
);

-- ProductInstalled table
CREATE TABLE IF NOT EXISTS "tenant_testt".productinstalled (
    id uuid primary key references "tenant_testt".saleable,
    repairjobid uuid not null references "tenant_testt".repairjob,
    jnr smallint not null,
    code varchar not null,
    notes varchar,
    status smallint not null,
    serviceid uuid references "tenant_testt".serviceperformed
);

-- Storage table
CREATE TABLE IF NOT EXISTS "tenant_testt".storage (
    id uuid primary key,
    name varchar not null,
    address varchar,
    description varchar,
    introducedat timestamp with time zone not null
);

-- UnitedMotorsPrice table
CREATE TABLE IF NOT EXISTS "tenant_testt".unitedmotorsprice (
    id uuid primary key,
    price double precision not null,
    name varchar NOT NULL,
    address varchar
);

-- SparePart table
CREATE TABLE IF NOT EXISTS "tenant_testt".sparepart (
    id uuid primary key,
    code varchar not null,
    name varchar not null,
    price double precision,
    storageid uuid null references "tenant_testt".storage,
    quantity double precision,
    discount smallint,
    description varchar,
    introducedat timestamp with time zone not null,
    umpriceid uuid references "tenant_testt".unitedmotorsprice(id)
);

-- PricingLine table
CREATE TABLE IF NOT EXISTS "tenant_testt".pricingline (
    pricingid uuid not null references "tenant_testt".pricing,
    nr smallint not null,
    description varchar not null,
    quantity double precision not null,
    unitprice double precision not null,
    unit varchar not null,
    discount smallint not null default 0,
    total double precision not null,
    totalwithvat double precision not null,
    primary key (pricingid, nr)
);

-- ServiceRequest table
CREATE TABLE IF NOT EXISTS "tenant_testt".servicerequest (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customername VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    email VARCHAR(255),
    vehicleinfo VARCHAR(500),
    servicetype VARCHAR(100),
    message TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'New',
    submittedat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes TEXT
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_vehicle_vin ON "tenant_testt".vehicle(vin);
CREATE INDEX IF NOT EXISTS idx_client_address ON "tenant_testt".client(address);
CREATE INDEX IF NOT EXISTS idx_client_phone ON "tenant_testt".client(phone);
CREATE INDEX IF NOT EXISTS idx_privateclient ON "tenant_testt".privateclient(firstname, lastname);
CREATE INDEX IF NOT EXISTS idx_pricing_issuerid ON "tenant_testt".pricing(issuerid);
CREATE INDEX IF NOT EXISTS idx_work_clientid ON "tenant_testt".work(clientid);
CREATE INDEX IF NOT EXISTS idx_work_starterid ON "tenant_testt".work(starterid);
CREATE INDEX IF NOT EXISTS idx_work_vehicleid ON "tenant_testt".work(vehicleid);
CREATE INDEX IF NOT EXISTS idx_offer_workid_ordernr ON "tenant_testt".offer(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_repairjob_workid_ordernr ON "tenant_testt".repairjob(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_offer_estimateid ON "tenant_testt".offer(estimateid);
CREATE INDEX IF NOT EXISTS idx_saleable_name ON "tenant_testt".saleable(name);
CREATE INDEX IF NOT EXISTS idx_productoffered_code ON "tenant_testt".productoffered(code);
CREATE INDEX IF NOT EXISTS idx_productinstalled_code ON "tenant_testt".productinstalled(code);
CREATE INDEX IF NOT EXISTS idx_number_work ON "tenant_testt".work(number);
CREATE INDEX IF NOT EXISTS idx_number_estimate ON "tenant_testt".estimate(number);
CREATE INDEX IF NOT EXISTS idx_number_invoice ON "tenant_testt".invoice(number);
CREATE INDEX IF NOT EXISTS idx_servicerequest_status ON "tenant_testt".servicerequest(status);
CREATE INDEX IF NOT EXISTS idx_servicerequest_submittedat ON "tenant_testt".servicerequest(submittedat DESC);

-- Add unique constraint on work number
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'tenant_testt_work_number_key') THEN
        ALTER TABLE "tenant_testt".work ADD CONSTRAINT tenant_testt_work_number_key UNIQUE (number);
    END IF;
END
$$;

-- Create tenant config schema
CREATE SCHEMA IF NOT EXISTS "tenant_testt_config";

-- Create requisites table
CREATE TABLE IF NOT EXISTS "tenant_testt_config".requisites (
    id uuid PRIMARY KEY,
    name VARCHAR NOT NULL,
    phone VARCHAR,
    address VARCHAR,
    email VARCHAR,
    bank_account VARCHAR,
    reg_nr VARCHAR,
    tax_id VARCHAR,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create pricing table
CREATE TABLE IF NOT EXISTS "tenant_testt_config".pricing (
    id uuid PRIMARY KEY,
    vat_rate INTEGER NOT NULL DEFAULT 20,
    surcharge VARCHAR,
    disclaimer VARCHAR,
    signature_line BOOLEAN NOT NULL DEFAULT true,
    invoice_email_content TEXT,
    estimate_email_content TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert default values if they don't exist
INSERT INTO "tenant_testt_config".requisites (id, name, phone, address, email, bank_account, reg_nr, tax_id)
SELECT '6dd57256-2774-424f-a61b-887bf8327329', 'Default Company', '+1234567890', '123 Main St', 'info@example.com', 'EE123456789012', 'REG12345', 'VAT123456'
WHERE NOT EXISTS (SELECT 1 FROM "tenant_testt_config".requisites WHERE id = '6dd57256-2774-424f-a61b-887bf8327329');

INSERT INTO "tenant_testt_config".pricing (id, vat_rate, surcharge, disclaimer, signature_line, invoice_email_content, estimate_email_content)
SELECT '3b9806b3-287b-46cc-bc17-a2d40500327b', 20, 'Default Surcharge', 'Default Disclaimer', true,
       'Thank you for your business. Please find your invoice attached.',
       'Thank you for your interest. Please find your estimate attached.'
WHERE NOT EXISTS (SELECT 1 FROM "tenant_testt_config".pricing WHERE id = '3b9806b3-287b-46cc-bc17-a2d40500327b');

-- Create public.user table if not exists
CREATE TABLE IF NOT EXISTS public."user" (
    username varchar NOT NULL,
    password varchar NOT NULL,
    tenantname varchar NOT NULL,
    email varchar NULL,
    validated boolean NOT NULL DEFAULT false,
    profile_image bytea null,
    employeeid uuid,
    PRIMARY KEY (username, tenantname)
);

-- Create default admin employee for tenant_testt
DO $$
DECLARE
    v_employee_id uuid;
BEGIN
    -- Check if admin already exists for this tenant
    IF NOT EXISTS (SELECT 1 FROM public."user" WHERE username = 'admin' AND tenantname = 'testt') THEN
        -- Generate new employee ID
        v_employee_id := gen_random_uuid();

        -- Create employee record
        INSERT INTO "tenant_testt".employee (id, firstname, lastname, email, phone, proffession, description, introducedat)
        VALUES (v_employee_id, 'System', 'Administrator', 'admin@example.com', '', 'Administrator', 'Default system administrator', CURRENT_TIMESTAMP);

        -- Create admin user (password: carcare)
        INSERT INTO public."user" (username, password, tenantname, email, validated, profile_image, employeeid)
        VALUES ('admin', '$2a$11$zsTS62pGn5Cfca4CgqRJxebx45je/3nJj.puxIArFwtAjHew67m6i', 'testt', 'admin@example.com', true, null, v_employee_id);

        RAISE NOTICE 'Created admin user for tenant testt with employee ID %', v_employee_id;
    ELSE
        RAISE NOTICE 'Admin user already exists for tenant testt';
    END IF;
END
$$;

-- Verify the schema was created
SELECT schema_name FROM information_schema.schemata WHERE schema_name LIKE 'tenant_testt%';

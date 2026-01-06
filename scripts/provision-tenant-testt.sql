-- Provision database schema for tenant: testt
-- Run this script against the mechanicbuddy-testt database
-- Usage: psql -U postgres -d 'mechanicbuddy-testt' -f provision-tenant-testt.sql

-- Create helper function if not exists
CREATE OR REPLACE FUNCTION f_concat_ws(text, VARIADIC text[])
RETURNS text LANGUAGE sql IMMUTABLE AS 'SELECT array_to_string($2, $1)';

-- Create the domain schema (standard for all tenants)
CREATE SCHEMA IF NOT EXISTS domain;

-- Employee table
CREATE TABLE IF NOT EXISTS domain.employee (
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
CREATE TABLE IF NOT EXISTS domain.vehicle (
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
CREATE TABLE IF NOT EXISTS domain.client (
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
CREATE TABLE IF NOT EXISTS domain.vehicleregistration (
    ownerid uuid NOT NULL references domain.client,
    vehicleid uuid not null references domain.vehicle,
    datetimefrom timestamp with time zone not null,
    datetimeto timestamp with time zone null,
    primary key (ownerid, vehicleid, datetimefrom)
);

-- ClientEmail table
CREATE TABLE IF NOT EXISTS domain.clientemail (
    address varchar not null,
    clientid uuid references domain.client,
    isactive boolean not null,
    primary key (address, clientid)
);

-- PrivateClient table
CREATE TABLE IF NOT EXISTS domain.privateclient (
    id uuid PRIMARY KEY references domain.client,
    firstname varchar NOT NULL,
    lastname varchar,
    personalcode varchar
);

-- LegalClient table
CREATE TABLE IF NOT EXISTS domain.legalclient (
    id uuid PRIMARY KEY references domain.client,
    name varchar NOT NULL,
    regnr varchar
);

-- Pricing table
CREATE TABLE IF NOT EXISTS domain.pricing (
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
    issuerid uuid not null references domain.employee
);

-- Estimate table
CREATE TABLE IF NOT EXISTS domain.estimate (
    id uuid primary key references domain.pricing,
    number varchar not null unique
);

-- Invoice table
CREATE TABLE IF NOT EXISTS domain.invoice (
    id uuid primary key references domain.pricing,
    number int not null unique,
    paymenttype smallint not null,
    duedays smallint not null,
    ispaid boolean default false not null,
    iscredited boolean default false NULL
);

-- Work table
CREATE TABLE IF NOT EXISTS domain.work (
    id uuid primary key,
    number int not null,
    invoiceid uuid references domain.invoice,
    clientid uuid references domain.client,
    vehicleid uuid null references domain.vehicle,
    startedon timestamp with time zone not null,
    changedon timestamp with time zone not null unique,
    starterid uuid not null references domain.employee,
    notes varchar,
    odo int,
    userstatus varchar default 'Default' NOT NULL,
    completedon timestamp with time zone,
    completerid uuid references domain.employee
);

-- Offer table
CREATE TABLE IF NOT EXISTS domain.offer (
    id uuid primary key,
    workid uuid not null references domain.work(id),
    ordernr smallint not null,
    notes varchar,
    estimateid uuid references domain.estimate,
    isvehilelesonestimate boolean default false not null,
    startedon timestamp with time zone not null,
    starterid uuid not null references domain.employee,
    acceptedon timestamp with time zone,
    acceptorid uuid references domain.employee,
    unique(workid, ordernr)
);

-- RepairJob table
CREATE TABLE IF NOT EXISTS domain.repairjob (
    id uuid primary key,
    workid uuid not null references domain.work(id),
    ordernr smallint not null,
    notes varchar,
    startedon timestamp with time zone not null,
    starterid uuid not null references domain.employee,
    unique(workid, ordernr)
);

-- Assignment table
CREATE TABLE IF NOT EXISTS domain.assignment (
    workid uuid not null references domain.work(id),
    mechanicid uuid not null references domain.employee,
    primary key (workid, mechanicid)
);

-- Saleable table
CREATE TABLE IF NOT EXISTS domain.saleable (
    id uuid primary key,
    name varchar not null,
    quantity double precision not null,
    unit varchar not null,
    price double precision not null,
    discount smallint
);

-- ServiceOffered table
CREATE TABLE IF NOT EXISTS domain.serviceoffered (
    id uuid primary key references domain.saleable,
    offerid uuid not null references domain.offer(id)
);

-- ProductOffered table
CREATE TABLE IF NOT EXISTS domain.productoffered (
    id uuid primary key references domain.saleable,
    offerid uuid not null references domain.offer(id),
    code varchar not null,
    jnr smallint not null,
    serviceid uuid references domain.serviceoffered
);

-- ServicePerformed table
CREATE TABLE IF NOT EXISTS domain.serviceperformed (
    id uuid primary key references domain.saleable,
    repairjobid uuid not null references domain.repairjob,
    notes varchar,
    mechanicid uuid references domain.employee
);

-- ProductInstalled table
CREATE TABLE IF NOT EXISTS domain.productinstalled (
    id uuid primary key references domain.saleable,
    repairjobid uuid not null references domain.repairjob,
    jnr smallint not null,
    code varchar not null,
    notes varchar,
    status smallint not null,
    serviceid uuid references domain.serviceperformed
);

-- Storage table
CREATE TABLE IF NOT EXISTS domain.storage (
    id uuid primary key,
    name varchar not null,
    address varchar,
    description varchar,
    introducedat timestamp with time zone not null
);

-- UnitedMotorsPrice table
CREATE TABLE IF NOT EXISTS domain.unitedmotorsprice (
    id uuid primary key,
    price double precision not null,
    name varchar NOT NULL,
    address varchar
);

-- SparePart table
CREATE TABLE IF NOT EXISTS domain.sparepart (
    id uuid primary key,
    code varchar not null,
    name varchar not null,
    price double precision,
    storageid uuid null references domain.storage,
    quantity double precision,
    discount smallint,
    description varchar,
    introducedat timestamp with time zone not null,
    umpriceid uuid references domain.unitedmotorsprice(id)
);

-- PricingLine table
CREATE TABLE IF NOT EXISTS domain.pricingline (
    pricingid uuid not null references domain.pricing,
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
CREATE TABLE IF NOT EXISTS domain.servicerequest (
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

-- Create public.user table if not exists
CREATE TABLE IF NOT EXISTS public."user" (
    username varchar NOT NULL,
    password varchar NOT NULL,
    tenantname varchar NOT NULL,
    email varchar NULL,
    validated boolean NOT NULL DEFAULT false,
    profile_image bytea null,
    employeeid uuid
);

-- Create tenant_config schema
CREATE SCHEMA IF NOT EXISTS tenant_config;

-- Create requisites table
CREATE TABLE IF NOT EXISTS tenant_config.requisites (
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
CREATE TABLE IF NOT EXISTS tenant_config.pricing (
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
INSERT INTO tenant_config.requisites (id, name, phone, address, email, bank_account, reg_nr, tax_id)
SELECT '6dd57256-2774-424f-a61b-887bf8327329', 'Default Company', '+1234567890', '123 Main St', 'info@example.com', 'EE123456789012', 'REG12345', 'VAT123456'
WHERE NOT EXISTS (SELECT 1 FROM tenant_config.requisites WHERE id = '6dd57256-2774-424f-a61b-887bf8327329');

INSERT INTO tenant_config.pricing (id, vat_rate, surcharge, disclaimer, signature_line, invoice_email_content, estimate_email_content)
SELECT '3b9806b3-287b-46cc-bc17-a2d40500327b', 20, 'Default Surcharge', 'Default Disclaimer', true,
       'Thank you for your business. Please find your invoice attached.',
       'Thank you for your interest. Please find your estimate attached.'
WHERE NOT EXISTS (SELECT 1 FROM tenant_config.pricing WHERE id = '3b9806b3-287b-46cc-bc17-a2d40500327b');

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_vehicle_vin ON domain.vehicle(vin);
CREATE INDEX IF NOT EXISTS idx_client_address ON domain.client(address);
CREATE INDEX IF NOT EXISTS idx_client_phone ON domain.client(phone);
CREATE INDEX IF NOT EXISTS idx_privateclient ON domain.privateclient(firstname, lastname);
CREATE INDEX IF NOT EXISTS idx_pricing_issuerid ON domain.pricing(issuerid);
CREATE INDEX IF NOT EXISTS idx_work_clientid ON domain.work(clientid);
CREATE INDEX IF NOT EXISTS idx_work_starterid ON domain.work(starterid);
CREATE INDEX IF NOT EXISTS idx_work_vehicleid ON domain.work(vehicleid);
CREATE INDEX IF NOT EXISTS idx_offer_workid_ordernr ON domain.offer(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_repairjob_workid_ordernr ON domain.repairjob(workid, ordernr);
CREATE INDEX IF NOT EXISTS idx_offer_estimateid ON domain.offer(estimateid);
CREATE INDEX IF NOT EXISTS idx_saleable_name ON domain.saleable(name);
CREATE INDEX IF NOT EXISTS idx_productoffered_code ON domain.productoffered(code);
CREATE INDEX IF NOT EXISTS idx_productinstalled_code ON domain.productinstalled(code);
CREATE INDEX IF NOT EXISTS idx_number_work ON domain.work(number);
CREATE INDEX IF NOT EXISTS idx_number_estimate ON domain.estimate(number);
CREATE INDEX IF NOT EXISTS idx_number_invoice ON domain.invoice(number);
CREATE INDEX IF NOT EXISTS idx_servicerequest_status ON domain.servicerequest(status);
CREATE INDEX IF NOT EXISTS idx_servicerequest_submittedat ON domain.servicerequest(submittedat DESC);

-- Add unique constraint on work number
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'work_number_key') THEN
        ALTER TABLE domain.work ADD CONSTRAINT work_number_key UNIQUE (number);
    END IF;
END
$$;

-- Create default admin employee
DO $$
DECLARE
    v_employee_id uuid;
BEGIN
    -- Check if admin already exists for this tenant
    IF NOT EXISTS (SELECT 1 FROM public."user" WHERE username = 'admin' AND tenantname = 'testt') THEN
        -- Generate new employee ID
        v_employee_id := gen_random_uuid();

        -- Create employee record
        INSERT INTO domain.employee (id, firstname, lastname, email, phone, proffession, description, introducedat)
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

-- Verify the schemas were created
SELECT 'Schemas:' as info;
SELECT schema_name FROM information_schema.schemata WHERE schema_name IN ('domain', 'tenant_config', 'public');

SELECT 'Tables in domain schema:' as info;
SELECT table_name FROM information_schema.tables WHERE table_schema = 'domain' ORDER BY table_name;

SELECT 'Admin user:' as info;
SELECT username, tenantname, validated FROM public."user" WHERE username = 'admin';

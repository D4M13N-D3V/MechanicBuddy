-- Service Request table for customer submissions from landing page
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

-- Index for querying by status and date
CREATE INDEX IF NOT EXISTS idx_servicerequest_status ON domain.servicerequest(status);
CREATE INDEX IF NOT EXISTS idx_servicerequest_submittedat ON domain.servicerequest(submittedat DESC);

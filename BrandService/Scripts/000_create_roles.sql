-- 000_create_roles.sql (must run before 001_create_upgrade_history.sql)

DO $$ 
BEGIN
    -- Ensure the "brands" role exists
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'brands') THEN
        CREATE ROLE brands WITH LOGIN PASSWORD 'your_secure_password';
        COMMENT ON ROLE brands IS 'Application role for brand services';
    END IF;

    -- Grant privileges to "brands" role on "brand" database
    GRANT ALL PRIVILEGES ON DATABASE brand TO brands;

    -- Ensure the "upgrade" role exists
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'upgrade') THEN
        CREATE ROLE upgrade WITH NOLOGIN;
        COMMENT ON ROLE upgrade IS 'Role for managing database upgrade infrastructure';
    END IF;
END $$;


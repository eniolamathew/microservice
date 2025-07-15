-- 000_create_roles.sql (must run before 001_create_upgrade_history.sql)

DO $$ 
BEGIN
    -- Ensure the "platforms" role exists
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'platforms') THEN
        CREATE ROLE platforms WITH LOGIN PASSWORD 'your_secure_password';
        COMMENT ON ROLE platforms IS 'Application role for platform services';
    END IF;
    
    -- Ensure the "platform" database exists
    IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'platform') THEN
        CREATE DATABASE platform;
        COMMENT ON DATABASE platform IS 'Database for platform services';
    END IF;

    -- Grant privileges to "platforms" role on "platform" database
    GRANT ALL PRIVILEGES ON DATABASE platform TO platforms;

    -- Ensure the "upgrade" role exists
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'upgrade') THEN
        CREATE ROLE upgrade WITH NOLOGIN;
        COMMENT ON ROLE upgrade IS 'Role for managing database upgrade infrastructure';
    END IF;
END $$;


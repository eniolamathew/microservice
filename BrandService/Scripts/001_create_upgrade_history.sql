-- 001_create_upgrade_history.sql

DO $$
BEGIN
    -- Create schema with explicit authorization
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'upgrade') THEN
        CREATE SCHEMA upgrade AUTHORIZATION upgrade;
    END IF;

    -- Create version history tracking table
    CREATE TABLE IF NOT EXISTS upgrade.upgrade_history (
        id SERIAL PRIMARY KEY,
        upgraded_on TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        version INT NOT NULL CHECK (version >= 0),
        file_name TEXT NOT NULL CHECK (file_name ~ '^\d{3}_[a-zA-Z0-9_]+\.sql$'),
        checksum TEXT NOT NULL,
        executed_by TEXT DEFAULT current_user,
        success BOOLEAN NOT NULL,
        error_message TEXT,
        execution_time INTERVAL
    ) WITH (fillfactor=90);

    -- Set ownership
    ALTER TABLE upgrade.upgrade_history OWNER TO upgrade;
    
    -- Grant permissions to brands user (changed from brandService)
    GRANT USAGE ON SCHEMA upgrade TO brands;
    GRANT SELECT, INSERT ON upgrade.upgrade_history TO brands;
    GRANT USAGE, SELECT ON SEQUENCE upgrade.upgrade_history_id_seq TO brands;
    
    -- Add indexes for faster lookups
    CREATE INDEX IF NOT EXISTS idx_upgrade_history_version ON upgrade.upgrade_history(version);
    CREATE INDEX IF NOT EXISTS idx_upgrade_history_filename ON upgrade.upgrade_history(file_name);
    
    -- Add table comments for documentation
    COMMENT ON TABLE upgrade.upgrade_history IS 'Tracks all database upgrade script executions';
    COMMENT ON COLUMN upgrade.upgrade_history.checksum IS 'SHA-256 hash of script content for verification';
END $$;
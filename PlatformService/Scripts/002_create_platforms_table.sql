-- 002_create_platforms_table.sql
DO $$
BEGIN
    -- Create the role if it doesn't exist (corrected role name)
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'platforms') THEN
        CREATE ROLE platforms WITH NOLOGIN;
        COMMENT ON ROLE platforms IS 'Role for platform data management';
    END IF;

    -- Create the schema with explicit authorization
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'platform') THEN
        CREATE SCHEMA platform AUTHORIZATION platforms;  -- Changed to platforms role
    END IF;

    -- Create the platforms table with complete constraints
    CREATE TABLE IF NOT EXISTS platform.platforms (
        id INT NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
        name VARCHAR(255) NOT NULL CHECK (name <> ''),
        description TEXT NOT NULL CHECK (description <> ''),
        price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
        owner VARCHAR(255) NOT NULL CHECK (owner <> ''),
        isdeleted BOOLEAN NOT NULL DEFAULT FALSE,
        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT pk_platforms PRIMARY KEY (id)
    );

    -- Set ownership and permissions
    ALTER TABLE platform.platforms OWNER TO platforms;
    
    -- Grant permissions (corrected GRANT spelling)
    GRANT ALL PRIVILEGES ON TABLE platform.platforms TO platforms;
    GRANT USAGE, SELECT ON SEQUENCE platform.platforms_id_seq TO platforms;
    
    -- Add indexes for better performance
    CREATE INDEX IF NOT EXISTS idx_platforms_name ON platform.platforms(name);
    CREATE INDEX IF NOT EXISTS idx_platforms_isdeleted ON platform.platforms(isdeleted);
    
    -- Add comments for documentation
    COMMENT ON TABLE platform.platforms IS 'Stores platform information';
    COMMENT ON COLUMN platform.platforms.isdeleted IS 'Soft delete flag';
END $$;
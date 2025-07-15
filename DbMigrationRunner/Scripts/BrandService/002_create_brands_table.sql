-- 002_create_brands_table.sql
DO $$
BEGIN
    -- Create the role if it doesn't exist (corrected role name)
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'brands') THEN
        CREATE ROLE brands WITH NOLOGIN;
        COMMENT ON ROLE brands IS 'Role for brand data management';
    END IF;

    -- Create the schema with explicit authorization
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'brand') THEN
        CREATE SCHEMA brand AUTHORIZATION brands;  -- corrected: assign schema to 'brands' role
    END IF;

    -- Create the brands table with complete constraints
    CREATE TABLE IF NOT EXISTS brand.brands (
        id INT NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
        description TEXT NOT NULL CHECK (description <> ''),
        supplierId INT NOT NULL CHECK (supplierId >= 0),
        supplierName VARCHAR(255) NOT NULL CHECK (supplierName <> ''),
        importId VARCHAR(255) NOT NULL CHECK (importId <> ''),
        isdeleted BOOLEAN NOT NULL DEFAULT FALSE,
        statusId INT NOT NULL CHECK (statusId >= 0),
        showInNavigation BOOLEAN NOT NULL DEFAULT FALSE,
        countryRestrictionTypeId INT NOT NULL CHECK (countryRestrictionTypeId >= 0),
        articleContentFilterId INT NOT NULL CHECK (articleContentFilterId >= 0),

        CONSTRAINT pk_brands PRIMARY KEY (id)
    );

    -- Set ownership and permissions
    ALTER TABLE brand.brands OWNER TO brands;

    -- Grant all privileges on the table to brands role
    GRANT ALL PRIVILEGES ON TABLE brand.brands TO brands;

    -- Grant usage and select on sequence to brands role
    GRANT USAGE, SELECT ON SEQUENCE brand.brands_id_seq TO brands;

    -- Add indexes for better performance
    CREATE INDEX IF NOT EXISTS idx_brands_description ON brand.brands(description);
    CREATE INDEX IF NOT EXISTS idx_brands_isdeleted ON brand.brands(isdeleted);

    -- Add comments for documentation
    COMMENT ON TABLE brand.brands IS 'Stores brand information';
    COMMENT ON COLUMN brand.brands.isdeleted IS 'Soft delete flag';
END $$;

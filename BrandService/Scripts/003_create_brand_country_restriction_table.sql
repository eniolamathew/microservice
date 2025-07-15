-- 003_create_brand_country_restriction_table.sql

DO $$
BEGIN
    -- Create the brand_country_restriction table
    CREATE TABLE IF NOT EXISTS brand.brand_country_restriction (
        id INT NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
        brand_id INT NOT NULL,
        country_id INT NOT NULL,

        CONSTRAINT pk_brand_country_restriction PRIMARY KEY (id),
        CONSTRAINT fk_brand_country_restriction_brand FOREIGN KEY (brand_id)
            REFERENCES brand.brands (id)
            ON DELETE CASCADE
    );

    -- Set table owner
    ALTER TABLE brand.brand_country_restriction OWNER TO brands;

    -- Grant permissions
    GRANT ALL PRIVILEGES ON TABLE brand.brand_country_restriction TO brands;
    GRANT USAGE, SELECT ON SEQUENCE brand.brand_country_restriction_id_seq TO brands;

    -- Add indexes
    CREATE INDEX IF NOT EXISTS idx_brand_country_restriction_brand_id ON brand.brand_country_restriction (brand_id);
    CREATE INDEX IF NOT EXISTS idx_brand_country_restriction_country_id ON brand.brand_country_restriction (country_id);

    -- Add comments
    COMMENT ON TABLE brand.brand_country_restriction IS 'Stores country restrictions for each brand';
    COMMENT ON COLUMN brand.brand_country_restriction.brand_id IS 'FK to brand.brands(id)';
    COMMENT ON COLUMN brand.brand_country_restriction.country_id IS 'Restricted country ID';
END $$;

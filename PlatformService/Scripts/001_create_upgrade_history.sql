-- 001_create_upgrade_history.sql
-- Create the schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS upgrade;

-- Create the upgrade_history table
CREATE TABLE IF NOT EXISTS upgrade.upgrade_history (
    id SERIAL PRIMARY KEY,
    upgraded_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    version INT NOT NULL,
    file_name TEXT NOT NULL
);

ALTER TABLE upgrade.upgrade_history
    OWNER to upgrade;

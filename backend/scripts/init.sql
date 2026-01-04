-- Database initialization script for Joiabagur PV
-- This script runs when the PostgreSQL container starts for the first time

-- Create extensions if needed
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Add any initial data setup here
-- This is where you might add seed data for development

DO $$
BEGIN
    RAISE NOTICE 'Joiabagur PV database initialized successfully';
END
$$;
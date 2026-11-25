-- SitesLend: one-time database setup.
-- Run this yourself in pgAdmin (Query Tool). The AI agent never runs it.
--
-- Run as a PostgreSQL superuser (usually "postgres").
-- Steps 1 and 2 run while connected to the default "postgres" database.
-- Steps 3 and 4 run while connected to the "siteslend" database.
--
-- Before running: replace <CHOOSE_A_STRONG_PASSWORD> in step 1.
-- Never commit the real password. It belongs only in dotnet user-secrets (see README).

-- 1. Create the application login. No superuser, no create-database, no create-role.
--    Skip this step if the role already exists.
CREATE ROLE siteslend_app
    LOGIN
    PASSWORD '<CHOOSE_A_STRONG_PASSWORD>'
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;

-- 2. Create the database if it does not exist yet (CREATE DATABASE cannot run
--    inside a transaction block, so run it on its own).
--    Skip this step if "siteslend" already exists.
CREATE DATABASE siteslend;

-- 3. Connect to "siteslend". Only siteslend_app (and superusers) may connect to it.
--    siteslend_app is granted nothing on any other database. Note that PostgreSQL
--    lets every login CONNECT to a database by default, so on other databases it can
--    connect but can read and change nothing. To block even that, run
--    REVOKE CONNECT ON DATABASE <name> FROM PUBLIC; on each of them.
REVOKE ALL ON DATABASE siteslend FROM PUBLIC;
GRANT CONNECT ON DATABASE siteslend TO siteslend_app;

-- 4. Still in "siteslend": allow siteslend_app to use and create objects in the
--    public schema. EF Core migrations run as this user, so it needs CREATE.
--    (Since PostgreSQL 15, ordinary users no longer get CREATE on "public" by default.)
GRANT USAGE, CREATE ON SCHEMA public TO siteslend_app;

-- Objects that siteslend_app creates (tables from migrations) are owned by it,
-- so no further GRANT or ALTER DEFAULT PRIVILEGES is needed.

-- Optional check, still in "siteslend", connected as siteslend_app:
--   SELECT current_user, current_database();
-- Expected: siteslend_app | siteslend

-- SitesLend: fresh database setup (run once, on an empty server).
-- Run this yourself in pgAdmin (Query Tool). The AI agent never runs it.
--
-- Run as a PostgreSQL superuser (usually "postgres").
-- The app connects as siteslend_app only: no superuser, no create-database, no create-role.
--
-- Before running: replace <CHOOSE_A_STRONG_PASSWORD> in step 1.
-- Never commit the real password. It belongs only in dotnet user-secrets (see README).
--
-- Steps 1 to 3 run while connected to the default "postgres" database.
-- Step 4 runs once in "siteslend" and once in "siteslend_test".

-- 1. Application login.
CREATE ROLE siteslend_app
    LOGIN
    PASSWORD '<CHOOSE_A_STRONG_PASSWORD>'
    NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;

-- 2. The two databases, owned by siteslend_app.
--    IMPORTANT: CREATE DATABASE cannot run inside a transaction block, so run these
--    two statements ONE AT A TIME (select one statement, run it, then the next).
CREATE DATABASE siteslend
    OWNER siteslend_app
    ENCODING 'UTF8'
    TEMPLATE template0;

CREATE DATABASE siteslend_test
    OWNER siteslend_app
    ENCODING 'UTF8'
    TEMPLATE template0;

-- 3. Only the owner (and superusers) may connect. Other logins get no access at all.
REVOKE ALL ON DATABASE siteslend FROM PUBLIC;
REVOKE ALL ON DATABASE siteslend_test FROM PUBLIC;

-- 4. Connect to "siteslend" and run the next line. Then connect to "siteslend_test"
--    and run it again. EF Core migrations run as siteslend_app and create tables
--    in the public schema, so it must own that schema.
ALTER SCHEMA public OWNER TO siteslend_app;

-- Optional check, connected as siteslend_app to each database:
--   SELECT current_user, current_database();
-- Expected: siteslend_app | siteslend   (and siteslend_app | siteslend_test)

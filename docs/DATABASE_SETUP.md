# Database Setup and Configuration Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Database Configuration](#database-configuration)
4. [Connection Requirements](#connection-requirements)
5. [User Roles and Permissions](#user-roles-and-permissions)
6. [Environment Variables](#environment-variables)
7. [Connection Scripts](#connection-scripts)
8. [Backup and Restore](#backup-and-restore)
9. [Troubleshooting](#troubleshooting)
10. [Security Best Practices](#security-best-practices)

---

## Overview

The CMS application uses PostgreSQL as its database backend. The database server is accessible through a WireGuard VPN connection at `10.0.0.1:5432`.

**Key Information:**
- **Database Server**: PostgreSQL 14+
- **Host**: 10.0.0.1 (via WireGuard VPN)
- **Port**: 5432
- **Database Name**: cms
- **Connection Method**: WireGuard VPN required
- **SSL/TLS**: Required for production, preferred for development

---

## Prerequisites

### 1. WireGuard VPN Setup

All database connections require an active WireGuard VPN connection to reach the database server at `10.0.0.1`.

**Installation:**
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install wireguard

# macOS
brew install wireguard-tools

# Verify installation
wg --version
```

**Configuration:**
1. Obtain your WireGuard configuration file from your administrator
2. Place the configuration file at `/etc/wireguard/wg0.conf`
3. Start the VPN:
   ```bash
   sudo wg-quick up wg0
   ```
4. Verify connectivity:
   ```bash
   ping 10.0.0.1
   ```

### 2. PostgreSQL Client Installation

**Installation:**
```bash
# Ubuntu/Debian
sudo apt-get install postgresql-client

# macOS
brew install postgresql

# Verify installation
psql --version
```

### 3. Environment Variables

Copy the template and configure your credentials:
```bash
cp .env.example .env
```

Edit `.env` with your actual credentials (see [Environment Variables](#environment-variables) section).

---

## Database Configuration

### Current Database Setup

| Component | Value |
|-----------|-------|
| **Host** | 10.0.0.1 (WireGuard) |
| **Port** | 5432 |
| **Database** | cms |
| **Admin User** | postgres |
| **App User** | cmssystem |
| **SSL Mode (Production)** | require |
| **SSL Mode (Development)** | prefer |
| **Connection Pooling** | Enabled |

### Configuration Files

The application uses the following configuration files:

1. **`connectionstrings.json`** (Bootstrap configuration)
   - Contains initial connection string
   - Used to load additional configuration from database
   - **Not in version control** - must be created manually

2. **`appsettings.Development.json`** (Development environment)
   - Development database settings
   - Lower connection pool limits
   - Enhanced logging
   - SSL preferred but not required

3. **`appsettings.Production.json`** (Production environment)
   - Production database settings
   - Higher connection pool limits
   - Minimal logging
   - SSL required

4. **`.env`** (Environment variables)
   - Sensitive credentials
   - **Not in version control** - use `.env.example` as template

---

## Connection Requirements

### Network Requirements

1. **WireGuard VPN**
   - Must be active before connecting
   - Network range: `10.0.0.0/24`
   - Database server at: `10.0.0.1`

2. **Firewall Rules**
   - PostgreSQL port (5432) must be accessible through VPN
   - Only VPN clients can connect to the database

3. **DNS/Hosts**
   - Database server accessible at IP `10.0.0.1`
   - No DNS resolution required

### SSL/TLS Requirements

**Development:**
- SSL Mode: `prefer`
- Connection will work with or without SSL
- Certificate validation not strictly enforced

**Production:**
- SSL Mode: `require`
- SSL/TLS connection is mandatory
- Certificate should be valid

---

## User Roles and Permissions

### 1. Admin User (`postgres`)

**Purpose**: Database administration, backups, restore operations

**Permissions**:
- Superuser privileges
- Full database access
- Create/drop databases
- Manage users and permissions

**Usage**:
```bash
# Only use for administrative tasks
export PGPASSWORD="admin_password"
psql -h 10.0.0.1 -U postgres -d cms
```

**Security Note**: 🔒 Keep this password secure. Only use for maintenance tasks.

### 2. Application User (`cmssystem`)

**Purpose**: Day-to-day application operations

**Permissions**:
- Connect to database
- SELECT, INSERT, UPDATE, DELETE on application tables
- EXECUTE on application functions
- USAGE on application schemas
- **No superuser privileges**
- **No CREATE DATABASE**
- **No user management**

**Usage**:
```bash
# Used by the application
export DB_DEV_PASSWORD="app_password"
./scripts/db-connect.sh -e development
```

**Security Note**: 🔒 Limited permissions reduce security risk.

### Permission Setup Script

To set up the `cmssystem` user with appropriate permissions:

```sql
-- Connect as postgres
CREATE USER cmssystem WITH PASSWORD 'secure_password_here';

-- Grant connection
GRANT CONNECT ON DATABASE cms TO cmssystem;

-- Grant schema usage
GRANT USAGE ON SCHEMA public TO cmssystem;
GRANT USAGE ON SCHEMA admin TO cmssystem;

-- Grant table permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO cmssystem;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA admin TO cmssystem;

-- Grant sequence permissions
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO cmssystem;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA admin TO cmssystem;

-- Set default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO cmssystem;
ALTER DEFAULT PRIVILEGES IN SCHEMA admin GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO cmssystem;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO cmssystem;
ALTER DEFAULT PRIVILEGES IN SCHEMA admin GRANT USAGE, SELECT ON SEQUENCES TO cmssystem;
```

---

## Environment Variables

### Required Variables

Create a `.env` file from `.env.example` and configure:

```bash
# Development Database
DB_DEV_HOST=10.0.0.1
DB_DEV_PORT=5432
DB_DEV_NAME=cms
DB_DEV_USER=cmssystem
DB_DEV_PASSWORD=your_development_password

# Production Database
DB_PROD_HOST=10.0.0.1
DB_PROD_PORT=5432
DB_PROD_NAME=cms
DB_PROD_USER=cmssystem
DB_PROD_PASSWORD=your_production_password

# Admin User (for backups/restore)
DB_ADMIN_USER=postgres
DB_ADMIN_PASSWORD=your_admin_password
```

### Connection String Format

The connection strings are constructed from environment variables:

**Development:**
```
Host=10.0.0.1;Port=5432;Database=cms;Username=cmssystem;Password=${DB_DEV_PASSWORD};SSL Mode=prefer;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;
```

**Production:**
```
Host=10.0.0.1;Port=5432;Database=cms;Username=cmssystem;Password=${DB_PROD_PASSWORD};SSL Mode=require;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=200;
```

---

## Connection Scripts

### 1. Database Connection Script (`scripts/db-connect.sh`)

Connect to PostgreSQL through WireGuard VPN with automatic validation.

**Usage:**
```bash
# Connect to development
./scripts/db-connect.sh

# Connect to production
./scripts/db-connect.sh -e production

# Connect with specific user
./scripts/db-connect.sh -e development -u cmssystem

# Show help
./scripts/db-connect.sh --help
```

**Features:**
- ✅ Validates WireGuard VPN connectivity
- ✅ Tests database credentials before connecting
- ✅ Supports both development and production environments
- ✅ Loads credentials from `.env` file
- ✅ Color-coded output for easy reading

**Example Output:**
```
ℹ Conectando al entorno: development
ℹ Cargando variables de .env...
ℹ Verificando conectividad a 10.0.0.1:5432...
✓ Conectividad verificada
ℹ Validando credenciales de base de datos...
✓ Credenciales válidas

ℹ ═══════════════════════════════════════════════════
ℹ   Conectando a PostgreSQL
ℹ ═══════════════════════════════════════════════════
ℹ   Host:        10.0.0.1:5432
ℹ   Database:    cms
ℹ   User:        cmssystem
ℹ   Environment: development
ℹ   SSL Mode:    prefer
ℹ ═══════════════════════════════════════════════════

psql (14.x)
Type "help" for help.

cms=>
```

---

## Backup and Restore

### Backup Configuration

**Schedule**: Daily at 02:00 UTC  
**Location**: `/var/backups/postgresql/`  
**Retention**: 7 days  
**Format**: gzipped SQL files

### Backup Naming Convention

```
cms_backup_YYYYMMDD_HHMMSS.sql.gz
```

Example: `cms_backup_20260209_020000.sql.gz`

### Manual Backup

```bash
# Create a backup manually
pg_dump -h 10.0.0.1 -U postgres -d cms | gzip > backup/cms_backup_$(date +%Y%m%d_%H%M%S).sql.gz
```

### Restore Script (`scripts/db-restore.sh`)

Restore PostgreSQL backup with safety checks and validation.

**Usage:**

```bash
# Basic restore (development)
./scripts/db-restore.sh backup/cms_backup.sql

# Restore from gzipped file
./scripts/db-restore.sh backup/cms_backup.sql.gz

# Restore to production (creates safety backup first)
./scripts/db-restore.sh -e production backup/cms_backup.sql

# Clean restore (drops all existing data)
./scripts/db-restore.sh -c backup/cms_backup.sql

# Incremental restore (preserves existing data)
./scripts/db-restore.sh -i backup/cms_backup.sql

# Skip confirmation (use with caution!)
./scripts/db-restore.sh --no-confirm backup/cms_backup.sql

# Show help
./scripts/db-restore.sh --help
```

**Features:**
- ✅ Validates backup file exists
- ✅ Checks VPN and database connectivity
- ✅ Supports both .sql and .sql.gz files
- ✅ Creates safety backup before production restore
- ✅ Multiple restore modes (clean, incremental)
- ✅ Confirmation prompt for safety
- ✅ Detailed progress output

**Restore Modes:**

1. **Normal Mode** (default)
   - Restores all data from backup
   - May conflict with existing data
   - Use when database is empty or backup is complete

2. **Clean Mode** (`-c` / `--clean`)
   - ⚠️ **Drops all existing tables, sequences, and views**
   - Then restores from backup
   - Use for fresh installation or complete rebuild

3. **Incremental Mode** (`-i` / `--incremental`)
   - Preserves existing data
   - Only adds new data from backup
   - Use for partial restores or updates

**Example Output:**
```
ℹ Preparando restauración para: development
ℹ Cargando variables de .env...
ℹ Verificando conectividad a 10.0.0.1:5432...
✓ Conectividad verificada
ℹ Validando credenciales...
✓ Credenciales válidas

⚠ ═══════════════════════════════════════════════════
⚠   RESTAURACIÓN DE BASE DE DATOS
⚠ ═══════════════════════════════════════════════════
ℹ   Archivo:     backup/cms_backup.sql
ℹ   Host:        10.0.0.1:5432
ℹ   Database:    cms
ℹ   User:        postgres
ℹ   Environment: development
ℹ   Modo Clean:  false
ℹ   Incremental: false
⚠ ═══════════════════════════════════════════════════

¿Desea continuar? (escriba 'yes' para confirmar): yes
ℹ Restaurando backup...

✓ ════════════════════════════════════════
✓   Restauración completada exitosamente
✓ ════════════════════════════════════════
ℹ Tablas restauradas: 42
ℹ Proceso completado
```

---

## Troubleshooting

### Issue 1: Cannot Connect to Database

**Symptoms:**
```
✗ No se puede conectar a 10.0.0.1:5432
```

**Solutions:**

1. **Check WireGuard VPN:**
   ```bash
   # Check if VPN is running
   sudo wg show
   
   # If not running, start it
   sudo wg-quick up wg0
   
   # Test connectivity
   ping 10.0.0.1
   ```

2. **Check PostgreSQL Service:**
   ```bash
   # On database server
   sudo systemctl status postgresql
   
   # If not running
   sudo systemctl start postgresql
   ```

3. **Check Firewall:**
   ```bash
   # Test port connectivity
   telnet 10.0.0.1 5432
   # or
   nc -zv 10.0.0.1 5432
   ```

### Issue 2: Authentication Failed

**Symptoms:**
```
✗ No se pudo conectar a la base de datos
⚠ Verifique las credenciales en .env
```

**Solutions:**

1. **Verify credentials in `.env`:**
   ```bash
   cat .env | grep DB_
   ```

2. **Test credentials manually:**
   ```bash
   export PGPASSWORD="your_password"
   psql -h 10.0.0.1 -U cmssystem -d cms -c "SELECT 1;"
   ```

3. **Check user exists in database:**
   ```sql
   -- Connect as postgres
   SELECT usename, usesuper FROM pg_user WHERE usename = 'cmssystem';
   ```

4. **Reset user password:**
   ```sql
   -- Connect as postgres
   ALTER USER cmssystem WITH PASSWORD 'new_secure_password';
   ```

### Issue 3: SSL Connection Error

**Symptoms:**
```
FATAL: SSL connection required
```

**Solutions:**

1. **For development, change SSL mode in `.env`:**
   ```bash
   DB_DEV_SSL_MODE=prefer
   ```

2. **For production, ensure SSL is properly configured:**
   ```bash
   DB_PROD_SSL_MODE=require
   ```

3. **Check PostgreSQL SSL configuration:**
   ```sql
   SHOW ssl;
   ```

### Issue 4: Connection Pool Exhausted

**Symptoms:**
```
Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool.
```

**Solutions:**

1. **Increase pool size in appsettings:**
   ```json
   "ConnectionPool": {
     "MinSize": 10,
     "MaxSize": 200
   }
   ```

2. **Check for connection leaks:**
   ```sql
   -- See active connections
   SELECT count(*) FROM pg_stat_activity WHERE datname = 'cms';
   ```

3. **Kill idle connections:**
   ```sql
   SELECT pg_terminate_backend(pid) 
   FROM pg_stat_activity 
   WHERE datname = 'cms' 
   AND state = 'idle' 
   AND state_change < now() - interval '10 minutes';
   ```

### Issue 5: Restore Fails

**Symptoms:**
```
✗ Error durante la restauración
```

**Solutions:**

1. **Verify backup file integrity:**
   ```bash
   # For gzipped files
   gunzip -t backup/cms_backup.sql.gz
   
   # Check file size
   ls -lh backup/cms_backup.sql.gz
   ```

2. **Check database space:**
   ```sql
   SELECT pg_size_pretty(pg_database_size('cms'));
   ```

3. **Try restore with more verbose output:**
   ```bash
   gunzip -c backup/cms_backup.sql.gz | psql -h 10.0.0.1 -U postgres -d cms -v ON_ERROR_STOP=0
   ```

4. **Use clean mode if conflicts exist:**
   ```bash
   ./scripts/db-restore.sh -c backup/cms_backup.sql
   ```

### Useful Diagnostic Commands

```bash
# Check database connectivity
pg_isready -h 10.0.0.1 -p 5432 -U cmssystem

# View connection string (from application)
cat connectionstrings.json

# Check PostgreSQL logs (on server)
sudo tail -f /var/log/postgresql/postgresql-14-main.log

# List databases
psql -h 10.0.0.1 -U postgres -c "\l"

# List tables in cms database
psql -h 10.0.0.1 -U postgres -d cms -c "\dt"

# Check table sizes
psql -h 10.0.0.1 -U postgres -d cms -c "
  SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
  FROM pg_tables 
  WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
  ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC 
  LIMIT 10;
"
```

---

## Security Best Practices

### 1. Credential Management

✅ **DO:**
- Use strong, unique passwords (minimum 16 characters)
- Store credentials in `.env` file (not in version control)
- Use environment variables for production deployments
- Rotate passwords regularly (every 90 days)
- Use different passwords for development and production

❌ **DON'T:**
- Commit `.env` or `connectionstrings.json` to version control
- Share credentials via email or chat
- Use default or weak passwords
- Reuse passwords across environments
- Store passwords in application code

### 2. User Permissions

✅ **DO:**
- Use `cmssystem` user for application operations
- Limit `cmssystem` to necessary permissions only
- Use `postgres` user only for administration
- Review and audit user permissions regularly
- Grant minimum required privileges

❌ **DON'T:**
- Use superuser account for application operations
- Grant unnecessary privileges to application user
- Share admin credentials with developers
- Use same user for all operations

### 3. Network Security

✅ **DO:**
- Always use WireGuard VPN for connections
- Require SSL/TLS in production
- Monitor connection attempts and failures
- Use firewall to restrict access to database port
- Keep WireGuard configuration secure

❌ **DON'T:**
- Allow direct internet access to database
- Disable SSL/TLS in production
- Share VPN configuration files
- Use insecure network connections

### 4. Backup Security

✅ **DO:**
- Encrypt backup files at rest
- Store backups in secure location with restricted access
- Test restore procedures regularly
- Maintain backup retention policy (7 days)
- Create safety backup before production restore

❌ **DON'T:**
- Store backups in publicly accessible locations
- Keep backups indefinitely without rotation
- Skip testing restore procedures
- Store backups on same server as database

### 5. Monitoring and Auditing

✅ **DO:**
- Monitor failed login attempts
- Log all administrative operations
- Review database logs regularly
- Set up alerts for unusual activity
- Track connection pool usage

❌ **DON'T:**
- Ignore failed authentication attempts
- Disable audit logging
- Skip regular security reviews

### 6. Application Security

✅ **DO:**
- Use connection pooling to manage connections efficiently
- Implement retry logic for transient failures
- Use parameterized queries to prevent SQL injection
- Set appropriate command timeouts
- Enable detailed error logging in development

❌ **DON'T:**
- Create new connection for every request
- Ignore transient connection failures
- Concatenate SQL queries with user input
- Use unlimited timeouts
- Expose detailed errors in production

---

## Additional Resources

### PostgreSQL Documentation
- [PostgreSQL Official Documentation](https://www.postgresql.org/docs/)
- [Connection String Parameters](https://www.postgresql.org/docs/current/libpq-connect.html#LIBPQ-CONNSTRING)
- [SSL Support](https://www.postgresql.org/docs/current/ssl-tcp.html)

### WireGuard Resources
- [WireGuard Official Site](https://www.wireguard.com/)
- [WireGuard Quick Start](https://www.wireguard.com/quickstart/)

### Entity Framework Core
- [EF Core with PostgreSQL](https://www.npgsql.org/efcore/)
- [Connection Pooling](https://www.npgsql.org/doc/connection-string-parameters.html#pooling)

### Support
For additional help or issues not covered in this guide, please contact:
- **Email**: support@biti-solutions.com
- **Internal Wiki**: [Database Team Wiki](https://wiki.internal/database)

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-09  
**Maintained By**: BITI SOLUTIONS S.A.

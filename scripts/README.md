# Database Scripts

This directory contains helper scripts for PostgreSQL database operations.

## Available Scripts

### 1. `db-connect.sh` - Database Connection Script

Connect to PostgreSQL database through WireGuard VPN with automatic validation.

**Quick Start:**
```bash
# Connect to development
./scripts/db-connect.sh

# Connect to production
./scripts/db-connect.sh -e production
```

**Full Documentation:** See [DATABASE_SETUP.md](../docs/DATABASE_SETUP.md#connection-scripts)

### 2. `db-restore.sh` - Database Restore Script

Restore PostgreSQL backup from SQL or gzipped SQL file with safety checks.

**Quick Start:**
```bash
# Restore to development
./scripts/db-restore.sh backup/cms_backup.sql

# Clean restore (drops all existing data first)
./scripts/db-restore.sh -c backup/cms_backup.sql

# Restore to production (creates safety backup first)
./scripts/db-restore.sh -e production backup/cms_backup.sql
```

**Full Documentation:** See [DATABASE_SETUP.md](../docs/DATABASE_SETUP.md#backup-and-restore)

## Prerequisites

Before using these scripts, ensure you have:

1. **WireGuard VPN** - Active connection required
2. **PostgreSQL Client** - psql command available
3. **Environment Variables** - Configured in `.env` file

## Setup

1. Copy the environment template:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` with your credentials:
   ```bash
   nano .env
   ```

3. Start WireGuard VPN:
   ```bash
   sudo wg-quick up wg0
   ```

4. Test connection:
   ```bash
   ./scripts/db-connect.sh
   ```

## Help

For detailed documentation, see:
- [docs/DATABASE_SETUP.md](../docs/DATABASE_SETUP.md) - Complete database setup guide
- Run any script with `--help` flag for usage information

## Security Notes

⚠️ **Important:**
- Never commit `.env` file to version control
- Keep database passwords secure
- Use limited-privilege user (cmssystem) for application operations
- Only use admin user (postgres) for maintenance tasks

For more security best practices, see [DATABASE_SETUP.md](../docs/DATABASE_SETUP.md#security-best-practices)

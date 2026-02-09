# Connection Strings Configuration

This file (`connectionstrings.json`) is required for bootstrapping the CMS application. It should be placed in the `CMS.API` directory for local development, or at `/app/connectionstrings.json` for Docker deployments.

## Format

```json
{
  "CompanySchema": "ADMIN",
  "ConnectionString": "Host=localhost;Port=5432;Database=cms;Username=cmssystem;Password=SYScmsPOStgres1024;Include Error Detail=true"
}
```

## Fields

- **CompanySchema**: The schema name to use for loading company configuration from the database. Typically `ADMIN`.
- **ConnectionString**: PostgreSQL connection string with the following components:
  - **Host**: Database server hostname (e.g., `localhost` for local, or service name in Docker)
  - **Port**: PostgreSQL port (default: `5432`)
  - **Database**: Database name (e.g., `cms`)
  - **Username**: Database user (default: `cmssystem`)
  - **Password**: Database password (default: `SYScmsPOStgres1024`)
  - **Include Error Detail**: Set to `true` for development to get detailed error messages

## Security Note

⚠️ **IMPORTANT**: This file contains sensitive credentials and should NEVER be committed to source control. It is already excluded via `.gitignore`.

## Environment Detection

The application automatically detects the environment:
- **Docker**: Uses `/app/connectionstrings.json` (detected via `DOTNET_RUNNING_IN_CONTAINER` environment variable)
- **Local Development**: Uses `../CMS.API/connectionstrings.json` relative to the application root

## Configuration Loading Process

1. Application reads `connectionstrings.json` to get the bootstrap connection string
2. Connects to the database using this connection string
3. Loads full configuration from `[ADMIN].[COMPANY]` table based on `CompanySchema`
4. All runtime settings (Azure AD, API URLs, etc.) come from the database

## Example for Different Environments

### Local Development
```json
{
  "CompanySchema": "ADMIN",
  "ConnectionString": "Host=localhost;Port=5432;Database=cms;Username=cmssystem;Password=SYScmsPOStgres1024;Include Error Detail=true"
}
```

### Docker (with external database)
```json
{
  "CompanySchema": "ADMIN",
  "ConnectionString": "Host=host.docker.internal;Port=5432;Database=cms;Username=cmssystem;Password=SYScmsPOStgres1024;Include Error Detail=true"
}
```

### Docker Compose (with database service)
```json
{
  "CompanySchema": "ADMIN",
  "ConnectionString": "Host=postgres-service;Port=5432;Database=cms;Username=cmssystem;Password=SYScmsPOStgres1024;Include Error Detail=true"
}
```

### Production
```json
{
  "CompanySchema": "ADMIN",
  "ConnectionString": "Host=your-production-db.example.com;Port=5432;Database=cms;Username=cmssystem;Password=<secure-password>;SSL Mode=Require"
}
```

## Troubleshooting

If you see the error:
```
❌ ERROR: No se encontró 'connectionstrings.json' en: [path]
```

1. Ensure the file exists in the correct location:
   - Local: `CMS.API/connectionstrings.json`
   - Docker: `/app/connectionstrings.json`
2. Check file permissions
3. Verify the JSON format is valid
4. Ensure all required fields are present

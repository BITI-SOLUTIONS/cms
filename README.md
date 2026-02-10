# CMS - Content Management System

[![Build and Deploy Full Stack](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-full.yml/badge.svg)](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-full.yml)
[![Build and Deploy API](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-api.yml/badge.svg)](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-api.yml)
[![Build and Deploy UI](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-ui.yml/badge.svg)](https://github.com/BITI-SOLUTIONS/cms/actions/workflows/build-and-deploy-ui.yml)

Sistema de gestión de contenidos desarrollado con .NET 9.0

## 🚀 Quick Links

- **API:** http://cms.biti-solutions.com/health
- **UI:** https://cms.biti-solutions.com

## 🏗️ Architecture

- **Backend API:** ASP.NET Core 9.0 Web API
- **Frontend UI:** ASP.NET Core 9.0 MVC/Razor Pages
- **Database:** SQL Server
- **Container Registry:** Harbor (registry.biti-solutions.com)
- **Orchestration:** Kubernetes on DigitalOcean
- **CI/CD:** GitHub Actions

## 📦 Components

- **CMS.API** - REST API backend
- **CMS.UI** - Web frontend
- **CMS.Application** - Business logic layer
- **CMS.Data** - Data access layer
- **CMS.Entities** - Domain entities

## 🛠️ Development

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- SQL Server (local or remote)

### Running Locally

1. Clone the repository
2. Update `connectionstrings.json` with your database connection
3. Run the API:
   ```bash
   cd CMS.API
   dotnet run
   ```
4. Run the UI:
   ```bash
   cd CMS.UI
   dotnet run
   ```

### Building Docker Images

**API:**
```bash
docker build -f CMS.API/Dockerfile -t cms-api .
```

**UI:**
```bash
docker build -f Dockerfile -t cms-ui .
```

## 🚀 Deployment

Deployments are automated via GitHub Actions. Every push to `main` triggers:

1. **Automatic build** of Docker images
2. **Push to Harbor registry**
3. **Deployment to Kubernetes cluster**
4. **Health checks**

See [`.github/workflows/README.md`](.github/workflows/README.md) for detailed CI/CD documentation.

## 📚 Documentation

- [CI/CD Pipeline Documentation](.github/workflows/README.md)
- [Master Specifications](MASTER_ESPECIFICACIONES.md)

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Push to your branch
4. Create a Pull Request to `main`
5. Wait for automated checks to pass
6. Get approval and merge

## 📝 License

Copyright © BITI Solutions

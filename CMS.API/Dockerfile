FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solución y proyectos
COPY ["CMS.Solution.sln", "./"]
COPY ["CMS.API/CMS.API.csproj", "CMS.API/"]
COPY ["CMS.Application/CMS.Application.csproj", "CMS.Application/"]
COPY ["CMS.Data/CMS.Data.csproj", "CMS.Data/"]
COPY ["CMS.Entities/CMS.Entities.csproj", "CMS.Entities/"]
COPY ["CMS.UI/CMS.UI.csproj", "CMS.UI/"]

# Copiar connectionstrings.json
COPY ["connectionstrings.json", "CMS.API/"]

RUN dotnet restore "CMS.Solution.sln"
COPY . .
WORKDIR "/src/CMS.API"
RUN dotnet build "CMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM base AS final
COPY --from=publish /app/publish .
COPY ["connectionstrings.json", "."]
ENTRYPOINT ["dotnet", "CMS.API.dll"]

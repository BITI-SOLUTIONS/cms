# STAGE 1: BASE (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# STAGE 2: BUILD
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CMS.Solution.sln", "./"]
COPY ["CMS.API/CMS.API.csproj", "CMS.API/"]
COPY ["CMS.Application/CMS.Application.csproj", "CMS.Application/"]
COPY ["CMS.Data/CMS.Data.csproj", "CMS.Data/"]
COPY ["CMS.Entities/CMS.Entities.csproj", "CMS.Entities/"]

# ⭐ Copia la versión de producción al build context (solo a context, no sobrescribe tu solución)
COPY ["CMS.API/connectionstrings.production.json", "CMS.API/connectionstrings.json"]
COPY ["CMS.API/connectionstrings.production.json", "CMS.API/connectionstrings.production.json"]

RUN dotnet restore "CMS.Solution.sln"
COPY . .

WORKDIR "/src/CMS.API"
RUN dotnet build "CMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 3: FINAL (runtime)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# ⭐ Copia el connectionstrings.production.json y lo renombra en el contenedor final a connectionstrings.json
COPY --from=build /src/CMS.API/connectionstrings.production.json ./connectionstrings.json

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CMS.API.dll"]
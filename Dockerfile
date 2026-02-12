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

# ⭐ Copia el archivo de producción como el nombre esperado por el API
COPY ["CMS.UI/connectionstrings.production.json", "CMS.API/connectionstrings.json"]

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

# ⭐ Copia el connectionstrings de producción al contenedor final
COPY --from=build /src/CMS.API/connectionstrings.json .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CMS.API.dll"]
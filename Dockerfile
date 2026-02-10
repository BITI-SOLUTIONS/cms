FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CMS.Solution.sln", "./"]
COPY ["CMS.API/CMS.API.csproj", "CMS.API/"]
COPY ["CMS.Application/CMS.Application.csproj", "CMS.Application/"]
COPY ["CMS.Data/CMS.Data.csproj", "CMS.Data/"]
COPY ["CMS.Entities/CMS.Entities.csproj", "CMS.Entities/"]
COPY ["CMS.UI/CMS.UI.csproj", "CMS.UI/"]

# ⭐ Copiar connectionstrings de producción
COPY ["CMS.UI/connectionstrings.production.json", "CMS.API/connectionstrings.json"]

RUN dotnet restore "CMS.Solution.sln"

COPY . .

WORKDIR "/src/CMS.UI"
RUN dotnet build "CMS.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CMS.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ⭐ Copiar connectionstrings al contenedor final
COPY --from=build /src/CMS.API/connectionstrings.json .

# ⭐ Variable de entorno para forzar producción
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CMS.UI.dll"]
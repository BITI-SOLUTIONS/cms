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

COPY CMS.API/ CMS.API/
COPY CMS.Application/ CMS.Application/
COPY CMS.Data/ CMS.Data/
COPY CMS.Entities/ CMS.Entities/

COPY CMS.API/appsettings.json CMS.API/appsettings.json

WORKDIR "/src/CMS.API"
RUN dotnet restore "CMS.API.csproj"
RUN dotnet build "CMS.API.csproj" -c Release -o /app/build
RUN dotnet publish "CMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet --version

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/CMS.API/appsettings.json /app/appsettings.json

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "CMS.API.dll"]
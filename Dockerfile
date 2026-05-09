FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["DontMissPassword.API/DontMissPassword.API.csproj", "DontMissPassword.API/"]
COPY ["DontMissPassword.Application/DontMissPassword.Application.csproj", "DontMissPassword.Application/"]
COPY ["DontMissPassword.Domain/DontMissPassword.Domain.csproj", "DontMissPassword.Domain/"]
COPY ["DontMissPassword.Infrastructure/DontMissPassword.Infrastructure.csproj", "DontMissPassword.Infrastructure/"]

RUN dotnet restore "./DontMissPassword.API/DontMissPassword.API.csproj"

COPY . .

WORKDIR "/src/DontMissPassword.API"

RUN dotnet build "./DontMissPassword.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./DontMissPassword.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DontMissPassword.API.dll"]
# ========================
# BUILD STAGE (.NET 8)
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier le fichier projet et restaurer
COPY GoWheels.csproj .
RUN dotnet restore

# Copier tout le projet
COPY . .
RUN dotnet publish -c Release -o /out

# ========================
# RUNTIME STAGE (.NET 8)
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GoWheels.dll"]

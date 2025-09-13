# ============================================================================
# DOCKERFILE PARA SISTEMA DE VACACIONES - BACKEND .NET 8
# ============================================================================
# Dockerfile optimizado para Azure Container Apps con multi-stage build
# para minimizar el tamaño de la imagen final
# ============================================================================

# Etapa 1: Runtime base - imagen mínima para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Configuración para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Etapa 2: Build - imagen completa para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias (layer caching)
COPY ["SISTEMA_VACACIONES.csproj", "."]
RUN dotnet restore "SISTEMA_VACACIONES.csproj"

# Copiar código fuente y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "SISTEMA_VACACIONES.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa 3: Publish - crear distribución optimizada
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SISTEMA_VACACIONES.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa 4: Final - imagen final mínima
FROM base AS final
WORKDIR /app

# Copiar la aplicación publicada
COPY --from=publish /app/publish .

# Configurar usuario no-root para seguridad
RUN addgroup --system --gid 1001 dotnetgroup
RUN adduser --system --uid 1001 --gid 1001 dotnetuser
USER dotnetuser

# Punto de entrada
ENTRYPOINT ["dotnet", "SISTEMA_VACACIONES.dll"]
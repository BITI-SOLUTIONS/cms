#!/bin/bash
# ================================================================================
# ARCHIVO: scripts/db-connect.sh
# PROPÓSITO: Conectar a PostgreSQL a través de WireGuard VPN
# DESCRIPCIÓN: Script para conectar a la base de datos PostgreSQL con validación
# AUTOR: BITI SOLUTIONS S.A
# ================================================================================

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para imprimir mensajes
print_info() {
    echo -e "${BLUE}ℹ ${NC}$1"
}

print_success() {
    echo -e "${GREEN}✓ ${NC}$1"
}

print_warning() {
    echo -e "${YELLOW}⚠ ${NC}$1"
}

print_error() {
    echo -e "${RED}✗ ${NC}$1"
}

# Función para mostrar uso
show_usage() {
    cat << EOF
Uso: $0 [OPCIONES]

Conectar a PostgreSQL a través de WireGuard VPN

OPCIONES:
    -e, --environment ENV    Entorno (development|production). Default: development
    -u, --user USER         Usuario de base de datos. Default: desde .env
    -d, --database DB       Nombre de base de datos. Default: desde .env
    -h, --help             Mostrar esta ayuda

EJEMPLOS:
    $0                              # Conectar al entorno de desarrollo
    $0 -e production                # Conectar al entorno de producción
    $0 -e development -u cmssystem  # Conectar con usuario específico

NOTA: Este script requiere:
    - WireGuard VPN activa
    - Variables de entorno configuradas en .env
    - PostgreSQL client (psql) instalado

EOF
}

# Variables por defecto
ENVIRONMENT="development"
DB_USER=""
DB_NAME=""
DB_HOST="10.0.0.1"
DB_PORT="5432"

# Procesar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -u|--user)
            DB_USER="$2"
            shift 2
            ;;
        -d|--database)
            DB_NAME="$2"
            shift 2
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_error "Opción desconocida: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Validar entorno
if [[ "$ENVIRONMENT" != "development" && "$ENVIRONMENT" != "production" ]]; then
    print_error "Entorno inválido: $ENVIRONMENT"
    print_info "Debe ser 'development' o 'production'"
    exit 1
fi

print_info "Conectando al entorno: $ENVIRONMENT"

# Cargar variables de entorno si existe .env
if [ -f ".env" ]; then
    print_info "Cargando variables de .env..."
    export $(grep -v '^#' .env | xargs)
fi

# Establecer valores por defecto según el entorno
if [ "$ENVIRONMENT" == "production" ]; then
    DB_USER="${DB_USER:-${DB_PROD_USER:-cmssystem}}"
    DB_NAME="${DB_NAME:-${DB_PROD_NAME:-cms}}"
    DB_PASSWORD="${DB_PROD_PASSWORD}"
    REQUIRE_SSL="require"
else
    DB_USER="${DB_USER:-${DB_DEV_USER:-cmssystem}}"
    DB_NAME="${DB_NAME:-${DB_DEV_NAME:-cms}}"
    DB_PASSWORD="${DB_DEV_PASSWORD}"
    REQUIRE_SSL="prefer"
fi

# Validar que tengamos las credenciales necesarias
if [ -z "$DB_PASSWORD" ]; then
    print_error "No se encontró la contraseña de base de datos"
    print_info "Configure DB_${ENVIRONMENT^^}_PASSWORD en .env"
    exit 1
fi

# Verificar que psql esté instalado
if ! command -v psql &> /dev/null; then
    print_error "PostgreSQL client (psql) no está instalado"
    print_info "Instale con: sudo apt-get install postgresql-client"
    exit 1
fi

# Verificar conectividad a través de WireGuard
print_info "Verificando conectividad a $DB_HOST:$DB_PORT..."

if ! timeout 5 bash -c "cat < /dev/null > /dev/tcp/$DB_HOST/$DB_PORT" 2>/dev/null; then
    print_error "No se puede conectar a $DB_HOST:$DB_PORT"
    print_warning "Verifique que:"
    print_warning "  1. WireGuard VPN esté activa"
    print_warning "  2. El servidor de base de datos esté corriendo"
    print_warning "  3. El firewall permita conexiones"
    exit 1
fi

print_success "Conectividad verificada"

# Validar conexión a la base de datos
print_info "Validando credenciales de base de datos..."

export PGPASSWORD="$DB_PASSWORD"

if ! psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" > /dev/null 2>&1; then
    print_error "No se pudo conectar a la base de datos"
    print_warning "Verifique las credenciales en .env"
    exit 1
fi

print_success "Credenciales válidas"

# Mostrar información de conexión
echo ""
print_info "═══════════════════════════════════════════════════"
print_info "  Conectando a PostgreSQL"
print_info "═══════════════════════════════════════════════════"
print_info "  Host:        $DB_HOST:$DB_PORT"
print_info "  Database:    $DB_NAME"
print_info "  User:        $DB_USER"
print_info "  Environment: $ENVIRONMENT"
print_info "  SSL Mode:    $REQUIRE_SSL"
print_info "═══════════════════════════════════════════════════"
echo ""

# Conectar a la base de datos
psql "postgresql://$DB_USER@$DB_HOST:$DB_PORT/$DB_NAME?sslmode=$REQUIRE_SSL"

# Limpiar la contraseña del entorno
unset PGPASSWORD

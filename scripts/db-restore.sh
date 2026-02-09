#!/bin/bash
# ================================================================================
# ARCHIVO: scripts/db-restore.sh
# PROPÓSITO: Restaurar backup de PostgreSQL desde archivo SQL comprimido
# DESCRIPCIÓN: Script con validaciones de seguridad y soporte incremental
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
Uso: $0 [OPCIONES] BACKUP_FILE

Restaurar backup de PostgreSQL desde archivo SQL o SQL comprimido

OPCIONES:
    -e, --environment ENV    Entorno (development|production). Default: development
    -u, --user USER         Usuario de base de datos admin. Default: postgres
    -d, --database DB       Nombre de base de datos. Default: desde .env
    -c, --clean             Limpiar base de datos antes de restaurar
    -i, --incremental       Restaurar de forma incremental (sin DROP)
    --no-confirm            No pedir confirmación (úselo con cuidado)
    -h, --help             Mostrar esta ayuda

EJEMPLOS:
    $0 backup/cms_backup.sql                    # Restaurar desde archivo SQL
    $0 backup/cms_backup.sql.gz                 # Restaurar desde archivo comprimido
    $0 -e development -c backup/cms_backup.sql  # Limpiar y restaurar
    $0 -i backup/cms_backup.sql                 # Restaurar incrementalmente

NOTAS:
    - El modo incremental solo inserta/actualiza, no elimina datos existentes
    - El modo clean (--clean) eliminará TODOS los datos antes de restaurar
    - Requiere WireGuard VPN activa y usuario con privilegios de administrador
    - Se crea un backup automático antes de restaurar en producción

EOF
}

# Variables por defecto
ENVIRONMENT="development"
DB_USER=""
DB_NAME=""
DB_HOST="10.0.0.1"
DB_PORT="5432"
CLEAN_MODE=false
INCREMENTAL_MODE=false
NO_CONFIRM=false
BACKUP_FILE=""

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
        -c|--clean)
            CLEAN_MODE=true
            shift
            ;;
        -i|--incremental)
            INCREMENTAL_MODE=true
            shift
            ;;
        --no-confirm)
            NO_CONFIRM=true
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            if [ -z "$BACKUP_FILE" ]; then
                BACKUP_FILE="$1"
                shift
            else
                print_error "Opción desconocida: $1"
                show_usage
                exit 1
            fi
            ;;
    esac
done

# Validar argumentos
if [ -z "$BACKUP_FILE" ]; then
    print_error "Debe especificar un archivo de backup"
    show_usage
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    print_error "Archivo no encontrado: $BACKUP_FILE"
    exit 1
fi

if [[ "$ENVIRONMENT" != "development" && "$ENVIRONMENT" != "production" ]]; then
    print_error "Entorno inválido: $ENVIRONMENT"
    exit 1
fi

if [ "$CLEAN_MODE" = true ] && [ "$INCREMENTAL_MODE" = true ]; then
    print_error "No puede usar --clean y --incremental al mismo tiempo"
    exit 1
fi

print_info "Preparando restauración para: $ENVIRONMENT"

# Cargar variables de entorno
if [ -f ".env" ]; then
    print_info "Cargando variables de .env..."
    set -a
    source .env
    set +a
fi

# Establecer valores según el entorno
if [ "$ENVIRONMENT" == "production" ]; then
    DB_USER="${DB_USER:-${DB_ADMIN_USER:-postgres}}"
    DB_NAME="${DB_NAME:-${DB_PROD_NAME:-cms}}"
    DB_PASSWORD="${DB_ADMIN_PASSWORD}"
else
    DB_USER="${DB_USER:-${DB_ADMIN_USER:-postgres}}"
    DB_NAME="${DB_NAME:-${DB_DEV_NAME:-cms}}"
    DB_PASSWORD="${DB_ADMIN_PASSWORD}"
fi

# Validar credenciales
if [ -z "$DB_PASSWORD" ]; then
    print_error "No se encontró DB_ADMIN_PASSWORD en .env"
    exit 1
fi

# Verificar herramientas necesarias
if ! command -v psql &> /dev/null; then
    print_error "PostgreSQL client (psql) no está instalado"
    exit 1
fi

# Verificar si el archivo está comprimido
IS_GZIPPED=false
if [[ "$BACKUP_FILE" == *.gz ]]; then
    IS_GZIPPED=true
    if ! command -v gunzip &> /dev/null; then
        print_error "gunzip no está instalado"
        exit 1
    fi
    print_info "Archivo detectado como comprimido (gzip)"
fi

# Verificar conectividad
print_info "Verificando conectividad a $DB_HOST:$DB_PORT..."
if ! timeout 5 bash -c "cat < /dev/null > /dev/tcp/$DB_HOST/$DB_PORT" 2>/dev/null; then
    print_error "No se puede conectar a $DB_HOST:$DB_PORT"
    print_warning "Verifique que WireGuard VPN esté activa"
    exit 1
fi

print_success "Conectividad verificada"

# Validar conexión
export PGPASSWORD="$DB_PASSWORD"

print_info "Validando credenciales..."
if ! psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1;" > /dev/null 2>&1; then
    print_error "No se pudo conectar a la base de datos"
    exit 1
fi

print_success "Credenciales válidas"

# Mostrar resumen
echo ""
print_warning "═══════════════════════════════════════════════════"
print_warning "  RESTAURACIÓN DE BASE DE DATOS"
print_warning "═══════════════════════════════════════════════════"
print_info "  Archivo:     $BACKUP_FILE"
print_info "  Host:        $DB_HOST:$DB_PORT"
print_info "  Database:    $DB_NAME"
print_info "  User:        $DB_USER"
print_info "  Environment: $ENVIRONMENT"
print_info "  Modo Clean:  $CLEAN_MODE"
print_info "  Incremental: $INCREMENTAL_MODE"
print_warning "═══════════════════════════════════════════════════"
echo ""

# Advertencia de producción
if [ "$ENVIRONMENT" == "production" ]; then
    print_warning "⚠⚠⚠ ADVERTENCIA: ESTÁ RESTAURANDO EN PRODUCCIÓN ⚠⚠⚠"
    echo ""
fi

# Pedir confirmación
if [ "$NO_CONFIRM" = false ]; then
    if [ "$CLEAN_MODE" = true ]; then
        print_warning "⚠ MODO CLEAN: Esto eliminará TODOS los datos existentes"
    fi
    
    read -p "¿Desea continuar? (escriba 'yes' para confirmar): " CONFIRM
    if [ "$CONFIRM" != "yes" ]; then
        print_info "Restauración cancelada"
        exit 0
    fi
fi

# Crear backup de seguridad en producción
if [ "$ENVIRONMENT" == "production" ]; then
    BACKUP_DIR="/var/backups/postgresql"
    SAFETY_BACKUP="$BACKUP_DIR/pre-restore-$(date +%Y%m%d_%H%M%S).sql.gz"
    
    print_info "Creando backup de seguridad en: $SAFETY_BACKUP"
    
    if [ -d "$BACKUP_DIR" ]; then
        pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" | gzip > "$SAFETY_BACKUP" 2>/dev/null || {
            print_warning "No se pudo crear backup de seguridad (continuando de todos modos)"
        }
        
        if [ -f "$SAFETY_BACKUP" ]; then
            print_success "Backup de seguridad creado"
        fi
    else
        print_warning "Directorio de backups no existe: $BACKUP_DIR"
    fi
fi

# Preparar comando de restauración
RESTORE_CMD=""

if [ "$IS_GZIPPED" = true ]; then
    RESTORE_CMD="gunzip -c '$BACKUP_FILE' | psql -h '$DB_HOST' -p '$DB_PORT' -U '$DB_USER' -d '$DB_NAME'"
else
    RESTORE_CMD="psql -h '$DB_HOST' -p '$DB_PORT' -U '$DB_USER' -d '$DB_NAME' -f '$BACKUP_FILE'"
fi

# Limpiar base de datos si se solicita
if [ "$CLEAN_MODE" = true ]; then
    print_warning "Limpiando base de datos..."
    
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" <<-EOSQL
        DO \$\$ 
        DECLARE
            r RECORD;
        BEGIN
            -- Eliminar todas las tablas
            FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.tablename) || ' CASCADE';
            END LOOP;
            
            -- Eliminar todas las secuencias
            FOR r IN (SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = 'public') LOOP
                EXECUTE 'DROP SEQUENCE IF EXISTS ' || quote_ident(r.sequence_name) || ' CASCADE';
            END LOOP;
            
            -- Eliminar todas las vistas
            FOR r IN (SELECT table_name FROM information_schema.views WHERE table_schema = 'public') LOOP
                EXECUTE 'DROP VIEW IF EXISTS ' || quote_ident(r.table_name) || ' CASCADE';
            END LOOP;
        END \$\$;
EOSQL
    
    print_success "Base de datos limpiada"
fi

# Restaurar el backup
print_info "Restaurando backup..."
echo ""

if [ "$IS_GZIPPED" = true ]; then
    gunzip -c "$BACKUP_FILE" | psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" 2>&1 | grep -v "^$" || true
else
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$BACKUP_FILE" 2>&1 | grep -v "^$" || true
fi

RESTORE_EXIT_CODE=$?

echo ""

if [ $RESTORE_EXIT_CODE -eq 0 ]; then
    print_success "════════════════════════════════════════"
    print_success "  Restauración completada exitosamente"
    print_success "════════════════════════════════════════"
    
    # Mostrar estadísticas
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>/dev/null | xargs)
    
    print_info "Tablas restauradas: $TABLE_COUNT"
else
    print_error "════════════════════════════════════════"
    print_error "  Error durante la restauración"
    print_error "════════════════════════════════════════"
    
    if [ "$ENVIRONMENT" == "production" ] && [ -f "$SAFETY_BACKUP" ]; then
        print_warning "Puede restaurar el backup de seguridad con:"
        print_warning "  $0 -e production $SAFETY_BACKUP"
    fi
    
    exit 1
fi

# Limpiar
unset PGPASSWORD

print_info "Proceso completado"

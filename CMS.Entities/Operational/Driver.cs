// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Driver.cs
// PROPÓSITO: Entidad Conductor para Fleet Management (por compañía)
// DESCRIPCIÓN: Almacena datos del conductor, ligado opcionalmente a un usuario
//              activo del sistema. Tabla en la BD de la compañía.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    [Table("driver")]
    public class Driver
    {
        [Key]
        [Column("id_driver")]
        public int Id { get; set; }

        // ── Vínculo con usuario del sistema ───────────────────────
        /// <summary>ID del usuario del sistema (cms.admin.user) — FK lógica cross-DB</summary>
        [Column("id_system_user")]
        public int? IdSystemUser { get; set; }

        // ── Identificación personal ───────────────────────────────

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required][MaxLength(100)][Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)][Column("second_last_name")]
        public string? SecondLastName { get; set; }

        /// <summary>Nombre completo desnormalizado para búsquedas rápidas</summary>
        [MaxLength(300)][Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Número de cédula / pasaporte / identificación</summary>
        [Required][MaxLength(30)][Column("id_number")]
        public string IdNumber { get; set; } = string.Empty;

        [MaxLength(20)][Column("id_type")]
        public string IdType { get; set; } = "Cedula";   // Cedula, Pasaporte, DIMEX

        // ── Contacto ──────────────────────────────────────────────

        [MaxLength(30)][Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(30)][Column("mobile")]
        public string? Mobile { get; set; }

        [MaxLength(150)][Column("email")]
        public string? Email { get; set; }

        [MaxLength(500)][Column("address")]
        public string? Address { get; set; }

        // ── Licencia de conducir ──────────────────────────────────

        [MaxLength(30)][Column("license_number")]
        public string? LicenseNumber { get; set; }

        /// <summary>Categoría / tipo de licencia (A1, B1, B3, C, D, E, F…)</summary>
        [MaxLength(30)][Column("license_category")]
        public string? LicenseCategory { get; set; }

        [Column("license_expiry_date")]
        public DateOnly? LicenseExpiryDate { get; set; }

        // ── Datos laborales ───────────────────────────────────────

        [Column("hire_date")]
        public DateOnly? HireDate { get; set; }

        [MaxLength(100)][Column("position")]
        public string? Position { get; set; }

        [MaxLength(200)][Column("emergency_contact_name")]
        public string? EmergencyContactName { get; set; }

        [MaxLength(30)][Column("emergency_contact_phone")]
        public string? EmergencyContactPhone { get; set; }

        // ── Estado ────────────────────────────────────────────────

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(2000)][Column("notes")]
        public string? Notes { get; set; }

        // ── Auditoría ─────────────────────────────────────────────

        [Column("createdate")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("record_date")]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Required][MaxLength(30)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(30)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }
}

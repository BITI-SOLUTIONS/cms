// ================================================================================
// ARCHIVO: CMS.Entities/Operational/Insurer.cs
// PROPÓSITO: Entidad Aseguradora para Fleet Management (por compañía)
// DESCRIPCIÓN: Almacena datos de aseguradoras con las que opera la compañía.
//              Tabla en la BD de la compañía.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Entities.Operational
{
    [Table("insurer")]
    public class Insurer
    {
        [Key]
        [Column("id_insurer")]
        public int Id { get; set; }

        // ── Identificación ────────────────────────────────────────

        [Required][MaxLength(30)][Column("code")]
        public string Code { get; set; } = string.Empty;

        [Required][MaxLength(200)][Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)][Column("trade_name")]
        public string? TradeName { get; set; }

        /// <summary>Número de identificación fiscal / RUC / cédula jurídica</summary>
        [MaxLength(30)][Column("tax_id")]
        public string? TaxId { get; set; }

        // ── Contacto ──────────────────────────────────────────────

        [MaxLength(30)][Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(30)][Column("phone_claims")]
        public string? PhoneClaims { get; set; }

        [MaxLength(150)][Column("email")]
        public string? Email { get; set; }

        [MaxLength(150)][Column("website")]
        public string? Website { get; set; }

        [MaxLength(500)][Column("address")]
        public string? Address { get; set; }

        // ── Contacto ejecutivo ────────────────────────────────────

        [MaxLength(200)][Column("agent_name")]
        public string? AgentName { get; set; }

        [MaxLength(30)][Column("agent_phone")]
        public string? AgentPhone { get; set; }

        [MaxLength(150)][Column("agent_email")]
        public string? AgentEmail { get; set; }

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

        [Required][MaxLength(150)][Column("created_by")]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Required][MaxLength(150)][Column("updated_by")]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Column("rowpointer")]
        public Guid Rowpointer { get; set; } = Guid.NewGuid();
    }
}

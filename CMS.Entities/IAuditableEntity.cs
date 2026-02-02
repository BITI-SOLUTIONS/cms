namespace CMS.Entities
{
    public interface IAuditableEntity
    {
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        DateTime CreateDate { get; set; }
        DateTime RecordDate { get; set; }
    }
}
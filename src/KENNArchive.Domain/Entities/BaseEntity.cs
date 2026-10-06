namespace KENNArchive.Domain.Entities;

// Bütün əsas entity-lərdə istifadə etdiyimiz ümumi ID və tarix məlumatları.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

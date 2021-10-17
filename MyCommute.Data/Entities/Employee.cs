namespace MyCommute.Data.Entities;

public class Employee : BaseEntity
{
    [MaxLength(128)] 
    public string Name { get; set; } = null!;
        
    [MaxLength(128)] 
    public string Email { get; set; } = null!;

    public Point HomeLocation { get; set; } = null!;

    public Point DefaultWorkLocation { get; set; } = null!;
        
    public CommuteType DefaultCommuteType { get; set; }

    public ICollection<Commute> Commutes { get; set; } = null!;
}
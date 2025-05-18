using CESIZen.Data.Enums;

namespace CESIZen.Data.Entities;

public class Progression
{
    public int Id { get; set; }
    public float Percentage { get; set; }
    public DateTime LastInteractionDate { get; set; }
    public ProgressionStatus Status { get; set; }
}

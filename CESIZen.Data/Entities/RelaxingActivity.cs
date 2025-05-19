using CESIZen.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;

namespace CESIZen.Data.Entities;

public class RelaxingActivity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Duration { get; set; } 
    public bool Enabled { get; set; } = true;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public Enums.DifficultyLevel DifficultyLevel { get; set; }

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public virtual CategoryRelaxingActivity? Category { get; set; }
}

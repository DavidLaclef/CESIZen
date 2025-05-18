using CESIZen.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;

namespace CESIZen.Data.Entities;

public class InformationalArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreationDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public virtual CategoryInformation? Category { get; set; }
}

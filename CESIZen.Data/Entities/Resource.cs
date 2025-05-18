﻿using CESIZen.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;

namespace CESIZen.Data.Entities;

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public Enums.ResourceType Type { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Private;

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}

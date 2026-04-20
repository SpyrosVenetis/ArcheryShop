using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcheryShop.Models;

public partial class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    public int BrandId { get; set; }

    public string Model { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public string? ImageLink { get; set; }

    public virtual Brand? Brand { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public int Quantity { get; set; }
}

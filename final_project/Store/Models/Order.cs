using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int Id { get; set; } // Id as integer

    [Required]
    public string Name { get; set; }

    [Required]
    public string Address { get; set; }

    public decimal ChargeTotal { get; set; }
    public string OrderTotal { get; set; }

    [Required]
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();

    public int NumItemsInCart { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class CartItem
{
    [Required]
    public string CartID { get; set; }

    [Required]
    public int ProductID { get; set; }

    [Required]
    public string Company { get; set; }

    [Required]
    public string Image { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string ProductColor { get; set; }

    [Required]
    public decimal Price { get; set; } // Changed from string to decimal

    [Required]
    public int Amount { get; set; }
}

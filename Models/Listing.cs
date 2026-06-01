using System.ComponentModel.DataAnnotations;

namespace RentalsApi.Models;

public enum ListingType
{
    Apartment,
    House,
    Shop
}

public class Listing
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public ListingType Type { get; set; } = ListingType.Apartment;

    public int Price { get; set; }

    [MaxLength(200)]
    public string Location { get; set; } = "";

    [MaxLength(2000)]
    public string Description { get; set; } = "";

    [MaxLength(20)]
    public string Phone { get; set; } = "";

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double Area { get; set; }
    public bool Featured { get; set; }

    [MaxLength(8)]
    public string Emoji { get; set; } = "🏢";

    // إحداثيات الموقع على الخريطة
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

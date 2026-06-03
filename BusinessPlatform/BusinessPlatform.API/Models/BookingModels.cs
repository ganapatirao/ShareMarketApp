using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessPlatform.API.Models
{
    public class Transport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("source")]
        public string Source { get; set; } = string.Empty;

        [BsonElement("destination")]
        public string Destination { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("duration")]
        public string Duration { get; set; } = string.Empty;

        [BsonElement("operator")]
        public string Operator { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TravelPackage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("duration")]
        public string Duration { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("destinations")]
        public List<string> Destinations { get; set; } = new List<string>();

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Movie
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("genre")]
        public string Genre { get; set; } = string.Empty;

        [BsonElement("language")]
        public string Language { get; set; } = string.Empty;

        [BsonElement("duration")]
        public int Duration { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MovieShowtime
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("movieId")]
        public string? MovieId { get; set; }

        [BsonElement("movieTitle")]
        public string MovieTitle { get; set; } = string.Empty;

        [BsonElement("theater")]
        public string Theater { get; set; } = string.Empty;

        [BsonElement("showDate")]
        public DateTime ShowDate { get; set; }

        [BsonElement("showTime")]
        public string ShowTime { get; set; } = string.Empty;

        [BsonElement("screenType")]
        public string ScreenType { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty;

        [BsonElement("itemId")]
        public string? ItemId { get; set; }

        [BsonElement("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; }

        [BsonElement("bookingDate")]
        public DateTime BookingDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Confirmed";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

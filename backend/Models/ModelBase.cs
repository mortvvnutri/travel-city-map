namespace TSM.Models
{
    using Microsoft.EntityFrameworkCore;
    using System;

    public class ModelBase : DbContext
    {
        public ModelBase(DbContextOptions<ModelBase> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<PaymentOption> PaymentOptions { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Category> Categories { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? YandexId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string FirebaseToken { get; set; } = string.Empty;
    }

    public class PaymentOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Config { get; set; }
    }

    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int[]? POptions { get; set; }
        public int CategoryId { get; set; }
    }

    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public int PlaceId { get; set; }
    }

    public class RoutePoint
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? RouteId { get; set; }
        public int? PlaceId1 { get; set; }
        public int SequenceNumber { get; set; }
    }

    public class Route
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PublicTransportInfo { get; set; }
        public int? UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
        public bool? IsRead { get; set; }
        public int? UserId { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}

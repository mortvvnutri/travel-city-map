namespace TSM.Models
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;

    public class ModelBase : DbContext
    {
        public ModelBase(DbContextOptions<ModelBase> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<CustomPlace> CustomPlaces { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RoutesHistory> RoutesHistories { get; set; }
        public DbSet<Category> Categories { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Pwd { get; set; } = string.Empty;
        public string Pic { get; set; } = string.Empty;
        public string? YandexIdToDo { get; set; }
        public List<long>? PreferredCats { get; set; } // Измените на long
        public long? DefCustomPlace { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string FirebaseToken { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }



    public class CustomPlace
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Long { get; set; }
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Place
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public long CategoryId { get; set; }
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Feedback
    {
        public long UserId { get; set; }
        public long PlaceId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public float Rating { get; set; }
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Route
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<long> Places { get; set; } = new List<long>();
        public List<long> Categories { get; set; } = new List<long>();
        public int TimesCompleted { get; set; }
        public string TotalDistance { get; set; } = string.Empty;
        public string StartP { get; set; } = string.Empty;
        public string EndP { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string RouteData { get; set; } = string.Empty;
        public string TimeTook { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class RoutesHistory
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RouteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? ParentId { get; set; }
        public string Meta { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}

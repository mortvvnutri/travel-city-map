using Microsoft.EntityFrameworkCore;

namespace TSM.Models
{
    public class YourDbContext : DbContext
    {
        public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<CustomPlace> CustomPlaces { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RoutesHistory> RoutesHistories { get; set; }
    }
}

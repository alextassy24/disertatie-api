using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApiDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
    {
        public DbSet<Location> Locations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Wearer> Wearers { get; set; }
        public DbSet<Token> Tokens {get;set;}
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Greenfield.Models;

namespace Greenfield.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Greenfield.Models.Basket> Basket { get; set; } = default!;
        public DbSet<Greenfield.Models.BasketProduct> BasketProduct { get; set; } = default!;
        public DbSet<Greenfield.Models.Order> Order { get; set; } = default!;
        public DbSet<Greenfield.Models.OrderProduct> OrderProduct { get; set; } = default!;
        public DbSet<Greenfield.Models.Producer> Producer { get; set; } = default!;
        public DbSet<Greenfield.Models.Product> Product { get; set; } = default!;
    }
}

using KnowItAllWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowItAllWebApp.DataAccess
{
    public class KnowItAllContext : DbContext 
    {
        public KnowItAllContext(DbContextOptions<KnowItAllContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Material>();
            builder.Entity<Offer>();
            builder.Entity<OfferMaterial>();
        }

        public DbSet<Offer> Offer { get; set; }
        public DbSet<Material> Material { get; set; }
        public DbSet<OfferMaterial> OfferMaterial { get; set; }
    }
}

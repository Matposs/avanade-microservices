using Microsoft.EntityFrameworkCore;
using Vendas.Service.Modelo;

namespace Vendas.Service.Infra
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> opts) : base(opts) { }
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<ItemPedido> ItemsPedido { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pedido>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.CriadoEm).HasDefaultValueSql("GETUTCDATE()");
                e.Property(p => p.Status).IsRequired();
                e.HasMany(p => p.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemPedido>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
            });
        }
    }
}

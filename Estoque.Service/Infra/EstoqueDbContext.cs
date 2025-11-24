using Microsoft.EntityFrameworkCore;
using Estoque.Service.Modelo;

namespace Estoque.Service.Infra
{
    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> opcoes)
           : base(opcoes) { }
        public DbSet<Produto> Produtos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Produto>(entidade =>
            {
                entidade.ToTable("Produtos");

                entidade.HasKey(p => p.Id);

                entidade.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                entidade.Property(p => p.Descricao)
                    .HasMaxLength(500);

                entidade.Property(p => p.Preco)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entidade.Property(p => p.Quantidade)
                    .IsRequired();

                entidade.Property(p => p.CriadoEm)
                    .HasDefaultValueSql("GETUTCDATE()");

                entidade.Property(p => p.AtualizadoEm)
                    .IsRequired(false);
            });
        }
    }
}
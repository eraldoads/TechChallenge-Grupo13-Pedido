using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class MySQLContextProduto : DbContext
    {
        public MySQLContextProduto(DbContextOptions<MySQLContextProduto> options) : base(options) { }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Aqui você pode configurar as opções do DbContext, como a string de conexão, o provedor do banco de dados, etc.
            if (!optionsBuilder.IsConfigured)
            {
                // Configurações do DbContext.
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração das entidades do modelo, incluindo chaves primárias, chaves estrangeiras e outros relacionamentos.
            modelBuilder.Entity<Produto>().HasKey(p => p.IdProduto);

        }

        public DbSet<Produto>? Produto { get; set; }
    }
}

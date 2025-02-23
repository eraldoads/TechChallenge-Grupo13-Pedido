﻿using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

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
            modelBuilder.Entity<Pedido>().HasKey(p => p.IdPedido);
            modelBuilder.Entity<Combo>().HasKey(c => c.IdCombo);
            modelBuilder.Entity<ComboProduto>().HasKey(pc => pc.IdProdutoCombo);

        }

        public DbSet<Pedido>? Pedido { get; set; }
        public DbSet<Combo>? Combo { get; set; }
        public DbSet<ComboProduto>? ComboProduto { get; set; }
    }
}

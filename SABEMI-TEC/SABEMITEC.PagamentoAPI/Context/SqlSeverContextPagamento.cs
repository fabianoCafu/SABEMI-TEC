using Microsoft.EntityFrameworkCore;
using SABEMITEC.PagamentoAPI.Model;

namespace SABEMITEC.PagamentoAPI.Context
{
    public class SqlSeverContextPagamento
        : DbContext
    {
        public SqlSeverContextPagamento(DbContextOptions<SqlSeverContextPagamento> options)
            : base(options) { }

        public DbSet<EventoBruto> ? LogEventosBruto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Create Table LogEventosBrutos

            modelBuilder.Entity<EventoBruto>(entity =>
            {
                entity.ToTable("LogEventosBruto");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .IsRequired();

                entity.Property(e => e.Payload) 
                      .HasColumnType("nvarchar(max)")
                      .IsRequired();

                entity.Property(e => e.DataRecebimento)
                      .IsRequired();
            });

           #endregion
        }
    }
}



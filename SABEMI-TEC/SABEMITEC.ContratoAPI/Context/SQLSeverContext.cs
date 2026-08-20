using Microsoft.EntityFrameworkCore;
using SABEMITEC.ContratoAPI.Models;

namespace SABEMITEC.ContratoAPI.Context
{
    public class SQLSeverContext : DbContext
    {
        public SQLSeverContext(DbContextOptions<SQLSeverContext> options)
            : base(options) { }

        public DbSet<StatusContrato>? StatusContrato { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Create Table StatusContrato

            modelBuilder.Entity<StatusContrato>(entity =>
            {
                entity.ToTable("StatusContrato");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.IdTransacao)
                      .HasMaxLength(100)
                      .IsRequired(false);

                entity.Property(e => e.IdContrato)
                      .HasMaxLength(100)
                      .IsRequired(false);

                entity.Property(e => e.Status)
                      .HasMaxLength(50)
                      .IsRequired(false);

                entity.Property(e => e.Falha)
                      .HasMaxLength(500)
                      .IsRequired(false);

                entity.Property(e => e.DataProcessamento)
                      .IsRequired();
            });

            #endregion
        }
    }
}

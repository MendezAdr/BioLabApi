using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BioLabApi.Models;
using BioLabApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BioLabApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<UsuarioModel> Usuarios { get; set; } = null!;
    public DbSet<RolModel> Roles { get; set; } = null!;
    public DbSet<PacienteModel> Pacientes { get; set; } = null!;
    public DbSet<ExamenModel> Examenes { get; set; } = null!;
    public DbSet<OrdenesModel> Ordenes { get; set; } = null!;
    public DbSet<DetalleModel> Detalles { get; set; } = null!;
    public DbSet<PagosModel> Pagos { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source = Laboratorio.Db")
        .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Seeding de Roles
        // 2. Seeding de Usuario Administrador Inicial



        // 3. Relación Orden -> Detalles (¡Solo una vez!)
        modelBuilder.Entity<DetalleModel>()
            .HasOne(d => d.Orden)
            .WithMany(o => o.Detalles)
            .HasForeignKey(d => d.OrdenId)
            .OnDelete(DeleteBehavior.Cascade); // Recomendado para que si borras una orden, se borren sus detalles

        // 4. Relación Orden -> Pagos
        modelBuilder.Entity<PagosModel>()
            .HasOne(p => p.Orden)
            .WithMany(o => o.Pagos)
            .HasForeignKey(p => p.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

    // ==============================================================
    // EL MOTOR DE AUDITORÍA AUTOMÁTICA
    // ==============================================================
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Auditable && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var auditable = (Auditable)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                auditable.FechaCreacion = DateTime.Now;
                // Nota: El CreadoPorId debe venir lleno desde el Servicio antes de llegar aquí.
            }
            else
            {
                // Si se está modificando, actualizamos la fecha y protegemos la fecha original de creación
                auditable.FechaModificacion = DateTime.Now;
                entityEntry.Property(nameof(Auditable.FechaCreacion)).IsModified = false;
                entityEntry.Property(nameof(Auditable.CreadoPorId)).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
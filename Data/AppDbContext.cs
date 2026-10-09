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
        string dbPath;

#if DEBUG
        dbPath = "Laboratorio.db";
#else
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "RIV_CARR_DATA_Production"); 
        
        if (!Directory.Exists(folder)) 
        {
            Directory.CreateDirectory(folder);
        }
        
        dbPath = Path.Combine(folder, "Laboratorio.db");
#endif

        options.UseSqlite($"Data Source={dbPath}")
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ==============================================================
        // 1. SEEDING DE SEGURIDAD (Roles y Usuarios)
        // ==============================================================
        var fechaSeed = new DateTime(2026, 1, 1, 12, 0, 0); // Fecha estática para evitar migraciones fantasma

        modelBuilder.Entity<RolModel>().HasData(
            new RolModel
            {
                Id = 1,
                RolName = "Admin",
                Permisos = RolModel.PermisosSistema.Todos // Simplificado si ya tienes 'Todos'
            },
            new RolModel
            {
                Id = 2,
                RolName = "Cajero",
                Permisos = RolModel.PermisosSistema.Totalizar | RolModel.PermisosSistema.CrearOrdenesYDetalles | RolModel.PermisosSistema.GestionarPagos
            }
        );

        modelBuilder.Entity<UsuarioModel>().HasData(
            new UsuarioModel
            {
                Id = 1,
                Username = "admin",
                Nombre = "Admin",
                Apellido = "Principal",
                Cedula = "V-0000000",
                RolId = 1,
                Contrasena = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsActive = true,
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        // ==============================================================
        // 2. SEEDING DE OPERACIONES (Pacientes y Exámenes)
        // ==============================================================
        modelBuilder.Entity<PacienteModel>().HasData(
            new PacienteModel
            {
                Id = 1,
                Nombre = "Juan",
                Apellido = "Pérez",
                Cedula = "V-12345678",
                FechaNacimiento = new DateTime(1990, 5, 15),
                Sexo = "M",
                Telefono = "0414-1234567",
                Direccion = "Centro, Valera",
                IsActive = true,
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        modelBuilder.Entity<ExamenModel>().HasData(
            new ExamenModel
            {
                Id = 1,
                NombreExamen = "Hematología Completa",
                CostoEnDivisa = 15.50m,
                Descripcion = "Análisis de sangre estándar con fórmula leucocitaria.",
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            },
            new ExamenModel
            {
                Id = 2,
                NombreExamen = "Perfil Lipídico",
                CostoEnDivisa = 22.00m,
                Descripcion = "Colesterol total, HDL, LDL y Triglicéridos.",
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        // ==============================================================
        // 3. SEEDING DE TRANSACCIONES (Órdenes, Detalles y Pagos)
        // ==============================================================
        modelBuilder.Entity<OrdenesModel>().HasData(
            new OrdenesModel
            {
                Id = 1,
                NumeroFactura = "ORD-10001",
                PacienteId = 1,
                TotalDivisa = 37.50m, // 15.50 + 22.00
                TasaBcv = 36.50m,
                Fecha = fechaSeed,
                Estado = OrdenesModel.EstadoPago.Parcial, // Forzamos un pago parcial para probar tu vista de morosos
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        modelBuilder.Entity<DetalleModel>().HasData(
            new DetalleModel
            {
                Id = 1,
                OrdenId = 1,
                ExamenId = 1,
                PrecioMomentoDivisa = 15.50m,
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            },
            new DetalleModel
            {
                Id = 2,
                OrdenId = 1,
                ExamenId = 2,
                PrecioMomentoDivisa = 22.00m,
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        modelBuilder.Entity<PagosModel>().HasData(
            new PagosModel
            {
                Id = 1,
                OrdenId = 1,
                Metodo = PagosModel.MetodoPago.PagoMovil,
                Monto = 20.00m, // Paga 20, queda debiendo 17.50
                Referencia = "000123456",
                CreadoPorId = 1,
                FechaCreacion = fechaSeed
            }
        );

        // ==============================================================
        // CONFIGURACIÓN DE RELACIONES (Fluent API)
        // ==============================================================
        modelBuilder.Entity<DetalleModel>()
            .HasOne(d => d.Orden)
            .WithMany(o => o.Detalles)
            .HasForeignKey(d => d.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PagosModel>()
            .HasOne(p => p.Orden)
            .WithMany(o => o.Pagos)
            .HasForeignKey(p => p.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

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
            }
            else
            {
                auditable.FechaModificacion = DateTime.Now;
                entityEntry.Property(nameof(Auditable.FechaCreacion)).IsModified = false;
                entityEntry.Property(nameof(Auditable.CreadoPorId)).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

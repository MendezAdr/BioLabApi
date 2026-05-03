using System;
using BioLabProject.Models;
using BioLabProject.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace BioLabProject.Data;

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
        options.UseSqlite("Data Source = Laboratorio.Db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<DetalleModel>()
            .HasOne(d => d.Orden)
            .WithMany() // Una orden puede tener muchos detalles
            .HasForeignKey(d => d.OrdenId);

        // 2. Seeding de Roles (Vital para tu sistema de Login)
        modelBuilder.Entity<RolModel>().HasData(
            new RolModel { Id = 1, RolName = "Administrador", Permisos = RolModel.PermisosSistema.HacerCierre},
            new RolModel { Id = 2, RolName = "Bioanalista", Permisos = RolModel.PermisosSistema.CrearVenta}
        );

        // 3. Usuario administrador inicial (Password en texto plano solo para el ejemplo, usa hashing luego)
        modelBuilder.Entity<UsuarioModel>().HasData(
            new UsuarioModel { Id = 1, Nombre = "Adrian", Apellido = "Mendez", Cedula = "12345678", RolId = 1, Contrasena = PasswordHasher.PasswordHash("1234567890") }
        );

        // Configurar que una Orden tiene muchos Detalles
        modelBuilder.Entity<DetalleModel>()
            .HasOne(d => d.Orden)
            .WithMany(o => o.Detalles)
            .HasForeignKey(d => d.OrdenId);

        // Configurar que una Orden tiene muchos Pagos 
        modelBuilder.Entity<PagosModel>()
            .HasOne(p => p.Orden)
            .WithMany(o => o.Pagos)
            .HasForeignKey(p => p.OrdenId)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }

}

using BioLabApi.Data;
using BioLabApi.Services.Interfaces;
using BioLabApi.Services.Servicios;
using BioLabApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE LA BASE DE DATOS
// ==========================================
// Registramos el AppDbContext. 
builder.Services.AddDbContext<AppDbContext>();

// ==========================================
// 2. INYECCIÓN DE DEPENDENCIAS (SERVICIOS)
// ==========================================
// Aquí le decimos a .NET: "Cuando un Controlador pida una Interfaz (I...), 
// entrégale la Implementación (...Service)".
// Usamos AddScoped para que se cree una nueva instancia por cada petición HTTP.

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPacientesService, PacienteService>();
builder.Services.AddScoped<IExamenesService, ExamenesService>(); 
builder.Services.AddScoped<IDetalleService, DetalleService>();
builder.Services.AddScoped<IPagosService, PagosService>();
builder.Services.AddScoped<IOrdenesService, OrdenesService>();

// Registrar HttpClient para hacer peticiones limpias
builder.Services.AddHttpClient();

// Registrar la clase helper como Singleton (una sola instancia en toda la app)
builder.Services.AddSingleton<GetDollarPrice>();

// Registrar el proceso en segundo plano
builder.Services.AddHostedService<DollarUpdateWorker>();

// ==========================================
// 3. CONFIGURACIÓN DE CORS (Para Tauri)
// ==========================================
// Esto es vital. Sin esto, el frontend en Tauri (Node.js) será bloqueado 
// por el navegador interno cuando intente hacer peticiones a la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTauriApp", policy =>
    {
        policy.AllowAnyOrigin()   // Permite peticiones desde cualquier origen (localhost, archivo local, etc)
              .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();  // Permite cualquier tipo de cabecera (JSON, tokens, etc.)
    });
});

// ==========================================
// 4. HABILITAR CONTROLADORES
// ==========================================
// Le decimos a la API que busque clases que hereden de ControllerBase
builder.Services.AddControllers();

// (Opcional pero recomendado) Swagger para probar la API sin frontend
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 5. CONFIGURACIÓN DEL PIPELINE (MIDDLEWARES)
// ==========================================

// Aplicar la política de CORS que creamos arriba (DEBE ir antes de MapControllers)
app.UseCors("AllowTauriApp");

// Habilitar Swagger (La interfaz web para probar tus endpoints)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Rutas automáticas para los controladores
app.MapControllers();

// ¡Arrancar el servidor!
app.Run();

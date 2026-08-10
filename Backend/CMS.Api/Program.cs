using System.Text;
using System.Text.Json.Serialization;
using CMS.Api;
using CMS.Application.Auth;
using CMS.Application.Comercios;
using CMS.Application.DBInterfaces;
using CMS.Application.Interacciones;
using CMS.Application.Oportunidad;
using CMS.Application.TiposInteraccion;
using CMS.Infrastructure.Ai;
using CMS.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CMS Api",
            Description = "Gestor de Comercios - API de prueba para Zoco",
            Version = "v1",
        });
        c.SchemaFilter<StringEnumSchemaFilter>();
    });
}

// Base de datos
builder.Services.AddDbContext<CmsDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Repositorio
builder.Services.AddScoped<IRepository, EfRepository>();

// Configuración y autenticación JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");
builder.Services.AddSingleton(jwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
builder.Services.AddAuthorization();

// Services de Application
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITipoInteraccionService, TipoInteraccionService>();
builder.Services.AddScoped<IComercioService, ComercioService>();
builder.Services.AddScoped<IInteraccionService, InteraccionService>();
builder.Services.AddScoped<AnalizadorHeuristico>();
builder.Services.AddScoped<IAnalisisOportunidadService, AnalisisOportunidadService>();

// Proveedor de IA (Gemini)
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Manejo de errores centralizado
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS (orígenes del frontend, ver Cors:AllowedOrigins en appsettings)
builder.Services.AddCors(o =>
{
    o.AddPolicy("front", p =>
    {
        var origenes = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod();
    });
});

// Headers de proxy (Render/nginx): permite que UseHttpsRedirection y los logs
// vean el esquema y la IP original del cliente detrás del balanceador.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Debe ir lo antes posible en el pipeline, antes de cualquier middleware que use el esquema.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS Api v1"));
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors("front");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check sin autenticación para Render.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// Garantiza los usuarios iniciales (admin/ventas) para que exista al menos
// un usuario con acceso a la API desde el primer arranque. No es fatal: si la
// base aún no existe o no tiene las migraciones aplicadas, la API igual levanta
// y se reintenta en el próximo arranque.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
    try
    {
        await SeedUsuarios.EnsureAsync(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "No se pudieron inicializar los usuarios por defecto. Verificá que la base exista y esté migrada: dotnet ef database update --project CMS.Infrastructure --startup-project CMS.Api");
    }
}

app.Run();

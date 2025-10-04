using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portal_Academico.Data;
using Portal_Academico.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

try
{
    var redisHost = builder.Configuration["Redis:Host"];
    var redisPort = builder.Configuration.GetValue<int>("Redis:Port");
    var redisUser = builder.Configuration["Redis:User"];
    var redisPassword = builder.Configuration["Redis:Password"];

    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { { redisHost!, redisPort } },
        User = redisUser,
        Password = redisPassword,
        Ssl = false,
        AbortOnConnectFail = false, // No fallar si Redis no está disponible
        ConnectTimeout = 5000
    };

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions = configurationOptions;
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        ConnectionMultiplexer.Connect(configurationOptions));
}
catch (Exception ex)
{
    // Si hay error con Redis, usar cache en memoria como fallback
    builder.Services.AddMemoryCache();
    Console.WriteLine($"Redis no disponible, usando cache en memoria: {ex.Message}");
}

builder.Services.Configure<RedisConfiguration>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AppTrade.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.IsEssential = true;
});

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (InvalidOperationException ex) when (ex.Message?.Contains("PendingModelChangesWarning") == true || ex.Message?.Contains("pending changes") == true)
    {
        // Log and continue: in development you may need to add a migration instead of auto-migrating
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Hay cambios pendientes en el modelo. Crea una migración con 'dotnet ef migrations add <Name>' antes de actualizar la base de datos.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error aplicando migraciones automáticas.");
        // rethrow in non-development scenarios if desired
        if (!app.Environment.IsDevelopment()) throw;
    }
    var userManager = services.GetRequiredService<UserManager<Usuario>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.Initialize(services, userManager, roleManager);
}
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

internal class RedisConfiguration
{
}
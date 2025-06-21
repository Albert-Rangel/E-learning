using E_learning.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Esto ya estaba en tu archivo

builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlString"));
});

// Tu configuración original de autenticación de cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Access/LogIn";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // Puedes ajustar esto
        options.AccessDeniedPath = "/Home/AccessDenied"; // Opcional: Ruta para acceso denegado
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// *** IMPORTANTE: El orden es crucial para la autenticación ***
app.UseAuthentication(); // Primero: Identifica quién es el usuario
app.UseAuthorization();  // Segundo: Determina si el usuario tiene permiso para acceder

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=LogIn}/{id?}"); // Tu ruta de login personalizada

app.Run();


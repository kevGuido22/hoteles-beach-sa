using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//configuracion  autenticación por medio de cookies en la app web
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(config => {
    config.Cookie.Name = "CookieAuthentication";
    config.LoginPath = "/Usuarios/Login";
    config.Cookie.HttpOnly = true;
    config.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    config.AccessDeniedPath = "/Usuarios/AccessDenied";
    config.SlidingExpiration = true;
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient();

// Add http client to the container to be used in the controllers to make requests to the Reservaciones API
builder.Services.AddHttpClient("ReservacionesHttpClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7016/api/");
});

builder.Services.AddHttpClient("GometaHttpClient", client =>
{
    client.BaseAddress = new Uri("http://apis.gometa.org/tdc/tdc.json");
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

app.UseSession();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

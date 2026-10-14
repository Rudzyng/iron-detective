using DotNetEnv;
using SearchingForItems.Data;
using SearchingForItems.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
var gamePathBase = Environment.GetEnvironmentVariable("GAME_PATH_BASE")?.TrimEnd('/') ?? string.Empty;

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<LocationRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<ApiService>();

builder.Services.AddDistributedMemoryCache(); // Для хранения сессий в памяти
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = Environment.GetEnvironmentVariable("GAME_COOKIE_NAME")
        ?? "lingvogameos_iron_detective_session";
    options.Cookie.Path = string.IsNullOrEmpty(gamePathBase) ? "/" : gamePathBase;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!string.IsNullOrEmpty(gamePathBase))
{
    app.Use((context, next) =>
    {
        context.Request.PathBase = gamePathBase;
        return next();
    });
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

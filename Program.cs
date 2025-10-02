using ASP_32.Data;
using ASP_32.Middleware.Auth;
using ASP_32.Services.Auth;
using ASP_32.Services.Kdf;
using ASP_32.Services.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IKdfService, PbKdf1Service>();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
));
builder.Services.AddScoped<DataAccessor>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, SessionAuthService>();
builder.Services.AddSingleton<IStorageService, DiskStorageService>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
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
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.MapStaticAssets();

app.UseSession();
app.UseSessionAuth();

app.UseJwtAuth();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

/* Null-Safety
 * Мовні конструкції для спрощення роботи з NULLable типами
 * 
 * x!   Null-Checking - скорочена форма для винятку (NullReferenceEx)
 * 
 * x ?? y ?? w   Null-Coalescence - повертає перший аргумент, що не є NULL
 * 
 * x?.y   Null-Forgiving (Null-Propagation) - повертає NULL, якщо
 *        x == null, інакше x.y
 *        
 * x ??= y   Null-Initialization якщо x == null, то здійснюється 
 *           присвоєння x = y, інакше інструкція ігнорується
 */

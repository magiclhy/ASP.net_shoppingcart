
//using Microsoft.AspNetCore.Hosting;

//namespace ShoppingCart
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseStartup<Startup>();
//                });
//    }
//}

using Microsoft.EntityFrameworkCore;
using ShoppingCart.DBs;
using ShoppingCart.Middlewares;
using ShoppingCart.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddDbContext<DbShoppingCart>(options =>
//    options.UseLazyLoadingProxies()
//           .UseSqlServer(builder.Configuration.GetConnectionString("DbConn")));

// Add custom services as singleton
builder.Services.AddSingleton<Hasher>();
builder.Services.AddSingleton<Verify>();

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

app.UseAuthorization();

// Custom middleware for session management
app.UseMiddleware<SessionKeeper>();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

// 初始化
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<DbShoppingCart>();
//    db.Database.EnsureDeleted();  // Ensure the database is deleted
//    db.Database.EnsureCreated();  // Ensure the database is created
//    new DbSeeder(db).Seed();      // Seed the database with initial data
//}

app.Run();


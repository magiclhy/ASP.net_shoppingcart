using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShoppingCart.DBs;
using ShoppingCart.Models;
using ShoppingCart.Middlewares;
namespace ShoppingCart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<DbShoppingCart>(opt =>
                opt.UseLazyLoadingProxies().UseSqlServer(
                    Configuration.GetConnectionString("DbConn")
                    ));

            services.AddSingleton<Hasher>();

            services.AddSingleton<Verify>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DbShoppingCart db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<SessionKeeper>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });

            // Comment next line away if you don't want to restart the database every time
            db.Database.EnsureDeleted();

            db.Database.EnsureCreated();
            // Comment next line away to not reseed database
            new DbSeeder(db).Seed();
        }
    }
}

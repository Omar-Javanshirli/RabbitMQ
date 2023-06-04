using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Web.ExcelCreate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.Web.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
          var host=  CreateHostBuilder(args).Build();

            //Startup terefinde ki servicedere catmag ucun CreateScope methodunnan istifade olunur.
            using (var scope = host.Services.CreateScope())
            {
                var appDbContext=scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                appDbContext.Database.Migrate();

                if (!appDbContext.Users.Any())
                {
                    userManager.CreateAsync(new IdentityUser() { UserName = "Omer", Email = "omer@gmail.com" }, "Password123*").Wait();
                    userManager.CreateAsync(new IdentityUser() { UserName = "Amin", Email = "amin@gmail.com" }, "Password123*").Wait();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

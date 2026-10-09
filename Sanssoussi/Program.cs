using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Data;

namespace Sanssoussi
{
    public class Program
    {
        public const string AdminRole = "admin";

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await MigrateDatabaseAsync(host);
            await SeedAdminRoleAsync(host);
            await host.RunAsync();
        }

        // Brings the database schema up to date with the migrations of the project.
        private static async Task MigrateDatabaseAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SanssoussiContext>().Database.MigrateAsync();
        }

        // Creates the admin role and assigns it to the account named by the "AdminEmail" setting, if any.
        private static async Task SeedAdminRoleAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SanssoussiUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            var adminEmail = configuration["AdminEmail"];
            if (string.IsNullOrEmpty(adminEmail))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin != null && !await userManager.IsInRoleAsync(admin, AdminRole))
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                // Logs go to the console and to a structured (JSON) file, one file per day, kept for 90 days.
                // Every entry carries its timestamp.
                .UseSerilog(
                    (context, logger) => logger
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                        .WriteTo.Console()
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            Path.Combine(context.Configuration["LogDirectory"] ?? "logs", "sanssoussi-.json"),
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 90))
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
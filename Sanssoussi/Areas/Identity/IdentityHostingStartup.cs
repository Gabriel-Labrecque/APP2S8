using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Sanssoussi.Areas.Identity;
using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Data;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace Sanssoussi.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        // NIST SP 800-63B recommends at least 15 characters when the password is the only factor
        public const int MinPasswordLength = 15;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(
                (context, services) =>
                {
                    services.AddDbContext<SanssoussiContext>(
                        options =>
                            options.UseSqlite(
                                context.Configuration.GetConnectionString("SanssoussiContextConnection")));

                    services.AddDefaultIdentity<SanssoussiUser>(
                            options =>
                            {
                                options.SignIn.RequireConfirmedAccount = true;
                                options.Password.RequiredLength = MinPasswordLength;

                                // The account is locked for 5 minutes after 5 failed sign-in attempts
                                options.Lockout.MaxFailedAccessAttempts = 5;
                                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                            })
                        .AddRoles<IdentityRole>()
                        .AddEntityFrameworkStores<SanssoussiContext>();
                });
        }
    }
}
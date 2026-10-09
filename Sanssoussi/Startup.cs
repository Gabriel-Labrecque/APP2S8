using System;
using System.Security.Claims;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sanssoussi.Services;

namespace Sanssoussi
{
    public class Startup
    {
        public const string RateLimitPolicy = "member";

        public const string GoogleScheme = "Google";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private static ILogger Logger(HttpContext context)
        {
            return context.RequestServices.GetRequiredService<ILogger<Startup>>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddScoped<CommentService>();

            // At most 30 requests per minute for each member (or each address, for anonymous visitors)
            services.AddRateLimiter(
                options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    options.OnRejected = (context, _) =>
                    {
                        Logger(context.HttpContext).LogWarning(
                            "Rate limit exceeded on {Path} for {UserId} from {Ip}",
                            context.HttpContext.Request.Path.Value,
                            context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                            context.HttpContext.Connection.RemoteIpAddress?.ToString());
                        return default;
                    };
                    options.AddPolicy(
                        RateLimitPolicy,
                        context => RateLimitPartition.GetFixedWindowLimiter(
                            context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? context.Connection.RemoteIpAddress?.ToString()
                            ?? "anonymous",
                            _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
                });

            services.AddHsts(
                options =>
                {
                    options.MaxAge = TimeSpan.FromDays(365);
                    options.IncludeSubDomains = true;
                });

            // Authentication and anti-forgery cookies are only ever sent over HTTPS
            services.ConfigureApplicationCookie(
                options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    // The session ends after 30 minutes without activity
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;

                    // Refused accesses are logged before the usual redirection
                    var redirectToLogin = options.Events.OnRedirectToLogin;
                    options.Events.OnRedirectToLogin = context =>
                    {
                        Logger(context.HttpContext).LogWarning(
                            "Anonymous access refused to {Path} from {Ip}",
                            context.Request.Path.Value,
                            context.HttpContext.Connection.RemoteIpAddress?.ToString());
                        return redirectToLogin(context);
                    };
                    var redirectToAccessDenied = options.Events.OnRedirectToAccessDenied;
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        Logger(context.HttpContext).LogWarning(
                            "Access denied to {Path} for {UserId} from {Ip}",
                            context.Request.Path.Value,
                            context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                            context.HttpContext.Connection.RemoteIpAddress?.ToString());
                        return redirectToAccessDenied(context);
                    };
                });
            services.AddAntiforgery(options => options.Cookie.SecurePolicy = CookieSecurePolicy.Always);

            // OWASP recommends at least 210,000 iterations for PBKDF2-HMAC-SHA512
            services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);

            // Authentication delegated to Google with OpenID Connect (authorization code flow with PKCE).
            // The client id and secret are supplied by the environment, never stored in the code.
            var google = this.Configuration.GetSection("Authentication:Google");
            if (!string.IsNullOrEmpty(google["ClientId"]))
            {
                services.AddAuthentication().AddOpenIdConnect(
                    GoogleScheme,
                    GoogleScheme,
                    options =>
                    {
                        options.SignInScheme = IdentityConstants.ExternalScheme;
                        options.Authority = "https://accounts.google.com";
                        options.ClientId = google["ClientId"];
                        options.ClientSecret = google["ClientSecret"];
                        options.ResponseType = "code";
                        options.UsePkce = true;
                        options.CallbackPath = "/signin-google";
                        options.Scope.Add("email");
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Security headers sent with every response
            app.Use(
                async (context, next) =>
                {
                    var headers = context.Response.Headers;
                    headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; frame-ancestors 'none'";
                    headers["X-Frame-Options"] = "DENY";
                    headers["X-Content-Type-Options"] = "nosniff";
                    await next();
                });

            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();
            app.UseRateLimiter();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapRazorPages();
                });
        }
    }
}
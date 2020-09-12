using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Mvc;
using AspNet5SQLite.Model;
using AspNet5SQLite.Repositories;

namespace AspNet5SQLite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataEventRecordRepository, DataEventRecordRepository>();

            var connection = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DataEventRecordContext>(options =>
                options.UseSqlite(connection)
            );

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .AllowCredentials()
                            .WithOrigins("https://localhost:44357")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            var guestPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "dataEventRecords")
                .Build();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
              .AddIdentityServerAuthentication(options =>
              {
                  options.Authority = "https://localhost:44356/";
                  options.ApiName = "DataEventRecordsApi";
                  options.ApiSecret = "dataEventRecordsSecret";
                  options.NameClaimType = "email";
              });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("dataEventRecordsAdmin", policyAdmin =>
                {
                    policyAdmin.RequireClaim("role", "dataEventRecords.admin");
                });
                options.AddPolicy("dataEventRecordsUser", policyUser =>
                {
                    policyUser.RequireClaim("role", "dataEventRecords.user");
                });
                options.AddPolicy("dataEventRecords", policyUser =>
                {
                    policyUser.RequireClaim("scope", "dataEventRecords");
                });
            });

            services.AddControllers()
                .AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseCors("AllowAllOrigins");
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

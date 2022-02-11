using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System;
using AspNet5SQLite.Repositories;
using AspNet5SQLite.Model;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.AccessTokenValidation;

namespace AspNet5SQLite;

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

        services.AddSwaggerGen(c =>
        {
            // add JWT Authentication
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // must be lower case
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {securityScheme, new string[] { }}
            });

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "User API",
                Version = "v1",
                Description = "User API",
                Contact = new OpenApiContact
                {
                    Name = "damienbod",
                    Email = string.Empty,
                    Url = new Uri("https://damienbod.com/"),
                },
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSecurityHeaders(
            SecurityHeadersDefinitions
                .GetHeaderPolicyCollection(env.IsDevelopment()));

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseCors("AllowAllOrigins");

        app.UseStaticFiles();

        app.UseCookiePolicy();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}

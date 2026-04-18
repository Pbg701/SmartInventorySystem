using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Application.Services;
using SmartInventorySystem.Application.Validators;
using SmartInventorySystem.Domain.Entities;
using SmartInventorySystem.Infrastructure.Data;
using SmartInventorySystem.Infrastructure.Repositories;
using SmartInventorySystem.Infrastructure.Services;
using System.Text;

namespace SmartInventorySystem.API.Extensions
{

    public static class ServiceExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
                ));
    

            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");

            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "SmartInventory:";
                });
                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddScoped<ICacheService, NullCacheService>();
            }

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Smart Inventory Management System API",
                    Version = "v1",
                    Description = "A comprehensive inventory management system API",
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@inventory.com"
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAuthService, AuthService>();

            // AutoMapper         
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

            return services;
        }
    }
}

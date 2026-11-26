using System.Security.Claims;
using System.Text;
using MediCare.Data;
using MediCare.Data.Entities;
using MediCare.Data.Persistence;
using MediCare.Services;
using MediCare.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediCare.API.Extensions;

     var builder = WebApplication.CreateBuilder(args);
     
     builder.Services.AddControllers();
     
     builder.Services.AddDataServices(builder.Configuration);
     builder.Services.Configure<JwtOptions>(
                 builder.Configuration.GetSection(JwtOptions.SectionName));
     builder.Services.AddApplicationServices();
     
     var jwtOptions = builder.Configuration
         .GetSection(JwtOptions.SectionName)
         .Get<JwtOptions>()
         ?? throw new InvalidOperationException(
             "JWT configuration is missing.");
     
     if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
         Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
     {
         throw new InvalidOperationException(
             "JWT key must contain at least 32 bytes. Configure it using User Secrets or environment variables.");
     }
     
     if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
         string.IsNullOrWhiteSpace(jwtOptions.Audience))
     {
         throw new InvalidOperationException(
             "JWT issuer and audience must be configured.");
     }
     
     if (jwtOptions.AccessTokenMinutes is < 5 or > 1440)
     {
         throw new InvalidOperationException(
             "JWT access token duration must be between 5 and 1440 minutes.");
     }
     
     builder.Services
         .AddIdentityCore<ApplicationUser>(options =>
         {
             options.User.RequireUniqueEmail = true;
     
             options.Password.RequiredLength = 10;
             options.Password.RequireDigit = true;
             options.Password.RequireUppercase = true;
             options.Password.RequireLowercase = true;
             options.Password.RequireNonAlphanumeric = false;
     
             options.Lockout.AllowedForNewUsers = true;
             options.Lockout.MaxFailedAccessAttempts = 5;
             options.Lockout.DefaultLockoutTimeSpan =
                 TimeSpan.FromMinutes(15);
         })
         .AddRoles<IdentityRole<Guid>>()
         .AddEntityFrameworkStores<MediCareDbContext>()
         .AddDefaultTokenProviders();
     
     var signingKey = new SymmetricSecurityKey(
         Encoding.UTF8.GetBytes(jwtOptions.Key));
     
     builder.Services
         .AddAuthentication(options =>
         {
             options.DefaultAuthenticateScheme =
                 JwtBearerDefaults.AuthenticationScheme;
     
             options.DefaultChallengeScheme =
                 JwtBearerDefaults.AuthenticationScheme;
         })
         .AddJwtBearer(options =>
         {
             options.TokenValidationParameters =
                 new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidIssuer = jwtOptions.Issuer,
     
                     ValidateAudience = true,
                     ValidAudience = jwtOptions.Audience,
     
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = signingKey,
     
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.FromMinutes(1),
     
                     NameClaimType = ClaimTypes.Name,
                     RoleClaimType = ClaimTypes.Role
                 };
         });
     
     builder.Services.AddAuthorization(options =>
     {
         options.AddPolicy("PatientOnly", policy =>
             policy.RequireRole(MediCareRoles.Patient));
     
         options.AddPolicy("DoctorOnly", policy =>
             policy.RequireRole(MediCareRoles.Doctor));
     
         options.AddPolicy("ApprovedDoctor", policy =>
             policy
                 .RequireRole(MediCareRoles.Doctor)
                 .RequireClaim("doctor_approved", "true"));
     
         options.AddPolicy("AdminOnly", policy =>
             policy.RequireRole(MediCareRoles.Admin));
     });
     
     var reactOrigins = builder.Configuration
         .GetSection("Cors:ReactOrigins")
         .Get<string[]>() ?? Array.Empty<string>();
     
     if (reactOrigins.Length == 0)
     {
         throw new InvalidOperationException(
             "At least one React origin must be configured under Cors:ReactOrigins.");
     }
     
     builder.Services.AddCors(options =>
     {
         options.AddPolicy("ReactApp", policy =>
         {
             policy
                 .WithOrigins(reactOrigins)
                 .AllowAnyHeader()
                 .AllowAnyMethod();
         });
     });
     
     builder.Services.AddEndpointsApiExplorer();
     
     builder.Services.AddSwaggerGen(options =>
     {
         options.SwaggerDoc("v1", new OpenApiInfo
         {
             Title = "MediCare API",
             Version = "v1"
         });
     
         options.AddSecurityDefinition(
             JwtBearerDefaults.AuthenticationScheme,
             new OpenApiSecurityScheme
             {
                 Name = "Authorization",
                 Type = SecuritySchemeType.Http,
                 Scheme = "bearer",
                 BearerFormat = "JWT",
                 In = ParameterLocation.Header,
                 Description = "Enter the JWT access token."
             });
     
         options.AddSecurityRequirement(
         new OpenApiSecurityRequirement
         {
             {
                 new OpenApiSecurityScheme
                 {
                     Reference = new OpenApiReference
                     {
                         Type = ReferenceType.SecurityScheme,
                         Id = JwtBearerDefaults.AuthenticationScheme
                     }
                 },
                 Array.Empty<string>()
             }
         });
     });
     
     
     
     var app = builder.Build();

     await app.SeedInitialAdminAsync();

     if (app.Environment.IsDevelopment())
     {
         app.UseSwagger();
         app.UseSwaggerUI();
     }
     
     app.UseHttpsRedirection();
     
     app.UseCors("ReactApp");
     
     app.UseAuthentication();
     app.UseAuthorization();
     
     app.MapControllers();
     
     app.Run();
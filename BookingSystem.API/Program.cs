using System.Text;
using BookingSystem.Application.Decorators;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using BookingSystem.Domain.Other;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Infrastructure.Repositories;
using BookingSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region Logging and Controllers

// Configure Serilog for logging
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "Logs/app-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
});

// Add controllers and configure serialization to handle object cycles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

#endregion

#region Swagger

// Configure Swagger and support for JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Booking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region Authentication and Infrastructure

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

#endregion

#region Database

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(); // Only for debugging!
    options.EnableDetailedErrors();

    // Reset EF Core model cache (usually not required)
    var serviceProvider = options.Options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
    if (serviceProvider != null)
    {
        var modelCache = serviceProvider.GetService<IMemoryCache>();
        modelCache?.Remove(typeof(AppDbContext));
    }
});

#endregion

#region Redis and Cache

// Register Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var connectionString = configuration?.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString ?? throw new InvalidOperationException("Redis connection string not configured"));
});

// Register distributed cache using Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Register cache service abstraction
builder.Services.AddScoped<ICacheService, RedisCacheService>();

#endregion

#region Repositories

builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRepository<RoomType>, BaseRepository<RoomType>>();

builder.Services.AddScoped<IRepository<RoomPricing>, BaseRepository<RoomPricing>>();
builder.Services.AddScoped<IRepository<HotelPhoto>, BaseRepository<HotelPhoto>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

#region Services and Decorators

// Register main services for use inside decorators
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<HotelService>();

builder.Services.AddScoped<UserService>();

// Use cached decorator implementations instead of standard services
builder.Services.AddScoped<IBookingService>(provider =>
    new CachedBookingService(
        provider.GetRequiredService<BookingService>(),
        provider.GetRequiredService<ICacheService>(),
        provider.GetRequiredService<ILogger<CachedBookingService>>())
);

builder.Services.AddScoped<IHotelService>(provider =>
    new CachedHotelService(
        provider.GetRequiredService<HotelService>(),
        provider.GetRequiredService<ICacheService>(),
        provider.GetRequiredService<ILogger<CachedHotelService>>())
);


builder.Services.AddScoped<IUserService>(provider =>
    new CachedUserService(
        provider.GetRequiredService<UserService>(),
        provider.GetRequiredService<ICacheService>(),
        provider.GetRequiredService<ILogger<CachedUserService>>())
);

builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRoomPricingService, RoomPricingService>();
builder.Services.AddScoped<DatabaseSeeder>();

#endregion

#region Cloudinary

// Cloudinary configuration and repository
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoRepository>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new CloudinaryPhotoRepository(settings);
});

builder.Services.AddScoped<IHotelPhotoService, HotelPhotoService>();

#endregion

#region CORS

// Configure CORS for allowed origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
        policyBuilder
            .WithOrigins(
                "http://localhost:5044",
                "http://localhost:3000" // frontend
                // "https://localhost:7050",
                // "http://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

#endregion

#region JWT

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"))),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Extract token from cookie or Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["X-Access-Token"];
                if (string.IsNullOrEmpty(context.Token))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length);
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

#endregion

var app = builder.Build();

#region Development Tools and Database Seeding

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Automatically seed database with initial data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
            Console.WriteLine("Database migrated and seeded successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            Console.WriteLine($"Database migration/seeding error: {ex.Message}");
        }
    }
}

#endregion

#region Middleware

// Enable Swagger always to simplify debugging
app.UseSwagger();
app.UseSwaggerUI();

// Log requests to see failing endpoints and status codes
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#endregion

app.Run();

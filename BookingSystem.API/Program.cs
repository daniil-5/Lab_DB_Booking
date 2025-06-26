using System.Text;
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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This preserves references and handles circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Booking API", Version = "v1" });
    
    // Add JWT Authentication support to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
            new string[] {}
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure Entity Framework Core with PostgreSQL
// builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppDbContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // Important: Disable query caching
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    
    // Force EF to rebuild the model
    var serviceProvider = options.Options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
    if (serviceProvider != null)
    {
        var modelCache = serviceProvider.GetService<IMemoryCache>();
        modelCache?.Remove(typeof(AppDbContext));
    }
});

builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRepository<RoomType>, BaseRepository<RoomType>>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();
builder.Services.AddScoped<IRepository<Room>, BaseRepository<Room>>();

builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();

builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRepository<RoomType>, BaseRepository<RoomType>>();

builder.Services.AddScoped<IRepository<RoomPricing>, BaseRepository<RoomPricing>>();
builder.Services.AddScoped<IRoomPricingService, RoomPricingService>();

// builder.Services.AddScoped<IRepository<HotelPhoto>, BaseRepository<HotelPhoto>>();
// builder.Services.AddScoped<IHotelPhotoService, HotelPhotoService>();

builder.Services.AddScoped<DatabaseSeeder>(); // Add seeder service

builder.Services.AddScoped<ISerializationService, SerializationService>(); // Serializer service

// Setup the Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoRepository>(provider => {
    var settings = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new CloudinaryPhotoRepository(settings);
});

builder.Services.AddScoped<IHotelPhotoService, HotelPhotoService>();
builder.Services.AddScoped<IRepository<HotelPhoto>, BaseRepository<HotelPhoto>>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder => policyBuilder
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

// Add JWT Authentication
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
        
        // Add cookie extraction logic
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from cookies first
                context.Token = context.Request.Cookies["X-Access-Token"];
                
                // If no token in cookies, fall back to Authorization header
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


using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
            Console.WriteLine("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            Console.WriteLine($"Database seeding error: {ex.Message}");
        }
    }
}
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


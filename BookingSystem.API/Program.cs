using System.Text;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Application.Tests;
using BookingSystem.Domain.Entities;
using BookingSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Infrastructure.Repositories;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();
builder.Services.AddScoped<IRepository<Room>, BaseRepository<Room>>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRepository<Booking>, BaseRepository<Booking>>();
builder.Services.AddScoped<IRepository<Room>, BaseRepository<Room>>();
builder.Services.AddScoped<IRepository<Hotel>, BaseRepository<Hotel>>();
builder.Services.AddScoped<IRepository<Room>, BaseRepository<Room>>();

builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRepository<RoomType>, BaseRepository<RoomType>>();

builder.Services.AddScoped<IRepository<RoomPricing>, BaseRepository<RoomPricing>>();
builder.Services.AddScoped<IRoomPricingService, RoomPricingService>();

builder.Services.AddScoped<IRepository<HotelPhoto>, BaseRepository<HotelPhoto>>();
builder.Services.AddScoped<IHotelPhotoService, HotelPhotoService>();

builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IHotelService, HotelService>();

builder.Services.AddScoped<DatabaseSeeder>(); // Add seeder service

builder.Services.AddSingleton<ITestOutputHelper, TestOutputHelper>();
builder.Services.AddScoped<TestRunner>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder => policyBuilder
            .WithOrigins(
                "http://localhost:5044",
                "https://localhost:7050",
                "http://localhost:5173",
                "https://localhost:5173"
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
    
    // _ = Task.Run(async () =>
    // {
    //     try
    //     {
    //         var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    //         using var scope = scopeFactory.CreateScope();
    //         var testRunner = scope.ServiceProvider.GetRequiredService<TestRunner>();
    //         await testRunner.RunAllTests();
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"⚠️ Test Runner Error: {ex.Message}");
    //     }
    // });
}
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


public class TestRunner
{
    private readonly IServiceProvider _serviceProvider;

    public TestRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task RunAllTests()
    {
        var testClasses = new List<Type>
        {
            typeof(BookingServiceTests),
            typeof(HotelServiceTests),
            typeof(RoomTypeServiceTests),
            typeof(RoomServiceTests),
            typeof(HotelPhotoServiceTests)
        };

        foreach (var testClass in testClasses)
        {
            Console.WriteLine($"\n=== Running tests for {testClass.Name} ===");
            var testInstance = ActivatorUtilities.CreateInstance(_serviceProvider, testClass);
            await RunTestsForClass(testInstance, testClass);
        }
    }

    private async Task RunTestsForClass(object testInstance, Type testClass)
    {
        foreach (var method in testClass.GetMethods()
                     .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any()))
        {
            Console.WriteLine($"Running test: {method.Name}");
            try
            {
                var result = method.Invoke(testInstance, null);
                if (result is Task taskResult)
                {
                    await taskResult;
                }

                Console.WriteLine($"✅ {method.Name} passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {method.Name} failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Application.Services;
using IncidentManagement.Application.Configuration;
using IncidentManagement.Api.Middleware;
using IncidentManagement.Api.Hubs;
using IncidentManagement.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add memory caching for performance
builder.Services.AddMemoryCache();

// Add output caching for HTTP responses
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("FireStationBoundaries", builder => 
        builder.Expire(TimeSpan.FromMinutes(10)).Tag("fire-stations"));
});

// Configuration
builder.Services.Configure<FireStationDataOptions>(
    builder.Configuration.GetSection(FireStationDataOptions.SectionName));
builder.Services.Configure<FireDistrictDataOptions>(
    builder.Configuration.GetSection(FireDistrictDataOptions.SectionName));
builder.Services.Configure<FireHydrantDataOptions>(
    builder.Configuration.GetSection(FireHydrantDataOptions.SectionName));
builder.Services.Configure<CoastGuardStationDataOptions>(
    builder.Configuration.GetSection(CoastGuardStationDataOptions.SectionName));
builder.Services.Configure<HospitalDataOptions>(
    builder.Configuration.GetSection(HospitalDataOptions.SectionName));
builder.Services.Configure<PoliceStationDataOptions>(
    builder.Configuration.GetSection(PoliceStationDataOptions.SectionName));

// Application services
builder.Services.AddScoped<INotificationService, RealTimeNotificationService>();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
builder.Services.AddScoped<IFireStationDataService, FireStationDataService>();
builder.Services.AddScoped<IFireHydrantDataService, FireHydrantDataService>();
builder.Services.AddScoped<ICoastGuardStationDataService, CoastGuardStationDataService>();
builder.Services.AddScoped<IHospitalDataService, HospitalDataService>();
builder.Services.AddScoped<IPoliceStationDataService, PoliceStationDataService>();
builder.Services.AddScoped<IGeographicService, CachedGeographicService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IPatrolZoneService, PatrolZoneService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IIncidentTypeService, IncidentTypeService>();
builder.Services.AddScoped<IStationAssignmentService, StationAssignmentService>();
builder.Services.Configure<AisStreamConfig>(builder.Configuration.GetSection("AisStream"));
builder.Services.AddSingleton<IShipStore, ShipStore>();

// Conditionally register AIS Stream service based on configuration
var aisStreamConfig = builder.Configuration.GetSection("AisStream").Get<AisStreamConfig>();
if (aisStreamConfig?.Enabled == true)
{
    builder.Services.AddHostedService<AisStreamBackgroundService>();
}

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };

        // Configure JWT for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Database configuration
var useSqlServer = builder.Configuration.GetValue<bool>("UseSqlServer");
var usePostgres = builder.Configuration.GetValue<bool>("UsePostgres");

if (useSqlServer)
{
    builder.Services.AddDbContext<IncidentManagementDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AegisIncidentManagementServer")));
}
else if (usePostgres)
{
    builder.Services.AddDbContext<IncidentManagementDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}
else
{
    builder.Services.AddDbContext<IncidentManagementDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=incident_management.db"));
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Use output caching middleware
app.UseOutputCache();

app.UseAuthentication();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<IncidentHub>("/api/hubs/incidents");

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IncidentManagementDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // For SQL Server, use migrations instead of EnsureCreated
        if (useSqlServer)
        {
            logger.LogInformation("Applying database migrations for SQL Server...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
        }
        else
        {
            logger.LogInformation("Ensuring database is created...");
            context.Database.EnsureCreated();
            logger.LogInformation("Database creation completed successfully");
        }

        // Seed agencies first (required for all other data)
        logger.LogInformation("Seeding agencies...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedAgencies(context);
        logger.LogInformation("Agencies seeding completed successfully");

        //Seed users (depends on agencies)
        logger.LogInformation("Seeding users...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedUsers(context);
        logger.LogInformation("Users seeding completed successfully");

        // Load fire station data
        logger.LogInformation("Starting fire station data initialization...");
        var fireStationDataService = scope.ServiceProvider.GetRequiredService<IFireStationDataService>();
        await fireStationDataService.LoadFireStationLocationsAsync();
        logger.LogInformation("Fire station data initialization completed successfully");

        // Load fire hydrant data
        logger.LogInformation("Starting fire hydrant data initialization...");
        var fireHydrantDataService = scope.ServiceProvider.GetRequiredService<IFireHydrantDataService>();
        await fireHydrantDataService.SeedFireHydrantsAsync();
        logger.LogInformation("Fire hydrant data initialization completed successfully");

        // Load coast guard station data
        logger.LogInformation("Starting coast guard station data initialization...");
        var coastGuardStationDataService = scope.ServiceProvider.GetRequiredService<ICoastGuardStationDataService>();
        await coastGuardStationDataService.LoadCoastGuardStationDataAsync();
        logger.LogInformation("Coast guard station data initialization completed successfully");

        // Load hospital data
        logger.LogInformation("Starting hospital data initialization...");
        var hospitalDataService = scope.ServiceProvider.GetRequiredService<IHospitalDataService>();
        if (!await hospitalDataService.IsDataLoadedAsync())
        {
            await hospitalDataService.LoadHospitalDataAsync();
        }
        logger.LogInformation("Hospital data initialization completed successfully");

        // Load police station data
        logger.LogInformation("Starting police station data initialization...");
        var policeStationDataService = scope.ServiceProvider.GetRequiredService<IPoliceStationDataService>();
        if (!await policeStationDataService.IsDataAlreadyLoadedAsync())
        {
            await policeStationDataService.LoadDataAsync();
        }
        logger.LogInformation("Police station data initialization completed successfully");

        // Seed stations (depends on agencies)
        // logger.LogInformation("Seeding stations...");
        // await IncidentManagement.Infrastructure.Data.SeedData.SeedStations(context);
        // logger.LogInformation("Stations seeding completed successfully");

        // Seed user station assignments (depends on users and stations)
        logger.LogInformation("Seeding user station assignments...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedUserStationAssignments(context);
        logger.LogInformation("User station assignments seeding completed successfully");

        // Seed demo data - vehicles (depends on agencies and stations)
        logger.LogInformation("Seeding vehicles for demo...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedVehicles(context);
        logger.LogInformation("Vehicles seeding completed successfully");

        // Seed demo data - personnel (depends on agencies and stations)
        logger.LogInformation("Seeding personnel for demo...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedPersonnel(context);
        logger.LogInformation("Personnel seeding completed successfully");

        // Seed demo data - incidents (depends on agencies, stations, and users)
        logger.LogInformation("Seeding incidents for demo...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedIncidents(context);
        logger.LogInformation("Incidents seeding completed successfully");

        // Seed demo data - callers (depends on incidents)
        logger.LogInformation("Seeding callers for demo...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedCallers(context);
        logger.LogInformation("Callers seeding completed successfully");

        // Seed demo data - assignments (depends on incidents, vehicles, and personnel)
        // logger.LogInformation("Seeding assignments for demo...");
        // await IncidentManagement.Infrastructure.Data.SeedData.SeedAssignments(context);
        // logger.LogInformation("Assignments seeding completed successfully");

        // Seed demo data - shifts (depends on stations)
        logger.LogInformation("Seeding shifts for demo...");
        await IncidentManagement.Infrastructure.Data.SeedData.SeedShifts(context);
        logger.LogInformation("Shifts seeding completed successfully");

        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Critical error occurred during database initialization. Application startup may be affected.");
        
        // Log specific error details for troubleshooting
        if (ex is InvalidOperationException && ex.Message.Contains("fire station"))
        {
            logger.LogError("Fire station data loading failed. The application will continue without fire station boundaries.");
        }
        else if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
        }
        
        // Don't throw - allow application to start even if fire station data fails
        // The application should be functional without fire station boundaries
    }
}

app.Run();
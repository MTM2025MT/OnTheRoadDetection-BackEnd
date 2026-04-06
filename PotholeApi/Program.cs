using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.IO.Converters;
using PotholeApi;
using PotholeApi.Data;
using PotholeApi.GrpcServices;
using PotholeApi.Helpers;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using PotholeApi.Protos;
using PotholeApi.Repositories;
using PotholeApi.Services;
using PotholeApi.Services.DashboardsServices;
using PotholeApi.Services.RoleHandlers;
using System;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*


*/
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<DetectionQueue>();
builder.Services.AddHostedService<DetectionDatabaseWorker>();
builder.Services.AddGrpc();

builder.Services.AddScoped<IPotholeRepository, PotholeRepository>();
builder.Services.AddScoped<IRepairTeamRepository, RepairTeamRepository>();
builder.Services.AddScoped<IOfficerRepository, OfficerRepository>();
builder.Services.AddScoped<IJobTaskRepository, JobTaskRepository>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoService, PotholeApi.Services.PhotoService>();
builder.Services.AddScoped<IDistrictRepository,DistrictRepository>();
builder.Services.AddScoped<INeighbourhoodRepository, NeighbourhoodRepository>();
builder.Services.AddScoped<ISeedRoleAdminServices, SeedRoleAdminServices>();
builder.Services.AddScoped<IPotholeRepository, PotholeRepository>();

builder.Services.AddScoped<IRoleHandler, OfficerHandler>();
builder.Services.AddScoped<IRoleHandler, AdminHandler>();
builder.Services.AddScoped<IRoleHandler, ManagerHandler>();
builder.Services.AddScoped<IPotholeDashboardsService, PotholeDashboardsService>();
//builder.Services.AddScoped<IRoleHandler, RepairTeamHandler>();
// jwl part
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.UseNetTopologySuite()
    )
);


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<Context>()
    .AddDefaultTokenProviders();
// JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// This is the part you were missing:
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production if possible
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // 1. Validate the Issuer (Who created the token)
        ValidateIssuer = true,
        ValidIssuer = "PotholeServer", // Must match the string in your Login Controller

        // 2. Validate the Audience (Who the token is for)
        ValidateAudience = true,
        ValidAudience = "PotholeReactApp", // Must match the string in your Login Controller

        // 3. Validate the Secret Key (Signature)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),

        // 4. Ensure token hasn't expired
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Removes the default 5-minute buffer so expiration is exact
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // <--- REPLACE THIS with your React URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // <--- This is REQUIRED for cookies to work
    });
});

var app = builder.Build();
app.MapGrpcService<DetectionImplementationService>();
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await IdentitySeed.SeedRolesAsync(services);
//}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<Context>();
//    DistrictSeeder.Seed(db);
//}

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<Context>();
//        var logger = services.GetRequiredService<ILogger<Program>>();

//        logger.LogInformation("Starting District seeding...");
//        DistrictSeeder.Seed(context, logger, app.Environment.ContentRootPath);  // ← Pass base path
//        logger.LogInformation("District seeding completed!");
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}
// Configure the HTTP request pipeline.
app.UseSwagger();
    app.UseSwaggerUI();
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();

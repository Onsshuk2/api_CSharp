using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using pd311_web_api.BLL;
using pd311_web_api.BLL.DTOs.Account;
using pd311_web_api.BLL.Services.Account;
using pd311_web_api.BLL.Services.Cars;
using pd311_web_api.BLL.Services.Email;
using pd311_web_api.BLL.Services.Image;
using pd311_web_api.BLL.Services.Manufactures;
using pd311_web_api.BLL.Services.Role;
using pd311_web_api.BLL.Services.User;
using pd311_web_api.DAL;
using pd311_web_api.DAL.Repositories.Cars;
using pd311_web_api.DAL.Repositories.Manufactures;
using pd311_web_api.Middlewares;
using Serilog;
using System.Text;
using static pd311_web_api.DAL.Entities.IdentityEntities;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log_.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RequireExpirationTime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"] ?? ""))
        };
    });

// Add services to the container.
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IManufactureService, ManufactureService>();

// Add repositories
builder.Services.AddScoped<IManufactureRepository, ManufactureRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();

builder.Services.AddControllers();

// Add fluent validation
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// Add automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql("name=NpgsqlLocal");
});

// Add identity
builder.Services
    .AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("localhost3000", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Middlewares
app.UseMiddleware<MiddlewareExceptionHandler>();
app.UseMiddleware<MiddlewareNullExceptionHandler>();
app.UseMiddleware<MiddlewareLogger>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("localhost3000");

Settings.RootPath = builder.Environment.ContentRootPath;
string rootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
string imagesPath = Path.Combine(rootPath, Settings.RootImagesPath);

if(!Directory.Exists(rootPath))
{
    Directory.CreateDirectory(rootPath);
}

if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

// static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Seeder.SeedRolesAndAdmin(services);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Utils;
using TSM.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<ModelBase>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Add TokenHelper and PasswordHelper
builder.Services.AddSingleton(new TokenHelper(jwtSettings));
builder.Services.AddSingleton<PasswordHelper>();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // This will make Swagger UI available at the root URL
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // Use CORS policy

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost("/register", async ([FromBody] RegisterUserRequest request, [FromServices] ModelBase dbContext, [FromServices] TokenHelper tokenHelper, [FromServices] PasswordHelper passwordHelper) =>
{
    var user = new User
    {
        Email = request.Email,
        Pwd = passwordHelper.HashPassword(request.Password),
        DisplayName = request.Name,
        PreferredCats = request.PreferredCats?.Select(cat => (long)cat).ToList() // Приведение к long
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    var token = tokenHelper.GenerateToken(user);

    return Results.Ok(new { Token = token });
});

app.MapPost("/login", async ([FromBody] LoginUserRequest request, [FromServices] ModelBase dbContext, [FromServices] TokenHelper tokenHelper, [FromServices] PasswordHelper passwordHelper) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

    if (user == null || !passwordHelper.VerifyPassword(request.Password, user.Pwd))
    {
        return Results.Unauthorized();
    }

    var token = tokenHelper.GenerateToken(user);

    return Results.Ok(new { Token = token });
});

// New endpoint to get categories
app.MapGet("/categories", async ([FromServices] ModelBase dbContext) =>
{
    var categories = await dbContext.Categories
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.ParentId
        })
        .ToListAsync();

    return Results.Ok(categories);
});

// Endpoint to get max 100 places, paged
app.MapGet("/places", async ([FromServices] ModelBase dbContext, int page = 1) =>
{
    int pageSize = 100;
    var places = await dbContext.Places
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.CategoryId,
            p.Lat,
            p.Long
        })
        .ToListAsync();

    return Results.Ok(places);
});

// Endpoint to get places by category
app.MapGet("/place/byCat/{categoryId}", async (int categoryId, [FromServices] ModelBase dbContext) =>
{
    var places = await dbContext.Places
        .Where(p => p.CategoryId == categoryId)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.CategoryId,
            p.Lat,
            p.Long
        })
        .ToListAsync();

    return Results.Ok(places);
});

// Endpoint to get single place by ID
app.MapGet("/place/byId/{id}", async (int id, [FromServices] ModelBase dbContext) =>
{
    var place = await dbContext.Places
        .Where(p => p.Id == id)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.CategoryId,
            p.Lat,
            p.Long
        })
        .FirstOrDefaultAsync();

    if (place == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(place);
});

app.MapPost("/getroute", async ([FromBody] RouteRequest request, [FromServices] ModelBase dbContext) =>
{
    var route = await RouteService.GetRouteAsync(request.MyLat, request.MyLong, request.Categories, dbContext);
    return Results.Ok(route);
});

app.Run();

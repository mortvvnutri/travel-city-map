using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Utils;
using TSM.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ModelBase>(options =>
    options.UseNpgsql(connectionString));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost("/register", async ([FromBody] RegisterUserRequest request, [FromServices] ModelBase dbContext, [FromServices] TokenHelper tokenHelper, [FromServices] PasswordHelper passwordHelper) =>
{
    var user = new User
    {
        Username = request.Username,
        PasswordHash = passwordHelper.HashPassword(request.Password),
        Email = request.Email
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    var token = tokenHelper.GenerateToken(user);

    return Results.Ok(new { Token = token });
});

app.MapPost("/login", async ([FromBody] LoginUserRequest request, [FromServices] ModelBase dbContext, [FromServices] TokenHelper tokenHelper, [FromServices] PasswordHelper passwordHelper) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

    if (user == null || !passwordHelper.VerifyPassword(request.Password, user.PasswordHash))
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
            c.Name
        })
        .ToListAsync();

    return Results.Ok(categories);
});

// Endpoint to get max 100 places, paged
app.MapGet("/place", async ([FromServices] ModelBase dbContext, int page = 1) =>
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
            p.Latitude,
            p.Longitude,
            p.POptions
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
            p.CategoryId
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
            p.CategoryId
        })
        .FirstOrDefaultAsync();

    if (place == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(place);
});

// User routes
app.MapGet("/users", async ([FromServices] ModelBase context) =>
{
    var users = await context.Users.ToListAsync();
    return Results.Ok(users);
});

app.MapGet("/users/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var user = await context.Users.FindAsync(id);
    if (user == null) return Results.NotFound();
    return Results.Ok(user);
});

// PaymentOption routes
app.MapGet("/paymentOptions", async ([FromServices] ModelBase context) =>
{
    var paymentOptions = await context.PaymentOptions.ToListAsync();
    return Results.Ok(paymentOptions);
});

app.MapGet("/paymentOptions/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var paymentOption = await context.PaymentOptions.FindAsync(id);
    if (paymentOption == null) return Results.NotFound();
    return Results.Ok(paymentOption);
});

// Review routes
app.MapGet("/reviews", async ([FromServices] ModelBase context) =>
{
    var reviews = await context.Reviews.ToListAsync();
    return Results.Ok(reviews);
});

app.MapGet("/reviews/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var review = await context.Reviews.FindAsync(id);
    if (review == null) return Results.NotFound();
    return Results.Ok(review);
});

// RoutePoint routes
app.MapGet("/routePoints", async ([FromServices] ModelBase context) =>
{
    var routePoints = await context.RoutePoints.ToListAsync();
    return Results.Ok(routePoints);
});

app.MapGet("/routePoints/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var routePoint = await context.RoutePoints.FindAsync(id);
    if (routePoint == null) return Results.NotFound();
    return Results.Ok(routePoint);
});

// Route routes
app.MapGet("/routes", async ([FromServices] ModelBase context) =>
{
    var routes = await context.Routes.ToListAsync();
    return Results.Ok(routes);
});

app.MapGet("/routes/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var route = await context.Routes.FindAsync(id);
    if (route == null) return Results.NotFound();
    return Results.Ok(route);
});

// Notification routes
app.MapGet("/notifications", async ([FromServices] ModelBase context) =>
{
    var notifications = await context.Notifications.ToListAsync();
    return Results.Ok(notifications);
});

app.MapGet("/notifications/{id}", async (int id, [FromServices] ModelBase context) =>
{
    var notification = await context.Notifications.FindAsync(id);
    if (notification == null) return Results.NotFound();
    return Results.Ok(notification);
});

app.Run();

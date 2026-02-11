using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NavExpo.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// JWT CONFIGURATION
// =======================
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is missing. Please configure Jwt:Key in appsettings.json.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtKey)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// =======================
// MONGODB CONFIGURATION
// =======================
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings");
    var connString = settings["ConnectionString"];

    if (string.IsNullOrEmpty(connString))
    {
        throw new InvalidOperationException(
            "Could not find 'ConnectionString' inside 'MongoDbSettings' in appsettings.json."
        );
    }

    return new MongoClient(connString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings");
    var dbName = settings["DatabaseName"];

    if (string.IsNullOrEmpty(dbName))
    {
        throw new InvalidOperationException(
            "Could not find 'DatabaseName' inside 'MongoDbSettings' in appsettings.json."
        );
    }

    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(dbName);
});

// =======================
// SERVICES
// =======================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AttendeeService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// =======================
// DATABASE MIGRATIONS
// =======================
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var database = serviceProvider.GetRequiredService<IMongoDatabase>();

    var migrationService = new MigrationService(database);
    await migrationService.ApplyMigrations();
}

// =======================
// MIDDLEWARE PIPELINE
// =======================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NavExpo API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();  
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using payzen_backend.Data;
using payzen_backend.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration explicite pour accepter JSON
// Pourquoi: S'assurer que l'API accepte et retourne du JSON correctement
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Options de sérialisation JSON si nécessaire
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Garde les noms de propriétés tels quels
    });

builder.Services.AddOpenApi();

// Configuration de la base de données
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conn));

// Configuration JWT
var jwtKey = builder.Configuration["JwtSettings:Key"] 
    ?? throw new InvalidOperationException("JWT Key not found in configuration");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            
            NameClaimType = "unique_name",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine($"\n✅ ========== TOKEN VALIDE ==========");
                var claims = context.Principal?.Claims;
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"✅ Claim: {claim.Type} = {claim.Value}");
                    }
                }
                
                var userId = context.Principal?.FindFirst("uid")?.Value;
                var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                Console.WriteLine($"👤 User ID: {userId}");
                Console.WriteLine($"👤 Email: {email}");
                Console.WriteLine($"✅ ========== FIN VALIDATION ==========\n");
                
                return Task.CompletedTask;
            },
            
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"\n❌ Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            
            OnChallenge = context =>
            {
                Console.WriteLine($"\n⚠️ Challenge: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<JwtService>();
builder.Services.AddAuthorization();

// CHANGEMENT: Ajout de CORS si nécessaire (pour frontend)
// Pourquoi: Permettre les requêtes cross-origin si vous avez un frontend séparé
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        Console.WriteLine($"\n🔍 ========== REQUEST ==========");
        Console.WriteLine($"🔍 Method: {context.Request.Method}");
        Console.WriteLine($"🔍 Path: {context.Request.Path}");
        Console.WriteLine($"🔍 Content-Type: {context.Request.ContentType}");
        Console.WriteLine($"🔍 Content-Length: {context.Request.ContentLength}");
        
        var authHeader = context.Request.Headers.Authorization.ToString();
        Console.WriteLine($"🔍 Authorization présent: {!string.IsNullOrEmpty(authHeader)}");
        
        if (!string.IsNullOrEmpty(authHeader))
        {
            Console.WriteLine($"🔍 Auth Header: {authHeader.Substring(0, Math.Min(50, authHeader.Length))}...");
        }
        Console.WriteLine($"🔍 ========== END REQUEST ==========\n");
    }
    await next();
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 👇 Configuration de Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PayZen API",
        Description = "API REST pour la gestion de la paie développée avec .NET 9",
        Contact = new OpenApiContact
        {
            Name = "Support PayZen",
            Email = "support@payzen.com",
            Url = new Uri("https://github.com/mo-shab/payzen_backend")
        },
        License = new OpenApiLicense
        {
            Name = "Propriétaire",
            Url = new Uri("https://payzen.com/license")
        }
    });

    // 👇 Support des commentaires XML
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // 👇 Configuration de l'authentification JWT dans Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header en utilisant le schéma Bearer. \n\n" +
                      "Entrez 'Bearer' [espace] puis votre token.\n\n" +
                      "Exemple: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// 👇 Activer Swagger en développement ET production
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PayZen API v1");
        options.RoutePrefix = string.Empty; // Swagger à la racine: http://localhost:5119/
        options.DocumentTitle = "PayZen API Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
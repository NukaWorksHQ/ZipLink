using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Server.Contexts;
using Server.Services;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: Bearer abc123"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});

// Contexts registration
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

builder.Services.AddAuthentication("Bearer").AddJwtBearer(
    "Bearer",
    options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal.Claims
                .FirstOrDefault(c => c.Type == "UserId")?.Value;

            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            var userExists = await dbContext.Users
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                context.Fail("Unauthorized: User no longer exists.");
            }
        }
    };
});

builder.Services.AddAutoMapper(config =>
{
    config.AddMaps(typeof(Program).Assembly);
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins(builder.Configuration["Jwt:Audience"]!)
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .AllowAnyHeader();
        });
});

// Services registration
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LinkStatsService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClaimsPrincipal>();
builder.Services.AddScoped<UserAccessValidator>();
builder.Services.AddScoped<IApiHostService, ApiHostService>();

builder.Services.AddHttpClient();

builder.Services.AddTransient<Func<HttpClient>>(serviceProvider =>
{
    return () =>
    {
        var handler = new SocketsHttpHandler()
        {
            // Durée de vie courte des connexions poolées pour forcer la résolution DNS
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            ConnectTimeout = TimeSpan.FromSeconds(10)
        };

        return new HttpClient(handler);
    };
});

// HttpClient spécifique pour la géolocalisation
builder.Services.AddSingleton<Func<string, HttpClient>>(serviceProvider =>
{
    return (clientName) =>
    {
        var handler = new SocketsHttpHandler()
        {
            // Connexions très courtes pour forcer la résolution DNS à chaque requête
            PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30),
            ConnectTimeout = TimeSpan.FromSeconds(5)
        };

        var client = new HttpClient(handler);

        if (clientName == "GeoLocation")
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "ZipLink-GeoResolver/1.0");
        }

        return client;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");

app.UseHttpsRedirection();

// Redirection vers stfu.lat pour la racine de l'API
app.MapGet("/", () => Results.Redirect(builder.Configuration["Jwt:Audience"]!));

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

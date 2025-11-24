using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================
builder.Services.AddControllers();

// 🔐 Authentification JWT pour Gateway
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7273"; // AuthAPI
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false // ⚠️ on ignore l'Audience pour simplifier
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOcelot(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================== MIDDLEWARE ==================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔥 Très important : d'abord authentification
app.UseAuthentication();  // ⬅️ AJOUT ICI

// Puis autorisation
app.UseAuthorization();

app.MapControllers();

// Ocelot doit être exécuté AVANT app.Run()
await app.UseOcelot();

app.Run();

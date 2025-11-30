using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173") // ton frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});
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

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

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
// 🔥 AJOUT ICI
app.UseCors("AllowFrontend");

// 🔥 Très important : d'abord authentification
app.UseAuthentication();  // ⬅️ AJOUT ICI

// Puis autorisation
app.UseAuthorization();

app.MapControllers();

app.UseOcelot().Wait();

app.Run();
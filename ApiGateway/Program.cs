using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== CORS ==================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend",
		policy =>
			policy.WithOrigins("http://localhost:5173")
				  .AllowAnyHeader()
				  .AllowAnyMethod()
	);
});

// ================== SERVICES ==================
builder.Services.AddControllers();

// 🔐 JWT Auth for Gateway (🔥 FIX ICI 🔥)
builder.Services
	.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		options.RequireHttpsMetadata = false;

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],

			ValidateAudience = true,
			ValidAudience = builder.Configuration["Jwt:Audience"],

			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
			),

			// 🔥🔥🔥 OBLIGATOIRE 🔥🔥🔥
			RoleClaimType = ClaimTypes.Role,
			NameClaimType = ClaimTypes.NameIdentifier
		};
	});

builder.Services.AddAuthorization();

// ================== OCELOT ==================
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

// ================== Swagger ==================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================== MIDDLEWARE ==================
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// ❌ PAS DE HTTPS REDIRECT EN DEV
// app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// 🔐 Auth
app.UseAuthentication();
app.UseAuthorization();

// Facultatif
app.MapControllers();

// 🚪 Ocelot DOIT être dernier
await app.UseOcelot();

app.Run();

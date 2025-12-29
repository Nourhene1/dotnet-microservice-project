using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrdersAPI.Data;
using System.Security.Claims;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ===================== Services =====================
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    );
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 🔥 Authentification JWT (1 seule fois !)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
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
			NameClaimType = ClaimTypes.NameIdentifier, // ✅ Pour HttpContext.User.Identity.Name
			RoleClaimType = ClaimTypes.Role            // ✅ Pour HttpContext.User.IsInRole()
		};

		// 🔥 AJOUTEZ CETTE PARTIE POUR DEBUG
		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				Console.WriteLine($"❌ JWT Auth Failed: {context.Exception.Message}");
				return Task.CompletedTask;
			},
			OnTokenValidated = context =>
			{
				var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
				Console.WriteLine($"✅ JWT Valid! Claims: {string.Join(", ", claims ?? new string[0])}");
				return Task.CompletedTask;
			}
		};
	});



builder.Services.AddAuthorization();

// 📌 Swagger + Auth support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Entrez: Bearer {votre_token}",
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	// 🔥 AJOUTEZ CETTE PARTIE MANQUANTE
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
                Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});
builder.Services.AddHttpClient();
var app = builder.Build();

// ===================== Pipeline =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // 🔥 AVANT auth
// 🔥 Authentification d’abord
app.UseAuthentication();

// Puis autorisation
app.UseAuthorization();

app.MapControllers();
app.Run();
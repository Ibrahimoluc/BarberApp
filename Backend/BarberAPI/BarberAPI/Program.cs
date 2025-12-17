using System.Text;
using BarberAPI.Data;
using BarberAPI.Helper.GmailHelper;
using BarberAPI.Helper.GoogleWebAppHelper;
using BarberAPI.Helper.JwtHelper;
using BarberAPI.Models.Concrete;
using BarberAPI.Services.Abstract;
using BarberAPI.Services.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lowercase
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token only (without 'Bearer ' prefix)"
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
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.Configure<GmailOptions>(builder.Configuration.GetSection("GmailOptions"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleWebAppOptions>(builder.Configuration.GetSection("GoogleWebAppOptions"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtOptions>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMailService, GmailService>();
builder.Services.AddScoped<ITokenService, JwtService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Token'ý kimin imzaladýðýný kontrol et (Issuer)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        // Token'ýn kime gönderildiðini kontrol et (Audience)
        // Eðer TokenService'de "berberclient" dediysen, burasý da aynýsý olmalý.
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        // Token'ýn süresi dolmuþ mu kontrol et
        ValidateLifetime = true,

        // Ýmza anahtarý doðru mu kontrol et (EN ÖNEMLÝSÝ)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        // Saat farký toleransý (Varsayýlan 5 dk'dýr, sýfýrlamak iyidir)
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // KURAL 1: "Tamamlanmýþ Berber" Politikasý
    // Bu kurala uymak için hem Berber olmalý HEM DE profili tamamlanmýþ olmalý.
    options.AddPolicy("CompletedBarber", policy =>
        policy.RequireRole("Barber")
              .RequireClaim("IsProfileCompleted", "True")); // Token'da bu True olmalý!
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

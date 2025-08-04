using Microsoft.Extensions.DependencyInjection;
using Gym.IOC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Gym.BLL;
using Microsoft.AspNetCore.Http.Features;
using Gym.Model;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Aprende más sobre cómo configurar Swagger/OpenAPI en https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.InyectarDependecias(builder.Configuration);



// Configuración de autenticación JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Opcional: puede establecer el tiempo de retraso de la validación del token.
        };


    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("angularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "https://appgym-c350c.web.app")
            .SetIsOriginAllowedToAllowWildcardSubdomains()
             .AllowAnyMethod()
             .AllowAnyHeader();


    });
});



var app = builder.Build();



// Configurar la canalización de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("angularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

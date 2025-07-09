using Microsoft.EntityFrameworkCore;
using TRT_backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TRT_backend.Hubs;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using TRT_backend.Services;
using TRT_backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


 builder.Services.AddCors(options =>
 {
     
     //emre main comp test server 
     /* options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("http://127.0.0.1:5500", "http://127.0.0.1:5501") // frontend adresleri
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();  // Çok önemli, SignalR için gerekli
    });  */

    options.AddPolicy("CorsPolicy", policy =>
     {
         policy.WithOrigins("http://localhost:3000") // Frontend adresin
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
     });  

 });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Service registrations
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAssigneeService, AssigneeService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Repository registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IAssigneeRepository, AssigneeRepository>();
builder.Services.AddScoped<ITaskCategoryRepository, TaskCategoryRepository>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<IClaimLanguageRepository, ClaimLanguageRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

builder.Services.AddSignalR();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key")),
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // OpenAPI endpoint'i aktif et
}

app.UseHttpsRedirection();

// CORS'u Authentication ve Authorization'dan önce kullan
app.UseCors("CorsPolicy");
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// 24 saatte bir eski mesajları temizleme task
app.Lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        while (true)
        {
            var silinecekler = db.Messages
                .Where(m => m.CreatedAt < DateTime.UtcNow.AddHours(-24));
            db.Messages.RemoveRange(silinecekler);
            await db.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromHours(24));
        }
    });
});


app.Run();


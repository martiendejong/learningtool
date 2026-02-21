using LearningTool.Application.Services;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using LearningTool.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=learningtool.db";

builder.Services.AddDbContext<LearningToolDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<LearningToolDbContext>()
.AddDefaultTokenProviders();

// Authentication configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LearningTool";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LearningToolUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
// Google OAuth disabled for now - uncomment when ClientId/Secret configured
//.AddGoogle(options =>
//{
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
//});

builder.Services.AddAuthorization();

// Repository registration
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
builder.Services.AddScoped<IUserCourseRepository, UserCourseRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// Service registration
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IUserLearningService, UserLearningService>();
builder.Services.AddScoped<IChatService, ChatService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5190", "http://localhost:5192", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LearningTool API",
        Version = "v1",
        Description = "AI-powered learning management system API"
    });

    // JWT authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your JWT token"
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

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-apply migrations in development
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LearningToolDbContext>();
        db.Database.Migrate();

        // Seed initial data
        await DataSeeder.SeedAsync(db);
    }
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

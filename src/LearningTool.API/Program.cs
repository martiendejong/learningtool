using LearningTool.API.Services;
using LearningTool.Application.Services;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using LearningTool.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Rate limiting — 10 requests per minute per IP on auth endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=learningtool.db";

builder.Services.AddDbContext<LearningToolDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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

    // Validate security stamp on every authenticated request
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var stampInToken = context.Principal?.FindFirstValue("SecurityStamp");

            if (userId == null || stampInToken == null)
            {
                context.Fail("Invalid token");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.SecurityStamp != stampInToken)
            {
                context.Fail("Token has been revoked");
            }
        }
    };
});
// Google OAuth — only wired up when credentials are present in config
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddCookie("GoogleTemp", opt =>
        {
            opt.Cookie.SameSite = SameSiteMode.Lax;
            opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddGoogle(opt =>
        {
            opt.ClientId = googleClientId;
            opt.ClientSecret = googleClientSecret;
            opt.SignInScheme = "GoogleTemp";
            opt.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/auth/google/callback");
            opt.CorrelationCookie.SameSite = SameSiteMode.Lax;
            opt.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
}

builder.Services.AddAuthorization();

// Repository registration
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
builder.Services.AddScoped<IUserCourseRepository, UserCourseRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// Email service
builder.Services.AddScoped<EmailService>();

// Service registration
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IUserLearningService, UserLearningService>();
builder.Services.AddScoped<IChatService, ChatService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5190", "http://localhost:5191", "http://localhost:5192", "http://localhost:3000")
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

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await LearningTool.API.RoleSeeder.SeedRolesAndAdmin(roleManager, userManager, configuration);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-apply migrations in development - DISABLED FOR PRODUCTION DEPLOY
    // Migration already applied manually to production database
    // using (var scope = app.Services.CreateScope())
    // {
    //     var db = scope.ServiceProvider.GetRequiredService<LearningToolDbContext>();
    //     db.Database.Migrate();

    //     // Seed initial data
    //     await DataSeeder.SeedAsync(db);
    // }
}

// app.UseHttpsRedirection(); // Temporarily disabled for production - SSL certificate needs configuration

app.UseCors();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

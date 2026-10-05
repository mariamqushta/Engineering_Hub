using Engineering_Hub.AutoMapper;
using Engineering_Hub.Hubs;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Engineering_Hub.Repository;
using Engineering_Hub.Services;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 300 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 300 * 1024 * 1024;
});
builder.Services.AddScoped<GenericRepository<RefreshToken>>();
builder.Services.AddScoped<UnitWork>();
builder.Services.AddScoped<ITrackBookingService, TrackBookingService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonAccessService, LessonAccessService>();
builder.Services.AddScoped<ILessonCompletionService, LessonCompletionService>();
builder.Services.AddScoped<IWorkshopBookingService, WorkshopBookingService>();
builder.Services.AddScoped<IInteractiveBookingService, InteractiveBookingService>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<ILessonContentService, LessonContentService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<
    IInstructorAuthorizationService,
    InstructorAuthorizationService>();
builder.Services.AddScoped<
    IInteractiveActivityService,
    InteractiveActivityService>();
builder.Services.AddScoped<ICoachingService, CoachingService>();
builder.Services.AddScoped<ITrackCompletionService, TrackCompletionService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<EngineeringHubContext>(options =>
               options.UseSqlServer(
                   builder.Configuration.GetConnectionString("DefaultConnection")
               ));
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MapingProfile>();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
         .AddEntityFrameworkStores<EngineeringHubContext>()
         .AddDefaultTokenProviders();

builder.Services.AddAuthentication(op => op.DefaultAuthenticateScheme = "myscheme")
          .AddJwtBearer("myscheme", op =>
          {
              string secertkey = builder.Configuration["Jwt:Key"];
              var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secertkey));
              op.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
              {
                  IssuerSigningKey = key,

                  ValidateIssuerSigningKey = true,

                  ValidateLifetime = true,

                  ValidateIssuer = false,

                  ValidateAudience = false,

                  ClockSkew = TimeSpan.Zero
              };

              op.Events = new JwtBearerEvents
              {
                  OnMessageReceived = context =>
                  {
                      context.Token = context.Request.Cookies["jwt"];

                      return Task.CompletedTask;
                  },

                  OnTokenValidated = async context =>
                  {
                      var userManager =
                          context.HttpContext.RequestServices
                              .GetRequiredService<UserManager<ApplicationUser>>();

                      var userId = context.Principal?
                          .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                      if (string.IsNullOrEmpty(userId))
                      {
                          context.Fail("User ID not found.");
                          return;
                      }

                      var user = await userManager.FindByIdAsync(userId);

                      if (user == null || !user.IsActive)
                      {
                          context.Fail("User account is inactive.");
                      }
                  }
              };
          });

builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.MapSwagger();
    app.MapSwaggerUI();
}


using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = {
        "User",
        "Student",
        "Instructor",
        "Expert",
        "Admin",
        "SuperAdmin" };

    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(role))
                       .GetAwaiter()
                       .GetResult();
        }
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CoachingHub>("/coachingHub");
app.Run();

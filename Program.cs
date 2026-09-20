using Engineering_Hub.AutoMapper;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Engineering_Hub.Repository;
using Engineering_Hub.Services;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<GenericRepository<RefreshToken>>();
builder.Services.AddScoped<UnitWork>();
builder.Services.AddScoped<ITrackBookingService, TrackBookingService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonAccessService, LessonAccessService>();
builder.Services.AddScoped<ILessonCompletionService, LessonCompletionService>();
builder.Services.AddScoped<IWorkshopBookingService, WorkshopBookingService>();
builder.Services.AddScoped<IInteractiveBookingService, InteractiveBookingService>();
builder.Services.AddScoped<ITrackPackageBookingService, TrackPackageBookingService>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<
    IInteractiveActivityService,
    InteractiveActivityService>();
builder.Services.AddScoped<
    ITrackPackageService,
    TrackPackageService>();

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
                  ValidateIssuer = false,
                  ValidateAudience = false,
              };

              op.Events = new JwtBearerEvents
              {
                  OnMessageReceived = context =>
                  {
                      context.Token = context.Request.Cookies["jwt"];

                      return Task.CompletedTask;
                  }
              };
          });


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


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

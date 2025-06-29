using Serilog;
using Serilog.Events;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ToDoApi.Middleware;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Application.ServicesInterfaces;
using LibraryApp.JwtFeatures;
using Application.Validators;
using FluentValidation;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using LibraryApp;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/tasks-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Добавление сервисов
    builder.Services.AddAutoMapper(typeof(Program));

    // Настройка БД
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Настройка Identity
    builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Настройка валидации
    builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

    // Отключаем встроенную валидацию ModelState
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // Регистрация фильтра валидации
    builder.Services.AddScoped<ValidationFilter>();

    // Репозитории
    builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<ICommentRepository, CommentRepository>();
    builder.Services.AddScoped<ITaskHistoryRepository, TaskHistoryRepository>();

    // Сервисы
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<ITaskHistoryService, TaskHistoryService>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddSingleton<JwtHandler>();

    // Настройка Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please insert JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
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

    // Настройка JWT аутентификации
    var jwtSettings = builder.Configuration.GetSection("JWTSettings");
    builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["validIssuer"],
            ValidAudience = jwtSettings["validAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(jwtSettings.GetSection("securityKey").Value))
        };
    });

    // Настройка авторизации
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("OnlyAdminUsers", policy => policy.RequireRole("Admin"));
        options.AddPolicy("AuthenticatedUsers", policy => policy.RequireAuthenticatedUser());
    });

    builder.Services.AddControllers()
        .AddFluentValidation(fv =>
        {
            fv.RegisterValidatorsFromAssemblyContaining<ChangeTaskStatusDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<UpdateTaskDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<UserForAuthenticationDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<UserForRegistrationDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<CreateCommentDtoValidator>();
            fv.RegisterValidatorsFromAssemblyContaining<UpdateCommentDtoValidator>();
            fv.AutomaticValidationEnabled = true;
            fv.ImplicitlyValidateChildProperties = true;
        });
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<Middleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            Log.Information("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while applying migrations");
        }
    }

    app.MapControllers();

    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
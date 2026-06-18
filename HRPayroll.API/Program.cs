using HRPayroll.Application;
using HRPayroll.Infrastructure;
using MassTransit;
using HRPayroll.Application.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication: chỉ VALIDATE token bằng shared secret (token do HR Core cấp)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.")))
        };
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AttendanceMonthlyClosedEventConsumer>();
    x.AddConsumer<EmployeeCreatedEventConsumer>();
    x.AddConsumer<EmployeeUpdatedEventConsumer>();
    x.AddConsumer<EmployeeResignedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Ưu tiên RABBITMQ_URL (Railway cấp URI đầy đủ kèm user/pass), fallback host + guest/guest cho local/docker.
        // Phải giống HR Core để 2 service nối CHUNG một broker thì event mới sync được.
        var rabbitMqUrl = builder.Configuration["RABBITMQ_URL"];
        if (!string.IsNullOrEmpty(rabbitMqUrl))
        {
            cfg.Host(new Uri(rabbitMqUrl), h =>
            {
                // Heartbeat 10s: giữ kết nối qua TCP proxy Railway khỏi bị ngắt khi idle,
                // và phát hiện rớt kết nối nhanh (~10s thay vì mặc định 60s) → hết trễ ~30s khi nhận event.
                h.Heartbeat(TimeSpan.FromSeconds(10));
            });
        }
        else
        {
            var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
            cfg.Host(rabbitMqHost, "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
                h.Heartbeat(TimeSpan.FromSeconds(10));
            });
        }

        cfg.ReceiveEndpoint("payroll-event-queue", e =>
        {
            e.ConfigureConsumer<AttendanceMonthlyClosedEventConsumer>(context);
            e.ConfigureConsumer<EmployeeCreatedEventConsumer>(context);
            e.ConfigureConsumer<EmployeeUpdatedEventConsumer>(context);
            e.ConfigureConsumer<EmployeeResignedEventConsumer>(context);
        });
    });
});

builder.Services.AddControllers();

// CORS: cho phép frontend (Vue) gọi API qua trình duyệt. JWT đi trong header nên không cần AllowCredentials.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Cho phép dán JWT (Bearer) ngay trong Swagger UI để test endpoint có [Authorize]
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT (token do HR Core cấp). Chỉ cần dán token, không cần thêm chữ 'Bearer'.",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HRPayroll.Infrastructure.Persistence.HRPayrollDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
// Luôn bật Swagger để test/demo (kể cả production trên Railway), giống HR Core
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

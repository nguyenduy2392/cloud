using Application;
using Application.Options;
using Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10 GB
    options.Limits.MinRequestBodyDataRate = null;
    options.Limits.MinResponseDataRate = null;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
});

#region Cài đặt EF Core

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var accessor = sp.GetRequiredService<IAppContextAccessor>();
    options.UseSqlServer(accessor.GetConnectionString(), o => o.MigrationsAssembly("Core"));
}, ServiceLifetime.Transient);
#endregion

// Đăng ký DI hệ thống
builder.Registers();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024; // 10 GB
});

#region Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cloud",
        Version = "v1",
    });

    c.CustomSchemaIds(i => i.FullName);

    c.MapType<Microsoft.AspNetCore.Http.IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

#endregion

#region Quản lý CORS

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
buiders =>
{
    buiders
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin();
}));

#endregion

builder.Services.AddHttpContextAccessor();

// Sử dụng cache trong ứng dụng
builder.Services.AddDistributedMemoryCache();

#region Cài đặt JWT Token

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
              .GetBytes(builder.Configuration.GetSection("AppSetting:Token").Value)),
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero,
      };
  });
#endregion

#region Cài đặt FrontEnd Folder

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp";
});

#endregion

#region Đọc dữ liệu appsettings.json

builder.Services.Configure<AppSetting>
    (builder.Configuration.GetSection(nameof(AppSetting)));
builder.Services.Configure<ConnectionStrings>
    (builder.Configuration.GetSection(nameof(ConnectionStrings)));
builder.Services.Configure<ServerSetting>
    (builder.Configuration.GetSection(nameof(ServerSetting)));
builder.Services.Configure<SsoSettings>
    (builder.Configuration.GetSection(nameof(SsoSettings)));

builder.Services.AddHttpClient();

#endregion

var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

Log.Logger = logger;
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "YourSessionCookieName";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();
var env = app.Environment;
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.ConfigureExceptionHandler();
        app.UseHsts();

    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "--project api | 1.0.0");
    });

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");

    // Đảm bảo thư mục tồn tại
    if (!Directory.Exists(assetsPath))
    {
        Directory.CreateDirectory(assetsPath);
    }

    // Tạo thư mục Assets nếu chưa tồn tại
    if (!Directory.Exists(assetsPath))
    {
        Directory.CreateDirectory(assetsPath);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(assetsPath),
        RequestPath = "/Files"
    });

    app.UseCors("CorsPolicy");
    app.UseHttpsRedirection();

    // HMAC verify cho API database/initialize, migration/run (gọi từ EcoControl)
    app.UseMiddleware<Api.Security.HmacAuthMiddleware>();

    // Chuyển ?t=TOKEN thành Authorization header (cho preview ảnh/video trong <img>/<video>)
    app.UseMiddleware<Api.Security.QueryTokenAuthMiddleware>();

    app.UseAuthentication();
    app.UseRouting();
    app.UseSession(); // Add this line after UseRouting
    app.UseAuthorization();

    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseSpaStaticFiles();
    app.UseCookiePolicy();


    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    app.UseSpa(spa =>
    {
    });


}

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

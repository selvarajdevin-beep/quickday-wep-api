using AquaERP.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Shop.Web.Api.Repositories;
using Shop.Web.Api.Services;
using Shop.Web.API.Middleware;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Repositories;
using Shop.Web.API.Services;
using Shop.Web.API.Validators;
using System.Text;

// ── Bootstrap Serilog before anything else ────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog (full config from appsettings) ─────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "AquaERP.Api"));

    // ── Controllers + FluentValidation ────────────────────
    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(opt =>
        {
            // Disable default 400 response — we handle ModelState manually
            // so our ApiResponse<T> envelope is used consistently
            opt.SuppressModelStateInvalidFilter = true;
        });

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

    // ── DI ────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
    builder.Services.AddScoped<ISupplierService, SupplierService>();
    builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
    builder.Services.AddScoped<IPurchaseService, PurchaseService>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
    builder.Services.AddScoped<IExpenseService, ExpenseService>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IReportRepository, ReportRepository>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IAppConstantsRepository, AppConstantsRepository>();
    builder.Services.AddSingleton<IAppConstantsService, AppConstantsService>();
    builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
    builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();

    // ── JWT Authentication ────────────────────────────────
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtSecret = jwtSection["Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret must be set in appsettings.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero  // strict expiry — no grace period
            };

            // Return JSON 401, not an HTML redirect
            opt.Events = new JwtBearerEvents
            {
                OnChallenge = async ctx =>
                {
                    ctx.HandleResponse();
                    ctx.Response.StatusCode = 401;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync(
                        """{"success":false,"message":"Unauthorized. Please log in.","errorCode":"UNAUTHORIZED"}""");
                },
                OnForbidden = async ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.WriteAsync(
                        """{"success":false,"message":"You do not have permission to perform this action.","errorCode":"FORBIDDEN"}""");
                }
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperAdminOnly", policy =>
            policy
                .RequireAuthenticatedUser()
                .RequireClaim("isSuperAdmin", "true"));
    });

    // ── CORS ──────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:4200", "https://quickday-angular.vercel.app", "https://fabulous-chaja-c510d8.netlify.app"];

    builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()));

    // ── Swagger (dev only) ────────────────────────────────
    builder.Services.AddOpenApi();

    //// ─────────────────────────────────────────────────────
    //var app = builder.Build();
    //// ─────────────────────────────────────────────────────

    //// ── Middleware pipeline (ORDER MATTERS) ───────────────
    //app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1st — catch all
    //app.UseMiddleware<RequestLoggingMiddleware>();     // 2nd — log every request

    ////if (app.Environment.IsDevelopment())
    ////{
    ////    app.MapOpenApi();       // serves at /openapi/v1.json
    ////    app.MapScalarApiReference(); // serves at /scalar/v1
    ////}

    //app.UseHttpsRedirection();
    //app.UseCors("Angular");
    //app.UseAuthentication();  // must be before UseAuthorization
    //app.UseAuthorization();
    //app.MapControllers();

    //Log.Information("AquaERP API starting up on {Env}", app.Environment.EnvironmentName);
    //app.Run();

    // ─────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────

    // ── Middleware pipeline (ORDER MATTERS) ───────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1st — catch all
    app.UseMiddleware<RequestLoggingMiddleware>();     // 2nd — log every request

    app.MapOpenApi();              // serves at /openapi/v1.json
    app.MapScalarApiReference();   // serves at /scalar/v1

    app.MapGet("/", () => "AquaERP API Running");

    app.UseHttpsRedirection();
    app.UseCors("Angular");
    app.UseAuthentication();  // must be before UseAuthorization
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("AquaERP API starting up on {Env}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AquaERP API failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
using Microsoft.OpenApi.Models;
using EventManagementApi.Entity;
using EventManagementApi.Database;
using EventManagementApi.utils;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers(
    Options =>
    {
        Options.SuppressAsyncSuffixInActionNames = false;
    }
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EventProvider", policy => policy.RequireRole("EventProvider"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

builder.Services.AddSingleton(s => new BlobServiceClient(builder.Configuration["BlobStorage:ConnectionString"]));
builder.Services.AddSingleton(s => new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));
// builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["RedisCache:ConnectionString"]));
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

builder.Services.AddSingleton(s =>
{
    var accountEndpoint = builder.Configuration["CosmosDb:Account"];
    var accountKey = builder.Configuration["CosmosDb:Key"];
    return new CosmosClient(accountEndpoint, accountKey);
});

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Event Management System API",
        Version = "v1"
    });

    // Add security definitions
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
});

// add automapper dependency injection
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOriginPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

// Ensure seed data on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedingData.Initialize(services).Wait();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management System API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseCors("AllowAnyOriginPolicy");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
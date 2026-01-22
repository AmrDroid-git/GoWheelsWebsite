using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services;
using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using GoWheels.Validators;


var builder = WebApplication.CreateBuilder(args);

// This allows Npgsql to handle "Unspecified" DateTimes like it did during Model Seeding.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// DataBase:
builder.Services.AddDbContext<GoWheelsDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(
        options => options.SignIn.RequireConfirmedAccount = false
        )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GoWheelsDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddScoped<IUserValidator<ApplicationUser>, PhoneNumberValidator>();


// Registering Services
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

builder.Services.AddHostedService<DbInitializer>();


// Controllers
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // This stops the cycle by simply ignoring the repeating object
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Add Razor Pages support
builder.Services.AddRazorPages();

// Application 
var app = builder.Build();

// DB Initialization (Migration must happen before app starts serving requests)
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.DropAndMigrateDatabaseAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages
app.MapRazorPages();

// Temporary: list all endpoints
app.MapGet("/endpoints", () =>
{
    var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

    var endpoints = endpointDataSource.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            e.DisplayName,
            Route = e.RoutePattern.RawText
        });

    return Results.Json(endpoints);
});


app.Run();
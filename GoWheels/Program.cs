using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services;
using GoWheels.Services.Startup;
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
builder.Services.AddScoped<IUserValidator<ApplicationUser>, PhoneNumberValidator>();


// Registering Services
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ICommentsService, CommentsService>();
builder.Services.AddScoped<IRatingsService, RatingsService>();

builder.Services.AddHostedService<PostRatingsStartupService>();


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

// DB Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<GoWheelsDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    
    // 1. Create Roles
    var roles = new List<string> { "ADMIN", "EXPERT", "USER" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 2. Create Default Users
    var names = new List<string> { "admin", "expert", "user" };
    foreach (var name in names)
    {
        if (await userManager.FindByNameAsync(name) == null)
        {
            var user = new ApplicationUser { UserName = name, Email = $"{name}@{name}.com", EmailConfirmed = true, PhoneNumber = "98756683", Address = "CUN"};
            var result = await userManager.CreateAsync(user, "Password123!"); // Identity usually requires a strong password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, name.ToUpper());
            }
        }
    }
    
    // 3. Feed JSON Seed
    if (await context.Posts.CountAsync() == 0)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var basePath = Path.Combine("Data", "Seed");

        var usersPath      = Path.Combine(basePath, "users.json");
        var postsPath      = Path.Combine(basePath, "posts_clean.json");
        var postImagesPath = Path.Combine(basePath, "post_images.json");
        var commentsPath   = Path.Combine(basePath, "comments_seed.json");
        var postsRatingsPath    = Path.Combine(basePath, "ratings_posts.json");

        if (File.Exists(usersPath))
        {
            var users = JsonSerializer.Deserialize<List<ApplicationUser>>(
                File.ReadAllText(usersPath), options);
            if(users != null) await context.Users.AddRangeAsync(users);
        }
        if (File.Exists(postsPath))
        {
            var posts = JsonSerializer.Deserialize<List<Post>>(
                File.ReadAllText(postsPath), options);
            if(posts != null) await context.Posts.AddRangeAsync(posts);
        }
        if (File.Exists(postImagesPath))
        {
            var postImages = JsonSerializer.Deserialize<List<PostImage>>(
                File.ReadAllText(postImagesPath), options);
            if(postImages != null) await context.PostImages.AddRangeAsync(postImages);
        }
        if (File.Exists(postsRatingsPath))
        {
            var postsRatings = JsonSerializer.Deserialize<List<RatingPost>>(
                File.ReadAllText(postsRatingsPath), options);
            if (postsRatings != null) await context.PostsRatings.AddRangeAsync(postsRatings);
        }
        if (File.Exists(commentsPath))
        {
            var comments = JsonSerializer.Deserialize<List<Comment>>(
                File.ReadAllText(commentsPath), options);
            if(comments != null) await context.Comments.AddRangeAsync(comments);
        }
        
        await context.SaveChangesAsync();
    }
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
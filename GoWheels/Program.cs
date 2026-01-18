using Microsoft.EntityFrameworkCore;
using GoWheels.Data;
using GoWheels.Models;
using GoWheels.Services;
using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

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

// // Registering Services
// builder.Services.AddScoped<IPostsService, PostsService>();
// builder.Services.AddScoped<IUsersService, UsersService>();
// builder.Services.AddScoped<ICommentsService, CommentsService>();
// builder.Services.AddScoped<IRatingsService, RatingsService>();

// Controllers
builder.Services.AddControllersWithViews();

// Application 
var app = builder.Build();

// Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Create Roles
    var roles = new List<string> { "ADMIN", "EXPERT", "USER" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create Default Users
    var names = new List<string> { "admin", "expert", "user" };
    foreach (var name in names)
    {
        if (await userManager.FindByNameAsync(name) == null)
        {
            var user = new ApplicationUser { UserName = name, Email = $"{name}@{name}.com", EmailConfirmed = true, PhoneNumber = "98756683", Location = "CUN"};
            var result = await userManager.CreateAsync(user, "Password123!"); // Identity usually requires a strong password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, name.ToUpper());
            }
        }
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

app.Run();
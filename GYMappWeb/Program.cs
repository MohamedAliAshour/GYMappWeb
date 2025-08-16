using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using GYMappWeb.Services; // Add this namespace for your services
using GYMappWeb.Interface;
using GYMappWeb.Service; // Add this namespace for your interfaces

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("GYMappWebContextConnection") ?? throw new InvalidOperationException("Connection string 'GYMappWebContextConnection' not found.");

// Add services to the container.
builder.Services.AddDbContext<GYMappWebContext>(options =>
    options.UseSqlServer(connectionString));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register your services
builder.Services.AddScoped<ITblUserMemberShip, TblUserMemberShipService>();
builder.Services.AddScoped<ITblUser, TblUserService>();
builder.Services.AddScoped<ITblOffer, TblOfferService>();
builder.Services.AddScoped<ITblMembershipType, TblMembershipTypeService>();
builder.Services.AddScoped<ITblMemberShipFreeze, TblMemberShipFreezeService>();
// Add other services if needed
// builder.Services.AddScoped<IMyOtherService, MyOtherService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
    .AddEntityFrameworkStores<GYMappWebContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionCheckFilter>();
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
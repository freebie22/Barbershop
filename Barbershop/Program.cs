using Barbershop.Data;
using Barbershop.Initializer;
using Barbershop.Models;
using Barbershop.Utility;
using Barbershop.Utility.BrainTreeSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<BrainTreeSettings>(builder.Configuration.GetSection("BrainTree"));
builder.Services.AddSingleton<IBrainTreeGate, BrainTreeGate>();
var apiKey = builder.Configuration.GetSection("Twillio").GetValue<string>("VerifyApiKey");
builder.Services.AddHttpClient<TwilioVerifyClient>(client =>
{
    client.BaseAddress = new Uri("https://api.authy.com/");
    client.DefaultRequestHeaders.Add("X-Authy-API-Key", apiKey);
});
builder.Services.AddAuthentication().AddFacebook(Options =>
{
    Options.AppId = "779058353446737";
    Options.AppSecret = "608376fbe0c3d9af9021e4468aa395d7";

} );
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(Options =>
{
    Options.IdleTimeout = TimeSpan.FromDays(30);
    Options.Cookie.HttpOnly = true;
    Options.Cookie.IsEssential = true;
});


builder.Services.Configure<IdentityOptions>(options =>
{
    // Lockout settings
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 3;
});

builder.Services.AddScoped<IDbInitializer, DbInitiazlizer>();

var app = builder.Build();

var dbInitializerServiceProvider = app.Services.CreateScope().ServiceProvider;
var dbInitializer = dbInitializerServiceProvider.GetRequiredService<IDbInitializer>();


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
dbInitializer.Initialize();
app.UseSession();


app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

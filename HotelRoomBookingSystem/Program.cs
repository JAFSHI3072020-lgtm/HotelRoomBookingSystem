using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Data;
using HotelBookingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================

builder.Services.AddControllersWithViews();

// ---------- DATABASE ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- STRIPE SERVICE ----------
builder.Services.AddScoped<IStripeService, StripeService>();

// ---------- SESSION ----------
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".HotelBooking.Session";
});

// ---------- LOGGING ----------
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// ================= APP =================

var app = builder.Build();

// ---------- MIDDLEWARE ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

// ---------- ROUTES ----------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ---------- DATABASE MIGRATION ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();   // auto apply migrations
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();

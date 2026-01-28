using CinemaInfrastructure;
using CinemaWeb.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddMvcOptions(options =>
    {
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Це поле є обов'язковим.");
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((val, prop) => $"Значення '{val}' є некоректним.");
    });

builder.Services.AddRazorPages();

builder.Services
    .AddDefaultIdentity<IdentityUser>(OptionsBuilderConfigurationExtensions =>
{
    OptionsBuilderConfigurationExtensions.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<CinemaDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        })
);

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddHttpClient();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.MapRazorPages();
app.Run();

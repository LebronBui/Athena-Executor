using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.AutoMapperProfile;
using RealEstateAuction.DAL;
using RealEstateAuction.Services;
using RealEstateAuction.ViewComponents;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add automapper service
        builder.Services.AddAutoMapper(typeof(DataModelToModel).Assembly, typeof(ModelToDataModel).Assembly);

        builder.Services.AddTransient<IEmailSender, EmailSender>();

        // Add background worker service
        builder.Services.AddHostedService<BackgroundWokerService>();

        //refresh page after 1 second
        builder.Services.AddSignalR();


        //Take notification view 
        builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
        builder.Services.AddScoped<NotificationDAO>();
        builder.Services.AddTransient<NotificationViewComponent>();


        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/denied";
                options.AccessDeniedPath = "/denied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            });
        builder.Services.AddSession(options =>
        {
            options.Cookie.IsEssential = true;
        });

        var app = builder.Build();

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
        app.UseSession();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

using ConferenceTracker.Data;
using ConferenceTracker.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace ConferenceTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private string _allowedOrigins { get; set; } = "_allowedOrigins";

        public IConfiguration Configuration { get; }
        public string SecretMessage { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ConferenceTracker"));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.Configure<CookiePolicyOptions>(option => { option.CheckConsentNeeded = context => true; option.MinimumSameSitePolicy = SameSiteMode.None; });
            services.AddControllersWithViews();
            services.AddCors(options => { options.AddPolicy(_allowedOrigins, builder => { builder.WithOrigins("http://pluralsight.com"); }); });
            services.AddRazorPages();
            services.AddTransient<IPresentationRepository, PresentationRepository>();
            services.AddTransient<ISpeakerRepository, SpeakerRepository>();
            SecretMessage = Configuration["SecretMessage"];


        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {            
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                context.Database.EnsureCreated();

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseCors(_allowedOrigins);
            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseExceptionHandler("/Home/Error");
            }
            else
            {
                logger.LogInformation("Environment is in development");
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
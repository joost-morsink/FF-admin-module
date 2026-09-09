using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using FfAdminWeb.Utils;
using System.Text.Json.Serialization;
using Calculator.ApiClient;
using FfAdmin.EventStore.ApiClient;
using FfAdminWeb.Middleware;

namespace FfAdminWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFfAdmin()
                .AddMiddlewares();
            services.AddEventStoreClient().BindConfiguration("ApiClient:EventStore");
            services.AddCalculatorClient().BindConfiguration("ApiClient:Calculator");
            services.AddControllersWithViews().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                o.JsonSerializerOptions.Converters.Add(EventConverter.Instance);
            });
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/browser";
            });

            services.AddHostedService<TimedHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        System.IO.Path.Combine(env.ContentRootPath, "ClientApp", "dist", "browser"))
                });
            }

            app.UseRouting();
            app.UseMiddleware<LastRequestMiddleware>();
            app.UseMiddleware<CurrentBranchMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
            });
        }
    }
}

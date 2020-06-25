using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using reCAPTCHA.AspNetCore;

namespace MailApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IMailService>(Configuration.Get<MailService>());
           
            // Add support for ReCAPTCHA
            services.AddRecaptcha(options =>
            {
                options.SiteKey = Configuration["RECAPTCHA_KEY"];
                options.SecretKey = Configuration["RECAPTCHA_SECRET"];
                options.Site = "www.google.com";
            });
            services.AddTransient<IRecaptchaService, RecaptchaService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMailService ms)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(builder =>
                builder.WithOrigins(ms.AllowedHosts())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}

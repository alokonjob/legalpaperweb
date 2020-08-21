using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using PaperWorks.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Emailer;
using SMSer;
using AspNetCore.Identity.Mongo;

namespace PaperWorks
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDefaultIdentity<AspNetCore.Identity.Mongo.Model.MongoUser>(options => options.SignIn.RequireConfirmedAccount = true);
            services.AddIdentityMongoDbProvider<AspNetCore.Identity.Mongo.Model.MongoUser, AspNetCore.Identity.Mongo.Model.MongoRole>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 6;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireDigit = false;
                
            }, mongoIdentityOptions => {
                mongoIdentityOptions.ConnectionString = "mongodb://localhost/MongoIdentityTestDb3";
            });

            services.AddRazorPages();
            services.AddAuthentication()
        .AddGoogle(options =>
        {
            IConfigurationSection googleAuthNSection =
                Configuration.GetSection("Google");
            IConfigurationSection fbAuthNSection =
                Configuration.GetSection("Facebook");

            options.ClientId = googleAuthNSection["ClientId"];
            options.ClientSecret = googleAuthNSection["ClientSecret"];
        }).AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = Configuration["Facebook:AppId"];
            facebookOptions.AppSecret = Configuration["Facebook:AppSecret"];
        }).AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = Configuration["Microsoft:ClientId"];
            microsoftOptions.ClientSecret = Configuration["Microsoft:ClientSecret"];
        });

            services.AddScoped<IEmailer, EmailerBoy>();

            InitTwilio.Init(Configuration);
            services.Configure<TwilioVerifySettings>(Configuration.GetSection("Keys:Twilio"));
            services.AddSingleton<PhoneCountryService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}

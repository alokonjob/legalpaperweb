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

using AspNetCore.Identity.Mongo;
using Address;
using Users;
using User;
using MongoDB.Bson.Serialization.Conventions;
using Fundamentals;
using Fundamentals.Managers;
using Fundamentals.Repository;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Tax;
using Phone;
using OrderAndPayments;
using Fundamentals.DbContext;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Asgard;
using Messaging;
using Twilio;

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
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));
            IHeimdall gateKeeper = new Heimdall(Configuration);
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            //services.AddDefaultIdentity<Clientele>(options => options.SignIn.RequireConfirmedAccount = true);
            services.AddIdentityMongoDbProvider<Clientele, AspNetCore.Identity.Mongo.Model.MongoRole>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 6;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireUppercase = true;
                identityOptions.Password.RequireNonAlphanumeric = true;
                identityOptions.Password.RequireDigit = true;

            }, mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = gateKeeper.GetSecretValue("MongoConnection");
            });
            //https://docs.microsoft.com/en-us/azure/key-vault/general/vs-key-vault-add-connected-service#:~:text=Go%20to%20the%20Azure%20portal,from%20the%20All%20account%20section.
            services.AddRazorPages();
            
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = gateKeeper.GetSecretValue("GooglClientId");// Configuration["GooglClientId"]; ;
                    options.ClientSecret = gateKeeper.GetSecretValue("GoogleClientSecret");// Configuration["GoogleClientSecret"];
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = gateKeeper.GetSecretValue("FacebookAppId");
                    facebookOptions.AppSecret = gateKeeper.GetSecretValue("FacecbookAppSecret");// Configuration["FacecbookAppSecret"];
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = gateKeeper.GetSecretValue("MicrosoftClientId");// Configuration["MicrosoftClientId"];
                    microsoftOptions.ClientSecret = gateKeeper.GetSecretValue("MicrosoftClientSecret");// Configuration["MicrosoftClientSecret"];
                });


            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                 {
                    new CultureInfo("en"),
                    new CultureInfo("de"),
                    new CultureInfo("fr"),
                    new CultureInfo("es"),
                    new CultureInfo("ru"),
                    new CultureInfo("ja"),
                    new CultureInfo("ar"),
                    new CultureInfo("hi"),
                    new CultureInfo("en-GB")
                  };
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            services.AddSession();
            services.AddMemoryCache();
            services.AddMvc().AddViewLocalization();
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddScoped<IEmailer, EmailerBoy>();
            services.AddScoped<IMessaging, MessagingService>();
            services.AddScoped<IHeimdall, Heimdall>();
            MessagingService.Init(gateKeeper);
            //var one = gateKeeper.GetSecretValue("TwilioAccountSID");
            //var two = gateKeeper.GetSecretValue("TwilioAuthToken");
            //TwilioClient.Init(one, two);
            services.AddSingleton<CountryService>();
            services.AddSingleton<StateStaticService>();
            services.AddScoped<IClienteleRepository, ClienteleRepository>();

            services.AddScoped<IClienteleServices, ClienteleServices>();
            services.AddScoped<IServiceManagement, ServiceManager>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IGeographyManagement, GeographyManagement>();
            services.AddScoped<IGeographyRepository, GeographyRepository>();
            services.AddScoped<IEnabledServices, EnabledServicesManager>();
            services.AddScoped<IServiceEnableRepository, ServiceEnableRepository>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IPhoneService, PhoneService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IMongoDbContext, MongoDbContext>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            //services.AddApplicationInsightsTelemetry();
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
            app.UseSession();//For storing session values
            app.UseRouting();
            //https://docs.microsoft.com/en-us/troubleshoot/aspnet/set-current-culture
            //https://www.mikesdotnetting.com/article/345/localisation-in-asp-net-core-razor-pages-cultures
            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            app.UseCookiePolicy();
        }
    }
}

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
using CaseManagementSpace;
using CaseManagement;
using Consultant;
using MongoDB.Bson.Serialization;
using Audit;

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
            //services.AddControllers();  
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
                mongoIdentityOptions.ConnectionString = "mongodb+srv://alok:Host123456@mflix.cxpea.azure.mongodb.net/onjob2?authSource=admin&replicaSet=atlas-x3ev7x-shard-0&readPreference=primary&appname=MongoDB%20Compass&ssl=true";// gateKeeper.GetSecretValue("MongoConnection");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AddEditPeople", policy => policy.RequireRole("Founder", "Staff").RequireClaim("access", new List<string>() { "webadmin", "founder" }));//Staff Management
                options.AddPolicy("AccessConsultants", policy => policy.RequireRole("Founder", "Staff").RequireClaim("access", new List<string>() { "webadmin", "founder","finance" }));//ConsultantMAnagement

                options.AddPolicy("AccessCasesPolicy", policy => policy.RequireRole("Founder","Staff","CaseManager").RequireClaim("access", new List<string>() { "founder", "caselist","finance" }));//CaseManager
                options.AddPolicy("UpdateCasesPolicy", policy => policy.RequireRole("Founder", "Staff", "CaseManager").RequireClaim("access", new List<string>() { "founder", "caseupdate" }));//CaseManager


                options.AddPolicy("RequireConsultantRole", policy => policy.RequireRole("Founder", "Consultant"));

            options.AddPolicy("ManageFinancePolicy", policy => policy.RequireRole("Founder", "Staff").RequireClaim("access", new List<string>() { "founder","finance" }));//Finance

                //Founder Role - Founder , Claims founder
                //consultnant - Role Consultant
                //caseManager - Role CaseManager, claims caselist,caseupdate
                //Finance -> Role Staff , claims finance
                //Webadmin Role Staff, claims webadmin

                //So Roles Founder  are Founder, Consultant, CaseManager , Staff
                //Claims are founder, webadmin


            });
            BsonClassMap.RegisterClassMap<RazorPePaymentDetails>(cm =>
            {
                cm.MapProperty(c => c.PaymentGateWay_OrderId);
                cm.MapProperty(c => c.PaymentGateWay_PayId);
                cm.MapIdProperty(c => c.PaymentGateWay_Signature);
            });
            //https://docs.microsoft.com/en-us/azure/key-vault/general/vs-key-vault-add-connected-service#:~:text=Go%20to%20the%20Azure%20portal,from%20the%20All%20account%20section.
            services.AddRazorPages();

            services.AddAuthentication();
                //.AddGoogle(options =>
                //{
                //    options.ClientId = gateKeeper.GetSecretValue("GooglClientId");// Configuration["GooglClientId"]; ;
                //    options.ClientSecret = gateKeeper.GetSecretValue("GoogleClientSecret");// Configuration["GoogleClientSecret"];
                //});
                //.AddFacebook(facebookOptions =>
                //{
                //    facebookOptions.AppId = gateKeeper.GetSecretValue("FacebookAppId");
                //    facebookOptions.AppSecret = gateKeeper.GetSecretValue("FacecbookAppSecret");// Configuration["FacecbookAppSecret"];
                //})
                //.AddMicrosoftAccount(microsoftOptions =>
                //{
                //    microsoftOptions.ClientId = gateKeeper.GetSecretValue("MicrosoftClientId");// Configuration["MicrosoftClientId"];
                //    microsoftOptions.ClientSecret = gateKeeper.GetSecretValue("MicrosoftClientSecret");// Configuration["MicrosoftClientSecret"];
                //});


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
            services.AddScoped<IClienteleStaffRepository, ClienteleRepository>();


            services.AddScoped<IClienteleServices, ClienteleServices>();
            services.AddScoped<IClienteleStaffServices, ClienteleServices>();
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

            services.AddScoped<ICaseManagement, CaseManagementSpace.CaseManagement>();
            services.AddScoped<ICaseRepository, CaseRepository>();
            services.AddScoped<ICaseUpdateService, CaseUpdateService>();
            services.AddScoped<ICaseUpdateRepository, CaseUpdateRepository>();
            services.AddScoped<IConsultantCareerManagement, ConsultantCareerManagement>();
            services.AddScoped<IConsultantCareerRepository, ConsultantCareerRepository>();
            services.AddScoped<ICasePaymentReleaseService, CasePaymentReleaseService>();
            services.AddScoped<ICasePaymentReleaseRepository, CasePaymentReleaseRepository>();

            services.AddScoped<IOrderAuditService, OrderAuditService>();
            services.AddScoped<IOrderAuditRepository, OrderAuditRepository>();
            services.AddScoped<INudgeService, NudgeService>();
            services.AddScoped<INudgeRepository, NudgeRepository>();
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
                //endpoints.MapControllers();
            });
            app.UseCookiePolicy();
        }
    }
}

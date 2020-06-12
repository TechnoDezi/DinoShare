using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinoShare.Helpers;
using DinoShare.Helpers.EmailServiceFactory;
using DinoShare.Models;
using DinoShare.ViewModels;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DinoShare
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
            //Add Entity Framework
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));

            //Add DI
            services.Configure<SecurityOptions>(Configuration.GetSection("Security"));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<EmailOptions>(Configuration.GetSection("Email"));
            services.AddScoped<BackgroundJobHelper>();
            services.AddTransient<IEmailService, EmailService>();

            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = GetJwtSigningCredentials();
            });

            //Add Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.Cookie.Name = "AppAuthenticationCookie";
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["JwtIssuerOptions:Issuer"],
                        ValidAudience = Configuration["JwtIssuerOptions:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Security:SecretKey"]))
                    };
                });

            //Add Swagger Generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Integration", Version = "v1" });
                c.IncludeXmlComments(GetXmlCommentsPath(PlatformServices.Default.Application));
                c.OperationFilter<SwaggerAuthorizationHeaderParameterOperationFilter>();
            });

            //Add caching and session support
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString =
                    Configuration.GetConnectionString("CacheConnection");
                options.SchemaName = Configuration["CacheSettings:SchemaName"];
                options.TableName = Configuration["CacheSettings:TableName"];
                options.DefaultSlidingExpiration = new TimeSpan(12, 0, 0);
            });
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
            });

            services.AddControllersWithViews();
        }

        private string GetXmlCommentsPath(ApplicationEnvironment appEnvironment)
        {
            return Path.Combine(appEnvironment.ApplicationBasePath, "App.xml");
        }

        private SigningCredentials GetJwtSigningCredentials()
        {
            var secSettings = Configuration.GetSection("Security");

            string secretKey = secSettings["SecretKey"];
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDBContext dbcontext, IOptions<SecurityOptions> securityOptions,
            IRecurringJobManager recurringJobs)
        {
            dbcontext.Database.Migrate();

            app.UseOptions();
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
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
            app.UseSession();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizeFilter() }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            DbInitializer.Initialize(dbcontext, securityOptions.Value);

            //Setup Background Jobs
            SetupBackgroundJobs(recurringJobs);
        }

        private void SetupBackgroundJobs(IRecurringJobManager recurringJobs)
        {
            recurringJobs.AddOrUpdate("CleanApplicationLog", Job.FromExpression<BackgroundJobHelper>(x => x.CleanApplicationLog()), "0 8 * * SAT");

            recurringJobs.AddOrUpdate("RescanDirectories", Job.FromExpression<BackgroundJobHelper>(x => x.RescanDirectories()), "0 23 * * *");
        }
    }
}

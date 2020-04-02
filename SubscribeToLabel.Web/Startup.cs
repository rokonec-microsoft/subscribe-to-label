using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNet.SubscribeToLabel.Web.Models.Settings;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Repositories;
using DotNet.SubscribeToLabel.Web.Features.AreaOwner;

namespace DotNet.LabelNotifier.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // settings
            services.Configure<GitHubAppOptions>(Configuration.GetSection("GitHubApp").Bind);

            // authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            })
            .AddGitHub(options =>
            {
                options.ClientId = "Iv1.9a6d83ca2e0101b5";
                options.ClientSecret = "a7c92d38808e84806e0705d18ad1f039ba5b63f5";
                options.Scope.Add("user:email");
                options.SaveTokens = true;
            });

            services.AddSingleton<ILabelSubscription, LabelSubscription>();
            services.AddSingleton<IIssueSubscriber, GitHubIssueSubscriber>();
            services.AddSingleton<IIssueQuery, GitHubIssueQuery>();
            services.AddSingleton<IListRepositories, GitHubInstallationRepositories>();
            services.AddSingleton<IGitHubClientFactory, GitHubClientFactory>();
            services.AddSingleton<ILabelSubscriptionRepository, InMemoryLabelSubscriptionRepository>();
            services.AddSingleton<IIssueLabelRepository, InMemoryIssueLabelRepository>();
            services.AddSingleton<IAreaOwnerProvider, GitHubAreaOwnerProvider>();
            services.AddSingleton<IIssueSubscription, IssueSubscription>();

            services.AddControllersWithViews()
                .AddGitHubWebHooks();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

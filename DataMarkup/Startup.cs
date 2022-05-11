using DataMarkup.Data;
using DataMarkup.Models;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<DataMarkupContext>(options => options
            .UseSqlServer(Configuration.GetConnectionString(nameof(DataMarkup)) ?? throw new NullReferenceException()));

        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<DataMarkupContext>();

        services.AddControllersWithViews()
            .AddRazorRuntimeCompilation()
            .AddNewtonsoftJson();
    }

    public static void Configure(IApplicationBuilder application, DataMarkupContext dataMarkupContext)
    {
        dataMarkupContext.Database.EnsureCreated();

        application.UseDeveloperExceptionPage();

        application.UseHttpsRedirection();
        application.UseStaticFiles();

        application.UseRouting();

        application.UseAuthentication();
        application.UseAuthorization();

        application.UseEndpoints(endpoints =>
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}"));
    }
}

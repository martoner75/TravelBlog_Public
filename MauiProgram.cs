using Auth0.OidcClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TravelBlog.Models;
using TravelBlog.Services;

namespace TravelBlog
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("TravelBlog.appsettings.json");

            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();

            builder.Configuration.AddConfiguration(config);

            var settings = config.GetRequiredSection("Settings").Get<Settings>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddSingleton(settings);
            builder.Services.AddSingleton(new Auth0Client(new()
            {
                Domain = settings.Domain,
                ClientId = settings.ClientId,
                RedirectUri = settings.RedirectUri,
                PostLogoutRedirectUri = settings.PostLogoutRedirectUri,
                Scope = settings.Scope,
            }));
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddSingleton<IInAppPurchaseService, InAppPurchaseService>();
            builder.Services.AddSingleton<IBaseRepository, BaseRepository>();
            builder.Services.AddSingleton<IRepositoryService, RepositoryService>();

            return builder.Build();
        }
    }
}
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using MetroLog.Maui;
using Microsoft.Extensions.Logging;
using Plugin.InAppBilling;
using System.Text.Json;
using TravelBlog.Models;
using TravelBlog.Services;

namespace TravelBlog
{
    public partial class MainPage : ContentPage
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IInAppPurchaseService _inAppPurchaseService;
        private readonly IRepositoryService _repositoryService;
        private readonly Settings _settings;
        private readonly string _appName = AppInfo.Current.Name;
        private readonly string _appPackage = AppInfo.Current.PackageName;
        private readonly string _appVersion = AppInfo.Current.VersionString;
        private readonly string _appBuild = AppInfo.Current.BuildString;
        private readonly ILogger<MainPage> _logger;

        public MainPage(
            ILogger<MainPage> logger,
            IAuthenticationService authenticationService,
            IInAppPurchaseService inAppPurchaseService,
            IRepositoryService repositoryService,
            Settings settings)
        {
            InitializeComponent();
            BindingContext = new LogController();
            _logger = logger;
            _authenticationService = authenticationService;
            _inAppPurchaseService = inAppPurchaseService;
            _repositoryService = repositoryService;
            _settings = settings;
            AppNameLbl.Text = _appName;
            AppPackageLbl.Text = _appPackage;
            AppVersionLbl.Text = $"v. {_appVersion} ({_appBuild})";
        }

        private async void OnLoginClicked(
            object sender,
            EventArgs e)
        {
            var loginResult = await _authenticationService.LoginAsync();

            if (!loginResult.IsError)
            {
                UsernameLbl.Text = loginResult.User.Identity.Name;
                var claims = _authenticationService.GetClaims(loginResult);

                UserPictureImg.Source = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                var profiles = claims.Where(c => c.Type == "profiles").ToList();
                UsernameProfilesLbl.Text = string.Join(", ", profiles.Select(c => c.Value).ToList());

                LoginView.IsVisible = false;
                HomeView.IsVisible = true;
            }
            else
                await DisplayAlert("Error", loginResult.ErrorDescription, "OK");
        }

        private async void OnLogoutClicked(
            object sender,
            EventArgs e)
        {
            var logoutResult = await _authenticationService.LogoutAsync();

            if (logoutResult == BrowserResultType.Success)
            {
                HomeView.IsVisible = false;
                LoginView.IsVisible = true;
            }
            else
                await DisplayAlert("Error", "There was an error while logging off, please try later.", "OK");
        }

        private async void OnSyncPurchaseHistoryClicked(
            object sender,
            EventArgs e)
        {
            _logger.LogInformation($"{nameof(OnSyncPurchaseHistoryClicked)}> Retrieving the list of purchases");

            var purchasesFromStore = await _inAppPurchaseService.GetAllPurchasesAsync();

            if (purchasesFromStore.Any())
                await _repositoryService.StorePurchases(purchasesFromStore);

            var result = await _repositoryService.GetStoredPurchases();

            if (!result.Any())
                await DisplayAlert("Information", "No purchases so far!", "OK");

            await DisplayAlert("Information", "Purchases are now synced", "OK");
        }

        private async void OnPurchaseSubscriptionClicked(
            object sender,
            EventArgs e)
        {
            var result = await _inAppPurchaseService.PurchaseAsync(
                ItemType.Subscription,
                _settings.Subscription);

            await DisplayAlert("Information", JsonSerializer.Serialize(result), "OK");
        }

        private async void OnPurchaseSubscriptionNRClicked(
            object sender,
            EventArgs e)
        {
            var result = await _inAppPurchaseService.PurchaseAsync(
                ItemType.Subscription,
                _settings.SubscriptionNR);

            await DisplayAlert("Information", JsonSerializer.Serialize(result), "OK");
        }

        private async void OnPurchaseConsumableClicked(
            object sender,
            EventArgs e)
        {
            var result = await _inAppPurchaseService.PurchaseAsync(
                ItemType.InAppPurchaseConsumable,
                _settings.ConsumableIAP);

            await DisplayAlert("Information", JsonSerializer.Serialize(result), "OK");
        }

        private async void OnPurchaseNonConsumableClicked(
            object sender,
            EventArgs e)
        {
            var result = await _inAppPurchaseService.PurchaseAsync(
                ItemType.InAppPurchase,
                _settings.NonConsumableIAP);

            await DisplayAlert("Information", JsonSerializer.Serialize(result), "OK");
        }
    }
}
using IdentityModel.OidcClient.Browser;
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
        private readonly IBaseRepository _baseRepository;
        private readonly Settings _settings;
        private readonly string _appName = AppInfo.Current.Name;
        private readonly string _appPackage = AppInfo.Current.PackageName;
        private readonly string _appVersion = AppInfo.Current.VersionString;
        private readonly string _appBuild = AppInfo.Current.BuildString;

        public MainPage(
            IAuthenticationService authenticationService,
            IInAppPurchaseService inAppPurchaseService,
            IBaseRepository baseRepository,
            Settings settings)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
            _inAppPurchaseService = inAppPurchaseService;
            _baseRepository = baseRepository;
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
            var purchasesFromStore = await _inAppPurchaseService.GetAllPurchasesAsync();

            if (purchasesFromStore.Any())
            {
                foreach (var purchase in purchasesFromStore)
                {
                    await _baseRepository.SaveItemAsync(
                        new PurchaseDBModel()
                        {
                            ProductId = purchase._productId,
                            PurchaseType = purchase._purchaseType.ToString()
                        });
                }
            }
            else
                await _baseRepository.SaveItemAsync(
                        new PurchaseDBModel()
                        {
                            ProductId = "TravelBlogFREE",
                            PurchaseType = "Free"
                        });

            var storedPurchases = await _baseRepository.GetItemsAsync();

            if (!storedPurchases.Any())
                await DisplayAlert("Information", "No purchases so far!", "OK");
            else
                PurchaseList.Text = JsonSerializer.Serialize(storedPurchases);
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
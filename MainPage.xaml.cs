using IdentityModel.OidcClient.Browser;
using Plugin.InAppBilling;
using System.Text.Json;
using TravelBlog.Helpers;
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

            PurchaseList.Text = $"{JsonSerializer.Serialize(purchasesFromStore)}\r\n";

            if (purchasesFromStore.Any())
            {
                foreach (var purchase in purchasesFromStore)
                {
                    var productId = await _baseRepository.SaveItemAsync(
                        new PurchaseDBModel()
                        {
                            ProductId = purchase._productId,
                            PurchaseType = purchase._purchaseType
                        });

                    PurchaseList.Text += $"Adding product {productId}: {purchase._productId}/{purchase._purchaseType}\r\n";

                    foreach (var purchaseDetail in purchase.PurchaseItems)
                    {
                        var itemAdded = await _baseRepository.SaveItemDetailAsync(
                            new PurchaseDBModelDetail()
                            {
                                PurchaseModelId = productId,
                                PurchaseId = purchaseDetail.Id,
                                IsAcknowledged = purchaseDetail.IsAcknowledged != null ? purchaseDetail.IsAcknowledged : false,
                                TransactionDateUtc = purchaseDetail.TransactionDateUtc,
                                AutoRenewing = purchaseDetail.AutoRenewing
                            });
                        PurchaseList.Text += $"Adding product detail {itemAdded} for product {productId}: {purchaseDetail.Id}/{purchaseDetail.TransactionDateUtc}/{purchaseDetail.IsAcknowledged} of type: {purchaseDetail.AutoRenewing}\r\n";
                    }
                }
            }
            else
            {
                var productId = await _baseRepository.SaveItemAsync(
                        new PurchaseDBModel()
                        {
                            Id = 1,
                            ProductId = _settings.FreeProductName,
                            PurchaseType = ItemType.InAppPurchase
                        });

                PurchaseList.Text += $"Adding free subscription {productId}: {_settings.FreeProductId}/{ItemType.InAppPurchase}\r\n";

                var detaildId = await _baseRepository.SaveItemDetailAsync(
                        new PurchaseDBModelDetail()
                        {
                            Id = 1,
                            PurchaseModelId = productId,
                            PurchaseId = _settings.FreeProductId,
                            IsAcknowledged = true,
                            TransactionDateUtc = DateTime.Now.ToUniversalTime(),
                            AutoRenewing = false
                        });

                PurchaseList.Text += $"Adding details for free subscription {detaildId}\r\n";
            }

            var result = new List<PurchaseDBModelResponse>();

            var storedPurchases = await _baseRepository.GetItemsAsync();
            var storedPurchaseDetailss = await _baseRepository.GetItemsDetailsAsync();

            result.AddRange(
                ProductMapper.GetProductsFromModel(
                    storedPurchases, 
                    storedPurchaseDetailss)
                );

            if (!result.Any())
                await DisplayAlert("Information", "No purchases so far!", "OK");
            else
                PurchaseList.Text += $"{JsonSerializer.Serialize(result)}\r\n";
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
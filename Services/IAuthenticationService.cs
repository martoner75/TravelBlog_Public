using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Security.Claims;

namespace TravelBlog.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResult> LoginAsync();

        Task<BrowserResultType> LogoutAsync();

        IEnumerable<Claim> GetClaims(LoginResult loginResult);
    }
}
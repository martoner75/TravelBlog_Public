using Auth0.OidcClient;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Security.Claims;

namespace TravelBlog.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Auth0Client _auth0Client;

        public AuthenticationService(Auth0Client auth0Client)
        {
            _auth0Client = auth0Client;
        }

        public IEnumerable<Claim> GetClaims(LoginResult loginResult)
        {
            return loginResult.User.Claims;
        }

        public async Task<LoginResult> LoginAsync()
        {
            return await _auth0Client.LoginAsync();
        }

        public async Task<BrowserResultType> LogoutAsync()
        {
            return await _auth0Client.LogoutAsync();
        }
    }
}
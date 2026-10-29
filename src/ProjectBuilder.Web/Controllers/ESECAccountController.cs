using AppliedResearchAssociates.iAM.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjectBuilder.Models;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Models;
using ProjectBuilder.Web.Security;
using ProjectBuilder.Web.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static ProjectBuilder.Web.Common.LogSanitizer;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;

namespace ProjectBuilder.Web.Controllers
{
    [Authorize]
    [Route("ESECAccount/[action]")]
    public class ESECAccountController : Controller
    {
        private readonly ILogger<ESECAccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IEsecSecurity _esecSecurity;
        private readonly IdentityUserManager _identityUserManager;
        private readonly AbpSignInManager _signInManager;

        public ESECAccountController(
                IUserRepository userRepository,
                ILogger<ESECAccountController> logger,
                IConfiguration configuration,
                IEsecSecurity esecSecurity,
                IdentityUserManager identityUserManager,
                AbpSignInManager signInManager)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
            _esecSecurity = esecSecurity;
            _identityUserManager = identityUserManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult SignIn(bool login = false)
        {
            var securityType = _configuration.GetValue<string>("SecurityType");

            if (securityType == SecurityConstants.SecurityTypes.Esec)
            {
                var esecBase = _configuration["EsecConfig:AuthorizationEndpoint"];
                var clientId = _configuration["EsecConfig:EsecClientId"];
                var redirectUri = _configuration["EsecConfig:EsecRedirect"];
                var scopes = _configuration.GetSection("EsecConfig:scopes_supported").Get<string[]>() ?? new string[0];

                var scope = string.Join(" ", scopes);
                if (!scope.Contains("openid", StringComparison.OrdinalIgnoreCase))
                {
                    scope = $"openid {scope}".Trim();
                }

                var url = $"{esecBase}?response_type=code" +
                          $"&scope={Uri.EscapeDataString(scope)}" +
                          $"&client_id={Uri.EscapeDataString(clientId)}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

                if (login)
                {
                    url += "&prompt=login";
                }

                return Redirect(url);
            }

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, "OpenIdConnect");
        }

        [AllowAnonymous]
        public async Task<IActionResult> AuthorizeESECUser(string? code, bool login = false)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect("/");
            }

            if (!string.IsNullOrEmpty(code))
            {
                var tokens = await GetUserTokens(code);
                var userInfo = await GetUserInfo(tokens.access_token, tokens.refresh_token, tokens.expires_in, tokens.id_token);
                await AddEsecUserWithClaimsAsync(userInfo);
                return Redirect("/");
            }
            return Redirect("/Account/Login");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRefreshToken(string refreshToken)
        {
            try
            {
                // These lines should be removed once certificate issues are fixed.
                var handler = new HttpClientHandler();

                using var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(_configuration["EsecConfig:EsecBaseAddress"])
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Encode the token instead of decoding → prevents injection
                var safeRefreshToken = WebUtility.UrlEncode(refreshToken);

                var query = $"?grant_type=refresh_token&refresh_token={safeRefreshToken}";

                // Build safe URI (prevents SSRF)
                var tokenUri = new Uri(client.BaseAddress!, "token" + query);

                var formData = new[]
                {
                new KeyValuePair<string, string>("client_id", _configuration["EsecConfig:EsecClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["EsecConfig:EsecClientSecret"])
            };

                var content = new FormUrlEncodedContent(formData);

                var responseMsg = await client.PostAsync(tokenUri, content);
                var json = await responseMsg.Content.ReadAsStringAsync();

                ValidateResponse(json);

                // Return safe structured JSON so it cannot trigger XSS
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);
                return Json(tokenResponse);
            }
            catch (Exception e)
            {
                //HubService.SendRealTimeErrorMessage(...)
                return BadRequest(new { error = "Failed to refresh token." });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevokeIdToken()
        {
            try
            {
                // A JWT is too large to store in the URL, so it is passed in the authorization header.
                var tokenParts = Request.Headers["Authorization"].ToString().Split(" ");
                var idToken = tokenParts.Count() > 1 ? tokenParts[1] : string.Empty;
                if (!string.IsNullOrEmpty(idToken))
                {
                    _esecSecurity.RevokeToken(idToken);
                }
                return Ok();
            }
            catch (Exception e)
            {
                // HubService.SendRealTimeErrorMessage(UserInfo?.Name, $"{AuthenticationError}::RevokeToken - {e.Message}", e);
                return Ok();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeAccessToken([FromForm] string token)
        {
            return await RevokeTokenInternal(token);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeRefreshToken([FromForm] string token)
        {
            return await RevokeTokenInternal(token);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutEsecUser()
        {
            try
            {
                // Sign out of the ABP Identity session.
                await _signInManager.SignOutAsync();

                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Sends the browser to the configured ESEC logout page; the target never comes from the request
        [AllowAnonymous]
        [HttpGet]
        public IActionResult EsecLogoutRedirect()
        {
            var esecLogoutUrl = _configuration["EsecConfig:EsecLogoutUrl"]
                ?? throw new InvalidOperationException("EsecLogoutUrl missing in config.");

            return Redirect(esecLogoutUrl);
        }

        #region Private Methods

        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            if (input.Length <= 4) return "****";
            return new string('*', input.Length - 4) + input[^4..];
        }


        public static string MaskTokenResponse(string raw)
        {
            try
            {
                var json = JsonDocument.Parse(raw);
                var root = json.RootElement;

                var result = new Dictionary<string, string>();

                foreach (var prop in root.EnumerateObject())
                {
                    string value = prop.Value.GetString() ?? "";

                    if (prop.Name.Contains("token", StringComparison.OrdinalIgnoreCase))
                        value = Mask(value);

                    result[prop.Name] = value;
                }

                return System.Text.Json.JsonSerializer.Serialize(result);
            }
            catch
            {
                return "***invalid token response***";
            }
        }

        public static string MaskUserInfo(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var result = new Dictionary<string, object?>();

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var name = prop.Name;
                    var value = prop.Value.ToString();

                    // Mask personally identifiable data
                    if (name.Equals("email", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("phone_number", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("name", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("given_name", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("family_name", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("sub", StringComparison.OrdinalIgnoreCase))
                    {
                        value = Mask(value);
                    }

                    result[name] = value;
                }

                return System.Text.Json.JsonSerializer.Serialize(result);
            }
            catch
            {
                return "***invalid userinfo response***";
            }
        }

        private string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var parts = email.Split('@');
            if (parts.Length != 2) return Mask(email);

            var name = parts[0];
            var domain = parts[1];

            var maskedName = name.Length <= 2
                ? new string('*', name.Length)
                : name[0] + new string('*', name.Length - 2) + name[^1];

            return $"{maskedName}@{domain}";
        }

        private string MaskName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return Mask(name);
        }

        private string MaskRoles(string? roles)
        {
            if (string.IsNullOrWhiteSpace(roles)) return "";

            // If roles contain comma-separated entries, mask each role securely
            if (roles.Contains(','))
            {
                return string.Join(",",
                    roles.Split(',')
                         .Select(r => Mask(r.Trim()))
                );
            }

            return Mask(roles);
        }

        private async Task<UserTokensModel> GetUserTokens(string code)
        {
            try
            {
                _logger.LogInformation(
                    "ESEC: Starting token exchange for code"
                );
                var client = CreateHttpClient();
                var responseBody = await CreateHttpResponpse(code, client);
                _logger.LogInformation(
                    "ESEC: Raw token response (masked) => {ResponseBody}",
                    SanitizeForLog(MaskTokenResponse(responseBody))
                );


                ValidateResponse(responseBody);

                return JsonConvert.DeserializeObject<UserTokensModel>(responseBody)
                       ?? throw new InvalidOperationException("Failed to deserialize token response.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching user tokens from ESEC.");
                throw;
            }
        }
        private async Task<UserInfoModel> GetUserInfo(string token, string refresh, int expire, string tokenid)
        {
            try
            {
                var responseJson = await GetUserInfoString(token);
                _logger.LogInformation(
                    "ESEC: UserInfo response (masked) => {UserInfoJsonMasked}",
                    SanitizeForLog(MaskUserInfo(responseJson))
                );

                ValidateResponse(responseJson);
                HttpContext.Session.SetString("AccessToken", token);
                HttpContext.Session.SetString("RefreshToken", refresh);
                HttpContext.Session.SetString("TokenId", tokenid);
                HttpContext.Session.SetString("TokenExpiration",
                    DateTimeOffset.UtcNow.AddSeconds(expire).ToUnixTimeMilliseconds().ToString());

                var userInfo = JsonConvert.DeserializeObject<UserInfoModel>(responseJson)
                 ?? throw new InvalidOperationException("Failed to deserialize user info response.");

                _logger.LogInformation(
                    "ESEC: Parsed user info => Email: {Email}, Name: {Name}, Roles: {Roles}, Sub: {Sub}",
                    SanitizeForLog(MaskEmail(userInfo.Email)),
                    SanitizeForLog(MaskName(userInfo.GivenName + " " + userInfo.LastName)),
                    SanitizeForLog(MaskRoles(userInfo.Roles)),
                    SanitizeForLog(Mask(userInfo.Sub))
                );


                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing user info from ESEC.");
                throw;
            }
        }
        private async Task<IActionResult> RevokeTokenInternal(string token)
        {
            try
            {
                var handler = new HttpClientHandler();

                using var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(_configuration["EsecConfig:EsecBaseAddress"])
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                // SAFETY: encode instead of decode
                var safeToken = WebUtility.UrlEncode(token);

                var query = $"?token={safeToken}";

                // Prevent SSRF by combining with fixed BaseAddress
                var revokeUri = new Uri(client.BaseAddress!, "revoke" + query);

                var formData = new[]
                {
                new KeyValuePair<string, string>("client_id", _configuration["EsecConfig:EsecClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["EsecConfig:EsecClientSecret"])
            };

                var content = new FormUrlEncodedContent(formData);

                var responseMsg = await client.PostAsync(revokeUri, content);
                var json = await responseMsg.Content.ReadAsStringAsync();

                if (responseMsg.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(); // success
                }

                ValidateResponse(json);

                // SAFETY: return parsed JSON to avoid XSS
                var parsed = JsonConvert.DeserializeObject(json);
                return Json(parsed);
            }
            catch
            {
                return BadRequest(new { error = "Token revoke failed." });
            }
        }

        private async Task AddEsecUserWithClaimsAsync(UserInfoModel userInfo)
        {
            _logger.LogInformation(
                "ESEC: Adding user with claims => Email: {Email}, FirstName: {FirstName}, LastName: {LastName}, Roles: {Roles}",
                SanitizeForLog(MaskEmail(userInfo.Email)),
                SanitizeForLog(MaskName(userInfo.GivenName)),
                SanitizeForLog(MaskName(userInfo.LastName)),
                SanitizeForLog(MaskRoles(userInfo.Roles))
            );


            var email = !string.IsNullOrEmpty(userInfo.Email) ? userInfo.Email : ExtractUserId(userInfo.Sub);
            var firstName = userInfo.GivenName;
            var lastName = userInfo.LastName;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError(
                    "ESEC user information is incomplete. Email: {Email}, FirstName: {FirstName}, LastName: {LastName}",
                    SanitizeForLog(MaskEmail(email)),
                    SanitizeForLog(MaskName(firstName)),
                    SanitizeForLog(MaskName(lastName))
                );

                throw new AuthenticationException("Incomplete user information received from ESEC. Missing Email.");
            }

            await AddUser(userInfo, email, firstName, lastName);

            await SignInEsecUserAsync(email);

            _logger.LogInformation("User {Email} authenticated and signed in via ESEC.", SanitizeForLog(MaskEmail(email)));
        }
        private async Task<string> GetUserInfoString(string token)
        {
            try
            {
                var client = CreateHttpClient();

                var formData = new List<KeyValuePair<string, string>>
                    {
                        new("access_token", WebUtility.UrlDecode(token))
                    };

                var content = new FormUrlEncodedContent(formData);

                var response = await client.PostAsync("userinfo", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                         "ESEC userinfo request failed with status {StatusCode}.",
                         response.StatusCode
                    );

                    throw new HttpRequestException($"User info request failed with status code {response.StatusCode}");
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving user info from ESEC.");
                throw;
            }
        }
        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(_configuration["EsecConfig:EsecBaseAddress"])
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
        private async Task<string> CreateHttpResponpse(string code, HttpClient client)
        {
            var formData = new List<KeyValuePair<string, string>>
                    {
                        new("grant_type", "authorization_code"),
                        new("code", WebUtility.UrlDecode(code)),
                        new("redirect_uri", _configuration["EsecConfig:EsecRedirect"]),
                        new("client_id", _configuration["EsecConfig:EsecClientId"]),
                        new("client_secret", _configuration["EsecConfig:EsecClientSecret"])
                    };

            var content = new FormUrlEncodedContent(formData);

            _logger.LogInformation("Sending token request to ESEC.");

            var response = await client.PostAsync("token", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "ESEC token request failed with status {StatusCode}.",
                    response.StatusCode
                );

                throw new HttpRequestException($"Token request failed with status: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        private void ValidateResponse(string response)
        {
            var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            if (!responseJson.ContainsKey("error"))
            {
                return;
            }
            throw new AuthenticationException(responseJson["error_description"]);
        }
        private string ExtractUserId(string sub)
        {
            var parts = sub.Split(',');
            foreach (var part in parts)
            {
                if (part.StartsWith("UserID="))
                {
                    return part.Substring("UserID=".Length);
                }
                else if (part.StartsWith("CN="))
                {
                    return part.Substring("CN=".Length).Trim();
                }
            }
            return null; // or throw exception if UserID is required
        }
        private async Task AddUser(UserInfoModel userInfo, string email, string firstName, string lastName)
        {
            var roleName = GetMappedRoleName(userInfo.Roles);

            if (!await _userRepository.EsesUserExists(email))
            {
                var newUser = new UserModel
                {
                    Email = email,
                    Name = firstName,
                    RoleName = roleName
                };

                await _userRepository.AddEsecUser(newUser);
            }
        }

        private string GetMappedRoleName(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return null;

            var allRolesList = roles
                .Split('^', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.StartsWith("CN=") ? r.Substring(3).Split(',')[0] : r)
                .ToList();

            var roleMappingSection = _configuration.GetSection("RoleMappings");
            var companyKeyword = roleMappingSection["Company"] ?? string.Empty;
            var rolesSection = roleMappingSection.GetSection("Roles").GetChildren();

            var matchingRole = allRolesList.FirstOrDefault(cn =>
                cn.Contains(companyKeyword, StringComparison.OrdinalIgnoreCase) &&
                rolesSection.Any(role => cn.Contains(role.Key, StringComparison.OrdinalIgnoreCase)));

            if (matchingRole != null)
            {
                return matchingRole;
            }

            return null;
        }
        private async Task SignInEsecUserAsync(string email)
        {
            email = NormalizeEmail(email);

            var user = await _identityUserManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new AuthenticationException(
                    $"ESEC user '{email}' was not found in ABP Identity after provisioning.");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
        }
        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            return new EmailAddressAttribute().IsValid(email)
                ? email
                : $"{email}@projectbuilder.com";
        }
        #endregion
    }
}

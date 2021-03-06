﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Indice.AspNetCore.Filters;
using Indice.AspNetCore.Identity.Extensions;
using Indice.AspNetCore.Identity.Models;
using Indice.AspNetCore.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Indice.Identity.Controllers
{
    /// <summary>
    /// Contains all methods related to a user's account.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly IClientStore _clientStore;
        private readonly ExtendedUserManager<User> _userManager;
        private readonly ExtendedSignInManager<User> _signInManager;
        private readonly AccountService _accountService;
        /// <summary>
        /// The name of the controller.
        /// </summary>
        public const string Name = "Account";

        /// <summary>
        /// Creates a new instance of <see cref="AccountController"/>.
        /// </summary>
        /// <param name="interaction">Provide services be used by the user interface to communicate with IdentityServer.</param>
        /// <param name="events">Interface for the event service.</param>
        /// <param name="clientStore">Retrieval of client configuration.</param>
        /// <param name="userManager">Provides the APIs for managing user in a persistence store.</param>
        /// <param name="signInManager">Provides the APIs for user sign in.</param>
        /// <param name="schemeProvider">Responsible for managing what authenticationSchemes are supported.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public AccountController(IIdentityServerInteractionService interaction, IEventService events, IClientStore clientStore, ExtendedUserManager<User> userManager, ExtendedSignInManager<User> signInManager, IAuthenticationSchemeProvider schemeProvider, 
            IHttpContextAccessor httpContextAccessor) {
            _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _accountService = new AccountService(interaction, httpContextAccessor, schemeProvider, clientStore);
        }

        /// <summary>
        /// Displayes the login page.
        /// </summary>
        /// <param name="returnUrl">The URL to navigate after a successful login.</param>
        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl) {
            // Build a model so we know what to show on the login page.
            var viewModel = await _accountService.BuildLoginViewModelAsync(returnUrl);
            if (viewModel.IsExternalLoginOnly) {
                // We only have one option for logging in and it's an external provider.
                return RedirectToAction(nameof(ExternalController.Challenge), ExternalController.Name, new {
                    provider = viewModel.ExternalLoginScheme,
                    returnUrl
                });
            }
            return View(viewModel);
        }

        /// <summary>
        /// Posts the login form to the server.
        /// </summary>
        /// <param name="model">The model that contains user's login info.</param>
        /// <param name="button">The name of the button pressed by the user.</param>
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button) {
            // Check if we are in the context of an authorization request.
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            // The user clicked the 'cancel' button.
            if (button != "login") {
                if (context != null) {
                    // If the user cancels, send a result back into IdentityServer as if they denied the consent (even if this client does not require consent).
                    // This will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);
                    // We can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null.
                    if (await _clientStore.IsPkceClientAsync(context.ClientId)) {
                        // If the client is PKCE then we assume it's native, so this change in how to return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel {
                            RedirectUrl = model.ReturnUrl
                        });
                    }
                    return Redirect(model.ReturnUrl);
                } else {
                    // Since we don't have a valid context, then we just go back to the home page.
                    return Redirect("~/");
                }
            }
            if (ModelState.IsValid) {
                // Validate username/password against database.
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, AccountOptions.AllowRememberLogin && model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded) {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
                    if (context != null) {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId)) {
                            // If the client is PKCE then we assume it's native, so this change in how to return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }
                        // We can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null.
                        return Redirect(model.ReturnUrl);
                    }
                    // Request for a local page.
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl)) {
                        return Redirect(model.ReturnUrl);
                    } else if (string.IsNullOrEmpty(model.ReturnUrl)) {
                        return Redirect("~/");
                    } else {
                        // User might have clicked on a malicious link - should be logged.
                        throw new Exception("Invalid return URL");
                    }
                }
                if (result.IsLockedOut) {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "User locked out."));
                    ModelState.AddModelError(string.Empty, "Your account is temporarily locked. Please contact system administrator.");
                } else {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Invalid credentials."));
                    ModelState.AddModelError(string.Empty, "Please check your credentials.");
                }
            }
            // Something went wrong, show form with error.
            var viewModel = await _accountService.BuildLoginViewModelAsync(model);
            return View(viewModel);
        }

        /// <summary>
        /// Renders the logout page.
        /// </summary>
        /// <param name="logoutId">The logout id.</param>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string logoutId) {
            // Build a model so the logout page knows what to display.
            var viewModel = await _accountService.BuildLogoutViewModelAsync(logoutId);
            if (!viewModel.ShowLogoutPrompt) {
                // If the request for logout was properly authenticated from IdentityServer, then we don't need to show the prompt and can just log the user out directly.
                return await Logout(viewModel);
            }
            return View(viewModel);
        }

        /// <summary>
        /// Posts logout information to the server.
        /// </summary>
        /// <param name="model">The model that contains user's logout info.</param>
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model) {
            // Build a model so the logged out page knows what to display.
            var viewModel = await _accountService.BuildLoggedOutViewModelAsync(model.LogoutId);
            if (User?.Identity.IsAuthenticated == true) {
                // Delete local authentication cookies.
                await _signInManager.SignOutAsync();
                // Raise the logout event.
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }
            // Check if we need to trigger sign-out at an upstream identity provider.
            if (viewModel.TriggerExternalSignout) {
                // Build a return URL so the upstream provider will redirect back to us after the user has logged out. this allows us to then complete our single sign-out processing.
                var url = Url.Action(nameof(Logout), new {
                    logoutId = viewModel.LogoutId
                });
                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties {
                    RedirectUri = url
                }, viewModel.ExternalAuthenticationScheme);
            }
            // Set this so UI rendering sees an anonymous user.
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return View("LoggedOut", viewModel);
        }

        /// <summary>
        /// Displays the access denied page.
        /// </summary>
        [HttpGet("access-denied")]
        public IActionResult AccessDenied() => View();
    }
}

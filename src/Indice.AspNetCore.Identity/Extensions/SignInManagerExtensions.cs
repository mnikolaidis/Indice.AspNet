﻿using System;
using System.Linq;
using System.Security.Claims;
using Indice.AspNetCore.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Indice.AspNetCore.Identity.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="SignInManager{T}"/> type.
    /// </summary>
    public static class SignInManagerExtensions
    {
        /// <summary>
        /// Returns true if the principal has an identity with the specified cookie scheme.
        /// </summary>
        /// <param name="signInManager">Provides the APIs for user sign in.</param>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> instance.</param>
        /// <param name="authenticationScheme"></param>
        /// <returns>True if the user is logged in with specified identity and scheme.</returns>
        public static bool IsSignedInWithScheme<TUser>(this SignInManager<TUser> signInManager, ClaimsPrincipal principal, string authenticationScheme) where TUser : User {
            if (principal == null) {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal?.Identities != null && principal.Identities.Any(x => x.AuthenticationType == authenticationScheme); 
        }
    }
}

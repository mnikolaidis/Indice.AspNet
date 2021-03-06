﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace Indice.AspNetCore.Identity.Scopes
{
    /// <summary>
    /// A service that will provide metadata for a scope. 
    /// This is useful for dynamic scopes that may need dynamic names and descriptions.
    /// </summary>
    public interface IDynamicScopeMetadataService
    {
        /// <summary>
        /// Resolve metadata for the given dynamic scope. 
        /// Returns a static version of this scope populated with all necessary information 
        /// from the metadata endpoint.
        /// </summary>
        /// <param name="dynamicScope"></param>
        /// <param name="culture">Culture is used to translate literals</param>
        /// <returns>a static version (clone) of the input populated with all necessary information</returns>
        Task<DynamicScope> ResolveMetadata(DynamicScope dynamicScope, CultureInfo culture = null);
    }
}

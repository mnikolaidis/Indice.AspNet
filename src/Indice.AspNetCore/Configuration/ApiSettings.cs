﻿using System.Collections.Generic;

namespace Indice.Configuration
{
    /// <summary>
    /// Settings related to APIs.
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// Security wise the name of the api. Used for identifyinh the api as a resource in oAuth2 as well as the main scope.
        /// </summary>
        public string ResourceName { get; set; } = "api1";
        /// <summary>
        /// List of secrets. Usualy here is the apisecret used to communicate with IdSrv in order to exchange the reference tokens with actual info.
        /// </summary>
        public Dictionary<string, string> Secrets { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// The sub scopes avialable for the api.
        /// </summary>
        public Dictionary<string, string> Scopes { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Friendly name for the api
        /// </summary>
        public string FriendlyName { get; set; } = "My Api Name";
        /// <summary>
        /// The api verison number.
        /// </summary>
        public string DefaultVersion { get; set; } = "1";
        /// <summary>
        /// The api terms of service url.
        /// </summary>
        public string TermsOfServiceUrl { get; set; }
        /// <summary>
        /// The api licence url.
        /// </summary>
        public LegalDocument License { get; set; }
        /// <summary>
        /// Api contact info. (developer) Will appear usualy on the swagger documentation page.
        /// </summary>
        public ApiContact Contact { get; set; }

        /// <summary>
        /// Used to configure a legal document resource name and location
        /// </summary>
        public class LegalDocument {
            
            /// <summary>
            /// The name of the document
            /// </summary>
            public string Name { get; set; }
            
            /// <summary>
            /// The name of the document
            /// </summary>
            public string Url { get; set; }
        }

        /// <summary>
        /// Api contact info. (developer) Will appear usualy on the swagger documentation page.
        /// </summary>
        public class ApiContact {
            /// <summary>
            /// The name of the contact.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// A URL to the developer portal or github account/repo.
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// The contact email.
            /// </summary>
            public string Email { get; set; }
        }
    }
}

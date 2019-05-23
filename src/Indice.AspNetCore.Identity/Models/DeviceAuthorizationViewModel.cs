// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Indice.AspNetCore.Identity.Models
{
    /// <summary>
    /// Device authorization view model
    /// </summary>
    public class DeviceAuthorizationViewModel : ConsentViewModel
    {
        /// <summary>
        /// the user code
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// Should confirm the code or not.
        /// </summary>
        public bool ConfirmUserCode { get; set; }
    }
}
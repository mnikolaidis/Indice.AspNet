﻿using System;
using System.Threading.Tasks;
using Indice.AspNetCore.Identity.Features;
using Microsoft.Extensions.Logging;

namespace Indice.Identity.Services
{
    /// <summary>
    /// Handler for client created event raised by IdentityServer API.
    /// </summary>
    public class ClientCreatedEventHandler : IIdentityServerApiEventHandler<ClientCreatedEvent>
    {
        private readonly ILogger<ClientCreatedEventHandler> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ClientCreatedEventHandler(ILogger<ClientCreatedEventHandler> logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle client created event.
        /// </summary>
        /// <param name="event">Client created event info.</param>
        public Task Handle(ClientCreatedEvent @event) {
            _logger.LogDebug($"Client created: {@event.ToString()}");
            return Task.CompletedTask;
        }
    }
}

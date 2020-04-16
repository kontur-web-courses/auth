using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace PhotosApp.Services.TicketStores
{
    public class MemoryCacheTicketStore : ITicketStore
    {
        private const string KeyPrefix = "AuthSessionStore-";
        private readonly IMemoryCache cache;

        public MemoryCacheTicketStore()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var guid = Guid.NewGuid();
            var key = KeyPrefix + guid;
            await RenewAsync(key, ticket);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new MemoryCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                options.SetAbsoluteExpiration(expiresUtc.Value);
            }
            options.SetSlidingExpiration(TimeSpan.FromHours(1));

            cache.Set(key, ticket, options);

            return Task.FromResult(0);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            cache.TryGetValue(key, out AuthenticationTicket ticket);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            cache.Remove(key);
            return Task.FromResult(0);
        }
    }
}
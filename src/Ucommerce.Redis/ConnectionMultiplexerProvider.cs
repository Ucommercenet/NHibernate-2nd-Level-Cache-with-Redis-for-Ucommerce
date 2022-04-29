using NHibernate.Caches.StackExchangeRedis;
using StackExchange.Redis;

namespace Ucommerce.Redis
{
    /// <inheritdoc/>
    public class ConnectionMultiplexerProvider : IConnectionMultiplexerProvider
    {
        private static readonly object _lock = new object();
        
        /// <inheritdoc/>
        public IConnectionMultiplexer Get(string configuration)
        {
            lock (_lock)
            {
                return ConnectionMultiplexer.Connect(configuration);
            }
        }
    }
}

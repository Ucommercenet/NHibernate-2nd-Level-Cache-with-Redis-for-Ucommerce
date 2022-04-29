using System;
using NHibernate;

namespace Ucommerce.Redis.Logging
{
    /// <summary>
    /// Instantiate a <see cref="UcommerceNHibernateLogger"/>, when NHibernate ask for it
    /// </summary>
    public class UcommerceNHibernateLoggerFactory : INHibernateLoggerFactory
    {
        /// <inheritdoc />
        public INHibernateLogger LoggerFor(string keyName)
        {
            return new UcommerceNHibernateLogger();
        }

        /// <inheritdoc />
        public INHibernateLogger LoggerFor(Type type)
        {
            return new UcommerceNHibernateLogger();
        }
    }
}

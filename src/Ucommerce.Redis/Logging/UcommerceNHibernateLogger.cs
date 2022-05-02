using System;
using NHibernate;
using Ucommerce.Infrastructure;
using Ucommerce.Infrastructure.Logging;

namespace Ucommerce.Redis.Logging
{
    /// <summary>
    /// <see cref="UcommerceNHibernateLogger"/> writes logs from NHibernate to the default Ucommerce log
    /// </summary>
    public class UcommerceNHibernateLogger : INHibernateLogger
    {
        private readonly ILoggingService _logger;

        /// <summary>
        /// Default constructor used to resolve <see cref="ILoggingService"/>
        /// </summary>
        public UcommerceNHibernateLogger()
        {
            _logger = ObjectFactory.Instance.Resolve<ILoggingService>();
        }

        /// <inheritdoc />
        public bool IsEnabled(NHibernateLogLevel logLevel)
        {
            switch (logLevel)
            {
                case NHibernateLogLevel.Trace:
                    return false;
                case NHibernateLogLevel.Debug:
                    return false;
                case NHibernateLogLevel.Info:
                    return true;
                case NHibernateLogLevel.Warn:
                    return true;
                case NHibernateLogLevel.Error:
                    return true;
                case NHibernateLogLevel.Fatal:
                    return false;
                case NHibernateLogLevel.None:
                    return false;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception exception)
        {
            switch (logLevel)
            {
                case NHibernateLogLevel.Info:
                    _logger.Information<UcommerceNHibernateLogger>(state.Format, state.Args);
                    break;
                case NHibernateLogLevel.Warn:
                    _logger.Information<UcommerceNHibernateLogger>(state.Format, state.Args);
                    break;
                case NHibernateLogLevel.Error:
                    _logger.Error<UcommerceNHibernateLogger>(exception, state.Format, state.Args);
                    break;
            }
        }
    }
}

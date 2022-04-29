// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using NHibernate;
// using Ucommerce.Infrastructure;
// using Ucommerce.Infrastructure.Logging;
//
// namespace RedisForUcommerce.Logger
// {
//     public class LoggerFactory : ILoggerFactory
//     {
//         public IInternalLogger LoggerFor(string keyName)
//         {
//             return new UCommerceNHibernateLogger();
//         }
//
//         public IInternalLogger LoggerFor(Type type)
//         {
//             return new UCommerceNHibernateLogger();
//         }
//     }
//
//     public class UCommerceNHibernateLogger : IInternalLogger
//     {
//         private bool _onlyLogCacheHits = true;
//         protected ILoggingService LoggingService
//         {
//             get
//             {
//                 return ObjectFactory.Instance.Resolve<ILoggingService>();
//
//             }
//         }
//
//         public void Error(object message)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"ERROR: { message.ToString() }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Error(object message, Exception exception)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"ERROR: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(exception);
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void ErrorFormat(string format, params object[] args)
//         {
//             if (ShouldBreak(format.ToString())) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"ErrorFormat {string.Format(format, args) }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//
//         }
//
//         public void Fatal(object message)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Fatal: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Fatal(object message, Exception exception)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Fatal: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(exception);
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Debug(object message)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Debug: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Debug(object message, Exception exception)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Debug: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(exception);
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         private bool ShouldBreak(string message)
//         {
//             string messageInternal = message.ToLower();
//             return _onlyLogCacheHits &&
//                    !((messageInternal.StartsWith("get from cache") || messageInternal.StartsWith("cache miss")));
//         }
//
//         public void DebugFormat(string format, params object[] args)
//         {
//             if (ShouldBreak(format)) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"DebugFormat {string.Format(format, args) }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Info(object message)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Info: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Info(object message, Exception exception)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Info: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(exception);
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void InfoFormat(string format, params object[] args)
//         {
//             if (ShouldBreak(format.ToString())) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"InfoFormat {string.Format(format, args) }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//
//         }
//
//         public void Warn(object message)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>($"Warn: { message.ToString() }");
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void Warn(object message, Exception exception)
//         {
//             if (ShouldBreak(message.ToString())) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"Warn {message.ToString() }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public void WarnFormat(string format, params object[] args)
//         {
//             if (ShouldBreak(format.ToString())) return;
//
//             LoggingService.Log<UCommerceNHibernateLogger>($"DebugFormat {string.Format(format, args) }");
//             var stackTrace = Environment.StackTrace;
//             LoggingService.Log<UCommerceNHibernateLogger>(stackTrace);
//         }
//
//         public bool IsErrorEnabled { get; }
//         public bool IsFatalEnabled { get; }
//         public bool IsDebugEnabled { get; }
//         public bool IsInfoEnabled { get; }
//         public bool IsWarnEnabled { get; }
//     }
// }

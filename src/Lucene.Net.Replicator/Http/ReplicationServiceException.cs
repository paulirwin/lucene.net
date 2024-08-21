using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Lucene.Net.Replicator.Http
{
    /// <summary>
    /// An exception thrown by <see cref="ReplicationService"/> for serialization in the response.
    /// </summary>
    /// <remarks>
    /// This exception intentionally does not include the original exception as an inner exception
    /// as this is unable to be deserialized by the client. Instead, the original exception's message
    /// and stack trace are included in the response. The stack trace is included in the OriginalStackTrace
    /// property because the base StackTrace property is read-only.
    /// </remarks>
    [Serializable]
    public sealed class ReplicationServiceException : Exception
    {
        /// <summary>
        /// Creates a new exception from the original exception.
        /// </summary>
        /// <param name="originalException">The original exception that caused this exception.</param>
        public ReplicationServiceException(Exception originalException)
            : base(originalException.Message)
        {
            OriginalStackTrace = originalException.StackTrace;
            OriginalExceptionType = originalException.GetType().FullName;
        }

        public ReplicationServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OriginalExceptionType = info.GetString(nameof(OriginalExceptionType));
            OriginalStackTrace = info.GetString(nameof(OriginalStackTrace));
        }

        /// <summary>
        /// The full type name of the original exception.
        /// </summary>
        public string OriginalExceptionType { get; set; }

        /// <summary>
        /// The stack trace of the original exception.
        /// </summary>
        public string OriginalStackTrace { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(OriginalExceptionType), OriginalExceptionType);
            info.AddValue(nameof(OriginalStackTrace), OriginalStackTrace);
        }
    }
}

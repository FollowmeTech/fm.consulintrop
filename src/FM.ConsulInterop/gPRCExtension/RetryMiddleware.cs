using Grpc.Core;

namespace FM.ConsulInterop
{
    /// <summary>
    /// RetryMiddleware
    /// </summary>
    /// <seealso cref="FM.ConsulInterop.IClientCallAction" />
    public class RetryMiddleware : IClientCallAction
    {
        private const string RETRY_TIMES = "grpc-retry";
        public int Times { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryMiddleware"/> class.
        /// </summary>
        /// <param name="times">times重试次数</param>
        public RetryMiddleware(int times)
        {
            this.Times = times;
        }

        public void PostAction<TResponse>(TResponse response)
        {
            //bypass
        }

        public CallOptions PreAction<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(RETRY_TIMES, $"{this.Times}");
            return options;
        }
    }
}
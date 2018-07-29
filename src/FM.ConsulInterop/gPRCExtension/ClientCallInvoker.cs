using Grpc.Core;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FM.ConsulInterop
{
    internal sealed class ClientCallInvoker : CallInvoker
    {
        private Channel grpcChannel;
        private ClientCallActionCollection _callActionCollection;

        public ClientCallInvoker(Channel channel)
        {
            this.grpcChannel = channel;
        }

        public ClientCallInvoker(Channel channel, ClientCallActionCollection clientCallActionCollection) : this(channel)
        {
            this._callActionCollection = clientCallActionCollection;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
            string host, CallOptions options, TRequest request)
        {
            ServerCallInvoker callInvoker = new ServerCallInvoker(grpcChannel);
            TResponse response = default(TResponse);

            try
            {
                _callActionCollection?.ForEach(callAction =>
                {
                    InnerLogger.Log(LoggerLevel.Debug, "pre:" + callAction.GetType().Name);
                    options = (CallOptions)callAction?.PreAction(method, host, options, request);
                    InnerLogger.Log(LoggerLevel.Debug, "end :" + callAction.GetType().Name);
                });

                var times = 0;
                if (options.Headers != null)
                {
                    var retry = options.Headers.FirstOrDefault(t => t.Key == "grpc-retry");
                    if (!string.IsNullOrWhiteSpace(retry?.Value))
                    {
                        times = int.Parse(retry?.Value);
                    }
                }
                Policy
                .Handle<Exception>()
                .Retry(times, (exception, retryCount) =>
                {
                    response = callInvoker.BlockingUnaryCall(method, host, options, request);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _callActionCollection?.ForEach(callAction =>
                {
                    InnerLogger.Log(LoggerLevel.Debug, "post:" + callAction.GetType().Name);
                    callAction?.PostAction(response);
                    InnerLogger.Log(LoggerLevel.Debug, "end:" + callAction.GetType().Name);
                });
            }

            return response;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            ServerCallInvoker callInvoker = new ServerCallInvoker(grpcChannel);
            var response = default(AsyncUnaryCall<TResponse>);

            try
            {
                _callActionCollection?.ForEach(callAction =>
                {
                    InnerLogger.Log(LoggerLevel.Debug, "pre:" + callAction.GetType().Name);
                    options = (CallOptions)callAction?.PreAction(method, host, options, request);
                    InnerLogger.Log(LoggerLevel.Debug, "end :" + callAction.GetType().Name);
                });
                var times = 0;
                if (options.Headers != null)
                {
                    var retry = options.Headers.FirstOrDefault(t => t.Key == "grpc-retry");
                    if (!string.IsNullOrWhiteSpace(retry?.Value))
                    {
                        times = int.Parse(retry?.Value);
                    }
                }
                Policy
                .Handle<Exception>()
                .Retry(times, (exception, retryCount) =>
                {
                    response = callInvoker.AsyncUnaryCall(method, host, (CallOptions)options, request);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _callActionCollection?.ForEach(callAction =>
                {
                    InnerLogger.Log(LoggerLevel.Debug, "post:" + callAction.GetType().Name);
                    callAction?.PostAction(response);
                    InnerLogger.Log(LoggerLevel.Debug, "end:" + callAction.GetType().Name);
                });
            }

            return response;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options,
            TRequest request)
        {
            throw new NotImplementedException("FM.ConsulInterop 未实现");
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new NotImplementedException("FM.ConsulInterop 未实现");
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            throw new NotImplementedException("FM.ConsulInterop 未实现");
        }
    }
}
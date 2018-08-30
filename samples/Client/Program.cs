using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using FM.ConsulInterop;
using FM.ConsulInterop.Config;
using System.Threading;

namespace Client
{
    internal class Program
    {

        private static ConsulRemoteServiceConfig config4RemoteService;
        private static void Main(string[] args)
        {
            //查看内部细节
            InnerLogger.ConsulLog += c => Console.WriteLine(c.Content);

            //load config
            var conf = new ConfigurationBuilder().SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)).AddJsonFile("appsetting.json", false, false).Build();
            config4RemoteService = conf.GetSection("consul:remotes:demo").Get<ConsulRemoteServiceConfig>();

            Demo4Middleware();
        }

        static void Demo4Middleware()
        {

            var clientWithClientMiddleware =
                    new ClientAgent<FM.Demo.HelloSrv.HelloSrvClient>(config4RemoteService,
                     new ClientAgentOption().AddLoggerMiddleWare().AddRetryMiddleWare(3).AddTimeOutMiddleWare(10 * 1000));

            try
            {
                while (true)
                {
                    clientWithClientMiddleware.Proxy.Hi(new FM.Demo.HiRequest());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void Demo4Raw()
        {
            var rawClient =
            new ClientAgent<FM.Demo.HelloSrv.HelloSrvClient>(config4RemoteService);
            try
            {
                while (true)
                {
                    rawClient.Proxy.Hi(new FM.Demo.HiRequest());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    static class ClientExtension
    {
        public static ClientAgentOption AddLoggerMiddleWare(this ClientAgentOption opt)
        {
            opt.ClientCallActionCollection.Add(new LoggerClientCallAction());
            return opt;
        }
        public static ClientAgentOption AddRetryMiddleWare(this ClientAgentOption opt, int times)
        {
            opt.ClientCallActionCollection.Add(new RetryMiddleware(times));
            return opt;
        }

        public static ClientAgentOption AddTimeOutMiddleWare(this ClientAgentOption opt, int timeout)
        {
            opt.ClientCallActionCollection.Add(new InvokeTimeoutMiddleware(timeout));
            return opt;
        }
    }
}
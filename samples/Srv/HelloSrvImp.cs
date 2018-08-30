using System;
using System.Threading.Tasks;
using FM.Demo;
using Grpc.Core;

namespace Srv
{
    class HelloSrvImp : FM.Demo.HelloSrv.HelloSrvBase
    {
        static int whenErrorRaise = 3;
        static int requestTimes = 0;

        public override async Task<HiResponse> Hi(HiRequest request, ServerCallContext context)
        {
            requestTimes++;
            if (requestTimes % whenErrorRaise == 0)
                throw new Grpc.Core.RpcException(new Status(StatusCode.Unavailable, "自定义错误"));

            //mock api delay 
            await Task.Delay(new Random().Next(10, 12 * 1000)).ConfigureAwait(false);
            return new HiResponse { };
        }
    }
}

using ServiceDefine;
using SimpleRPC;

namespace ConsoleServer
{
    [RpcService(nameof(ITestService))]
    public class TestService : ITestService
    {
        public int Add(MathAddParam param)
        {
            return param.X + param.Y; 
        }

        public void DoNothing()
        {
            Console.WriteLine("DoNothing");
        }

        public string GetServiceName()
        {
            return nameof(ITestService);
        }
    }
}

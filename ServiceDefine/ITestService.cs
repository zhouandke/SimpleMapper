using  SimpleRPC;

namespace ServiceDefine
{
    public interface ITestService
    {
        int Add(MathAddParam param);

        string? GetServiceName();

        void DoNothing();
    }

    public class MathAddParam
    {
        public int X { get; set; }

        public int Y { get; set; }
    }
}

using SimpleRPC;

namespace ConsoleServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost();
            var msg = host.Init(6000, "ConsoleServer.dll");
            if (msg != null)
            {
                Console.WriteLine(msg);
                return;
            }

            Console.WriteLine("Server started");
            Console.ReadLine();
        }
    }
}

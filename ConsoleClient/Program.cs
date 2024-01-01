namespace ConsoleClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            Console.WriteLine("Hello, World!");
            TestClient mathClient = new();
            mathClient.Init("127.0.0.1", 6000);

            try
            {
                var sum = mathClient.Add(new ServiceDefine.MathAddParam() { X = 1, Y = 2 });
                Console.WriteLine(sum);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                var sum = mathClient.AddAsync(new ServiceDefine.MathAddParam() { X = 1, Y = 2 }).Result;
                Console.WriteLine(sum);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            try
            {
                var serviceName = mathClient.GetServiceName();
                Console.WriteLine(serviceName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                mathClient.DoNothing();
                Console.WriteLine("DoNothing finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }


    }
}

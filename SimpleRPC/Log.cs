using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRPC
{
    internal class Log
    {
        public static void Debug(string log)
        {
            Console.WriteLine($"{DateTime.Now} Debug: {log}");
        }

        public static void Info(string log)
        {
            Console.WriteLine($"{DateTime.Now} Info: {log}");
        }

        public static void Warn(string log)
        {
            Console.WriteLine($"{DateTime.Now} Warn: {log}");
        }

        public static void Error(string log)
        {
            Console.WriteLine($"{DateTime.Now} Error: {log}");
        }
    }
}

using GzipConsole.Model;
using System;
using System.Threading;

namespace GzipConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            GzipOperator.Starter.Configure(args, new ConsoleReporter());

            GzipOperator.Starter.Service.InputProgress += Service_InputProgress;
            GzipOperator.Starter.Service.OutputProgress += Service_OutputProgress;
            GzipOperator.Starter.Service.ProcessingProgress += Service_ProcessProgress;

           GzipOperator.Starter.Start();

        }

        private static void Service_InputProgress (double i)
        {
            Console.WriteLine($"Прогресс чтения файла {i}");
        }

        private static void Service_OutputProgress (double i)
        {
            Console.WriteLine($"Прогресс записи файла {i}");
        }

        private static void Service_ProcessProgress (double i)
        {
            Console.WriteLine($"Прогресс обработки файла {i}");
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static void MainThread()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Hello, Thread!");
            }
        }
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(10, 10);

            for (int i = 0; i < 10; i++)
            {
                Task t = new Task(() => { Console.WriteLine("A"); });
                t.Start();
            }



            // for (int i = 0; i < 10; i++)
            // {
            //     // Console.WriteLine($"{i}");
            //     ThreadPool.QueueUserWorkItem(MainThread);
            // }


            // for (int i = 0; i < 1000; i++)
            // {
            //     Thread t = new Thread(MainThread);
            //     t.Name = $"Test Thread{i}";
            //     t.IsBackground = true;

            //     Console.WriteLine($"Thread{i}: start");
            //     t.Start();
            //     Console.WriteLine($"Thread{i}: Done");
            // }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(
            () =>
            {
                return $"My Name is {Thread.CurrentThread.ManagedThreadId}";
            }
        );

        static void WhoAmI()
        {
            // 스레드 개인 영역에서만 ThreadName이 변경된 상태이다
            Thread.Sleep(10);

            Console.WriteLine(ThreadName.Value);
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(15, 15);

            Parallel.Invoke(
                WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI,
                WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI,
                WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI
            );
        }
    }
}
using System;
using System.Threading;
using System.Text;

namespace ServerCore
{
    // 재귀적 락 허용 X(동일한 스레드에서 이 Lock에 다시 Acquire할 수 없도록 함.)
    // SpinLock 정책 : 5000번 Spin 후 Yield
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN = 5000;

        //[Unused(1)] [WriteThreadID(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;

        public void WriteLock()
        {
            // 아무도 Write, Read주이 아닐 때 Lock의 소유권을 얻는다.

            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;

            while (true)
            {
                for (int i = 0; i < MAX_SPIN; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                        return;

                }
                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 아무도 WriteLock을 획득하고 있지 않으면 ReadCount를 1 늘린다. ReadLock은 여러 스레드가 동시에 점유할 수 있다.

            while (true)
            {
                for (int i = 0; i < MAX_SPIN; i++)
                {
                    int expected = (_flag & READ_MASK);

                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
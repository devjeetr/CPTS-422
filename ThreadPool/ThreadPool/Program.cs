using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreadPool
{
    class Program
    {
        static void Main(string[] args)
        {
            MyThreadPool tp = new MyThreadPool();

            for (int i = 0; i < 1000; i++)
            {
                tp.QueueWork(i, new WorkDelegate(PerformWork));
            }

            Console.ReadLine();
            tp.Shutdown();
        }

        static private void PerformWork(object o)
        {
            int i = (int)o;

            Console.WriteLine("Work Performed: " + i.ToString());
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("End Work Performed: " + i.ToString());
        }
    }
}

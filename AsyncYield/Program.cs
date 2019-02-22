using System;
using System.Collections.Generic;

namespace AsyncYield
{
    internal class Program
    {
        public static async void MyIteratorMethod1(YieldEnumerator<int> e)
        {
            Console.WriteLine("A");
            await e.YieldReturn(1);
            Console.WriteLine("B");
            await e.YieldReturn(2);
            Console.WriteLine("C");
            await e.YieldReturn(3);
            Console.WriteLine("D");
        }

        public static async void MyIteratorMethod2(YieldEnumerator<int> e)
        {
            try
            {
                Console.WriteLine("A");
                await e.YieldReturn(1);
                Console.WriteLine("B");
                await e.YieldReturn(2);
                Console.WriteLine("C");
                await e.YieldReturn(3);
                Console.WriteLine("D");
            }
            finally
            {
                Console.WriteLine("Running finally");
            }
        }

        public static async void MyIteratorMethod3(YieldEnumerator<int, string> e)
        {
            Console.WriteLine("A");
            Console.WriteLine(await e.YieldReturn(1));
            Console.WriteLine("B");
            Console.WriteLine(await e.YieldReturn(2));
            Console.WriteLine("C");
            Console.WriteLine(await e.YieldReturn(3));
            Console.WriteLine("D");
        }

        private static void Main(string[] args)
        {
            foreach (var i in new YieldEnumerable<int>(MyIteratorMethod1))
                Console.WriteLine("Yielded: " + i);

            foreach (var i in new YieldEnumerable<int>(MyIteratorMethod2))
            {
                Console.WriteLine("Yielded: " + i);
            }


            Console.ReadLine();
        }
    }
}

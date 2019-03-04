using System;
using System.Collections.Generic;
using System.IO;

// Based on https://smellegantcode.wordpress.com/2010/12/14/unification-of-async-await-and-yield-return/
namespace AsyncYield
{
    internal class Program
    {
        public static async void MyIteratorMethod1(YieldEnumerator<int, int> e)
        {
            Console.WriteLine("A");
            await e.YieldReturn(1);
            Console.WriteLine("B");
            await e.YieldReturn(2);
            Console.WriteLine("C");
            await e.YieldReturn(3);
            Console.WriteLine("D");
        }

        public static async void MyIteratorMethod2(YieldEnumerator<int, int> e)
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
        
        public static void ForEach<TIn, TOut>(
            Action<YieldEnumerator<TIn, TOut>> iteratorMethod,
            Action<TIn, Action<TOut>> forEach)
        {
            IEnumerator<TIn> enumerator = null;
 
            try
            {
                enumerator = new YieldEnumerable<TIn, TOut>(iteratorMethod).GetEnumerator();
                var enumerator2 = (YieldEnumerator<TIn, TOut>) enumerator;
                while (enumerator.MoveNext())
                {
                    try
                    {
                        forEach(enumerator2.Current, enumerator2.Return);
                    }
                    catch (Exception x) // Oh the humanity!
                    {
                        enumerator2.Throw(x);
                    }
                }
            }
            finally
            {   
                enumerator?.Dispose();
            }
        }

        public static async void MyIteratorMethod4(YieldEnumerator<int, string> e)
        {
            try
            {
                Console.WriteLine("A");
                Console.WriteLine(await e.YieldReturn(1));
                Console.WriteLine("B");
                Console.WriteLine(await e.YieldReturn(2));
                Console.WriteLine("C");
                Console.WriteLine(await e.YieldReturn(3));
                Console.WriteLine("D");
            }
            catch (IOException x)
            {
                Console.WriteLine(x.Message);
            }
        }

        private static void Main(string[] args)
        {
            foreach (var i in new YieldEnumerable<int,int>(MyIteratorMethod1))
                Console.WriteLine("Yielded 1: " + i);

            foreach (var i in new YieldEnumerable<int,int>(MyIteratorMethod2))
                Console.WriteLine("Yielded 2: " + i);

            var counter = 100;
            ForEach(MyIteratorMethod3, (int i, Action<string> loopReturn) =>
            {
                Console.WriteLine("Yielded: " + i);
                loopReturn("Counter: " + (counter++));
            });

            var counter2 = 100;
            ForEach(MyIteratorMethod4, (int i, Action<string> loopReturn) =>
            {
                Console.WriteLine("Yielded: " + i);
                loopReturn("Counter: " + (counter2++));
 
                if (counter2 == 102)
                    throw new IOException("Catastrophic disk failure");
            });

            Console.ReadLine();
        }
    }
}

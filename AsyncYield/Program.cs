using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Based on https://smellegantcode.wordpress.com/2010/12/14/unification-of-async-await-and-yield-return/

// updated code to support Task following https://smellegantcode.wordpress.com/2012/01/29/asyncawait-iterator-updated-for-visual-studio-11-preview/
namespace AsyncYield
{
    internal class Program
    {
        public static async Task MyIteratorMethod1(YieldEnumerator<int, Unit> e)
        {
            Console.WriteLine("A");
            await e.YieldReturn(1);
            Console.WriteLine("B");
            await e.YieldReturn(2);
            Console.WriteLine("C");
            await e.YieldReturn(3);
            Console.WriteLine("D");
        }

        public static async Task MyIteratorMethod2(YieldEnumerator<int, Unit> e)
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

        public static async Task MyIteratorMethod3(YieldEnumerator<int, string> e)
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
            IteratorMethod<TIn, TOut> iteratorMethod,
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

        public static async Task MyIteratorMethod4(YieldEnumerator<int, string> e)
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

        public static async Task MyIteratorMethodInfinite(YieldEnumerator<int,Unit> e)
        {
            for (var n = 0; ; n++)
                await e.YieldReturn(n);
        }
 
        public static async Task MyIteratorBroken1(YieldEnumerator<int,Unit> e)
        {
            // always happens, but compiler doesn't know that
            if (DateTime.Now.Year < 10000)
                throw new IOException("Bad");
 
            await e.YieldReturn(1);
        }
 
        public static async Task MyIteratorBroken2(YieldEnumerator<int,Unit> e)
        {
            await e.YieldReturn(1);
 
            if (DateTime.Now.Year < 10000)
                throw new IOException("Bad");
        }
 
        public static async Task MyIteratorBroken3(YieldEnumerator<int,Unit> e)
        {
            await e.YieldReturn(1);
 
            if (DateTime.Now.Year < 10000)
                throw new IOException("Bad");
 
            await e.YieldReturn(2);
        }

        private static void Main(string[] args)
        {
            foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorMethod1))
                Console.WriteLine("Yielded 1: " + i);

            foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorMethod2))
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

            //************** from update

            foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorMethod1))
                Console.WriteLine("Yielded: " + i);
 
            foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorMethod2))
            {
                Console.WriteLine("Yielded: " + i);
                break; // finally should still run
            }
 
            foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorMethodInfinite))
            {
                if (i % 1000000 == 0) // every million times...
                    Console.WriteLine("Yielded: " + i);
 
                if (i > 10000000)
                    break;
            }
 
            try
            {
                foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorBroken1))
                    Console.WriteLine("Yielded: " + i);
            }
            catch (IOException)
            {
                Console.WriteLine("Caught expected exception");
            }
 
            try
            {
                foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorBroken2))
                    Console.WriteLine("Yielded: " + i);
            }
            catch (IOException)
            {
                Console.WriteLine("Caught expected exception");
            }
 
            try
            {
                foreach (var i in new YieldEnumerable<int,Unit>(MyIteratorBroken3))
                    Console.WriteLine("Yielded: " + i);
            }
            catch (IOException)
            {
                Console.WriteLine("Caught expected exception");
            }

            Console.ReadLine();
        }
    }
}

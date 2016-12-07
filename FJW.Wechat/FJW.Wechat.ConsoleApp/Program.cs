using System;
using FJW.Unit;


namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("a".ToInt());
           // new T1().Test();
            DateTime t1;
            DateTime.TryParse("a", out t1);
            Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss}, IsMinValue:{1} ", t1, DateTime.MinValue == t1);
            Console.WriteLine("OVER");
            Console.ReadLine();
        }
    }
}
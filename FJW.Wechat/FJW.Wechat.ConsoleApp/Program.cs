using System;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Models;


namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var respose = new ResponseModel();
            Console.WriteLine(respose.ToJson());
            Console.WriteLine(DateTime.Now.Ticks.ToString("N.jpg"));
            Console.ReadLine();
        }
    }
}
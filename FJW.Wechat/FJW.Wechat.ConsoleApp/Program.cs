using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp;
using FJW.Wechat.WebApp.Controllers;
using FJW.Wechat.WebApp.Models;

using Newtonsoft.Json;


namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RedisManager.Init(Config.RedisConfig.ConnectionString);
            Console.WriteLine(Config.RedisConfig.ConnectionString);

            try
            {
                var q = RedisManager.GetIncrement("Increment:luckcoupon");
                while (q < 970)
                {
                    q = RedisManager.GetIncrement("Increment:luckcoupon");
                    Console.WriteLine(q);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("OVER");
            Console.ReadLine();
        }
    }
}
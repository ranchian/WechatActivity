using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp;
using FJW.Wechat.WebApp.Areas.Activity.Controllers;
using FJW.Wechat.WebApp.Controllers;
using FJW.Wechat.WebApp.Models;

using Newtonsoft.Json;


namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new MineSweeperConfig();
            config.A = 10012;
            config.E = 5;
            config.ExpresssProductId = 2;
            config.CouponActivityId = 10015;// QuotaActivityConfig.Id
            config.H = 10108;
            config.I = 10109;
            config.J = 10110;
            config.K = 10111;
            config.L = 10112;
            config.M = 10113;
            config.N = 10114;
            config.O = 10115;
            config.P = 10116;
            config.Q = 10117;
            var d = new ActivityModel
            {
                ID = Guid.NewGuid(),
                Config = config.ToJson(),
                Key = "minesweeper"
            };

            var repository = new ActivityRepository( Config.ActivityConfig.DbName, Config.ActivityConfig.MongoHost);
            var act = repository.GetActivity("minesweeper");
            if (act != null)
            {
                Console.WriteLine("exists");
                Console.WriteLine(act.ID);
            }
            else
            {
                repository.Add(d);
            }
            


            Console.WriteLine("OVER");
            Console.ReadLine();
        }
    }
}
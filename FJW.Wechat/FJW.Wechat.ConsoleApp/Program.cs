using System;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.CommonLib.Configuration;

namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int prize;
            decimal money;
            string name;

            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("主线程开始执行,时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("--------------------------------------------------------------------");

            /*
             * 使用Task.Factory.StartNew进行异步方法操作
             */

            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i <= 400; i++)
                {
                    var cnt = Luckdraw(out prize, out money, out name);
                    Console.WriteLine(string.Format("异步循环显示序号：{0},时间:{1},计数器:{2},奖品:{3},金额:{4},奖品名称:{5}", i.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), cnt.ToString(), prize.ToString(), money.ToString(), name.ToString()));
                }
            });

            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("主线程执行结束,时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("--------------------------------------------------------------------");
            Console.ReadKey();

            return;

            string _mongoHost;
            string _dbName;
            string _sqlConnectString;

            try
            {
                _mongoHost = ConfigurationManager.AppSettings["MongoHost"];
                _dbName = ConfigurationManager.AppSettings["DbName"];
                _sqlConnectString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

                var repository = new ActivityRepository(_dbName, _mongoHost);
                var mberRepository = new MemberRepository(_sqlConnectString);
                var list = repository.Query<ActivityModel>(it => it.Key == "pm25").ToArray();
                if (list.Length > 0)
                {
                    Console.WriteLine(list.Length);
                    Console.WriteLine(list[0].Key);
                    Console.WriteLine(list[0].StartTime);
                    Console.WriteLine(list[0].EndTime);
                    var acty = list[0];
                    var records = repository.Query<RecordModel>(it => it.Status == 1 && it.Result == -1).ToArray();
                    //foreach (var it in records)
                    //{
                    //   var r = mberRepository.Give(it.MemberId, acty.RewardId, acty.ProductId, it.Score, 0);
                    //    Console.WriteLine("MemberId:{0}, Score:{1}", it.MemberId, it.Score);
                    //    it.Result = r;
                    //    it.LastUpdateTime = DateTime.Now;
                    //    repository.Update(it);
                    //}
                }

                Console.WriteLine("OVER");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.ReadLine();
        }

        private const string GameKey = "turntable";

        private static long Luckdraw(out int prize, out decimal money, out string name)
        {
            //prize = 0;
            money = 0;

            RedisManager.Init(ConfigManager.GetWebConfig("RedisConnection", ""));
            var l = RedisManager.GetIncrement("Increment:" + GameKey);
            if (l % 400 == 0)
            {
                prize = 1;
                name = "iphone 7 plus(128G)";
                return l;
            }
            if (l % 200 == 0)
            {
                prize = 2;
                name = "apple watch2";
                return l;
            }
            if (l % 60 == 0)
            {
                prize = 3;
                money = 800;
                name = "800元现金";
                return l;
            }
            if (l % 10 == 0)
            {
                prize = 4;
                money = 80;
                name = "80元现金";
                return l;
            }
            if (l % 5 == 0)
            {
                prize = 5;
                money = 10;
                name = "10元现金";
                return l;
            }
            prize = 6;
            money = 5;
            name = "5元现金";
            return l;
        }
    }
}
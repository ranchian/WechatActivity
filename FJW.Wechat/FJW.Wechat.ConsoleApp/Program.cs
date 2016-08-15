using FJW.Wechat.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
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
    }
}

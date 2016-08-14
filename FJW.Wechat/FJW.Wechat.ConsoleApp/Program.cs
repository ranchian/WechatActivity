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

                var list = repository.Query<ActivityModel>(it => it.Key == "pm25").ToArray();
                if (list.Length > 0)
                {
                    Console.WriteLine(list.Length);
                    Console.WriteLine(list[0].Key);
                    Console.WriteLine(list[0].StartTime);
                    Console.WriteLine(list[0].EndTime);
                }
                else
                {
                    Console.WriteLine("Length：0");
                    var m = new ActivityModel
                    {
                        Key = "pm25",
                        RewardId = 0,
                        RewardType = 1,
                        GameUrl = "/html/pm25/index.html",
                        StartTime = DateTime.Now.AddDays(1).Date,
                        EndTime = DateTime.Now.AddDays(8).Date,
                        MaxValue = 6000
                    };
                    repository.Add(m);
                    Console.WriteLine("ADD");
                }
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

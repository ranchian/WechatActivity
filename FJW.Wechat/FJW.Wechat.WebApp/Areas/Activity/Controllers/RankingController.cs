using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.Data;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class RankingController : ActivityController
    {

        [OutputCache(Duration = 2)]
        public ActionResult Current()
        {

            var repository = new SqlDataRepository(SqlConnectString);
            var rows = repository.TodayRanking().ToArray();
            foreach (var it in rows)
            {
                if (it.Sequnce == 0)
                {
                    it.Sequnce = 1;
                }
                it.Phone = StringHelper.CoverPhone(it.Phone);
            }
            var t = repository.GetDate().ToString("yyyy-MM-dd HH:mm:ss");
            return Json(new {t, rows});

        }

        public ActionResult Time()
        {
            var repository = new SqlDataRepository(SqlConnectString);
            var t = repository.GetDate().ToString("yyyy-MM-dd HH:mm:ss.fff");
            return Json(new { t});
        }

        [OutputCache(Duration = 5)]
        public ActionResult Today()
        {
            const string key = "Ranking:Today";
            var rows = RedisManager.Get<RankingRow[]>(key);
            if (rows == null)
            {
                if (DateTime.Now > new DateTime(2016,10,27))
                {
                    rows = new RankingRow[4];
                    rows[0] = new RankingRow { Title = "房金月宝"  , Phone = ""};
                    rows[1] = new RankingRow { Title = "房金季宝", Phone = "" };
                    rows[2] = new RankingRow { Title = "房金双季宝", Phone = "" };
                    rows[3] = new RankingRow { Title = "房金年宝", Phone = "" };
                }
                else
                {
                    var repository = new SqlDataRepository(SqlConnectString);
                    rows = repository.TodayRanking().ToArray();
                    foreach (var it in rows)
                    {
                        if (it.Sequnce == 0)
                        {
                            it.Sequnce = 1;
                        }
                        it.Phone = StringHelper.CoverPhone(it.Phone);
                    }
                }
                
                RedisManager.Set(key, rows, 30);
            }

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 10)]
        public ActionResult Total()
        {
            const string key = "Ranking:Total";
            var rows = RedisManager.Get<List<RankingRow>>(key);
            if (rows == null)
            {
                if (DateTime.Now > new DateTime(2016, 10, 27))
                {
                    rows = new List<RankingRow>(4);
                    rows[0] = new RankingRow { Title = "房金月宝", Phone = "" };
                    rows[1] = new RankingRow { Title = "房金季宝", Phone = "" };
                    rows[2] = new RankingRow { Title = "房金双季宝", Phone = "" };
                    rows[3] = new RankingRow { Title = "房金年宝", Phone = "" };
                }
                else
                {
                    var repository = new SqlDataRepository(SqlConnectString);
                    rows = repository.TotalRanking().ToList();
                    AppendSequnce(rows, 6, "房金季宝");
                    AppendSequnce(rows, 7, "房金双季宝");
                    AppendSequnce(rows, 8, "房金年宝");
                    foreach (var it in rows)
                    {
                        if (it.Sequnce == 0)
                        {
                            it.Sequnce = 1;
                        }
                        it.Phone = StringHelper.CoverPhone(it.Phone);
                    }
                    rows = rows.OrderByDescending(it => it.Id).ThenBy(it => it.Sequnce).ToList();
                }
               
                RedisManager.Set(key, rows, 60);
            }
            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        public static void AppendSequnce(List<RankingRow> rows, long id, string title)
        {
            var cnt = rows.Count(it => it.Id == id);
            if (cnt < 3)
            {
                rows.Add(new RankingRow { Id = id, Sequnce = cnt + 1, Title = title });
                AppendSequnce(rows, id, title);
            }

        }

    }
}
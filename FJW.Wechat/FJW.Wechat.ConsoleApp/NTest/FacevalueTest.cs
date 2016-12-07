using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.Controllers;
using FJW.Wechat.Cache;
using NUnit.Framework;

namespace FJW.Wechat.ConsoleApp.NTest
{
    public class FacevalueTest
    {

        private static FaceValueConfig GetConfig()
        {
            return JsonConfig.GetJson<FaceValueConfig>("Config/activity.facevalue.json");
        }

        [Test]
        public void PrizeExchange()
        {
            var config = GetConfig();
            var  result1 = CardCouponApi.UserGrant(4, config.ActivityId, config.RateCouponA);
            Console.WriteLine("result1: {0}", result1.ToJson());
            var result2 = CardCouponApi.UserGrant(4, config.ActivityId, config.CashCouponA);
            Console.WriteLine("result2: {0}", result2.ToJson());


        }
    }
}

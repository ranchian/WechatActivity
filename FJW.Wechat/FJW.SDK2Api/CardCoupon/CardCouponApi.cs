using FJW.Unit;

using System.Collections.Generic;

using System.Net;
using System.Text;


namespace FJW.SDK2Api.CardCoupon
{
    public class CardCouponApi
    {
        /// <summary>
        /// 发送卡券
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="eventType"></param>
        /// <param name="activityId"></param>
        /// <param name="couponModelId"></param>
        /// <returns></returns>
        public static ApiResponse Grant(long memberId, int eventType, long activityId, long couponModelId )
        {

            var dict = new Dictionary<string, object> {
                { "MemberId", memberId},
                { "EventType", eventType},
                { "ActivityId", activityId },
                { "CardCouponModelID", couponModelId }
            };


            var reqestData = new ApiRequestData
            {
                Method = "CouponService.CardCouponGrant",
                Data = dict.ToJson()
            };

            var conf = ApiConfig.Section.Value.Methods["CouponService"];
#if DEBUG
            Logger.Dedug("url:{0}", conf.EntryPoint);
#endif

            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);
#if DEBUG
            Logger.Dedug("req over:{0}", result.ToJson());
#endif

            if (result.Code == HttpStatusCode.OK)
            {
                return result.Reponse.Deserialize<ApiResponse>();
            }
            return new ApiResponse { Status = ServiceResultStatus.Error};

        }
    }
}

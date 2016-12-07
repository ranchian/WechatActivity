using FJW.Unit;

using System.Collections.Generic;

using System.Net;
using System.Text;


namespace FJW.SDK2Api.CardCoupon
{
    public class CardCouponApi
    {
        /// <summary>
        /// 用户领取卡券
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="activityId"></param>
        /// <param name="couponModelId"></param>
        /// <returns></returns>
        public static ApiResponse UserGrant(long memberId, long activityId, long couponModelId)
        {
            var dict = new Dictionary<string, object> {
                { "MemberId", memberId},
                { "EventType", 2},
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
            return new ApiResponse { Status = ServiceResultStatus.Error };

        }

        /// <summary>
        /// 管理者发送卡券
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="activityId"></param>
        /// <param name="couponModelIds"></param>
        /// <returns></returns>
        public static ApiResponse ManagerGrant(IEnumerable<long> memberIds, long activityId,  IEnumerable<long> couponModelIds )
        {

            var dict = new Dictionary<string, object> {
                { "ReceiveMemberId", string.Join(",", memberIds)},
                { "EventType", 5},
                { "ActivityId", activityId },
                { "CardCouponModelID", string.Join("," , couponModelIds) }
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

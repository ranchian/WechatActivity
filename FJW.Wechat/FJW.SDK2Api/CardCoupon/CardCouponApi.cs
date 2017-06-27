using System;
using FJW.Unit;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
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

            Logger.Dedug("url:{0}", conf.EntryPoint);


            var result = HttpUnit.Post(conf.EntryPoint, reqestData.ToJson(), Encoding.UTF8);

            Logger.Dedug("req over:{0}", result.ToJson());

            var response = result.Reponse.Deserialize<ApiResponse>();
            if (result.Code == HttpStatusCode.OK)
            {
                return response;
            }
            return new ApiResponse { Status = ServiceResultStatus.Error, ExceptionMessage = response.ExceptionMessage };

        }

        /// <summary>
        /// 管理者发送卡券
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="activityId"></param>
        /// <param name="couponModelIds"></param>
        /// <returns></returns>
        public static ApiResponse ManagerGrant(IEnumerable<long> memberIds, long activityId, IEnumerable<long> couponModelIds)
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
            return new ApiResponse { Status = ServiceResultStatus.Error };

        }

        /// <summary>
        /// 批量发送卡券
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="numberIds"></param>
        /// <param name="couponModelIds"></param>
        /// <returns></returns>
        public static CouponGive CouponGive(long memberIds, IEnumerable<long> numberIds, IEnumerable<long> couponModelIds)
        {
            try
            {
                string method = "";
                if (numberIds != null && numberIds.Any())
                    method = "exchange";
                else if (couponModelIds != null && couponModelIds.Any())
                    method = "grant";
                if (string.IsNullOrEmpty(method))
                    return new CouponGive { Status = ServiceResultStatus.Error, ExceptionMessage = "method is null" };

                var dict = new Dictionary<string, object> {
                { "MemberId", memberIds},
                { "EventType", 2},
                { "Numbers", numberIds },
                { "Coupons", couponModelIds }
            };
                var conf = ApiConfig.Section.Value.Methods["GiveCouponListService"];

                var result = HttpUnit.Post(conf.EntryPoint + "/" + method, dict.ToJson(), Encoding.UTF8, "application/json");
                Logger.Dedug("CouponGive req over:{0}", result.ToJson());

                var response = result.Reponse.Deserialize<CouponGive>();
                if (result.Code == HttpStatusCode.OK)
                {
                    return response;
                }
                return new CouponGive { Status = ServiceResultStatus.Error, ExceptionMessage = response.ExceptionMessage };
            }
            catch (Exception ex)
            {
                Logger.Error("CouponGive Error:{0}", ex.ToJson());
                throw;
            }

        }
    }

    public class CouponGive : ApiResponse
    {
        public long Total { get; set; }
        public long Success { get; set; }
        public string Detail { get; set; }
    }
}

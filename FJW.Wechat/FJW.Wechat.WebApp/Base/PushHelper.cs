using System;
using System.Collections.Generic;

using FJW.CommonLib.XService;
using FJW.Ad.Entity.Parameter;
using FJW.CommonLib.ExtensionMethod;

namespace FJW.Unit.Helper
{
    public class PushHelper
    {
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="dic"></param>
        public void PushMsg(Dictionary<string, string> dic)
        {
            if (dic == null || dic.Count == 0)
                return;
            try
            {
                var msgParameter = new PushMsgParameter
                {
                    ReceiveType = (ReceiveType)dic["ReceiveType"].ToInt(),
                    PushType = (PushType)dic["PushType"].ToInt(),
                    Intro = dic["Intro"],
                    PhoneList = dic["PhoneList"],
                    Url = dic["Url"],
                    ProductId = dic["ProductId"].ToInt()
                };
                var result = ServiceEngine.Request("PushMsg", msgParameter);
                if (result.Status == 0)
                {
                    CommonLib.Utils.Logger.Info("Push Success");
                }
            }
            catch (Exception ex)
            {
                CommonLib.Utils.Logger.Error("PushMsg Exception Phone：[" + dic["PhoneList"] + "]", ex);
            }
        }
    }
}
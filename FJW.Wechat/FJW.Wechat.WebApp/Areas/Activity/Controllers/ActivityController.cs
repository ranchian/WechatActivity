using FJW.Wechat.WebApp.Base;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{

    /// <summary>
    /// 活动 
    /// </summary>
    public abstract class ActivityController :WController
    {
        private const string SelfGameKey = "WAC_SELF";

        /// <summary>
        /// 获取自己玩的 记录Id
        /// </summary>
        /// <returns></returns>
        protected string GetSelfGameRecordId()
        {
            var id = Session[SelfGameKey];
            if (id != null)
            {
                return id.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 临时保存自己玩的 记录Id
        /// </summary>
        /// <param name="id"></param>
        protected void SetSelfGameRecordId(string id)
        {
            HttpContext.Session[SelfGameKey] = id;
        }


       /// <summary>
       /// 获取帮别人玩的 记录Id
       /// </summary>
       /// <returns></returns>
        protected string GetHelpGamRecordId(string fid)
        {
            var id = HttpContext.Session[fid];
            if (id != null)
            {
                return id.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 临时保存帮别人玩的 记录Id
        /// </summary>
        /// <param name="fid"></param>
        protected void SetHelpGamRecordId(string fid, string id)
        {
            HttpContext.Session[fid] = id;
        }
    }
}
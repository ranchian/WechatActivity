
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using FJW.SDK2Api.Member;
using FJW.SDK2Api.Statics;
using FJW.Unit;
using FJW.Wechat.WebApp.Base;
using FJW.Wechat.WebApp.Models;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;


namespace FJW.Wechat.WebApp.Controllers
{

    public class WechatFunctionController : WController
    {


        #region  上传图片

        public ActionResult UploadImg(UploadImgModel model)
        {
            FileType t;
            if (model == null || string.IsNullOrEmpty(model.Url) || string.IsNullOrEmpty(model.Type) || !Enum.TryParse(model.Type, out t))
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified });
            }
           
            ;
            /*
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel {ErrorCode = ErrorCode.NotLogged, Message = "未登录"});
            }
            */

            using (var ms = new MemoryStream())
            {
                var token = AccessTokenContainer.GetAccessToken(Config.WechatConfig.AppId);
                MediaApi.Get(token, model.Url, ms);
                if (ms.Length > 0)
                {
                    var result = UploadApi.UploadImg(ms, t, model.Url + ".jpg");
                    Logger.Dedug("UploadImg.Result:{0}", result.ToJson());
                    if (result.IsOk && !string.IsNullOrEmpty(result.Content?.Url))
                    {
                        //TODO:
                        switch (t)
                        {
                            case FileType.Avator:
                                MemberApi.UploadAvator(UserInfo.Id, result.Content.Url, t);
                                break;

                            case FileType.CardId:
                                break;

                            case FileType.BankCard:
                                break;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "获取图片数据失败" });
                }
            }

            return Json(new ResponseModel());
        }

        #endregion





    }
}
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FJW.Unit;
using FJW.Wechat.WebApp.Models;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities.Menu;
using Newtonsoft.Json;
using Senparc.Weixin;


namespace FJW.Wechat.WebApp.Controllers
{
    public class MenusController : WController
    {

        public ActionResult All(string appId)
        {
            try
            {
                var token = AccessTokenContainer.GetAccessToken(appId);

                var menu = CommonApi.GetMenu(token);
                if (menu.errcode != ReturnCode.请求成功)
                {
                    return Json(new ResponseModel
                    {
                        ErrorCode = ErrorCode.Exception,
                        Message = string.Format("微信请求发生错误！错误代码：{0}，说明：{1}", menu.errcode, menu.errmsg)
                    });
                }
                return Json(new ResponseModel {Data = menu});
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return Json(new ResponseModel {ErrorCode = ErrorCode.Exception, Message = ex.Message});
            }
        }

        [HttpPost]
        public ActionResult Add(MenuButtonModel m)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotVerified
                });
            }

            var menu = m.Menu.Deserialize<Menu>();
            if (menu == null)
            {
                return Json(new ResponseModel {ErrorCode = ErrorCode.Exception, Message = "无效的菜单数据"});
            }
            if (menu.Buttons == null || menu.Buttons.Count == 0 || menu.Buttons.Count > 3)
            {
                return Json(new ResponseModel {ErrorCode = ErrorCode.Exception, Message = "一级菜单必须有，且不可以多于3个"});
            }
            var d = JsonConvert.SerializeObject(menu,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            Logger.Info(d);
            var token = AccessTokenContainer.GetAccessToken(m.AppId);
            var bg = new ButtonGroup();
            foreach (var btn in menu.Buttons)
            {
                var subButton = new SubButton(btn.Name);
                foreach (var it in btn.SubButtons)
                {
                    if (string.IsNullOrEmpty(it.FuctionType) || it.FuctionType == "view")
                    {
                        subButton.sub_button.Add(new SingleViewButton {url = it.Url, name = it.Name, type = "view"});
                        continue;
                    }
                    if (it.FuctionType == "click")
                    {
                        subButton.sub_button.Add(new SingleClickButton {key = it.Key, name = it.Name, type = "click"});
                        continue;
                    }
                }
                bg.button.Add(subButton);
            }

            var result = CommonApi.CreateMenu(token, bg);
            if (result.errcode != ReturnCode.请求成功)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.Exception,
                    Message = string.Format("微信请求发生错误！错误代码：{0}，说明：{1}", result.errcode, result.errmsg)
                });
            }
            return Json(new ResponseModel
            {
                ErrorCode = ErrorCode.None,
                Data = result
            });

        }
        
    }




    /// <summary>
    /// 菜单容器
    /// </summary>
    public class Menu
    {
        [JsonProperty("button")]
        public List<MenuButton> Buttons { get; set; }
    }

    /// <summary>
    /// 菜单按钮
    /// </summary>
    public class MenuButton
    {
        /// <summary>
        /// 显示名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// media_id：下发消息媒体Id
        /// </summary>
        [JsonProperty("meida_id")]
        public string MediaId { get; set; }

        /// <summary>
        /// 功能类型
        /// </summary>
        [JsonProperty("type")]
        public string FuctionType { get; set; }

        /// <summary>
        /// click 事件key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// view 跳转URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 子菜单
        /// </summary>
        [JsonProperty("sub_button")]
        public List<MenuButton> SubButtons { get; set; }
    }
     

}
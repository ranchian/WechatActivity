using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Controllers;
using FJW.Wechat.WebApp.Models;

using Newtonsoft.Json;


namespace FJW.Wechat.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //Console.WriteLine(FuctionType.View.ToString());
             
            var menu = new Menu();

            menu.Buttons = new List<MenuButton>();
            var parentBtn1 = new MenuButton
            {
                Name = "个人中心",
                SubButtons = new List<MenuButton>()
            };
            parentBtn1.SubButtons.Add(new MenuButton
            {
                Name = "一键注册",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/wx/account/regist?Channel=WXF"
            });

            parentBtn1.SubButtons.Add(new MenuButton
            {
                Name = "下载APP",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/down/index"
            });

            parentBtn1.SubButtons.Add(new MenuButton
            {
                Name = "专属订阅号",
                FuctionType = "view",
                Url = "http://mp.weixin.qq.com/mp/getmasssendmsg?__biz=MzI1MzM1MTI3Mg==#wechat_webview_type=1&wechat_redirect"
            });




            var parentBtn2 = new MenuButton
            {
                Name = "活动福利",
                SubButtons = new List<MenuButton>()
            };
            parentBtn2.SubButtons.Add(new MenuButton
            {
                Name = "投资大赛",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/htmls/activity/invest.html"
            });

            parentBtn2.SubButtons.Add(new MenuButton
            {
                Name = "免费发券",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/htmls/activity/coupon.html"
            });

            parentBtn2.SubButtons.Add(new MenuButton
            {
                Name = "新手活动",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/htmls/activity/moneyReward.html?from=singlemessage&isappinstalled=1"
            });

            parentBtn2.SubButtons.Add(new MenuButton
            {
                Name = "新手专享",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/htmls/vip.html?from=singlemessage&isappinstalled=1"
            });

            parentBtn2.SubButtons.Add(new MenuButton
            {
                Name = "邀请好友",
                FuctionType = "view",
                Url = "http://www.fangjinnet.com/htmls/activity/honghuangzhili.html?from=singlemessage&isappinstalled=1"
            });

            var parentBtn3 = new MenuButton
            {
                Name = "关于我们",
                SubButtons = new List<MenuButton>()
            };

            parentBtn3.SubButtons.Add(new MenuButton
            {
                Name = "公司介绍",
                Url = "http://c.eqxiu.com/s/toQXQgkM?eqrcode=1&from=singlemessage&isappinstalled=0"
            });

            menu.Buttons.Add(parentBtn1);

            menu.Buttons.Add(parentBtn2);

            menu.Buttons.Add(parentBtn3);

            var d = JsonConvert.SerializeObject(menu, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Console.WriteLine(d);
             

            Console.ReadLine();
        }
    }
}
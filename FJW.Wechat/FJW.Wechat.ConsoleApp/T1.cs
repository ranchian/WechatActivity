using System;

using FJW.SDK2Api.Message;
using FJW.Unit;

namespace FJW.Wechat.ConsoleApp
{
    public class T1
    {
        public void Test()
        {
            var result = SmsApi.Send("18217016934", "您的定期产品今天到期，房金网感恩回馈送您理财加息券！ 打开房金网APP查看，退订回复TD");
            Console.WriteLine(result.ToJson());
        }

    }
}

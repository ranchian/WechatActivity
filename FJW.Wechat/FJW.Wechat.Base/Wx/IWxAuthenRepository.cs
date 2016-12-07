using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Wx
{
    public interface IWxAuthenRepository
    {
        WxUserInfo GetUserInfo(string openId);

        void Add(WxUserInfo u);
    }
}

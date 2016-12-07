using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Wx
{
    public interface IWxMediaApi
    {
        void Get(string url, Stream stream);

        byte[] Get(string mediaId);
    }
}

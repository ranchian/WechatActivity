using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Data.Model.RDBS
{
    public class MemberChannel
    {
        public long MemberId { get; set; }

        public string Channel { get; set; }

        public DateTime CreateTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Model.MongoDb;

namespace FJW.Wechat.Data.Model.Mongo
{
    [Table("WxShareSupport")]
    public class WxShareSupportModel : BaseModel
    {
        public string RowId { get; set; }

        public string Key { get; set; }

        public string OpenId { get; set; }
        

        public string NickName { get; set; }

        public string HeadimgUrl { get; set; }

        public long UserId { get; set; }

    }
}

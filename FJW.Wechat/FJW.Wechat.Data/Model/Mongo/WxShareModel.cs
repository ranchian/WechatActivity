using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;

namespace FJW.Wechat.Data.Model.Mongo
{
    [Table("WxShare")]
    public class WxShareModel : BaseModel
    {
        public string Key { get; set; }
        public string OpenId { get; set; }

        

        public string NickName { get; set; }

        public string HeadimgUrl { get; set; }

        public long UserId { get; set; }
    }
 
}

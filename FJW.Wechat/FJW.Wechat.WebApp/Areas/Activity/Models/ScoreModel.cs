using System.ComponentModel.DataAnnotations;

namespace FJW.Wechat.WebApp.Areas.Activity.Models
{
    public class ScoreModel
    {
        [Required]
        public string Key { get; set; }

        public int Score { get; set; }

        public string Fid { get; set; }
    }
}
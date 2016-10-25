
using System.ComponentModel.DataAnnotations;


namespace FJW.Wechat.WebApp.Models
{
    public class MenuButtonModel
    {
        [Required]
        public string AppId { get; set; }

        [Required]
        public string Menu { get; set; }

    }


}
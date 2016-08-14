using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FJW.Wechat.WebApp.Models
{
    /// <summary>
    /// 登录模型
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Phone { get; set; }


        [Required]
        public string Password { get; set; }


        public string ValidateCode { get; set; }
    }
}
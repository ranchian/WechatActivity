using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.SDK2Api
{
    public class ApiConfig
    {
        public static readonly Lazy<ApiSection> Section = new Lazy<ApiSection>(() => System.Configuration.ConfigurationManager.GetSection("api") as ApiSection);
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Wechat.Activity.Controllers;

namespace FJW.Wechat.Activity.Models
{
    public class CardExchangeModel
    {
        public int Type { get; set; }

        public CardTuple[] Cards { get; set; }

        public int Count { get; set; }
    }

    public class CardTuple
    {
        public CardType Type { get; set; }

        public int Count { get; set; }
    }

}

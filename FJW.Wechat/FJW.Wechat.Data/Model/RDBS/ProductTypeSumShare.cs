﻿using System;

namespace FJW.Wechat.Data.Model.RDBS
{
    /// <summary>
    /// 产品购买信息
    /// </summary>
    public class ProductTypeSumShare
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        public long ProductTypeId { get; set; }

        /// <summary>
        /// 产品份额
        /// </summary>
        public decimal BuyShares { get; set; }

        /// <summary>
        /// 购买时间
        /// </summary>
        public DateTime BuyTime { get; set; }
    }
}

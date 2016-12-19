namespace FJW.Wechat.Data.Model.RDBS
{
    /// <summary>
    /// 产品购买信息
    /// </summary>
    public class ProductTypeSumShare
    {
        /// <summary>
        /// 产品类型
        /// </summary>
        public long ProductTypeId { get; set; }

        /// <summary>
        /// 产品份额
        /// </summary>
        public decimal Shares { get; set; }
    }
}

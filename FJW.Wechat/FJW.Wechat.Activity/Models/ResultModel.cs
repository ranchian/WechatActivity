namespace FJW.Wechat.Activity.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultModel<T>
    {
        /// <summary>
        /// 结果实体
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 操作状态 0失败 1成功
        /// </summary>
        public int Success { get; set; }
    }
}
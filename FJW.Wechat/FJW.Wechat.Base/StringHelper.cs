namespace FJW.Wechat
{
    public class StringHelper
    {
        /// <summary>
        /// 给电话号码打码
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static string CoverPhone(string phone)
        {
            if (phone == null)
            {
                return string.Empty;
            }
            if (phone.Length < 7)
            {
                return phone;
            }
            return phone.Substring(0, 3) + "****" + phone.Substring(7, 4);
        }
    }
}
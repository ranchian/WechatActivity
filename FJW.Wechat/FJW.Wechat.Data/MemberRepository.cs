using System;

using System.Linq;

using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

using Dapper;


namespace FJW.Wechat.Data
{
    /// <summary>
    /// 用户数据
    /// </summary>
    public class MemberRepository
    {

        private readonly string _connectionString;

        public MemberRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public MemberModel GetMemberInfo(string token)
        {
            using (var conn = GetDbConnection())
            {
               return conn.Query<MemberModel>(
                   "select top 1 MemberID, Token from dbo.BD_MemberLoginLog where IsDelete = 0 and Token = @token order by LoginTime desc;", 
                   new { token }).FirstOrDefault();
            }
        }

        /// <summary>
        /// 送 体验金
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="experienceId"></param>
        /// <param name="productId"></param>
        /// <param name="amount"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public int Give(long memberId, long experienceId, long productId, decimal amount, long objectId)
        {
            using (var conn = GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@memberId", memberId, DbType.Int64);
                parameters.Add("@amount", amount, DbType.Decimal);
                parameters.Add("@experienceId", experienceId, DbType.Int64);
                parameters.Add("@productId", productId, DbType.Int64);
                parameters.Add("@returnValue", null, DbType.Int32, ParameterDirection.ReturnValue);
                conn.Execute( "Trading.dbo.SP_GiveExperienceAmount", parameters,  commandType: CommandType.StoredProcedure);
                var v = parameters.Get<int>("@returnValue");
                return v;
            }
        }
    }
}

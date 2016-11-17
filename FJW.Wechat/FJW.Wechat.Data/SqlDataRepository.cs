using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Dapper;

namespace FJW.Wechat.Data
{
    public class SqlDataRepository
    {
        private readonly string _connectionString;

        public SqlDataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        #region  投资排行榜

        public IEnumerable<RankingRow> TodayRanking()
        {
            const string sql = @"with T1 as (
	select 
		MemberID, ProductTypeID, Shares
	from (
		select 
			MemberID, ProductTypeID, Shares, ROW_NUMBER() OVER( partition by T.ProductTypeID ORDER BY Shares desc ) _RN
		from (
			select 
				B.MemberID , B.ProductTypeID  , SUM(B.BuyShares) Shares
			from Trading..TC_ProductBuy B 
			where B.IsDelete = 0 and B.Status = 1 and B.ProductTypeID < 9 and B.ProductTypeParentID = 2 and DATEDIFF(DAY, B.BuyTime, GETDATE()) = 0 
            and DATEDIFF(DAY, '2016-10-12', GETDATE()) >= 0
			group by B.MemberID , B.ProductTypeID having SUM(B.BuyShares) >= 3000
		) T 
	) Temp  where _RN = 1
)
select    M.Phone, T.Title, Cast( T1.Shares as int) Shares
from  Basic..BD_ProductType T 
left join T1 on T.ID = T1.ProductTypeID 
left join Basic..BD_Member M on T1.MemberID = M.ID
where  T.ID in (5, 6, 7, 8)";
            using (var conn = GetDbConnection())
            {
                return conn.Query<RankingRow>(sql);
            }
        }

        /// <summary>
        /// Select getdate()
        /// </summary>
        /// <returns></returns>
        public DateTime GetDate()
        {
            using (var conn = GetDbConnection())
            {
                return conn.ExecuteScalar<DateTime>("select GETDATE()");
            }
        }

        public IEnumerable<RankingRow> TotalRanking()
        {
            const string sql = @"with T1 as (
	select MemberID, ProductTypeID, Shares, _RN as Sequnce
	from 
	(
		select MemberID, ProductTypeID, Shares, ROW_NUMBER() OVER( partition by T.ProductTypeID ORDER BY Shares desc ) _RN
		from (
			select 
				B.MemberID , B.ProductTypeID  , SUM(B.BuyShares) Shares
			from Trading..TC_ProductBuy B 
			where B.IsDelete = 0 and B.Status = 1 and B.ProductTypeID < 9 and B.ProductTypeID > 5 
			and DATEDIFF(DAY, B.BuyTime, @startdate) <= 0 and DATEDIFF(DAY, B.BuyTime, @endDate) >= 0
			group by B.MemberID , B.ProductTypeID  having SUM(B.BuyShares) >= 15000
		) T 
	) Temp where Temp._RN < 4
)
select T.ID, M.Phone, T.Title, Cast( T1.Shares as int) Shares, Sequnce
from  Basic..BD_ProductType T 
left join T1 on T.ID = T1.ProductTypeID 
left join Basic..BD_Member M on T1.MemberID = M.ID
where  T.ID in (6, 7, 8)";

            using (var conn = GetDbConnection())
            {
                return conn.Query<RankingRow>(sql, new { startDate = new DateTime(2016, 10, 12), endDate = new DateTime(2016, 10, 26) });
            }

        }

        #endregion

        #region 红包雨 拆卡券

        /// <summary>
        /// 各类型产品购买金额
        /// </summary>
        /// <param name="memberId">用户Id</param>
        /// <param name="startTime">开始时间（包含）</param>
        /// <param name="endTime">结束时间（不包含）</param>
        /// <returns></returns>
        public IEnumerable<ProductTypeSumShare> GetProductTypeShares(long memberId, DateTime startTime, DateTime endTime)
        {
            const string sql = @"select ProductTypeID, SUM(BuyShares) as Shares from Trading..TC_ProductBuy 
where IsDelete = 0 and MemberID =  @memberId and BuyTime >= @startTime and BuyTime < @endTime and Status = 1
group by ProductTypeID";
            using (var conn = GetDbConnection())
            {
                return conn.Query<ProductTypeSumShare>(sql, new {memberId, startTime, endTime});
            }
        }

        #endregion

        #region Other
        /// <summary>
        /// 获取用户渠道
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public MemberChannel GetMemberChennel(long memberId)
        {
            const string sql = @"select top 1 [Channel], [CreateTime] from [Report].[dbo].[DR_MemberChannel] where Isdelete = 0 and MemberId = @memberId";
            using (var conn = GetDbConnection())
            {
                var d = conn.QueryFirstOrDefault<MemberChannel>(sql, new {memberId});
                if (d != null)
                {
                    d.MemberId = memberId;
                }
                return d;
            }
            
        }

        #endregion
    }

    public class MemberChannel
    {
        public long MemberId { get; set; }

        public string Channel { get; set; }

        public DateTime CreateTime { get; set; }
    }
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

    public class RankingRow
    {
        public long Id { get; set; }

        public string Phone { get; set; }

        public string Title { get; set; }

        public int Shares { get; set; }

        public int Sequnce { get; set; }
    }
}

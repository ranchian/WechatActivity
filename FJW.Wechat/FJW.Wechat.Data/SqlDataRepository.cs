using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using FJW.Unit;
using FJW.Wechat.Data.Model.RDBS;

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

        /// <summary>
        /// 按照产品类型分的购买记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IEnumerable<ProductTypeSumShare> ProductTypeBuyRecrods(long memberId, DateTime startTime, DateTime endTime)
        {
            const string sql = @"select ProductTypeID, BuyShares from Trading..TC_ProductBuy 
where IsDelete = 0 and MemberID = @memberId and BuyTime >= @startTime and BuyTime < @endTime and Status = 1";

            using (var conn = GetDbConnection())
            {
                return conn.Query<ProductTypeSumShare>(sql, new { memberId, startTime, endTime });
            }
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
				B.MemberID , B.ProductTypeID  , MAX(B.BuyShares) Shares
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

        /// <summary>
        /// 定期产品购买排行榜
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IEnumerable<RankingRow> ProductBuyRanking(DateTime startTime, DateTime endTime)
        {
            const string sql = @"with T1 as (
	select 
		MemberID, ProductTypeID, Shares
	from (
		select 
			MemberID, ProductTypeID, Shares, ROW_NUMBER() OVER( partition by T.ProductTypeID ORDER BY Shares desc, BuyID ) _RN
		from (
			select 
				B.MemberID , B.ProductTypeID  , Max(B.BuyShares) Shares, MIN(B.ID) BuyID
			from Trading..TC_ProductBuy B 
            left join Basic..BD_MemberInviteFriends F on F.FriendID = B.MemberID 
			where B.IsDelete = 0 and B.Status = 1 and B.ProductTypeID < 9 and B.ProductTypeParentID = 2 and  B.BuyTime >= @startTime and  B.BuyTime <= @endTime
            and not exists ( select ID from Trading..TC_DisableMember D where D.MemberID = F.MemberID )
            and not exists ( select ID from [Report].[dbo].[Data_MemberChannel] C where C.MemberID = B.MemberId and C.Channel = 'WQWLCPS' )
			group by B.MemberID , B.ProductTypeID  
		) T 
	) Temp  where _RN = 1
)

select 
	M.Phone, T1.ProductTypeId as Id, T1.Shares 
from T1 
left join Basic..BD_Member M on T1.MemberID = M.ID;";

            using (var conn = GetDbConnection())
            {
                return conn.Query<RankingRow>(sql, new { startTime, endTime });
            }
        }

        #endregion

        /// <summary>
        /// 用户购买次数
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public int ProductBuyCount(long memberId, DateTime startTime, DateTime endTime)
        {
            using (var conn = GetDbConnection())
            {
                return conn.ExecuteScalar<int>(
                    @"select COUNT(ID) from Trading..TC_ProductBuy 
where MemberID = @memberId and BuyTime >= @startTime and BuyTime < @endTime and ProductTypeParentID = 2 and Status = 1 and IsDelete = 0",
                    new { memberId, startTime, endTime });
            }
        }

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
            const string sql = @"select ProductTypeID, SUM(BuyShares) as BuyShares from Trading..TC_ProductBuy 
where IsDelete = 0 and MemberID =  @memberId and BuyTime >= @startTime and BuyTime < @endTime and Status = 1
group by ProductTypeID";
            using (var conn = GetDbConnection())
            {
                return conn.Query<ProductTypeSumShare>(sql, new { memberId, startTime, endTime });
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
                var d = conn.QueryFirstOrDefault<MemberChannel>(sql, new { memberId });
                if (d != null)
                {
                    d.MemberId = memberId;
                }
                return d;
            }

        }

        #endregion

        #region 2017春节翻倍
        /// <summary>
        /// 获取 2017春节翻倍 的倍率
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Tuple<decimal, long> GetSpringFestivalMultiple(long orderId)
        {
            using (var conn = GetDbConnection())
            {
                var d = conn.QueryFirstOrDefault("select Multiple, ProductTypeId from Trading..TC_OrderMutiple where ID= @orderId", new { orderId });
                if (d == null || d.Multiple == 0 || d.ProductTypeId == 0)
                {
                    return new Tuple<decimal, long>(0, 0);
                }
                return new Tuple<decimal, long>(d.Multiple, d.ProductTypeId);
            }
        }

        /// <summary>
        /// 春节翻倍记录
        /// </summary>
        /// <returns></returns>
        public object GetSpringFestivalRows()
        {
            using (var conn = GetDbConnection())
            {
                return conn.Query(@"select top 10  
    O.Multiple as multiple , LEFT( M.Phone, 3) + '****' + RIGHT(M.Phone, 4) as phone, 
	case O.ProductTypeID when 5 then '月宝' when 6 then '季宝' when 7 then '双季宝' when 8 then '年宝' else ''  end as title
from Trading..TC_OrderMutiple O
left join Basic..BD_Member M on O.MemberId = M.ID
order by O.ID desc");
            }
        }

        #endregion

        /// <summary>
        /// 获取春龙天天赚排名
        /// </summary>
        /// <returns></returns>
        public List<SpringDragonRanking> GetSpringDragonRanking(long productId, int topN)
        {
            using (var conn = GetDbConnection())
            {
                return conn.Query<SpringDragonRanking>(
                @"SELECT top " + topN + @" T.*,STUFF(m.Phone, 4, 4, '****') AS Phone FROM 
                    (SELECT A.MemberID,(A.BuyShares - ISNULL(B.RedeemShares,0) ) TotalBuyShares,A.LastBuyTime FROM (SELECT MemberID,SUM(BuyShares) AS  BuyShares,MAX(BuyTime) as LastBuyTime FROM Trading..TC_ProductBuy   WHERE ProductID=@productId group by MemberID) A
                    LEFT JOIN (SELECT MemberID,SUM(RedeemShares) AS  RedeemShares   FROM Trading..TC_ProductRedeem  WHERE ProductID=@productId group by MemberID) B
                    ON A.MemberID=B.MemberID) T 
                JOIN Basic..BD_Member m ON T.MemberID = m.ID WHERE T.TotalBuyShares>=2000
                ORDER BY TotalBuyShares DESC,LastBuyTime ASC", new { productId }).ToList();
            }
        }

        /// <summary>
        /// 用户购买次数（不含新手专享）
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public int BuyCount(long memberId, DateTime startTime, DateTime endTime)
        {
            using (var conn = GetDbConnection())
            {
                return conn.ExecuteScalar<int>(
                    @"select COUNT(ID) from Trading..TC_ProductBuy 
where MemberID = @memberId and ProductTypeParentID = 2 and ProductTypeID!=9 and Status = 1 and IsDelete = 0 and BuyTime >= @startTime and BuyTime < @endTime",
                    new { memberId, startTime, endTime });
            }
        }

        /// <summary>
        /// 换购活动 用户购买记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<EntityReward> MemberBuyRecord(long memberId, DateTime startTime, DateTime endTime, decimal money, long productMappingDetailId = 0)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    return conn.Query<EntityReward>($@"SELECT MPMD.MemberID AS MemberId,
                                                              MPMD.ID AS ProductMappingDetailId,
		                                                      MPMD.ProductIncome AS TotalIncome,
                                                              MPMD.ProductShares AS ProductShares,    
                                                              PR.CreateTime AS ExChangeTime,
                                                              PT.Title AS Title, 															  
                                            ( CASE WHEN ISNULL(PR.ID, '') != '' THEN 2											       
                                                   WHEN ISNULL(MPMD.SurplusIncome, 0) < @money  THEN 1                                                                                       		       
                                                   ELSE 0
                                              END ) AS [State]
                                    FROM    Trading..TC_MemberProductMappingDetail MPMD
                                            LEFT JOIN Trading..TC_EntityReward PR ON MPMD.ID = PR.ProductMappingDetailId                                              
                                            AND PR.IsDelete = MPMD.IsDelete
                                            LEFT JOIN Basic..BD_ProductType PT ON PT.ID=MPMD.ProductTypeID
                                    WHERE   MPMD.MemberID = @memberId
                                            AND ProductTypeParentID = 2
                                            AND ProductTypeID != 9
                                            AND MPMD.Status = 1
                                            AND MPMD.IsDelete = 0                                            
                                            AND MPMD.BuyTime >= @startTime
                                            AND MPMD.BuyTime < @endTime
                                            {(productMappingDetailId == 0 ? "" : "AND MPMD.Id = @productMappingDetailId")} ", new { memberId, startTime, endTime, money, productMappingDetailId }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Info("MemberBuyRecord : {0}", ex.ToString());
                return new List<EntityReward>();
            }
        }

        public long GetLasted(long productType)
        {
            using (var conn = GetDbConnection())
            {
                return
                    conn.ExecuteScalar<long>(
                        "select top 1 ID from Trading..TC_Product T where T.Isdelete = 0 and T.ProductTypeId = @productType and T.SalesStatus < 3;",
                        new {productType});
            }
        }

        /// <summary>
        /// 奖品列表
        /// </summary>
        /// <returns></returns>
        public List<RealThing> GetPrize()
        {
            using (var conn = GetDbConnection())
            {
                return conn.Query<RealThing>(@"SELECT ID AS PrizeId, Name,[Money], ExchangeMoney  FROM Basic..BD_RealThing WHERE IsDelete = 0").ToList();
            }
        }
        /// <summary>
        /// 添加奖品记录
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public int Add(EntityReward er)
        {
            try
            {

                using (var conn = GetDbConnection())
                {
                    var param = new DynamicParameters();
                    param.Add("@ActivityId", er.ActivityId, DbType.Int64);
                    param.Add("@ActivityName", er.ActivityName, DbType.String);
                    param.Add("@MemberId", er.MemberId, DbType.Int64);
                    param.Add("@Phone", er.Phone, DbType.String);
                    param.Add("@ProductMappingDetailId", er.ProductMappingDetailId, DbType.Int64);
                    param.Add("@PrizeId", er.PrizeId, DbType.Int64);
                    param.Add("@PrizeName", er.PrizeName, DbType.String);
                    param.Add("@TotalIncome", er.TotalIncome, DbType.Decimal);
                    param.Add("@PrizeMoney", er.PrizeMoney, DbType.Decimal);
                    param.Add("@IncomeReduceState", er.IncomeReduceState, DbType.Int32);
                    param.Add("@ReceiveState", er.ReceiveState, DbType.Int32);
                    param.Add("@Remark", er.Remark, DbType.String);
                    return conn.Execute(@"INSERT INTO [Trading].[dbo].[TC_EntityReward]
                                                    (
                                                     [ActivityId]
                                                    ,[ActivityName]
                                                    ,[MemberId]
                                                    ,[Phone]
                                                    ,[ProductMappingDetailId]
                                                    ,[PrizeId]
                                                    ,[PrizeName]
                                                    ,[TotalIncome]
                                                    ,[PrizeMoney]                                                    
                                                    ,[IncomeReduceState]
                                                    ,[ReceiveState]
                                                    ,[Remark])
                                              VALUES
                                                    (
                                                     @ActivityId
                                                    ,@ActivityName
                                                    ,@MemberId
                                                    ,@Phone
                                                    ,@ProductMappingDetailId
                                                    ,@PrizeId
                                                    ,@PrizeName
                                                    ,@TotalIncome
                                                    ,@PrizeMoney                                                    
                                                    ,@IncomeReduceState
                                                    ,@ReceiveState
                                                    ,@Remark)", param);
                }
            }
            catch (Exception ex)
            {

                Logger.Error($"ADD Error:{ex}  Data:{er}");
                return -1;
            }
        }

        //发送短信
        public int AddSms(Sms sms)
        {

            using (var conn = GetDbConnection())
            {
                var param = new DynamicParameters();
                param.Add("@Phone", sms.Phone, DbType.Int64);
                param.Add("@Msg", sms.Msg, DbType.String);
                param.Add("@SIGN", sms.sign, DbType.Int16);
                param.Add("@CreateTime", sms.CreateTime, DbType.DateTime);
                param.Add("@Channel", sms.Channel, DbType.String);
                return conn.Execute(@"INSERT  INTO SMS..SMS_Message([Phone], [Msg], [Sign],[CreateTime],Channel )
                                VALUES  ( @Phone, @Msg, @SIGN ,@CreateTime,@Channel );
           ", param);
            }
        }
    }

    public class EntityReward
    {
        public long ActivityId { get; set; }
        public string ActivityName { get; set; }
        public long MemberId { get; set; }
        public string Phone { get; set; }
        public long ProductMappingDetailId { get; set; }
        public long PrizeId { get; set; }
        //奖品名称
        public string PrizeName { get; set; }
        //总收益
        public decimal TotalIncome { get; set; }
        //奖品价格
        public decimal PrizeMoney { get; set; }

        public int ReduceType { get; set; }

        public string Title { get; set; }
        public decimal ProductShares { get; set; }

        public int IncomeReduceState { get; set; }
        //领取状态(0未领取 1已领取)
        public int ReceiveState { get; set; }

        public string Remark { get; set; }
        //0未兑换 1无法兑换 2兑换
        public int State { get; set; }
        //兑换时间
        public DateTime ExChangeTime { get; set; }
    }

    public class RealThing
    {
        //实物名称
        public int PrizeId { get; set; }
        //实物名称
        public string Name { get; set; }
        //实物价格
        public decimal Money { get; set; }
        //兑换价格
        public decimal ExchangeMoney { get; set; }

    }

    public class Sms
    {
        public long Phone { get; set; }

        public string Msg { get; set; }

        public int sign { get; set; }

        public DateTime CreateTime => DateTime.Now;

        public string Channel=>"changlan";
    }
}

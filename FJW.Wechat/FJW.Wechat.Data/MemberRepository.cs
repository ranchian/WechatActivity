using Dapper;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;

using FJW.CommonLib.ExtensionMethod;
using System.Collections.Generic;

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
                    "select top 1 MemberID, Phone, Token from dbo.BD_MemberLoginLog where IsDelete = 0 and Token = @token order by LoginTime desc;",
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
                conn.Execute("Trading.dbo.SP_GiveExperienceAmount", parameters, commandType: CommandType.StoredProcedure);
                var v = parameters.Get<int>("@returnValue");
                return v;
            }
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public int GetMemberAward(long memberId)
        {
            //抽奖次数
            var awardCnt = 0;
            DataTable dt = new DataTable();
            //用户自己的购买记录
            using (var conn = GetDbConnection())
            {
                var reader = conn.ExecuteReader(string.Format(@"select
	                                                                MemberID,ProductTypeID,sum(BuyShares)BuyShares
                                                                from trading..TC_ProductBuy PB
                                                                where ProductTypeID in (5,6,7,8) and Status = 1 and MemberID = {0}
                                                                group by MemberID,ProductTypeID ", memberId));

                int intFieldCount = reader.FieldCount;
                for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                {
                    dt.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
                }

                dt.BeginLoadData();

                object[] objValues = new object[intFieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objValues);
                    dt.LoadDataRow(objValues, true);
                }

                //计算用户自己购买后的抽奖次数
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var dr = dt.Rows[i];
                        switch (dr["ProductTypeID"].ToString())
                        {
                            case "5":
                                awardCnt += int.Parse(dr["BuyShares"].ToString()) / 10000;
                                break;

                            case "6":
                                awardCnt += int.Parse(dr["BuyShares"].ToString()) / 3000;
                                break;

                            case "7":
                                awardCnt += int.Parse(dr["BuyShares"].ToString()) / 2000;
                                break;

                            case "8":
                                awardCnt += int.Parse(dr["BuyShares"].ToString()) / 1000;
                                break;
                        }
                    }
                }
            }

            //用户自己已使用的抽奖次数
            using (var conn = GetDbConnection())
            {
                var useCnt = conn.ExecuteScalar(string.Format(@"SELECT COUNT(0) FROM Other..OT_AwardItem
                                                WHERE IsDelete = 0 AND ActivityID = 8 AND MemberID = {0}", memberId)).ToInt();
                awardCnt -= useCnt;
            }

            //好友的购买记录
            using (var conn = GetDbConnection())
            {
                var reader = conn.ExecuteReader(string.Format(@"SELECT  b.MemberID ,
                                                                        ProductTypeID ,
                                                                        SUM(BuyShares) BuyShares
                                                                FROM    Basic..BD_MemberInviteFriends a
                                                                        INNER JOIN Trading..TC_ProductBuy b ON a.FriendID = b.MemberID
                                                                WHERE   ProductTypeID IN ( 5, 6, 7, 8 )
                                                                        AND b.Status = 1
                                                                        AND a.MemberID = {0}
		                                                                AND a.IsDelete = 0
                                                                        AND a.IsDelete = 0
                                                                        AND a.Status = 1
                                                                GROUP BY b.MemberID ,
                                                                        ProductTypeID", memberId));

                dt.Clear();
                dt.Columns.Clear();

                int intFieldCount = reader.FieldCount;
                for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                {
                    dt.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
                }

                dt.BeginLoadData();

                object[] objValues = new object[intFieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objValues);
                    dt.LoadDataRow(objValues, true);
                }

                //计算好友购买产品是否符合规则
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var dr = dt.Rows[i];
                        switch (dr["ProductTypeID"].ToString())
                        {
                            case "5":
                                if (decimal.Parse(dr["BuyShares"].ToString()) >= 10000)
                                {
                                    awardCnt++;
                                    return awardCnt;
                                }
                                break;

                            case "6":
                                if (decimal.Parse(dr["BuyShares"].ToString()) >= 3000)
                                {
                                    awardCnt++;
                                    return awardCnt;
                                }
                                break;

                            case "7":
                                if (decimal.Parse(dr["BuyShares"].ToString()) >= 2000)
                                {
                                    awardCnt++;
                                    return awardCnt;
                                }
                                break;

                            case "8":
                                if (decimal.Parse(dr["BuyShares"].ToString()) >= 1000)
                                {
                                    awardCnt++;
                                    return awardCnt;
                                }
                                break;
                        }
                    }
                }
            }
            return awardCnt;
        }

        /// <summary>
        /// 送 现金奖励
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="amount"></param>
        /// <param name="rewardId"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public void GiveMoney(long memberId, decimal amount, long rewardId, long objectId)
        {
            using (var conn = GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@memberId", memberId, DbType.Int64);
                parameters.Add("@amount", amount, DbType.Decimal);
                parameters.Add("@rewardId", rewardId, DbType.Int64);
                parameters.Add("@objectId", objectId, DbType.Int64);
                conn.Execute("Trading.dbo.SP_GiveRewardAmount", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="itemName"></param>
        /// <param name="itemType"></param>
        public void AddRecord(long memberId, string itemName, int itemType, decimal itemValue, string key)
        {
            using (var conn = GetDbConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ItemName", itemName, DbType.String);
                param.Add("@ItemType", itemType, DbType.Int32);
                param.Add("@ItemValue", itemValue, DbType.Decimal);
                param.Add("@GameKey", key, DbType.String);
                param.Add("@MemberID", memberId, DbType.Int64);
                conn.ExecuteScalar(@"INSERT INTO Other.dbo.OT_AwardItem
                                            ( ItemName ,
                                              ItemType ,
                                              ItemValue,
                                              GameKey,
                                              IsUsed ,
                                              ActivityID ,
                                              MemberID
                                            )
                                    VALUES  ( @ItemName ,
                                              @ItemType ,
                                              @ItemValue,
                                              @GameKey,
                                              1 ,
                                              8 ,
                                              @MemberID
                                            )", param);
            }
        }

        /// <summary>
        /// 获奖记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DataTable GetRecord(string key)
        {
            using (var conn = GetDbConnection())
            {
                var sql = string.Format(@"SELECT  LEFT(m.Phone,3)+'****'+RIGHT(m.Phone,4) Phone,
                                                a.ItemName ,
                                                a.ItemType
                                        FROM    ( SELECT    ItemName ,
                                                            MemberID ,
                                                            ItemType
                                                  FROM      Other..OT_AwardItem
                                                  WHERE     gamekey = 'turntable'
                                                            AND IsDelete = 0
                                                            AND ItemType > 2
                                                  UNION
                                                  SELECT    ItemName ,
                                                            MemberID ,
                                                            ItemType
                                                  FROM      Other..OT_AwardItem
                                                  WHERE     gamekey = '{0}'
                                                            AND IsDelete = 0
                                                            AND ItemType <= 2
                                                ) a
                                                INNER JOIN Basic..BD_Member m ON a.MemberID = m.ID", key);
                DataTable dt = new DataTable();
                var reader = conn.ExecuteReader(sql);
                int intFieldCount = reader.FieldCount;
                for (int intCounter = 0; intCounter < intFieldCount; ++intCounter)
                {
                    dt.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
                }

                dt.BeginLoadData();

                object[] objValues = new object[intFieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objValues);
                    dt.LoadDataRow(objValues, true);
                }
                return dt;
            }
        }
    }
}
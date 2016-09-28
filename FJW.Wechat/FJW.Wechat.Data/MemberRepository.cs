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
                var sql = string.Format(@"SELECT  MemberID ,
                                                ProductTypeID ,
                                                SUM(BuyShares) BuyShares
                                        FROM    trading..TC_ProductBuy PB
                                        WHERE   ProductTypeID IN ( 5, 6, 7, 8 )
                                                AND Status = 1
                                                AND BuyTime >= '2016-09-28'
                                                AND BuyTime <= '2016-10-08'
                                                AND MemberID NOT IN (
                                                SELECT  FriendID
                                                FROM    Basic..BD_MemberInviteFriends
                                                WHERE   MemberID IN ( SELECT    MemberID
                                                                      FROM      Trading..TC_DisableMember ) )
                                                AND MemberID = {0}
                                                GROUP BY MemberID , ProductTypeID;", memberId);
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
                var sql = string.Format(@"SELECT  PB.MemberID ,
                                                ProductTypeID ,
                                                SUM(BuyShares) BuyShares
                                        FROM    Basic..BD_MemberInviteFriends MIF
                                                INNER JOIN Trading..TC_ProductBuy PB ON MIF.FriendID = PB.MemberID
                                        WHERE   ProductTypeID IN ( 5, 6, 7, 8 )
                                                AND PB.Status = 1
                                                AND MIF.MemberID = {0}
                                                AND MIF.IsDelete = 0
                                                AND MIF.IsDelete = 0
                                                AND MIF.Status = 1", memberId);
                if (memberId != 27329 && memberId != 27331)
                {
                    sql += string.Format(" AND MIF.CreateTime >= '2016-09-28' AND MIF.CreateTime <= '2016-10-08'");
                }
                sql += "GROUP BY PB.MemberID ,ProductTypeID;";
                var reader = conn.ExecuteReader(sql);

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
        public void AddRecord(long memberId, string itemName, int itemType, decimal itemValue, string key, long sequnce)
        {
            using (var conn = GetDbConnection())
            {
                var param = new DynamicParameters();
                param.Add("@ItemName", itemName, DbType.String);
                param.Add("@ItemType", itemType, DbType.Int32);
                param.Add("@ItemValue", itemValue, DbType.Decimal);
                param.Add("@GameKey", key, DbType.String);
                param.Add("@MemberID", memberId, DbType.Int64);
                param.Add("@Sequnce", sequnce, DbType.Int64);
                conn.ExecuteScalar(@"INSERT INTO Other.dbo.OT_AwardItem
                                            ( ItemName ,
                                              ItemType ,
                                              ItemValue,
                                              GameKey,
                                              IsUsed ,
                                              ActivityID ,
                                              MemberID ,
                                              Sequnce
                                            )
                                    VALUES  ( @ItemName ,
                                              @ItemType ,
                                              @ItemValue,
                                              @GameKey,
                                              1 ,
                                              8 ,
                                              @MemberID ,
                                              @Sequnce
                                            )", param);
            }
        }

        /// <summary>
        /// 获奖记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DataTable GetRecord(int type, long mid, string key)
        {
            using (var conn = GetDbConnection())
            {
                var sql = string.Format(@"SELECT  LEFT(M.Phone, 3) + '****' + RIGHT(M.Phone, 4) Phone ,
                                                T.ItemName ,
                                                T.ItemType
                                        FROM    ( SELECT    ItemID ,
                                                            ItemName ,
                                                            MemberID ,
                                                            ItemType
                                                  FROM      Other..OT_AwardItem
                                                  WHERE     GameKey = '{0}'
                                                            AND IsDelete = 0
                                                            AND ItemType > 2
                                                  UNION
                                                  SELECT    ItemID ,
                                                            ItemName ,
                                                            MemberID ,
                                                            ItemType
                                                  FROM      Other..OT_AwardItem
                                                  WHERE     GameKey = '{0}'
                                                            AND IsDelete = 0
                                                            AND ItemType <= 2
                                                ) T
                                                INNER JOIN Basic..BD_Member M ON T.MemberID = M.ID
                                                ORDER BY T.ItemID DESC", key);
                if (type == 1)
                {
                    sql += string.Format("WHERE MemberID = {0}", mid);
                }
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
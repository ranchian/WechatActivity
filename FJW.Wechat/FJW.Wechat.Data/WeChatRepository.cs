using FJW.Data.MongoDb;
using FJW.Model.MongoDb;
using System;
using System.Collections.Generic;

using System.Linq.Expressions;


namespace FJW.Wechat.Data
{
    public class WeChatRepository
    {
        private readonly string _dbName;

        private readonly string _mongoHost;

        public WeChatRepository(string dbName, string mongoHost)
        {
            _dbName = dbName;
            _mongoHost = mongoHost;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public WeChatUserModel GetByOpenId(string openId)
        {
            return new Repository(_mongoHost, _dbName).GetEntity<WeChatUserModel>(it => it.OpenId == openId);
        }


        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> exp) where T : BaseModel
        {
            return new Repository(_mongoHost, _dbName).GetList(exp);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        public void Add<T>(T model) where T : BaseModel
        {
            new Repository(_mongoHost, _dbName).AddEntity(model);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        public void Update<T>(T model) where T : BaseModel
        {
            model.LastUpdateTime = DateTime.Now;
            new Repository(_mongoHost, _dbName).UpdateEntity(model);
        }

    }
}

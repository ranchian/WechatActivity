using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FJW.Data.MongoDb;
using FJW.Model.MongoDb;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Data
{
    /// <summary>
    /// 活动数据存储
    /// </summary>
    public class ActivityRepository
    {
        private readonly string _dbName;

        private readonly string _mongoHost;

        public ActivityRepository(string dbName, string mongoHost)
        {
            _dbName = dbName;
            _mongoHost = mongoHost;
        }

        /// <summary>
        /// 活动配置数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActivityModel GetActivity(string key)
        {
            return new Repository(_mongoHost, _dbName).GetEntity<ActivityModel>(it => it.Key == key);
        }

        /// <summary>
        /// 活动记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RecordModel GetById(string id)
        {
            return new Repository(_mongoHost, _dbName).GetEntity<RecordModel>(it => it.RecordId == id);
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> exp) where T : BaseModel
        {
            return new Repository(_mongoHost, _dbName).GetList(exp);
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> exp, int pageSize, int pageIndex, out int rowCount) where T : BaseModel
        {
            return new Repository(_mongoHost, _dbName).GetList(exp, pageSize, pageIndex, out rowCount);
        }

        public IEnumerable<T> QueryDesc<T, TKey>(Expression<Func<T, bool>> exp, Expression<Func<T, TKey>> orderExp, int pageSize, int pageIndex, out int rowCount) where T : BaseModel
        {
            return new Repository(_mongoHost, _dbName).GetListDesc(exp, orderExp, pageSize, pageIndex, out rowCount);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        public void Add<T>(T model) where T : BaseModel
        {
            new Repository(_mongoHost, _dbName).AddEntity(model);
        }

        public void AddMany<T>(IEnumerable<T> models) where T : BaseModel
        {
            new Repository(_mongoHost, _dbName).AddEntity(models.ToArray());
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
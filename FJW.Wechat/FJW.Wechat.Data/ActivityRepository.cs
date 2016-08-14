using System;


using FJW.Data.MongoDb;
using System.Linq.Expressions;
using FJW.Model.MongoDb;
using System.Collections.Generic;

namespace FJW.Wechat.Data
{
    /// <summary>
    /// 活动数据存储
    /// </summary>
    public class ActivityRepository
    {
        private readonly string _DbName;

        private readonly string _mongoHost;

        public ActivityRepository(string dbName, string mongoHost)
        {
            _DbName = dbName;
            _mongoHost = mongoHost;
        }

        public ActivityModel GetActivity(string key)
        {
            return new Repository(_mongoHost, _DbName).GetEntity<ActivityModel>(it => it.Key == key);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RecordModel GetById(string id)
        {
            return new Repository(_mongoHost, _DbName).GetEntity<RecordModel>(it => it.RecordId == id);
        }


        public IEnumerable<T> Query<T>( Expression<Func<T, bool>> exp) where T: BaseModel
        {
            return new Repository(_mongoHost, _DbName).GetList(exp);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model"></param>
        public void Add<T>(T model) where T: BaseModel
        {
            new Repository(_mongoHost, _DbName).AddEntity(model);            
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        public void Update(RecordModel model)
        {
            model.LastUpdateTime = DateTime.Now;
            new Repository(_mongoHost, _DbName).UpdateEntity(model);
        }

        
    }
}

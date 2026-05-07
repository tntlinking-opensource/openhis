using FrameworkBase.MultiOrg.Infrastructure;

using Newtouch.Infrastructure;
using Newtouch.Infrastructure.EF;
using System.Linq;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;

namespace Newtouch.Herp.Repository
{
    /// <summary>
    /// 损益原因
    /// </summary>
    public class KcSyyyRepo : RepositoryBase<DeptKcSyyyEntity>, IDeptKcSyyyRepo
    {
        public KcSyyyRepo(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// delete Syyy by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteSyyyById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return 0;
            var entity = FindEntity(p => p.Id == id);
            return entity == null ? 0 : Delete(entity);
        }

        /// <summary>
        /// submit syyy maintenance form
        /// </summary>
        /// <param name="kcSyyyEntity"></param>
        /// <param name="keyWord"></param>
        public int SubmitForm(DeptKcSyyyEntity kcSyyyEntity, string keyWord)
        {
            if (string.IsNullOrWhiteSpace(keyWord))
            {
                kcSyyyEntity.Create(true);
                return Insert(kcSyyyEntity);
            }
            var dbSyyy = FindEntity(p => p.Id == keyWord);
            if (dbSyyy == null) return 0;
            dbSyyy.sybz = kcSyyyEntity.sybz;
            dbSyyy.syyy = kcSyyyEntity.syyy;
            dbSyyy.zt = kcSyyyEntity.zt;
            dbSyyy.Modify();
            return Update(dbSyyy);
        }
    }
}

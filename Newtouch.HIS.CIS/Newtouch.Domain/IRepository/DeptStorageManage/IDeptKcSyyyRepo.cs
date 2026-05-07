using Newtouch.Domain.Entity;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Infrastructure.EF;

namespace Newtouch.Domain.IRepository.DeptStorageManage
{
    /// <summary>
    /// 损益原因
    /// </summary>
    public interface IDeptKcSyyyRepo : IRepositoryBase<DeptKcSyyyEntity>
    {
        /// <summary>
        /// delete Syyy by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int DeleteSyyyById(string id);

        /// <summary>
        /// submit syyy maintenance form
        /// </summary>
        /// <param name="deptKcSyyyEntity"></param>
        /// <param name="keyWord"></param>
        int SubmitForm(DeptKcSyyyEntity deptKcSyyyEntity, string keyWord);
    }
}
using Newtouch.Common.Model;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.ValueObjects;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IRepository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISysMedicineBaseRepo : IRepositoryBase<SysMedicineBaseEntity>
    {
        /// <summary>
        /// 根据组织机构获取药品信息列表
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        IList<SysMedicineBaseEntity> GetValidListByOrg(string OrganizeId, string keyword = null);

        /// <summary>
        /// 获取医保字典库药品List
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        IList<GzybybItemCodeVO> GetYbMedicineList(string OrganizeId, string keyword = null);
        /// <summary>
        /// 查询医保姓名表信息
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        IList<GzybNameCodeVO> GetYbName(string OrganizeId ,string lx, string keyword=null);

        IList<GzxnhybItemCodeVO> GetYbXNHMedicineList(string keyword = null);

        /// <summary>
        /// 查询基础表最后一个id
        /// </summary>
        /// <returns></returns>
        int GetBaseLastYpId();
    }
}

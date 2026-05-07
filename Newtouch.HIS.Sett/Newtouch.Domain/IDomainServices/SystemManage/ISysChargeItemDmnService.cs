using Newtouch.Core.Common;
using Newtouch.HIS.Domain.ValueObjects.SystemManage;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISysChargeItemDmnService
    {
        /// <summary>
        /// 项目检索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="xmzlfzlbz">项目的治疗非治疗标志：true 治疗项目 false 非治疗项目 null都包含</param>
        /// <param name="mzzybz"></param>
        /// <returns></returns>
        IList<SysChargeItemVO> SelectSearch(string keyword, string orgId, bool? xmzlfzlbz = true, string mzzybz = null, string dlCode = null, bool isContansChildDl = true);

        /// <summary>
        /// 项目And药品检索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="zlfzlbz">项目的治疗非治疗标志：true 治疗项目 false 非治疗项目 null都包含</param>
        /// <param name="mzzybz"></param>
        /// <returns></returns>
        IList<SysChargeItemAndMedicineVO> SelectItemAndMedicineSearch(string keyword, string orgId, bool? zlfzlbz = true, string mzzybz = null);

        /// <summary>
        /// 获取大类
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<SysChargeClassVO> GetSFDLList(string orgId);

        /// <summary>
        /// 收费模板页面index list
        /// </summary>
        /// <returns></returns>
        IList<SysChargeTemplateGridVO> Search(Pagination pagination, string keyword, string organizeId, string yxbz);

        /// <summary>
        /// 获取用法
        /// </summary>
        /// <returns></returns>
        IList<SysUsageVO> GetYF();
    }

}

using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity.SystemManage;
using Newtouch.HIS.Domain.ValueObjects;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISysChargeItemDmnService
    {
        /// <summary>
        /// 收费项目
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="Pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        IList<SysChargeItemVO> GetPagintionList(string orgId, Pagination pagination, string sfdl, string zt,string keyword = null);
        /// <summary>
        /// 获取医保性质字典
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<ShybBrxzblVo> Getybbxbldata(string keyword, string orgId);


        void SaveYbblValue(List<Sh_YbfyxzblEntity> entity, string xmbm, string xmmc,string orgId,string CreatorCode);
    }
}

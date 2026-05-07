using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.Entity;
using Newtouch.Domain.ValueObjects;
using System.Collections.Generic;

namespace Newtouch.Domain.IDomainServices
{
    public interface IInspectionTemplateDmnService
    {
        /// <summary>
        /// 根据type 查询检验检查模板
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jyjcmbLx"></param>
        List<InspectionTemplateTreeVO> GetTemplateListByType(string orgId, int jyjcmbLx);

        /// <summary>
        /// 根据mbId 查询模板下组套项目
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mbId"></param>
        List<GPackageTreeDetailVO> GetTemplateDetailByMbId(string orgId, string mbId, string ztKeyword);

        /// <summary>
        /// 根据ztId查询组套下的收费项目
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="ztId"></param>
        /// <returns></returns>
        List<GPackageZTTreeDetailVO> GetGPackageDetailByZtId(string orgId, string ztId,string mbId=null);
        /// <summary>
        /// 获取组套信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="ztId"></param>
        /// <returns></returns>
        List<GPackageZTTreeDetailVO> GetGPackageInfoByZtId(string orgId, string ztId, string mbId);

        /// <summary>
        /// 模板详情
        /// </summary>
        /// <param name="ztId"></param>
        InspectionTemplateDetailBO GetTemplateDetail(string mbId, string orgId);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ztobj"></param>
        /// <param name="ztxmlist"></param>
        void SaveData(InspectionTemplateEntity mbobj, List<TemplateGroupPackageEntity> mbztlist);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ztId"></param>
        void DeleteData(string mbId);
    }
}

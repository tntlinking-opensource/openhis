using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtouch.Domain.Entity.Outpatient;

namespace Newtouch.Domain.IDomainServices
{
    public interface IMRecordTemplateDmnService
    {
        List<MRTemplateListBO> SelectTemplateList(int mblx, string orgId,string deptCode, string userCode);
        /// <summary>
        /// 查询模板明细
        /// </summary>
        /// <param name="mbId"></param>
        MRTemplateBO SelectTemplateDetailByMbId(string mbId, string orgId);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="blmbObject"></param>
        /// <param name="zdList"></param>
        /// <param name="cfDto"></param>
        void SaveData(MRTemplateEntity blmbObject, List<WMDiagnosisHtmlVO> zdList, List<TCMDiagnosisHtmlVO> zyzdList,List<CfTemplateGroupPackageEntity> mbztList);


        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="mbId"></param>
        void DeleteTemplate(string mbId);
    }
}

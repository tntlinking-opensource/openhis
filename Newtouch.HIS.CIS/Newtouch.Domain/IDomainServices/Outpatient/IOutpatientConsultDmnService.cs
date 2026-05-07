using Newtouch.Core.Common;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Domain.ValueObjects.Outpatient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.IDomainServices.Outpatient
{
    public interface IOutpatientConsultDmnService
    {
        IList<OutpatientConsultVO> GetExpertInfo(Pagination pagination, string ksCode, string keyword, string ghrq, string orgId);
        IList<OutpatientConsultVO> GetNormalInfo(Pagination pagination, string ksCode, string keyword, string ghrq, string orgId);
        List<RehabVO> SelectConsultList(string ksCode, string keyword, string orgId);
        List<OutpatientConsultCountVO> getConsultCount(string ksCode, string orgId,string ghrq);
        IList<OutpatientConsultVO> GetConsultCall(string ksCode, string keyword, string ghrq, string orgId);
        IList<RehabVO> GetDeptListByKeyValue(Pagination pagination, string orgId, string keyValue);
        IList<OutpatientConsultDoctorVO> GetConsultDoctorByDept(string orgId, string ksCode);
        OutpatientConsultInfoVO GetConsultInfo(string orgId, string zsCode,string ghrq);
		OutpatientConsultVO GetConsultnext(string orgid,string ghnm,string mzh);
		int UpdataZSinsert(string mzh,string rygh,string orgid);
        /// <summary>
        /// 叫号修改状态
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="calledstu"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
		int UpdatePatient(string mzh,int calledstu, string orgid);
        /// <summary>
        /// 验证叫号状态
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
		int ISfalgPatient(string mzh,string orgid);
        /// <summary>
        /// 机构名称
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        string GetOrgName(string orgId);
	}
}

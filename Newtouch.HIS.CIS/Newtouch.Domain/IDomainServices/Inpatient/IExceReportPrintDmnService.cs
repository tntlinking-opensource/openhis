using Newtouch.Core.Common;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Inpatient;
using System;
using System.Collections.Generic;

namespace Newtouch.Domain.IDomainServices
{
    public interface IExceReportPrintDmnService
    {
        /// <summary>
        /// 获取病区发药患者树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        IList<InpWardPatTreeVO> GetPatTree(string orgId, string zyzt, string keyword);

        /// <summary>
        /// 查询医嘱详情
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="zxsj"></param>
        /// <param name="yzlx"></param>
        /// <returns></returns>
         IList<ExecReportReportRightVO> GetExecDetailGridJson(string orgId, string zyh, DateTime zxsj,string zxdlb,string yzxz);
        IList<ExecReportReportRightVO> QueryExecDetailGridJson(Pagination pagination, string orgId, string zyh, DateTime zxsj, DateTime zxsjend, string zxdlb,string yzxz);

        IList<InpWardPatTreeVO> GetPatCenterTree(string orgId, string zyzt, string keyword);
    }
}
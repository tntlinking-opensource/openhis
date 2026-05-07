using System.Collections.Generic;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.ValueObjects.PatientManage;
using System;
using Newtouch.HIS.Domain.ValueObjects.HospitalizationManage;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.DTO.InputDto.OutpatientAccounting;
using Newtouch.HIS.Domain.DTO.OutputDto;
using Newtouch.Infrastructure.EF;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBookkeepInHosDmnService
    {
        /// <summary>
        /// 查询收费项目模板
        /// </summary>
        /// <param name="ks"></param>
        /// <returns></returns>
        List<ChargeItemTemplateVO> GetChargeTemplate(string ks, string orgid);


        /// <summary>
        /// 根据收费项目编号获取模板内容
        /// </summary>
        /// <param name="sfxmbh"></param>
        /// <returns></returns>
        List<TemplateContentVO> GetChargeItemContent(string sfmbbh, string orgId);

        /// <summary>
        /// 根据住院号 查询 记账计划
        /// </summary>
        /// <param name="zyhList"></param>
        /// <param name="zxrq"></param>
        IList<HospAccountingPlanVO> BeenAccountingPlanQuery(string orgId, string zyh);

        /// <summary>
        /// 保存记账计划
        /// </summary>
        /// <param name="jzjh"></param>
        /// <param name="jhmxEntityList"></param>
        /// <param name="ysEntityList"></param>
        void SaveAccountingPlan(HospAccountingPlanEntity jzjh, List<HospAccountingPlanDetailEntity> jhmxEntityList, List<string> updateToStopJzjhxmIdList);

        /// <summary>
        /// 获取机构病人列表（当前医生、存在未执行记账计划）
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<PendingExecutionPatientVO> GetPendingExecutionPatientVOList(string orgId,string kscode);

        /// <summary>
        /// 获取机构门诊病人列表（当前医生、存在未执行记账计划）
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<PendingExecutionPatientVO> GetOutPendingExecutionPatientVOList(string orgId,string kscode);

        /// <summary>
        /// 待执行记账计划 查询
        /// </summary>
        /// <param name="zyhList"></param>
        /// <param name="zxrq"></param>
        IList<HospAccountingPlanWaitingExeVO> WaitingAccountingPlanQuery(string orgId, string zyhStr, string gh, string zxks, bool isRehabDoctor);

        /// <summary>
        /// 待执行门诊记账计划 查询（仅收费项目，不包含药品）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="Str"></param>
        /// <param name="zxrq"></param>
        /// <param name="ysCode"></param>
        /// <returns></returns>
        IList<HospAccountingPlanWaitingExeVO> WaitingOutAccountingPlanQuery(string orgId, string Str,string gh, string zxks, bool isRehabDoctor);

        /// <summary>
        /// 执行记账计划，生成计费
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zxItem"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq">治疗日期</param>
        void ExecuteAccountingPlan(string orgId, IList<zxGirdDto> zxItem, string curOprCode, string curOprDeptCode, DateTime zxrq, out Dictionary<string, string> jfbStr);

        /// <summary>
        /// 执行记账计划，生成计费
        /// </summary>
        /// <param name="jzjhmxIdList">jzjhmxId列表</param>
        void ExecuteOutAccountingPlan(string orgId, IList<zxGirdDto> zxItem, string curOprCode, string curOprDeptCode, DateTime zxrq);
        /// <summary>
        /// 停止记账计划
        /// </summary>
        /// <param name="jzjhmxIdList">jzjhmxId列表</param>
        void StopAccountingPlan(IList<string> mxIdList, DateTime stopDate, string curOprCode, string curOprDeptCode);

        /// <summary>
        /// 取消预停止
        /// </summary>
        /// <param name="jzjhmxIdList">jzjhmxId列表</param>
        void CancelPreStopAccountingPlan(IList<string> mxIdList, string curOprCode, string curOprDeptCode);

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zxzt"></param>
        /// <param name="orgId"></param>
        /// <param name="zls"></param>
        /// <param name="cxzt"></param>
        /// <param name="jzjhmxId"></param>
        /// <returns></returns>
        List<AccountingExecuteVO> SelectAccountingExecuteList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, int? zxzt, string orgId
            , string zls, string cxzt, string jzjhmxId, string sfzt);

        /// <summary>
        /// 记账计划查询
        /// </summary>
        List<AccountingPlanVO> SelectAccountingPlanList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj
            , int? zxzt, string orgId, int? yzxz);

        /// <summary>
        /// 门诊计划执行查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zxzt"></param>
        /// <param name="orgId"></param>
        /// <param name="zls"></param>
        /// <param name="cxzt"></param>
        /// <param name="jzjhmxId"></param>
        /// <param name="sfzt"></param>
        /// <returns></returns>
        List<AccountingExecuteVO> SelectOutAccountingExecuteList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, int? zxzt, string orgId
          , string zls, string cxzt, string jzjhmxId, string sfzt);
        /// <summary>
        /// 查询记账计划详情
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<AccoutingPlanDetailVO> GetFormPlan(Pagination pagination, string jzjhmxId, string orgId, string from);

        /// <summary>
        /// 查询记账计划执行详情
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        AccoutingPlanDetailVO GetFormPlan(string zxId, string orgId, string from);
        /// <summary>
        /// 根据住院号 获取所有计费（项目/药品）总金额
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        decimal? GetJfZjeByZyh(string zyh, string orgId);

        /// <summary>
        /// 保存住院计费
        /// </summary>
        void SaveInpatientAccounting(string zyh, IList<InpatientAccountingItemDto> xmList, string orgId, string curUserCode);

        /// <summary>
        /// 撤销执行入库
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="zxbh"></param>
        /// <param name="orgId"></param>
        /// <param name="from"></param>
        void RevokeExec(string jzjhmxId, string zxbh, string orgId, string from);

        /// <summary>
        /// 更改执行的治疗师和执行时间
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="zxbh"></param>
        /// <param name="orgId"></param>
        /// <param name="from"></param>
        void UpdateExec(cxzxGirdDto zxItem, string from, string orgId);

        #region 住院记账，和门诊记账同个页面版本

        /// <summary>
        /// 更新住院计划 剩余次数，同时停止需要停止的计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgId"></param>
        /// <param name="jzjhmxIdList"></param>
        void updateHospitaljzjh(string orgId, IList<string> jzjhmxIdList);

        /// <summary>
        /// 判断是否需要停止计划
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        bool StopOutpatientjzjh(string jzjhmxId, string orgId);

        /// <summary>
        /// 执行完的康复项目同步到秦皇岛中间库
        /// </summary>
        /// <param name="jfbStr"></param>
        /// <param name="orgId"></param>
        void SyncInpatientToInterfaceBasic(Dictionary<string, string> jfbStr, string orgId, DateTime? zxrq, string rygh);
        #endregion

        #region 费用结算 出院结算

        /// <summary>
        /// 获取最后一次中途结算 结束结算日期
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        DateTime? GetLastValidMidwaySettTime(string zyh, string orgId);

        DateTime? GetLastValidSettTime(string zyh, string orgId);

        /// <summary>
        /// 根据住院号 获取未结明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<HosFeeItemInfoDto> GetFeeItemPaginationList(Pagination pagination, string zyh, string orgId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 中途结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="jsItemList"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        void MidwaySettlement(string zyh, string orgId, DateTime startTime, DateTime endTime, int? expectedCount = null);

        /// <summary>
        /// 撤销最后一次中途结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        void CancelTheLastMidwaySett(string zyh,string zffs1, string orgId, out decimal refoundmoney);

        /// <summary>
        /// 根据 中途结算 出院结算
        /// 中途结算 已 结完
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        void DischargeSettlement(string zyh, string orgId, DateTime cyrq);

        /// <summary>
        /// 根据住院号 获取时间段内的 项目药品计费明细（考虑退费的）
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<HosFeeItemInfoDto> GetChargeItemPaginationList(Pagination pagination, string zyh, string orgId, DateTime startTime, DateTime endTime);

        #endregion

        #region 住院记账业务
        string getzfxzById(string sfxmCode, string orgId);

        /// <summary>
        /// 住院记账保存
        /// </summary>
        string SaveFee(List<HospItemBillingEntity> xmjfbEntitylist, List<HospDrugBillingEntity> ypjfbEntitylist);
        List<HospItemFeeVO> Verification(string xmCodes, string orgId, string ks);

        #endregion
    }
}

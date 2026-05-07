using System;
using Newtouch.HIS.Domain.ValueObjects;
using System.Collections.Generic;
using System.Data;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;

namespace Newtouch.HIS.Domain.IDomainServices
{
    public interface IRefundDmnService
    {
        List<MZJSInfo> GetFPHByKh(string kh, string startTime, string endTime, string fph);

        List<MZJS> GetMZJSByJsnm(int jsnm);
        List<GridViewMx> GetMz_xmRecordsByJsnm(int jsnm);
        List<GridViewMx> GetMz_ghxmRecordsByJsnm(int jsnm);
        List<GridViewMx> GetMz_cfmxRecordsByJsnm(int jsnm);
        decimal getSysl(int patid, int ghnm);
        // DataTable GetMzJszffs(int jsnm);
        List<OutpatientRegistNonAttendanceEntity> getGhjsByGhnm(int tmpGhnm);
        SysPatientNatureEntity GetBrxzByBrbh(string brxz);
        List<SysPatientChargeWaiverEntity> GetXT_brsfjm(string parmBrxz);
        List<OutpatientRegistEntity> getGhs(List<int> ghnmList);
        SysPatientBasicInfoEntity GetBrjbxxByPatid(int patid);
        int GetPrimaryKeyByTableName(string tableName);

        List<SysPatientChargeAlgorithmEntity> getMzActive();
        DataTable GetMzJszffsForJsnm(int jsnm);
        int getTFForFPH(int jsnm, string fph);
        List<OutpatientSettlementDetailEntity> getMzJsMx(string jslx, int jsnm);
        List<OutpatientSettlementPaymentModelEntity> GetJszffsByJsnm(int jsnm);
        OutpatientSettlementEntity GetMZJSFromMz_jsByJsnm(int jsnm);

        /// <summary>
        /// 插入退费剩余部分 mz_js mz_jsmx mz_jszffs mz_jsdl 数据库操作
        /// </summary>
        /// <param name="js"></param>
        /// <param name="listMzJsMx"></param>
        /// <param name="jszffsArray"></param>
        /// <param name="jsdlList"></param>
        /// <param name="jsnmEx"></param>
        /// <returns></returns>
        bool InsertRemainFromReturns(OutpatientSettlementEntity js, List<OutpatientSettlementDetailEntity> listMzJsMx, List<OutpatientSettlementPaymentModelEntity> jszffsArray, List<OutpatientSettlementCategoryEntity> jsdlList);
        DataTable GetMz_cfmxRecords(int jsnm);

        /// <summary>
        /// 全退  mz_js mz_jsmx mz_ghth
        /// </summary>
        /// <param name="mzJs"></param>
        /// <param name="mzJsmxList"></param>
        /// <param name="ghjsDt"></param>
        /// <returns></returns>
        bool mzJsFullBack(OutpatientSettlementEntity mzJs, List<OutpatientSettlementDetailEntity> mzJsmxList, List<OutpatientRegistNonAttendanceEntity> ghjsDt);

        bool CheckFPH(string fph);


        #region GRS门诊退费

        /// <summary>
        /// 系统病人信息 分页数据
        /// </summary>
        /// <param name="pagination">分页信息</param>
        /// <param name="blh">病历号</param>
        /// <param name="xm">姓名</param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<OutpatAccInfoDto> GetBasicInfoSearchList(Pagination pagination, string blh, string mzh, string xm,string kssj,string jssj, string orgId, string usercode, string jiuzhenbiaozhi);

        /// <summary>
        /// 病人管理查询病人信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="blh"></param>
        /// <param name="xm"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<OutpatAccInfoDto> GetBasicInfoSearchListInRegister(Pagination pagination, string blh, string xm, string orgId,string zjh,string zjlx);

        /// <summary>
        /// 病人门诊登记，浮层查询病人信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<OutpatAccInfoDto> GetBasicInfoSearchListInRegister(string keyword, string orgId);

        List<PatInfoQuery> Getjzjsxx(string type, string keyword, string grbh, string orgId);
        /// <summary>
        /// 根据病历号获取结算信息
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<MZJSInfo> GetjsInfoByblh(string blh, string startTime, string endTime, string orgId);

        /// <summary>
        /// 按结算内码查门诊项目
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        /// 门诊项目明细
        List<GridViewMx> GetMz_xmDetailByJsnm(int jsnm, string orgId);


        /// <summary>
        /// 根据结算内码获取结算项目信息
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<MZJS> GetMZJSByJsnmInAcc(int jsnm, string orgId);

        List<GridViewMx> GetGridViewMx(int jsnm, string orgId);

        /// <summary>
        /// 全退时更新医保次数
        /// </summary>
        /// <param name="jzrqList"></param>
        /// <param name="IsReturnAll"></param>
        /// <param name="orgId"></param>
        /// <param name="patid"></param>
        void Minusybcs(DateTime jzrqList, bool IsReturnAll, string orgId, int patid);

        #endregion

        /// <summary>
        /// 门诊退费 结算 主记录 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        IList<OutPatientRefundableJsVO> RefundableJsQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh);
        /// <summary>
        /// 贵安结算退费时查询结算信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        IList<OutPatientRefundableGuiAnJsVO> RefundableGuiAnJsQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh);
        
        /// <summary>
        /// 门诊退费 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        IList<OutPatientRefundableVO> RefundableQuery(string orgId, int jsnm);

        /// <summary>
        /// 门诊/住院计划录入 患者检索筛选
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="usercode"></param>
        /// <param name="jiuzhenbiaozhi"></param>
        /// <returns></returns>
        List<OutpatAccInfoDto> GetBasicInfoSearchList(string keyword, string orgId, string usercode, string jiuzhenbiaozhi, string from="mz", string brzybzType = null);


		#region 重庆医保
	    IList<OutPatChongQingVO> RefundableChongQingQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh);
        #endregion
        List<OutpatAccInfoDto> GetZyMzYjjPatientSearch(string keyword, string orgId, string type);

    }
}

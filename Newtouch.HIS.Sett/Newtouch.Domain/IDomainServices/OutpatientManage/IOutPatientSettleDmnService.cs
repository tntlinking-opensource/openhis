
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.OutpatientManage;
using System;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 门诊记账
    /// </summary>
    public interface IOutPatientSettleDmnService
    {
        /// <summary>
        /// 查询当日已挂号
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        IList<OutPatientRegInfoVO> GetCurrentDayRegList(string orgId, string creatorCode);

        /// <summary>
        /// 门诊退号（未结挂号）
        /// </summary>
        /// <param name="mzh"></param>
        /// <returns></returns>
        void UnSettedRegBackNum(string mzh, string orgId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ghxm"></param>
        /// <param name="isZzjm"></param>
        /// <param name="isCkf"></param>
        /// <param name="isGbf"></param>
        /// <returns></returns>
        GHZGroupAndFeesVO GetOutpatientFees(string ghxm, string zlxm, bool isCkf, bool isGbf, string orgId = null);
        /// <summary>
        /// 收费项目组合费用
        /// </summary>
        /// <param name="ghxm"></param>
        /// <param name="zlxm"></param>
        /// <param name="isCkf"></param>
        /// <param name="isGbf"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        GHZGroupAndFeesVO GetOutpatientFeesbyGroup(string ghxm, string zlxm, bool isCkf, bool isGbf, string orgId = null);
        /// <summary>
        /// 门诊挂号保存
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="patid"></param>
        /// <param name="kh"></param>
        /// <param name="ghly"></param>
        /// <param name="mjzbz"></param>
        /// <param name="ks"></param>
        /// <param name="ys"></param>
        /// <param name="ksmc"></param>
        /// <param name="ysmc"></param>
        /// <param name="ghxm"></param>
        /// <param name="zlxm"></param>
        /// <param name="fph"></param>
        /// <param name="sfrq"></param>
        /// <param name="isCkf"></param>
        /// <param name="isGbf"></param>
        /// <param name="jzxh"></param>
        /// <param name="ghpbId"></param>
        /// <param name="feeRelated"></param>
        /// <param name="brxz">病人性质由页面传过来，因为医保患者可以挂自费</param>
        /// <param name="ybjsh">医保端结算号</param>
        /// <param name="mzh">门诊号</param>
        /// <param name="mzh">门诊号</param>
        /// <param name="mzyyghId">门诊预约挂号ID 对应Newtouch_CIS.dbo.mz_yygh.Id</param>
        /// <returns></returns>
        void Save(string orgId, int patid, string kh, string ghly, string mjzbz,
            string ks, string ys, string ksmc, string ysmc, string ghxm, string zlxm
            , string fph, DateTime? sfrq, bool isCkf, bool isGbf
            , int jzxh
            , int ghpbId
            , OutpatientSettFeeRelatedDTO feeRelated, string brxz
            , string ybjsh, string mzh
            , string jzyy, string jzid, string jzlx, string bzbm, string bzmc, string mzyyghId,string isjm, out object newJszbInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gh"></param>
        /// <param name="ssk"></param>
        /// <param name="fph"></param>
        /// <param name="isCkf"></param>
        /// <param name="isGbf"></param>
        /// <returns></returns>
        long Book(OutpatientRegistEntity gh, decimal ssk, string fph, bool isCkf, bool isGbf);

        /// <summary>
        /// check患者是否有待结费用挂号记录（不区分某次挂号）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="patid"></param>
        /// <returns></returns>
        bool CheckHasUnsettedRegister(string orgId, int patid);

        /// <summary>
        /// check患者是否有未结费用记录（不区分某次挂号）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="patid"></param>
        /// <returns></returns>
        bool CheckHasUnsetted(string orgId, int patid);

        /// <summary>
        /// 生成唯一门诊号、就诊序号
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="ghlx"></param>
        /// <param name="ks"></param>
        /// <param name="ys"></param>
        /// <returns></returns>
        Tuple<short, string> GetMzhJzxh(int patid, string ghpbId, string ks, string ys, string OrganizeId, string UserCode);
        /// <summary>
        /// 根据已知jzxh 挂号日期 得门诊号
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="ghpbId"></param>
        /// <param name="ks"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="UserCode"></param>
        /// <param name="jzxh"></param>
        /// <param name="jzrq"></param>
        /// <returns></returns>
        string GetRegMzh(int patid, string ghpbId, string ks, string OrganizeId, string UserCode, int jzxh,
            string jzrq);

        /// <summary>
        /// 生成chongqing就诊序号门诊号
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="ghpbId"></param>
        /// <param name="ks"></param>
        /// <param name="ys"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        Tuple<short, string> GetCQMzhJzxh(int patid, string ghpbId, string ks, string ys, string OrganizeId, string UserCode,string mjzbz,string QueueNo,string OutDate);
        CqybGjbmInfoVo GetDepartmentDoctorIdC(string orgId, string ks,string ys);
        List<GhJzInfo> GetRegListJson(Inparameter inparameter, string orgid);
        /// <summary>
        /// 获取医保就诊登记所需信息
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgId"></param>
        CqybGjbmInfoVo GetYbjzdjVo(string mzh,string orgId);
        /// <summary>
        /// 获取医院行政区划代码来判断是否异地医保
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        string GetOrgXzqh(string orgId);
    }
}

using System.Collections.Generic;
using System.Data;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto.Outpatient;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Outpatient;
using Newtouch.Domain.ViewModels;

namespace Newtouch.Domain.IDomainServices
{
    public interface IMedicalRecordDmnService
    {
        /// <summary>
        /// 历史病历树
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        List<MedicalRecordTreeVO> GetHistoryMedicalRecordTree(string blh, int queryDate, string orgId);

        /// <summary>
        /// 详情树节点内容 （根据jzId查询病历和处方内容）
        /// </summary>
        /// <param name="jzId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        NodeContentDto SelectNodeContent(string jzId);

        /// <summary>
        /// 详情树节点内容 （根据MbId查询病历和处方内容）
        /// </summary>
        /// <param name="jzId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        NodeContentDto SelectNodeContentByBlmb(string mbId,string orgId);
        
        /// <summary>
        /// 保存就诊记录、诊断、病历、处方、处方明细
        /// </summary>
        /// <param name="jzObject"></param>
        /// <param name="xydList"></param>
        /// <param name="zyzdList"></param>
        /// <param name="blObject"></param>
        /// <param name="cfDto"></param>
        /// <param name="operatorCode"></param>
        /// <param name="addedYpCfList"></param>
        /// <param name="updatedYpCfList"></param>
        void SaveMedicalRecord(TreatmentEntity jzObject,
            List<WMDiagnosisHtmlVO> xydList,
            List<TCMDiagnosisHtmlVO> zyzdList,
            MedicalRecordEntity blObject,
            List<PrescriptionDTO> cfDto,
            List<CFZDDiagnosisHtmlVO> cfzdlist,
            string operatorCode,
            out List<string> addedYpCfList, out List<string> updatedYpCfList);

        /// <summary>
        /// 作废病历 包括病历诊断 处方 处方明细
        /// </summary>
        void ObsoleteMedicalRecord(string jzId, string orgId);

        /// <summary>
        /// 作废his单次就诊的所有处方
        /// </summary>
        void ObsoleteAllPresToHIS(string jzId, string orgId);

        /// <summary>
        /// 发送单次就诊的所有处方（但不推已收费的）、或指定处方
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgId"></param>
        /// <param name="opr">操作员</param>
        /// <param name="cfId">指定时发送单一处方，否则发送单次就诊的所有处方（但不推已收费的）</param>
        bool sendPresToHis(string mzh, string orgId, string opr, string cfId = null);

        bool sendPresToPDS(string orgId, string operatorCode, string mzh, string cfId = null, List<string> addedYpCfList = null, List<string> updatedYpCfList = null);

        /// <summary>
        /// 更新pds已有处方
        /// </summary>
        /// <param name="jzObject"></param>
        /// <param name="cfh"></param>
        /// <param name="cfId"></param>
        /// <param name="operatorCode"></param>
        /// <param name="rpDetail"></param>
        /// <param name="ypyfList"></param>
        /// <returns></returns>
        string UpdateRpToPds(TreatmentEntity jzObject,
            string cfh,
            string cfId,
            string operatorCode,
            List<PrescriptionDetailEntity> rpDetail,
            IList<SysMedicineUsageVEntity> ypyfList);

        /// <summary>
        /// 发送新处方至药房药库
        /// </summary>
        /// <param name="jzObject"></param>
        /// <param name="cfh"></param>
        /// <param name="cfId"></param>
        /// <param name="operatorCode"></param>
        /// <param name="rpDetail"></param>
        /// <param name="ypyfList"></param>
        /// <returns></returns>
        string SendNewRpToPds(TreatmentEntity jzObject,
            string cfh,
            string cfId,
            string operatorCode,
            List<PrescriptionDetailEntity> rpDetail,
            IList<SysMedicineUsageVEntity> ypyfList);

        /// <summary>
        /// 作废处方（并推送给HIS）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mzh"></param>
        /// <param name="cfId"></param>
        void cancelSinglePres(string orgId, string mzh, string cfId
            , out int apicflx, out string apicfh, out bool isAlertinherit);

        void delDzcf(string orgId, string mzh, string cfId
            , out int apicflx, out string apicfh, out bool isAlertinherit);
        /// <summary>
        /// 作废HIS单张
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mzh"></param>
        /// <param name="cfId"></param>
        void cancelSinglePresToHIS(string orgId, string mzh, string cfId
            , int apicflx, string apicfh);

        /// <summary>
        /// 作废PDS单张
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mzh"></param>
        /// <param name="cfId"></param>
        void cancelSinglePresToPDS(string orgId, string operatorCode, string mzh, string cfId
            , int apicflx, string apicfh);

        /// <summary>
        /// 作废处方时获取
        /// </summary>
        /// <param name="cfId"></param>
        /// <returns></returns>
        List<PrescriptionHtmlVO> GetzfcfJson(string cfId);
        /// <summary>
        /// 获取项目或药品信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="cxlx"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        SqtxXmYpInfoVO GetXmYpInfo(string code, string cxlx, string orgId);

        IList<OutpatientLogQueryVO> GetLogGridList(Pagination pagination, string keyword, string kssj, string jssj, string jzysgh, string orgId, string hznl, string xtxy, string qbdc);

        IList<OutpatientLogQueryVO> GetLogGridList(string keyword, string kssj, string jssj, string jzysgh, string orgId, string hznl, string xtxy, string qbdc);

        DataTable DTXD_UploadData(YCYL_DtxdscDTO dto);
        DataTable DTXD_SqdUploadData(YCYL_DtxdSqdDTO dto);
        /// <summary>
        /// 获取动态心电名称
        /// </summary>
        /// <param name="mzh"></param>
        /// <returns></returns>
        YCYL_DtxdscDTO GetJcDtxdmc(string mzh, string orgId);
        YCYL_DtxdSqdDTO GetJcDtxdSqd(string mzh, string orgId, string sqdh);
        DataTable chakanmingxidiaoyong(string kh);

        /// <summary>
        /// 远程医疗心电申请
        /// </summary>
        /// <param name="PlaceerOrderNo"></param>
        DataTable xindianshenqing(string patid, string orderno);

        /// <summary>
        /// 远程检验申请
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="jzlsh"></param>
        /// <returns></returns>
        DataTable jianyanshenqing(string patid, string jzlsh, string OrgId);

        /// <summary>
        /// 远程医疗影像申请
        /// </summary>
        /// <param name="hispatientid"></param>
        /// <param name="name"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        DataTable yingxiangshenqing(string patid, string AccessionNumber);

        /// <summary>
        /// 远程医疗心电调阅
        /// </summary>
        /// <param name="PlaceerOrderNo"></param>
        DataTable xindiandiaoyue(string kh, string orderno);

        List<string> GetCfhBycflx(string orgId, int cflx, string mzh);

        List<PrescriptionEntity> GetcfByJzId(string orgId, string jzId);

        /// <summary>
        /// 获取最后诊查费
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        lastzcfDto GetLastzcf(string blh, string orgId);
        /// <summary>
        /// 获取组织机构信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        OrganizationData GetOrgInfo(string orgId);
        /// <summary>
        /// 处方事前提醒
        /// </summary>
        /// <param name="xyzdList"></param>
        /// <param name="zyzdList"></param>
        /// <param name="strCfList"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        string GetqhdCfSqtxData(List<WMDiagnosisHtmlVO> xyzdList, List<TCMDiagnosisHtmlVO> zyzdList, string strCfList, string mzh, out string jlId, string orgId, string rygh, string userName);

        List<LisReportSqdhValueVo> GetLisSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj);
        List<PacsReportSqdhValueVo> GetPacsSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj);
        IList<LisReportSqdyczValueVo> GetLisSqdyczData(Pagination pagination, string orgId, string zymzh, string type, string ztmc, string ycbz, string sqdht);

        void SyncToPdsQuery(string operatorCode, string ghrq);

        List<PrescriptionDetailVO> Getsfmbxm(string orgId, string sfmb, string sfmbmc);

        /// <summary>
        /// 代煎项目内容
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        TCMDjXMVO GetBindTCMDj(string OrganizeId);


        List<OutPatientRegistrationInfoDTO> Getdjzlist(Pagination pagination, string orgid, string ksCode, string ysgh, string mjzbz, string jiuzhenbz);

        IList<MedicineInfoVO2> GetMedicineInfoList(Pagination pagination, string xmbm, string xmmc, string ck_kc, string orgId);

        int CountLISztmz(string orgId, string jzId);

        string GetYpjx(string code, string orgId);
    }
}

using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Domain.ValueObjects;
using Newtouch.EMR.Domain.BusinessObjects;
using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.ValueObjects.MedicalRecord;
using System.Collections.Generic;

namespace Newtouch.EMR.Domain.IDomainServices
{
    public interface IMedicalRecordDmnService
    {
        medicalRecordVO GetMedicalRecord(string blid, string bllx);

        void MedicalRecordSave(medicalRecordVO medicalRecord, ZymeddocsrelationEntity Entity);
        void LockRecord(string blid, string bllx, string OrganizeId, string UserCode, int isLock);
        /// <summary>
        /// 病历统一存储
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="bllx"></param>
        /// <param name="mbbh"></param>
        /// <param name="path"></param>
        /// <param name="BLMC"></param>
        /// <param name="user"></param>
        /// <param name="zyh"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        string BL_Save(string orgId, string bllx, string mbbh, string path, string BLMC, OperatorModel user, string zyh, string mzh = null, string doctype = null);
        /// <summary>
        /// 获取统一存储病历
        /// </summary>
        /// <param name="blid"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        medicalRecordVO GetMedicalRecordbyId(string blid, string bllx);
        /// <summary>
        /// 门诊病历保存
        /// </summary>
        /// <param name="medicalRecord"></param>
        /// <param name="Entity"></param>
        void MedicalRecordSaveMz(medicalRecordVO medicalRecord, MzmeddocsrelationEntity Entity);

        void TbDataSavebyWriter(IList<BlHljlData> data, string ksrq, string jsrq);

        List<LisReportSqdhValueVo> GetLisSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj);//检验项目
        List<PacsReportSqdhValueVo> GetPacsSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj);//检查项目
        List<PacsReportSqdhValueMxVo> GetPacsSqdhMxData(string sqdh, string OrganizeId);//检查项目明细
        List<LisReportSqdhValueMxVo> GetLisSqdhMxData(string zyh, string lissqdh, string organizeId);//检验项目明细

        List<AdviceListGridVO> AdviceGridView(Pagination pagination, AdviceListRequestVO req);//医嘱查询
        blzybrjbxxVO GetBlZybrjbxx(string OrgId, string zyh,string user,bool newbz);//获取病历元素值
        int updateLock(string OrgId, string blid,string user);//修改锁定状态
        void BLJG_Save(List<bl_ysjgnrEntity> bljgnr);
        string BLJG_Delete(string blid, string orgId);
    }
}
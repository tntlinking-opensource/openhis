using Newtouch.Domain.Entity;
using Newtouch.Domain.Entity.Clinic;
using Newtouch.Domain.Entity.Inpatient;
using Newtouch.Domain.Entity.Outpatient;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.EF;
using Newtouch.Infrastructure.EF.Conventions;
using System.Data.Entity;
using Newtouch.Domain.Entity.DeptStorageManage;

namespace Newtouch.Domain.DBContext.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultDbContextTBRegister
    {
        /// <summary>
        /// 注册表（底层已经注册了一部分）
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Registe(DbModelBuilder modelBuilder)
        {
            //注册新表
            modelBuilder.Entity<CommonChargeItemEntity>().RegisterTable().HasKey(p => p.cyxmId);
            modelBuilder.Entity<CommonDrugEntity>().RegisterTable().HasKey(p => p.cyypId);
            modelBuilder.Entity<PresTemplateDetailEntity>().RegisterTable().HasKey(p => p.mxId);
            modelBuilder.Entity<PresTemplateEntity>().RegisterTable().HasKey(p => p.mbId);
            modelBuilder.Entity<WMDiagnosisEntity>().RegisterTable().HasKey(p => p.xyzdId);
            modelBuilder.Entity<TCMDiagnosisEntity>().RegisterTable().HasKey(p => p.zyzdId);
            modelBuilder.Entity<SysObjectActionRecordEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<MedicalRecordEntity>().RegisterTable().HasKey(p => p.blId);
            modelBuilder.Entity<PrescriptionDetailEntity>().RegisterTable().HasKey(p => p.cfmxId);
            modelBuilder.Entity<PrescriptionEntity>().RegisterTable().HasKey(p => p.cfId);
            modelBuilder.Entity<TreatmentEntity>().RegisterTable().HasKey(p => p.jzId);
            modelBuilder.Entity<SysUsageLinkageEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<KcPdxxEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<KcPdxxmxEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<DeptKcSyyyEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<KcSyxxEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<KcKcjzEntity>().RegisterTable().HasKey(p => p.Id);
            

            //病历模板
            modelBuilder.Entity<MRTemplateEntity>().RegisterTable().HasKey(p => p.mbId);
            modelBuilder.Entity<MRTemplateWMDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<MRTemplateTCMDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<CfTemplateGroupPackageEntity>().RegisterTable().HasKey(p => p.mbztId);

            //检验检查
            modelBuilder.Entity<GroupPackageEntity>().RegisterTable().HasKey(p => p.ztId);
            modelBuilder.Entity<GroupPackageItemEntity>().RegisterTable().HasKey(p => p.ztxmId);
            modelBuilder.Entity<InspectionCategoryEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InspectionTemplateEntity>().RegisterTable().HasKey(p => p.mbId);
            modelBuilder.Entity<TemplateGroupPackageEntity>().RegisterTable().HasKey(p => p.mbztId);
            //医技科室执行
            modelBuilder.Entity<XtjyjcExecEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<XtjyjcFileUploadEntity>().RegisterTable().HasKey(p => p.Id);

            modelBuilder.Entity<PatientVitalSignsEntity>().RegisterTable().HasKey(p => p.Id);

            //系统
            modelBuilder.Entity<SysAuxiliaryDictionaryEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<SysDoctorRemarkEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<SysBodyPartsEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientVitalSignsEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientPatientInfoEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientLongTermOrderEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientSTATOrderEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientBedChargeItemEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientBedUseRecordEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientMedicineGrantEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientFeeDetailEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientMedicineReturnEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientChargeItemDietExeEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientPatientDoctorEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientPatientDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientOperationArrangementEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientOrderPackageEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientOrderPackageDetailEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientDietDetailSplitEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientDietBaseEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientDietSfxmdyEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientDietTemplateDetailSplitEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<SysBespeakRegisterEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientBedLevelChargeItemEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<VisitDeptSetEntity>().RegisterTable().HasKey(p => p.Id);
			modelBuilder.Entity<InpatientBedCardEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<InpatientDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);

            //门诊部分
            modelBuilder.Entity<MzyyghEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<MzsyypxxEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<MzsyzxxxEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<PrescripDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);

			modelBuilder.Entity<DzcfEntity>().RegisterTable().HasKey(p => p.cfId);
			modelBuilder.Entity<DzcfmxEntity>().RegisterTable().HasKey(p => p.cfmxId);
			//过敏信息
			modelBuilder.Entity<AllergyEntity>().RegisterTable().HasKey(p => p.Id);

            //影像部位方法
            modelBuilder.Entity<Pacs_ExamBodyPartsEntity>().RegisterTable().HasKey(p => p.Id);

            //中医馆  
            modelBuilder.Entity<CmmHis01Entity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<CmmHis02RecordEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<UsedDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);

            //智能审核日志
            modelBuilder.Entity<QhdZnshSqtxEntity>().RegisterTable().HasKey(p => p.Id);
            //事前事中审核drg入参出参日志
            modelBuilder.Entity<shjkldrzEntity>().RegisterTable().HasKey(p => p.Id);
            //条码码
            modelBuilder.Entity<MzZyBarCodeEntity>().RegisterTable().HasKey(p => p.Id);

            modelBuilder.Conventions.Add(new DecimalPrecisionAttributeConvention());//注入

            //病历词条
            modelBuilder.Entity<BlctglEntity>().RegisterTable().HasKey(p => p.ID);
            //门诊病历常用诊断
            modelBuilder.Entity<ComDiagnosisEntity>().RegisterTable().HasKey(p => p.Id);

            //科室备药

            modelBuilder.Entity<SysMedicineProfitLossReasonEntity>().RegisterTable().HasKey(p => p.syyyId);
            modelBuilder.Entity<PreparationdrugsEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<PreparationdrugsMXEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<PrapareMedicineReturnEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<PrepareMedicineReturnMXEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<SysMedicineProfitLossEntity>().RegisterTable().HasKey(p => p.syId);

            //诊室
            modelBuilder.Entity<OutpatientRegConsultEntity>().RegisterTable().HasKey(p => p.Id);
            modelBuilder.Entity<OutpatientConsultDoctorEntity>().RegisterTable().HasKey(p => p.Id);

            //预约看诊
            modelBuilder.Entity<ReservationEntity>().RegisterTable().HasKey(p => p.Id);
            //住院通知单
            modelBuilder.Entity<AdmissionNoticeEntity>().RegisterTable().HasKey(p => p.Id);
            //远程诊疗
            modelBuilder.Entity<ClinicApplyInfoEntity>().RegisterTable().HasKey(p => p.Id);
            //医嘱执行绑定费用
            modelBuilder.Entity<MedicalAdviceBindingFeeEntity>().RegisterTable().HasKey(p => p.newid);
        }

    }
}
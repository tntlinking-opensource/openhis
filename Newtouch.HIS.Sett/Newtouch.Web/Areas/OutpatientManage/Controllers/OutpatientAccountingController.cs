using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.OutpatientManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class OutpatientAccountingController : ControllerBase
    {
        private readonly IOutPatientDmnService _outPatientDmnService;
        private readonly IOutPatChargeApp _outPatChargeApp;
        private readonly IOutPatChargeDmnService _outPatChargeDmSer; // 挂号收费Service 
        private readonly IRefundDmnService _refundService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IOutPatChargeDmnService _outChargeDmnService;
        private readonly ISyncTreatmentServiceRecordDmnService _syncTreatmentServiceRecordDmnService;
        private readonly ISyncTreatmentServiceRecordRepo _syncTreatmentServiceRecordRepo;
        private readonly IOutPatChargeDmnService _outPatChargeDmnService;

        #region 页面初始化

        /// <summary>
        /// 新增登记
        /// </summary>
        /// <returns></returns>

        public ActionResult SysPatEntitiesblhView(string from = "")
        {
            ViewBag.from = from;
            return View();
        }

        public override ActionResult Index()
        {
            ViewBag.IsUpperLimitReminder = _sysConfigRepo.GetByCode("xt_UpperLimitReminder", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetValueByCode("xt_UpperLimitReminder", this.OrganizeId);
            ViewBag.IsBlh = _sysConfigRepo.GetByCode("IsBlh", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("IsBlh", OrganizeId).Value;
            ViewBag.sfxm_dj = _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId).Value;
            var duty = _sysUserDmnService.CheckStaffIsBelongDuty(this.UserIdentity.StaffId, "RehabDoctor");
            if (duty)
            {
                ViewBag.CurYsStaffId = UserIdentity.StaffId;
                ViewBag.CurYs = UserIdentity.rygh;
                ViewBag.CurYsmc = UserIdentity.UserName;
                ViewBag.CurKs = UserIdentity.DepartmentCode;
                ViewBag.CurKsmc = UserIdentity.DepartmentName;
            }

            return View();
        }

        public ActionResult AddOutpatientInfo()
        {
            // ReSharper disable once Mvc.ViewNotResolved
            return View();
        }

        /// <summary>
        /// 记账项目详情页面
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientAccountingDetail()
        {
            return View();
        }

        public ActionResult AccountingInOptima()
        {
            ViewBag.IsUpperLimitReminder = _sysConfigRepo.GetByCode("xt_UpperLimitReminder", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetValueByCode("xt_UpperLimitReminder", this.OrganizeId);
            ViewBag.sfxm_dj = _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId).Value;
            var duty = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "RehabDoctor");
            if (duty)
            {
                ViewBag.CurYsStaffId = UserIdentity.StaffId;
                ViewBag.CurYs = UserIdentity.rygh;
                ViewBag.CurYsmc = UserIdentity.UserName;
                ViewBag.CurKs = UserIdentity.DepartmentCode;
                ViewBag.CurKsmc = UserIdentity.DepartmentName;
            }
            return View();
        }

        /// <summary>
        /// 门诊收费
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatCharge()
        {
            ViewBag.IsBlh = _sysConfigRepo.GetByCode("IsBlh", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("IsBlh", OrganizeId).Value;
            ViewBag.sfxm_dj = _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("sfxm_dj", OrganizeId).Value;
            var duty = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "RehabDoctor");
            if (duty)
            {
                ViewBag.CurYsStaffId = UserIdentity.StaffId;
                ViewBag.CurYs = UserIdentity.rygh;
                ViewBag.CurYsmc = UserIdentity.UserName;
                ViewBag.CurKs = UserIdentity.DepartmentCode;
                ViewBag.CurKsmc = UserIdentity.DepartmentName;
            }
            return View();
        }

        #endregion

        #region 门诊记账 Vision-1.1 门诊记账的第一个版本
        /// <summary>
        /// 加载病人基本信息
        /// </summary>
        /// <param name="blh">病历号</param>
        /// <returns></returns>
        public ActionResult GetOutpatientBasicInfo(string IsBlh, string mzh)
        {
            IsBlh = IsBlh == "true" ? _sysConfigRepo.GetByCode("IsBlh", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("IsBlh", OrganizeId).Value : null;
            var patChargeInfo = _outPatChargeApp.GetOutPatBasicInfoInAcc(IsBlh, mzh);
            if (patChargeInfo.Count == 1)
            {
                return Success("提交成功", patChargeInfo.FirstOrDefault());
            }
            else if (patChargeInfo.Count > 1)
            {
                return Success("提交成功", patChargeInfo);
            }
            return Error("失败");
        }

        /// <summary>
        /// 保存门诊记账
        /// </summary>
        /// <param name="bacDto"></param>
        /// <param name="xmList"></param>
        /// <returns></returns>
        public ActionResult SaveAcountInfo(OutpatAccBasicInfoDto bacDto, List<OutpatAccGridInfoDto> xmList)
        {

            _outPatientDmnService.SaveOutpatientAccountInfo(bacDto, xmList, OrganizeId);
            return Success("提交成功");
        }

        public ActionResult PatSearchInfo(Pagination pagination, string blh, string xm,string zjh=null,string zjlx=null)
        {
            var data = new
            {
                rows = _refundService.GetBasicInfoSearchListInRegister(pagination, blh, xm, OrganizeId,zjh,zjlx),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        #endregion

        #region Optima记账 Vision-1.2 门诊记账的第二个版本（包括门诊记账和住院记账）
        /// <summary>
        /// 获取所有病人治疗记录
        /// </summary>
        /// <param name="begindate"></param>
        /// <param name="enddate"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetIssueGridJson(string type, string kssj, string jssj)
        {
            if (type == "2" && string.IsNullOrWhiteSpace(kssj) && string.IsNullOrWhiteSpace(jssj))
            {
                throw new FailedException("开始日期和结束日期不能同时为空");
            }
            var rygh = this.UserIdentity.rygh;
            var data = _syncTreatmentServiceRecordRepo.GetUnitList(OrganizeId, type, rygh, kssj, jssj).Where(p => p.hidden == 1);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载病人基本信息
        /// </summary>
        /// <param name="mzh">门诊号</param>
        /// <returns></returns>
        public ActionResult GetpatientInfoInOptima(string admsnum, string type, string kssj, string jssj)
        {
            var resultDto = new OutpatOptimAccInfoDto()
            {
                OutpatAccInfoDto = null,
                OptimAccInfoDto = null,
            };
            string rygh = UserIdentity.rygh;
            resultDto.OutpatAccInfoDto = _outChargeDmnService.GetPatInfoInOptima(admsnum, OrganizeId, type);
            if (type == "门诊")
            {
                resultDto.OptimAccInfoDto = _outChargeDmnService.GetOutAccountInfo(admsnum, OrganizeId, kssj, jssj, rygh).OrderBy(p => p.jzsj).ToList();
            }
            else if (type == "住院")
            {

                resultDto.OptimAccInfoDto = _outChargeDmnService.GetintAccountInfo(admsnum, OrganizeId, kssj, jssj, rygh).OrderBy(p => p.jzsj).ToList();
            }
            return Success("提交成功", resultDto);
        }

        /// <summary>
        /// 查询同步治疗记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUnConfirmedSyncedTreatmentRecord(string zyh, string mzh)
        {
            var admsnum = mzh;
            var resultDto = new OutpatOptimInfoDto()
            {
                OutpatAccInfoDto = null,
                SyncTreatmentServiceRecordVO = null,
            };

            var type = "门诊";
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                type = "住院";
                admsnum = zyh;
            }
            resultDto.OutpatAccInfoDto = _outChargeDmnService.GetPatInfoInOptima(admsnum, OrganizeId, type);
            resultDto.SyncTreatmentServiceRecordVO = _syncTreatmentServiceRecordDmnService.GetList(OrganizeId, 1, mzh: mzh, zyh: zyh, zlsgh: UserIdentity.rygh);

            return Success(null, resultDto);
        }
        public ActionResult SaveOptimaAcountInfo(string patientType, OutpatAccBasicInfoDto bacDto, List<OutpatAccGridInfoDto> xmList)
        {
            _outPatientDmnService.CommitAccounting(patientType, bacDto, xmList, OrganizeId);
            return Success("提交成功");
        }
        #endregion

        #region 门诊收费 Vision-1.3 门诊记账的第三个版本
        /// <summary>
        /// 获取所有病人挂号信息
        /// </summary>
        /// <param name="begindate"></param>
        /// <param name="enddate"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetRegisterGridJson(string begindate, string enddate)
        {
            var data = _outChargeDmnService.GetRegisterGridJson(begindate, enddate, OrganizeId, UserIdentity.UserCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取记账历史记录
        /// </summary>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetpatientAccountInfo(string IsBlh, string mzh)
        {
            var resultDto = new OutpatOptimAccInfoDto()
            {
                OutpatAccInfoDto = null,
                OptimAccInfoDto = null,
            };
            var paiInfoList = _outPatChargeApp.GetOutPatBasicInfoInAcc(IsBlh, mzh);
            if (paiInfoList != null && paiInfoList.Count > 0)
            {
                if (paiInfoList.Count > 1)
                {
                    return Success("提交成功", paiInfoList);
                }
                else
                {
                    resultDto.OutpatAccInfoDto = paiInfoList.FirstOrDefault();
                    resultDto.OptimAccInfoDto = _outPatChargeApp.GetInfo(resultDto.OutpatAccInfoDto.mzh);
                    return Success("提交成功", resultDto);
                }
            }
            return Error("查询失败");
        }



        /// <summary>
        /// 保存门诊记账
        /// </summary>
        /// <param name="bacDto"></param>
        /// <param name="xmList"></param>
        /// <returns></returns>
        public ActionResult SavepatientAcountInfo(OutpatAccBasicInfoDto bacDto, List<OutpatAccGridInfoDto> xmList)
        {
            _outPatientDmnService.SavepatientAccountInfo(bacDto, xmList, OrganizeId);
            return Success("提交成功");
        }
        #endregion

        #region 门诊记账，第四个版本
        /// <summary>
        /// 获取记账历史记录
        /// </summary>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetpatientAccountInfoV4(string mzh)
        {
            var resultDto = new OutpatOptimAccInfoDto()
            {
                OutpatAccInfoDto = null,
                OptimAccInfoDto = null,
            };
            var paiInfoList = _outPatChargeApp.GetOutPatBasicInfoInAcc("OFF", mzh);
            if (paiInfoList != null && paiInfoList.Count > 0)
            {
                if (paiInfoList.Count > 1)
                {
                    return Success("提交成功", paiInfoList);
                }
                else
                {
                    resultDto.OutpatAccInfoDto = paiInfoList.FirstOrDefault();
                    resultDto.OptimAccInfoDto = _outPatChargeDmnService.GetpatientAccountInfoV4(mzh, OrganizeId);
                    return Success("提交成功", resultDto);
                }
            }
            return Error("查询失败");
        }
        #endregion
    }
}
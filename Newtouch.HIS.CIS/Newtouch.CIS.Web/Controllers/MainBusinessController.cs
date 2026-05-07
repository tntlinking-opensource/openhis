using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Application.Interface;
using Newtouch.Domain.DTO;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.IDomainServices;
using Newtouch.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.CIS.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class MainBusinessController : OrgControllerBase
    {
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysOrganizeDmnService _organizeDmnService;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IPrescriptionDmnService _prescriptionDmnService;
        private readonly ISysStaffRepo _sysStaffRepo;
        private readonly IOutpatientCmmManagerApp _outpatientCmmManagerApp;


        public override ActionResult Index()
        {
            //登录人是否是护士
            ViewBag.IsNurse = _sysUserDmnService.CheckStaffIsBelongDuty(this.UserIdentity.StaffId, "Nurse");

            //开关：护士录入生命体征
            ViewBag.IsVitalSignsSwitch = _sysConfigRepo.GetValueByCode("vitalSignsSwitch", this.OrganizeId);
            //开关：是否开放检验检查
            ViewBag.IsOpenJyJcSwitch = _sysConfigRepo.GetBoolValueByCode("openJyJcSwitch", this.OrganizeId);
            //开关：门诊是否开放康复处方
            ViewBag.ISOpenKfcf = _sysConfigRepo.GetBoolValueByCode("openKfcf", this.OrganizeId);
            //开关：门诊是否开放常规项目处方
            ViewBag.ISOpenCgxmcf = _sysConfigRepo.GetBoolValueByCode("openCgxmcf", this.OrganizeId);
            ViewBag.isOpenRationalUse = _sysConfigRepo.GetValueByCode("OpenRationalUse", OrganizeId);//是否开启合理用药
            //开关：门诊是否开放中药处方
            ViewBag.ISOpenZycf = _sysConfigRepo.GetBoolValueByCode("openZycf", this.OrganizeId);
            ViewBag.ISOpenZycfnew = _sysConfigRepo.GetBoolValueByCode("openZycfnew", this.OrganizeId);
            //开关：门诊是否开放医用耗材
            ViewBag.ISOpenWzhc = _sysConfigRepo.GetBoolValueByCode("openWzhccf", this.OrganizeId);
            //开关：药品是否关联药房库存
            ViewBag.ISMedicineSearchRelatedKC = _sysConfigRepo.GetBoolValueByCode("IS_MedicineSearchRelatedKC", OrganizeId);
            //康复处方打印开关
            ViewBag.ISPrintRehabPres = _sysConfigRepo.GetBoolValueByCode("IS_PrintRehabPres", OrganizeId) ?? true;
            //治疗单打印开关
            ViewBag.ISPrintRehabTreatment = _sysConfigRepo.GetBoolValueByCode("IS_PrintRehabTreatment", OrganizeId);
            #region 抗生素相关
            ViewBag.IsQyKssKz = _sysConfigRepo.GetBoolValueByCode("openKssQxSwitch", OrganizeId);//是否启用抗生素
            ViewBag.qxjb = "0";//抗生素权限级别(默认 0 非限制用药)
            SysStaffVEntity staff = _sysStaffRepo.GetValidStaffByGh(UserIdentity.rygh, OrganizeId);
            if (staff != null)
            {
                switch (staff.zc)
                {
                    case "ys":
                        ViewBag.qxjb = "0";//医师 对应 非限制用药 0
                        break;
                    case "zzys":
                        ViewBag.qxjb = "1";//主治医师 对应 限制用药 1
                        break;
                    case "zrys":
                        ViewBag.qxjb = "2";//主任医师 对应 特殊用药 2
                        break;
                    default:
                        ViewBag.qxjb = "0";//医师 对应 非限制用药 0
                        break;
                }
            }
            #endregion 抗生素相关

            //开关：预约挂号
            ViewBag.ISOpenBespeakRegister = _sysConfigRepo.GetBoolValueByCode("IS_BespeakRegister", OrganizeId);
            var defualtFrequency = _sysConfigRepo.GetValueByCode("RehabItemDefaultFrequency", OrganizeId);
            if (!string.IsNullOrWhiteSpace(defualtFrequency))
            {
                var arr = defualtFrequency.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2)
                {
                    ViewBag.RehabItemDefaultFrequencyName = arr[0];
                    ViewBag.RehabItemDefaultFrequencyCode = arr[1];
                }
            }
            //医保控制code
            ViewBag.ControlbrxzCode = _sysConfigRepo.GetValueByCode("ControlbrxzCode", OrganizeId);
            //注射用法控制
            ViewBag.ControlzsyfCode = _sysConfigRepo.GetValueByCode("zsyfpz", OrganizeId);
            //雾化用法控制
            ViewBag.ControlwhyfCode = _sysConfigRepo.GetValueByCode("whyfpz", OrganizeId);


            //影像配置
            ViewBag.PACSCode = _sysConfigRepo.GetValueByCode("PACSCode", OrganizeId);
            ViewBag.yysz_hyfqURL = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("yysz_hyfqURL");
            //影像调阅程序位置配置
            ViewBag.exelink_pacs = _sysConfigRepo.GetValueByCode("exelink_pacs", OrganizeId);
            //远程医疗
            ViewBag.OrganizeCodeSd = _organizeDmnService.GetCodeByOrgId(OrganizeId);
            //ViewBag.OrganizeCodeSd = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("OrganizeCodeSd");
            ViewBag.RemoteTreatRPTURL = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("RemoteTreatRPTURL");
          

            #region 滴速
            var frds = _sysConfigRepo.GetValueByCode("frequencyRelDroppingSpeed", OrganizeId);
            frds = string.IsNullOrWhiteSpace(frds) ? "静滴" : frds;
            ViewBag.frequencyRelDroppingSpeed = frds;
            #endregion
            return View();
        }

        #region 秦皇岛三期
        /// <summary>
        /// 医保患者审核收费项目是否含自费
        /// </summary>
        /// <param name="sfxmCode"></param>
        /// <returns></returns>
        public ActionResult ValidateMedicalInsurance(string cfDto)
        {
            var cfDtoList = Tools.Json.ToList<PrescriptionHtmlDTO>(cfDto);
            Dictionary<int, string> sfxmdic = new Dictionary<int, string>();
            foreach (var item in cfDtoList)
            {

                var sfxmlist = "";
                foreach (var cf in item.cfHtml)
                {
                    if (cf.sfbz == false)
                    {
                        sfxmlist = sfxmlist + "," + (cf.xmCode ?? cf.ypCode);
                        var cflx = item.cflx == "kfcf" ? (int)EnumCflx.RehabPres : item.cflx == "cgxmcf" ? (int)EnumCflx.RegularItemPres : item.cflx == "xycf" ? (int)EnumCflx.WMPres : item.cflx == "zycf" ? cf.cflx = (int)EnumCflx.TCMPres : item.cflx == "jycf" ? (int)EnumCflx.InspectionPres : item.cflx == "jccf" ? (int)EnumCflx.ExaminationPres : 0;
                        if (sfxmdic.ContainsKey(cflx))
                        {
                            sfxmdic.Remove(cflx);
                        }
                        sfxmdic.Add(cflx, sfxmlist);
                    }
                }
            }
            var noybdmList = _prescriptionDmnService.ValidateMedicalInsurance(sfxmdic, OrganizeId);
            var noybdstr = string.Join(",", noybdmList.ToArray());
            return Success("", noybdstr);
        }
        #endregion

        #region 中医馆

        /// <summary>
        /// 中医馆部分视图
        /// </summary>
        /// <returns></returns>
        public ActionResult TcmHis()
        {
            return View();
        }

        /// <summary>
        /// 推送患者信息
        /// </summary>
        /// <param name="jzxx"></param>
        /// <returns></returns>
        public ActionResult TcmHis01(TreatEntityObj jzxx)
        {
            var result = _outpatientCmmManagerApp.TcmHis01(jzxx, OrganizeId, UserIdentity.UserCode);
            return "1".Equals(result.code) ? Success() : Error(result.desc);
        }

        /// <summary>
        /// 电子病历集成模块
        /// </summary>
        /// <param name="mzh">门诊号</param>
        /// <param name="jzys">就诊医生工号</param>
        /// <returns></returns>
        public ActionResult GetIntegrateEmrUrl(string mzh, string jzys)
        {
            string response;
            var result = _outpatientCmmManagerApp.GetIntegrateEmrUrl(mzh, jzys, OrganizeId, out response);
            return string.IsNullOrWhiteSpace(result) ? Success("", response) : Error(result);
        }

        /// <summary>
        /// 辩证论治模块
        /// </summary>
        /// <param name="aeRequest"></param>
        /// <returns></returns>
        public ActionResult GetIntegrateAeUrl(GetAeRequestDto aeRequest)
        {
            string response;
            if (aeRequest == null) aeRequest = new GetAeRequestDto();
            aeRequest.organizeId = OrganizeId;
            if (string.IsNullOrWhiteSpace(aeRequest.jzys)) aeRequest.jzys = UserIdentity.UserCode;
            var result = _outpatientCmmManagerApp.GetIntegrateAeUrl(aeRequest, out response);
            return string.IsNullOrWhiteSpace(result) ? Success("", response) : Error(result);
        }

        /// <summary>
        /// 提取电子病历
        /// </summary>
        /// <param name="mzh">门诊号</param>
        /// <returns></returns>
        public ActionResult ExtractIntegrateEmr(string mzh)
        {
            var result = _outpatientCmmManagerApp.ExtractIntegrateEmr(mzh, OrganizeId);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 提取诊断信息
        /// </summary>
        /// <param name="mzh">门诊号</param>
        /// <returns></returns>
        public ActionResult ExtractIntegrateDiagnosis(string mzh)
        {
            var result = _outpatientCmmManagerApp.ExtractIntegrateDiagnosis(mzh, OrganizeId);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 中医馆弹框
        /// </summary>
        /// <returns></returns>
        public ActionResult CmmAlertForm()
        {
            return View();
        }

        /// <summary>
        /// 推送草药
        /// </summary>
        /// <returns></returns>
        public ActionResult PushMedicineInfo()
        {
            var result = _outpatientCmmManagerApp.PushMedicineInfo(OrganizeId, UserIdentity.UserCode);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 提取处方
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="mzzybz">门诊住院标志 0-药库  1-门诊  2-住院  3-通用(取门诊单位) </param>
        /// <returns></returns>
        public ActionResult ExtractIntegrateRp(string mzh, string mzzybz)
        {
            var result = _outpatientCmmManagerApp.ExtractIntegrateRp(mzh, OrganizeId, mzzybz);
            return Success("", result.ToJson());
        }

        /// <summary>
        /// 知识库系统模块
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIntegrateKUrl()
        {
            var response = "";
            var result = _outpatientCmmManagerApp.GetIntegrateKUrl(out response);
            return string.IsNullOrWhiteSpace(result) ? Success("", response) : Error(result);
        }

        /// <summary>
        /// 集成治未病模块
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="mzh"></param>
        /// <param name="zjlx"></param>
        /// <param name="zjh"></param>
        /// <returns></returns>
        public ActionResult GetIntegrateHEALUrl(string blh, string mzh, string zjlx, string zjh)
        {
            string response;
            var result = _outpatientCmmManagerApp.GetIntegrateHEALUrl(blh, mzh, zjlx, zjh, OrganizeId, out response);
            return string.IsNullOrWhiteSpace(result) ? Success("", response) : Error(result);
        }

        /// <summary>
        /// 获取患者基本信息
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="mzh"></param>
        /// <param name="jzys"></param>
        /// <returns></returns>
        public JsonResult GetIntegrateRcRequsetParam(string blh, string mzh, string jzys)
        {
            if (string.IsNullOrWhiteSpace(jzys)) jzys = UserIdentity.UserCode;
            var result = _outpatientCmmManagerApp.GetIntegrateRCRequsetParam(blh, mzh, jzys, OrganizeId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
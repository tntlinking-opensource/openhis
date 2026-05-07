using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System.Web.Mvc;
using System.Linq;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.Infrastructure.Model;
using FrameworkBase.MultiOrg.Domain.IRepository;

namespace Newtouch.HIS.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : FrameworkBase.MultiOrg.Web.Controllers.HomeController
    {
        private readonly ICommonDmnService _commonDmnService;
        private readonly ISyncTreatmentServiceRecordRepo _syncTreatmentServiceRecordRepo;
        private readonly ISysRoleShortcutMenuDmnService _sysRoleShortcutMenuDmnService;
        private readonly ISysUserExDmnService _sysUserDmnService;
        private readonly ISysPharmacyDepartmentRepo _sysPharmacyDepartmentRepo;
        private readonly ISysConfigRepo _sysConfigRepo;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public override ActionResult Index()
        {
            var loginFromFlag = WebHelper.GetCookie(Constants.AppId + "_LoginFromFlag");
            ViewBag.loginFromFlag = loginFromFlag;

            return base.Index();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opr"></param>
        public override void SwithOrgSuccessOprBeforeSave(OperatorModel opr)
        {
            //更新用户药房部分
            if (ConfigurationHelper.GetAppConfigBoolValue("Is_UserYfbmRelated") == true && opr.OrganizeId != null)
            {
                //当前要操作的药房部门
                if (!string.IsNullOrWhiteSpace(opr.UserId) && !string.IsNullOrWhiteSpace(opr.OrganizeId))
                {
                    opr.yfbmCodeList = _sysUserDmnService.GetYfbmCodeListByUserId(opr.UserId, opr.OrganizeId);
                }
                if (opr.yfbmCodeList != null && opr.yfbmCodeList.Count > 0)
                {
                    //当前要操作的药房部门    //在OrgId关联了多个yfbm怎么办
                    var curYfbmCode = opr.yfbmCodeList.FirstOrDefault();
                    Constants.SetCurrentYfbm(opr.UserId, new LoginUserCurrentYfbmModel()
                    {
                        yfbmCode = curYfbmCode,
                        yfbmjb = _sysPharmacyDepartmentRepo.GetYjbmjbByCode(curYfbmCode, opr.OrganizeId),
                        mzzybz = _sysPharmacyDepartmentRepo.GetMzzybzByCode(curYfbmCode, opr.OrganizeId),
                    });
                }
                else
                {
                    Constants.SetCurrentYfbm(opr.UserId, null);  //移除
                }
            }

            //切换药房部门
            WebHelper.WriteCookie(Constants.AppId + "_" + "CookieKey_IsHospAdministrator", opr.IsHospAdministrator.ToString().ToLower());

            LoginController.UALoginPre(opr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public override ActionResult Default()
        {
            if (this.UserIdentity.RoleIdList != null && this.UserIdentity.RoleIdList.Count > 0)
            {
                ViewBag.ShortcutMenuList = _sysRoleShortcutMenuDmnService.GetAuthedSCMList(this.UserIdentity.RoleIdList, this.UserIdentity.OrganizeId);
            }
            ViewBag.Metod_VisitsCountChart =  _sysConfigRepo.GetValueByCode("Metod_VisitsCountChart_Outpatient", OrganizeId);
            return View();
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 获取就诊人数（门诊记账、住院记账）
        /// </summary>
        public ActionResult GetVisitNum()
        {
            VisitNumBO BO = null;
            var Metod_VisitsCountChart = _sysConfigRepo.GetValueByCode("Metod_VisitsCountChart_Outpatient", OrganizeId);
            BO = _commonDmnService.GetVisitNum(Metod_VisitsCountChart ?? "", OrganizeId);
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = null, data = BO }.ToJson());
        }

        public ActionResult GetWeekNum()
        {
          var Metod_VisitsCountChart = _sysConfigRepo.GetValueByCode("Metod_VisitsCountChart_Outpatient", OrganizeId);
          var data=  _commonDmnService.GetWeekNum(Metod_VisitsCountChart ?? "", OrganizeId);
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = null, data = data }.ToJson());
        }

        public ActionResult GetLastWeek() {
            var data = _commonDmnService.GetLastWeek();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRemindCount()
        {
            var countTreatmentRecordConfirm = 0;
            if (!string.IsNullOrWhiteSpace(this.UserIdentity.OrganizeId))
            {
                var countdata = _syncTreatmentServiceRecordRepo.GetUnitList(this.UserIdentity.OrganizeId, "1", this.UserIdentity.rygh).Where(p => p.hidden == 1);
                if (countdata != null && countdata.Count() > 0)
                {
                    countTreatmentRecordConfirm = countdata.Count();
                }
            }

            var data = new
            {
                TreatmentRecordConfirm = countTreatmentRecordConfirm,

            };
            return Content(data.ToJson());
        }

        public ActionResult GetNewFieldUniqueValue(string fieldName, string orgId)
        {
            var data = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue(fieldName, orgId);
            return Success("", data);
        }
        
        /**
         * 同步系统参数配置
         */ 
        public ActionResult SyncSysConfigParams(string orgId)
        {
            //基础数据
            var sysConfigBaseEntities = _sysConfigRepo.GetList("", "*").ToList();
            //组织机构自带数据
            var sysConfigEntities = _sysConfigRepo.GetList("", orgId).ToList();
            //根据code 去重
            var sysConfigCodes = new HashSet<string>(sysConfigEntities.Select(entity => entity.Code));
            var filteredEntities = sysConfigBaseEntities
                .Where(baseEntity => !sysConfigCodes.Contains(baseEntity.Code))
                .ToList();
            foreach (var item in filteredEntities)
            {
                item.Id = Guid.NewGuid().ToString();
                item.OrganizeId = orgId;
            }
            var insert = _sysConfigRepo.Insert(filteredEntities);
            return Success("",insert);
        }

    }
}

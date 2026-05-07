using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common.Operator;
using Newtouch.Core.Common.Utils;
using Newtouch.Infrastructure;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.ReportManage.Controllers
{
    public class YpckQueryController : ControllerBase
    {
        private readonly ISysDepartmentRepo _sysDepartmentRepo;
        private readonly ISysStaffRepo _sysStaffRepo;

        public ActionResult YpckQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }
        public ActionResult AntibioticQuery()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult OutpatientPrescription()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult HospitalPrescription()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult DrugSubtotal()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult AntibioticCFQuery()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult CFQuantity()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult YPDrugUsage()
        {
            ViewBag.OrganizeId = OrganizeId;
            ReportingServiceCom();
            return View();
        }
        public ActionResult YPInOutBound()
        {
            ViewBag.OrganizeId = OrganizeId;
            ReportingServiceCom();
            return View();
        }
        /// <summary>
        /// 获取科室
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSysDepartmentList(string keyword=null)
        {
            var data = _sysDepartmentRepo.GetList(OperatorProvider.GetCurrent().OrganizeId, "1", keyword);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSysStaffList(string keyword=null)
        {
            List<SysStaffVEntity> list = new List<SysStaffVEntity>();
            list = (List<SysStaffVEntity>)_sysStaffRepo.GetValidStaffListByOrganizeId(this.OrganizeId);
            if (!string.IsNullOrWhiteSpace(keyword))
                list = list.FindAll(p => p.gh == keyword || p.Name == keyword);

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        private void ReportingServiceCom()
        {

            ViewBag.OrgId = this.OrganizeId;
            ViewBag.topOrgId = Constants.TopOrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            ViewBag.IsHospAdministrator = this.UserIdentity.IsHospAdministrator.ToString().ToLower();  //是否是医院管理员
            ViewBag.CurUserCode = this.UserIdentity.UserCode;
            ViewBag.curUsergh = UserIdentity.rygh;
        }


        //药品价格分析表
        public ActionResult YPDrugComparisonTable() {
            ReportingServiceCom();
            return View();
        }

        // 药品入库查询
        public ActionResult YprkQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }
        public ActionResult YpckQueryNew()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }
        
        
    }
}
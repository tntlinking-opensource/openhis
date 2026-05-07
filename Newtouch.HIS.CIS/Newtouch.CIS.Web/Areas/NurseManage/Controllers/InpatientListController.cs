using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.CIS.Web.Areas.NurseManage.Controllers
{
    public class InpatientListController : OrgControllerBase
    {

        private readonly ITreatmentRepo _treatmentRepo;
        private readonly IPatientVitalSignsRepo _patientVitalSignsRepo;
        private readonly IInpatientPatientDmnService _inpatientPatientDmnService;
        private readonly FrameworkBase.MultiOrg.Domain.IDomainServices.IBaseDataDmnService _iBaseDataDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IIDBDmnService _idbDmnService;
        private readonly IVisitDeptSetRepo visitDeptSetRepo;
        private readonly IExceReportPrintDmnService _IExceReportPrintDmnService;
        private readonly IOrderAuditDmnService _OrderAuditDmnService;


        public InpatientListController()
        {

        }
        // GET: NurseManage/InpatientList
        public ActionResult Inpatient()
        {
            var bqAuth = _iBaseDataDmnService.GetWardListByStaffGh(UserIdentity.rygh, OrganizeId).FirstOrDefault();
            if (bqAuth != null)
            {
                ViewBag.bqval = bqAuth.bqCode;
                ViewBag.ysgh = UserIdentity.rygh;
                return View();
            }
            else
            {
                return Content("javascript:void()");
            }
        }
        #region  View
        public ActionResult IndexCenter()
        {
            return View();
        }
        public ActionResult MedicalorderQuery()
        {
            return View();
        }
        public ActionResult OrderAudit()
        {
            return View();
        }
        public ActionResult OrderExecution()
        {
            return View();
        }
        public ActionResult InHosBookkeep()
        {
            return View();
        }
        public ActionResult InHosFeeRefund()
        {
            return View();
        }
        public ActionResult InHosMedReturn()
        {
            return View();
        }
        public ActionResult InHosFee()
        {
            return View();
        }
        public ActionResult InHosRegistration()
        {
            return View();
        }
        public ActionResult InHosRecallOutArea()
        {
            return View();
        }
        public ActionResult InHosQueryIndex()
        {
            return View();
        }
        public ActionResult InHosNursingInput()
        {
            ViewBag.MutipatientNursingInputFlag = _sysConfigRepo.GetIntValueByCode("MutipatientNursingInputFlag", OrganizeId, 0);
            ViewBag.ScdCode = _sysConfigRepo.GetValueByCode("ScdTimePoint", OrganizeId);//三测单时间点奇偶数设置 1 奇数 0 偶数
            return View();
        }
        public ActionResult InHosPrint()
        {
            return View();
        }
        #endregion

        public ActionResult GetPatWardTree(string zyzt, string keyword,string checkzyh=null)
        {
            var wardTree = _OrderAuditDmnService.GetWardTree(this.UserIdentity.StaffId);
            var patInfo = _IExceReportPrintDmnService.GetPatCenterTree(OrganizeId, zyzt, keyword);
            var treeList = new List<TreeViewModel>();
            string[] checkzyhs = new string[200];
            if (!string.IsNullOrWhiteSpace(checkzyh))
            {
                checkzyhs = checkzyh.Split(',');
            }
            foreach (var item in wardTree)
            {
                var NewPatInfo = patInfo.Where(p => p.bqCode == item.bqCode).OrderBy(p=>p.BedNo).ToList();
                foreach (InpWardPatTreeVO itempat in NewPatInfo)
                {
                    string gender = itempat.sex == "1" ? "男" : "女";
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.zyh;
                    //床号 + 姓名(住院天数)+住院号 + 年龄 +性别+病人性质
                    treepat.text = itempat.BedNo + "-" + itempat.hzxm + "(" + itempat.inHosDays + "天)" + "-" + itempat.zyh + "-" + itempat.nl + "岁-" + gender + "-" + itempat.brxzmc;
                    treepat.value = itempat.zyh;
                    treepat.parentId = item.bqCode;
                    treepat.isexpand = false;
                    treepat.complete = true;
                    treepat.showcheck = true;
                    treepat.checkstate = 0;
                    treepat.hasChildren = false;
                    treepat.Ex1 = "c";
                    treepat.Ex2 = itempat.sex;
                    treepat.Ex3 = itempat.nl;
                    treepat.Ex4 = itempat.hzxm;
                    treepat.Ex5 = itempat.rqrq;
                    treepat.Ex6 = itempat.cqrq;
                    treeList.Add(treepat);
                    if (((IList)checkzyhs).Contains(itempat.zyh))
                    {
                        treepat.checkstate = 1;
                    }
                }

                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = patInfo.Count == 0 ? false : true;
                tree.id = item.bqCode;
                tree.text = item.bqmc;
                tree.value = item.bqCode;
                tree.parentId = null;
                tree.isexpand = true;
                tree.complete = true;
                tree.showcheck = true;
                tree.checkstate = 0;
                tree.hasChildren = hasChildren;
                tree.Ex1 = "p";
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson(null));

        }
    }
}
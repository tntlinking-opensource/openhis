using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Utils;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.CIS.Web.Areas.NurseManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderAuditController : OrgControllerBase
    {
        private readonly IOrderAuditDmnService _OrderAuditDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IAllergyManageDmnService _allergyManageDmnService;
        private readonly ISysDepartmentRepo _sysDepartmentRepo;
        private string IsRehabAuthtoNurse;
        private bool isNurse;
        private bool isRehabDoctor;

        public OrderAuditController(IOrderAuditDmnService OrderAuditDmnService)
        {
            this._OrderAuditDmnService = OrderAuditDmnService;
            //IsRehabAuthtoNurse= _sysConfigRepo.GetValueByCode("IsRehabAuthtoNurse", this.OrganizeId);
            //isNurse = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "Nurse");
            //isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "RehabDoctor");
        }


        // GET: NurseManage/OrderAudit
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult GetGridJson(Pagination pagination, string patList, string organizeId)
        {
            IsRehabAuthtoNurse = _sysConfigRepo.GetValueByCode("IsRehabAuthtoNurse", this.OrganizeId);
            isNurse = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "Nurse");
            isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "RehabDoctor");
            IList<OrderAuditVO> list = new List<OrderAuditVO>();
            if (!string.IsNullOrWhiteSpace(IsRehabAuthtoNurse) && IsRehabAuthtoNurse == "0")
            {
                if (isNurse && isRehabDoctor)
                {
                    list = _OrderAuditDmnService.GetOrderAuditYZList(pagination, patList, OrganizeId);
                }
                else if (isRehabDoctor) //康复医嘱仅康复治疗师审核
                {
                    list = _OrderAuditDmnService.GetOrderAuditYZList_KF(pagination, patList, OrganizeId, this.UserIdentity.DepartmentCode);
                }
                else if (isNurse)  //护士无授权则无法审核康复医嘱
                {
                    list = _OrderAuditDmnService.GetOrderAuditYZList(pagination, patList, OrganizeId, IsRehabAuthtoNurse);
                }
            }
            else
            {
                list = _OrderAuditDmnService.GetOrderAuditYZList(pagination, patList, OrganizeId);
            }
            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }


        public ActionResult submitOrderList(IList<OrderAuditVO> orderList)
        {
            //entity.zt = entity.zt == "true" ? "1" : "0";
            OperatorModel user = this.UserIdentity;
            string data = _OrderAuditDmnService.OrderAuditSubmit(user, orderList);

            if (data == "")
            {
                return Success("审核成功");
            }
            else
            {
                return Success("", data);
            }

        }

        public ActionResult submitOrderListbyPat(string patList, int yzxz, IList<OrderAuditVO> orderList)
        {
            IsRehabAuthtoNurse = _sysConfigRepo.GetValueByCode("IsRehabAuthtoNurse", this.OrganizeId);
            isNurse = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "Nurse");
            isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(UserIdentity.StaffId, "RehabDoctor");
            OperatorModel user = this.UserIdentity;
            if (IsRehabAuthtoNurse != null && IsRehabAuthtoNurse == "0")
            {
                if (isNurse && isRehabDoctor)
                {
                    _OrderAuditDmnService.OrderAuditSubmitbyPat(user, patList, yzxz, orderList);
                }
                else if (isRehabDoctor)
                {
                    _OrderAuditDmnService.OrderAuditSubmitbyPat(user, patList, yzxz, orderList, IsRehabAuthtoNurse, true, this.UserIdentity.DepartmentCode);
                }
                else if (isNurse)
                {
                    _OrderAuditDmnService.OrderAuditSubmitbyPat(user, patList, yzxz, orderList, IsRehabAuthtoNurse);
                }
            }
            else
            {
                var data = _OrderAuditDmnService.OrderAuditSubmitbyPat(user, patList, yzxz, orderList);
                if (data == "")
                {
                    return Success("审核成功");
                }
                else
                {
                    //data = data.Remove(data.Length - 1, 1);
                    return Success("", data);
                }
            }
            return Success("审核成功");
        }

        /// <summary>
        /// 获取病区患者待审核医嘱树
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetPatWardTree(string aa,string keyword)
        {
            IsRehabAuthtoNurse = _sysConfigRepo.GetValueByCode("IsRehabAuthtoNurse", this.OrganizeId);
            var medicalInsurance = _sysConfigRepo.GetValueByCode("medicalInsurance", this.OrganizeId);
            string staffId = this.UserIdentity.StaffId;
            var wardTree = _OrderAuditDmnService.GetWardTree(staffId);
            IList<InpWardPatTreeVO> patTree = new List<InpWardPatTreeVO>();
            if (medicalInsurance != "qinhuangdao")
                patTree = _OrderAuditDmnService.GetPatTree(staffId);
            else
                patTree = _OrderAuditDmnService.GetPatTree(staffId, IsRehabAuthtoNurse);
            if(!string.IsNullOrWhiteSpace(keyword))
                patTree = patTree.Where(p => p.zyh.Contains(keyword)).ToList();
            string[] aasz = new string[200];
            if (!string.IsNullOrWhiteSpace(aa))
            {
                aasz = aa.Split(',');
            }

            var treeList = new List<TreeViewModel>();
            foreach (InpWardPatTreeVO item in wardTree)
            {
                if (IsRehabAuthtoNurse == "0" && isNurse)
                {

                }
                var patInfo = patTree.Where(p => p.bqCode == item.bqCode).ToList();
                var NewPatInfo = patInfo.OrderBy(p => p.BedNo);
                foreach (InpWardPatTreeVO itempat in NewPatInfo)
                {  
                    string gender = itempat.sex == "1" ? "男" : "女";
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.zyh;
                    //床号 + 姓名(住院天数)+住院号 + 年龄 +性别
                    treepat.text = itempat.BedNo + "-" + itempat.hzxm + "(" + itempat.inHosDays + "天)" + "-" + itempat.zyh + "-" + itempat.nl + "岁-" + gender;
                    treepat.value = itempat.zyh;
                    treepat.parentId = item.bqCode;
                    treepat.isexpand = false;
                    treepat.complete = true;
                    treepat.showcheck = true;
                    treepat.checkstate = 0;
                    treepat.hasChildren = false;
                    treepat.Ex1 = "c";
                    treeList.Add(treepat);
                    if (((IList)aasz).Contains(itempat.zyh))
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
         
        /// <summary>
        /// 录入皮试结果
        /// </summary>
        /// <returns></returns>
        public ActionResult InputresultsSkintest()
        {
            return View();
        }

        //皮试页面树控件
        public ActionResult SkintestTree(string keyword, string selectkey)
        {
            //IList<SkintestVO> patTree = new List<SkintestVO>();
            //patTree = _OrderAuditDmnService.SkintestVO(this.OrganizeId);
            var wardTree = _OrderAuditDmnService.SkintestVO(this.OrganizeId, keyword, selectkey);
            var wardonly = wardTree.GroupBy(p => new { p.WardCode, p.bqmc }).Select(p => new { p.Key.WardCode, p.Key.bqmc });


            var treeList = new List<TreeViewModel>();
            foreach (var item in wardonly)
            {
                var patInfo = wardTree.Where(p => p.WardCode == item.WardCode).Where(p => p.zyh != "").Where(p => p.zyh != null).ToList();

                foreach (SkintestVO itempat in patInfo)
                {
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.zyh;
                    treepat.text = itempat.zyh + "-" + itempat.hzxm;
                    treepat.value = itempat.zyh;
                    treepat.parentId = item.WardCode;
                    treepat.isexpand = false;
                    treepat.complete = true;
                    treepat.showcheck = true;
                    treepat.checkstate = 0;
                    treepat.hasChildren = false;
                    treepat.Ex1 = "c";
                    treeList.Add(treepat);
                }

                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = patInfo.Count == 0 ? false : true;
                tree.id = item.WardCode;
                tree.text = item.bqmc;
                tree.value = item.WardCode;
                tree.parentId = null;
                tree.isexpand = true;
                tree.complete = true;
                tree.showcheck = true;
                tree.checkstate = 0;
                tree.hasChildren = hasChildren;
                tree.Ex1 = "p";
                treeList.Add(tree);
            }
            var a = treeList.TreeViewJson(null);
            return Content(treeList.TreeViewJson(null));
        }



        public ActionResult Inputinformation(Pagination pagination, string patList, string organizeId, string selectkey)
        {

            IList<SkintestqueryVO> list = new List<SkintestqueryVO>();

            list = _OrderAuditDmnService.Skintestquery(pagination, patList, OrganizeId, selectkey);

            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }


        public ActionResult Inputskintestresults(IList<SkintestqueryVO> orderList)
        {
            //entity.zt = entity.zt == "true" ? "1" : "0";
            OperatorModel user = this.UserIdentity;
            _OrderAuditDmnService.Inputskintestresults(user, orderList);
            return Success("皮试录入成功");
        }

        public ActionResult AuditTips()
        {

            return View();
        }

        public ActionResult Displayinformation(string patList)
        {
            var data = _OrderAuditDmnService.Displayinformation(patList, this.OrganizeId);
            return Content(data.ToJson());
        }

        public ActionResult Inputresults()
        {

            return View();
        }

        public ActionResult Enteragain(string zyh, string yzid, string lrjg, string yzlb)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.Enteragain(user, zyh, yzid, lrjg, yzlb);
            return Success(data);

        }
        public ActionResult EnteragainMuti(string yzids, string lrjg)
        {
            OperatorModel user = this.UserIdentity;
            string msg = _OrderAuditDmnService.Enteragain(user, yzids, lrjg);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                return Error(msg);
            }
            else
            {
                return Success("保存成功");
            }

        }
        public ActionResult Drug_Inquiry()
        {
            return View();
        }
        public ActionResult Ward_Application()
        {
            return View();
        }
        /// <summary>
        /// 住院医嘱发药查询
        /// </summary>
        /// <param name="xm">病人姓名</param>
        /// <param name="bqCode">病区编码</param>
        /// <param name="ypmc">药品名称</param>
        /// <param name="cw">床位</param>
        /// <param name="kssj">开始时间</param>
        /// <param name="jssj">结束时间</param>
        /// <returns></returns>
        public ActionResult GetOrdersDrugsGridJson(Pagination pagination, string xm, string bqCode, string ypmc, string cw, string zyh, DateTime kssj, DateTime jssj)
        {
            var tt = _OrderAuditDmnService.GetOrdersDrugsGridJson(pagination, xm, bqCode, ypmc, cw, zyh, kssj, jssj, OrganizeId);
            var data = new
            {
                rows = tt,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson()); ;
        }

        public ActionResult SkinTestInfo()
        {

            return View();
        }
        public ActionResult GetSkinTestInfoGridJson(Pagination pagination, string zyh)
        {
            var tt = _OrderAuditDmnService.GetSkinTestInfoGridJson(pagination, zyh, OrganizeId);
            var data = new
            {
                rows = tt,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson()); ;
        }

        #region 科室备药申请管理
        /// <summary>
        /// 获取需要申领的病区
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetTheLowerKsCodeList(string keyword)
        {
            var data = _OrderAuditDmnService.GetTheLowerKsCodeList(OrganizeId, keyword);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取科室
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSysDepartmentList()
        {
            var data = _sysDepartmentRepo.GetList(OperatorProvider.GetCurrent().OrganizeId, "1");
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存科室备药
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SubmitKsby(BYDjInfoDTO Djnr)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.PrepareMedicine(user, OrganizeId, Djnr);
            return Success(data);
        }
        public ActionResult DrugAndStockSearch(string keyword, string yfbm)
        {
            var result = _OrderAuditDmnService.GetDrugAndStock(yfbm, keyword, OrganizeId);
            return Content(result.ToJson());
        }
        public ActionResult GetTheLowerYfbmCodeList(string keyword)
        {
            var data = _OrderAuditDmnService.GetTheLowerYfbmCodeList(keyword, OrganizeId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 单据号生成
        /// </summary>
        /// <param name="djmc"></param>
        /// <returns></returns>
        public ActionResult InitDjh(string djmc, string yfbmCode)
        {
            return Success(null, EFDBBaseFuncHelper.Instance.GetNewMedicineReceiptNo(djmc, yfbmCode, OrganizeId));
        }
        /// <summary>
        /// 备药单据提交
        /// </summary>
        /// <param name="djmc"></param>
        /// <returns></returns>
        public ActionResult Bydjtj(string ById)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.PrepareMedicineSubmit(ById, OrganizeId, user);
            return Success(data);
        }
        public ActionResult Bydjupdate(string ById)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.PrepareMedicineSubmit(ById, OrganizeId, user);
            return Success(data);
        }


        /// <summary>
        /// 科室备药申请
        /// </summary>
        /// <returns></returns>
        public ActionResult PrepareMedicineApply()
        {
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            return View();
        }
        /// <summary>
        /// 科室备药库存管理
        /// </summary>
        /// <returns></returns>
        public ActionResult PreparationStock()
        {
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            return View();
        }
        /// <summary>
        /// 科室备药列表
        /// </summary>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public ActionResult PreparationStockGridJson(Pagination pagination, string ypmc)
        {
            var data = new
            {
                rows = _OrderAuditDmnService.PreparationStockGridJson(this.OrganizeId, pagination: pagination, ypmc: ypmc),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 科室备药列表
        /// </summary>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public ActionResult PrepareMedicineApplyGridJson(Pagination pagination, string ksbyzt, DateTime? kssj = null, DateTime? jssj = null)
        {
            var data = new
            {
                rows = _OrderAuditDmnService.PrepareMedicineApplyGridJson(this.OrganizeId, pagination: pagination, ksbyzt: ksbyzt, kssj: kssj, jssj: jssj),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 单据明细
        /// </summary>
        /// <param name="djId"></param>
        /// <returns></returns>
        public ActionResult QueryPrepareMedicine(string djId)
        {
            var result = _OrderAuditDmnService.QueryPrepareMedicine(djId, this.OrganizeId);
            return Content(result.ToJson());
        }
        /// <summary>
        /// 获取药品库存
        /// </summary>
        /// <param name="djId"></param>
        /// <returns></returns>
        public ActionResult BydjQueryKykc(string ypbm, string pc, string ph, string yfbm)
        {
            var result = new
            {
                kysl = _OrderAuditDmnService.BydjQueryKykc(ypbm, pc, ph, yfbm, this.OrganizeId)
            };
            return Content(result.ToJson());
        }

        /// <summary>
        /// 主单据内容
        /// </summary>
        /// <param name="djId"></param>
        /// <returns></returns>
        public ActionResult QueryPrepareMedicinebyId(string djId)
        {
            var result = _OrderAuditDmnService.QueryPrepareMedicinebyId(djId, this.OrganizeId);
            return Content(result.ToJson());
        }
        /// <summary>
        /// 备药单据撤回
        /// </summary>
        /// <param name="djmc"></param>
        /// <returns></returns>
        public ActionResult Bydjback(string Djh)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.PrepareMedicineback(Djh, OrganizeId, user);
            return Success(data);
        }
        /// <summary>
        /// 备药单据撤回
        /// </summary>
        /// <param name="djmc"></param>
        /// <returns></returns>
        public ActionResult Bydjdelete(string ById)
        {
            OperatorModel user = this.UserIdentity;
            var data = _OrderAuditDmnService.PrepareMedicinedelete(ById, OrganizeId, user);
            return Success(data);
        }
        #endregion

    }
}
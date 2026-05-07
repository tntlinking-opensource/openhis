using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Newtouch.CIS.Web.Controllers
{
    public class OutpatientNurseController : OrgControllerBase
    {
        private readonly IOutpatientNurseDmnServise _OutpatientNurseDmnService;
        private readonly IOrderAuditDmnService _OrderAuditDmnService;

        #region View 视图
        // GET: OutpatientNurse
        /// <summary>
        /// 皮试执行页面
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformIndex()
        {
            return View();
        }
        /// <summary>
        /// 皮试查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult Skintestquery()
        {
            return View();
        }

        /// <summary>
        /// 门诊处方查询
        /// </summary>
        /// <returns></returns>
        public ActionResult prescriptionquery()
        {
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.ReportServerHOST = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            return View();
        }
        /// <summary>
        /// 门诊留观查询
        /// </summary>
        /// <returns></returns>
        public ActionResult observationQuery()
        {
            return View();
        }
        /// <summary>
        /// 门诊划价
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientPricing()
        {
            return View();
        }
        #endregion
        
        #region 皮试
        //皮试页面树控件
        public ActionResult SkintestTree(string keyword, DateTime? kssj, DateTime? jssj, string type)
        {
            var wardTree = _OutpatientNurseDmnService.OutpatientNurseTreeVO(this.OrganizeId, keyword, kssj, jssj, type);
            var wardonly = wardTree.GroupBy(p => new { p.ghksmc }).Select(p => new { p.Key.ghksmc });


            var treeList = new List<TreeViewModel>();
            foreach (var item in wardonly)
            {
                var patInfo = wardTree.Where(p => p.ghksmc == item.ghksmc).Where(p => p.mzh != "").Where(p => p.mzh != null).ToList();

                foreach (OutpatientNurseTreeVO itempat in patInfo)
                {
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.jzId;
                    treepat.text = itempat.mzh + "-" + itempat.xm;
                    treepat.value = itempat.jzId;
                    treepat.parentId = item.ghksmc;
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
                tree.id = item.ghksmc;
                tree.text = item.ghksmc;
                tree.value = item.ghksmc;
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

            IList<OutpatientNursequeryVO> list = new List<OutpatientNursequeryVO>();

            list = _OutpatientNurseDmnService.OutpatientNursequery(pagination, patList, OrganizeId);

            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }


        public ActionResult EnteragainMuti(string cfmxid, string lrjg)
        {
            OperatorModel user = this.UserIdentity;
            string msg = _OutpatientNurseDmnService.Enteragain(user, cfmxid, lrjg);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                return Error(msg);
            }
            else
            {
                return Success("保存成功");
            }

        }
        
        public ActionResult skintesfrom(Pagination pagination, DateTime? kssj, DateTime? jssj, string keyword)
        {
            IList<OutpatientNursequeryVO> list = new List<OutpatientNursequeryVO>();

            list = _OutpatientNurseDmnService.skintesfrom(pagination, keyword, kssj, jssj, OrganizeId);
            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());

        }

        public ActionResult skintescancel(string gmxxid)
        {
            OperatorModel user = this.UserIdentity;
            string msg = _OutpatientNurseDmnService.skintescancel(gmxxid, user);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                return Error(msg);
            }
            else
            {
                return Success("取消执行成功");
            }
        }
        #endregion

        #region 门诊病历打印
        /// <summary>
        /// 获取处方人员查询树
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPatcfTree(string keyword)
        {

            var wardTree = _OutpatientNurseDmnService.GetPatTree(OrganizeId, keyword);
            var wardonly = wardTree.GroupBy(p => new { p.ghksmc }).Select(p => new { p.Key.ghksmc });


            var treeList = new List<TreeViewModel>();
            foreach (var item in wardonly)
            {
                var patInfo = wardTree.Where(p => p.ghksmc == item.ghksmc).Where(p => p.mzh != "").Where(p => p.mzh != null).ToList();

                foreach (OutpatientNurseTreeVO itempat in patInfo)
                {
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.jzId;
                    treepat.text = itempat.mzh + "-" + itempat.xm;
                    treepat.value = itempat.mzh;
                    treepat.parentId = item.ghksmc;
                    treepat.isexpand = false;
                    treepat.complete = true;
                    treepat.showcheck = true;
                    treepat.checkstate = 0;
                    treepat.hasChildren = false;
                    treepat.Ex1 = "c";
                    treepat.Ex2 = itempat.sex;
                    treepat.Ex3 = itempat.nlshow;
                    treepat.Ex4 = itempat.xm;
                    treeList.Add(treepat);
                }

                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = patInfo.Count == 0 ? false : true;
                tree.id = item.ghksmc;
                tree.text = item.ghksmc;
                tree.value = item.ghksmc;
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

        public ActionResult GetBarCodeBycfh(string cfid)
        {
            var cfh = _OutpatientNurseDmnService.getcfh(cfid, OrganizeId);
            return Content(CommmHelper.GenerateBarCode(cfh, 25, 25));
        }
        public ActionResult prescriptionfrom(Pagination pagination, string jzid, DateTime? klrq, string cfdlb)
        {
            IList<OutpatientNursequeryVO> list = new List<OutpatientNursequeryVO>();

            list = _OutpatientNurseDmnService.prescriptionfrom(pagination, jzid, klrq, OrganizeId, cfdlb);
            if (list.Count > 0)
            {
                var Total = list.Sum(t => t.je);
                OutpatientNursequeryVO zje = new OutpatientNursequeryVO();
                zje.cfh = "总计"; zje.je = Total; zje.kssj = list[0].kssj;
                list.Add(zje);
            }
            foreach (var item in list)
            {
                if (item.cflx == 5 || item.cflx == 4)
                {
                    item.barcode = CommmHelper.GenerateBarCode(item.cfh, 25, 25);
                }
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
        #endregion

        #region 留观
        public ActionResult observationfrom(Pagination pagination, DateTime? kssj, DateTime? jssj, string keyword)
        {
            IList<ObservationFromVO> list = new List<ObservationFromVO>();

            list = _OutpatientNurseDmnService.observationquery(pagination, keyword, kssj, jssj, OrganizeId);
            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());

        }
        #endregion

        #region 门诊划价


        #endregion
    }
}
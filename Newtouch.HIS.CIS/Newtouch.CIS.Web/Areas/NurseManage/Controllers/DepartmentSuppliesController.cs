using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Utils;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ValueObjects;
using Newtouch.Infrastructure;
using Newtouch.PDS.Requset.Zyypyz;
using Newtouch.Tools;

namespace Newtouch.CIS.Web.Areas.NurseManage.Controllers
{
    public class DepartmentSuppliesController : OrgControllerBase
    {

        private readonly IWZsldDmnService _wZsldDmnService;
        // GET: NurseManage/sldAPPlication
        public ActionResult WzSldIndex()
        {
            return View();
        }

        #region  废弃代码  
        /// <summary>
        /// 申领单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewCkzksdh()
        {
            var result = "SLD" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
            return Content(result);
        }

        /// <summary>
        /// 下拉物资信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slksbm"></param>
        /// <returns></returns>

        [HttpGet]
        public ActionResult DepartmentStockListQuery(string key, string slksbm)
        {
            var param = new DepartmentStockListQueryParamDTO
            {
                key = key,
                organizeId = OrganizeId,
                warehouseId = UserIdentity.DepartmentCode,
                zt = "1"
            };
            var result = _wZsldDmnService.DepartmentStockListQuery(param);
            return Content(result.ToJson());
        }


        /// <summary>
        /// 下拉物资批号批次信息
        /// </summary>
        /// <param name="proId">物资ID</param>
        /// <param name="gysId">供应商ID</param>
        /// <param name="deliveryNo">配送单号</param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ProductBatchQuery(string proId, string slksbm, string keyword = "")
        {
            return Content(_wZsldDmnService.ProductBatchQuery(proId, UserIdentity.DepartmentCode, OrganizeId, keyword: keyword).ToJson());
        }

        /// <summary>
        /// 获取库房
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeptByKf(string keyword)
        {
            var list = _wZsldDmnService.GetList(OrganizeId, keyword ?? "");
            return Content(list.ToJson());
        }

        /// <summary>
        /// 获取病区
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeptBy(string keyword)
        {
            var list = _wZsldDmnService.GetDeptList(OrganizeId, keyword ?? "");
            return Content(list.ToJson());
        }
        #endregion

        #region 科室物资
        public ActionResult StorageQuery()
        {
            return View();
        }
        public ActionResult ExpiredStorageQuery()
        {
            ViewBag.OrgId = this.OrganizeId;
            return View();
        }
        
        public ActionResult ProfitAndLoss()
        {
            return View();
        }
        
        
        
        

        /// <summary>
        /// 获取物资类型
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPatientTreeSelectJson()
        {
            var warehouse = _wZsldDmnService.GetWzTreeSelectJson();
            var treeList = new List<TreeSelectModel>();
            foreach (var item in warehouse)
            {
                var treeModel = new TreeSelectModel
                {
                    id = item.Id,
                    text = item.name,
                    parentId = item.parentId
                };
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson(null));
        }

        public ActionResult GetWarehouseStorage(Pagination pagination, string ksCode, string keyWord, string lb, string xslkc)
        {
            var list = new
            {
                rows = _wZsldDmnService.GetProductStorage(pagination, ksCode, this.OrganizeId, (keyWord ?? "").Trim(), lb,  xslkc),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }
        /// <summary>
        /// 根据物资ID获取各批次库存
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="zt">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        public ActionResult GetWarehouseStorageDetail(string ks,string proId, string zt)
        {
            return Content(_wZsldDmnService.GetProductStorageDetail(ks, this.OrganizeId, proId, zt).ToJson());
        }

        /// <summary>
        /// 修改库存状态
        /// </summary>
        /// <param name="ph"></param>
        /// <param name="pc"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public ActionResult UpdateKcxxZt(string mxId, string zt)
        {
            return _wZsldDmnService.UpdateZt(mxId, this.OrganizeId, zt) > 0 ? Success() : Error("修改查库存状态失败");
        }
        /// <summary>
        /// 同步物资系统耗材
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public ActionResult UpdateSyncWz()
        {
            _wZsldDmnService.UpdateSyncWz(this.OrganizeId,this.UserIdentity.UserCode);
            return Success();
        }
        
        /// <summary>
        /// 获取过期物资
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetExpiredWarehouseStorage(Pagination pagination, string ksCode, string keyWord, string lb, string xslkc)
        {
            var list = new
            {
                rows = _wZsldDmnService.GetExpiredProductStorage(pagination, ksCode, this.OrganizeId, (keyWord ?? "").Trim(), lb,  xslkc),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }

        #endregion
    }
}
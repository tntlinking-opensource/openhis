using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtouch.Core.Common;
using Newtouch.Herp.Application.Implementation.DeliveryAutoProcss;
using Newtouch.Herp.Application.Interface;
using Newtouch.Herp.Domain.DTO.InputDto;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Domain.IRepository;
using Newtouch.Herp.Infrastructure;
using Newtouch.Herp.Infrastructure.Common;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.Tools;

namespace Newtouch.Herp.Web.Areas.StorageManage.Controllers
{
    /// <summary>
    /// 库存管理
    /// </summary>
    public class StorageController : ControllerBase
    {
        private readonly IStorageManageDmnService _storageDmnService;
        private readonly IGysSupplierRepo _gysSupplierRepo;
        private readonly IWzCrkfsRepo _CrkFs;
        private readonly IStorageApp _storageApp;
        private readonly IKfCrkdjDmnService _kfCrkdjDmnService;
        private readonly IKfWarehouseRepo _kfWarehouseRepo;
        private readonly IKfKcxxRepo _kfKcxxRepo;
        private readonly IRelWarehouseDeptRepo iRelWarehouseDeptRepo;
        private readonly IRelProductWarehouseRepo _relProductWarehouseRepo;
        private readonly IApplyBillApp _applyBillApp;

        #region 物资获取


        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="gysId"></param>
        /// <param name="deliveryNo"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DepartmentStockListQuery(string key, string gysId, string deliveryNo)
        {
            var param = new DepartmentStockListQueryParam
            {
                gysId = gysId,
                key = key,
                organizeId = OrganizeId,
                warehouseId = Constants.CurrentKf.currentKfId,
                deliveryNo = deliveryNo,
                zt = "1"
            };
            var result = _storageDmnService.DepartmentStockListQuery(param);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="warehouseId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StockQueryByWarehouse(string key, string warehouseId)
        {
            var result = _storageDmnService.GetProductStorage(warehouseId, OrganizeId, key);
            return Content(result.ToJson());
        }
        /// <summary>
        /// 物资权限判断
        /// </summary>
        /// <param name="targetDepartment"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ProductJurisdiction(string targetDepartment, string productId)
        {
            var entity = _relProductWarehouseRepo.FindEntity(p => p.productId == productId && p.warehouseId == targetDepartment && p.OrganizeId == OrganizeId && p.zt == ((int)Enumzt.Enable).ToString());
            return entity != null && !string.IsNullOrWhiteSpace(entity.Id) ? Success() : Error("目标库房没有权限使用该物资，请确保物资已同步");
        }

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="key">物资名称/拼音</param>
        /// <returns></returns>
        public ActionResult DepartmentStockListQueryByPage(Pagination pagination, string key)
        {
            var list = new
            {
                rows = _storageDmnService.DepartmentStockListQuery(pagination, key, Constants.CurrentKf.currentKfId, OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }

        /// <summary>
        /// 下拉列表物资信息  外部出库
        /// </summary>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="gysId"></param>
        /// <param name="deliveryNo">配送单号</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryStockListByGys(string key, string deliveryNo, string gysId)
        {
            var result = _storageDmnService.DepartmentStockListQuery(key, gysId, deliveryNo, Constants.CurrentKf.currentKfId, OrganizeId);
            return Content(result.ToJson());
        }

        #endregion

        #region 库存量查询

        /// <summary>
        /// 库存量查询视图
        /// </summary>
        /// <returns></returns>
        public ActionResult WarehouseStorageQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            return View();
        }

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyWord">物资名称/拼音</param>
        /// <param name="lb">类别ID</param>
        /// <param name="kzbz">控制标志</param>
        /// <param name="wzzt">物资状态</param>
        /// <param name="xslkc">显示零库存  1：显示  0：不显示</param>
        /// <param name="ygq">已过期</param>
        /// <param name="mxyx">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        public ActionResult GetWarehouseStorage(Pagination pagination,
            string keyWord,
            string lb,
            string kzbz,
            string wzzt,
            string xslkc,
            string ygq,
            string mxyx,string kccg=null)
        {
            var list = new
            {
                rows = _storageDmnService.GetProductStorage(pagination, Constants.CurrentKf.currentKfId, OrganizeId, (keyWord ?? "").Trim(), lb, kzbz, wzzt, xslkc, ygq, mxyx,kccg),
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
        public ActionResult GetWarehouseStorageDetail(string proId, string zt)
        {
            return Content(_storageDmnService.GetProductStorageDetail(Constants.CurrentKf.currentKfId, OrganizeId, proId, zt).ToJson());
        }

        /// <summary>
        /// 修改库存状态
        /// </summary>
        /// <param name="ph"></param>
        /// <param name="pc"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public ActionResult UpdateKcxxZt(string proId, string ph, string pc, string zt)
        {
            return _kfKcxxRepo.UpdateZt(proId, ph, pc, zt) > 0 ? Success() : Error("修改查库存状态失败");
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
        public ActionResult ProductBatchQuery(string proId, string gysId = "", string deliveryNo = "", string keyword = "")
        {
            return Content(_storageDmnService.ProductBatchQuery(proId, Constants.CurrentKf.currentKfId, OrganizeId, gysId, deliveryNo, keyword: keyword).ToJson());
        }

        /// <summary>
        /// 获取供应商列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSupplierList(string key)
        {
            var list = _gysSupplierRepo.IQueryable(p => p.zt == ((int)Enumzt.Enable).ToString() && (p.py.Contains(key) || p.name.Contains(key) || string.IsNullOrEmpty(key)) && (p.supplierType == null || p.supplierType == 2)).ToList();
            return Content(list.ToJson());
        }
        #endregion

        #region 过期库存查询

        /// <summary>
        /// 过期库存 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult ExpiredStorageQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            return View();
        }

        /// <summary>
        /// 获取过期物资
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetExpiredStorageDetail(Pagination pagination,
            string keyWord,
            string lb,
            string kzbz,
            string wzzt,
            string xslkc,
            string mxyx)
        {
            var result = _storageDmnService.SelectStorageDetail(pagination, Constants.CurrentKf.currentKfId, OrganizeId, keyWord, lb, kzbz, wzzt, xslkc, mxyx, true);
            return Content(result.ToJson());
        }

        #endregion

        #region 外部入库

        /// <summary>
        /// 外部入库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult InStorage()
        {
            return View();
        }

        /// <summary>
        /// 外部入库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult InStorageInlineEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取新的入库单号
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateRkdh()
        {
            var rkdh = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.Wbrk.GetDescription());
            return Success(rkdh);
        }

        /// <summary>
        /// 获取出入库方式
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStorageIOMode(string crkbz)
        {
            var list = _CrkFs.IQueryable(p => p.zt == ((int)Enumzt.Enable).ToString() && p.crkbz == crkbz).ToList();
            return Content(list.ToJson());
        }

        /// <summary>
        /// 入库
        /// </summary>
        /// <param name="crkdj">出入库单据</param>
        /// <param name="crkdjmx">出入库单据明细</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveInStorageInfo(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            crkdj.auditState = ((int)EnumAuditState.Waiting).ToString();
            var result = _storageApp.InStorageSubmit(crkdj, crkdjmx);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 入库
        /// </summary>
        /// <param name="crkdj">出入库单据</param>
        /// <param name="crkdjmx">出入库单据明细</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TemporaryStorageInStorageInfo(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            crkdj.auditState = ((int)EnumAuditState.Temporary).ToString();
            var result = _storageApp.InStorageSubmit(crkdj, crkdjmx);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        #endregion

        #region 外部出库

        /// <summary>
        /// 外部出库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult OutStorage()
        {
            return View();
        }

        /// <summary>
        /// 外部出库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult OutStorageInlineEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取新的入库单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewCkdh()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.Wbck.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 获取发票信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFphAndGys(string keyWord)
        {
            return Content(_kfCrkdjDmnService.GetFphAndGysInfo(keyWord, Constants.CurrentKf.currentKfId, OrganizeId).ToJson());
        }

        /// <summary>
        /// 外部出库
        /// </summary>
        /// <param name="crkdj"></param>
        /// <param name="crkdjmx"></param>
        /// <returns></returns>
        public ActionResult SaveOutStorageInfo(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            var result = _storageApp.OutStorageSubmit(crkdj, crkdjmx);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        #endregion

        #region 直接出库 

        /// <summary>
        /// 直接出库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DirectDelivery()
        {
            return View();
        }

        /// <summary>
        /// 直接出库 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DirectDeliveryInlineEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取新的直接出库单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewZjckdh()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.Zjck.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 获取入库部门(兄弟节点和子节点)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetBrothersOrChildren(string keyword)
        {
            return Content(_kfWarehouseRepo.GetBrothersOrChildren(Constants.CurrentKf.currentKfId, OrganizeId, keyword).ToJson());
        }

        /// <summary>
        /// 提交直接出库
        /// </summary>
        /// <param name="crkdj"></param>
        /// <param name="crkdjmx"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveDirectDelivery(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            var result = _storageApp.DirectDeliverySubmit(crkdj, crkdjmx);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        #endregion

        #region 内部发货退回

        /// <summary>
        /// 内部发货退回 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DeliveryOfReturn()
        {
            return View();
        }

        /// <summary>
        /// 内部发货退回 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DeliveryOfReturnInlineEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取新的内部发货退回单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewNbthdh()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.Nbth.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 提交内部发货退回
        /// </summary>
        /// <param name="crkdj"></param>
        /// <param name="crkdjmx"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveDeliveryOfReturn(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            var result = _storageApp.DeliveryOfReturnSubmit(crkdj, crkdjmx);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 获取部门(父节点和兄弟节点)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetParentOrBrothers(string keyword)
        {
            return Content(_kfWarehouseRepo.GetParentOrBrothers(Constants.CurrentKf.currentKfId, OrganizeId, keyword).ToJson());
        }
        #endregion

        #region  出库至科室

        /// <summary>
        /// 出库至科室 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DeliveryToDepartment()
        {
            return View();
        }

        /// <summary>
        /// 出库至科室 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DeliveryToDepartmentInlineEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取新的出库至科室单据号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewCkzksdh()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.chukuzhikeshi.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 获取库房对应科室
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeptByKf(string keyword)
        {
            var list = iRelWarehouseDeptRepo.GetList(Constants.CurrentKf.currentKfId, OrganizeId, keyword ?? "");
            return Content(list.ToJson());
        }

        /// <summary>
        /// 保存出库到科室
        /// </summary>
        /// <param name="crkdj"></param>
        /// <param name="crkdjmx"></param>
        /// <returns></returns>
        public ActionResult SaveDeliveryToDepartment(KfCrkdjEntity crkdj, KfCrkmxEntity[] crkdjmx)
        {
            var result = new DeliveryToDepartmentProcess(new DjInfDTO { crkdj = crkdj, crkdjmx = crkdjmx.ToList() }).Process();
            if (result.IsSucceed)//同步科室物资-CIS
                _storageDmnService.SynchWz(crkdj.Pdh,this.UserIdentity.UserCode,this.OrganizeId);
            return result.IsSucceed ? Success() : Error(result.ResultMsg);
        }
        #endregion

        #region 科室申领

        /// <summary>
        /// 视图
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentApply()
        {
            return View();
        }

        /// <summary>
        /// 获取新的科室申领单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewKsSldh()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.kssl.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 提交科室深林
        /// </summary>
        /// <param name="sld"></param>
        /// <param name="sldmx"></param>
        /// <returns></returns>
        public ActionResult SubmitDepartmentApply(KfApplyOrderEntity sld, List<KfApplyOrderDetailEntity> sldmx)
        {
            var result = _applyBillApp.SubmitDepartmentApply(sld, sldmx, OrganizeId);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }
        #endregion

        #region 申领出库

        /// <summary>
        /// 申领出库
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyOutStock()
        {
            return View();
        }

        /// <summary>
        /// 提交申领发货
        /// </summary>
        /// <param name="ckmx"></param>
        /// <returns></returns>
        public ActionResult SubmitApplyOutStock(List<VApplyBillDetailEntity> ckmx)
        {
            var result = _applyBillApp.SubmitApplyOutStock(ckmx, OrganizeId, UserIdentity.UserCode);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }
        #endregion
    }
}

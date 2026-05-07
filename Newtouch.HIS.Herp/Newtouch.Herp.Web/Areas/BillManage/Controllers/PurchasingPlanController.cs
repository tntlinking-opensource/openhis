using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtouch.Core.Common;
using Newtouch.Herp.Application.Interface;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Infrastructure.Common;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.Tools;

namespace Newtouch.Herp.Web.Areas.BillManage.Controllers
{
    /// <summary>
    /// 采购计划
    /// </summary>
    public class PurchasingPlanController : ControllerBase
    {
        private readonly IPurchasingPlanApp _purchasingPlanApp;
        private readonly ICgPurchaseOrderDmnService _cgPurchaseOrderDmnService;

        #region 采购计划填写

        /// <summary>
        /// 填写采购计划视图
        /// </summary>
        /// <returns></returns>
        public ActionResult FillPurchasingPlan()
        {
            return View();
        }

        /// <summary>
        /// 获取新的采购计划单号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNewPurchasingPlanBillNo()
        {
            var result = ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.purchasingPlan.GetDescription());
            return Content(result);
        }

        /// <summary>
        /// 提交采购计划
        /// </summary>
        /// <param name="pp"></param>
        /// <param name="ppmx"></param>
        /// <returns></returns>
        public ActionResult SubmitFillPurchasingPlan(CgPurchaseOrderEntity pp, List<CgPurchaseOrderDetailEntity> ppmx)
        {
            var userCode = UserIdentity.UserCode;
            pp.rkbmCode = pp.rkbmCode ?? this.UserIdentity.DepartmentCode;
            pp.cgdh= pp.cgdh ?? ReceiptNoManage.GetNewReceiptNo(EnumOutOrInStorageBillType.purchasingPlan.GetDescription());
            pp.zt = ((int)Enumzt.Enable).ToString();
            var result = _purchasingPlanApp.SubmitFillPurchasingPlan(pp, ppmx, userCode, OrganizeId);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 检查是否是暂存单
        /// </summary>
        /// <param name="cgdh"></param>
        /// <returns></returns>
        public ActionResult CheckIsTemporaryBill(string cgdh)
        {
            var entity = new CgPurchaseOrderEntity();
            var result = _purchasingPlanApp.CheckIsAuditRefuseOrTemporaryBill(cgdh, OrganizeId, ref entity);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        #endregion

        #region 审核采购计划

        /// <summary>
        /// 采购计划审核
        /// </summary>
        /// <returns></returns>
        public ActionResult AuditPurchasingPlan()
        {
            return View();
        }

        /// <summary>
        /// 审核采购计划
        /// </summary>
        /// <param name="cgdh"></param>
        /// <param name="auditState"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public ActionResult SubmitAuditPurchasingPlan(string cgdh, int auditState, string remarks)
        {
            var result = _purchasingPlanApp.SubmitAuditPurchasingPlan(cgdh, auditState, remarks, OrganizeId, UserIdentity.UserCode);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }
        #endregion

        #region 我的采购

        /// <summary>
        /// 采购计划查询
        /// </summary>
        /// <returns></returns>
        public ActionResult PurchasingPlanQuery()
        {
            return View();
        }

        /// <summary>
        /// 获取我的采购计划
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="auditState"></param>
        /// <returns></returns>
        public ActionResult GetPurchasingPlanInfo(Pagination pagination, string cgdh, DateTime kssj, DateTime jssj, int auditState)
        {
            var userCode = UserIdentity.UserCode;
            var result = _cgPurchaseOrderDmnService.SelectPurchaseOrder(pagination, cgdh, kssj, jssj, auditState, userCode, OrganizeId);
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取审核通过的采购计划
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="deptCode"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public ActionResult GetAdoptPurchasingPlanInfo(Pagination pagination, string cgdh, string deptCode, DateTime kssj, DateTime jssj)
        {
            var result = _cgPurchaseOrderDmnService.SelectAdoptPurchasingPlanInfo(pagination, cgdh, deptCode, kssj, jssj, OrganizeId);
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取待审核采购计划
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="auditState"></param>
        /// <returns></returns>
        public ActionResult GetAuditPurchasingPlanInfo(Pagination pagination, string cgdh, DateTime kssj, DateTime jssj, int auditState)
        {
            var userCode = UserIdentity.UserCode;
            var result = _cgPurchaseOrderDmnService.SelectPurchaseOrder(pagination, cgdh, kssj, jssj, auditState, userCode, OrganizeId);
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取我的采购计划明细
        /// </summary>
        /// <param name="cgdh"></param>
        /// <returns></returns>
        public ActionResult GetPurchasingPlanDetail(string cgdh)
        {
            if (string.IsNullOrWhiteSpace(cgdh)) return Content(new List<VCgPurchaseOrderDetailEntity>().ToJson());
            var cgdhArr = cgdh.Split(',');
            if (cgdhArr.Length == 0) return Content(new List<VCgPurchaseOrderDetailEntity>().ToJson());
            var result = new List<VCgPurchaseOrderDetailEntity>();
            foreach (var t in cgdhArr)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;
                result.AddRange(_cgPurchaseOrderDmnService.SelectPurchaseOrderDetail(t, OrganizeId));
            }
            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取待采购的采购计划明细
        /// </summary>
        /// <param name="cgdh"></param>
        /// <returns></returns>
        public ActionResult GetWaitingPurchasePlanDetail(string cgdh)
        {
            if (string.IsNullOrWhiteSpace(cgdh)) return Content(new List<VCgPurchaseOrderDetailEntity>().ToJson());
            var cgdhArr = cgdh.Split(',');
            if (cgdhArr.Length == 0) return Content(new List<VCgPurchaseOrderDetailEntity>().ToJson());
            var result = new List<VCgPurchaseOrderDetailEntity>();
            foreach (var t in cgdhArr)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;
                result.AddRange(_cgPurchaseOrderDmnService.SelectWaitingPurchasePlanDetail(t, OrganizeId));
            }
            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取审核通过的采购计划
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="deptCode"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public ActionResult GetAdoptWaitingPurchasePlanInfo(Pagination pagination, string cgdh, string deptCode, DateTime kssj, DateTime jssj)
        {
            var result = _cgPurchaseOrderDmnService.SelectAdoptWaitingPurchasePlanInfo(pagination, cgdh, deptCode, kssj, jssj, OrganizeId);
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 作废采购计划
        /// </summary>
        /// <param name="cgdh"></param>
        /// <returns></returns>
        public ActionResult CancelPurchasingPlan(string cgdh)
        {
            if (string.IsNullOrWhiteSpace(cgdh)) return Error("采购计划单号不能为空");
            var userCode = UserIdentity.UserCode;
            var result = _purchasingPlanApp.CancelPurchasingPlan(cgdh, OrganizeId, userCode);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 将暂存采购计划提交审核
        /// </summary>
        /// <param name="cgdh"></param>
        /// <returns></returns>
        public ActionResult SubmitPurchasingPlan(string cgdh)
        {
            var result = _purchasingPlanApp.SubmitPurchasingPlan(cgdh, OrganizeId);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }
        #endregion

        #region 库存预警生成采购计划单
        public ActionResult PurchasingPlan()
        {
            return View();
        }

        #endregion

    }
}
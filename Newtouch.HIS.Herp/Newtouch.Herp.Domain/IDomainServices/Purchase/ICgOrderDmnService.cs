using System;
using System.Collections.Generic;
using Newtouch.Core.Common;
using Newtouch.Herp.Domain.Entity.VEntity;

namespace Newtouch.Herp.Domain.IDomainServices
{
    /// <summary>
    /// 采购单
    /// </summary>
    public interface ICgOrderDmnService
    {

        /// <summary>
        /// 查询采购单信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="orderNo"></param>
        /// <param name="orderType"></param>
        /// <param name="orderProcess"></param>
        /// <param name="organizeId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        IList<VCgOrderEntity> SelectCgOrder(Pagination pagination, string orderNo, int orderType,
           int orderProcess, string organizeId, DateTime kssj, DateTime jssj);

        /// <summary>
        /// 查询采购单明细
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VCgOrderDetailEntity> SelectCgOrderDetailGroupByCgdh(string orderNo, string organizeId);

        /// <summary>
        /// 查询采购单明细
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VCgOrderDetailEntity> SelectCgOrderDetailNoCgdh(string orderNo, string organizeId);
        /// <summary>
        /// 外部入库-采购单引用from视图
        /// </summary>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="fph"></param>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VCgOrderRkEntity> SelectCgOrderView(DateTime kssj, DateTime jssj, string fph, string orderNo, string organizeId);

        /// <summary>
        /// 外部入库-采购单引用
        /// </summary>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="fph"></param>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VCgOrderDetailEntity> SelectCgOrderDetail(string subOrderNo, string organizeId);
        /// <summary>
        /// 采购单明细导入入库列表
        /// </summary>
        /// <param name="subOrderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VCgOrderImportEntity> SelectCgOrderDetailImport(string subOrderNo, string organizeId);
        /// <summary>
        /// 补填采购单发票号
        /// </summary>
        /// <param name="subOrderNo"></param>
        /// <param name="fph"></param>
        /// <param name="userCode"></param>
        void PurchaseDdbhUpdate(string subOrderNo, string fph, string userCode);
    }
}
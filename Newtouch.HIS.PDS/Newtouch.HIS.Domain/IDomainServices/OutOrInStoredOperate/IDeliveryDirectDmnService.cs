using System;
using System.Collections.Generic;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.ValueObjects.DrugStorage;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 直接出库
    /// </summary>
    public interface IDeliveryDirectDmnService
    {
        /// <summary>
        /// 提交直接出库申请
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <returns></returns>
        string SubmitDeliveryDirect(SysMedicineStorageIOReceiptEntity dj, List<SysMedicineStorageIOReceiptDetailEntity> mx);
        /// <summary>
        /// 获取入库发票
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        List<BillFphVo> GetRkFphData(int djlx, string fph, DateTime kssj, DateTime jssj);
        /// <summary>
        /// 药库入库发票明细
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        List<BillFphMxVo> GetRkFphMxData(int djlx, string fph, string pc);

    }
}
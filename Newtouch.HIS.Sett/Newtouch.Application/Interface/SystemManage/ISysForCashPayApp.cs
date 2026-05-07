using System;
using System.Collections.Generic;
using System.Data;
using Newtouch.HIS.Domain.Entity;

namespace Newtouch.HIS.Application
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISysForCashPayApp
    {
        List<SysCashPaymentModelEntity> GetList(string keyword = null);

        SysCashPaymentModelEntity GetForm(int keyValue);

        void DeleteForm(int keyValue);

        void SubmitForm(SysCashPaymentModelEntity sysForCashPayEntity, int? keyValue);

        DataTable GetListBySearch(string keyword);

        /// <summary>
        /// 获取系统现金支付方式 缓存下拉列表
        /// </summary>
        /// <returns></returns>
        object GetCashPay();
    }
}

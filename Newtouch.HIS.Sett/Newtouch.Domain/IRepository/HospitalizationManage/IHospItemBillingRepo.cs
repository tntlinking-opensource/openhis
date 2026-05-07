using System.Data;
using Newtouch.HIS.Domain.Entity;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IRepository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHospItemBillingRepo : IRepositoryBase<HospItemBillingEntity>
    {
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="hospItemFeeEntity"></param>
        /// <param name="keyValue"></param>
        void SubmitForm(HospItemBillingEntity hospItemFeeEntity, int? keyValue);

        /// <summary>
        /// 查询 时间段内的 项目计费EntityList
        /// 已考虑退费
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sourceQuery"></param>
        /// <returns></returns>
        IList<HospItemBillingEntity> GetItemFeeEntityListByTime(string zyh, string orgId, DateTime startTime, DateTime endTime
            , IQueryable<HospItemBillingEntity> sourceQuery = null);
        
        /// <summary>
        /// 记账中物资耗材库存扣减
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="yfbm"></param>
        /// <param name="userCode"></param>
        void Updatezyaddfee(string OrganizeId, string jfbhs, string yfbm,string userCode);
    }
}

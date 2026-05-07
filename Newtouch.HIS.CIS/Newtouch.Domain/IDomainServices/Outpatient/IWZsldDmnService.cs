using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Storage;

namespace Newtouch.Domain.IDomainServices
{
    public interface IWZsldDmnService
    {

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        List<VSelProductInfoVO> DepartmentStockListQuery(DepartmentStockListQueryParamDTO param);

        /// <summary>
        /// 获取物资批号批次信息  top 20
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="gysId"></param>
        /// <param name="deliveryNo"></param>
        /// <returns></returns>
        List<VProductBatchInfoVO> ProductBatchQuery(string productId, string warehouseId, string organizeId,  string keyword = "");

        List<RelWarehouseVO> GetList(string organizeId, string keyword);
        List<RelWarehouseVO> GetDeptList(string organizeId, string keyword);

        #region  科室物资管理
        List<WzTypeVo> GetWzTreeSelectJson();

        IList<VProductStorageEntity> GetProductStorage(Pagination pagination, string ks, string organizeId, string keyWord, string lbId, string xslkc);
        
        IList<VProductStorageEntity> GetExpiredProductStorage(Pagination pagination, string ks, string organizeId, string keyWord, string lbId, string xslkc);

        IList<VProductStorageDetailEntity> GetProductStorageDetail(string ks, string OrganizeId, string proId, string zt);

        int UpdateZt(string mxId, string OrganizeId, string zt);

        void UpdateSyncWz(string OrganizeId, string userCode);
        #endregion

    }
}

using System.Collections.Generic;
using Newtouch.Core.Common;
using Newtouch.Herp.Domain.DTO.InputDto;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.HIS.Domain.ValueObjects;

namespace Newtouch.Herp.Domain.IDomainServices
{
    /// <summary>
    /// 库存管理
    /// </summary>
    public interface IStorageManageDmnService
    {

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        List<VSelProductInfoEntity> DepartmentStockListQuery(DepartmentStockListQueryParam param);

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        List<VSelProductInfoEntity> DepartmentStockListQuery(Pagination pagination, string key, string warehouseId, string organizeId, string zt = "1");

        /// <summary>
        /// 下拉列表物资信息 外部出库
        /// </summary>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="ckbm"></param>
        /// <param name="deliveryNo"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        List<VSelProductInfoEntity> DepartmentStockListQuery(string key, string ckbm, string deliveryNo, string warehouseId, string organizeId, string zt = "1");


        /// <summary>
        /// 获取物资批号批次信息  top 20
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="gysId"></param>
        /// <param name="deliveryNo"></param>
        /// <returns></returns>
        List<VProductBatchInfoEntity> ProductBatchQuery(string productId, string warehouseId, string organizeId, string gysId = "", string deliveryNo = "", string keyword = "");

        /// <summary>
        /// 外部入库单据
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mxList"></param>
        string SaveOutOrInStorageInfo(KfCrkdjEntity dj, List<KfCrkmxEntity> mxList);

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="lbId"></param>
        /// <param name="kzbz"></param>
        /// <param name="wzzt"></param>
        /// <param name="xslkc">显示零库存  1：显示  0：不显示</param>
        /// <param name="ygq">已过期</param>
        /// <param name="mxyx">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        IList<VProductStorageEntity> GetProductStorage(Pagination pagination,
           string warehouseId,
           string organizeId,
           string keyWord,
           string lbId,
           string kzbz,
           string wzzt,
           string xslkc,
           string ygq,
           string mxyx, string kccg = null);

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        IList<VProductStorageEntity> GetProductStorage(string warehouseId, string organizeId, string keyWord);

        /// <summary>
        /// 物资库存分批次批号汇总
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="proId">物资ID</param>
        /// <param name="zt">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        IList<VProductStorageDetailEntity> GetProductStorageDetail(string warehouseId, string organizeId, string proId, string zt);

        /// <summary>
        /// 外部入库 审核通过
        /// </summary>
        /// <param name="kcxx"></param>
        /// <param name="dj"></param>
        void WbrkAdopt(List<KfKcxxEntity> kcxx, KfCrkdjEntity dj);

        /// <summary>
        /// 外部入库 作废
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="dj"></param>
        /// <returns></returns>
        string WbrkCancelled(List<KfCrkmxEntity> mx, KfCrkdjEntity dj);

        /// <summary>
        /// 外部出库/直接出库/内部发货退回
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mxList"></param>
        /// <returns></returns>
        string Wbck(KfCrkdjEntity dj, List<KfCrkmxEntity> mxList);

        /// <summary>
        /// 外部出库，扣库存，审核通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string SubtractKc(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode);

        /// <summary>
        /// 外部出库/直接出库/内部发货退回，解冻，审核不通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string Unfreeze(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode);

        /// <summary>
        /// 外部出库 作废
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="dj"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string WbckCancelled(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode);

        /// <summary>
        /// 直接出库 审核通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string DirectDeliveryAdopt(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode);

        /// <summary>
        /// 直接出库/内部发货退回 作废操作
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string DirectDeliveryCancel(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode);

        /// <summary>
        /// 出库数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VPsiStatisticsByDateEntity> GetCkCountByKf(string warehouseId, string organizeId);

        /// <summary>
        /// 入库数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VPsiStatisticsByDateEntity> GetRkCountByKf(string warehouseId, string organizeId);

        /// <summary>
        /// 损益数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VPsiStatisticsByDateEntity> GetSyCountByKf(string warehouseId, string organizeId);

        /// <summary>
        /// 按单据类型获取入库总数
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<ClassificationStatisticsEntity> GetRkCountByLx(string warehouseId, string organizeId);


        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="lbId"></param>
        /// <param name="kzbz"></param>
        /// <param name="wzzt"></param>
        /// <param name="xslkc">显示零库存  1：显示  0：不显示</param>
        /// <param name="mxyx">暂时有效的明细  true：是  false：否</param>
        /// <param name="isExpired">true:显示过期  false：显示不过期</param>
        /// <returns></returns>
        IList<VProductStorageDetailEntity> SelectStorageDetail(Pagination pagination,
           string warehouseId,
           string organizeId,
           string keyWord,
           string lbId,
           string kzbz,
           string wzzt,
           string xslkc,
           string mxyx,
           bool isExpired);
        
        /// <summary>
        /// 获取过期预警和库存预警总条数
        /// </summary>
        /// <param name="yfbm"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="gqyjz"></param>
        /// <returns></returns>
        IList<SysMSGQueryVO> MSGQuery(string yfbm, string OrganizeId, int gqyjz);

        /// <summary>
        /// 出库至科室-同步物资到CIS
        /// </summary>
        /// <param name="djh"></param>
        /// <param name="organizeId"></param>
        void SynchWz(string djh, string UserCode, string organizeId);
    }
}
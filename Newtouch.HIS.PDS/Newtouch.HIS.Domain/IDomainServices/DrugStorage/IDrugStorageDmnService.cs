using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity;
using System.Collections;
using System.Collections.Generic;
using Newtouch.HIS.Domain.Entity.V;
using Newtouch.HIS.Domain.DTO.DrugStorage;
using Newtouch.HIS.Domain.ValueObjects;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 库存应用
    /// </summary>
    public interface IDrugStorageDmnService
    {

        /// <summary>
        /// 查询当前部门药品(入库)
        /// </summary>
        /// <param name="keyword">ypmc、py、spm、ypCode</param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<DepartmentMedicineVO> SelectDepartmentMedicineList(string keyword, string yfbmCode, string organizeId);

        /// <summary>
        /// 药品发票信息
        /// </summary>
        /// <param name="fph"></param>
        /// <returns></returns>
        List<MedicineInvoiceInfoVO> SelectMedicineListByFPH(string fph);

        /// <summary>
        /// 查询当前部门药品(出库)
        /// </summary>
        /// <param name="keyword">ypmc、py、spm、ypCode</param>
        /// <param name="fph"></param>
        /// <param name="gyscode"></param>
        /// <returns></returns>
        List<DepartmentMedicineVO> SelectDepartmentMedicineList2(string keyword, string fph, string gyscode);

        /// <summary>
        /// 获取申领单信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="sldh">申领单号</param>
        /// <param name="slbm">申领部门</param>
        /// <param name="ffzt">发药状态</param>
        /// <param name="txtStartDate">开始日期</param>
        /// <param name="txtEndDate">结束日期</param>
        /// <returns></returns>
        IList<SysMedicineReInfoVO> GetMedicineRequestInfo(Pagination pagination, string sldh, string slbm, string ffzt, string txtStartDate = "", string txtEndDate = "");

        /// <summary>
        /// 获取申领单明细
        /// </summary>
        /// <param name="sldId"></param>
        /// <returns></returns>
        List<SysMedicineReDetailVO> GetMedicineDetail(string sldId);

        /// <summary>
        /// 入库
        /// </summary>
        /// <param name="ioReceiptEntity"></param>
        /// <param name="ioReceiptDetailList"></param>
        void SaveInStorageInfo(SysMedicineStorageIOReceiptEntity ioReceiptEntity, List<SysMedicineStorageIOReceiptDetailEntity> ioReceiptDetailList);

        /// <summary>
        /// 出库
        /// </summary>
        /// <param name="ioReceiptEntity"></param>
        /// <param name="ioReceiptDetailList"></param>
        void SaveOutStorageInfo(SysMedicineStorageIOReceiptEntity ioReceiptEntity, List<SysMedicineStorageIOReceiptDetailVO> ioReceiptDetailList);

        /// <summary>
        /// 调价申请 查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        IList<AdjustPriceMedicineInfoVO> SelectAdjustPriceMedicineInfoList(Pagination pagination, string inputCode);

        /// <summary>
        /// 调价审核查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="inputCode"></param>
        /// <param name="shzt"></param>
        /// <returns></returns>
        IList<AdjustPriceMedicineInfoVO> SelectMedicineAdjustPriceApprovalInfoList(Pagination pagination, string inputCode, string shzt);

        /// <summary>
        /// 药品调价执行
        /// </summary>
        /// <param name="ypCodeList"></param>
        string ExecteAdjustPrice(ArrayList ypCodeList);

        /// <summary>
        /// 调价历史查询
        /// </summary>
        /// <param name="inputCode"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<AdjustPriceHistoryInfoVO> SelectMedicineAdjustPriceHistoryInfoList(Pagination pagination, string inputCode, string startTime, string endTime);

        /// <summary>
        /// 终止申领出库状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        void UpgradeStatus(string id);

        /// <summary>
        /// 进销存统计
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="searchParam"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<PsiStatisticsVo> GetPsiStatisticsVo(Pagination pagination, PsiStatisticsParam searchParam, string yfbmCode, string organizeId);

        /// <summary>
        /// 获取品，不分批次批号，含有效期
        /// </summary>
        /// <param name="srm">关键字</param>
        /// <param name="yfbmCode">当前部门</param>
        /// <param name="organizeId"></param>
        /// <param name="topCount"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetMedicinesrmList(string srm, string yfbmCode, string organizeId, int topCount = 100);

        /// <summary>
        /// 获取在有效期内的药品，不分批次批号，不含有效期
        /// </summary>
        /// <param name="srm">关键字</param>
        /// <param name="yfbmCode">当前部门</param>
        /// <param name="organizeId"></param>
        /// <param name="topCount"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetValidMedicinesrmList(string srm, string yfbmCode, string organizeId, int topCount = 100);

        /// <summary>
        /// 获取在有效期内的药品，不分批次批号，不含有效期
        /// </summary>
        /// <param name="ypdm">药品代码</param>
        /// <param name="yfbmCode">当前部门</param>
        /// <param name="organizeId"></param>
        /// <param name="topCount"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetPcKcList(string ypdm, string yfbmCode, string organizeId, int topCount = 100);

        /// <summary>
        /// 获取出入库部门共同拥有的药品信息 库存数量为出库部门数量
        /// </summary>
        /// <param name="ckbm"></param>
        /// <param name="rkbm"></param>
        /// <param name="keyWord"></param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetDrugAndStock(string ckbm, string rkbm, string keyWord, string organizeid);

        /// <summary>
        /// 获取当前部门拥有的药品和库存信息  （外部入库）
        /// </summary>
        /// <param name="currentYfbmCode"></param>
        /// <param name="keyWord"></param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetDrugAndStock(string currentYfbmCode, string keyWord, string organizeid);

        /// <summary>
        /// 获取当前部门拥有的药品和库存信息  （外部出库）
        /// </summary>
        /// <param name="currentYfbmCode"></param>
        /// <param name="keyWord"></param>
        /// <param name="organizeid"></param>
        /// <param name="fph">发票号</param>
        /// <param name="gysCode">供应商代码</param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetDrugAndStockByFph(string currentYfbmCode, string keyWord, string organizeid, string fph, string gysCode);

        /// <summary>
        /// 批次分组
        /// </summary>
        /// <param name="ypdm"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetStockGroupByBatch(string ypdm, string yfbmCode, string organizeid);

        /// <summary>
        /// 根据发票号筛选 获取批次库存
        /// </summary>
        /// <param name="ypdm"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeid"></param>
        /// <param name="fph"></param>
        /// <param name="gysCode"></param>
        /// <returns></returns>
        List<DrugStockInfoVEntity> GetStockGroupByBatchByFph(string ypdm, string yfbmCode, string organizeid, string fph, string gysCode,string pc =null);

        /// <summary>
        /// 批次分组
        /// </summary>
        /// <param name="ypdm"></param>
        /// <param name="kczt">库存状态 1-展示有效库存  0-展示无效库存  空-全部展示</param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        IList<DrugStockInfoVEntity> GetStockGroupByBatch(Pagination pagination, string ypdm, string kczt, string yfbmCode, string organizeid);

        /// <summary>
        /// 获取库存信息  库存量查询用
        /// </summary>
        /// <param name="yfbmCode"></param>
        /// <param name="keyWord"></param>
        /// <param name="tybz">停用标志 1-展示有效的本部门信息 0-无效的本部门信息 0-全部</param>
        /// <param name="kczt">库存状态 1-展示有效库存  0-展示无效库存  空-全部展示</param>
        /// <param name="show0kc">是否展示零库存  0-不展示 1-展示</param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        IList<DrugStockInfoVEntity> GetDrugAndStock(Pagination pagination, string yfbmCode, string keyWord, string tybz, string kczt, string show0kc, string organizeid,string kcyjcode,string iskcyjz=null);

        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="ypdms"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeid"></param>
        /// <returns></returns>
        List<DrugStockInfo> SelectStock(string ypdm, string yfbmCode, string organizeid);

        /// <summary>
        /// 查询过期药品
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="gpyf">距离过期还剩x个月</param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<DrugStockInfoVEntity> SelectExpiredDrugs(Pagination pagination, string keyword, int gpyf, string yfbmCode, string organizeId, string gqyjcode, int? gqyyjts, string noShow0Kc = null);

		/// <summary>
		///获取单据全部数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		IList<CrkMxAll> GetCrkMxAll(string crkId,string orgId);

		/// <summary>
		/// 删除单据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		int DeleteCrkDj(string crkId);

		/// <summary>
		/// 修改单据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		int ReviseCrkDj(string crkId);

		/// <summary>
		/// 获取单据数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		DjGys GetCrkDjh(string crkId,int ? djlx);


        #region 药品有效期管理
        IList<DrugExpiredInfoVEntity> GetStockExpiredSearch(Pagination pagination, string keyword, string show0kc,string yfbmCode, string organizeid);
        #endregion


    }
}

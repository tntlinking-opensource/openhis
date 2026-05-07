using System;
using System.Web.Mvc;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Utils;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.HIS.Web.Areas.StorageManage.Controllers
{
	/// <summary>
	/// 药品库存查询
	/// </summary>
	public class DrugStockSearchController : ControllerBase
	{
		private readonly IDrugStorageDmnService drugStorageDmnService;
		private readonly ISysMedicineStockInfoRepo _kcxxRepo;
        private readonly ISysMedicineStockInfoRepo _sysMedicineStockInfoRepo;




        /// <summary>
        /// 药品查询 出库部门默认当前部门
        /// </summary>
        /// <param name="rkbm"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        [Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult DrugAndStockSearch(string rkbm, string keyWord)
		{
			var result = drugStorageDmnService.GetDrugAndStock(Constants.CurrentYfbm.yfbmCode, rkbm, keyWord, OrganizeId);
			return Content(result.ToJson());
		}

		/// <summary>
		/// 获取当前部门拥有的药品和库存信息  （外部入库）
		/// </summary>
		/// <param name="rkbm"></param>
		/// <param name="keyWord"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult DrugAndStockSearch(string keyword)
		{
			var result = drugStorageDmnService.GetDrugAndStock(Constants.CurrentYfbm.yfbmCode, keyword, OrganizeId);
			return Content(result.ToJson());
		}

		/// <summary>
		/// 获取当前部门拥有的药品和库存信息  （外部入库）
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="fph"></param>
		/// <param name="gysCode"></param>
		/// <returns></returns>
		[HttpGet]
		[Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult DrugAndStockSearchByFph(string keyword, string fph, string gysCode)
		{
			var result = drugStorageDmnService.GetDrugAndStockByFph(Constants.CurrentYfbm.yfbmCode, keyword, OrganizeId, fph, gysCode);
			return Content(result.ToJson());
		}

		/// <summary>
		/// 获取库存信息  库存量查询用
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="keyword"></param>
		/// <param name="tybz">停用标志  dbo.xt_yp_bmypxx.zt  1-可用 0-停用</param>
		/// <param name="kczt">库存标志 空-全部 0-无效库存  1-有效库存</param>
		/// <param name="show0kc">是否展示零库存</param>
		/// <returns></returns>
		[HttpGet]
		[Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult DrugAndStockSearchByPage(Pagination pagination, string keyword, string tybz, string kczt, string show0kc,string kcyjcode,string iskcyjz=null)
		{
			var stockTotalList = new
			{
				rows = drugStorageDmnService.GetDrugAndStock(pagination, Constants.CurrentYfbm.yfbmCode, keyword, tybz, kczt, show0kc, OrganizeId, kcyjcode, iskcyjz),
				total = pagination.total,
				page = pagination.page,
				records = pagination.records
			};
			return Content(stockTotalList.ToJson());
		}

		/// <summary>
		/// 根据药品代码获取按批次分组的库存数量 当前登录药房部门
		/// </summary>
		/// <param name="ypdm"></param>
		/// <returns></returns>
		[Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult StockGroupByBatchSearch(string ypdm)
		{
			var result = drugStorageDmnService.GetStockGroupByBatch(ypdm, Constants.CurrentYfbm.yfbmCode, OrganizeId);
			return Content(result.ToJson());
		}

		/// <summary>
		/// 根据药品代码获取按批次分组的库存数量 当前登录药房部门
		/// </summary>
		/// <param name="ypdm"></param>
		/// <param name="fph"></param>
		/// <param name="gysCode"></param>
		/// <returns></returns>
		[HttpGet]
		[Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult StockGroupByBatchSearchByFph(string ypdm, string fph, string gysCode, string pc = null)
		{
			var result = drugStorageDmnService.GetStockGroupByBatchByFph(ypdm, Constants.CurrentYfbm.yfbmCode, OrganizeId, fph, gysCode, pc);
			return Content(result.ToJson());
		}

		/// <summary>
		/// 根据药品代码获取按批次分组的库存数量 当前登录药房部门
		/// </summary>
		/// <param name="ypdm"></param>
		/// <returns></returns>
		[HttpGet]
		[Core.Attributes.HandlerAuthorizeIgnore]
		public ActionResult StockGroupByBatchSearchByPage(Pagination pagination, string ypdm, string kczt)
		{
			var result = new
			{
				rows = drugStorageDmnService.GetStockGroupByBatch(pagination, ypdm, kczt, Constants.CurrentYfbm.yfbmCode, OrganizeId),
				total = pagination.total,
				page = pagination.page,
				records = pagination.records
			};
			return Content(result.ToJson());
		}

		#region 库存查询功能页

		/// <summary>
		/// 库存查询 视图
		/// </summary>
		/// <returns></returns>
		public ActionResult StockQuery()
		{
			ViewBag.OrganizeId = OrganizeId;
			ViewBag.bmCode = Constants.CurrentYfbm.yfbmCode;
			return View();
		}

		/// <summary>
		/// 修改库存状态
		/// </summary>
		/// <param name="ypdm"></param>
		/// <param name="ph"></param>
		/// <param name="pc"></param>
		/// <param name="zt"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult UpdateKcxxZt(string ypdm, string ph, string pc, string zt)
		{
			return _kcxxRepo.UpdateZt(ypdm, ph, pc, zt, Constants.CurrentYfbm.yfbmCode, OrganizeId) > 0 ? Success() : Error("修改查库存状态失败");
		}
		#endregion

		#region 过期药品查询

		/// <summary>
		/// 过期药品查询 视图
		/// </summary>
		/// <returns></returns>
		public ActionResult ExpiredDrugsView()
        {
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
		}

		/// <summary>
		/// 查询过期药品
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="keyword"></param>
		/// <param name="gpyf"></param>
		/// <returns></returns>
		public ActionResult ExpiredDrugsQuery(Pagination pagination, string keyword, int gpyf,string gqyjcode,string noShow0Kc=null)
		{
            int gqyjz = -SysConfigReader.Int("GET_YFGQYG");//过期预警值
            var stockTotalList = new
			{
				rows = drugStorageDmnService.SelectExpiredDrugs(pagination, keyword.Trim(), gpyf, Constants.CurrentYfbm.yfbmCode, OrganizeId, gqyjcode, gqyjz, noShow0Kc),
				total = pagination.total,
				page = pagination.page,
				records = pagination.records
			};
			return Content(stockTotalList.ToJson());
		}
		#endregion


		#region 查询当前单据全部明细数据

		/// <summary>
		/// 查询单据数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		public ActionResult GetCrkMxAll(string crkId)
		{
			var data = drugStorageDmnService.GetCrkMxAll(crkId,this.OrganizeId);
			return Content(data.ToJson());
		}

		/// <summary>
		/// 删除单据数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		public int DeleteCrkDj(string crkId)
		{
			var data = drugStorageDmnService.DeleteCrkDj(crkId);
			return data;
		}

		/// <summary>
		/// 修改单据数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		public int ReviseCrkDj(string crkId)
		{
			var data = drugStorageDmnService.ReviseCrkDj(crkId);
			return data;
		}

		/// <summary>
		/// 查询单据数据
		/// </summary>
		/// <param name="crkId"></param>
		/// <returns></returns>
		public ActionResult GetCrkDjh(string crkId,int? djlx)
		{
			var data = drugStorageDmnService.GetCrkDjh(crkId, djlx);
			return Content(data.ToJson());
		}
        #endregion


        #region 药品有效期管理页面
        public ActionResult ExpiredDateManage()
        {
            return View();
        }

        public ActionResult ExpiredDateForm()
        {
            return View();
        }

        /// <summary>
        /// 获取药品有效期库存信息 当前登录药房部门
        /// </summary>
        /// <param name="ypdm"></param>
        /// <returns></returns>
        [HttpGet]
        [Core.Attributes.HandlerAuthorizeIgnore]
        public ActionResult GetStockExpiredSearchByPage(Pagination pagination, string keyword,string show0kc)
        {
            var result = new
            {
                rows = drugStorageDmnService.GetStockExpiredSearch(pagination, keyword, show0kc,Constants.CurrentYfbm.yfbmCode, OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(result.ToJson());
        }
        
        /// <summary>
        /// 更新药品有效期
        /// </summary>
        /// <param name="kcId"></param>
        /// <param name="yxq"></param>
        /// <returns></returns>
        public ActionResult UpdateExpired(string kcId, string yxq) {
            return _sysMedicineStockInfoRepo.UpdateExpired(kcId, yxq) > 0 ? Success() : Error("修改有效期失败");
        } 
        

        #endregion
    }
}
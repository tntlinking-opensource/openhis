using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Herp.Application.Interface;
using Newtouch.Herp.Domain.DTO.OutputDto;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.Herp.Web.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    public class HomeController : FrameworkBase.MultiOrg.Web.Controllers.HomeController
    {
        private readonly IHomeApp _homeApp;
        private readonly IStorageManageDmnService _storageManageDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;

        /// <summary>
        /// about
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 库房切换
        /// </summary>
        /// <returns></returns>
        public ActionResult KfChange()
        {
            ViewBag.KfList = Constants.CurrentKf.kfList;
            return View();
        }

        /// <summary>
        /// 切换库房
        /// </summary>
        /// <param name="kfId">库房ID</param>
        /// <param name="kfName">库房名称</param>
        /// <param name="kfLeve">库房等级</param>
        /// <returns></returns>
        public ActionResult SwithKf(string kfId, string kfName, int kfLeve)
        {
            var opr = Constants.CurrentKf;
            opr.currentKfId = kfId;
            opr.currentKfName = kfName;
            opr.currentKfLevel = kfLeve;
            Constants.SetCurrentKf(opr.userId, opr);
            return Success("切换成功");
        }

        #region 首页待办

        /// <summary>
        /// 获取待处理html
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNeedDealDiv()
        {
            var result = _homeApp.AssembleNeedDealHtml();
            return Success("", result);
        }

        /// <summary>
        /// 获取带处理总数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetPendingCount()
        {
            return Content(_homeApp.GetPendingCount().ToJson());
        }

        #endregion

        #region 统计

        /// <summary>
        /// 进销存统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPsiByKf()
        {
            var result = new MonthlySummaryDTO[3];
            result[0] = _homeApp.GetPsiCountVoByKf(_storageManageDmnService.GetCkCountByKf(Constants.CurrentKf.currentKfId, OrganizeId));
            result[1] = _homeApp.GetPsiCountVoByKf(_storageManageDmnService.GetRkCountByKf(Constants.CurrentKf.currentKfId, OrganizeId));
            result[2] = _homeApp.GetPsiCountVoByKf(_storageManageDmnService.GetSyCountByKf(Constants.CurrentKf.currentKfId, OrganizeId));
            return Content(result.ToJson());
        }

        /// <summary>
        /// 按单据类型获取入库总数
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRkCountByLx()
        {
            var result = _storageManageDmnService.GetRkCountByLx(Constants.CurrentKf.currentKfId, OrganizeId) ?? new List<ClassificationStatisticsEntity>();
            _homeApp.TransformRkCount(result);
            return Content(result.ToJson());
        }
        #endregion

        #region 消息提醒
        public ActionResult getmesquery()
        {
            string yfbm = Constants.CurrentKf.currentKfId;
            int gqyjz = -SysConfigReader.Int("GET_YFGQYG");//过期预警值
            var retdata = _storageManageDmnService.MSGQuery(yfbm, this.OrganizeId, gqyjz);
            int gqyjcount = 0, kcyjcount = 0;
            if (retdata != null)
            {
                kcyjcount = retdata.Where(p => p.typeas == "1").Count();
                gqyjcount = retdata.Where(p => p.typeas == "2").Count();
            }
            var data = new
            {
                rows = retdata,
                kcyj = kcyjcount,
                gqyj = gqyjcount
            };
            return Content(data.ToJson());
        }
        #endregion

        #region 同步系统参数配置
        public ActionResult SyncSysConfigParams(string orgId)
        {
            //基础数据
            var sysConfigBaseEntities = _sysConfigRepo.GetList("", "*").ToList();
            //组织机构自带数据
            var sysConfigEntities = _sysConfigRepo.GetList("", orgId).ToList();
            //根据code 去重
            var sysConfigCodes = new HashSet<string>(sysConfigEntities.Select(entity => entity.Code));
            var filteredEntities = sysConfigBaseEntities
                .Where(baseEntity => !sysConfigCodes.Contains(baseEntity.Code))
                .ToList();
            foreach (var item in filteredEntities)
            {
                item.Id = Guid.NewGuid().ToString();
                item.OrganizeId = orgId;
            }
            var insert = _sysConfigRepo.Insert(filteredEntities);
            return Success("",insert);
        }
        #endregion
    }
}
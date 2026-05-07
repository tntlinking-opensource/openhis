using FrameworkBase.MultiOrg.Web;
using Newtouch.Application.Interface;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Newtouch.Infrastructure;
using Newtouch.Domain.ViewModels;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ViewModels.Outpatient;
using Newtouch.Domain.ValueObjects;
using Newtouch.Common.Operator;

namespace Newtouch.CIS.Web.Areas.NurseManage.Controllers
{
    /// <summary>
    /// 输液管理
    /// </summary>
    public class InfusionController : OrgControllerBase
    {
        private readonly IMzsyypxxDmnService _mzsyypxxDmnService;
        private readonly IInfusionApp _infusionApp;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IMzsyzxxxRepo _mzsyzxxxRepo;
        private readonly IMzsyypxxRepo _mzsyypxxRepo;
        private readonly IOutpatientNurseDmnServise _OutpatientNurseDmnService;

        #region 视图
        /// <summary>
        /// 注射执行
        /// </summary>
        /// <returns></returns>
        public ActionResult Injection()
        {
            return View();
        }

        /// <summary>
        /// 注射历史查询
        /// </summary>
        /// <returns></returns>
        public ActionResult InfusionHistory()
        {
            return View();
        }

        /// <summary>
        /// 输液执行
        /// </summary>
        /// <returns></returns>
        public ActionResult InjectionForm()
        {
            return View();
        }

        /// <summary>
        /// 留观登记
        /// </summary>
        /// <returns></returns>
        public ActionResult observationFrom()
        {
            return View();
        }
        /// <summary>
        /// 不良反应药品管理
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugReactManage()
        {
            return View();
        }
        #endregion

        #region 获取数据

        /// <summary>
        /// 获取门诊患者列表
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientListQuery(Pagination pagination, string kh, string mzh,string fph, DateTime kssj, DateTime jssj)
        {
            var data = new
            {
                rows = _mzsyypxxDmnService.SelectKhAndXm(pagination, kh, mzh,fph, kssj, jssj, OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取门诊患者列表 (待做皮试)
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientHistoryListQuery(Pagination pagination, string kh, string mzh,string fph, DateTime kssj, DateTime jssj)
        {
            var data = new
            {
                rows = _mzsyypxxDmnService.SelectKhAndXm(pagination, kh,mzh, fph, kssj, jssj, OrganizeId, true),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据卡号获取未结束输液信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="kh"></param>
        /// <returns></returns>
        public ActionResult MzsyypxxQury(Pagination pagination, string kh,string mzh)
        {
            var data = new
            {
                rows = _mzsyypxxDmnService.SelectMzsyypxxByKh(pagination, kh,mzh, OrganizeId, false),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据卡号获取未结束输液信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="kh"></param>
        /// <returns></returns>
        public ActionResult HistoryMzsyypxxQury(Pagination pagination, string kh,string mzh)
        {
            var data = new
            {
                rows = _mzsyypxxDmnService.SelectMzsyypxxByKh(pagination, kh,mzh, OrganizeId, true),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 同步处方信息
        /// </summary>
        /// <param name="kh"></param>
        /// <param name="fph"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public ActionResult SyncSettRpDetail(string kh,string mzh, string fph, DateTime kssj, DateTime jssj)
        {
            var result = _infusionApp.SyncSettRpDetail(kssj, jssj, fph, kh,mzh);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 获取岗位信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStaffList(string keyword)
        {
            return Content(_sysUserDmnService.GetSatffList(OrganizeId, keyword).ToJson());
        }

        #endregion

        #region 执行

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="kh"></param>
        /// <returns></returns>
        public ActionResult Grouping(List<long> tags, string kh,string mzh)
        {
            var groupNo = _infusionApp.CreateGroupNo(kh,mzh);
            var result = _infusionApp.Grouping(tags, groupNo);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 开始输液
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="seatNum">座号/床号</param>
        /// <returns></returns>
        public ActionResult StartInfusion(List<long> tags, string seatNum)
        {
            var result = _infusionApp.StartInfusion(tags, DateTime.Now, seatNum);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 结束输液
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public ActionResult EndInfusion(List<long> tags)
        {
            var result = _infusionApp.EndInfusion(tags, DateTime.Now);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 设置座位号
        /// </summary>
        /// <param name="tag">目标ID</param>
        /// <param name="seatNum">座位号</param>
        /// <returns></returns>
        public ActionResult SetSeatNum(long tag, string seatNum)
        {
            var result = _infusionApp.SetSeatNum(tag, seatNum);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 打印瓶贴
        /// </summary>
        /// <returns></returns>
        public ActionResult PrintBottleLabel(string tags)
        {
            var ids = new List<long>();
            if (!string.IsNullOrWhiteSpace(tags))
            {
                foreach (var item in tags.Split(','))
                {
                    long tmp;
                    long.TryParse(item, out tmp);
                    if (tmp > 0) ids.Add(tmp);
                }
            }
            ViewBag.src = "data:image/png;base64," + GenerateInfusionQueryQr(ids);
            var drugs = _infusionApp.MzsyxxQuery(ids, OrganizeId);
            ViewBag.patInfo = new InfusionExecVO();
            ViewBag.drugs = drugs ?? new List<MzsyypxxVO>();
            if (drugs == null || drugs.Count <= 0) return View();
            ViewBag.patInfo.patientName = drugs[0].xm;
            ViewBag.patInfo.seatNum = drugs[0].seatNum;
            ViewBag.patInfo.remark = drugs[0].remark;
            ViewBag.patInfo.dispenser = drugs[0].dispenser;
            ViewBag.patInfo.dispenserName = drugs[0].dispenserName;
            ViewBag.patInfo.executor = drugs[0].executor;
            ViewBag.patInfo.executorName = drugs[0].executorName;
            return View();
        }

        /// <summary>
        /// 生成输液查询二维码
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private string GenerateInfusionQueryQr(List<long> tags)
        {
            var authority = "";
            if (HttpContext.Request.Url != null)
            {
                authority = HttpContext.Request.Url.Authority;
            }
            if (string.IsNullOrWhiteSpace(authority)) throw new Exception("获取服务器地址失败，请联系管理员");
            const string absolutePath = "/InfusionManage/Query";
            var query = new StringBuilder("?organizeId=").Append(OrganizeId).Append("&tags=");
            if (tags != null && tags.Count > 0)
            {
                tags.ForEach(p => { query.Append(p + ","); });
            }
            var url = new StringBuilder("http://").Append(authority).Append(absolutePath).Append(query.ToString().Trim(','));
            var iconPath = HttpContext.Server.MapPath("~/Content/img/cloud.png");
            var qrCode = QrEncoder.Code(url.ToString(), 12, 15, iconPath, 0, 0, false);
            using (var ms = new MemoryStream())
            {
                qrCode.Save(ms, ImageFormat.Png);
                var b = ms.ToArray();
                return Convert.ToBase64String(b);
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="syIds"></param>
        /// <param name="zt"></param>
        /// <param name="dispenser"></param>
        /// <param name="executor"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitExec(string syIds, string seatNum, string zt, string dispenser, string executor,DateTime? sykssj,DateTime? syjssj, string remark)
        {
            if (string.IsNullOrWhiteSpace(syIds)) return Error("请传入要执行的药品");
            var ids = new List<long>();
            foreach (var item in syIds.Split(','))
            {
                long tmp = 0;
                long.TryParse(item, out tmp);
                if (tmp > 0) ids.Add(tmp);
            }
            var result = _mzsyzxxxRepo.Exec(ids, seatNum, zt, dispenser, executor, sykssj,syjssj,remark, OrganizeId);
            _mzsyypxxRepo.Exec(ids,OrganizeId);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }

        /// <summary>
        /// 输液执行撤销
        /// </summary>
        /// <param name="syIds"></param>
        /// <returns></returns>
        public ActionResult CancelExec(string syIds)
        {
            if (string.IsNullOrWhiteSpace(syIds)) return Error("请传入要执行的药品");
            var idss = syIds.Split('|');
            var ids = new List<string>();
            var ypids = new List<long>();
            foreach (var item in idss[0].Split(','))
            {
                if(item!="") ids.Add(item);

            }
            foreach (var item in idss[1].Split(','))
            {
                long tmp = 0;
                long.TryParse(item, out tmp);
                if (tmp > 0) ypids.Add(tmp);
            }
            var result = _mzsyzxxxRepo.CanCelExec(ids, OrganizeId);
            _mzsyypxxRepo.CanCelExec(ypids, OrganizeId);
            return string.IsNullOrWhiteSpace(result) ? Success() : Error(result);
        }
        #endregion

        public ActionResult Getlgjl(string syxxids)
        {
            var data = new
            {
                rows = _OutpatientNurseDmnService.Getlgjl(syxxids, OrganizeId),

            };
            return Content(data.ToJson());
        }

        public ActionResult SavaLgdj(IList<ObservationFromVO> lgdjlist)
        {
            OperatorModel user = this.UserIdentity;
            var patOutMsg = _OutpatientNurseDmnService.SavaLgdj(lgdjlist, this.OrganizeId, user);
            return patOutMsg == "T" ? Success("保存成功。") : Error(patOutMsg);
        }
    }
}
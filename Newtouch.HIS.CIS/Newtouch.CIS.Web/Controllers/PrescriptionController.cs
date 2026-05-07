using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Application.Interface;
using Newtouch.Common;
using Newtouch.Domain.DTO;
using Newtouch.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Web.Core.Attributes;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.CIS.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class PrescriptionController : OrgControllerBase
    {
        private readonly IPrescriptionDetailRepo _prescriptionDetailRepo;
        private readonly IPrescriptionRepo _prescriptionRepo;
        private readonly IMedicalRecordDmnService _medicalRecordDmnService;
        private readonly IMedicalRecordRepo _medicalRecordRepo;
        private readonly IPrescriptionDmnService _prescriptionDmnService;
        private readonly ITherapistApp _therapistApp;
        private readonly IIDBDmnService _idbDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        // GET: Prescription

        #region View
        /// <summary>
        /// 西医处方
        /// </summary>
        /// <returns></returns>
        public ActionResult WMPrescription()
        {
            return View();
        }

        /// <summary>
        /// 中医处方
        /// </summary>
        /// <returns></returns>
        public ActionResult TCMPrescription()
        {
            return View();
        }
        /// <summary>
         /// 中医处方NEW
         /// </summary>
         /// <returns></returns>
        public ActionResult TCMPrescription2022()
        {
            return View();
        }

        /// <summary>
        /// 康复处方
        /// </summary>
        /// <returns></returns>
        public ActionResult RehabPrescription()
        {
            return View();
        }

        /// <summary>
        /// 常规项目处方
        /// </summary>
        /// <returns></returns>
        public ActionResult RegularItemPrescription()
        {
            return View();
        }

        public ActionResult ConsumablesPrescription()
        {
            return View();
        }

        /// <summary>
        /// 历史处方列表
        /// </summary>
        /// <returns></returns>
        public ActionResult HistoryPresForm()
        {
            return View();
        }
        #endregion

        /// <summary>
        /// 历史处方树
        /// </summary>
        /// <param name="jzId"></param>
        /// <returns></returns>
        public ActionResult GetHistoryPresTree(string blh, string jzId, int cflx)
        {
            var treeList = new List<TreeViewModel>();
            var blList = _medicalRecordRepo.IQueryable().Where(a => a.blh == blh && a.zt == "1"&&a.OrganizeId==OrganizeId).Select(a => new { a.CreateTime, a.jzId }).ToList();
            var historyPartList = blList.Where(p => p.jzId != jzId);    //else就诊
            var thisPart = blList.FirstOrDefault(p => p.jzId == jzId);  //本次就诊
            if (thisPart == null)
            {
                var ptreeItem = new TreeViewModel()
                {
                    id = Guid.NewGuid().ToString(),
                    value = DateTime.Now.ToString("yyyy-MM-dd"),
                    text = DateTime.Now.ToString("yyyy-MM-dd") + "（本次就诊）",
                    parentId = null,
                    hasChildren = true, //有下级 样式
                    isexpand = true,
                    complete = true,
                };
                treeList.Add(ptreeItem);
            }
            if (thisPart != null)
            {
                var cfList = _prescriptionRepo.IQueryable().Where(a => a.jzId == thisPart.jzId && a.cflx == cflx && a.zt == "1" && a.OrganizeId == OrganizeId).OrderBy(p => p.sfbz).ThenByDescending(p => p.cfh).ToList();
                var ptreeItem = new TreeViewModel()
                {
                    id = Guid.NewGuid().ToString(),
                    value = thisPart.CreateTime.ToString("yyyy-MM-dd"),
                    text = thisPart.CreateTime.ToString("yyyy-MM-dd") + "（本次就诊）",
                    parentId = null,
                    hasChildren = true, //有下级 样式
                    isexpand = true,
                    complete = true,
                };
                treeList.Add(ptreeItem);
                foreach (var cf in cfList)
                {
                    treeList.Add(new TreeViewModel()
                    {
                        id = cf.cfId,
                        value = cf.cfh,
                        text = cf.cfh,
                        parentId = ptreeItem.id,
                        hasChildren = false,
                        isexpand = false,
                        complete = true,
                        Ex1 = cf.sfbz.ToString(),
                        Ex2 = "current",
                        title = cf.sfbz ? "已收费" : "未收费",
                        img = cf.sfbz ? "fa fa-jpy" : null,
                    });
                }
            }
            //最新的排在上面
            foreach (var rq in historyPartList.Select(p => p.CreateTime.ToString("yyyy-MM-dd")).Distinct().OrderByDescending(p => p))
            {
                //日期下的所有病历
                var newId = Guid.NewGuid().ToString();
                bool hasChildren = false;

                var rqblList = historyPartList.Where(p => p.CreateTime.ToString("yyyy-MM-dd") == rq).ToList();
                foreach (var item in rqblList)
                {
                    var cfList = _prescriptionRepo.IQueryable().Where(a => a.jzId == item.jzId && a.cflx == cflx && a.zt == "1").ToList();
                    foreach (var cf in cfList)
                    {
                        hasChildren = true;
                        treeList.Add(new TreeViewModel()
                        {
                            id = cf.cfId,
                            value = cf.cfh,
                            text = cf.cfh,
                            parentId = newId,
                            hasChildren = false,
                            isexpand = false,
                            complete = true,
                            Ex1 = cf.sfbz.ToString(),
                            title = cf.sfbz ? "已收费" : "未收费",
                            img = cf.sfbz ? "fa fa-jpy" : null,
                        });
                    }
                }

                if (!hasChildren)
                {
                    continue;   //没病历的不显示
                }
                var ptreeItem = new TreeViewModel()
                {
                    id = newId,
                    value = rq,
                    text = rq,
                    parentId = null,
                    hasChildren = true,
                    isexpand = true,
                    complete = true,
                };
                treeList.Add(ptreeItem);
            }
            return Content(treeList.TreeViewJson(null));
        }

        /// <summary>
        /// 历史处方树明细
        /// </summary>
        /// <param name="cfId"></param>
        /// <returns></returns>
        public ActionResult GetHistoryPresDetailByCfId(string cfId)
        {
            var list = _prescriptionDmnService.GetHistoryPresDetailByCfId(cfId);
            return Content(list.ToJson());
        }

        /// <summary>
        /// 查询处方号
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNewPresNo()
        {
            var presNo = EFDBBaseFuncHelper.Instance.GetRequisitionNo(this.OrganizeId);
            return Content(presNo);
        }

        /// <summary>
        /// 根据cfId或cfmxIdStr查询明细
        /// </summary>
        /// <param name="cflx"></param>
        /// <param name="cfId"></param>
        /// <param name="cfmxIdStr"></param>
        /// <returns></returns>
        public ActionResult GetPresDetailList(int cflx, string cfId, string cfmxIdStr)
        { 
            var data = _prescriptionDmnService.GetPresDetailList(cflx, cfId, cfmxIdStr);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 引用治疗建议
        /// </summary>
        /// <returns></returns>
        public ActionResult AddKfjyForm()
        {
            return View();
        }

        /// <summary>
        /// 拉取待就诊治疗建议列表
        /// </summary>
        /// <param name="brlx"></param>
        /// <param name="mzzyh"></param>
        /// <returns></returns>
        public ActionResult kfjyGridJson(string brlx, string mzzyh)
        {
            int dayNum = 3;
            if (!string.IsNullOrWhiteSpace(_sysConfigRepo.GetValueByCode("GetRehabiDayNum", this.OrganizeId)))
            {
                dayNum = Convert.ToInt32(_sysConfigRepo.GetValueByCode("GetRehabiDayNum", this.OrganizeId));
            }
            var list = _therapistApp.GetWaitCvSuggestionList(brlx, mzzyh, dayNum);
            return Content(list.ToJson());
        }

        /// <summary>
        /// 更新建议的转换状态
        /// </summary>
        /// <param name="cvList"></param>
        /// <returns></returns>
        public ActionResult UpdateSuggestionCvStatus(IList<SuggestionCvStatuDTO> cvList)
        {
            _therapistApp.UpdateSuggestionCvStatus(cvList);
            return Success();
        }

        /// <summary>
        /// 秦皇岛账单接口（提供给HIS）
        /// </summary>
        /// <param name="Opt">接口类型：取值：zd(账单)</param>
        /// <param name="Brwym">病人唯一码(病人在HIS中的唯一码，不做SP的参数用，仅作核对用)</param>
        /// <param name="Zdlx">账单类型:00:条形码01:药品02:检查03:检验04:治疗不传递此字段则返回全部账单(处方，LIS，PACS，治疗)</param>
        /// <param name="Brxz">病人性质</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        [HandlerLoginIgnore]
        public ActionResult QhdSvscis(string Opt, string Brwym, string Zdlx, string Brxz, DateTime? startDate, DateTime? endDate)
        {
                var str = _idbDmnService.GetCisPrescription(Opt, Brwym, Zdlx, Brxz, startDate, endDate);
                RtnToQhd rtnToQhd = new RtnToQhd();
                rtnToQhd.zlMessage =
                    str.Where(p => p.zdlx == (int)EnumCflx.RegularItemPres).ToList();
                rtnToQhd.kfMessage = str.Where(p => p.zdlx == (int)EnumCflx.RehabPres).ToList();//康复
                rtnToQhd.ypMessage = str.Where(p => p.zdlx == (int)EnumCflx.TCMPres || p.zdlx == (int)EnumCflx.WMPres).ToList();//药品
                rtnToQhd.lisMessage = str.Where(p => p.zdlx == (int)EnumCflx.ExaminationPres).ToList();//检查
                rtnToQhd.pacsMessage = str.Where(p => p.zdlx == (int)EnumCflx.InspectionPres).ToList();//检验
                return Content(rtnToQhd.ToJson());
        }
        private readonly IDoctorserviceDmnService _iDoctorserviceDmnService;
        public ActionResult GetSfxmZxksSelectJson(string sfxmCode, string keyword)
        {
            var Zxks = _iDoctorserviceDmnService.GetSfxmZxksSelectJson(OrganizeId, keyword);
            return Content(Zxks.ToJson());
        }

        public ActionResult GetSfxmItem(string yfCode, string keyword)
        {
            var Zxks = _iDoctorserviceDmnService.GetSfxmYfList(OrganizeId, yfCode, keyword);
            return Content(Zxks.ToJson());
        }


		#region 物资
		/// <summary>
		/// 开立物资项目冻结库存数量
		/// </summary>
		/// <returns></returns>
		public ActionResult Sumwzdj(string[] cfh)
		{
			var list = _prescriptionDmnService.Sumwzdj(cfh,OrganizeId, UserIdentity.rygh);
			var data = new
			{
				row= list
			};
			return Content(data.ToJson());
		}

		/// <summary>
		/// 开立物资项目冻结库存数量
		/// </summary>
		/// <returns></returns>
		public ActionResult ZUOFwzdj(string cfh)
		{
			var list = _prescriptionDmnService.ZUOFwzdj(cfh, OrganizeId, UserIdentity.rygh);
			var data = new
			{
				row = list
			};
			return Content(data.ToJson());
		}

        #endregion
        public ActionResult Getmxbzd(string zdcode)
        {
            var mxbstr = _iDoctorserviceDmnService.GetmxbzdList(OrganizeId, zdcode);
            var data = new
            {
                mxbstr = mxbstr
            };
            return Content(data.ToJson());
        }
        public ActionResult Getxzkls(YpxzkldataDTO xzkldata)
        {
            var mxbstr = _iDoctorserviceDmnService.GetxzklsList(OrganizeId, xzkldata);
            var data = new
            {
                message = mxbstr
            };
            return Content(data.ToJson());
        }
        public ActionResult GetYfData()
        {
            var mxbstr = _iDoctorserviceDmnService.GetYfData(OrganizeId);
            return Content(mxbstr.ToJson());
        }

        /// <summary>
        /// 医保电子处方
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        public ActionResult getdzcfnrxs(string cfh)
        {
            var data = _prescriptionDmnService.getdzcfnrxs(cfh, this.OrganizeId);
            if (data.Count > 0)
            {
                return Content("ok");
            }
            return Content("");
        }
    }
}
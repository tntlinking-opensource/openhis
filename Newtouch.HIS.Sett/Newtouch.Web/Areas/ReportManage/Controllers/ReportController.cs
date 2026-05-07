using DotNetDBF;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.ReportManage;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.ReportManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportController : ControllerBase
    {
        private readonly IReportCommonDmnService _reportCommonDmnService;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IHospFeeDmnService _hospFeeDmnService;
        private readonly IPatientBasicInfoDmnService _patientBasicInfoDmnService;
	    private readonly IRptrptMzRjbRepo _rptrptMzRjbRepo;
        private readonly ISysConfigRepo _sysConfigRepo;

        #region

        public ActionResult RevenueDetailesByPatientReportCompare() {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 月报表
        /// </summary>
        /// <returns></returns>
        public ActionResult MonthReportIndex()
        {
            return View();
        }

        public ActionResult MonthlyProfit() {
            ReportingServiceCom();
            return View();
        }

        public ActionResult TherapistPerformance() {
            ReportingServiceCom();
            return View();
        }

        public ActionResult PatientDataDetail() {
            ReportingServiceCom();
            return View();
        }
        public ActionResult ChargeTypeQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult OutpatientWorkloadQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult OutpatientSituationQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult OutpatientQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult NormalMedicineInsuranceQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult AllMedicineInsuranceQuery()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult GetStaffByDutyCode(string orgId)
        {
            var data = _sysUserDmnService.GetStaffByDutyCode(orgId ?? OrganizeId, "RehabDoctor");//获取治疗师
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取药库药房下拉框数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSFDLCodeSelectJson()
        {
            var data = _reportCommonDmnService.GetSFDLCodeSelectJson(OrganizeId);
            var treeList = new List<TreeSelectModel>
            {
                new TreeSelectModel{
                id="",
                text="未选择"}
            };
            if (data != null)
            {
                foreach (SFDLReportVO item in data)
                {
                    TreeSelectModel treeModel = new TreeSelectModel();
                    treeModel.id = item.dlcode;
                    treeModel.text = item.dlmc;
                    treeList.Add(treeModel);

                }
                return Content(treeList.TreeSelectJson(null));
            }
            else
            {
                return Content("");
            }
        }

        /// <summary>
        /// 月报表 请求查询 实时 的 数据
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult MonthReportRealTimeData(int year, int month)
        {
            var data = _reportCommonDmnService.GetMonthReportRealTimeData(this.OrganizeId, new DateTime(year, month, 1), new DateTime(year, month, 1).AddMonths(1));
            return Success(null, data);
        }

        /// <summary>
        /// 月报表 固化 调整
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="calcZje"></param>
        /// <returns></returns>
        public ActionResult MonthReportDataConfirmPreview()
        {
            return View();
        }

        public ActionResult NewOutpatientFeeStatistics()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 月报表 固化 确认
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="calcZje"></param>
        /// <param name="resultZje"></param>
        /// <param name="details"></param>
        /// <param name="balance"></param>
        /// <param name="balanceReason"></param>
        /// <param name="isregenerate">重新生成 标志</param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult SubmitMonthReportData(int year, int month, decimal calcZje, decimal resultZje
            , IList<MonthReportMajorCateStatisticsVO> details, decimal balance = 0, string balanceReason = null
            , bool isregenerate = false)
        {
            if (resultZje <= 0)
            {
                throw new FailedException("费用合计结果应为正数，请确认");
            }
            if (calcZje + balance != resultZje)
            {
                throw new FailedException("金额计算错误，请确认<br/>（费用+补差!=结果）");
            }
            _reportCommonDmnService.SubmitMonthReportData(this.OrganizeId, this.UserIdentity.UserCode, year, month, calcZje, resultZje
                , details, balance, balanceReason
                , isregenerate);

            return Success("报表生成成功");
        }

        public ActionResult RehabilitationCategoryTreatDetail()
        {
            ReportingServiceCom();
            return View();
        }

        public ActionResult RevenueDetailesByTherapistReportCompare()
        {
            ReportingServiceCom();
            return View();
        }
        #endregion

        #region 治疗师工作统计

        /// <summary>
        /// 治疗师工作统计
        /// </summary>
        /// <returns></returns>
        public ActionResult TherapeWorkStatistics()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 分类清单 - 住院病人

        /// <summary>
        /// 分类清单 - 住院病人
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientClassification()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 分类清单 - 门诊病人

        /// <summary>
        /// 分类清单 - 门诊病人
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientClassification()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 结算单 - 住院病人

        /// <summary>
        /// 结算单 - 住院病人
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientSett()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 结算单 - 门诊病人

        /// <summary>
        /// 结算单 - 门诊病人
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientSett()
        {
            ReportingServiceCom();

            return View();
        }

        /// <summary>
        /// 结算单 - 门诊病人
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientSettFl()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 逐单收入报表

        /// <summary>
        /// 逐单收入报表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientFeeStatistics()
        {
            ReportingServiceCom();

            return View();
        }

        public ActionResult GetdotorcByDutyCode(string orgId)
        {
            var data = _sysUserDmnService.GetStaffByDutyCode(orgId ?? OrganizeId, "Doctor");//获取医生
            return Content(data.ToJson());
        }

        #endregion

        #region 费用一日清

        /// <summary>
        /// 费用一日清
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientDayFee()
        {
            
            //_hospFeeDmnService.SyncPatFee(OrganizeId, "", 0);//同步项目费用
            //_hospFeeDmnService.SyncPatFee(OrganizeId, "", 1);//同步药品费用
            ReportingServiceCom();

            return View();
        }

        public ActionResult GetInpatientcryrq(string zyh)
        {
            var hospPatientBasic = _patientBasicInfoDmnService.GetSysBasicVagueByZHY(zyh, this.OrganizeId);
            return Content(hospPatientBasic.ToJson());
        }

        [HttpPost]
        public ActionResult SynInpatientDayFee(string zyh)
        {
            if(!string.IsNullOrWhiteSpace(zyh))
            {
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 0);//同步项目费用
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 1);//同步药品费用
            }
            return Success();
        }
       

        /// <summary>
        /// 绩效指标（费用 人次）
        /// </summary>
        /// <returns></returns>
        public ActionResult PerformanceIndicatorFee()
        {
            ReportingServiceCom();

            return View();
        }

        #endregion

        #region 费用结算清单
        public ActionResult InpatientPatFeeDetail()
        {
            ReportingServiceCom();

            return View();
        }
        #endregion
        #region 执行计划查询
        public ActionResult TreatmentplanQuery()
        {
            ReportingServiceCom();

            return View();
        }
        #endregion
        #region 执行计划查询门诊
        public ActionResult TreatmentplanQueryOutpatient()
        {
            ReportingServiceCom();

            return View();
        }
        #endregion
        #region 门诊日结报表
        public ActionResult ChargeDailysettlement()
        {
            ReportingServiceCom();

            return View();
        }
        #endregion
        #region 住院医保费用统计报表
        public ActionResult HealthInsuranceReport()
        {
            ReportingServiceCom();

            return View();
        }
		#endregion

		#region 医院垫付金额明细
		public ActionResult HosAdvanceAmount()
		{
			ReportingServiceCom();
			return View();
		}
		#endregion

		#region 病人就诊信息（就诊次数 等）
		/// <summary>
		/// 病人就诊信息（就诊次数 等）
		/// </summary>
		/// <returns></returns>
		public ActionResult OutpatientTreatmentInfo()
		{
			ReportingServiceCom();
			return View();
		}
        #endregion

        public ActionResult HosOutpatientFees()
        {
            return View();
        }

        /// <summary>
        /// 服务代码盈利
        /// </summary>
        /// <returns></returns>
        public ActionResult RevenueDetailesReport()
        {
            ReportingServiceCom();

            return View();
        }

        /// <summary>
        /// 服务代码盈利（治疗师分组）
        /// </summary>
        /// <returns></returns>
        public ActionResult RevenueDetailesByTherapistReport()
        {
            ReportingServiceCom();

            return View();
        }

        /// <summary>
        /// 同步服务代码盈利
        /// </summary>
        /// <returns></returns>
        public ActionResult SyncRevenueDetailesReport()
        {
            ReportingServiceCom();

            return View();
        }

        /// <summary>
        /// 同步服务代码盈利（治疗师分组）
        /// </summary>
        /// <returns></returns>
        public ActionResult SyncRevenueDetailesByTherapistReport()
        {
            ReportingServiceCom();

            return View();
        }

        /// <summary>
        /// 病人收费总览
        /// </summary>
        /// <returns></returns>
        public ActionResult PatientRevenue()
        {
            ReportingServiceCom();

            return View();
        }

        public ActionResult PatientRevenueTB()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 门诊病人一览表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientListChart()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 住院病人一览表
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientListChart()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 治疗师下拉
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRehabDoctorList(string orgId)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                orgId = this.OrganizeId;
            }
            var data = _sysUserDmnService.GetStaffByDutyCode(orgId, "RehabDoctor");
            return Content(data.ToJson());
        }

		

		/// <summary>
		/// 病人收费执行报表
		/// </summary>
		/// <returns></returns>
		public ActionResult PatientChargeExecution()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 月度申报结算表（医保）
        /// </summary>
        /// <returns></returns>
        public ActionResult SettMonthForYB()
        {
            ReportingServiceCom();

            return View();
        }
        public ActionResult GetdoctorByDutyCode(string orgId)
        {
            var data = _sysUserDmnService.GetStaffByDutyCode(orgId ?? OrganizeId, "Doctor");//获取医生
            return Content(data.ToJson());
        }

        /// <summary>
        /// 住院全院报表
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientWhileHospital()
        {
            ReportingServiceCom();
            //return View();
            return View();
        }

        /// <summary>
        /// 门诊全院报表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientWhileHospital()
        {
            ReportingServiceCom();
            //return View();
            return View();
        }

        public ActionResult PrintCryRegisterReport() {
            ReportingServiceCom();
            //return View();
            return View();
        }
       

        public ActionResult OutpWorkStacDeptReport () {
            ReportingServiceCom();
            return View();
        }

        //费用一日清视图
        public ActionResult PrintReport()
        {
            ReportingServiceCom();

           return View();
        }


        //分类汇总日清单视图
        public ActionResult SubtotalsReport()
        {
            ReportingServiceCom();

            return View();
        }

        //住院费用明细视图
        public ActionResult Hospitalizationcost() {
            ReportingServiceCom();

            return View();
        }

        //住院病人分类清单
        public ActionResult Hospitalizationclassification()
        {
            ReportingServiceCom();

            return View();
        }





        #region 新农合申报
        /// <summary>
        /// 门诊新农合申报明细
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientXnhBcmxsqd()
        {
            ReportingServiceCom();
            ViewBag.usercode = UserIdentity.UserCode;
            return View();
        }

        public ActionResult OutpatientXnhBcsqd()
        {
            ReportingServiceCom();
            ViewBag.usercode = UserIdentity.UserCode;
            return View();
        }

        public ActionResult InpatientXnhBcsqd()
        {
            ReportingServiceCom();
            ViewBag.usercode = UserIdentity.UserCode;
            return View();
        }

        public ActionResult InpatientXnhBcmxsqd()
        {
            ReportingServiceCom();
            ViewBag.usercode = UserIdentity.UserCode;
            return View();
        }
        #endregion

        #region
        /// <summary>
        /// 门诊日结算表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientDailyFee()
        {
            ReportingServiceCom();
            var DailyFeeHbOpen = _sysConfigRepo.GetValueByCode("DailyFeeHbOpen",this.OrganizeId);//日结报表是否合并所有收费员收费记录
            RptrptMzRjbEntity mzRjbEntity= _rptrptMzRjbRepo.GetLastMzrjEntity(this.OrganizeId, DailyFeeHbOpen == "ON"? "":this.UserIdentity.UserCode);
            ViewBag.usercode = DailyFeeHbOpen == "ON" ?"": UserIdentity.UserCode;
			ViewBag.kssj = mzRjbEntity.jssj.ToString("yyyy-MM-dd HH:mm:ss");
            ViewBag.isfirst = string.IsNullOrWhiteSpace(mzRjbEntity.Id) ?"True" :"False";
            ViewBag.OrgId = this.OrganizeId;
            return View();
        }

	    public ActionResult turnInFee(DateTime kssj,DateTime jssj)
	    {
			_reportCommonDmnService.turnInFee(this.OrganizeId,this.UserIdentity.UserCode,kssj,jssj);
		    return Success();
	    }
	    public ActionResult turnInFeeSelect(string keyword)
	    {
            var DailyFeeHbOpen = _sysConfigRepo.GetValueByCode("DailyFeeHbOpen", this.OrganizeId);
            var rjList = _rptrptMzRjbRepo.GetLastMzrjEntityList(this.OrganizeId, DailyFeeHbOpen == "ON" ? "" : this.UserIdentity.UserCode, keyword);
			return Content(rjList.ToJson());
	    }
		/// <summary>
		/// 门诊结算明细信息表
		/// </summary>
		/// <returns></returns>
		public ActionResult OutpatientSettDetailsInfo()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 科室财务核算汇总表
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentFinancialAccSummary()
        {
            ReportingServiceCom();
            return View();
        }

        public ActionResult HosPovertyVisitSummary()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 科室业务收入统计表
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentBusinessIncomeSummary()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 药品分类统计表
        /// </summary>
        /// <returns></returns>
        public ActionResult MedicineClassificationSettedSummary()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 门诊收费员工作量统计表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientTollCollectorWorkloadSummary()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 门诊医生工作量统计表
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientDoctorWorkloadSummary()
        {
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 项目分类统计表
        /// </summary>
        /// <returns></returns>
        public ActionResult ChargeItemClassificationSettedSummary()
        {
            ReportingServiceCom();
            return View();
        }

        public ActionResult InpatientCrytjb()
        {
            ReportingServiceCom();
            return View();
        }

	    public ActionResult OutpatientKsstjb()
	    {
		    ReportingServiceCom();
		    return View();
	    }

	    public ActionResult HosMedicalIncome()
	    {
		    ReportingServiceCom();
		    return View();
		}

	    #endregion

		#region private methods

		/// <summary>
		/// 
		/// </summary>
		private void ReportingServiceCom()
        {
            
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.topOrgId = Constants.TopOrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            ViewBag.IsHospAdministrator = this.UserIdentity.IsHospAdministrator.ToString().ToLower();  //是否是医院管理员
            ViewBag.CurUserCode = this.UserIdentity.UserCode;
            ViewBag.curUsergh = UserIdentity.rygh;
        }

		#endregion

		#region 医保
	    public ActionResult OutpatientCqYbDz()
	    {
		    ReportingServiceCom();
		    return View();
	    }
        #endregion
        #region 医保自费科室结算费用统计
        public ActionResult HosExpenseSettDeatilInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosExpenseSettInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosYbPatientDrYbSettInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosYbPatientNoInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosYbPatientSettDetailInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosYbPatientSettInfo()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosYbSettApplyInfo()
        {
            ReportingServiceCom();
            return View();
        }

        public ActionResult HosAcceptingGold()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosMedicalBill()
        {
            ReportingServiceCom();
            return View();
        }
        public ActionResult HosFeeSettSummry()
        {
            ReportingServiceCom();
            return View();
        }
        #endregion

         //科室收入明细
        public ActionResult Department_income_statement() {
            ReportingServiceCom();

            return View();
        }

        //科室药品成本价
        public ActionResult Drugcostprice() {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 月度收费明细
        /// </summary>
        /// <returns></returns>
        public ActionResult MonthlyFeeDetails()
        {
            ReportingServiceCom();

            return View();
        }
        
        /// <summary>
        /// 获取大类名称
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDlMc()
		{
			var data = _reportCommonDmnService.GetDlMc(this.OrganizeId);
			return Content(data.ToJson());
		}

        #region 特病门诊统计
        public ActionResult TBStatistics()
        {
            ReportingServiceCom();

            return View();
        }
        #endregion

		/// <summary>
		/// 城保费用分析
		/// </summary>
		/// <returns></returns>
		public ActionResult MZCBFYFX()
		{
			ReportingServiceCom();

			return View();
		}
		/// <summary>
		/// 居保费用分析
		/// </summary>
		/// <returns></returns>
		public ActionResult MZJBFYFX()
		{
			ReportingServiceCom();

			return View();
		}

		public ActionResult FYXMMXQuery()
		{
			ReportingServiceCom();

			return View();
		}
		
		/// <summary>
		/// 门诊费用日报表汇总
		/// </summary>
		/// <returns></returns>
		public ActionResult MZFYRBHZ_Query()
		{
			ReportingServiceCom();

			return View();
		}

		#region 医保申报上传报表
		/// <summary>
		/// 医保申报汇总清单
		/// </summary>
		/// <returns></returns>
		public ActionResult YBSBHZQDQuery()
		{
			ReportingServiceCom();

			return View();
		}

		/// <summary>
		/// 医保申报上传职工
		/// </summary>
		/// <returns></returns>
		public ActionResult YBSBSC_ZGQuery()
		{
			ReportingServiceCom();

			return View();
		}


		/// <summary>
		/// 医保申报上传居民
		/// </summary>
		/// <returns></returns>
		public ActionResult YBSBSC_JMQuery()
		{
			ReportingServiceCom();

			return View();
		}

		/// <summary>
		/// 医保申报互助帮困
		/// </summary>
		/// <returns></returns>
		public ActionResult YBSBSC_HZBKQuery()
		{
			ReportingServiceCom();

			return View();
		}

		/// <summary>
		/// 医保申报异地
		/// </summary>
		/// <returns></returns>
		public ActionResult YBSBSC_YDQuery()
		{
			ReportingServiceCom();

			return View();
		}

		/// <summary>
		/// 医保申报导出文件dbf格式 并返回下载文件地址 在前端页面进行下载弹出
		/// </summary>
		/// <returns></returns>
		public ActionResult DCQuery_dbf(string types, string ksrq, string jsrq)
		{
			string year = DateTime.Now.ToString("yy");
			int mone= DateTime.Now.Month;
			string Month = mone.ToString();
			if (mone>9)
			{
				switch (mone)
				{
					case 10:
						Month = "A";
						break;
					case 11:
						Month = "B";
						break;
					case 12:
						Month = "C";
						break;
				}
			}
			string pathname = "YNE" + year + Month + types + ".dbf";
			var uri = Request.Url.AbsoluteUri;
			string filepath = Server.MapPath("~/医保申报上传文件/" + pathname);
			var Scheme = Request.Url.Scheme;
			var Host = Request.Url.Authority;
			
			var refdata = new
			{
				path =""
			};
			//"D:\\医保申报上传文件" + "\\" + "YNE" + year + Month + types + ".dbf";
			if (System.IO.File.Exists(Path.GetFullPath(filepath)))
			{
				System.IO.File.Delete(Path.GetFullPath(filepath));
			}
			//获取数据 判断是否有数据需要导出下载
			var list = _reportCommonDmnService.yBSBMX_DCdbfs(types, ksrq, jsrq, OrganizeId);
			IEnumerable<YBSBMX> data = null;
			switch (types)
			{
				case "1":
					data = new List<ZGMJZ_Query>();
					data = list.zgmjzvo;
					if (list.zgmjzvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "2":
					data = new List<ZGDBMZ_Qury>();
					data = list.zgdbmzvo;
					if (list.zgdbmzvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "3":
					data = new List<ZGZY_Query>();
					data = list.zgzyvo;
					if (list.zgzyvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "4":
					data = new List<ZGMJZTS_Query>();
					data = list.zgmjztsvo;
					if (list.zgmjztsvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "5":
					data = new List<ZGZY_Query>();
					data = list.zgzyvo;
					if (list.zgzyvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "Q":
					data = new List<JMMJZ_Query>();
					data = list.jmmjzvo;
					if (list.jmmjzvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "R":
					data = new List<JMZY_Query>();
					data = list.jmzyvo;
					if (list.jmzyvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "S":
					data = new List<HZBK_Query>();
					data = list.hzbkvo;
					if (list.hzbkvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "W":
					data = new List<MZYD_Query>();
					data = list.ydmzvo;
					if (list.ydmzvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
				case "E":
					data = new List<ZYYD_Query>();
					data = list.ydzyvo;
					if (list.ydzyvo.Count <= 0)
					{
						return Content(refdata.ToJson());
					}
					break;
			}
			using (System.IO.FileStream fs = new System.IO.FileStream(filepath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
			using (var writer = new DBFWriter(fs))
			{
				foreach (var item in data)
				{
					System.Reflection.PropertyInfo[] properties = item.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
					DBFField[] fields = new DBFField[properties.Length];
					object[] listval = new object[properties.Length];
					var i = 0;
					foreach (var typeitem in properties)
					{
						var type = NativeDbType.Char;
						var lenth = 50;
						var decimallentg = 0;
						listval[i] = typeitem.GetValue(item, null);
						switch (typeitem.PropertyType.Name.ToLower())
						{
							case "string":
								type = NativeDbType.Char;
								lenth = 32;
								break;
							case "decimal":
								type = NativeDbType.Numeric;
								lenth = 10;
								decimallentg = 2;
								break;
						}
						if (decimallentg > 0)
						{
							fields[i] = new DBFField(typeitem.Name, type, lenth, decimallentg);
						}
						else
						{
							fields[i] = new DBFField(typeitem.Name, type, lenth);
						}

						i++;
					}
					if (writer.Fields == null)
					{
						writer.Fields = fields;
					}
					writer.CharEncoding = System.Text.Encoding.Default;
					writer.WriteRecord(listval);
					listval = null;
				}
			}

			#region  dbf文件FileStream读取不到文件 未找到原因 现使用拼接地址前端下载
			//try
			//{
			//	System.IO.FileStream fs = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 40960,true);

			//	byte[] b = new Byte[fs.Length];
			//	fs.Read(b, 0, b.Length);
			//	fs.Flush();
			//	fs.Close();

			//	//System.IO.File.Delete(SavePdfPath);
			//	Response.Clear();
			//	Response.ClearHeaders();
			//	Response.Clear();
			//	Response.ClearHeaders();
			//	Response.Buffer = false;
			//	Response.ContentType = "application/octet-stream";      //ContentType;
			//	Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(pathname, System.Text.Encoding.UTF8));
			//	Response.AppendHeader("Content-Length", b.Length.ToString());
			//	fs.Close();
			//	fs.Close();
			//	if (b.Length > 0)
			//	{
			//		Response.OutputStream.Write(b, 0, b.Length);
			//	}

			//	Response.Flush();
			//	Response.End();
			//}
			//catch (Exception ex)
			//{
			//	string msg = ex.Message;
			//}
			//return File(new FileStream(filepath, FileMode.OpenOrCreate), "application/octet-stream", pathname);//“text/plain”是文件MIME类型
			//return File(filepath, "application/x-dbf", pathname);
			#endregion

			var refpath = Scheme + "://" + Host + "/医保申报上传文件/" + pathname;
			refdata = new
			{
				path = refpath
			};
			return Content(refdata.ToJson());
		}

        #endregion
        #region 获取收费项目
        /// <summary>
        /// 获取大类名称
        /// </summary>
        /// <returns></returns>
        public ActionResult Getsfxm(Pagination pagination, string xzstr,string py,string sfdl)
        {
            var list = new
            {
                rows = _reportCommonDmnService.Getsfxm(pagination, xzstr, py, sfdl, this.OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }
        #endregion 医保上报报表
        #region
        /// <summary>
        /// 总页面
        /// </summary>
        /// <returns></returns>
        public ActionResult YBXXQueryUpload()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 交易记录库
        /// </summary>
        /// <returns></returns>
        public ActionResult JYJLQueryUpload()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 门急诊大病挂号库
        /// </summary>
        /// <returns></returns>
        public ActionResult DBGHQueryUpload()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 门急诊家床收费库
        /// </summary>
        /// <returns></returns>
        public ActionResult JCSFQueryUpload()
        {
            ReportingServiceCom();

            return View();
        }
        /// <summary>
        /// 获取交易记录信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetJyjlData(Pagination pagination, string ksrq, string jsrq)
        {
            var list = new
            {
                rows = _reportCommonDmnService.GetJyjlData(pagination, ksrq, jsrq,this.OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }
        public ActionResult Ybsjsctotxt(string ksrq, string jsrq)
        {
            StringBuilder sb = new StringBuilder();
            var rows = _reportCommonDmnService.GetJyjlDatatotxt(ksrq, jsrq, this.OrganizeId);
            return Content(rows.ToJson()); 
        }
        /// <summary>
        /// 获取门急诊大病挂号库
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMjzdbghData(Pagination pagination, string ksrq, string jsrq)
        {
            var list = new
            {
                rows = _reportCommonDmnService.GetMjzdbghData(pagination, ksrq, jsrq, this.OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }
        public ActionResult YbMjzdbghtxt(string ksrq, string jsrq)
        {
            StringBuilder sb = new StringBuilder();
            var rows = _reportCommonDmnService.GetYbMjzdbghtxt(ksrq, jsrq, this.OrganizeId);
            return Content(rows.ToJson());
        }
        /// <summary>
        /// 获取门急诊大病加床收费数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMjzjcsfData(Pagination pagination, string ksrq, string jsrq)
        {
            var list = new
            {
                rows = _reportCommonDmnService.GetMjzjcsfData(pagination, ksrq, jsrq, this.OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }
        public ActionResult GetMjzjcsfDatatxt(string ksrq, string jsrq)
        {
            StringBuilder sb = new StringBuilder();
            var rows = _reportCommonDmnService.GetMjzjcsfDatatxt(ksrq, jsrq, this.OrganizeId);
            return Content(rows.ToJson());
        }
        #endregion
    }
}
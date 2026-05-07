using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtonsoft.Json;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ValueObjects.InpatientVitalSigns;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using Newtouch.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtouch.Common.Web;
using Newtouch.Domain.ValueObjects;
using Newtouch.Common;
using Newtouch.Domain.ValueObjects.Inpatient;

namespace Newtouch.CIS.Web.Areas.NurseManage.Controllers
{
    /// <summary>
    /// 住院患者生命体征
    /// </summary>
    public class InpatientVitalSignsController : OrgControllerBase
    {
        private readonly IInpatientVitalSignsRepo _inpatientVitalSignsRepo;
        private readonly IInpatientPatientDmnService _inpatientPatientDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;

        public ActionResult NursingInput() {
            ViewBag.MutipatientNursingInputFlag = _sysConfigRepo.GetIntValueByCode("MutipatientNursingInputFlag", OrganizeId, 0);
            ViewBag.ScdCode = _sysConfigRepo.GetValueByCode("ScdTimePoint", OrganizeId);//三测单时间点奇偶数设置 1 奇数 0 偶数
            return View();
        }
        /// <summary>
        /// 护理查询
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryIndex()
        {
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            return View();
        }

        public override ActionResult  Form() {
            ViewBag.hsqm = UserIdentity.UserName;
            ViewBag.ScdCode = _sysConfigRepo.GetValueByCode("ScdTimePoint", OrganizeId);//三测单时间点奇偶数设置 1 奇数 0 偶数
            return View();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(Pagination pagination, DateTime? kssj, DateTime? jssj, string zyh,string wardCode,string isShowDelete)
        {
            var data = new
            {
                rows = _inpatientVitalSignsRepo.GetPaginationList(pagination, this.OrganizeId, kssj, jssj, zyh,wardCode,isShowDelete),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 护理查询（按日期时间排序）
        /// </summary>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetTemperatureChartJson(DateTime? kssj, DateTime? jssj, string zyh)
        {
            if (!kssj.HasValue || !jssj.HasValue)
            {
                throw new FailedException("HLCX_DATE_REQUIRED", "请选择日期");
            }
            if ((jssj.Value.Date - kssj.Value.Date).TotalDays < 0)
            {
                throw new FailedException("结束时间不能大于开始时间");
            }
            if ((jssj.Value.Date - kssj.Value.Date).TotalDays > 30)
            {
                throw new FailedException("HLCX_DATE_OUTOFRANGE", "日期范围不能超过30天");
            }
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("HLCX_ZYH_REQUIRED", "请选择住院号");
            }
            var patInfo = _inpatientPatientDmnService.GetInfoByZyh(zyh, this.OrganizeId);
            //2020-4-28 取消体温单查询时间强制到入院出院时间
            if (patInfo!=null&&patInfo.ryrq>kssj)
            {
                kssj = patInfo.ryrq;
                //throw new FailedException("入院日期："+patInfo.ryrq+";请输入正确的开始时间");
            }
            if (patInfo != null && patInfo.cqrq != null && patInfo.cqrq < jssj)
            {
                jssj = patInfo.cqrq;
                //throw new FailedException("出院日期：" + patInfo.cqrq + ";请输入正确的结束时间");
            }
           
            var ValidList = _inpatientVitalSignsRepo.GetValidList(this.OrganizeId, kssj, jssj, zyh);
            //var treeList = new List<ValidListVO>();
            //for (int i = 0; i < ValidList.Count(); i++)
            //{
            //    var rqpar = ValidList.Where(p => p.rq == ValidList[i].rq);
            //    var timeslist = new List<times>();
            //    foreach (var item in rqpar)
            //    {
            //        var entity = new times
            //        {
            //            hx = item.hx,
            //            mb = item.mb,
            //            sj = item.sj,
            //            tw = item.tw,
            //            xysz = item.xysz,
            //            xyxz = item.xyxz
            //        };
            //        timeslist.Add(entity);
            //    }
            //    treeList.Add(new ValidListVO { rq = ValidList[i].rq, times = timeslist });
            //}
            //var data = new
            //{
            //    patInfo = patInfo,
            //    list = treeList
            //};

            //20190529回滚至之前的


            var req =new {
                OrganizeId = this.OrganizeId,
                ksrq=kssj,
                jsrq=jssj,
                zyh=zyh,
                djzt=2
            };
            var patoplist = SiteORAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/ORPatient/ORPatList", req);
            List<PatOpInfoVO> oplist =new List<PatOpInfoVO>();
            if (patoplist != null && patoplist.code == APIRequestHelper.ResponseResultCode.SUCCESS)
            {
                oplist = Tools.Json.ToObject<List<PatOpInfoVO>>(patoplist.data.ToString());
            }

            var data = new
            {
                patInfo = patInfo,
                list = ValidList,
                oplist= oplist
            };

            return Content(data.ToJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue)
        {
            var data = _inpatientVitalSignsRepo.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 提交保存护理录入
        /// </summary>
        /// <param name="moduleEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        //[ValidateAntiForgeryToken]
        public ActionResult SubmitForm(InpatientVitalSignsEntity entity, string keyValue)
        {
            entity.OrganizeId = this.OrganizeId;
            entity.zt = "1";
            _inpatientVitalSignsRepo.SubmitForm(entity, keyValue);
            return Success("保存成功。");
        }

        public ActionResult Export(DateTime? kssj, DateTime? jssj, string zyh)
        {
            var data = _inpatientVitalSignsRepo.GetList(OrganizeId,kssj,jssj,zyh).ToList();
            var path = DateTime.Now.ToString("\\\\护理录入查询yyyyMMdd\\\\HHmmssfff") + ".xls";
            var filePath = CommmHelper.GetLocalFilePath("\\Excel导出\\护理录入查询" + path);
            var cols = new List<ExcelColumn>()
            {
                    new ExcelColumn("rq","日期"),
                    new ExcelColumn("sj","时间"),
                    new ExcelColumn("hljb","护理级别"),
                    new ExcelColumn("hlys","意<br/>识"),
                    new ExcelColumn("brfood","饮食"),
                    new ExcelColumn("tw","体温"),
                    new ExcelColumn("mb","脉搏"),
                    new ExcelColumn("hx","呼吸"),
                     new ExcelColumn("tiwen","体温"),
                      new ExcelColumn("xueya","血压"),
                       new ExcelColumn("cfz","初复诊"),
                        new ExcelColumn("zs","主要症状"),
                        new ExcelColumn("xyzdmc","诊断名称"),
                        new ExcelColumn("clfa","处理"),
                        new ExcelColumn("fzjc","辅助检查"),
                         new ExcelColumn("ysqz","医生签字"),
                          new ExcelColumn("sfzh","有效证件号"),
                          new ExcelColumn("ContactNum","电话"),
                    new ExcelColumn("ghksmc","科别"),
                    new ExcelColumn("xuetang","血糖"),
                    new ExcelColumn("xbs","现病史"),
                    new ExcelColumn("bz","备注"),
            };
            var sheet = new ExcelSheet()
            {
                Title = "门诊日志列表",
                columns = cols
            };

            var result = data.ToExcel(filePath, sheet);
            if (result)
            {
                return File(filePath, "application/x-xls", path.Replace("\\", ""));
            }
            else
            {
                return Content("文件导出失败，请返回列表页重试");
            }
        }

        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult DeleteForm(string keyValue)
        {
            _inpatientVitalSignsRepo.DeleteForm(keyValue);
            return Success("作废成功。");
        }

        /// <summary>
        /// 体温单
        /// </summary>
        /// <returns></returns>
        public ActionResult TemperatureChartIndex()
        {
            ViewBag.ScdCode = _sysConfigRepo.GetValueByCode("ScdTimePoint", OrganizeId);//三测单时间点奇偶数设置 1 奇数 0 偶数
            ViewBag.HospitalName = ConfigurationHelper.GetAppConfigValue("HospitalName");
            return View();
        }

        /// <summary>
        /// 体温单 全屏
        /// </summary>
        /// <returns></returns>
        public ActionResult FullScreenTemperatureChartIndex()
        {
            ViewBag.HospitalName = ConfigurationHelper.GetAppConfigValue("HospitalName");
            return View();
        }

        #region 秦皇岛护理录入版本
        public ActionResult GetNursingInputGridJson(string bq,string sj,DateTime rq,string zyh) {
           var data= _inpatientPatientDmnService.GetInPatSearchList(OrganizeId,bq,rq,sj, zyh, ((int)EnumZYBZ.Bqz).ToString());
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取病区人员执行树
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPatWardTree(string zyzt, string keyword)
        {

            var wardTree = _inpatientPatientDmnService.GetPatTree(OrganizeId, zyzt, keyword);
            var wardonly = wardTree.GroupBy(p => new { p.bqCode, p.bqmc }).Select(p => new { p.Key.bqCode, p.Key.bqmc });


            var treeList = new List<TreeViewModel>();
            foreach (var item in wardonly)
            {
                var patInfo = wardTree.Where(p => p.bqCode == item.bqCode).Where(p => p.zyh != "").Where(p => p.zyh != null).ToList();
                var NewPatInfo = patInfo.OrderBy(p => p.BedNo);
                foreach (InpWardPatTreeVO itempat in NewPatInfo)
                { 
                    string gender = itempat.sex == "1" ? "男" : "女";
                    TreeViewModel treepat = new TreeViewModel();
                    treepat.id = itempat.zyh;
                    //床号 + 姓名(住院天数)+住院号 + 年龄 +性别
                    treepat.text = itempat.BedNo + "-" + itempat.hzxm + "(" + itempat.inHosDays + "天)" + "-" + itempat.zyh + "-" + itempat.nl + "岁-" + gender;
                    treepat.value = itempat.zyh;
                    treepat.parentId = item.bqCode;
                    treepat.isexpand = false;
                    treepat.complete = true;
                    treepat.showcheck = true;
                    treepat.checkstate = 0;
                    treepat.hasChildren = false;
                    treepat.Ex1 = "c";
                    treepat.Ex2 = itempat.rqrq;
                    treepat.Ex3 = itempat.cqrq;
                    treepat.Ex4 = itempat.hzxm;
                    treeList.Add(treepat);
                }

                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = patInfo.Count == 0 ? false : true;
                tree.id = item.bqCode;
                tree.text = item.bqmc;
                tree.value = item.bqCode;
                tree.parentId = null;
                tree.isexpand = true;
                tree.complete = true;
                tree.showcheck = true;
                tree.checkstate = 0;
                tree.hasChildren = hasChildren;
                tree.Ex1 = "p";
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson(null));
        } 
        public ActionResult GetPatWard(string zyh)
        {
            var wardTree = _inpatientPatientDmnService.GetPatList(OrganizeId, zyh);
            return Content(wardTree.ToJson());
        }
        public ActionResult submitmutiple(List<InpatientVitalSignsEntity> entityList, int? sjd,DateTime rq,string flag,List<string> xysxzList)
        {
            for (var i = 0; i < entityList.Count; i++) {
                var xysz = 0;
                var xyxz = 0;
                var xyszx = xysxzList[i].Split('/');
                if (xyszx.Length == 2)
                {
                    xysz = xyszx[0].ToInt();
                    xyxz = xyszx[1].ToInt();
                }
                entityList[i].xysz = xysz;
                entityList[i].xyxz = xyxz;
            }
            _inpatientPatientDmnService.submitmutiple(entityList,OrganizeId,sjd,rq,flag);
            return Success();
        }
        #endregion


        #region DrawData 
        public ActionResult DrawData(DateTime? kssj, DateTime? jssj, string zyh,int? pagesize)
		{
			var patInfo = _inpatientPatientDmnService.GetInfoByZyh(zyh, this.OrganizeId);
            //2021-9-7修改：三测单去掉时间查询条件
            //if (!kssj.HasValue || !jssj.HasValue)
            //{
            //    throw new FailedException("HLCX_DATE_REQUIRED", "请选择日期");
            //}
            //if ((jssj.Value.Date - kssj.Value.Date).TotalDays < 0)
            //{
            //    throw new FailedException("结束时间不能大于开始时间");
            //}
            //if ((jssj.Value.Date - kssj.Value.Date).TotalDays > 30)
            //{
            //    //2021-5-11修改:住院时间超过30天的患者 可查询入院出院范围内的日期
            //    if (!(patInfo.ryrq <= kssj && patInfo.cqrq >= jssj))
            //    {
            //        throw new FailedException("HLCX_DATE_OUTOFRANGE", "日期范围不能超过30天");
            //    }
            //}
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("HLCX_ZYH_REQUIRED", "请选择住院号");
            }
            //2020-4-28 取消体温单查询时间强制到入院出院时间
            if (patInfo != null && patInfo.ryrq > kssj)
            {
                kssj = patInfo.ryrq;
                //throw new FailedException("入院日期："+patInfo.ryrq+";请输入正确的开始时间");
            }
            if (patInfo != null && patInfo.cqrq != null && patInfo.cqrq < jssj  && patInfo.zybz==(int)EnumZYBZ.Ycy)
            {
                jssj = patInfo.cqrq;
                //throw new FailedException("出院日期：" + patInfo.cqrq + ";请输入正确的结束时间");
            }

            var ValidList = _inpatientVitalSignsRepo.GetTempratureData(this.OrganizeId, kssj, jssj, zyh, pagesize??6);
            var req = new
            {
                OrganizeId = this.OrganizeId,
                ksrq = kssj,
                jsrq = jssj,
                zyh = zyh,
                djzt = 2
            };
            string enableor = SysConfigReader.String("EnableLinkToOR");
            List<PatOpInfoVO> oplist = new List<PatOpInfoVO>(); //是否启用手术系统
            if (enableor == "ON")
            {
                var patoplist = SiteORAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/ORPatient/ORPatList", req);
                if (patoplist != null && patoplist.code == APIRequestHelper.ResponseResultCode.SUCCESS)
                {
                    oplist = Tools.Json.ToObject<List<PatOpInfoVO>>(patoplist.data.ToString());
                }
            }

            var data = new
            {
                patInfo = patInfo,
                list = ValidList,
                oplist = oplist
            };

            return Content(data.ToJson());

        }
        public ActionResult Getzyhrq(string zyh)
        {
            var patInfo = _inpatientPatientDmnService.GetInfoByZyh(zyh, this.OrganizeId);
            string ryrq = "";
            if (patInfo != null && patInfo.ryrq != null)
            {
                ryrq = patInfo.ryrq.ToString();
            }
            return Content(ryrq);
        }
        public ActionResult Getzyhcqrq(string zyh)
        {
            var patInfo = _inpatientPatientDmnService.GetInfoByZyh(zyh, this.OrganizeId);
            string cqrq = "";
            if (patInfo != null && patInfo.cqrq != null && patInfo.cqrq.ToString() != "")
            {
                cqrq = patInfo.cqrq.ToString();
            }
            return Content(cqrq);
        }
        #endregion
    }
}
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtonsoft.Json;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Utils;
using Newtouch.EMR.APIRequest.Bljgh.Request;
using Newtouch.EMR.Domain.DTO;
using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.IDomainServices;
using Newtouch.EMR.Domain.IRepository;
using Newtouch.EMR.Domain.ValueObjects;
using Newtouch.EMR.Domain.ValueObjects.API;
using Newtouch.EMR.Domain.ValueObjects.MedicalRecord;
using Newtouch.EMR.Infrastructure;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Newtouch.EMR.Web.Areas.MedicalRecordManage
{
    /// <summary>
    /// 住院患者病历
    /// </summary>
    public class PatInfoController : OrgControllerBase
    {
        private readonly IZybrjbxxRepo _ZybrjbxxRepo;
        private readonly IZybrjbxxDmnService _ZybrjbxxDmnService;
        private readonly IBlmblbRepo _BlmblbRepo;
        private readonly IBlmblbDmnService _BlmblbDmnService;
        private readonly IMedicalRecordDmnService _medicalRecordDmnService;
        private readonly Ibl_ElementDomainRepo _bljsRepo;
        private readonly IZymeddocsrelationRepo _zymeddocsrelationRepo;
        private readonly IZybrjbxxRepo _zybrjbxxRepo;
        private readonly IMrWritingRulesRepo mrwritingrulesRepo;
        private readonly IMrBlApplyRecordRepo _mrblapplyrecordRepo;
        private readonly IMRHomePageDmnService _MainRecordDmnService;
        private readonly string IsEnableWebEditor= ConfigurationHelper.GetAppConfigValue("EnableWebHomePage");
        public static string BlBackupFilePath = ConfigurationHelper.GetAppConfigValue("BlBackupFilePath");

        public PatInfoController(IZybrjbxxRepo ZybrjbxxRepo, IZybrjbxxDmnService ZybrjbxxDmnService, IBlmblbRepo BlmblbRepo, IBlmblbDmnService BlmblbDmnService,IZymeddocsrelationRepo zymeddocsrelationRepo)
        {
            this._ZybrjbxxRepo = ZybrjbxxRepo;
            this._ZybrjbxxDmnService = ZybrjbxxDmnService;
            this._BlmblbRepo = BlmblbRepo;
            this._BlmblbDmnService = BlmblbDmnService;
            this._zymeddocsrelationRepo = zymeddocsrelationRepo;
        }
       
        public override ActionResult Index()
        {
            //OperatorModel user = this.UserIdentity; 
            //_ZybrjbxxDmnService.Sync_HisPatinfo(DateTime.Now,user);
            return View();
        }

        public ActionResult CommitShow()
        {
            return View();
        }
        public ActionResult MedicalApplySend()
        {
            return View();
        }

        #region CPOE
        public ActionResult patientFloatingSelectorSource(string keyword)
        {
            var data = _ZybrjbxxDmnService.GetPatInfoBykeyword(keyword, OrganizeId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取在病区的病人
        /// </summary>
        /// <param name="bqCode"></param>
        /// <param name="ys"></param>
        /// <param name="bedCode"></param>
        /// <returns></returns>
        public ActionResult GetzbqPatientList(string page, string bqCode, string ys, string bedCode)
        {
            Pagination pagination = new Pagination();
            pagination.sidx = " cwmc";
            pagination.sord = " asc";
            pagination.rows = 15;
            pagination.page = page.ToInt();

            if (string.IsNullOrWhiteSpace(ys))
            {
                ys = this.UserIdentity.rygh;
            }
            var data = new
            {
                rows = _ZybrjbxxDmnService.GetzbqPatientList(new PatientzbqRequestDto { pagination = pagination, bqcode = bqCode, cw = bedCode, ysgh = ys}, OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取已出区病人
        /// </summary>
        /// <param name="bqCode"></param>
        /// <param name="ys"></param>
        /// <param name="bedCode"></param>
        /// <param name="ksrq"></param>
        /// <param name="jsrq"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetycqPatientList(Pagination pagination, string bqCode, string ys, string bedCode, string ksrq, string jsrq, string zyh)
        {


            if (string.IsNullOrWhiteSpace(ys))
            {
                ys = this.UserIdentity.rygh;
            }
          
            var data = new
            {
                rows = _ZybrjbxxDmnService.GetycqPatientList(new PatientycqRequestDto { pagination = pagination, bqcode = bqCode, cw = bedCode, ysgh = ys, cqksrq = ksrq.AsDateTime(), cqjsrq = jsrq.AsDateTime(), zyh = zyh },OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        /// <summary>
        ///  获取医生分管的病人
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="bqCode"></param>
        /// <param name="ys"></param>
        /// <param name="bedCode"></param>
        /// <param name="ksrq"></param>
        /// <param name="jsrq"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetmyPatientList(Pagination pagination, string bqCode, string ys, string bedCode, string ksrq, string jsrq, string zyh)
        {
            if (string.IsNullOrWhiteSpace(ys))
            {
                ys = this.UserIdentity.rygh;
            }
            var data = new
            {
                rows = _ZybrjbxxDmnService.GetmyPatientList(new PatientmyRequestDto { pagination = pagination, bqcode = bqCode, cw = bedCode, ysgh = ys, ksrq = ksrq.AsDateTime(), jsrq = jsrq.AsDateTime(), zyh = zyh },OrganizeId),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        #endregion

        public ActionResult PatList(Pagination pagination,string keyword,string zyh,string type,string cyts,string blzt, string bq = null)
        {
            var chargeQueryList = new
            {
                rows = _ZybrjbxxDmnService.GetPatList(pagination, keyword,zyh,cyts,blzt, this.UserIdentity.rygh, this.OrganizeId,Convert.ToInt32( type),bq),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(chargeQueryList.ToJson());

        }

        public ActionResult GetPatinfo()
        {
            DateTime bt = DateTime.Now.AddMonths(-1);
            DateTime et = DateTime.Now;
            _ZybrjbxxDmnService.Sync_HisPatinfo(this.OrganizeId, bt, et);
            return Success("同步成功");
        }

        #region 病历管理
        public ActionResult PatMedRecordList()
        {
            var OpenEditorSwitch = _MainRecordDmnService.DiagnosticSave("OpenEditorSwitch", this.OrganizeId);
            ViewBag.openEditorSwitch = OpenEditorSwitch.Count > 0 ? OpenEditorSwitch.FirstOrDefault().Value : "";
            ViewBag.IsEnableEditor = IsEnableWebEditor;
            ReportingServiceCom();
            return View();
        }

        /// <summary>
        /// 获取病历树
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetMedRecordTree(string zyh)
        {
            OperatorModel user = this.UserIdentity;
            var data = _ZybrjbxxDmnService.GetPatMedRecordTree(this.OrganizeId, zyh,user.rygh);
            var treeList = new List<TreeGridModel>();
            foreach (var item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.parentId == item.Id) == 0 ? false : true;
                treeModel.id = item.Id.ToString();
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.parentId == null ? null : item.parentId.ToString();
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);

            }
            return Content(treeList.TreeGridJson(null));
        }


        //public ActionResult MedRecordTreeEdit()
        //{
        //    return View();
        //}
        public ActionResult MedRecordTreeEdit()
        {
            ViewBag.IsEnableEditor= IsEnableWebEditor;
            return View();
        }

        public ActionResult MedRecordTreeEditV2()
        {
            var OpenEditorSwitch = _MainRecordDmnService.DiagnosticSave("OpenEditorSwitch", this.OrganizeId);
            ViewBag.openEditorSwitch = OpenEditorSwitch.Count>0 ? OpenEditorSwitch.FirstOrDefault().Value:"";
            ViewBag.IsEnableEditor = IsEnableWebEditor;
            return View();
        }

        public ActionResult MedRecordTreeEditJson(string keyValue, string bllx,string zyh)
        {
            var pat = _ZybrjbxxRepo.FindEntity(p => p.zyh == zyh && p.OrganizeId == OrganizeId);

            MedRecordTreeEditVO data = new MedRecordTreeEditVO();
            data.bllxId = keyValue;
            data.zyh = pat.zyh;
            data.xm = pat.xm;
            data.birth = pat.birth;
            data.brxzmc = pat.brxzmc;
            data.sex = pat.sex;
            data.bllx = bllx;
            data.mbqx = (int)Enummbqx.pub;


            data.brjs = pat.sex == EnumSex.M.GetHashCode().ToString() ? pat.xm + " / " + EnumSex.M.GetDescription() : pat.xm + " / " + EnumSex.F.GetDescription();
            data.brjs += " / " + "年龄" + " / " + pat.brxzmc + " / " + pat.zyh;

            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取病历大类
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMedRecordType()
        {
            var data = new List<object>();
            //if(string.IsNullOrWhiteSpace(selectId))
            //{
            //    var qxz = new
            //    {
            //        id = "",
            //        text = "==请选择=="
            //    };
            //    data.Add(qxz);
            //}

            var list = _ZybrjbxxDmnService.GetSysItemDic( this.OrganizeId,"MedRecordDocuments",null);

            foreach (var item in list)
            {
                var obj = new
                {
                    id = item.Id,
                    text = item.Name
                };
                data.Add(obj);
            }
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取病历模板列表
        /// </summary>
        /// <param name="mbqx"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        public ActionResult GetMedRecordTempList(int? mbqx,string bllx,string keyword)
        {
            OperatorModel user = this.UserIdentity;
            IList<BlmbListVO> data = new List<BlmbListVO>();
            if (!string.IsNullOrWhiteSpace(bllx))
            {
                int qx = mbqx == null ? (int)Enummbqx.pub : Convert.ToInt32(mbqx);
                data = _BlmblbDmnService.MedRecordTmpList(qx, bllx, user.DepartmentCode, user, this.OrganizeId, keyword);
            }
            return Content(data.ToJson());
        }
        #endregion

        #region 病历操作
        public ActionResult UpdatePatMedRecordRel(string blId,string blgxId,string blName)
        {
            OperatorModel user = this.UserIdentity;
            _ZybrjbxxDmnService.UpdtPatMedRecord(blId, blgxId, this.OrganizeId, user);

            _medicalRecordDmnService.BLJG_Delete(blId,this.OrganizeId);
            BljghReq entity = new BljghReq();
            entity.blId = blId;
            entity.czr= this.UserIdentity.UserCode;
            entity.organizeId = this.OrganizeId;
            entity.blmc = blName;
            entity.delzt = "0";
            _bljsRepo.BljghDataDealwith(entity);
            return Success("删除成功");
               
        }

        public ActionResult UpdatePlanStu(string blId,string stu)
        {
            var ety = _zymeddocsrelationRepo.FindEntity(p => p.blId == blId && p.OrganizeId == OrganizeId && p.zt == "1");
            if (ety != null)
            {
                if (stu == ((int)EnumPlanStu.tz).ToString())
                {
                    ety.PlanStu = ((int)EnumPlanStu.tzqx).ToString();
                }
                else if (stu == ((int)EnumPlanStu.tzqx).ToString() || string.IsNullOrWhiteSpace(stu))
                {
                    ety.PlanStu = ((int)EnumPlanStu.tz).ToString();
                }

                ety.LastModifyTime = DateTime.Now;
                ety.LastModifierCode = this.UserIdentity.rygh;
                _zymeddocsrelationRepo.Update(ety);
            }
            return Success("操作成功");
        }
        #endregion

        /// <summary>
        /// 病历提交病案
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult CommitPatMedRecord(string zyh)
        {
            _ZybrjbxxRepo.CommitRecord(zyh, OrganizeId, this.UserIdentity.rygh);
            //if (!string.IsNullOrWhiteSpace(BlBackupFilePath))
            //{
            //    string Getaddress = Server.MapPath(BlBackupFilePath);
            //    Getaddress = Getaddress + zyh;
            //    Directory.Delete(Getaddress, true);
            //}
            return Success("病历已提交");

        }

        /// <summary>
        /// 病历退回
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult BackPatMedRecord(string zyh)
        {
            _ZybrjbxxRepo.BackRecord(zyh, OrganizeId, this.UserIdentity.rygh);
            //if (!string.IsNullOrWhiteSpace(BlBackupFilePath))
            //{
            //    string Getaddress = Server.MapPath(BlBackupFilePath);
            //    Getaddress = Getaddress + zyh;
            //    Directory.Delete(Getaddress, true);
            //}
            return Success("病历已提交");

        }




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
        

        public string selectBJQX(string blId, string blgxId)
        {
            OperatorModel user = this.UserIdentity;
            var bjqx = _ZybrjbxxDmnService.selectBJQX(blId, blgxId, this.OrganizeId,user);
            return bjqx;
        }

        public ActionResult PatHistoricalMedRecord()
        {
            //OperatorModel user = this.UserIdentity; 
            //_ZybrjbxxDmnService.Sync_HisPatinfo(DateTime.Now,user);
            return View();
        }


        public ActionResult getRepeatBl(string zyh,string bllx)
        { 
            var data = _BlmblbDmnService.getRepeatBl(this.OrganizeId, zyh, bllx);
            
            return Content(data.ToJson());
        }
        /// <summary>
        /// 规则判断
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        public ActionResult checkBlRules(string zyh, string bllx)
        {
            //var newblbznewblbz = _BlmblbDmnService.CheckBlRules(this.OrganizeId, zyh, bllx);
            var newblbz = "T";
            var applystatus = "Y";
            var zybrxx = _zybrjbxxRepo.FindEntity(p => p.zyh == zyh && p.OrganizeId == this.OrganizeId && p.zt == "1");

            var rulseentity = mrwritingrulesRepo.FindEntity(p => p.Bllx == bllx && p.OrganizeId == this.OrganizeId && p.zt == "1");
            var rulsdate = zybrxx.ryrq;
            if (rulseentity != null )
            {
                if (rulseentity.RulesDay != 0)
                {
                    rulsdate = rulsdate.AddDays(rulseentity.RulesDay);
                    if (rulsdate <= DateTime.Now)
                    {
                        newblbz = "F";
                    }
                    else
                    {
                        newblbz = "T";
                    }
                }
                
            }
            if (newblbz == "F") //是否有申请记录
            {
                var checkapplyrecord = _mrblapplyrecordRepo.FindEntity(p =>p.zyh==zyh && p.bllx == bllx && p.OrganizeId == this.OrganizeId && p.zt == "1" && p.wcsj >= DateTime.Now);
                if (checkapplyrecord != null)
                {
                    if (checkapplyrecord.applyStatus ==(int)ApplyStatusEnum.ysp)
                    {
                        newblbz = "T";
                    }
                    else
                        applystatus = "N";
                }
            }
            var data = new
            {
                newblbznewblbz = newblbz,
                applystatus= applystatus,
                zkrq = rulsdate,
                zybrxx = zybrxx
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 病历申请
        /// </summary>
        /// <param name="applyVo"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(MrApplyWritingVo applyVo)
        {
            applyVo.CreatorCode = this.UserIdentity.UserCode;
            applyVo.ApplyDate = DateTime.Now;
            string msg = "病历申请发送成功";
            var reqObj = new
            {
                Data = applyVo,
                AppId = AuthenManageHelper.appId,
                OrganizeId = OrganizeId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            var outstr = AuthenManageHelper.HttpPost<ApplyWritingResp>(reqObj.ToJson(), AuthenManageHelper.SiteConmonAPIHost + "api/MedicalRecordApply/AddMedicalApplyRecord", this.UserIdentity);
            if (outstr.code == 10000)
            {
                _mrblapplyrecordRepo.SubmitForm(applyVo, outstr.data,this.UserIdentity.UserCode,this.OrganizeId);
                msg = outstr.data.ApplyStatus==(int)ApplyStatusEnum.ysp? "病历申请成功,审批已通过，可继续编写病历": "病历申请成功,请联系质控办审批";
            }
            return Success(outstr.code == 10000 ? msg : outstr.msg);
        }

        #region 病历质控

        public ActionResult MrqcAdd()
        {
            return View();
        }

        //获取病历质控达标情况
        public ActionResult GetMrqcScore(string zyh, string bllxId)
        {
            string token = AuthenManageHelper.GetToken();
            //string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJBcHBJZCI6IkFQSS5NYW5hZ2UiLCJPcmdhbml6ZUlkIjoiNmQ1NzUyYTctMjM0YS00MDNlLWFhMWMtZGY4YjQ1ZDM0NjlmIiwiVG9wT3JnYW5pemVJZCI6ImQ4ZWU4NWY4LWQ4ZDEtNGI1Yy1iZDllLTE3NjFmNDQyNDJkOCIsIkFjY291bnQiOiIwMDAwMDAiLCJVc2VyQ29kZSI6IjAwMDAwMCIsIlVzZXJOYW1lIjoiMDAwMDAwIiwiVXNlcklkIjoiYjI0NWE2MzMtMTk3Ni00ZGFlLTkxNTMtNThhZmVkOWQyYjcwIiwiVG9rZW5UeXBlIjoiIiwiVG9rZW4iOiIiLCJleHAiOjE3MDE2NzQyMDIsImlzcyI6Ik5ld3RvdWNoLk9wZW5ISVMiLCJhdWQiOiJUZXJtaW5hbCJ9.1nAXqKmJZE7NZ5UdTcl2R6TpCT9MrQwRrFBQN20jnBE";
            var DataObj = new
            {
                orgId=this.OrganizeId,
                zyh = zyh,
                bllxId = bllxId
            };
            var reqObj = new
            {
                Data = DataObj,
                AppId = AuthenManageHelper.accessAppId,
                OrganizeId = OrganizeId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            var outstr = AuthenManageHelper.HttpPost(reqObj.ToJson(), AuthenManageHelper.SiteConmonAPIHost + "api/MRQCScore/GetMrqcScore", token);
            AuthenTokenHelper apiResp = JsonConvert.DeserializeObject<AuthenTokenHelper>(outstr);
            var data = JsonConvert.DeserializeObject<List<MRQCScoreVO>>(apiResp.BusData.data.ToString());
            return Content(data.ToJson());
        }
        #endregion
    }
}
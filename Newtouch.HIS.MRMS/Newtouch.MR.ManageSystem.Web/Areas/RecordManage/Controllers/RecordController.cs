using DCSoft.Writer;
using DCSoft.Writer.Controls;
using DCSoft.Writer.Dom;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.MR.ManageSystem.Domain.IDomainServices;
using Newtouch.MR.ManageSystem.Domain.IRepository;
using Newtouch.MR.ManageSystem.Domain.ValueObjects;
using Newtouch.MR.ManageSystem.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace Newtouch.MR.ManageSystem.Web.Areas.RecordManage.Controllers
{
    public class RecordController : OrgControllerBase
    {
#pragma warning disable CS0649 // Field 'RecordController._EMRDmnService' is never assigned to, and will always have its default value null
        private readonly IEMRDmnService _EMRDmnService;
#pragma warning restore CS0649 // Field 'RecordController._EMRDmnService' is never assigned to, and will always have its default value null
#pragma warning disable CS0649 // Field 'RecordController._RecordDmnService' is never assigned to, and will always have its default value null
        private readonly IRecordDmnService _RecordDmnService;
#pragma warning restore CS0649 // Field 'RecordController._RecordDmnService' is never assigned to, and will always have its default value null
        public static string BlTemplatePath = ConfigurationHelper.GetAppConfigValue("BlTemplatePath");
        public static string BlFilePath = ConfigurationHelper.GetAppConfigValue("BlFilePath");
        public static string EditorRegCode = ConfigurationHelper.GetAppConfigValue("EditorRegCode");
#pragma warning disable CS0169 // The field 'RecordController._CommonDmnService' is never used
        private readonly ICommonDmnService _CommonDmnService;
#pragma warning restore CS0169 // The field 'RecordController._CommonDmnService' is never used
#pragma warning disable CS0649 // Field 'RecordController.engine' is never assigned to, and will always have its default value null
        private static WebWriterControlEngine engine;
#pragma warning restore CS0649 // Field 'RecordController.engine' is never assigned to, and will always have its default value null
#pragma warning disable CS0649 // Field 'RecordController._ZybrjbxxDmnService' is never assigned to, and will always have its default value null
        private readonly IZybrjbxxDmnService _ZybrjbxxDmnService;
#pragma warning restore CS0649 // Field 'RecordController._ZybrjbxxDmnService' is never assigned to, and will always have its default value null

        // GET: RecordManage/Record
        public ActionResult Receive()
        {
             EMRBasyPrintReportCom();

            return View();
        }

        #region 病历签收功能页
        public ActionResult PatList(Pagination pagination, string keyword, string zyh, string type, string cyts, string blzt,string kssj,string jssj)
        {
            var chargeQueryList = new
            {
                rows = _EMRDmnService.GetPagintionList(pagination, keyword, zyh, null, blzt, this.UserIdentity.rygh, this.OrganizeId, Convert.ToInt32(type),kssj,jssj),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            kssj = "";jssj = "";
            return Content(chargeQueryList.ToJson());

        }

        /// <summary>
        /// 获取病历树
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetMedRecordTree(string zyh)
        {
            OperatorModel user = this.UserIdentity;
            var data = _EMRDmnService.GetPatMedRecordTree(this.OrganizeId, zyh, user.rygh);
            var treeList = new List<TreeGridModel>();
            foreach (var item in data)
			{
				bool hasChildren = data.Count(t => t.parentId == item.Id) != 0;
				var treeModel = new TreeGridModel
				{
					id = item.Id,
					isLeaf = hasChildren,
					parentId = item.parentId == null ? null : item.parentId.ToString(),
					expanded = hasChildren,
					entityJson = item.ToJson()
				};

				treeList.Add(treeModel);

            }
            return Content(treeList.TreeGridJson(null));
        }


        #endregion

        #region  开源版编辑器
        public ActionResult PreViewV2()
        {
            return View();
        }

        public ActionResult LoadMedicalRecordHtml(string blid, string bllx, string zyh)
        {
            string currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + "\\File\\Template\\" + "newFile.html";
            string bllj = "";
            string blxtmc_yj = "";
            var MedicalRecord = _RecordDmnService.GetMedicalRecord(blid, bllx, OrganizeId);
            bllj = MedicalRecord.blxtml;
            blxtmc_yj = MedicalRecord.blxtmc_yj;
            if (string.IsNullOrWhiteSpace(MedicalRecord.blxtmc_yj))
            {
                blxtmc_yj = _RecordDmnService.GetZymeddocsrelation(blid, OrganizeId).blmc;
            }
            currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + bllj + blxtmc_yj.Trim() + ".html";
            currentFileName = currentFileName.Replace("~", "");
            currentFileName = currentFileName.Replace("/", "\\");
            if(System.IO.File.Exists(currentFileName) == false)//如果不存在就打开默认模板
            {
                currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + "\\File\\Template\\" + "newFile.html";
            }
            // 加载模板
            string htmldata = System.IO.File.ReadAllText(currentFileName);
            BlTemplateVo vo = new BlTemplateVo();
            vo.xmldata = htmldata;
            vo.templateId = blid;
            vo.templateName = blxtmc_yj;
            return Content(vo.ToJson());
        }

        #endregion

        public ActionResult PreView(string blid, string bllx, string zyh)
        {
            try
            {
                //如果找不到加载的xml就默认为空白的xml
                ViewBag.blid = blid;
                ViewBag.bllx = bllx;
                ViewBag.zyh = zyh;
                //string currentFileName = Server.MapPath(BlTemplatePath + "newFile.xml");
                string currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + "\\File\\Template\\" + "newFile.xml";
                string bllj = "";
                string blxtmc_yj = "";
                // 加载文件
                WebWriterControlEngine eng;
                if (engine == null)
                {
                    eng = GetControlEngine();
                }
                else
                {
                    eng = engine;
                }
                var MedicalRecord = _RecordDmnService.GetMedicalRecord(blid, bllx, OrganizeId);
                bllj = MedicalRecord.blxtml;
                blxtmc_yj = MedicalRecord.blxtmc_yj;

                if (string.IsNullOrWhiteSpace(MedicalRecord.blxtmc_yj))
                {
                    blxtmc_yj = _RecordDmnService.GetZymeddocsrelation(blid, OrganizeId).blmc;
                    //blxtmc_yj = _ZymeddocsrelationRepo.FindEntity(p => p.blId == blid).blmc;
                }

                int isLock = Convert.ToInt32(MedicalRecord.IsLock);
                ViewBag.isLocker = "";
                if (isLock == 1)
                {
                    if (MedicalRecord.LastModifierCode != this.UserIdentity.UserCode)
                    {
                        ViewBag.isLocker = MedicalRecord.LastModifierCode + "正在编辑中";
                    }
                    else
                    {
                        LockRecord(blid, bllx, 2);
                    }
                }
                currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + bllj + blxtmc_yj.Trim() + ".xml";
                currentFileName = currentFileName.Replace("~", "");
                currentFileName = currentFileName.Replace("/", "\\");
                //currentFileName = Server.MapPath(bllj + blxtmc_yj.Trim() + ".xml");
                if (System.IO.File.Exists(currentFileName) == false)//如果不存在就打开默认模板
                {
                    currentFileName = ConfigurationHelper.GetAppConfigValue("DcWriterFileRoter") + "\\File\\Template\\" + "newFile.xml";
                    //currentFileName = Server.MapPath(BlTemplatePath + "newFile.xml");
                }
                // 加载模板
                eng.LoadDocument(currentFileName, null);
                var Zybrjbxx = GetEMRZybrjbxxByZYH(zyh);
                XMLDataBind(Zybrjbxx, eng);

                eng.Options.ContentRenderMode = WebWriterControlRenderMode.PagePreviewHtml;
                var d = eng.GetAllContentHtml();
                string oldstr = "span style=&quot;color:black;font-size:9pt;background-color:white";
                string tip = "style=\"position:relative;left:4px;top:1px";
                ViewBag.WriterControlHtml = eng.GetAllContentHtml().Replace(oldstr + "&quot;&gt; 常", oldstr + ";display:none&quot;&gt; 常")
                                            .Replace(oldstr + "&quot;&gt; &amp;#x5E38", oldstr + ";display:none&quot;&gt; &amp;#x5E38")
                                            .Replace(tip, tip + ";display:none ");

                eng.Dispose();
                ViewBag.isLock = isLock;

                return View();
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return null;
            }
        }

        #region 数据初始化
        /// <summary>
        /// 根据住院号获取病人基本信息
        /// 实体必须是可序列化的才能在病历控件里绑定上数据所以使用ZybrjbxxVO
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        private ZybrjbxxVO GetEMRZybrjbxxByZYH(string zyh)
        {
            return _EMRDmnService.GetZyPatInfobyzyh(OrganizeId, zyh);
        }
        #endregion 

        /// <summary>
        /// 根据住院号获取病人基本信息
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public BabasyVO GetBasyByZYH(string zyh)
        {

            var BabasyVO = _ZybrjbxxDmnService.GetPatBasicInfo_basy(OrganizeId, zyh); //_ZybrjbxxRepo.GetZybrjbxx(zyh, OrganizeId);

            //var BabasyVO = new BabasyVO();
            //BabasyVO.zyh = ZybrjbxxEntity.zyh;
            //BabasyVO.blh = ZybrjbxxEntity.blh;
            //BabasyVO.brxm = ZybrjbxxEntity.;
            //BabasyVO.sfzhm = ZybrjbxxEntity.sfzh;
            //BabasyVO.xb = ZybrjbxxEntity.sex;
            //BabasyVO.csny = ZybrjbxxEntity.birth;
            //BabasyVO.hljb = ZybrjbxxEntity.hljb;
            //BabasyVO.ryrq = ZybrjbxxEntity.ryrq;
            ////BabasyVO.rqrq = ZybrjbxxEntity.rqrq;
            ////BabasyVO.cqrq = ZybrjbxxEntity.cqrq;
            ////BabasyVO.wzjb = ZybrjbxxEntity.wzjb;

            //BabasyVO.rytj = ZybrjbxxEntity.ryfs;
            ////BabasyVO.cyfs = ZybrjbxxEntity.cyfs;
            ////BabasyVO.gdxmzxrq = ZybrjbxxEntity.gdxmzxrq;
            ////BabasyVO.brxzdm = ZybrjbxxEntity.brxzdm;
            ////BabasyVO.brxzmc = ZybrjbxxEntity.brxzmc;
            ////BabasyVO.cardno = ZybrjbxxEntity.cardno;
            ////BabasyVO.cardtype = ZybrjbxxEntity.cardtype;
            ////BabasyVO.zybz = ZybrjbxxEntity.zybz;
            ////BabasyVO.sfqj = ZybrjbxxEntity.sfqj;
            ////BabasyVO.DeptCode = ZybrjbxxEntity.DeptCode;
            ////BabasyVO.WardCode = ZybrjbxxEntity.WardCode;
            ////BabasyVO.ysgh = ZybrjbxxEntity.ysgh;
            ////BabasyVO.BedCode = ZybrjbxxEntity.BedCode;
            //BabasyVO.lxr = ZybrjbxxEntity.lxr;
            //// BabasyVO.lxrgx = ZybrjbxxEntity.lxrgx;
            //BabasyVO.lxdh = ZybrjbxxEntity.lxrdh;

            //BabasyVO.cyzdmc = ZybrjbxxEntity.cyzdmc;

            return BabasyVO;
        }

        private WebWriterControlEngine GetControlEngine()
        {
            WebWriterControlEngine engine = new WebWriterControlEngine();
            if (!string.IsNullOrWhiteSpace(EditorRegCode))
            {
                engine.SetRegisterCode(EditorRegCode);
            }
            else
            {
                engine.SetRegisterCode("04DD8DFEA57F329DAECA2E1BA6EFF0FFBAD96AEAC18BE949A1241CE1EF917A3C65D8B37D5F6E79E9CEBECF27B0A7CE6F66DC0CE1B2892C2C8D6AC6C8D3AD8BD50D1777AC3B57D7988D785AFA2915480A1E8B38F99330C0A4ACCCAB8155A414B35DFFD72CC1EFB4FE4FF4D52485A7BB70A0DE88DBE1A1D1FB7637963FFCCDAB1021B570B8050A76C3C5BA968A9941265505AA4071C4722B817C3BF0750A4CA809270559130AEDCF6561327034D85DBB29F8A8C1CDC323889085D2FACCAE941A2D3A6D7EBE44782B2977D367E84690C849533CC9E546A41F0AD5");
            }
            //设置注册码
            //基础版注册码
            //升级版注册码（添加格式加载）
            // engine.SetRegisterCode("040149FF0AA70373F1D129EEEC0D495F734089A6530C102319381C25EAA3E07E86EBA8F8FFF3C9EDACB7574B418743218FEA078B710058C3A657F8C2F057A0B6AC54519E0FA0A5601AF1E2AE33BC207C5C45BBDFAA0583865F6EF983A182B3D285ED06AC9C8241A353");

            //临时
            // engine.SetRegisterCode("0402FE07A67E7CB3C6661C89C5873DA1BBEF4EBC8A6552B48C439FC8F6EF784264256136139729863B65CE84452B1665D304F7CD2D4C2AAEA499E3972ACC7D21F0C6260C7322448AB9B568103F9B59B561F817E6772E3FE9A73490208FA1163E46");


            //编辑器ID
            engine.Options.ControlName = "myWriterControl";

            //违禁关键字
            engine.Options.ExcludeKeywords = "月经,子宫,卵巢";
            // 是否显示输入域状态标签
            engine.DocumentOptions.ViewOptions.ShowInputFieldStateTag = true;
            // 文本输入域数据异常时的高亮度文本色            
            engine.DocumentOptions.ViewOptions.FieldInvalidateValueBackColor = Color.Yellow;
            // 用Tab键在各个输入域之间切换
            engine.DocumentOptions.BehaviorOptions.MoveFocusHotKey = MoveFocusHotKeys.Tab;
            //表单视图模式 Strict:严格视图模式
            engine.ControlOptions.FormView = FormViewMode.Strict;
            //显示段落标记
            engine.DocumentOptions.ViewOptions.ShowParagraphFlag = false;
            //是否显示输入域状态标签
            engine.DocumentOptions.ViewOptions.ShowInputFieldStateTag = true;
            //是否允许视图加密显示
            engine.DocumentOptions.ViewOptions.EnableEncryptView = true;
            engine.DocumentOptions.BehaviorOptions.EnableDataBinding = false;
            // 是否显示无边框的表格暗线
            engine.DocumentOptions.ViewOptions.ShowCellNoneBorder = false;
            //输入域边框颜色设置
            engine.DocumentOptions.ViewOptions.FieldBorderColor = Color.Black;
            //输入域背景文字的输出模式 Underline：输出下划线
            engine.Options.BackgroundTextOutputMode = DCBackgroundTextOutputMode.Underline;
            //内容呈现模式 NormalHtmlEditable：以普通的HTML模式显示可编辑的文档
            engine.Options.ContentRenderMode = WebWriterControlRenderMode.NormalHtmlEditable;
            //引用路径
            engine.Options.ServicePageURL = this.Url.Action("HandleDCWriterServicePage");
            //工作区域背景色
            engine.Options.WorkspaceBackColorString = "#B1CAEB";
            //工作区域背景图片
            //engine.Options.WorkspaceBackgroundImage = "Workspace-Background.jpg";
            //边框样式
            engine.BorderStyle = "0px solid black";
            return engine;
        }

        //设置编辑器的引用路径
        public ActionResult HandleDCWriterServicePage()
        {
            var url = new DCWriterActionResult();
            return url;
        }

        //处理操作结果生成一个带文档内容的编辑器
        private class DCWriterActionResult : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                //在web中显示文档内容的WEB控件
                DCSoft.Writer.Controls.WebWriterControl.HandleService();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blid"></param>
        /// <param name="bllx"></param>
        /// <param name="isLock">1请求编辑病历2请求解锁病历</param>
        /// <returns></returns>
        public ActionResult LockRecord(string blid, string bllx, int isToLock)
        {
            var MedicalRecord = _RecordDmnService.GetMedicalRecord(blid, bllx, OrganizeId);
            if (isToLock == 1)
            {
                if (MedicalRecord.IsLock != 1)
                {
                    _RecordDmnService.LockRecord(blid, bllx, OrganizeId, this.UserIdentity.UserCode, isToLock);
                }
            }
            else
            {
                if (MedicalRecord.IsLock == 1)
                {
                    if (MedicalRecord.LastModifierCode == this.UserIdentity.UserCode)
                    {
                        _RecordDmnService.LockRecord(blid, bllx, OrganizeId, this.UserIdentity.UserCode, isToLock);
                        return Success("解锁成功");
                    }
                    else
                    {
                        throw new FailedException("解锁失败，请" + MedicalRecord.LastModifierCode + "解锁");
                    }
                }
                else
                {
                    return Success("尚未锁定");
                }

            }
            return Success("解锁成功");
        }

        private WebWriterControlEngine XMLDataBind<T>(T data, WebWriterControlEngine eng)
        {
            string dataBind = eng.Document.GetDataSourceBindingDescriptionsXML();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(dataBind);


            DataSet xmlDS = new DataSet();
            var stream = new StringReader(dataBind);
            var reader = new XmlTextReader(stream);
            xmlDS.ReadXml(reader);


            if (data != null)
            {
                PropertyInfo[] properties = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                if (properties.Length <= 0)
                {
                    return eng;
                }


                if (xmlDS != null && xmlDS.Tables.Count > 0)
                {
                    foreach (DataRow item in xmlDS.Tables[0].Rows)
                    {
                        var ElementID = item["ElementID"].ToString();
                        var BindingPath = item["BindingPath"].ToString();
                        var DataSource = item["DataSource"].ToString();
                        var bdp = BindingPath.ToString().Split('/');

                        var eledata = properties.Where(p => p.Name == bdp[1]).FirstOrDefault();
                        if (eledata != null)
                        {
                            object value = eledata.GetValue(data, null);
                            if (value != null)
                            {
                                XTextElement ele = eng.Document.GetElementById(ElementID);
                                ele.InnerText = value.ToString();
                            }
                            //else if (BindingPath.Contains("ba_basy"))
                            //{
                            //    XTextElement ele = eng.Document.GetElementById(ElementID);
                            //    ele.InnerText = "[-]";
                            //}
                        }
                        if (eledata == null)
                        {
                            eledata = properties.Where(p => p.Name == ElementID).FirstOrDefault();
                            if (eledata != null)
                            {
                                object value = eledata.GetValue(data, null);
                                if (value != null)
                                {
                                    XTextElement ele = eng.Document.GetElementById(ElementID);
                                    ele.InnerText = value.ToString();
                                }
                            }
                        }

                    }
                }

                return eng;
            }

            return eng;
        }

        public ActionResult updateRecordStu(string Id,string blzt) {
            var LastModifierCode = UserIdentity.UserName;
            var LastModifyTime = DateTime.Now;
            _RecordDmnService.updateRecordStu(Id,OrganizeId, LastModifierCode, LastModifyTime,blzt);
            return Success("操作成功。");
        }




        private void EMRBasyPrintReportCom()
        {
            ViewBag.OrgId = this.OrganizeId;
            ViewBag.topOrgId = Constants.TopOrganizeId;
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            ViewBag.IsHospAdministrator = this.UserIdentity.IsHospAdministrator.ToString().ToLower();  //是否是医院管理员
            ViewBag.CurUserCode = this.UserIdentity.UserCode;
            ViewBag.curUsergh = UserIdentity.rygh;
        }








    }
}

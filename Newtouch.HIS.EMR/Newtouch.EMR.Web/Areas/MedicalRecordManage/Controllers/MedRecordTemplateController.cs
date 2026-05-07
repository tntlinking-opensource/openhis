using FrameworkBase.MultiOrg.Web;
using Newtouch.Core.Common;
using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.IDomainServices;
using Newtouch.EMR.Domain.IRepository;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtouch.EMR.Infrastructure;
using Newtouch.Core.Common.Utils;
using Newtouch.Infrastructure;
using Newtouch.Common.Operator;
using DCSoft.Writer.Controls;
using DCSoft.Writer;
using System.IO;
using Newtouch.EMR.Domain.ValueObjects;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;

namespace Newtouch.EMR.Web.Areas.MedicalRecordManage
{
    public class MedRecordTemplateController : OrgControllerBase
    {
    
        private readonly IBlmblbRepo _BlmblbRepo;
        private readonly IZybrjbxxDmnService _ZybrjbxxDmnService;
        private readonly ISysDepartmentRepo _SysDepartmentRepo;
        private readonly ISysStaffRepo _SysStaffRepo;
        private readonly IBlmblbDmnService _BlmblbDmnService;
        private readonly ICommonDmnService _CommonDmnService;
        public static string BlTemplatePath = ConfigurationHelper.GetAppConfigValue("BlTemplatePath");

        #region View
        public ActionResult FormQX()
        {
            return View();
        }
        //public ActionResult MedicalTemplate()
        //{
        //    return View();
        //}
        public ActionResult LeftMedicalTemplate()
        {
            return View();
        }
        public ActionResult RightMedicalTemplate()
        {
            return View();
        }
        public ActionResult BlTemplate()
        {
            return View();
        }
        public ActionResult testbl()
        {
            return View();
        }
        //openVer
        public ActionResult MedicalOpenEditor()
        {
            return View();
        }
        #endregion

        #region 病历模板管理
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            OperatorModel user = this.UserIdentity;
            var data = new
            {
                rows = _BlmblbDmnService.GetBlmbList(pagination, this.OrganizeId, keyword),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 读取病历模板
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="blId"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue, string blId,string bllx)
        {
            if (!string.IsNullOrWhiteSpace(blId)) //保存个人模板
            {
                var mod = _BlmblbDmnService.BlConvertToTemplate(OrganizeId, blId, bllx, 1);
                var bllxDic = _CommonDmnService.GetBllxListDetail(OrganizeId, bllx).FirstOrDefault();
                BlmblbEntity data = new BlmblbEntity();
                data.bllxId = bllxDic.Id;
                data.bllx = mod.bllx;
                data.bllxmc = bllxDic.bllxmc;
                data.Isempty = mod.Isempty;
                data.mbqx = mod.mbqx;
                data.Memo = mod.blxtml + mod.blxtmc;
                data.zt = "true";
                data.mblj= ConfigurationHelper.GetAppConfigValue("BlTemplatePath");
                data.LoadWay = Convert.ToInt32(EnummbqxTempLoadWay.DcWriter);
                if (data.mbqx == Convert.ToInt32( Enummbqx.prv))
                {
                    data.ysgh = this.UserIdentity.rygh;
                }
                return Content(data.ToJson());
            }
            else
            {
                var data = _BlmblbRepo.FindEntity(keyValue);
                return Content(data.ToJson());
            }
        }
        /// <summary>
        /// 修改/新增模板
        /// </summary>
        /// <param name="blmb"></param>
        /// <param name="keyValue"></param>
        /// <param name="mbly">模板来源</param>
        /// <returns></returns>
        public ActionResult submitForm(BlmblbEntity blmb,string keyValue,int? mbly)
        {
            blmb.OrganizeId = this.OrganizeId;
            if (blmb.Isempty == null)
            {
                blmb.Isempty = 0;
            }
            else
            {
                blmb.Isempty = 1;
            }
            if (string.IsNullOrWhiteSpace(blmb.IsYB) || blmb.IsYB == "false")
            {
                blmb.IsYB = "0";
            }
            if (string.IsNullOrWhiteSpace(blmb.EnableDataLoad) || blmb.EnableDataLoad == "false")
            {
                blmb.EnableDataLoad = "0";
            }
            if (mbly != null && mbly == 0)
            {
                blmb.mblj = blmb.mblj + blmb.mbmc+".xml";
            }
            var bllxDic = _CommonDmnService.GetBllxListDetail( OrganizeId,blmb.bllx).FirstOrDefault();
            blmb.bllxId = bllxDic.Id;
            blmb.bllxmc = bllxDic.bllxmc;


            _BlmblbRepo.SubmitForm(blmb, keyValue);

            if (mbly != null && mbly == 0)  //病历转模板标志，文件迁移操作
            {
                //文件迁移
                if (!string.IsNullOrWhiteSpace(blmb.Memo))
                {
                    blmb.mblj = Server.MapPath(blmb.mblj);
                    blmb.Memo = Server.MapPath(blmb.Memo);
                    _BlmblbDmnService.BlConvertToTemplateProcess(blmb);
                }
            }
            return Success();
        }

        public ActionResult DeleteForm(string keyValue)
        {
            var ety = _BlmblbRepo.FindEntity(keyValue);
            ety.zt = "0";
            _BlmblbRepo.SubmitForm(ety, keyValue);
            return Success();
            ////文件迁移
            //var delpath = Server.MapPath(ConfigurationHelper.GetAppConfigValue("BlTemplateDelPath"));
            //var cutpath = Server.MapPath(ety.mblj);
            //var flag = _BlmblbDmnService.CopyDir(cutpath, delpath, "",1);
            //if (flag == true)
            //{
            //    return Success();
            //}
            //else
            //    return Error("删除模板异常，请联系管理员");
        }

        public ActionResult GetSysDept()
        {
            var list = _SysDepartmentRepo.GetList(OrganizeId, "1", null);
            var data = new List<object>();
            foreach (var item in list)
            {
                var obj = new
                {
                    id = item.Code,
                    text = item.Name
                };
                data.Add(obj);
            }

            return Content(data.ToJson());
        }

        public ActionResult GetSysStaff()
        {
            var data = _SysStaffRepo.GetValidStaffListByOrganizeId(OrganizeId);
            return Content(data.ToJson());
        }
        #endregion

        #region 权限控制
        public ActionResult GetblmbDuty(string mbId)
        {
            OperatorModel user = this.UserIdentity;
            var data = _CommonDmnService.GetblmbDuty(mbId,user);
            return Content(data.ToJson());
        }
        
        public ActionResult SubmitAssigned(string list,string mbId,string mbbm)
        {
            OperatorModel user = this.UserIdentity;
           // submitForm(mbInfo, mbId, mbly);

            //新增时根据编码获取mbId 
            if (string.IsNullOrWhiteSpace(mbId))
            {
                mbId = _BlmblbRepo.FindEntity(p => p.mbbm == mbbm && p.OrganizeId == OrganizeId && p.zt == "1").Id;
            }
            if (!string.IsNullOrWhiteSpace(mbId))
            {
                _BlmblbDmnService.TempCtrlAssigned(list, mbId, user);
            }
            return Success();
        }
        #endregion

        public ActionResult TemplateEdit()
        {
            //根据id获取模板
            //如果找不到加载的xml就默认为空白的xml
            string currentFileName = Server.MapPath(BlTemplatePath + "newFile.xml");
            // 加载文件
            WebWriterControlEngine eng = GetControlEngine();
            if (this.Request.HttpMethod != "GET")
            {
                string sd = this.Request.Form["saveDocument"];
                bool loaded = false;
                if (sd == "保存文档" && eng.Options.ContentRenderMode == WebWriterControlRenderMode.NormalHtmlEditable)
                {
                    // 用户点击了“保存文档”按钮，
                    // 则试图从WEB请求中加载文档然后保存
                    loaded = eng.LoadDocumentFromRequestFormData();
                    if (loaded)
                    {
                        string path = BlTemplatePath + DateTime.Now.ToString("yyyy/MM/dd/");
                        IsExistDir(path);
                        string BLMC = DateTime.Now.ToString("ddHHmm");

                        currentFileName = Server.MapPath(path)+ BLMC + ".xml";
                        eng.SaveDocument(currentFileName , null);              
                    }
                }
            }
            eng.LoadDocument(currentFileName, null);
            ViewBag.WriterControlHtml = eng.GetAllContentHtml();
            eng.Dispose();
            return View();
        }
        private WebWriterControlEngine GetControlEngine()
        {
            WebWriterControlEngine engine = new WebWriterControlEngine();
            //编辑器ID
            engine.Options.ClientID = "myWriterControl";
            //违禁关键字
            engine.Options.ExcludeKeywords = "月经,子宫,卵巢";
            // 是否显示输入域状态标签
            engine.DocumentOptions.ViewOptions.ShowInputFieldStateTag = true;
            // 文本输入域数据异常时的高亮度文本色            engine.DocumentOptions.ViewOptions.FieldInvalidateValueBackColor = Color.Yellow;
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

            //输入域背景文字的输出模式 Underline：输出下划线
            engine.Options.BackgroundTextOutputMode = DCBackgroundTextOutputMode.Underline;
            //内容呈现模式 NormalHtmlEditable：以普通的HTML模式显示可编辑的文档
            engine.Options.ContentRenderMode = WebWriterControlRenderMode.NormalHtmlEditable;
            //引用路径
            engine.Options.ServicePageURL = this.Url.Action("HandelDCWriterServicePage");
            //工作区域背景色
            engine.Options.WorkspaceBackColorString = "#B1CAEB";
            //工作区域背景图片
            engine.Options.WorkspaceBackgroundImage = "Workspace-Background.jpg";
            //边框样式
            engine.BorderStyle = "1px solid black";
            return engine;
        }
        //设置编辑器的引用路径
        public ActionResult HandelDCWriterServicePage()
        {
            return new DCWriterActionResult();
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
        public void IsExistDir(string path)
        {
            if (Directory.Exists(Server.MapPath(path)) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(Server.MapPath(path));
            }
        }

        #region  dc版
        [ValidateInput(false)]
        public ActionResult MoreHandleDCWriterServicePage()
        {
            return new MoreActionResult();
        }

        public ActionResult MedicalTemplate()
        {
            //DCwriterServer dcControl = new DCwriterServer();
            //dcControl.MedicalTemplate();
            //WebWriterControlEngine eng = dcControl.DCWControl;//这是上一步创建的引擎 
            //eng.Options.ClientContextMenuType = WebClientContextMenuType.Custom;
            //ViewBag.WriterControlHtml = eng.GetAllContentHtml();
            //eng.Dispose();
            return View();
        }
        private class MoreActionResult : ActionResult
        {
            public MoreActionResult()
            {
            }
            public override void ExecuteResult(ControllerContext context)
            {
                WebWriterControlServicePageExecuter executer = new WebWriterControlServicePageExecuter();
                executer.CrossDomainSettings.Enabled = true;//是否跨域
                executer.EventSaveFileContent = new DCSoft.Writer.WriterSaveFileContentEventHandler(My_SaveFileContent);//事件
                executer.EventReadFileContent = new DCSoft.Writer.WriterReadFileContentEventHandler(My_ReadFileContent);
                if (executer.HandleService())
                {
                    return;
                }
            }

            private void My_ReadFileContent(object eventSender, WriterReadFileContentEventArgs args)
            {
                if (!string.IsNullOrWhiteSpace(args.FileName))
                {
                    string [] arr = args.FileName.Split('\\');
                    string filename = arr.LastOrDefault();
                    string ml = AppDomain.CurrentDomain.BaseDirectory;
                    var tempaleUrl = "";
                    for (var item=1;item<arr.Length; item++)
                    {
                        if (arr[item].Contains(".xml"))
                        {
                            var xmlfile = ml + tempaleUrl + arr[item];
                            var t = System.IO.File.Exists((xmlfile));
                            if (!System.IO.File.Exists((xmlfile)))
                            {
                                //var defalutxml = "<XTextDocumentxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"EditorVersionString=\"1.2023.2.14\"></XTextDocument>";
                                //System.IO.Directory.CreateDirectory(ml + tempaleUrl);
                                var d = $"{ml}{tempaleUrl}{filename}";
                                System.IO.File.WriteAllText($"{ml}{tempaleUrl}{filename}", "");
                                //args.Handled = true;
                                //return;
                            }
                            tempaleUrl += arr[item];
                        }
                        else
                        {
                            var d = arr[item];
                            var dd = ml + tempaleUrl + arr[item];
                            if (!System.IO.Directory.Exists(ml + tempaleUrl + arr[item]))
                            {
                                System.IO.Directory.CreateDirectory(ml + tempaleUrl + arr[item]);
                            }
                            tempaleUrl +=arr[item]+ "\\" ;
                            
                        }
                    }
                    //string filename = args.FileName + ".xml";
                    string xml = System.IO.File.ReadAllText($"{ml}{tempaleUrl}");

                    byte[] bs = System.IO.File.ReadAllBytes($"{ml}{tempaleUrl}");
                    args.ResultBinary = bs;
                    //args.Message = "加载以后的返回信息";//传递给前端的信息 
                    args.Handled = true;
                }

            }

            private void My_SaveFileContent(object sender, DCSoft.Writer.WriterSaveFileContentEventArgs args)
            {
                string filename = args.FileName;
                string ml = AppDomain.CurrentDomain.BaseDirectory;
                ml = ml.Substring(0, ml.Length-1);
                if (!System.IO.Directory.Exists(ml + "File"))
                {
                    System.IO.Directory.CreateDirectory(ml + "File");
                }
                var d= $"{ml}{filename}";
                System.IO.File.WriteAllText($"{ml}{filename}", args.Document.InnerXML);
            }

        }

        #region  dc5.0实现方式
        public ActionResult GetTemplateXml(string templateUrl)
        {
            string[] arr = templateUrl.Split('\\');
            string filename = arr.LastOrDefault();
            string ml = AppDomain.CurrentDomain.BaseDirectory;
            var tempaleUrl = "";
            for (var item = 1; item < arr.Length; item++)
            {
                if (arr[item].Contains(".xml"))
                {
                    var xmlfile = ml + tempaleUrl + arr[item];
                    var t = System.IO.File.Exists((xmlfile));
                    if (!System.IO.File.Exists((xmlfile)))
                    {
                        //var defalutxml = "<XTextDocumentxmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"EditorVersionString=\"1.2023.2.14\"></XTextDocument>";
                        //System.IO.Directory.CreateDirectory(ml + tempaleUrl);
                        var d = $"{ml}{tempaleUrl}{filename}";
                        System.IO.File.WriteAllText($"{ml}{tempaleUrl}{filename}", "");
                        //args.Handled = true;
                        //return;
                    }
                    tempaleUrl += arr[item];
                }
                else
                {
                    var d = arr[item];
                    var dd = ml + tempaleUrl + arr[item];
                    if (!System.IO.Directory.Exists(ml + tempaleUrl + arr[item]))
                    {
                        System.IO.Directory.CreateDirectory(ml + tempaleUrl + arr[item]);
                    }
                    tempaleUrl += arr[item] + "\\";

                }
            }
            //string filename = args.FileName + ".xml";
            string xml = System.IO.File.ReadAllText($"{ml}{tempaleUrl}");
            BlTemplateVo vo = new BlTemplateVo();
            vo.xmldata = xml;
            return Content(vo.ToJson());
            #region
            //string filename = templateName+".xml";
            //string ml = AppDomain.CurrentDomain.BaseDirectory;
            //if (!System.IO.Directory.Exists(ml + "File"))
            //{
            //    System.IO.Directory.CreateDirectory(ml + "File");
            //}
            //if (!System.IO.Directory.Exists(ml + "File\\YzContinueTem"))
            //{
            //    System.IO.Directory.CreateDirectory(ml + "File\\YzContinueTem");
            //}
            ////var d = ml + "File\\YzContinueTem\\" + filename;
            ////var dd = System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(d));
            //if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(ml + "File\\YzContinueTem\\"+ filename)))
            //{
            //    System.IO.Directory.CreateDirectory(ml + "File\\YzContinueTem\\" + filename);
            //    return null;
            //}
            //BlTemplateVo vo = new BlTemplateVo();
            //string xml = System.IO.File.ReadAllText($"{ml}File\\YzContinueTem\\{filename}");
            //vo.xmldata = xml;
            //return  Content(vo.ToJson()); 
            #endregion
        }

        public ActionResult SaveTemplateXml(string templateUrl,string templateData)
        {
            string filename = templateUrl;
            byte[] decodedBytes = Convert.FromBase64String(templateData);
            string xmlString = Encoding.UTF8.GetString(decodedBytes);
            string ml = AppDomain.CurrentDomain.BaseDirectory;
            ml = ml.Substring(0, ml.Length - 1);
            if (!System.IO.Directory.Exists(ml + "File"))
            {
                System.IO.Directory.CreateDirectory(ml + "File");
            }
            var d = $"{ml}{filename}";
            System.IO.File.WriteAllText($"{ml}{filename}", xmlString);
            return Success();
        }
        #endregion

        #endregion

        #region openeditor
        public ActionResult GetTemplateHtml(string templateUrl)
        {
            templateUrl = templateUrl.Replace("xml","html");
            string[] arr = templateUrl.Split('\\');
            string filename = arr.LastOrDefault();
            string ml = AppDomain.CurrentDomain.BaseDirectory;
            var tempaleUrl = "";
            for (var item = 1; item < arr.Length; item++)
            {
                if (arr[item].Contains(".html"))
                {
                    var xmlfile = ml + tempaleUrl + arr[item];
                    var t = System.IO.File.Exists((xmlfile));
                    if (!System.IO.File.Exists((xmlfile)))
                    {
                        var d = $"{ml}{tempaleUrl}{filename}";
                        System.IO.File.WriteAllText($"{ml}{tempaleUrl}{filename}", "<body data-hm-papersize=\"A4_portrait$top:5mm#right:5mm#bottom:5mm#left:5mm\" meta_json=\"\" style=\"\" class=\"\"><table class=\"dashed-border font-normal\" style=\"width: 100%; table-layout: auto;\" data-hm-table=\"true\" hm-table-id=\"ef00d3b996d222f2e8e56ef5e19a40486\" _paperheader=\"true\"><colgroup><col style=\"width: 17.1897%;\"><col style=\"width: 37.9707%;\"><col style=\"width: 19.8396%;\"><col style=\"width: 25%;\"></colgroup><tbody><tr><td style=\"width: 100%; text-align: center;\" colspan=\"4\"><p><br></p></td></tr><tr><td style=\"width: 100%; text-align: center;\" colspan=\"4\">页眉</td></tr><tr><td style=\"width: 17.1897%; border-bottom: 1px solid black;\"><br></td><td style=\"width: 37.9707%; border-bottom: 1px solid black;\"><br></td><td style=\"width: 19.8396%; border-bottom: 1px solid black;\"><br></td><td style=\"width: 25%; border-bottom: 1px solid black;\"><br></td></tr></tbody></table><p><br><span data-cke-bookmark=\"1\" style=\"display: none;\">&nbsp;</span>　　</p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><p><br></p><table class=\"dashed-border font-normal\" style=\"width: 100%; table-layout: auto;\" data-hm-table=\"true\" hm-table-id=\"e5765dc780bff98a90954d12e022e605d\" _paperfooter=\"true\"><colgroup><col style=\"width: 100%;\"></colgroup><tbody><tr><td style=\"width: 100%; text-align: center; border-top-width: 1px; border-top-style: solid;\">第<span contenteditable=\"false\" class=\"page\"> </span>页</td></tr></tbody></table></body>");
                    }
                    tempaleUrl += arr[item];
                }
                else
                {
                    if (!System.IO.Directory.Exists(ml + tempaleUrl + arr[item]))
                    {
                        System.IO.Directory.CreateDirectory(ml + tempaleUrl + arr[item]);
                    }
                    tempaleUrl += arr[item] + "\\";

                }
            }
            string xml = System.IO.File.ReadAllText($"{ml}{tempaleUrl}");
            BlTemplateVo vo = new BlTemplateVo();
            vo.xmldata = xml;
            return Content(vo.ToJson());
        }
        public ActionResult SaveTemplateHtml(string templateUrl, string templateData)
        {
            string filename = templateUrl.Replace("xml", "html");
            byte[] decodedBytes = Convert.FromBase64String(templateData);
            string xmlString = Encoding.UTF8.GetString(decodedBytes);
            string ml = AppDomain.CurrentDomain.BaseDirectory;
            ml = ml.Substring(0, ml.Length - 1);
            if (!System.IO.Directory.Exists(ml + "File"))
            {
                System.IO.Directory.CreateDirectory(ml + "File");
            }
            var d = $"{ml}{filename}";
            System.IO.File.WriteAllText($"{ml}{filename}", xmlString);
            return Success();
        }
        #endregion
    }
}
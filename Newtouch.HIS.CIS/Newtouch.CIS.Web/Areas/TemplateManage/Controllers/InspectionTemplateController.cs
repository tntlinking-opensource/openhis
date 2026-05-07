using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.CIS.Web.Areas.TemplateManage.Controllers
{
    public class InspectionTemplateController : OrgControllerBase
    {
        private readonly IInspectionTemplateRepo _inspectionTemplateRepo;
        private readonly IInspectionCategoryRepo _inspectionCategoryRepo;
        private readonly IInspectionTemplateDmnService _inspectionTemplateDmnService;
        // GET: TemplateManage/Inspection
        
        /// <summary>
        /// 检验检查大类 下拉
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCategoryList()
        {
            var data = _inspectionCategoryRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.zt == "1").ToList();
            return Content(data.ToJson());
        }
        
        /// <summary>
        /// 模板列表（查询）
        /// </summary>
        /// <param name="zutaoType"></param>
        /// <returns></returns>
        public ActionResult SelectGridList(int type)
        {
            var data = _inspectionTemplateRepo.GetList(type, this.OrganizeId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 模板详情
        /// </summary>
        /// <param name="mbId"></param>
        /// <returns></returns>
        public ActionResult GetTemplateDetail(string mbId)
        {
            var data = _inspectionTemplateDmnService.GetTemplateDetail(mbId, this.OrganizeId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ztobj"></param>
        /// <param name="ztxmlist"></param>
        /// <returns></returns>
        public ActionResult SaveData(InspectionTemplateEntity mbobj, List<TemplateGroupPackageEntity> mbztlist)
        {
            mbobj.zt = mbobj.zt == "on" ? "1" : "0";
            //模板表
            mbobj.OrganizeId = this.OrganizeId;

            _inspectionTemplateDmnService.SaveData(mbobj, mbztlist);
            return Success();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="mbId"></param>
        /// <returns></returns>
        public ActionResult DeleteData(string mbId)
        {
            _inspectionTemplateDmnService.DeleteData(mbId);
            return Success();
        }

        /// <summary>
        /// 处方页的检验检查模板树
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTreeList(int jyjcmbLx,string TemplateId, string ztKeyword)
        {
            var treeList = new List<TreeViewModel>();
            var alldldata = _inspectionTemplateDmnService.GetTemplateListByType(this.OrganizeId, jyjcmbLx);
            var dldata = alldldata.Select(a => new { a.dlmc }).Distinct().ToList();
            foreach (var item in dldata)
            {
                //检验检查大类
                treeList.Add(new TreeViewModel()
                {
                    id = item.dlmc,
                    value = item.dlmc,
                    text = item.dlmc,
                    parentId = null,
                    hasChildren = true,
                    isexpand = true,
                });

				var mbdata= string.IsNullOrWhiteSpace(TemplateId) == true? alldldata.Where(a => a.dlmc == item.dlmc).Select(a => new { a.mbId, a.mbmc }).ToList() : alldldata.Where(a => a.dlmc == item.dlmc && a.mbId == TemplateId).Select(a => new { a.mbId, a.mbmc }).ToList(); ;
				
				foreach (var mb in mbdata)
                {
                    //模板
                    var ztdata = _inspectionTemplateDmnService.GetTemplateDetailByMbId(this.OrganizeId, mb.mbId, ztKeyword).Select(a => new { a.ztId, a.ztmc }).Distinct();
                    //bool hasChildren = ztdata.Count() > 0 ? true : false;
                    bool hasChildren = true;
                    if (ztdata.Count() > 0)
                    {
                        treeList.Add(new TreeViewModel()
                        {
                            id = mb.mbId,
                            value = mb.mbId,
                            text = mb.mbmc,
                            parentId = item.dlmc,
                            hasChildren = hasChildren,
                            isexpand = hasChildren,
                            complete = true
                        });
                    }
                    //组套
                    foreach (var zt in ztdata)
                    {
                        treeList.Add(new TreeViewModel()
                        {
                            id = zt.ztId,
                            value = zt.ztId,
                            text = zt.ztmc,
                            parentId = mb.mbId,
                            hasChildren = false,
                            isexpand = false,
                            complete = true
                        });
                    }
                }
            }
            return Content(treeList.TreeViewJson(null));
        }

        /// <summary>
        /// 根据ztId查询组套下的收费项目
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGPackageDetailByZtId(string ztId,string mbId=null)
        {
            var sfxmdata = _inspectionTemplateDmnService.GetGPackageDetailByZtId(this.OrganizeId, ztId,mbId);
            return Content(sfxmdata.ToJson());
        }
        public ActionResult GetGPackageDetailByZtIdArray(string[] ztId,string mbId=null)
        {
            string ztIds = "";
            if (ztId != null && ztId.Length > 0)
            {
                ztIds=String.Join(",", ztId);
            }
            var sfxmdata = _inspectionTemplateDmnService.GetGPackageDetailByZtId(this.OrganizeId, ztIds,mbId);
            return Content(sfxmdata.ToJson());
        }
        public ActionResult GetGPackageInfoByZtId(string ztId,string mbId)
        {
            var sfxmdata = _inspectionTemplateDmnService.GetGPackageInfoByZtId(this.OrganizeId, ztId,mbId);
            return Content(sfxmdata.ToJson());
        }

        public ActionResult GetMbList(int jyjcmbLx)
		{
			var alldldata = _inspectionTemplateDmnService.GetTemplateListByType(this.OrganizeId, jyjcmbLx);
			var mbdata = alldldata.Select(a => new { a.mbId, a.mbmc }).ToList();
			return Content(mbdata.ToJson());
		}

	}
}
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.CIS.Web.Areas.TemplateManage.Controllers
{
    public class PresTemplateController : OrgControllerBase
    {
        // GET: TemplateManage/PresTemplate

        private readonly IPresTemplateRepo _presTemplateRepo;
        private readonly IPresTemplateDetailRepo _presTemplateDetailRepo;
        private readonly IPresTemplateDmnService _presTemplateDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IGroupPackageRepo _groupPackageRepo;
        /// <summary>
        /// 康复
        /// </summary>
        /// <returns></returns>
        public ActionResult RehabForm()
        {
            return View();
        }

        /// <summary>
        /// 常规项目处方
        /// </summary>
        /// <returns></returns>
        public ActionResult RegularItemForm()
        {
            return View();
        }

        /// <summary>
        /// 西药
        /// </summary>
        /// <returns></returns>
        public ActionResult WMForm()
        {
            return View();
        }

        /// <summary>
        /// 中药
        /// </summary>
        /// <returns></returns>
        public ActionResult TCMForm()
        {
            return View();
        }

        /// <summary>
        /// 处方模板树
        /// </summary>
        /// <param name="mblx"></param>
        /// <param name="cflx"></param>
        /// <returns></returns>
        public ActionResult GetTreeList(int mblx, int cflx, int? expandCflx,string mbKeyword=null)
        {
            var treeList = new List<TreeViewModel>();

            if (cflx == 0)
            {
                if (_sysConfigRepo.GetBoolValueByCode("openKfcf", this.OrganizeId) == true)
                {
                    treeList.AddRange(GetStaticTreeList(mblx, (int)EnumCflx.RehabPres, expandCflx, mbKeyword));
                }
                if (_sysConfigRepo.GetBoolValueByCode("openCgxmcf", this.OrganizeId) == true)
                {
                    treeList.AddRange(GetStaticTreeList(mblx, (int)EnumCflx.RegularItemPres, expandCflx,mbKeyword));
                }
                treeList.AddRange(GetStaticTreeList(mblx, (int)EnumCflx.WMPres, expandCflx, mbKeyword));
                treeList.AddRange(GetStaticTreeList(mblx, (int)EnumCflx.TCMPres, expandCflx, mbKeyword));
            }
            else
            {
                treeList.AddRange(GetStaticTreeList(mblx, cflx, expandCflx,mbKeyword));
            }


            return Content(treeList.TreeViewJson(null));
        }
        
        
        
        /// <summary>
        /// 获取处方模板
        /// </summary>
        /// <param name="mblx"></param>
        /// <param name="cflx"></param>
        /// <param name="expandCflx"></param>
        /// <param name="mbKeyword"></param>
        /// <returns></returns>
        public ActionResult GetCfmbList(int mblx, int cflx, int? expandCflx,string mbKeyword=null)
        {
            switch (cflx) {
                case (int)EnumCflx.WMPres:
                case (int)EnumCflx.TCMPres:
                case (int)EnumCflx.RehabPres:
                case (int)EnumCflx.RegularItemPres:
                    //var data = _presTemplateDmnService.SelectCfTemplateList(cflx,mblx,this.OrganizeId, this.UserIdentity.DepartmentCode, this.UserIdentity.rygh,mbKeyword);
                    var data = _presTemplateRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.mblx == mblx && (a.cflx == cflx || cflx == 0) && a.zt == "1" && a.mbmc.Contains(mbKeyword) && (mblx == 1 ? a.ysgh == this.UserIdentity.rygh : (mblx == 2 ? a.ksCode == this.UserIdentity.DepartmentCode : 1 == 1))).Select(a => new { a.mbmc, a.cflx, a.mbId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
                    return Content(data.ToJson());
                case (int)EnumCflx.InspectionPres:
                case (int)EnumCflx.ExaminationPres:
                    int type = cflx == (int)EnumCflx.InspectionPres ? 1 : 2;
                    var jyjcdata = _groupPackageRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.zt == "1" && a.Type == type && a.ztmc.Contains(mbKeyword)).Select(a => new { mbmc = a.ztmc, cflx = (a.Type == 1 ? (int)EnumCflx.InspectionPres : (int)EnumCflx.ExaminationPres), mbId = a.ztId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
                    return Content(jyjcdata.ToJson());
                default:
                    var data1 = _presTemplateRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.mblx == mblx && (a.cflx == cflx || cflx == 0) && a.zt == "1" && a.mbmc.Contains(mbKeyword) && (mblx == 1 ? a.ysgh == this.UserIdentity.rygh : (mblx == 2 ? a.ksCode == this.UserIdentity.DepartmentCode : 1 == 1))).Select(a => new { a.mbmc, a.cflx, a.mbId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
                    var jyjcdata1 = _groupPackageRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.zt == "1" && a.ztmc.Contains(mbKeyword)).Select(a => new { mbmc = a.ztmc, cflx = (a.Type == 1 ? (int)EnumCflx.InspectionPres : (int)EnumCflx.ExaminationPres), mbId = a.ztId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
                    data1 = data1.Concat(jyjcdata1).ToList();
                    return Content(data1.ToJson());
            }
            
            //if ((cflx == (int)EnumCflx.InspectionPres || cflx == (int)EnumCflx.ExaminationPres) && cflx!=0)
            //{
            //    int type = cflx == (int)EnumCflx.InspectionPres ? (int)EnumCflx.InspectionPres : (int)EnumCflx.ExaminationPres;
            //    var jyjcdata = _groupPackageRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.zt == "1" && a.Type == type && a.ztmc.Contains(mbKeyword)).Select(a => new { mbmc = a.ztmc, cflx=(a.Type==1?(int)EnumCflx.InspectionPres: (int)EnumCflx.ExaminationPres), mbId=a.ztId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
            //    data = data.Concat(jyjcdata).ToList();
            //}
            //else {
            //    var jyjcdata = _groupPackageRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.zt == "1" && a.ztmc.Contains(mbKeyword)).Select(a => new { mbmc = a.ztmc, cflx = (a.Type == 1 ? (int)EnumCflx.InspectionPres : (int)EnumCflx.ExaminationPres), mbId = a.ztId, a.CreateTime, a.LastModifyTime }).OrderBy(a => a.cflx).ThenByDescending(a => a.CreateTime).ToList();
            //    data = data.Concat(jyjcdata).ToList();
            //}
           
            //return Content(data.ToJson());
        }

        /// <summary>
        /// 处方模板 明细列表
        /// </summary>
        /// <param name="mbId"></param>
        /// <returns></returns>
        public ActionResult PresTemplateDetail()
        {
            //医保控制code
            ViewBag.ControlbrxzCode = _sysConfigRepo.GetValueByCode("ControlbrxzCode", OrganizeId);
            return View();
        }

        /// <summary>
        /// 查询模板明细 json
        /// </summary>
        /// <param name="mbId"></param>
        /// <param name="mxIdStr"></param>
        /// <returns></returns>
        public ActionResult SelectPresTemplateDetailByMbId(string mbId,  string mxIdStr)
        {
            var data = _presTemplateDmnService.SelectPresDetailByMbId(mbId, this.OrganizeId);
            return Content(data.ToJson());
        }

        public ActionResult Delete(string mbId)
        {
            try
            {
                _presTemplateDmnService.DeleteTemplate(mbId, OrganizeId);

                return Success();
            }
            catch (System.Exception e)
            {

                return Error("删除失败");
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveData(PresTemplateEntity mbObj, List<PresTemplateDetailmxVo> mxList)
        {
            if (mbObj.mblx == (int)EnumCfMbLx.department)
            {
                mbObj.ksCode = this.UserIdentity.DepartmentCode;
            }
            else if (mbObj.mblx == (int)EnumCfMbLx.personal)
            {
                mbObj.ysgh = this.UserIdentity.rygh;
            }

            //模板表
            mbObj.OrganizeId = this.OrganizeId;

            var mbId = _presTemplateDmnService.SaveData(mbObj, mxList);

            return Success(null, mbId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mblx"></param>
        /// <param name="cflx"></param>
        /// <returns></returns>
        private List<TreeViewModel> GetStaticTreeList(int mblx, int cflx, int? expandCflx = null, string mbKeyword = null)
        {
            var cflxmc = ((EnumCflx)cflx).GetDescription();

            var treeList = new List<TreeViewModel>();

            //处方模板
            treeList.Add(new TreeViewModel()
            {
                id = cflx.ToString(),
                value = cflx.ToString(),
                text = cflxmc,
                parentId = null,
                hasChildren = true,
                isexpand = (expandCflx ?? 0) != 0 ? (cflx == expandCflx ? true : false) : true,
                complete = true,
            });
            //处方模板明细
            //var data = _presTemplateDmnService.SelectCfTemplateList(cflx,mblx,this.OrganizeId, this.UserIdentity.DepartmentCode, this.UserIdentity.rygh,mbKeyword);
            var data = _presTemplateRepo.IQueryable().Where(a => a.OrganizeId == this.OrganizeId && a.mblx == mblx && a.cflx == cflx && a.zt == "1" && a.mbmc.Contains(mbKeyword) && (mblx == 1 ? a.ysgh == this.UserIdentity.rygh : (mblx == 2 ? a.ksCode == this.UserIdentity.DepartmentCode : 1 == 1))).Select(a => new { a.mbmc, a.cflx, a.mbId, a.CreateTime, a.LastModifyTime }).OrderByDescending(a => a.CreateTime).ThenByDescending(a => a.LastModifyTime).ToList();
            foreach (var item in data)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = "",   //模板Id
                    value = item.cflx.ToString(),
                    text = item.mbmc,
                    parentId = item.cflx.ToString(),
                    hasChildren = false,
                    isexpand = false,
                    complete = true,
                    Ex1 = item.mbId
                });
            }
            return treeList;
        }
        public ActionResult XzyyForm()
        {
            return View();
        }

    }
}
using FrameworkBase.MultiOrg.Web;
using Newtouch.Common;
using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtouch.Domain.Entity.Outpatient;

namespace Newtouch.CIS.Web.Areas.TemplateManage.Controllers
{
    public class MRecordTemplateController : OrgControllerBase
    {
        private readonly IMRecordTemplateDmnService _mRecordTemplateDmnService;
        private readonly IMRTemplateRepo _mRTemplateRepo;

        // GET: TemplateManage/MRecordTemplate

        /// <summary>
        /// 病历模板树
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTreeList(int mblx)
        {
            var treeList = new List<TreeViewModel>();
            List<MRTemplateListBO> data = _mRecordTemplateDmnService.SelectTemplateList(mblx,this.OrganizeId,this.UserIdentity.DepartmentCode,this.UserIdentity.UserCode);

            foreach (var item in data)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = item.mbId,   //模板Id
                    value = item.mbId,
                    text = item.mbmc,
                    parentId = null,
                    hasChildren = false,
                    isexpand = false,
                    complete = true
                });
            }

            return Content(treeList.TreeViewJson(null));
        }

        /// <summary>
        /// 查询模板明细
        /// </summary>
        /// <param name="mbId"></param>
        /// <returns></returns>
        public ActionResult SelectTemplateDetailByMbId(string mbId)
        {
            var data = _mRecordTemplateDmnService.SelectTemplateDetailByMbId(mbId, this.OrganizeId);
            data.xyzdList = data.xyzdList.OrderBy(a => a.zdlx).ToList();   //主诊断排在前面
            data.zyzdList = data.zyzdList.OrderBy(a => a.zdlx).ToList();   //主诊断排在前面
            

            return Content(data.ToJson());
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveData(MRTemplateEntity blmbObject, List<WMDiagnosisHtmlVO> xyzdList, List<TCMDiagnosisHtmlVO> zyzdList,List<CfTemplateGroupPackageEntity> mbztList)
        {
            blmbObject.OrganizeId = this.OrganizeId;
            if (blmbObject.mblx == (int)EnumCfMbLx.department)
            {
                blmbObject.ks = this.UserIdentity.DepartmentCode;   //科室
            }
            _mRecordTemplateDmnService.SaveData(blmbObject, xyzdList, zyzdList,mbztList);
            return Success();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mbId"></param>
        /// <returns></returns>
        public ActionResult DeleteTemplate(string mbId)
        {
            _mRecordTemplateDmnService.DeleteTemplate(mbId);
            return Success();
        }

    }

}
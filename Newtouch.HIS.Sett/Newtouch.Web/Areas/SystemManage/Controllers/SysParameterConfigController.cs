using Newtouch.HIS.Application.Interface;
using Newtouch.Tools;
using System.Web.Mvc;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.ViewModels;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;

namespace Newtouch.HIS.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 系统参数设置
    /// </summary>
    public class SysParameterConfigController : ControllerBase
    {
        private readonly ISysParameterConfigApp _sysParameterConfigApp;
        private readonly ISysChargeTemplateRepo _sysChargeTemplateRepo;
        private readonly ISysChargeItemDmnService _sysChargeItemDmnService;
        private readonly ISysDepartmentRepo _SysDepartmentRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysFeeDmnService _sysFeeDmnService;

        #region 收费模板

        /// <summary>
        /// 收费模板 Index
        /// </summary>
        /// <returns></returns>
        public ActionResult ChargeTemplateIndex()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfmbbh"></param>
        /// <returns></returns>
        public ActionResult ChargeTemplate_EditForm()
        {
            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult ChargeTemplate_GetGridJson(Pagination pagination, string keyword, string yxbz)
        {
            var list = _sysChargeItemDmnService.Search(pagination, keyword, this.OrganizeId,yxbz);
            var data = new
            {
                rows = list,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfmbbh"></param>
        /// <returns></returns>
        public ActionResult ChargeTemplate_EditForm_Data(string keyValue)
        {
            SysChargeTemplateInfoVM vm = null;
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                vm = _sysParameterConfigApp.GetSysChargeTemplateInfo(keyValue);
            }

            vm = vm ?? new SysChargeTemplateInfoVM();
            return Content(vm.ToJson());
        }

        /// <summary>
        /// 保存操作
        /// </summary>
        /// <param name="moduleEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        //[ValidateAntiForgeryToken]
        public ActionResult ChargeTemplate_SubmitForm(SysChargeTemplateEntity entity, string xmListStr)
        {
            entity.zt = entity.zt == "true" ? "1" : "0";
            entity.OrganizeId = this.OrganizeId;
            _sysParameterConfigApp.ChargeTemplate_SubmitForm(entity, xmListStr);
            return Success("操作成功。");
        }

        public ActionResult GetUsages() {

            var data = _sysChargeItemDmnService.GetYF();
            return Content(data.ToJson());

        }
        public ActionResult deleteid(string keyValue)
        {
            _sysFeeDmnService.DeleteById(keyValue, this.OrganizeId);
            return Success("删除成功");
        }
        #endregion

        #region


        #endregion

    }
}
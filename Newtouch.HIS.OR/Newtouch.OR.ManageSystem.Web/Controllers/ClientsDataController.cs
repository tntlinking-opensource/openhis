using Newtouch.Tools;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using Newtouch.Common.Operator;
using Newtouch.OR.ManageSystem.Domain.IDomainServices;
using Newtouch.Core.Redis;
using Newtouch.OR.ManageSystem.Domain.Entity;
using System.Collections.Generic;
using Newtouch.OR.ManageSystem.Infrastructure;

namespace Newtouch.OR.ManageSystem.Web.Controllers
{
    /// <summary>
    /// Home/Index加载时 默认加载的 缓存数据
    /// </summary>
    public class ClientsDataController : FrameworkBase.MultiOrg.Web.Controllers.ClientsDataController
    {
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly ICommonDmnService _syCommonDmnService;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetModuleDataJson()
        {
            var data = new
            {
                authorizeMenu = this.GetMenuList(),
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取同步的部分
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetClientsDataJson()
        {
            var _OperationsList = _syCommonDmnService.GetOperations("", OrganizeId??"1");
            RedisHelper.Set(string.Format(CacheKey.OperationDic,OrganizeId), _OperationsList);
            var data = new
            {
                OperationsList = _OperationsList,
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取异步的部分
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAsyncClientsDataJson()
        {
            var opr = OperatorProvider.GetCurrent();
            object sysStaffDutyList = null, doctorInHosBookkeep = null;
            doctorInHosBookkeep = _sysUserDmnService.GetStaffByDutyCode(opr.OrganizeId??"1", "Doctor");//住院记账获取门诊医生
            var data = new
            {
                itemDetails = this.GetItemDetailsList(),
                doctorInHosBookkeep = doctorInHosBookkeep,
                SysFailedCodeMessageMapList = _syCommonDmnService.FailMessage(opr.OrganizeId,null),
                enums = this.GetEnumList("Newtouch.OR.ManageSystem.Infrastructure"),
            };
            return Content(data.ToJson());
        }

    }
}
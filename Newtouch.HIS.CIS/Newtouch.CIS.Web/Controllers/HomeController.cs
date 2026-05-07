using System;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using Newtouch.Common;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ViewModels;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IRepository;

namespace Newtouch.CIS.Web.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    public class HomeController : FrameworkBase.MultiOrg.Web.Controllers.HomeController
    {
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IOrderExecutionDmnService _OrderExecutionDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public override ActionResult Index()
        {
            var loginFromFlag = WebHelper.GetCookie(Constants.AppId + "_LoginFromFlag");
            ViewBag.loginFromFlag = loginFromFlag;

            return base.Index();
        }

        public override void IndexBeforeReturnView()
        {
            //
        }

        /// <summary>
        /// 获取业务字段的随机产生值（自增+Format）
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="orgIdIsStar"></param>
        /// <param name="topOrgIdIsStar"></param>
        /// <param name="initFormat"></param>
        /// <param name="initFieldLength"></param>
        /// <returns></returns>
        public ActionResult GetNewFieldUniqueValue(string fieldName, string initFormat = "", int initFieldLength = 0)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrWhiteSpace(initFormat) && initFieldLength > 0)
            {
                initFormat = "{0:D" + initFieldLength + "}";
            }
            string value = null;
            if (string.IsNullOrWhiteSpace(orgId) || initFormat == null)
            {
                value = null;
            }
            else
            {
                value = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue(fieldName, orgId, initFormat);
            }
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = null, data = value }.ToJson());
        }

        #region 消息提醒
        public ActionResult MSGQuery(string gh) {
            var data = _OrderExecutionDmnService.MSGQuery(gh, this.OrganizeId);
            return Content(data.ToJson());
        }
        
        public ActionResult NoticeReadSync(List<NoticeSendBase> noticeSends)
        {
            var queueids = noticeSends.Where(p => !string.IsNullOrWhiteSpace(p.msgid)).GroupBy(p => p.msgid).Select(p => p.Key).ToArray();
            var apiReqObj = new ApiManageRequest<NoticeJobRequest>
            {
                Data = new NoticeJobRequest
                {
                    QueueIds = queueids
                },
                AppId = AuthenManageHelper.accessAppId,
                OrganizeId = OrganizeId,
            };
            var d = apiReqObj.ToJson();
            var readApi = AuthenManageHelper.HttpPost<bool>(apiReqObj.ToJson(), AuthenManageHelper.NoticeReadSendApi, this.UserIdentity);
            if(readApi.data)
            {
                return Success();
            }
            return Error("已读状态更新失败");
        }
        #endregion

        public ActionResult SyncSysConfigParams(string orgId)
        {
            //基础数据
            var sysConfigBaseEntities = _sysConfigRepo.GetList("", "*").ToList();
            //组织机构自带数据
            var sysConfigEntities = _sysConfigRepo.GetList("", orgId).ToList();
            //根据code 去重
            var sysConfigCodes = new HashSet<string>(sysConfigEntities.Select(entity => entity.Code));
            var filteredEntities = sysConfigBaseEntities
                .Where(baseEntity => !sysConfigCodes.Contains(baseEntity.Code))
                .ToList();
            foreach (var item in filteredEntities)
            {
                item.Id = Guid.NewGuid().ToString();
                item.OrganizeId = orgId;
                item.LastModifyTime = DateTime.Now;
            }
            var insert = _sysConfigRepo.Insert(filteredEntities);
            return Success("",insert);
        }
        
    }
}

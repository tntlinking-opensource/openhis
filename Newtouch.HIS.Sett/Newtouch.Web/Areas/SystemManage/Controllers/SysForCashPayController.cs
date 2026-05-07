using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtouch.Tools;
using Newtouch.HIS.Application;
using Newtouch.HIS.Domain.Entity;
using Newtouch.Common;

namespace Newtouch.HIS.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class SysForCashPayController : ControllerBase
    {

        private readonly ISysForCashPayApp _SysForCashPayApp;

        public ActionResult SysForCashPay()
        {
            return View();
        }

        public ActionResult SubmitForm(SysCashPaymentModelEntity SysForCashPayEntity, int? keyValue)
        {
            _SysForCashPayApp.SubmitForm(SysForCashPayEntity, keyValue);
            return Success("操作成功。");
        }

        [HttpPost]
        [HandlerAjaxOnly]
        //[HandlerAuthorize]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteForm(int keyValue)
        {

            _SysForCashPayApp.DeleteForm(keyValue);
            return Success("删除成功。");
        }

        /// <summary>
        /// 获取grid的值
        /// </summary>
        /// <returns></returns>
        public ActionResult GetdlGridJson(string keyword)
        {
            var data = _SysForCashPayApp.GetListBySearch(keyword);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 大类下拉框
        /// </summary>
        /// <returns></returns>
        public ActionResult GetdlSelect()
        {
            var data = _SysForCashPayApp.GetList();
            var treeList = new List<TreeSelectModel>();
            foreach (var item in data)
            {
                //TreeSelectModel treeModel = new TreeSelectModel();
                //treeModel.id = item.dl.ToString();
                //treeModel.text = item.dlmc;
                //treeModel.parentId = "0";
                //treeModel.data = item;
                //treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        /// <summary>
        /// 修改信息时，把信息带到新页面
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(int keyValue)
        {
            var entity = _SysForCashPayApp.GetForm(keyValue);
            return Content(entity.ToJson());
        }

        /// <summary>
        /// 获取grid的值
        /// </summary>
        /// <returns></returns>
        public ActionResult GetfsGridJson(string keyword)
        {
            var data = _SysForCashPayApp.GetList();
            return Content(data.ToJson());
        }

    }
}

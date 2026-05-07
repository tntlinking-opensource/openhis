using Newtouch.EMR.Domain.IRepository;
using Newtouch.Tools;
using System.Web.Mvc;

namespace Newtouch.EMR.Web.Controllers
{
    /// <summary>
    /// Home/Index加载时 默认加载的 缓存数据
    /// </summary>
    public class ClientsDataController : FrameworkBase.MultiOrg.Web.Controllers.ClientsDataController
    {
        private readonly IBlysRepo _blysRepo;

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
            var data = new
            {
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
            var data = new
            {
                ysdataList = _blysRepo.GetYsTreeV2(OrganizeId, ""),//病历元素
                ysmxdataList = _blysRepo.GetYsMX(OrganizeId, ""), //元素明细
                itemDetails = this.GetItemDetailsList(),
                enums = this.GetEnumList("Newtouch.EMR.Infrastructure"),
            };
            return Content(data.ToJson());
        }

    }
}
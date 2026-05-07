using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.OutputDto.DRGManage;
using Newtouch.HIS.Domain.IDomainServices.DRGManage;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.DRGManage.Controllers
{
    public class DRGUploadController : ControllerBase
    {
        private readonly IDRGUploadDmnService _DRGUploadDmnService;
        // GET: DRGManage/DRGUpload
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string kssj, string jssj,string scqk,string zyh)
        {
            var data = new
            {
                rows = _DRGUploadDmnService.GetList(pagination, this.OrganizeId, kssj, jssj,scqk,zyh),
                pagination.total,
                pagination.page,
                pagination.records
            };
            return Content(data.ToJson());
        }


        //DRG上传
        public ActionResult DRGUpload(List<DRGUploadDto> list)
        {
            var data = _DRGUploadDmnService.DRGUpload(this.OrganizeId,list);
            return Content(data.ToJson());
        }

    }
}
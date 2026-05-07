using FrameworkBase.MultiOrg.Web;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Newtouch.CIS.Web.Areas.DoctorManage.Controllers
{
    public class OrginalController : OrgControllerBase
    {
        private readonly IDoctorserviceDmnService _doctorserviceDmnService;

        public ActionResult CurrentdayView()
        {
            return View();
        }
        public ActionResult ConsumablesIndex()
        {
            return View();
        }
        public ActionResult gridPatientList(Pagination pagination, string zyh)
        {
            var data = new
            {
                rows = _doctorserviceDmnService.GetTodayValidService(pagination,OrganizeId, zyh),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
    }
}
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.OutputDto.DRGManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.IDomainServices.DRGManage
{
    public interface IDRGTradUploadDmnService
    {
        List<DRGUploadDto> GetList(string orgId, string kssj, string jssj, string scqk, string zyh,string tradiNumber);
        int DRGUpload(string orgId, List<DRGUploadDto> list,string tradiNumber);
        List<YPUploadDto> GetListYp(string orgId, string kssj, string jssj, string scqk, string tradiNumber);
        List<sqszUploadDto> GetListsqsz(string orgId, string kssj, string jssj, string scqk, string zyh, string tradiNumber);
		List<mxlist> Getmxlist(string orgid, string zyh, string type);
    }
}

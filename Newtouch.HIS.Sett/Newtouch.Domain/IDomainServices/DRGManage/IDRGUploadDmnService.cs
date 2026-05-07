using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.OutputDto.DRGManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.IDomainServices.DRGManage
{
    public interface IDRGUploadDmnService
    {
        List<DRGUploadDto> GetList(Pagination pagination, string orgId, string kssj, string jssj,string scqk,string zyh);
        int DRGUpload(string orgId,List<DRGUploadDto> list);
    }
}

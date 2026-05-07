using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity.PharmacyDrugStorage;
using Newtouch.HIS.Domain.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.IRepository.PharmacyDrugStorage
{
    public interface IPurchaseRepo
    {
        IList<PurchaseVo> GetPurchaseGridJson(Pagination pagination, DateTime kssj, DateTime jssj, string OrganizeId,int ddzt, string yyjhdjh = null, string gysCode = null, string ddbh = null,string ddyy=null);
        void PurchaseDelete(string cgId, string orgId);
        void PurchaseStateUpdate(string cgId, int ddzt, string orgId);
        void PurchaseDdbhUpdate(string cgId, string ddbh, string orgId);
        string SubmitForm(PurchaseEntity entity, string keyValue);
    }
}

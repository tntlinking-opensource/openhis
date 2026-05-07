using System.Collections.Generic;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Infrastructure.EF;

namespace Newtouch.Domain.IRepository.DeptStorageManage
{
    /// <summary>
    /// �����Ϣ
    /// </summary>
    public interface IKfKcxxRepo : IRepositoryBase<KfKcxxEntity>
    {

        /// <summary>
        /// ��ѯ�����Ϣ
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<KfKcxxEntity> SelectData(string warehouseId, string productId);

        /// <summary>
        /// �޸Ŀ��״̬
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="ph"></param>
        /// <param name="pc"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        int UpdateZt(string productId, string ph, string pc, string zt);
    }
}
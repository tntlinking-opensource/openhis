using System;
using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Tools;
using System.Collections.Generic;
using System.Linq;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;

namespace Newtouch.HIS.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class RptrptMzRjbRepo : RepositoryBase<RptrptMzRjbEntity>, IRptrptMzRjbRepo
	{
        public RptrptMzRjbRepo(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

		public RptrptMzRjbEntity GetLastMzrjEntity(string orgId, string UserCode)
		{
            RptrptMzRjbEntity rjEntity = new RptrptMzRjbEntity();
            List<RptrptMzRjbEntity> rjEntitylist = this.IQueryable()
               .Where(p => p.OrganizeId == orgId && p.zt == "1")//&& p.czr == UserCode )
               //.OrderByDescending(a => a.CreateTime)
               .ToList();
              // .FirstOrDefault();
            if (rjEntitylist.Count()==0)
            {
                rjEntity = new RptrptMzRjbEntity() { jssj = DateTime.Now.AddYears(-1) };
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(UserCode))
                    rjEntity = rjEntitylist.Where(p => p.czr == UserCode).OrderByDescending(a => a.CreateTime).FirstOrDefault();
                else
                    rjEntity = rjEntitylist.OrderByDescending(a => a.CreateTime).FirstOrDefault();
            }
			return rjEntity;
		}

		public IList<OutPatientMzrjDto> GetLastMzrjEntityList(string orgId, string UserCode, string keyword)
		{
			var entityList = this.IQueryable()
				.Where(p => p.OrganizeId == orgId && p.zt == "1" && (p.kssj.ToString().Contains(keyword) || p.jssj.ToString().Contains(keyword)))
				.OrderByDescending(a => a.CreateTime)
				.ToList();
            if (entityList.Count > 0) {
                if (!string.IsNullOrWhiteSpace(UserCode))
                    entityList = entityList.Where(p=>p.czr== UserCode).ToList();
            }
			IList<OutPatientMzrjDto> outList = new List<OutPatientMzrjDto>();
			foreach (var item in entityList)
			{
				OutPatientMzrjDto dto = new OutPatientMzrjDto()
				{
					Id = item.Id,
					kssj = item.kssj.ToString("yyyy-MM-dd HH:mm:ss"),
					jssj = item.jssj.ToString("yyyy-MM-dd HH:mm:ss"),
					xjzf = item.xjzf.ToString(),
					zje = item.zje.ToString()
				};
				outList.Add(dto);
            }
			return outList;
		}
	}
}

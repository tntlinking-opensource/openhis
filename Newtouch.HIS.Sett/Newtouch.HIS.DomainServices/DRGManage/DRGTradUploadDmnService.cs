using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.OutputDto.DRGManage;
using Newtouch.HIS.Domain.IDomainServices.DRGManage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Newtouch.HIS.DomainServices.DRGManage
{
    public class DRGTradUploadDmnService : DmnServiceBase, IDRGTradUploadDmnService
    {
        public DRGTradUploadDmnService(IDefaultDatabaseFactory databaseFactory)
    : base(databaseFactory)
        {
        }

        public List<DRGUploadDto> GetList(string orgId, string kssj, string jssj, string scqk, string zyh,string tradiNumber)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //if (!string.IsNullOrWhiteSpace(zyh))
                //{
                    string sql = "";
                    sql = @" exec [usp_drg_trad_upload_search] @orgId=@orgId,@zyh=@zyh,@scqk=@scqk  , @kssj=@kssj,@jssj=@jssj,@tradiNumber=@tradiNumber ";


                    SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@zyh",zyh),
                        new SqlParameter("@scqk",scqk),
                        new SqlParameter("@kssj",kssj),
                        new SqlParameter("@jssj",jssj),
                        new SqlParameter("@tradiNumber",tradiNumber),
                    };
                    //db.ExecuteSqlCommand(sql, para);
                    //db.Commit();

                    return FindList<DRGUploadDto>(sql, para);
                //}
                //else
                //{
                //    List<DRGUploadDto> list = new List<DRGUploadDto>();
                //    return list;
                //}
            }
        }


        public int DRGUpload(string orgId, List<DRGUploadDto> list,string tradiNumber)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var count = 0;
                foreach (var obj in list)
                {
                    var mdtrt_id = obj.mdtrt_id;
                    var setl_id = obj.setl_id;
                    if ((!string.IsNullOrWhiteSpace(mdtrt_id)) && (!string.IsNullOrWhiteSpace(setl_id)))
                    {
                        string sql = "";
                        sql = @" exec [usp_drg_trad_upload] @orgId=@orgId,@mdtrt_id=@mdtrt_id,@setl_id=@setl_id,@tradiNumber=@tradiNumber ";


                        SqlParameter[] para = new SqlParameter[] {
                            new SqlParameter("@orgId",orgId),
                            new SqlParameter("@mdtrt_id",mdtrt_id),
                            new SqlParameter("@setl_id",setl_id),
                            new SqlParameter("@tradiNumber",tradiNumber)
                        };
                        db.ExecuteSqlCommand(sql, para);
                        db.Commit();
                        count++;
                        //return FindList<DRGUploadDto>(sql, para);
                    }
                    else
                    {

                        //return new List<DRGUploadDto>();
                    }
                }
                return count;
            }
        }
        public List<YPUploadDto> GetListYp(string orgId, string kssj, string jssj, string scqk, string tradiNumber)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                string sql = "";
                sql = @" exec [usp_Ypxspc_trad_upload_search] @orgId=@orgId,@scqk=@scqk  , @kssj=@kssj,@jssj=@jssj,@tradiNumber=@tradiNumber ";
                SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@scqk",scqk),
                        new SqlParameter("@kssj",kssj),
                        new SqlParameter("@jssj",jssj),
                        new SqlParameter("@tradiNumber",tradiNumber),
                    };
                return FindList<YPUploadDto>(sql, para);
            }
        }
        public List<sqszUploadDto> GetListsqsz(string orgId, string kssj, string jssj, string scqk,string zyh, string tradiNumber)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                string sql = "";
                sql = @" exec [usp_sqszpc_trad_upload_search] @orgId=@orgId,@zyh=@zyh,@scqk=@scqk  , @kssj=@kssj,@jssj=@jssj,@tradiNumber=@tradiNumber ";
                SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@zyh",zyh),
                        new SqlParameter("@scqk",scqk),
                        new SqlParameter("@kssj",kssj),
                        new SqlParameter("@jssj",jssj),
                        new SqlParameter("@tradiNumber",tradiNumber),
                    };
                return FindList<sqszUploadDto>(sql, para);
            }
        }

		public List<mxlist> Getmxlist(string orgid, string zyh, string type)
		{
			string sql = "";

			if (type=="4402")
			{
				sql = @"select sta.Name kdys,dep.Name kdks,yznr xmmc,convert(varchar(30),kssj,120)kdrq from (
select zyh,ysgh,DeptCode,yznr,kssj,OrganizeId from Newtouch_CIS..zy_cqyz where zyh=@zyh and zt='1' and OrganizeId=@orgId
union all
select zyh,ysgh,DeptCode,yznr,kssj,OrganizeId from Newtouch_CIS..zy_lsyz where zyh=@zyh and zt='1'  and OrganizeId=@orgId
)q
left join NewtouchHIS_Base..Sys_Department dep on dep.Code=q.DeptCode and dep.OrganizeId=dep.OrganizeId
left join NewtouchHIS_Base..Sys_Staff sta on sta.gh=q.ysgh and sta.OrganizeId=q.OrganizeId
order by kssj 
";
			}else if (type=="4501")
			{
				sql = @"select sta.Name kdys,dep.Name kdks,xmmc,lcyx,convert(varchar(30),kssj,120)kdrq from (
select zyh,yzh,sqdh,ysgh,DeptCode,lcyx,ztmc xmmc,max(kssj) kssj,OrganizeId 
from Newtouch_CIS..zy_lsyz a where zyh=@zyh and yzlx=7 and zt='1' and OrganizeId=@orgId 
group by zyh,yzh,sqdh,lcyx,ztmc,ysgh,DeptCode,OrganizeId
)q
left join NewtouchHIS_Base..Sys_Department dep on dep.Code=q.DeptCode and dep.OrganizeId=dep.OrganizeId
left join NewtouchHIS_Base..Sys_Staff sta on sta.gh=q.ysgh and sta.OrganizeId=q.OrganizeId
order by kssj ";
			}
			else if (type == "4502")
			{
				sql = @"select xmzwmc xmmc,xmywmc ,gdbj wjz,xmdw,ckz,jyjg from Newtouch_Interface..Lis_Report_ZY where zyh=@zyh and zt='1' and OrganizeId=@orgId ";
			}

			SqlParameter[] para = new SqlParameter[] {
						new SqlParameter("@orgId",orgid),
						new SqlParameter("@zyh",zyh),
					};
			return FindList<mxlist>(sql, para);
		}
        
    }     
}

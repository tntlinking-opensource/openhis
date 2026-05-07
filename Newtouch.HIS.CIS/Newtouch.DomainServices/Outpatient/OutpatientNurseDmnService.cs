using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.DomainServices
{
    public class OutpatientNurseDmnService : DmnServiceBase, IOutpatientNurseDmnServise
    {
        public OutpatientNurseDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        public IList<OutpatientNurseTreeVO> OutpatientNurseTreeVO(string orgId, string keyword, DateTime? kssj, DateTime? jssj, string type)
        {
            string sql = @" select a.jzid,a.mzh,a.blh,a.xm,a.ghksmc from xt_jz(nolock) a
 left join xt_cf(nolock) b on a.jzId = b.jzId and b.zt='1' and a.OrganizeId=b.OrganizeId
 left join xt_cfmx(nolock)c on b.cfId = c.cfId and c.zt='1' and c.OrganizeId=a.OrganizeId
 where c.ispsbz = '1' and a.zt='1'
 and a.OrganizeId=@orgId
 and (a.mzh like @keyword or a.xm like @keyword)";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));

            if (type == "1")
            {
                sql += "  and not exists(select 1 from xt_gmxx x with(nolock) where c.cfmxid = x.cfmxid and x.zt = '1')";
            }
            else
            {
                sql += "  and  exists(select 1 from xt_gmxx x with(nolock) where c.cfmxid = x.cfmxid and x.zt = '1')";
            }
            if (kssj.HasValue)
            {
                sql += " and c.CreateTime>@kssj ";
                par.Add(new SqlParameter("@kssj", kssj));
            }
            if (jssj.HasValue)
            {
                sql += "  and c.CreateTime<@jssj+' 23:59:59'";
                par.Add(new SqlParameter("@jssj", jssj));
            }
            sql += " group by a.jzid,a.mzh,a.blh,a.xm,a.ghksmc";
            return this.FindList<OutpatientNurseTreeVO>(sql, par.ToArray());
        }

        public IList<OutpatientNursequeryVO> OutpatientNursequery(Pagination pagination, string patList, string orgId)
        {
            string sql = @" select a.cfmxId,c.mzh,c.blh,c.xm,a.ypmc,a.zh,a.CreateTime kssj,a.ypCode
 ,(a.ypmc + ' ' + d.ypgg + ' ' + convert(varchar(8), a.sl) + a.dw + ' (剂量)' + convert(varchar(8), a.mcjl) + a.mcjldw + ' ' + e.yzpcmc + ' ' + f.yfmc) cfmxnr
 ,(case when xb = '女' then '2'else'1' end) sex
   from xt_cfmx(nolock) a
 left join xt_cf(nolock) b on a.cfId = b.cfId and b.zt='1' and a.OrganizeId=b.OrganizeId
 left join xt_jz(nolock) c on c.jzId = b.jzId and c.zt='1' and c.OrganizeId=a.OrganizeId
 inner join[NewtouchHIS_Base]..xt_ypsx d on a.ypcode = d.ypcode  and a.OrganizeId=d.OrganizeId
 inner join[NewtouchHIS_Base]..xt_yzpc e on e.yzpcCode = a.pcCode  and a.OrganizeId=e.OrganizeId
 inner join[NewtouchHIS_Base]..xt_ypyf f on f.yfcode = a.yfcode
 where a.ispsbz = '1' and a.OrganizeId=@orgId and a.zt='1'
 and b.jzid in(select col from dbo.f_split(@jzid, ',') where col> '')
 and not exists(select 1 from xt_gmxx x with(nolock) where a.cfmxid = x.cfmxid and x.zt = '1')";
            if (string.IsNullOrWhiteSpace(patList))
            {
                return null;
            }

            return this.QueryWithPage<OutpatientNursequeryVO>(sql, pagination, new[] {
                new SqlParameter("@jzid", patList),
                new SqlParameter("@orgId", orgId)
            }, false);
        }

        public string Enteragain(OperatorModel user, string cfmxid, string lrjg)
        {
            string sql = "";

            if (string.IsNullOrWhiteSpace(cfmxid))
            {
                throw new FailedException("请选择皮试项目");
            }
            string sqll = "select * from xt_gmxx with(nolock) where OrganizeId=@orgId and cfmxid in(select col from dbo.f_split( @cfmxid,',')) and zt='1' and cancel='1'";
            var exist = FindList<AllergyEntity>(sqll, new SqlParameter[] {
                new SqlParameter("@orgId",user.OrganizeId),
                new SqlParameter("@cfmxid",cfmxid)
            });
            if (exist != null && exist.Count > 0)
            {
                throw new FailedException("部分项目已录入，请刷新页面重试");
            }
            try
            {
                sql = @" insert into [Newtouch_CIS].dbo.xt_gmxx ([Id],[blh],[xm],[sex],[xmCode],[xmmc],[Result],[Remark],
[CreateTime],[CreatorCode],[CreatorName],[LastModifyTime],[LastModifierCode],[LastModifierName],
[mzzybz],[yzid],[cfmxid],[OrganizeId],[zt],[gmlx],[ypCode],[cancel])
select newid(),c.blh,c.xm,(case when c.xb='女' then '2' else '1' end) sex,a.ypcode,a.ypmc,
@lrjg,(case when @lrjg='1' then '阳性' else '阴性' end  ),
getdate(),@user,@username,null,null,null,
'1',null,a.cfmxId,a.OrganizeId,'1',null,a.ypcode,'1' 
 from xt_cfmx(nolock) a
 left join xt_cf(nolock) b on a.cfId = b.cfId
 left join xt_jz(nolock) c on c.jzId = b.jzId
where a.cfmxid in(select col from dbo.f_split(@cfmxid, ',') where col> '')
and a.OrganizeId=@orgId and a.zt='1' and a.ispsbz='1'
and not exists(select 1 from xt_gmxx c with(nolock) where a.OrganizeId=c.OrganizeId and c.cfmxid=a.cfmxId and c.zt='1' and c.cancel='1')";
                ExecuteSqlCommand(sql, new SqlParameter[] {
                    new SqlParameter("@orgId",user.OrganizeId),
                    new SqlParameter("@cfmxid",cfmxid),
                    new SqlParameter("@lrjg",lrjg),
                    new SqlParameter("@user",user.rygh),
                    new SqlParameter("@username",user.UserName),
                });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }

        public IList<OutpatientNursequeryVO> skintesfrom(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, string orgId)
        {
            string sql = @"select a.cfmxId,c.mzh,c.blh,c.xm,a.ypmc,a.zh,a.CreateTime kssj,a.ypCode
 ,(a.ypmc + ' ' + d.ypgg + ' ' + convert(varchar(8), a.sl) + a.dw ) cfmxnr
 ,c.xb,gm.Remark lrjg,b.cfh,gm.CreatorName,gm.CreateTime,gm.LastModifyTime,gm.LastModifierName,gm.cancel,gm.cancel iscancel,gm.Id gmxxid
   from xt_cfmx(nolock) a
 left join xt_cf(nolock) b on a.cfId = b.cfId
 left join xt_jz(nolock) c on c.jzId = b.jzId
 left join xt_gmxx(nolock) gm on gm.cfmxid=a.cfmxId and gm.mzzybz='1' and gm.zt='1'
 inner join[NewtouchHIS_Base]..xt_ypsx d on a.ypcode = d.ypcode  and a.OrganizeId=d.OrganizeId
 inner join[NewtouchHIS_Base]..xt_yzpc e on e.yzpcCode = a.pcCode  and a.OrganizeId=e.OrganizeId
 inner join[NewtouchHIS_Base]..xt_ypyf f on f.yfcode = a.yfcode
 where a.ispsbz = '1' and a.OrganizeId=@orgId
 and (c.mzh like @keyword or c.xm like @keyword) and a.zt='1'
 and exists(select 1 from xt_gmxx x with(nolock) where a.cfmxid = x.cfmxid and x.zt = '1')";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            if (kssj.HasValue && jssj.HasValue)
            {
                sql += " and gm.CreateTime>@kssj and gm.CreateTime<@jssj+' 23:59:59'";
                par.Add(new SqlParameter("@kssj", kssj));
                par.Add(new SqlParameter("@jssj", jssj));
            }

            return this.QueryWithPage<OutpatientNursequeryVO>(sql, pagination, par.ToArray(), false);
        }

        public string skintescancel(string gmxxid, OperatorModel user)
        {
            string sql = "";
            try
            {
                sql = @"update xt_gmxx set cancel='0',LastModifyTime=GETDATE(),LastModifierCode=@user,LastModifierName=@username where id=@gmxxid and OrganizeId=@orgId";
                ExecuteSqlCommand(sql, new SqlParameter[] {
                    new SqlParameter("@orgId",user.OrganizeId),
                    new SqlParameter("@gmxxid",gmxxid),
                    new SqlParameter("@user",user.rygh),
                    new SqlParameter("@username",user.UserName),
                });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }

        public IList<OutpatientNurseTreeVO> GetPatTree(string orgid, string keyword)
        {
            string sql = @" select a.jzid,a.mzh,a.blh,a.xm,a.xb sex,a.nlshow,a.ghksmc from xt_jz(nolock) a
 left join xt_cf(nolock) b on a.jzId = b.jzId and b.zt='1' and a.OrganizeId=b.OrganizeId
 left join xt_cfmx(nolock)c on b.cfId = c.cfId and c.zt='1' and c.OrganizeId=a.OrganizeId
 where  a.zt='1'
 and a.OrganizeId=@orgId
 and (a.mzh like @keyword or a.xm like @keyword)";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgid));
            par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));

            if (true)//默认查询当前月份的处方人员
            {
                sql += "  and SUBSTRING(CONVERT(varchar(15),a.ghsj,120),0,8)=SUBSTRING(CONVERT(varchar(15),GETDATE(),120),0,8)";
            }

            sql += " group by a.jzid,a.mzh,a.blh,a.xm,a.xb,a.nlshow,a.ghksmc";

            return this.FindList<OutpatientNurseTreeVO>(sql, par.ToArray());
        }

        public IList<OutpatientNursequeryVO> prescriptionfrom(Pagination pagination, string jzid, DateTime? klsj, string orgid, string cflb)
        {

            string sql = @" select a.cfmxId,c.mzh,c.blh,c.xm,isnull(a.ypmc,a.xmmc) ypmc,a.zh,a.CreateTime kssj,isnull(a.ypCode,a.xmcode)ypCode
 ,(isnull(a.ypmc,a.xmmc) + ' ' + isnull(d.ypgg,'') + ' ' + convert(varchar(8), a.sl) + a.dw ) cfmxnr
 ,c.xb,b.cfh,b.cfId,f.yfmc ypyfmc,a.yfcode,b.ys yscode,ys.name ysmc,b.cflx,isnull(a.je,0)je
   from xt_cfmx(nolock) a
 left join xt_cf(nolock) b on a.cfId = b.cfId and a.OrganizeId=b.OrganizeId and b.zt='1'
 left join xt_jz(nolock) c on c.jzId = b.jzId and a.OrganizeId=c.OrganizeId and c.zt='1'
 left join xt_gmxx(nolock) gm on gm.cfmxid=a.cfmxId and gm.mzzybz='1' and gm.zt='1'  and gm.cancel=0
 left join[NewtouchHIS_Base]..xt_ypsx d on a.ypcode = d.ypcode  and a.OrganizeId=d.OrganizeId and d.zt='1'
 left join[NewtouchHIS_Base]..xt_yzpc e on e.yzpcCode = a.pcCode  and a.OrganizeId=e.OrganizeId and e.zt='1'
 left join[NewtouchHIS_Base]..xt_ypyf f on f.yfcode = a.yfcode and f.zt='1'
 left join NewtouchHIS_Base..Sys_Staff (nolock) ys on ys.gh=b.ys  and a.OrganizeId=ys.OrganizeId and ys.zt='1'
 where  a.zt='1' and a.OrganizeId=@orgId
 and b.jzid =@jzid ";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgid));
            par.Add(new SqlParameter("@jzid", jzid));
            if (klsj.HasValue)
            {
                sql += "  and SUBSTRING(CONVERT(varchar(15),b.CreateTime,120),0,11)=@klsj";
                par.Add(new SqlParameter("@klsj", klsj));
            }
            var cflbint = cflb == "xycf" ? "1" : cflb == "zycf" ? "2" : cflb == "jycf" ? "4" : cflb == "jccf" ? "5" : cflb == "zscf" ? "7" : cflb == "kfcf" ? "3" : cflb == "cgcf" ? "6" : "";
            if (cflbint != "")
            {
                if (cflbint != "7")
                {
                    sql += " and b.cflx=@cflb";
                    par.Add(new SqlParameter("@cflb", cflbint));
                }
                else
                {
                    sql += " and a.yfcode in('402','401','404','403','4')";

                }
            } 
            return this.QueryWithPage<OutpatientNursequeryVO>(sql, pagination, par.ToArray(), false);
        }

        public string getcfh(string cfid, string orgid)
        {
            string retcfh = "";
            string sql = "";
            try
            {
                sql = @" select cfh from xt_cf where cfid=@cfid and OrganizeId=@orgid";
                var par = new List<SqlParameter>();
                par.Add(new SqlParameter("@cfid", cfid));
                par.Add(new SqlParameter("@orgid", orgid));
                retcfh = FirstOrDefault<string>(sql, par.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return retcfh;

        }

        public IList<ObservationFromVO> observationquery(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, string orgId)
        {
            string sql = @"select *,stuff((select distinct ','+(b.ypmc+' '+d.ypgg+' '+ convert(varchar(8), b.sl) + b.dw+' '+e.yzpcmc +' '+f.yfmc) from mz_lgjl a
left join xt_cfmx b on a.cfid=b.cfid and a.OrganizeId=b.OrganizeId
 inner join [NewtouchHIS_Base]..xt_ypsx d on b.ypcode=d.ypcode
 inner join [NewtouchHIS_Base]..xt_yzpc e on e.yzpcCode=b.pcCode
 inner join [NewtouchHIS_Base]..xt_ypyf f on f.yfcode=b.yfcode
where a.lgjlId=q.lgjlId
for xml path('')),1,1,'')cfmxnr
from(select a.lgjlid,a.mzh,a.xm,b.xb,a.cfh,a.lgksrq,a.lgjsrq,a.miaoshu,a.djrycode,c.name djrymc,a.djrq,a.createtime 
from mz_lgjl a
left join xt_jz b on a.mzh = b.mzh and a.OrganizeId = b.OrganizeId
left join[NewtouchHIS_Base]..sys_staff c on a.djrycode = c.gh and a.OrganizeId = c.OrganizeId
where a.zt = '1' and a.OrganizeId=@orgId
and (a.xm like @keyword or a.mzh like @keyword)";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));

            if (kssj.HasValue && jssj.HasValue)
            {
                sql += " and a.djrq>@kssj and a.djrq<@jssj";
                par.Add(new SqlParameter("@kssj", kssj));
                par.Add(new SqlParameter("@jssj", jssj));
            }
            sql += ")q";
            return this.QueryWithPage<ObservationFromVO>(sql, pagination, par.ToArray(), false);
        }

        public IList<ObservationFromVO> Getlgjl(string syxxids, string orgid)
        {
            Pagination pagination = new Pagination();
            string sql = @"select b.lgjlid, c.xm,c.xb,c.nlshow,c.zjh,d.dh lxdh,convert(varchar(50), a.Id) syxxid,b.miaoshu,b.lgksrq,b.lgjsrq,e.mzmc from mz_syypxx a
left join mz_lgjl b on b.syxxid = a.id and a.organizeid = b.organizeid and b.zt = '1'
left join xt_jz c on a.mzh = c.mzh and c.organizeid = a.organizeid and c.zt = '1'
left join[NewtouchHIS_Sett]..xt_brjbxx d on c.blh = d.blh and d.organizeid = c.organizeid and d.zt = '1'
left join[NewtouchHIS_Base]..xt_mz e on d.mz = e.mzCode and e.zt = '1'
where a.zt = '1' and a.OrganizeId = @orgId
and a.id in (select col from dbo.f_split(@keyword, ',') where col> '')
order by b.createtime ";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgid));
            par.Add(new SqlParameter("@keyword", syxxids));
            return this.FindList<ObservationFromVO>(sql, par.ToArray());
        }

        public string SavaLgdj(IList<ObservationFromVO> lgdjlist, string orgid, OperatorModel user)
        {
            string sql = "";
            string upsql = "";
            bool insert = false;
            var refcount = 0;
            try
            {
                sql = "insert into mz_lgjl(lgjlId,OrganizeId,cfid,cfh,syxxid,miaoshu,lgksrq,lgjsrq,mzh,xm,djrq,djrycode,zt,createtime,createcode,lastmodifytime,lastmodifiercode)";
                for (int i = 0; i < lgdjlist.Count; i++)
                {

                    if (lgdjlist[i].lgjlid != null)
                    {
                        if (lgdjlist[i].deltag != null && lgdjlist[i].deltag == true)
                        {
                            upsql += " update mz_lgjl set zt='0',lastmodifiercode=@user,lastmodifytime=GETDATE() where lgjlId='" + lgdjlist[i].lgjlid + "'  ";
                            break;
                        }
                        upsql += "  update mz_lgjl set miaoshu='" + lgdjlist[i].miaoshu + @"',lgksrq='" + lgdjlist[i].lgksrq + @"',lgjsrq='" + lgdjlist[i].lgjsrq + @"',djrq=GETDATE(),djrycode=@user where lgjlId='" + lgdjlist[i].lgjlid + "'  ";
                    }
                    else
                    {
                        if (i == (lgdjlist.Count - 1))
                        {
                            insert = true;
                            sql += @" select NEWID() lgjlid,a.OrganizeId,c.cfid,a.cfh,a.id syxxid,'" + lgdjlist[i].miaoshu + @"' miaoshu,'" + lgdjlist[i].lgksrq + @"' lgksrq,'" + lgdjlist[i].lgjsrq + @"' lgjsrq,a.mzh,a.xm,GETDATE()djrq,@user djrqcode,'1'zt,GETDATE()createtime,@user createcode,null,null
from mz_syypxx a
left join xt_jz b on a.mzh = b.mzh and a.OrganizeId = b.OrganizeId and b.zt = '1'
left join xt_cf c on a.cfh = c.cfh and a.OrganizeId = c.OrganizeId and c.zt = '1'
where a.zt = '1' and a.OrganizeId = @orgid
and a.Id ='" + lgdjlist[i].syxxid + @"'  ";
                        }
                        else
                        {
                            sql += @" select NEWID() lgjlid,a.OrganizeId,c.cfid,a.cfh,a.id syxxid,'" + lgdjlist[i].miaoshu + @"' miaoshu,'" + lgdjlist[i].lgksrq + @"' lgksrq,'" + lgdjlist[i].lgjsrq + @"' lgjsrq,a.mzh,a.xm,GETDATE()djrq,@user djrqcode,'1'zt,GETDATE()createtime,@user createcode,null,null 
from mz_syypxx a
left join xt_jz b on a.mzh = b.mzh and a.OrganizeId = b.OrganizeId and b.zt = '1'
left join xt_cf c on a.cfh = c.cfh and a.OrganizeId = c.OrganizeId and c.zt = '1'
where a.zt = '1' and a.OrganizeId = @orgid
and a.Id ='" + lgdjlist[i].syxxid + @"'  union all   ";
                        }
                    }

                }
                if (insert)
                {
                    sql = upsql + "   " + sql;
                }
                else
                {
                    sql = upsql;
                }

                refcount = ExecuteSqlCommand(sql, new SqlParameter[] {
                    new SqlParameter("@orgid",orgid),
                    new SqlParameter("@user",user.rygh),
                });
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            string refsts = "F";
            if (refcount > 0)
            {
                refsts = "T";
            }
            return refsts;
        }
    }
}

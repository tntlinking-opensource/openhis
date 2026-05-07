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
    public class DRGUploadDmnService : DmnServiceBase, IDRGUploadDmnService
    {
        public DRGUploadDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        public List<DRGUploadDto> GetList(Pagination pagination, string orgId, string kssj, string jssj, string scqk, string zyh)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (!string.IsNullOrWhiteSpace(zyh))
                {
                    string sql = "";
                    sql = @" exec [usp_drg_upload_search] @orgId=@orgId,@zyh=@zyh,@scqk=@scqk  , @kssj=@kssj,@jssj=@jssj ";


                    SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@zyh",zyh),
                        new SqlParameter("@scqk",scqk),
                        new SqlParameter("@kssj",kssj),
                        new SqlParameter("@jssj",jssj),
                    };
                    //db.ExecuteSqlCommand(sql, para);
                    //db.Commit();

                    return FindList<DRGUploadDto>(sql, para);
                }
                else {
                    List < DRGUploadDto > list= new List<DRGUploadDto>();
                    return list;
                }
            }
        }


        public int DRGUpload(string orgId,List<DRGUploadDto> list)
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
                        sql = @" exec [usp_drg_upload] @orgId=@orgId,@mdtrt_id=@mdtrt_id,@setl_id=@setl_id ";


                        SqlParameter[] para = new SqlParameter[] {
                            new SqlParameter("@orgId",orgId),
                            new SqlParameter("@mdtrt_id",mdtrt_id),
                            new SqlParameter("@setl_id",setl_id)
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

        //        public List<DRGUploadDto> GetList(Pagination pagination, string orgId, string kssj, string jssj,string scqk,string zyh)
        //        {
        //            StringBuilder strsql = new StringBuilder();
        //            IList<SqlParameter> inSqlParameterList = null;
        //            strsql.Append(@"
        //create table #temp
        //(
        //id int identity(1,1),
        //setl_id varchar(50),
        //mdtrt_id varchar(50),
        //zjh varchar(50),
        //zyh varchar(50),
        //brxm varchar(50),
        //ryrq datetime,
        //cyrq datetime,
        //jsrq datetime,
        //basy varchar(50),
        //basy_rq datetime,
        //ksdm varchar(50),
        //ksmc varchar(50),
        //ysdm varchar(50),
        //ysmc varchar(50),
        //jsqd_sclsh varchar(20),
        //jsqd_scrq datetime,
        //)

        ///*就诊ID-mdtrt_id*/ 
        //if(1=1)     
        //begin     
        //insert into #temp(zyh)    
        //select @zyh as zyh  

        //end    

        ///*结算ID/结算流水号-setl_id*/    


        //update ls set ls.mdtrt_id=js.mdtrt_id  
        //  from NewtouchHIS_Sett..drjk_zyjs_output as js with(nolock),#temp as ls  
        // where js.setl_id=ls.setl_id 



        ///*1.病人信息更新*/
        //if(1=1)
        //begin


        ///*更新住院号*/
        //update ls set ls.mdtrt_id =ybry.mdtrt_id
        //  from NewtouchHIS_Sett..drjk_zyjs_input ybry with(nolock),#temp ls
        // where  ls.zyh=ybry.zyh
        //   and ybry.zt=1

        ///*更新病人信息*/
        //update ls set ls.ryrq=br.ryrq,ls.cyrq=br.cyrq,ls.zjh=br.zjh,ls.ksdm=br.ks,ls.ysdm=br.doctor,ls.brxm=br.xm
        //  from NewtouchHIS_Sett..zy_brjbxx as br with(nolock),#temp as ls
        // where br.zyh=ls.zyh
        //   and br.zt=1

        //update ls set ls.ksmc=dept.Name from #temp as ls,NewtouchHIS_Base..Sys_Department as dept with(nolock)
        //where ls.ksdm=dept.Code
        //  and dept.OrganizeId=@orgId
        //update ls set ls.ysmc=czy.Name from #temp as ls,NewtouchHIS_Base..Sys_Staff as  czy with(nolock)
        //where ls.ysdm=czy.gh
        //  and czy.OrganizeId=@orgId

        ///*更新结算日期*/
        //update ls set ls.jsrq=br.czrq,jsqd_sclsh=br.jsqd_sclsh,jsqd_scrq=br.jsqd_scrq,setl_id=br.setl_id
        //  from NewtouchHIS_Sett..drjk_zyjs_output as br with(nolock),#temp as ls
        // where br.mdtrt_id=ls.mdtrt_id
        //   and br.zt=1

        ///*更新病案首页书写状态*/
        //update ls set ls.basy='已书写',ls.basy_rq=ba.CreateTime from Newtouch_EMR..mr_basy as ba with(nolock),#temp as ls 
        //where ba.ZYH=ls.zyh
        //  and ba.zt=1
        //update #temp set #temp.basy='--未写病案首页--' where #temp.basy is null



        //end

        ///*2.结算上传情况获取*/  
        //select rz.hisId,max(rz.inNumbier) inNumbier
        //  into #rz
        //  from NewtouchHIS_Sett.dbo.ybjk_logcontent as rz with(nolock)
        // where rz.beginDate>='2022-04-01'
        //   and rz.tradiNumber='4101'
        // group by rz.hisId

        ///*3.查询输出*/
        //select ls.zyh [zyh],ls.brxm[xm],ls.zjh [sfzh],ls.setl_id [setl_id],ls.mdtrt_id [mdtrt_id],
        //       rz1.beginDate [scrq],rz1.inContent [scsr],rz1.outContent [scsc],
        //       ls.ryrq [ryrq],ls.cyrq [cyrq],ls.basy_rq [basyrq],ls.jsrq [jsrq],
        //       ls.ksdm [ksdm],ls.ksmc [ks],ls.ysdm [ysdm],ls.ysmc [ysxm],ls.basy [basyqk],  
        //       case when isnull(rz1.errorMsg,'')='' then '已上传' 
        //	        when isnull(rz1.errorMsg,'')<>'' then '上传失败' 
        //	   else '未上传'
        //	   end [scqk],
        //	   jsqd_sclsh [jsqd],
        //	   rz1.errorMsg [errorMsg]
        //INTO #temp_result
        //  from #temp as ls left join  #rz as rz on ls.zyh=rz.hisId
        //                   left join NewtouchHIS_Sett.dbo.ybjk_logcontent as rz1 with(nolock) on rz.inNumbier=rz1.inNumbier
        //order by ls.id desc

        //select * from #temp_result
        //where scqk =scqk

        //if(object_id('tempdb.dbo.#temp')is not null) drop table #temp
        //if(object_id('tempdb.dbo.#rz')is not null) drop table #rz
        //if(object_id('tempdb.dbo.#temptable')is not null) drop table #temptable
        //if(object_id('tempdb.dbo.#temp_result')is not null) drop table #temp_result

        //return

        //");
        //            inSqlParameterList = new List<SqlParameter>();
        //            //if (!string.IsNullOrEmpty(blh))
        //            //{
        //            //    strsql.Append(" AND brjbxx.blh like @blh ");
        //            //    inSqlParameterList.Add(new SqlParameter("@blh", "%" + blh.Trim() + "%"));
        //            //}
        //            //if (!string.IsNullOrEmpty(xm))
        //            //{
        //            //    strsql.Append(" AND (brjbxx.xm like @xm or brjbxx.py like @xm)");
        //            //    inSqlParameterList.Add(new SqlParameter("@xm", "%" + xm.Trim() + "%"));
        //            //}
        //            inSqlParameterList.Add(new SqlParameter("@orgId", orgId));
        //            inSqlParameterList.Add(new SqlParameter("@zyh", zyh));
        //            inSqlParameterList.Add(new SqlParameter("@scqk", scqk));
        //            return this.QueryWithPage<DRGUploadDto>(strsql.ToString(), pagination, inSqlParameterList.ToArray()).ToList();
        //        }
    }

}

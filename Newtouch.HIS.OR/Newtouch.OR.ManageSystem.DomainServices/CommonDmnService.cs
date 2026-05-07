using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.OR.ManageSystem.Domain.DTO;
using Newtouch.OR.ManageSystem.Domain.Entity;
using Newtouch.OR.ManageSystem.Domain.IDomainServices;
using Newtouch.OR.ManageSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.OR.ManageSystem.DomainServices
{
    public class CommonDmnService : DmnServiceBase, ICommonDmnService
    {
        public CommonDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {

        }
        /// <summary>
        /// 获取病区列表
        /// </summary>
        /// <param name="bqcode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<SysBqListVO> GetBqlist(string bqcode, string orgId, string staffgh)
        {
            string sql = @"select bqcode,bqmc from [NewtouchHIS_Base].dbo.xt_bq a with(nolock)
where zt='1' and organizeid=@orgId
";
            if (!string.IsNullOrWhiteSpace(bqcode))
            {
                sql += " and bqcode=@bqcode ";
            }
            if (!string.IsNullOrWhiteSpace(bqcode))
            {
                sql += @" and exists(select 1 from NewtouchHIS_Base..V_C_Sys_StaffWard b with(nolock) 
 where a.organizeid = b.organizeid and b.staffgh = @ysgh and b.bqCode = a.bqcode  and b.zt = 1)";
            }

            return this.FindList<SysBqListVO>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId??"1"),
                new SqlParameter("@bqcode",bqcode==null?"":bqcode),
                new SqlParameter("@ysgh",staffgh==null?"":staffgh)
            });
        }
        /// <summary>
        /// 获取手术字典列表
        /// </summary>
        /// <param name="bqcode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<OROperationEntity> GetOplist(string ssdm, string orgId)
        {
            string sql = @"SELECT top 100 [Id],[OrganizeId],[ssdm],[ssmc],[zjm],[ssjb]
,[zt],[CreateTime],[CreatorCode],[LastModifyTime],[LastModifierCode]
FROM [dbo].[OR_Operation] with(nolock)
where zt='1' and organizeid=@orgId
";
            if (!string.IsNullOrWhiteSpace(ssdm))
            {
                sql += " and ssdm=@ssdm ";
            }

            return this.FindList<OROperationEntity>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@ssdm",ssdm==null?"":ssdm)
            });
        }
        public IList<OperationDicVO> GetOperations(string ssdm, string orgId)
        {
            string sql = @"SELECT [Id],[ssdm],[ssdm]+' '+[ssmc] [ssmc],[zjm],[ssjb]
FROM [dbo].[OR_Operation] with(nolock)
where zt='1' and organizeid=@orgId
";
            if (!string.IsNullOrWhiteSpace(ssdm))
            {
                sql += " and (ssdm=@ssdm or ssdm like '@ssdm%' ";
            }

            if (!string.IsNullOrWhiteSpace(ssdm))
            {
                sql += " and (ssmc=@ssdm or ssmc like '@ssdm%' ";
            }

            return this.FindList<OperationDicVO>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId??"1"),
                new SqlParameter("@ssdm",ssdm==null?"":ssdm)
            });
        }
        public IList<OperationDicVO> GetOperations(string[] ssdm, string orgId)
        {
            if (ssdm.Length == 0)
            {
                throw new FailedException("手术代码不可为空");
            }
            string sql = @"SELECT [Id],[ssdm],[ssdm]+' '+[ssmc] [ssmc],[zjm],[ssjb]
FROM [dbo].[OR_Operation] with(nolock)
where zt='1' and organizeid=@orgId  and ssdm in(select col from dbo.f_split(@ssdm,',')) ;
";
            return this.FindList<OperationDicVO>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@ssdm",string.Join(",",ssdm))
            });
        }
        /// <summary>
        /// 院内职工
        /// </summary>
        /// <param name="rygh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<StaffListVO> GetStafflist(string rygh, string orgId)
        {
            string sql = @"select StaffGh rygh, StaffName ryxm
from NewtouchHIS_Base..V_C_Sys_StaffWard b with(nolock)
where zt='1'  and organizeid=@orgId ";

            if (!string.IsNullOrWhiteSpace(rygh))
            {
                sql += " and StaffGh=@rygh ";
            }

            return this.FindList<StaffListVO>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@rygh",rygh==null?"":rygh)
            });
        }


        public IList<ORAnesthesiaEntity> GetAneslistlist(string AnesCode, string orgId)
        {
            string sql = @"select [OrganizeId],[Id],[AnesCode],[AnesName],[Aneszjm],[zt]
,[CreateTime],[CreatorCode],[LastModifyTime],[LastModifierCode]
from [OR_Anesthesia] with(nolock) 
where organizeid=@orgId and zt='1' ";

            if (!string.IsNullOrWhiteSpace(AnesCode))
            {
                sql += " and AnesCode=@AnesCode ";
            }

            return this.FindList<ORAnesthesiaEntity>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId??"1"),
                new SqlParameter("@AnesCode",AnesCode==null?"":AnesCode)
            });
        }

        /// <summary>
        /// 获取手术人员字典列表
        /// </summary>
        /// <param name="bqcode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        //        public IList<ORStaffEntity> GetStafflist2(string Code, string orgId)
        //        {
        //            string sql = @"SELECT [OrganizeId],[Id],[Code],[Name],[zjm],[zt],[CreateTime],[CreatorCode],[LastModifyTime],[LastModifierCode]FROM [dbo].[OR_Staff] with(nolock) where zt='1' and organizeid=@orgId
        //";
        //            if (!string.IsNullOrWhiteSpace(Code))
        //            {
        //                sql += " and Code=@Code ";
        //            }

        //            var result = this.FindList<ORStaffEntity>(sql, new SqlParameter[] {
        //                new SqlParameter("@orgId",orgId),
        //                new SqlParameter("@Code",Code==null?"":Code)
        //            });
        //            return result;
        //        }

        /// <summary>
        /// 获取手术室字典列表
        /// </summary>
        /// <param name="bqcode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<ORRoomEntity> GetRoomlist(string Code, string orgId)
        {
            string sql = @"SELECT [OrganizeId],[Id],[Code],[Name],[Addr],[zt],[CreateTime],[CreatorCode],[LastModifyTime],[LastModifierCode]FROM [dbo].[OR_Room] with(nolock) where zt='1' and organizeid=@orgId";
            if (!string.IsNullOrWhiteSpace(Code))
            {
                sql += " and Code=@Code ";
            }

            var result = this.FindList<ORRoomEntity>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@Code",Code==null?"":Code)
            });
            return result;
        }


        /// <summary>
        /// 获取切口等级字典列表
        /// </summary>
        /// <param name="bqcode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<ORNotchGradeEntity> GetNotchGradelist(string Code, string orgId)
        {
            string sql = @"SELECT [OrganizeId],[Id],[Code] ,[Name],[zt],[CreateTime],[CreatorCode],[LastModifyTime],[LastModifierCode]FROM [dbo].[OR_NotchGrade] with(nolock) where zt='1' and organizeid=@orgId";
            if (!string.IsNullOrWhiteSpace(Code))
            {
                sql += " and Code=@Code ";
            }

            var result = this.FindList<ORNotchGradeEntity>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@Code",Code==null?"":Code)
            });
            return result;
        }

        public IList<SysFailedCodeMessageMappVO> FailMessage(string orgId, string appId)
        {
            string sql = @"select  code , msg 
from [Sys_FailedCodeMessageMapp]
where msg>''  and code>''
";
            List<SqlParameter> para = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(orgId))
            {
                sql += " and [OrganizeId]=@orgId";
                para.Add(new SqlParameter("@orgId", orgId));
            }
            if (!string.IsNullOrWhiteSpace(appId))
            {
                sql += " and [AppId]=@appId";
                para.Add(new SqlParameter("@appId", appId));
            }

            return this.FindList<SysFailedCodeMessageMappVO>(sql, para.ToArray());
        }

    }
}

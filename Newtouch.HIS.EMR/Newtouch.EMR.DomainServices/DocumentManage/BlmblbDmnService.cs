using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtonsoft.Json;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.EMR.Domain.DTO;
using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.IDomainServices;
using Newtouch.EMR.Domain.IRepository;
using Newtouch.EMR.Domain.ValueObjects;
using Newtouch.EMR.Domain.ValueObjects.DocumentManage;
using Newtouch.EMR.Domain.ValueObjects.MedicalRecord;
using Newtouch.EMR.Infrastructure;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.EMR.DomainServices
{
    public class BlmblbDmnService:DmnServiceBase, IBlmblbDmnService
    {
        private readonly ICommonDmnService _CommonDmnService;
        private readonly IBlmblbRepo _BlmblbRepo;
        private readonly IBlmbqxkzRepo _BlmbqxkzRepo;
        private readonly Ibl_bllxRepo _BllxRepo;
        private readonly IZybrjbxxRepo _zybrjbxxRepo;
        private readonly IMrWritingRulesRepo mrwritingrulesRepo;

        public BlmblbDmnService(IDefaultDatabaseFactory databaseFactory, ICommonDmnService CommonDmnService, IBlmblbRepo BlmblbRepo, IBlmbqxkzRepo BlmbqxkzRepo) 
            : base(databaseFactory)
        {
            _CommonDmnService = CommonDmnService;
            _BlmblbRepo = BlmblbRepo;
            _BlmbqxkzRepo = BlmbqxkzRepo;
        }
        /// <summary>
        /// 病历模板列表
        /// </summary>
        /// <param name="mbqx"></param>
        /// <param name="bllx"></param>
        /// <param name="ksbm"></param>
        /// <param name="user"></param>
        /// <param name="OrgId"></param>
        /// <returns></returns>
        public IList<BlmbListVO> MedRecordTmpList(int mbqx, string bllx, string ksbm, OperatorModel user, string OrgId,string keyword=null)
        {
            IList<BlmbListVO> data = null;

            if (bllx == null)
            {
                return data;
            }

            string sql = @"select Id, OrganizeId,mbqx,mbbm,mbmc,bllxId,bllxmc,ksbm,ysgh,mblj,py,Isempty,
                    Memo,CreateTime,CreatorCode,LastModifyTime,LastModifierCode,zt,bllx,LoadWay
                    from bl_mblb a with(nolock)
                    where organizeid=@OrgId and zt=1 
                    and exists(select 1
			                    from [NewtouchHIS_Base].[dbo].[V_C_Sys_StaffDuty] b with(nolock),[bl_mbqxkz] c with(nolock)
			                    where b.staffid=@staffId and
			                     b.dutyCode=c.dutyCode and b.zt=1 and c.zt=1 and c.ctrlLevel=2
			                     and a.id=c.mbId and a.organizeid=c.organizeid )";


            bl_bllxEntity mbety = _BllxRepo.FindEntity(p => p.bllx == bllx && p.OrganizeId == OrgId && p.zt == "1");
            if (mbety!=null && string.IsNullOrWhiteSpace(mbety.ParentId))
            {
                string bllxlist = "'" + bllx + "'";
                var list = _CommonDmnService.GetBllxListDetail(OrgId, mbety.Id, null,"2");
                if (list != null && list.Count > 0)
                {
                    
                    foreach (var item in list)
                    {
                        bllxlist +=",'"+ item.bllx+"'";
                    }
                }
                sql += " and bllx in(" + bllxlist + ") ";
            }
            else {
                sql += " and bllx=@bllx ";
            }
            var para = new List<SqlParameter>();
            para.Add(new SqlParameter("@OrgId", OrgId));
            para.Add(new SqlParameter("@mbqx", mbqx));
            para.Add(new SqlParameter("@staffId", user.StaffId));
            para.Add(new SqlParameter("@bllx", bllx??""));
            if (mbqx == Convert.ToInt32(Enummbqx.pub))
            {
                sql += " and mbqx=@mbqx ";
            }
            else if (mbqx == Convert.ToInt32(Enummbqx.prv))
            {
                if (!string.IsNullOrWhiteSpace(user.rygh))
                {
                    sql += " and mbqx=@mbqx and ysgh=@ysgh ";
                    para.Add(new SqlParameter("@ysgh", user.rygh));
                }
                else
                {
                    throw new FailedException("医生工号不可为空");
                }
            }
            else if (mbqx == Convert.ToInt32(Enummbqx.dept))
            {
                if (!string.IsNullOrWhiteSpace(ksbm))
                {
                    sql += " and mbqx=@mbqx and ksbm=@ksbm ";
                    para.Add(new SqlParameter("@ksbm", ksbm));
                }
                else
                {
                    throw new FailedException("所属科室不可为空");
                }
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and mbmc like @keyword ";
                para.Add(new SqlParameter("@keyword", "%"+keyword+"%"));
            }
            
            return this.FindList<BlmbListVO>(sql, para.ToArray());

        }
        /// <summary>
        /// 模板岗位权限控制维护
        /// </summary>
        /// <param name="list"></param>
        /// <param name="user"></param>
        public void TempCtrlAssigned(string list, string mbId, OperatorModel user)
        {
            var res = JsonConvert.DeserializeObject<IList<TemplateCtrlRequestDto>>(list);
            foreach (TemplateCtrlRequestDto item in res)
            {
                BlmbqxkzEntity ety = new BlmbqxkzEntity();
                ety.zt = item.zt;
                ety.ctrlLevel = item.ctrlLevel;
                ety.ctrlLevelDesc = item.ctrlLevel == Convert.ToInt32(EnummbqxFp.non) ? "未分配" : "";
                if (string.IsNullOrWhiteSpace(ety.ctrlLevelDesc))
                {
                    ety.ctrlLevelDesc = item.ctrlLevel == Convert.ToInt32(EnummbqxFp.edit) ? "读写权限" : "";
                }
                if (string.IsNullOrWhiteSpace(ety.ctrlLevelDesc))
                {
                    ety.ctrlLevelDesc = item.ctrlLevel == Convert.ToInt32(EnummbqxFp.read) ? "只读权限" : "";
                }
                ety.dutyCode = item.dutyCode;
                ety.dutyName = item.dutyName;
                ety.mbId = mbId;
                ety.OrganizeId = user.OrganizeId;
                _BlmbqxkzRepo.SubmitForm(ety, item.gxId);

            }
        }

        public IList<BlmbListVO> GetBlmbList(Pagination pagination, string OrganizeId, string keyword)
        {
            string sql = @"SELECT a.[Id],a.[OrganizeId],[mbqx],[mbbm],mbmc,[bllxId],(case when c.bllxmc>'' then c.bllxmc+'->'+a.[bllxmc] else a.[bllxmc] end) bllxmc,
[ksbm],[ysgh],[mblj],[py],[Isempty],IsYB,[Memo],a.[CreateTime],a.[CreatorCode],a.[LastModifyTime],a.[LastModifierCode],a.[zt],a.bllx,c.bllx bldl,c.bllxmc bldlmc,a.mzbz
FROM[dbo].[bl_mblb] a with(nolock)
left join bl_bllx b with(nolock) on a.OrganizeId=b.OrganizeId and a.bllx=b.bllx and b.zt='1' 
left join bl_bllx c with(nolock) on b.OrganizeId=c.OrganizeId and b.ParentId=c.Id and c.zt='1' 
where a.zt=1 and a.OrganizeId=@OrgId
";
            var para = new List<SqlParameter>();
            para.Add(new SqlParameter("@OrgId", OrganizeId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (charindex(@keyword,mbmc)>0  or charindex(@keyword,mbbm)>0) ";
                para.Add(new SqlParameter("@keyword", keyword));
            }
            return this.QueryWithPage<BlmbListVO>(sql, pagination, para.ToArray()).ToList();
        }

        #region 病历文件操作
        public BlConvertToTemplateVO BlConvertToTemplate(string OrgId, string blId, string bllx, int? mbqx)
        {
            try {
                bl_bllxEntity bllxtb = _BllxRepo.FindEntity(p => p.OrganizeId == OrgId && p.zt == "1" && (p.bllx == bllx || p.Id==blId));
                string sql = @"select @mbqx [mbqx],a.blId,a.[bllx],b.blxtml,b.blxtmc_yj blxtmc,0 [Isempty]
                        from [dbo].[zy_meddocs_relation] a with(nolock), " + bllxtb.relTB + @" b with(nolock)
                        where a.blid = b.id
                        and a.blid = @blId and a.OrganizeId = @OrgId";

                var para = new List<SqlParameter>();
                para.Add(new SqlParameter("@OrgId", OrgId));
                para.Add(new SqlParameter("@mbqx", mbqx));
                para.Add(new SqlParameter("@blId", blId));

                return this.FirstOrDefault<BlConvertToTemplateVO>(sql, para.ToArray());
            }
            catch (Exception ex)
            {
                throw new FailedException("转换模板异常："+ex.InnerException);
            }


        }

        public void BlConvertToTemplateProcess(BlmblbEntity ety)
        {
            string aimPath = Path.GetDirectoryName(ety.mblj);
            string srcPath = ety.Memo + ".xml";
            bool flag = false;
            try
            {
                flag = CopyDir(srcPath, aimPath, ety.mbmc + ".xml", 0);
            }
            catch
            {
                flag = false;
            }
            if (flag == false)
            {
                var mbinfo = _BlmblbRepo.FindEntity(p => p.OrganizeId == ety.OrganizeId && p.mbbm == ety.mbbm && p.zt == "1");
                mbinfo.zt = "0";
                _BlmblbRepo.SubmitForm(mbinfo, mbinfo.Id);
            }

        }
        /// <summary>
        /// 文件操作
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="aimPath"></param>
        /// <param name="newName"></param>
        /// <param name="type">0 复制  1 移动</param>
        /// <returns></returns>
        public bool CopyDir(string srcPath, string aimPath, string newName,int type)
        {
            bool flag = false;
            // 检查目标目录是否以目录分割字符结束如果不是则添加
            if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                aimPath += Path.DirectorySeparatorChar;
            }
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = Path.GetFileName(srcPath);
            }

            // 判断模板目录是否存在如果不存在则新建
            if (!Directory.Exists(aimPath))
            {
                Directory.CreateDirectory(aimPath);
            }

            if (File.Exists(srcPath) && (File.Exists(aimPath + newName)) && type==1)
            {
                File.Delete(aimPath + newName);
            }
            //文件存在则直接复制
            if (File.Exists(srcPath) && !(File.Exists(aimPath + newName)))
            {
                if (type == 0)
                {
                    File.Copy(srcPath, aimPath + newName, true);
                    flag = true;
                }
                else if (type == 1)
                {
                    File.Move(srcPath, aimPath + newName);
                    flag = true;
                }
            }
            

            return flag;
        }


        #endregion


        #region 权限
        public IList<TemplateQxkzVO> Getqxkz(string staffId, string mbId, string bllx)
        {
            string sql = @"select  a.bllx, isnull(max(c.ctrlLevel),0) ctrlLevel
from bl_mblb a with(nolock),[NewtouchHIS_Base].[dbo].[V_C_Sys_StaffDuty] b with(nolock),[bl_mbqxkz] c with(nolock)
where a.Id=c.mbId and b.StaffId=@staffId and b.OrganizeId=c.OrganizeId and b.DutyCode=c.dutyCode
and a.zt='1' and b.zt='1' and c.zt='1'
";
            if (!string.IsNullOrWhiteSpace(mbId))
            {
                sql += " and a.Id=@mbId ";
            }
            if (!string.IsNullOrWhiteSpace(bllx))
            {
                sql += " and a.bllx=@bllx ";
            }
            sql += " group by a.bllx ";

            return this.FindList<TemplateQxkzVO>(sql, new SqlParameter[] {
                new SqlParameter("@staffId",staffId),
                new SqlParameter("@mbId",mbId ?? ""),
                new SqlParameter("@bllx",bllx)
            });
        }

        #endregion
        /// <summary>
        /// 病历模板列表
        /// </summary>
        /// <param name="mbqx"></param>
        /// <param name="bllx"></param>
        /// <param name="ksbm"></param>
        /// <param name="user"></param>
        /// <param name="OrgId"></param>
        /// <returns></returns>
        public IList<BlmbListVO> MedRecordTmpListTree(string OrgId,string keyValue,string mbqx)
       {
            string sql = @"select id,bllx,bllxmc mbmc,ParentId,'' Turl,px from (SELECT a.[Id]
,a.[bllx]
,a.[bllxmc]
,a.[CreateTime]
,a.[CreatorCode]
,a.[LastModifyTime]
,a.[LastModifierCode]
,a.[zt]
,isnull(a.RelDutys,r.RelDutys)RelDutys
,(select b.[name]+'，' from [NewtouchHIS_Base].[dbo].[V_S_Sys_Duty] b where charindex(','+b.code+',',','+isnull(a.RelDutys,r.RelDutys))>0 for xml path('')) [RelDutysDesc]
,a.[OrganizeId]
,isnull(a.[bllxcode],r.[bllxcode])[bllxcode]
,a.[px],
isnull(a.[relTB],r.[relTB])[relTB],
isnull(a.MenuLev,r.MenuLev)MenuLev,a.ParentId,a.IsRoot,a.mzbz
FROM [dbo].[bl_bllx] a with(nolock)
left join [dbo].[bl_bllx] r with(nolock) on a.ParentId=r.Id
where a.organizeid=@OrgId and a.zt='1') bllx
union all 
select id,bllx,mbmc mbmc,bllxId ParentId,mblj Turl,99 px from (select Id, OrganizeId,mbqx,mbbm,REPLACE(mbmc,'\','/') mbmc,bllxId,bllxmc,ksbm,ysgh,mblj,py,Isempty,
                    Memo,CreateTime,CreatorCode,LastModifyTime,LastModifierCode,zt,bllx,LoadWay
                    from bl_mblb a with(nolock)
					where organizeid=@OrgId and zt=1 
                    and mbqx=@mbqx
                    and mbmc  like @keyValue
					) mbmx  order by px
";
            var para = new List<SqlParameter>();
            para.Add(new SqlParameter("@OrgId", OrgId));
            para.Add(new SqlParameter("@keyValue", "%" + keyValue + "%"));
            para.Add(new SqlParameter("@mbqx", mbqx ?? ""));
            return this.FindList<BlmbListVO>(sql, para.ToArray());

        }

        /// <summary>
        /// 患者已有模板的病历
        /// </summary>
        /// <param name="OrgId"></param>
        /// <param name="zyh"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        public IList<blmbmcVO> getRepeatBl(string OrgId, string zyh, string bllx)
        {
            string sql = @"
select c.bllx,c.bllxmc,mbbm,mbmc,blmc from [zy_meddocs_relation] a
left join [dbo].[bl_mblb] b
on a.mbid=b.id and a.zt=b.zt and a.organizeId=b.organizeId
left join bl_bllx c on c.id =b.bllxId and b.zt=c.zt and b.organizeId=c.organizeId
where a.zt=1 and a.organizeId=@OrgId
and zyh=@zyh
and c.bllx =@bllx
";
            var para = new List<SqlParameter>();
            para.Add(new SqlParameter("@OrgId", OrgId));
            para.Add(new SqlParameter("@zyh",  zyh ));
            para.Add(new SqlParameter("@bllx", bllx ?? ""));
            return this.FindList<blmbmcVO>(sql, para.ToArray());

        }

        public string CheckBlRules(string OrgId, string zyh, string bllx)
        {
            var zybrxx=_zybrjbxxRepo.FindEntity(p => p.zyh == zyh && p.OrganizeId == OrgId && p.zt == "1");
            
            var rulseentity = mrwritingrulesRepo.FindEntity(p => p.Bllx == bllx && p.OrganizeId == OrgId && p.zt == "1");
            var rulsdate = zybrxx.ryrq.AddDays(rulseentity.RulesDay);
            if (rulseentity != null && rulseentity.RulesDay!=0)
            {
                if (rulsdate<=DateTime.Now)
                {
                    return "F|"+ rulsdate;
                }
            }
            return "T|" + rulsdate;
        }
    }
}

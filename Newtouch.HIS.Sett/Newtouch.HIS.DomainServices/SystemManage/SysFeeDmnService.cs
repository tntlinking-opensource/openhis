using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.SystemManage;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 系统费用相关 DmnService
    /// </summary>
    public class SysFeeDmnService : DmnServiceBase, ISysFeeDmnService
    {
        public SysFeeDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 获取收费模板的项目列表
        /// </summary>
        /// <param name="sfmbbh"></param>
        /// <returns></returns>
        public IList<SysChargeItemTemplateVO> GetChargeTemplateChargeItemList(string sfmbbh, string orgId)
        {
            var sql = @"select 
                        b.sfxmId ,
                        b.sfxmCode ,
                        b.sfxmmc ,
                        b.sfdlCode ,
                        b.badlCode ,
                        b.nbdlCode ,
                        b.py ,
                        b.dw ,
                        b.dj ,
                        b.zfbl ,
                        b.mzzybz ,
                        b.CreateTime ,
                        b.ssbz ,
                        a.sl ,
                        a.duration ,
                        a.kflb ,
a.zll ,
a.zxcs ,
                        itemdetail.Name kflbmc ,
a.zxks, a.yzpc,
a.bw
from xt_sfmbxm(nolock) a
                left join NewtouchHIS_Base..V_S_xt_sfxm b
                on a.sfxm = b.sfxmCode and b.OrganizeId = @orgId
                LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail itemdetail ON ( itemdetail.OrganizeId = a.OrganizeId
                                                              OR itemdetail.OrganizeId = '*')
                                                              AND itemdetail.CateCode = 'RehabTreatmentMethod'
                                                              AND itemdetail.Code=a.kflb
                where a.OrganizeId = @orgId and a.sfmbbh = @sfmbbh
                and a.zt = '1' and b.zt = '1'";
            return this.FindList<SysChargeItemTemplateVO>(sql, new[] { new SqlParameter("@sfmbbh", sfmbbh)
                ,new SqlParameter("@orgId", orgId)});
        }

        /// <summary>
        /// 保存收费模板
        /// </summary>
        /// <param name="tmp"></param>
        /// <param name="itemList"></param>
        public void SaveChargeTemplate(SysChargeTemplateEntity entity, IList<SysChargeItemTemplateVO> itemList)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (!string.IsNullOrWhiteSpace(entity.sfmbbh))
                {
                    //Code重复判断
                    if (db.IQueryable<SysChargeTemplateEntity>().Any(p => p.sfmb == entity.sfmb && p.sfmbbh != entity.sfmbbh && p.OrganizeId == entity.OrganizeId))
                    {
                        throw new FailedException("编码不可重复");
                    }
                    entity.Modify();
                    db.Update(entity);

                    //删除old
                    db.Delete<SysChargeTemplateItemMappEntity>(p => p.sfmbbh == entity.sfmbbh);
                }
                else
                {
                    //Code重复判断
                    if (db.IQueryable<SysChargeTemplateEntity>().Any(p => p.sfmb == entity.sfmb && p.OrganizeId == entity.OrganizeId))
                    {
                        throw new FailedException("编码不可重复");
                    }
                    entity.Create(true);
                    db.Insert(entity);
                }

                foreach (var item in itemList)
                {
                    db.Insert(new SysChargeTemplateItemMappEntity()
                    {
                        sfmbxmId = Comm.GuId(),
                        OrganizeId = entity.OrganizeId,
                        sfmbbh = entity.sfmbbh,
                        sfxm = item.sfxmCode,
                        CreateTime = DateTime.Now,
                        sl = item.sl,
                        duration = item.duration,
                        kflb = item.kflb,
                        zll = item.zll,
                        zxcs = item.zxcs,
                        zxks = item.zxks,
                        yzpc = item.yzpc,
                        bw = item.bw,
                        dj=item.dj,
                        CreatorCode = OperatorProvider.GetCurrent().UserCode,
                        zt = "1",
                    });
                }

                db.Commit();
            }
        }

        /// <summary>
        /// 系统病人收费范围有效列表
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public List<SysPatientChargeRangeVO> GetSysPatientChargeRangeList(string keyValue, int? bh = null)
        {
            List<SysPatientChargeRangeVO> effectiveList;
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select a.brsffwbh,a.brxz,b.brxzmc,c.dlCode,c.dlmc,a.sfxm,d.sfxmmc,a.yp,e.ypmc,a.zt,a.CreatorCode,a.CreateTime
						    from xt_brsffw a
                            left join xt_brxz b on a.brxz=b.brxz
							left join NewtouchHIS_Base..V_S_xt_sfdl c on a.dl=c.dlCode
							left join NewtouchHIS_Base..V_S_xt_sfxm d on a.sfxm=d.sfxmCode
							left join NewtouchHIS_Base..V_S_xt_yp e on a.yp=e.ypCode
                            where 1=1 
                        ");
            var parms = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                strSql.Append(" and( b.brxz like @searchkeyValue or b.brxzmc like @searchkeyValue or b.py like @searchkeyValue )");

                parms.Add(new SqlParameter("@searchkeyValue", "%" + (keyValue ?? "") + "%"));

            }
            if (bh.HasValue && bh.Value > 0)
            {
                strSql.Append(@" and a.brsffwbh=@bh");
                parms.Add(new SqlParameter("@bh", bh));
            }
            strSql.Append(" order by a.px, a.CreateTime desc");
            if (parms.Count > 0)
            {
                effectiveList = this.FindList<SysPatientChargeRangeVO>(strSql.ToString(), parms.ToArray()).ToList();
            }
            else
            {
                effectiveList = this.FindList<SysPatientChargeRangeVO>(strSql.ToString()).ToList();

            }
            return effectiveList;
        }



        /// <summary>
        /// 收费减免有效列表
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public List<SysPatiChargeWaiverVo> GetSysPatiChargeWaiverList(string keyValue, int? bh = null)
        {
            List<SysPatiChargeWaiverVo> effectiveList;
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
						select brsfjmbh,brsfjm.brxz,brxzmc,brsfjm.dl,dlmc,brsfjm.sfxm,sfxmmc,jmbl,brsfjm.zt,brsfjm.CreatorCode,brsfjm.CreateTime,brsfjm.LastModifyTime 
						from xt_brsfjm brsfjm 
						left join xt_brxz brxz on brsfjm.brxz = brxz.brxz
					    left join NewtouchHIS_Base..V_S_xt_sfdl sfdl on brsfjm.dl = sfdl.dlCode 
						left join NewtouchHIS_Base..V_S_xt_sfxm sfxm on brsfjm.sfxm = sfxm.sfxmCode 
						where 1=1 
                        ");
            var parms = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                strSql.Append(" and ( brsfjm.brxz like @searchkeyValue or brxz.brxzmc like @searchkeyValue or brxz.py like @searchkeyValue )");

                parms.Add(new SqlParameter("@searchkeyValue", "%" + (keyValue ?? "") + "%"));

            }
            if (bh.HasValue && bh.Value > 0)
            {
                strSql.Append(@" and brsfjm.brsfjmbh=@bh");
                parms.Add(new SqlParameter("@bh", bh));
            }
            strSql.Append(" order by brsfjm.px, brsfjm.CreateTime desc");
            if (parms.Count > 0)
            {
                effectiveList = this.FindList<SysPatiChargeWaiverVo>(strSql.ToString(), parms.ToArray()).ToList();
            }
            else
            {
                effectiveList = this.FindList<SysPatiChargeWaiverVo>(strSql.ToString()).ToList();

            }
            return effectiveList;
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="orgId"></param>
        public void DeleteById(string keyValue, string orgId)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                SysChargeTemplateEntity entity = db.IQueryable<SysChargeTemplateEntity>().Where(p => p.sfmbbh == keyValue && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
                if (entity!=null)
                {
                    db.Delete<SysChargeTemplateItemMappEntity>(p => p.sfmbbh == entity.sfmbbh);
                    db.Delete<SysChargeTemplateEntity>(p => p.sfmbbh == entity.sfmbbh);
                }
                db.Commit();
            }
        }
    }

}

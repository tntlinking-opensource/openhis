using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.ValueObjects;
using Newtouch.EMR.Domain.BusinessObjects;
using Newtouch.EMR.Domain.DTO.OutputDto.MRUpload;
using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.IDomainServices;
using Newtouch.EMR.Domain.IRepository;
using Newtouch.EMR.Domain.ValueObjects;
using Newtouch.EMR.Domain.ValueObjects.MedicalRecord;
using Newtouch.EMR.Infrastructure;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Newtouch.EMR.DomainServices
{
    public class MedicalRecordDmnService : DmnServiceBase, IMedicalRecordDmnService
    {

        private readonly Ibl_ryblRepo _bl_ryblRepo;
        private readonly Ibl_bcjlRepo _bl_bcjlRepo;
        private readonly Ibl_hljlRepo _bl_hljlRepo;
        private readonly Ibl_zqwsRepo _bl_zqwsRepo;
        private readonly Ibl_patrecordsRepo _bl_patrecordsRepo;
        private readonly IBlbasyRepo _BlbasyRepo;
        private readonly ICommonDmnService _CommonDmnService;
        private readonly Ibl_bllxRepo _BllxRepo;

        public MedicalRecordDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        #region 病历操作


        /// <summary>
        ///锁定/解锁病历编辑
        /// </summary>
        /// <param name="blId"></param>
        /// <param name="blgxId">病历关系Id</param>
        /// <param name="OrgId"></param>
        /// <param name="user"></param>
        public void LockRecord(string blid, string bllx, string OrganizeId, string UserCode, int isLock)
        {
            var bllxtb = _BllxRepo.FindEntity(p => p.OrganizeId == OrganizeId && p.zt == "1" && p.bllx == bllx);
            string sql = @"update a
                        set a.IsLock = @IsLock,a.LastModifierCode = '" + UserCode + @"',a.LastModifyTime = getdate()
                        from " + bllxtb.relTB + @" a
                        where a.id = @id and a.OrganizeId = @OrgId ";
            try
            {
                var para = new List<SqlParameter>();
                para.Add(new SqlParameter("@OrgId", OrganizeId));
                para.Add(new SqlParameter("@Id", blid));
                para.Add(new SqlParameter("@IsLock", isLock));
                this.ExecuteSqlCommand(sql, para.ToArray());
            }
            catch (FailedException ex)
            {
                throw new FailedException("操作失败(" + ex.InnerException + ")");
            }

        }
        #endregion

        public medicalRecordVO GetMedicalRecord(string blid, string bllx)
        {
            var medicalRecordVO = new medicalRecordVO();
            switch (bllx.Substring(0, 1))
            {
                //入院病历
                case "1":
                    var rybl = _bl_ryblRepo.bl_ryblGetByID(blid);
                    medicalRecordVO.blxtml = rybl.blxtml;
                    medicalRecordVO.blxtmc_yj = rybl.blxtmc_yj;
                    medicalRecordVO.IsLock = rybl.IsLock;
                    medicalRecordVO.LastModifierCode = rybl.LastModifierCode;
                    break;
                //病程记录
                case "2":
                    var bcjl = _bl_bcjlRepo.bl_bcjlGetByID(blid);
                    medicalRecordVO.blxtml = bcjl.blxtml;
                    medicalRecordVO.blxtmc_yj = bcjl.blxtmc_yj;
                    medicalRecordVO.IsLock = bcjl.IsLock;
                    medicalRecordVO.LastModifierCode = bcjl.LastModifierCode;
                    break;
                //医疗文书
                case "3":
                    var ylws = _bl_zqwsRepo.bl_zqwsGetByID(blid);
                    medicalRecordVO.blxtml = ylws.blxtml;
                    medicalRecordVO.blxtmc_yj = ylws.blxtmc_yj;
                    medicalRecordVO.IsLock = ylws.IsLock;
                    medicalRecordVO.LastModifierCode = ylws.LastModifierCode;
                    break;
                //护理记录
                case "4":
                    var hljl = _bl_hljlRepo.bl_hljlGetByID(blid);
                    medicalRecordVO.blxtml = hljl.blxtml;
                    medicalRecordVO.blxtmc_yj = hljl.blxtmc_yj;
                    medicalRecordVO.IsLock = hljl.IsLock;
                    medicalRecordVO.LastModifierCode = hljl.LastModifierCode;
                    break;
                //病案首页
                case "5":
                    var basy = _BlbasyRepo.bl_basyGetByID(blid);
                    medicalRecordVO.blxtml = basy.blxtml;
                    medicalRecordVO.blxtmc_yj = basy.blxtmc_yj;
                    medicalRecordVO.IsLock = basy.IsLock;
                    medicalRecordVO.LastModifierCode = basy.LastModifierCode;
                    break;
                //康复评估
                case "6":
                    var kfpg = _bl_patrecordsRepo.GetpatrecordsByID(blid);
                    medicalRecordVO.blxtml = kfpg.blxtml;
                    medicalRecordVO.blxtmc_yj = kfpg.blxtmc_yj;
                    medicalRecordVO.IsLock = kfpg.IsLock;
                    medicalRecordVO.mbbh = kfpg.mbbh;
                    medicalRecordVO.LastModifierCode = kfpg.LastModifierCode;
                    break;
            }
            return medicalRecordVO;
        }

        /// <summary>
        /// 病历保存
        /// </summary>
        /// <param name="medicalRecord"></param>
        /// <param name="Entity"></param>
        public void MedicalRecordSave(medicalRecordVO medicalRecord, ZymeddocsrelationEntity Entity)
        {

            switch (medicalRecord.bllx.Substring(0, 1))
            {
                /// 入院病历保存
                case "1":
                    var bl_rybl = new bl_ryblEntity();
                    bl_rybl.Id = medicalRecord.ID;
                    bl_rybl.CreatorCode = medicalRecord.CreatorCode;
                    bl_rybl.ksdm = medicalRecord.ksdm;
                    bl_rybl.ksmc = medicalRecord.ksdm;
                    bl_rybl.OrganizeId = medicalRecord.OrganizeId;
                    bl_rybl.blxtml = medicalRecord.blxtml;
                    bl_rybl.mbbh = medicalRecord.mbbh;
                    bl_rybl.zyh = medicalRecord.zyh;
                    bl_rybl.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_rybl.CreateTime = DateTime.Now;
                    bl_rybl.zt = "1";
                    if (Entity == null)
                    {
                        /// 入院病历编辑保存
                        _bl_ryblRepo.SubmitForm(bl_rybl, medicalRecord.ID);
                    }
                    else
                    {
                        bl_rybl.dzbl_id = medicalRecord.dzbl_id;
                        bl_ryblSave(bl_rybl, Entity);
                    }

                    break;
                /// 病程记录保存
                case "2":
                    var bl_bcjl = new bl_bcjlEntity();
                    bl_bcjl.Id = medicalRecord.ID;
                    bl_bcjl.CreatorCode = medicalRecord.CreatorCode;
                    bl_bcjl.ksdm = medicalRecord.ksdm;
                    bl_bcjl.ksmc = medicalRecord.ksdm;
                    bl_bcjl.OrganizeId = medicalRecord.OrganizeId;
                    bl_bcjl.blxtml = medicalRecord.blxtml;
                    bl_bcjl.mbbh = medicalRecord.mbbh;
                    bl_bcjl.zyh = medicalRecord.zyh;
                    bl_bcjl.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_bcjl.CreateTime = DateTime.Now;
                    bl_bcjl.zt = "1";

                    if (Entity == null)
                    {
                        /// 病程记录编辑保存
                        _bl_bcjlRepo.SubmitForm(bl_bcjl, medicalRecord.ID);
                    }
                    else
                    {
                        bl_bcjl.dzbl_id = medicalRecord.dzbl_id;
                        bl_bcjlSave(bl_bcjl, Entity);
                    }
                    break;
                /// 知情文书保存
                case "3":
                    var bl_zqws = new bl_zqwsEntity();
                    bl_zqws.Id = medicalRecord.ID;
                    bl_zqws.CreatorCode = medicalRecord.CreatorCode;
                    bl_zqws.ksdm = medicalRecord.ksdm;
                    bl_zqws.CreateTime = DateTime.Now;
                    bl_zqws.OrganizeId = medicalRecord.OrganizeId;
                    bl_zqws.blxtml = medicalRecord.blxtml;
                    bl_zqws.mbbh = medicalRecord.mbbh;
                    bl_zqws.zyh = medicalRecord.zyh;
                    bl_zqws.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_zqws.CreateTime = DateTime.Now;
                    bl_zqws.zt = "1";
                    if (Entity == null)
                    {
                        /// 知情文书编辑保存
                        _bl_zqwsRepo.SubmitForm(bl_zqws, medicalRecord.ID);
                    }
                    else
                    {
                        bl_zqws.dzbl_id = medicalRecord.dzbl_id;
                        bl_ylwsSave(bl_zqws, Entity);
                    }

                    break;
                /// 护理记录保存
                case "4":
                    var bl_hljl = new bl_hljlEntity();
                    bl_hljl.Id = medicalRecord.ID;
                    bl_hljl.CreatorCode = medicalRecord.CreatorCode;
                    bl_hljl.ksdm = medicalRecord.ksdm;
                    bl_hljl.OrganizeId = medicalRecord.OrganizeId;
                    bl_hljl.blxtml = medicalRecord.blxtml;
                    bl_hljl.mbbh = medicalRecord.mbbh;
                    bl_hljl.zyh = medicalRecord.zyh;
                    bl_hljl.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_hljl.CreateTime = DateTime.Now;
                    bl_hljl.zt = "1";
                    if (Entity == null)
                    {
                        /// 护理记录编辑保存
                        _bl_hljlRepo.SubmitForm(bl_hljl, medicalRecord.ID);
                    }
                    else
                    {
                        bl_hljl.dzbl_id = medicalRecord.dzbl_id;
                        bl_hljlSave(bl_hljl, Entity);
                    }

                    break;
                /// 病案首页保存
                case "5":
                    var bl_basy = new BlbasyEntity();
                    bl_basy.Id = medicalRecord.ID;
                    bl_basy.CreatorCode = medicalRecord.CreatorCode;
                    bl_basy.ksdm = medicalRecord.ksdm;
                    bl_basy.OrganizeId = medicalRecord.OrganizeId;
                    bl_basy.blxtml = medicalRecord.blxtml;
                    bl_basy.mbbh = medicalRecord.mbbh;
                    bl_basy.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_basy.CreateTime = DateTime.Now;
                    bl_basy.zt = "1";
                    if (Entity == null)
                    {
                        /// 病案首页编辑保存
                        _BlbasyRepo.SubmitForm(bl_basy, medicalRecord.ID);
                    }
                    else
                    {
                        bl_basy.dzbl_id = medicalRecord.dzbl_id;
                        bl_basySave(bl_basy, Entity);
                    }

                    break;
                case "6": //康复
                    bl_patrecordsEntity bl_kf = new bl_patrecordsEntity();
                    bl_kf.Id = medicalRecord.ID;
                    bl_kf.CreatorCode = medicalRecord.CreatorCode;
                    bl_kf.ksdm = medicalRecord.ksdm;
                    bl_kf.OrganizeId = medicalRecord.OrganizeId;
                    bl_kf.blxtml = medicalRecord.blxtml;
                    bl_kf.mbbh = medicalRecord.mbbh;
                    bl_kf.zyh = medicalRecord.zyh;
                    bl_kf.blxtmc_yj = medicalRecord.blxtmc_yj;
                    bl_kf.CreateTime = DateTime.Now;
                    bl_kf.zt = "1";
                    bl_kf.bllx = medicalRecord.bllx;
                    if (Entity == null)
                    {
                        /// 护理记录编辑保存
                        _bl_patrecordsRepo.SubmitForm(bl_kf, medicalRecord.ID);
                    }
                    else
                    {
                        bl_kf.dzbl_id = medicalRecord.dzbl_id;
                        bl_patrecordsSave(bl_kf, Entity);
                    }

                    break;
            }

        }



        public void bl_ryblSave(bl_ryblEntity bl_rybl, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(bl_rybl);
                db.Insert(Entity);
                db.Commit();
            }
        }
        public void bl_bcjlSave(bl_bcjlEntity bl_bcjl, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(bl_bcjl);
                db.Insert(Entity);
                db.Commit();
            }
        }
        public void bl_hljlSave(bl_hljlEntity bl_hljl, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(bl_hljl);
                db.Insert(Entity);
                db.Commit();
            }
        }
        public void bl_ylwsSave(bl_zqwsEntity bl_zqws, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(bl_zqws);
                db.Insert(Entity);
                db.Commit();
            }
        }
        public void bl_basySave(BlbasyEntity bl_basy, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(bl_basy);
                db.Insert(Entity);
                db.Commit();
            }
        }

        public void bl_patrecordsSave(bl_patrecordsEntity doc, ZymeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(doc);
                db.Insert(Entity);
                db.Commit();
            }
        }
        public void bl_patrecordsSave(bl_patrecordsEntity doc, MzmeddocsrelationEntity Entity)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(doc);
                db.Insert(Entity);
                db.Commit();
            }
        }
        /// <summary>
        /// 病历统一存储
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="bllx"></param>
        /// <param name="mbbh"></param>
        /// <param name="path"></param>
        /// <param name="BLMC"></param>
        /// <param name="user"></param>
        /// <param name="zyh"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public string BL_Save(string orgId, string bllx, string mbbh, string path, string BLMC, OperatorModel user, string zyh, string mzh = null, string doctype = null)
        {
            var medicalRecord = new medicalRecordVO();
            medicalRecord.ID = Guid.NewGuid().ToString();
            medicalRecord.blxtml = path;
            medicalRecord.mbbh = mbbh;
            medicalRecord.zyh = zyh;
            medicalRecord.mzh = mzh;
            medicalRecord.blxtmc_yj = BLMC;
            medicalRecord.CreatorCode = user.UserCode;
            medicalRecord.ksdm = user.DepartmentCode;
            medicalRecord.ksmc = user.DepartmentName;
            medicalRecord.OrganizeId = orgId;
            medicalRecord.bllx = bllx;
            medicalRecord.dzbl_id = "R" + EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("YB_Inp_PatRegInfo.dzbl_id", orgId, "{0:D10}", false);

            //插入病人病历关系表
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                var zyEty = new ZymeddocsrelationEntity();
                zyEty.Id = Guid.NewGuid().ToString();
                zyEty.zyh = zyh;
                zyEty.blmc = BLMC;
                zyEty.bllx = bllx;
                zyEty.blzt = 0;
                zyEty.mbId = mbbh;
                zyEty.IsParent = 0;
                zyEty.blId = medicalRecord.ID;
                zyEty.OrganizeId = orgId;
                zyEty.blrq = DateTime.Now;
                zyEty.CreatorCode = user.UserCode;
                zyEty.ysxm = user.UserName;
                zyEty.ysgh = user.rygh;
                zyEty.CreateTime = DateTime.Now;
                zyEty.zt = "1";
                zyEty.doctype = doctype;
                MedicalRecordSave(medicalRecord, zyEty);
            }
            else if (!string.IsNullOrWhiteSpace(mzh))
            {
                var Entity = new MzmeddocsrelationEntity();
                Entity.Id = Guid.NewGuid().ToString();
                Entity.mzh = mzh;
                Entity.blmc = BLMC;
                Entity.bllx = bllx;
                Entity.blzt = 0;
                Entity.mbId = mbbh;
                Entity.IsParent = 0;
                Entity.blId = medicalRecord.ID;
                Entity.OrganizeId = orgId;
                Entity.blrq = DateTime.Now;
                Entity.CreatorCode = user.UserCode;
                Entity.ysxm = user.UserName;
                Entity.ysgh = user.rygh;
                Entity.CreateTime = DateTime.Now;
                Entity.zt = "1";
                Entity.doctype = doctype;
                MedicalRecordSaveMz(medicalRecord, Entity);
            }


            return medicalRecord.ID;
        }


        public void MedicalRecordSaveMz(medicalRecordVO medicalRecord, MzmeddocsrelationEntity Entity)
        {
            bl_patrecordsEntity blety = new bl_patrecordsEntity();
            blety.Id = medicalRecord.ID;
            blety.CreatorCode = medicalRecord.CreatorCode;
            blety.ksdm = medicalRecord.ksdm;
            blety.OrganizeId = medicalRecord.OrganizeId;
            blety.blxtml = medicalRecord.blxtml;
            blety.mbbh = medicalRecord.mbbh;
            blety.mzh = medicalRecord.mzh;
            blety.blxtmc_yj = medicalRecord.blxtmc_yj;
            blety.CreateTime = DateTime.Now;
            blety.zt = "1";
            blety.bllx = medicalRecord.bllx;
            if (Entity == null)
            {
                _bl_patrecordsRepo.SubmitForm(blety, medicalRecord.ID);
            }
            else
            {
                blety.dzbl_id = medicalRecord.dzbl_id;
                bl_patrecordsSave(blety, Entity);
            }

        }
        /// <summary>
        /// 获取统一存储病历
        /// </summary>
        /// <param name="blid"></param>
        /// <param name="bllx"></param>
        /// <returns></returns>
        public medicalRecordVO GetMedicalRecordbyId(string blid, string bllx)
        {
            var medicalRecordVO = new medicalRecordVO();
            var bl = _bl_patrecordsRepo.GetpatrecordsByID(blid);
            medicalRecordVO.blxtml = bl.blxtml;
            medicalRecordVO.blxtmc_yj = bl.blxtmc_yj;
            medicalRecordVO.IsLock = bl.IsLock;
            medicalRecordVO.mbbh = bl.mbbh;
            medicalRecordVO.LastModifierCode = bl.LastModifierCode;
            return medicalRecordVO;
        }


        public void TbDataSavebyWriter(IList<BlHljlData> data, string ksrq, string jsrq)
        {
            string sql = "";
            string sqlval = "";
            string sqlcols = "";
            if (data.Count > 0)
            {
                sql = @"INSERT INTO [dbo].[bl_hljldata]
([Id],[blId],[jlrq],[tw],[mb],[hx],[xy],[ybhd],[cxxdjc],[xroyx],[hljb],[xzjs],[pbjyxkt],[ycyf],[ddyf]
,[qtjh],[zkhl],[dglb],[hlzd],[nl],[wy],[bqhlcontent],[hsqm],[zt],[CreateTime],[CreatorCode],[OrganizeId]) ";
                //                sqlval = @"select @Id,@blId,@jlrq,@tw,@mb,@hx,@xy,@ybhd,@cxxdjc,@xroyx,@hljb,@xzjs,@pbjyxkt,@ycyf,@ddyf
                //,@qtjh,@zkhl,@dglb,@hlzd,@nl,@wy,@bqhlcontent,@hsqm,@zt,@CreateTime,@CreatorCode,@OrganizeId ";
                foreach (var item in data)
                {
                    if (!string.IsNullOrWhiteSpace(sqlval))
                    {
                        sqlval += " union all";
                    }
                    sqlval = "select '" + item.Id + "','" + item.blId + "','" + item.jlrq + "','" + item.tw + "','" + item.mb + "','" + item.hx + "','"
                         + item.xy + "','" + item.ybhd + "','" + item.cxxdjc + "','" + item.xroyx + "','" + item.hljb + "','" + item.xzjs + "','" + item.pbjyxkt + "','"
                          + item.ycyf + "','" + item.ddyf + "','" + item.qtjh + "','" + item.zkhl + "','" + item.dglb + "','" + item.hlzd + "','" + item.nl + "','"
                           + item.wy + "','" + item.bqhlcontent + "','" + item.hsqm + "','1',getdate(),'" + item.CreatorCode + "','" + item.OrganizeId + "' ";
                }

                ExecuteSqlCommand(sql + sqlval);
            }
        }

        public List<LisReportSqdhValueVo> GetLisSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj)
        {
            var sql = "";
            var par = new List<SqlParameter>();
            if (type == "mz")
            {
                sql = @" select OrganizeId,xm,lissqdh,sqdh,
stuff((select distinct ','+u.ztmc from xt_jz t 
join xt_cf y on t.jzId=y.jzId and t.OrganizeId=y.OrganizeId
join xt_cfmx u on y.cfId=u.cfId and y.OrganizeId=u.OrganizeId
where  t.OrganizeId=q.OrganizeId and q.lissqdh=y.cfh and q.sqdh=y.sqdh
for xml path('')),1,1,'') as ztmc,syncStatus,max(sqsj) sqsj from(select distinct a.OrganizeId,a.xm,b.cfh lissqdh,b.sqdh,c.ztId,c.ztmc,
 case d.sqdzt  when '0' then '已申请' when '1' then '已接收' when '2' then '已完成' else '待申请' end syncStatus,
CONVERT(varchar(100), b.CreateTime, 20) sqsj,row_number() over(partition by c.ztmc order by b.CreateTime desc) rn
 from xt_jz a
join xt_cf b on a.jzId=b.jzId and a.OrganizeId=b.OrganizeId
join xt_cfmx c on b.cfId=c.cfId and b.OrganizeId=c.OrganizeId
join [NewtouchHIS_Sett].dbo.mz_cf d on d.cfh=b.cfh and d.OrganizeId=b.OrganizeId 
where  b.cflx='4' and a.mzh=@zymzh  and a.OrganizeId=@orgId and a.zt=1";
                if (!string.IsNullOrEmpty(ztmc))
                {
                    sql += " and c.ztmc like @ztmc ";
                    par.Add(new SqlParameter("@ztmc", "%" + ztmc + "%"));
                }
                if (!string.IsNullOrEmpty(kssj))
                {
                    sql += " and b.CreateTime>=@kssj ";
                    par.Add(new SqlParameter("@kssj", kssj));
                }
                if (!string.IsNullOrEmpty(jssj))
                {
                    sql += " and b.CreateTime<=@jssj ";
                    par.Add(new SqlParameter("@jssj", jssj));
                }
                sql += ") q where q.rn=1 group by OrganizeId,xm,lissqdh,sqdh,syncStatus order by sqsj desc";
            }
            else
            {
                sql = @"select OrganizeId,xm,lissqdh,sqdh,
stuff((select distinct ','+b.ztmc from Newtouch_CIS.dbo.zy_lsyz b
where b.yzh=c.lissqdh and b.OrganizeId=c.OrganizeId
for xml path('')),1,1,'') as ztmc,
syncstatus,max(sqsj) sqsj  FROM (select distinct OrganizeId,hzxm xm,yzh lissqdh,sqdh,ztId,ztmc,
case syncStatus when '0' then '已申请' when '1' then '已接收' when '2' then '已完成' else '待申请' end syncStatus
,CONVERT(varchar(100), a.CreateTime, 20) sqsj,row_number() over(partition by a.ztmc order by a.CreateTime desc) rn  from Newtouch_CIS.dbo.zy_lsyz a
where  yzlx='6' and a.zyh=@zymzh  and a.OrganizeId=@orgId and a.zt=1";
                if (!string.IsNullOrEmpty(ztmc))
                {
                    sql += " and ztmc like @ztmc ";
                    par.Add(new SqlParameter("@ztmc", "%" + ztmc + "%"));
                }
                if (!string.IsNullOrEmpty(kssj))
                {
                    sql += " and CreateTime>=@kssj ";
                    par.Add(new SqlParameter("@kssj", kssj));
                }
                if (!string.IsNullOrEmpty(jssj))
                {
                    sql += " and CreateTime<=@jssj ";
                    par.Add(new SqlParameter("@jssj", jssj));
                }
                sql += ") c WHERE rn = 1 group by OrganizeId ,xm,lissqdh,sqdh,syncstatus order by sqsj desc";
            }

            par.Add(new SqlParameter("@zymzh", zymzh));
            par.Add(new SqlParameter("@orgId", orgId));

            return this.FindList<LisReportSqdhValueVo>(sql, par.ToArray());
            //return this.FindList<LisReportSqdhValueVo>(sql, new[]
            //{ new SqlParameter("@zymzh", zymzh), new SqlParameter("@orgId", orgId), new SqlParameter("@ztmc","%" + ztmc + "%"),new SqlParameter("@kssj",kssj),new SqlParameter("@jssj",jssj) });

        }

        public List<PacsReportSqdhValueVo> GetPacsSqdhData(string orgId, string zymzh, string type, string ztmc, string kssj, string jssj)
        {
            var sql = ""; var par = new List<SqlParameter>();
            if (type == "mz")
            {
                sql = @" select OrganizeId,xm,lissqdh,sqdh,
stuff((select distinct ','+u.ztmc from xt_jz t 
join xt_cf y on t.jzId=y.jzId and t.OrganizeId=y.OrganizeId
join xt_cfmx u on y.cfId=u.cfId and y.OrganizeId=u.OrganizeId
where  t.OrganizeId=c.OrganizeId and c.lissqdh=y.cfh and c.sqdh=y.sqdh
for xml path('')),1,1,'')  as ztmc,
syncstatus,max(sqsj) sqsj  FROM (select distinct a.OrganizeId,a.xm,b.cfh lissqdh,b.sqdh,c.ztId,c.ztmc,
 case d.sqdzt  when '0' then '已申请' when '1' then '已接收' when '2' then '已完成' else '待申请' end syncStatus
 ,CONVERT(varchar(100), a.CreateTime, 20) sqsj,row_number() over(partition by c.ztmc order by a.CreateTime desc) rn 
 from xt_jz a
join xt_cf b on a.jzId=b.jzId and a.OrganizeId=b.OrganizeId
join xt_cfmx c on b.cfId=c.cfId and b.OrganizeId=c.OrganizeId
join [NewtouchHIS_Sett].dbo.mz_cf d on d.cfh=b.cfh and d.OrganizeId=b.OrganizeId 
where  b.cflx='5' and a.mzh=@zymzh  and a.OrganizeId=@orgId and a.zt=1 and d.zt='1'";
                if (!string.IsNullOrEmpty(ztmc))
                {
                    sql += " and c.ztmc like @ztmc ";
                    par.Add(new SqlParameter("@ztmc", "%" + ztmc + "%"));
                }
                if (!string.IsNullOrEmpty(kssj))
                {
                    sql += " and b.CreateTime>=@kssj ";
                    par.Add(new SqlParameter("@kssj", kssj));
                }
                if (!string.IsNullOrEmpty(jssj))
                {
                    sql += " and b.CreateTime<=@jssj ";
                    par.Add(new SqlParameter("@jssj", jssj));
                }
                sql += ") c WHERE rn = 1 group by OrganizeId,xm,lissqdh,sqdh,syncStatus order by sqsj desc";
            }
            else
            {
                sql = @"select OrganizeId,xm,lissqdh,sqdh,
stuff((select distinct ','+b.ztmc from Newtouch_CIS.dbo.zy_lsyz b
where b.yzh=c.lissqdh and b.OrganizeId=c.OrganizeId
for xml path('')),1,1,'') as ztmc,
syncstatus,max(sqsj) sqsj FROM (select OrganizeId,hzxm xm,yzh lissqdh,sqdh,ztId,ztmc,
case syncStatus when '0' then '已申请' when '1' then '已接收' when '2' then '已完成' else '待申请' end syncStatus,CONVERT(varchar(100), a.CreateTime, 20) sqsj,row_number() over(partition by a.ztmc order by a.CreateTime desc) rn  
from Newtouch_CIS.dbo.zy_lsyz a
where  yzlx='7' and a.zyh=@zymzh  and a.OrganizeId=@orgId and a.zt=1";
                if (!string.IsNullOrEmpty(ztmc))
                {
                    sql += " and ztmc like @ztmc ";
                    par.Add(new SqlParameter("@ztmc", "%" + ztmc + "%"));
                }
                if (!string.IsNullOrEmpty(kssj))
                {
                    sql += " and CreateTime>=@kssj ";
                    par.Add(new SqlParameter("@kssj", kssj));
                }
                if (!string.IsNullOrEmpty(jssj))
                {
                    sql += " and CreateTime<=@jssj ";
                    par.Add(new SqlParameter("@jssj", jssj));
                }
                sql += ") c WHERE rn = 1 group by OrganizeId ,xm,lissqdh,sqdh,syncstatus order by sqsj desc";
            }

            par.Add(new SqlParameter("@zymzh", zymzh));
            par.Add(new SqlParameter("@orgId", orgId));

            return this.FindList<PacsReportSqdhValueVo>(sql, par.ToArray());

        }

        public List<PacsReportSqdhValueMxVo> GetPacsSqdhMxData(string sqdh, string OrganizeId)
        {
            var sql = ""; var par = new List<SqlParameter>();
            sql = @"select (report.DiagDesc+report.DiagName )jcjg
  from Newtouch_Interface..PACS_RIS_REPORT as report
  where report.ReqNO = @sqdh
   and report.OrganizeId = @OrganizeId
   and report.zt = '1'
   and report.MzZyBz = '2'--2סԺ";
			

			par.Add(new SqlParameter("@sqdh", sqdh));
            par.Add(new SqlParameter("@OrganizeId", OrganizeId));

            return this.FindList<PacsReportSqdhValueMxVo>(sql, par.ToArray());
        }

        public List<LisReportSqdhValueMxVo> GetLisSqdhMxData(string zyh, string lissqdh, string organizeId)
        {
            var sql = ""; var par = new List<SqlParameter>();
            sql = @"select xmzwmc as xmmc,/*项目名称*/
	   jyjg,/*检验结果*/
	   gdbj,/*高低标记*/
	   ckz as ckzfw,/*参考值范围*/
	   xmdw as dw/*单位*/
  from Newtouch_Interface..Lis_Report_ZY
 where zyh = @zyh
   and OrganizeId = @organizeId
   and sqdh = @lissqdh
order by xh";
			
            par.Add(new SqlParameter("@zyh", zyh));
            par.Add(new SqlParameter("@lissqdh", lissqdh));
            par.Add(new SqlParameter("@organizeId", organizeId));

            return this.FindList<LisReportSqdhValueMxVo>(sql, par.ToArray());

        }

        public List<AdviceListGridVO> AdviceGridView(Pagination pagination, AdviceListRequestVO req)
        {
            if (req == null || req.zyh == null)
            {
                throw new FailedException("缺少查询条件");
            }
            var rtn = new List<AdviceListGridVO>();
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"select * from(
select CONVERT(varchar(10),'长') yzlb,yz.yzlx,yz.yzzt, yz.kssj kssj ,ry.Name ysmc,yz.yznr yznr, yz.zh zh,yz.tzsj tzsj,yz.sl yl,yz.dw yldw,ypyf.yfmc yf,yzpc.yzpcmcsm pc,yz.ztnr yzt
  from  Newtouch_CIS.dbo.zy_cqyz yz with(nolock) left join NewtouchHIS_Base.dbo.Sys_Staff ry with(nolock) on yz.ysgh=ry.gh and yz.OrganizeId=ry.OrganizeId  left join NewtouchHIS_Base.dbo.xt_yzpc yzpc with(nolock) on yzpc.yzpcCode = yz.pcCode and yz.OrganizeId=yzpc.OrganizeId left join [NewtouchHIS_Base]..[V_S_xt_ypyf] ypyf with(nolock) on yz.ypyfdm = ypyf.yfCode
 where zyh=@zyh and yz.OrganizeId=@orgId and kssj > @kssj and kssj < DATEADD(dd, 1, @jssj) and yz.yzzt!='5' and yz.zt='1'
union all
select CONVERT(varchar(10),'临') yzlb,yz.yzlx,yz.yzzt, yz.kssj kssj ,ry.Name ysmc,yz.yznr yznr, yz.zh zh,yz.zxsj tzsj,yz.sl yl,yz.dw yldw,ypyf.yfmc yf,pcCode,yz.ztnr yzt
  from  Newtouch_CIS.dbo.zy_lsyz yz with(nolock) left join NewtouchHIS_Base.dbo.Sys_Staff ry with(nolock) on yz.ysgh=ry.gh and yz.OrganizeId=ry.OrganizeId left join NewtouchHIS_Base.dbo.xt_yzpc yzpc with(nolock) on yzpc.yzpcCode = yz.pcCode and yz.OrganizeId=yzpc.OrganizeId left join [NewtouchHIS_Base]..[V_S_xt_ypyf] ypyf with(nolock) on yz.ypyfdm = ypyf.yfCode
 where zyh=@zyh and yz.OrganizeId=@orgId and kssj > @kssj and kssj < DATEADD(dd, 1, @jssj) and yz.yzzt!='5' and yz.zt='1' 
) as a ");
            par.Add(new SqlParameter("@orgId", req.orgId));
            par.Add(new SqlParameter("@zyh", req.zyh));
            par.Add(new SqlParameter("@kssj", req.kssj));
            par.Add(new SqlParameter("@jssj", req.jssj));
            return this.QueryWithPage<AdviceListGridVO>(sqlstr.ToString(), pagination, par.ToArray(), false).ToList();
        }

        //获取住院病人基本信息元素
        public blzybrjbxxVO GetBlZybrjbxx(string OrgId, string zyh, string user, bool newbz = false)
        {
            try
            {
                SqlParameter[] para = new SqlParameter[] {
                            new SqlParameter("@zyh", zyh),
                            new SqlParameter("@organizeId", OrgId),
                            new SqlParameter("@user", user)
                };
                string sql = "";
                if(newbz)
                     sql = "exec  usp_bl_zybrjbxxV2 @zyh=@zyh ,@organizeId=@organizeId";
                else
                     sql = "exec  usp_bl_zybrjbxx @zyh=@zyh ,@organizeId=@organizeId,@user=@user";

                return this.FirstOrDefault<blzybrjbxxVO>(sql, para.ToArray());

            }
            catch (Exception ex)
            {
                throw new FailedException("获取病历元素异常，" + ex.Message);
            }
        }

        public int updateLock(string OrgId, string blid, string user)//修改锁定状态
        {
            SqlParameter[] para = new SqlParameter[] {
                            new SqlParameter("@blid", blid),
                            new SqlParameter("@organizeId", OrgId),
                            new SqlParameter("@user", user)
                };

            string sql = "update Newtouch_EMR..bl_hljl set IsLock='0',LastModifierCode=@user where Id=@blid and OrganizeId=@organizeId";

            return this.ExecuteSqlCommand(sql, para.ToArray());
        }
        public void BLJG_Save(List<bl_ysjgnrEntity> bl_ysjgnr)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                foreach (var Item in bl_ysjgnr)
                {
                    Item.Create(true);
                    Item.zt = "1";
                    db.Insert(Item);
                }
                db.Commit();
            }
        }
        public string BLJG_Delete(string blid,string orgId)
        {
            try
            {
                SqlParameter[] para = new SqlParameter[] {
                            new SqlParameter("@blid", blid),
                            new SqlParameter("@organizeId", orgId)
                };
                string sql = "delete from Newtouch_EMR..bl_ysjgnr where blid=@blid and OrganizeId=@organizeId";
                this.ExecuteSqlCommand(sql, para.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
    }
}
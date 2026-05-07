using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.HospitalizationManage;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.PatientManage;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.PatientManage;
using Newtouch.HIS.Domain.ValueObjects.YibaoInterfaceManage;
using Newtouch.HIS.Proxy.guian;
using Newtouch.HIS.Proxy.guian.DTO;
using Newtouch.HIS.Sett.Request.Patient;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.HIS.DomainServices
{
    public class PatientBasicInfoDmnService : DmnServiceBase, IPatientBasicInfoDmnService
    {
        private readonly ISysPatientBasicInfoRepo _SysPatBasicInfoRepository;
        private readonly IHospMultiDiagnosisRepo _HosMoreDiagnoseRepository;
        private readonly ISysDiagnosisRepo _SysDiagnoseRepo;
        private readonly ISysCardRepo _SysCardRepository;
        private readonly IHospPatientBasicInfoRepo _HosPatBasicInfoRepository;
        private readonly ISysPatientAccountRepo _SysPatAccRepository;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysPatientBasicInfoRepo _sysPatientBasicInfoRepo;
        private readonly ISysPatientNatureRepo _sysPatientNatureRepo;


        #region 贵安新农合
        private readonly ITTCataloguesComparisonDmnService _ttCataloguesComparisonDmnService;

        private List<TTCataloguesComparisonDetailEntity> allTT;

        #endregion
        public PatientBasicInfoDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }



        /// <summary>
        /// 根据病人内码找病人基本信息
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public OutpatAccInfoDto GetPatInfoByPatid(int patid, string orgId)
        {
            var strSql = new StringBuilder(@"SELECT  A.patid ,
                            A.blh ,
                            A.xm ,
                            A.xb ,
                            A.csny ,
                            A.zjlx ,
                            A.zjh ,
                            CAST(FLOOR(DATEDIFF(DY, A.csny, GETDATE()) / 365.25) AS INT) nl ,
                            xz.brxz ,
                            xz.brxzmc ,
                            xz.brxzbh ,
                            A.phone ,
                            A.hf
                    FROM    [dbo].[xt_brjbxx] (NOLOCK) A
                            LEFT JOIN dbo.xt_card (NOLOCK) c ON c.patid = A.patid
                                                                 AND A.OrganizeId = c.OrganizeId
                            LEFT JOIN xt_brxz (NOLOCK) xz ON xz.brxz = c.brxz
                                                             AND xz.OrganizeId = c.OrganizeId
                    WHERE   A.patid = @patid
                            AND A.OrganizeId = @OrganizeId;");
            DbParameter[] par =
            {
                new SqlParameter("@patid", patid),
                new SqlParameter("@OrganizeId", orgId)

            };
            return FirstOrDefault<OutpatAccInfoDto>(strSql.ToString(), par.ToArray());
        }

        /// <summary>
        /// 同时保存病人和卡信息
        /// </summary>
        /// <returns></returns>
        public void SavePatBasicCardInfo(SysHosBasicInfoVO vo, string orgId, string CreatorCode)
        {
            if (string.IsNullOrWhiteSpace(vo.kh) && vo.yktbz == "yktregister")
            {
                throw new FailedException("缺少卡号");
            }
            if (string.IsNullOrWhiteSpace(vo.blh))
            {
                throw new FailedException("缺少病历号");
            }
            if (string.IsNullOrWhiteSpace(vo.xm))
            {
                throw new FailedException("缺少姓名");
            }
            if (string.IsNullOrWhiteSpace(vo.brxz) && vo.yktbz == "yktregister")
            {
                throw new FailedException("缺少病人性质");
            }
            int newPatId = 0;
            SysCardEntity newCardEntity = null;
            SysPatientBasicInfoEntity Oldpatient = null, patiententity = null;
            SysxtbrjbxxlogEntity xtbrjbxxlog = null;
            //修改病人信息
            //根据证件号判断病人基本信息
            //string zjlx = ((EnumZJLX)(Convert.ToInt32(vo.zjlx))).GetDescription();
            var validatepat = _SysPatBasicInfoRepository.IQueryable().Where(p => p.zjlx == vo.zjlx && p.zjh == vo.zjh && p.OrganizeId == orgId && p.zt == "1" && p.zjh!=null);
            if ((vo.patid ?? 0) > 0 && vo.yktbz!= "yktcardregister")
            {
                if (!string.IsNullOrWhiteSpace(vo.zjh))
                {
                    if (validatepat.Count() > 1)
                    {
                        throw new FailedException("证件号为：" + vo.zjh + "的病人已存在");
                    }
                    if (validatepat.Count()==1)
                    {
                        if (validatepat.FirstOrDefault().patid != vo.patid)
                        {
                            throw new FailedException("证件号为：" + vo.zjh + "的病人一卡通信息已存在");
                        }
                    }
                    
                }
                //修改基本信息
                patiententity = _SysPatBasicInfoRepository.FindEntity(vo.patid);
                if (patiententity == null)
                {
                    throw new FailedException("患者信息未找到");
                }
               
                Oldpatient = patiententity.Clone();

            }
            else
            {
                //新增
                if (string.IsNullOrWhiteSpace(vo.cardtype))
                {
                    switch (vo.brxz)
                    {
                        //医保
                        case "2":
                            vo.cardtype = ((int)EnumCardType.YBJYK).ToString();
                            break;
                        //新农合
                        case "3":
                            vo.cardtype = ((int)EnumCardType.XNHJYK).ToString();
                            break;
                        default:
                            vo.cardtype = ((int)EnumCardType.XNK).ToString();
                            break;
                    }
                }
                if (validatepat.Count() > 1)
                {
                    throw new FailedException("证件号为：" + vo.zjh + "的一卡通信息存在多条");
                }
                if (validatepat.Count() == 1)
                {
                    patiententity = validatepat.FirstOrDefault();
                    vo.yktbz = "yktcardregister";//新增卡信息同时更新一卡通信息
                    newPatId = patiententity.patid;
                    var isexitCard = _SysCardRepository.GetCardEntity(patiententity.patid, vo.cardtype, orgId);
                    if (isexitCard != null&&isexitCard.brxz!=((int)EnumBrxz.zf).ToString())
                    {
                        throw new FailedException("证件号为：" + vo.zjh + "的" + isexitCard.CardTypeName + "信息已经存在");
                    }
                }
                newCardEntity = _SysCardRepository.GetCardEntity(vo.cardtype, vo.kh, orgId);
                if (vo.cardtype != ((int)EnumCardType.YBJYK).ToString() && vo.cardtype != ((int)EnumCardType.XNHJYK).ToString())
                {

                    //验卡号重复使用
                    if (newCardEntity != null)
                    {
                        throw new FailedException("卡号已存在");
                    }
                }

                if (newCardEntity == null)
                {
                    newCardEntity = new SysCardEntity();
                    newCardEntity.CardNo = vo.kh;
                    newCardEntity.CardType = vo.cardtype;
                    newCardEntity.CardTypeName = ((EnumCardType)(Convert.ToInt32(vo.cardtype))).GetDescription();
                    newCardEntity.grbh = vo.grbh;
                    newCardEntity.xzlx = vo.xzlx;
                    newCardEntity.cbdbm = vo.cbdbm;
                    newCardEntity.cblb = vo.cblb;
                    newCardEntity.brxz = vo.brxz;
                    newCardEntity.zt = "1";
                    newCardEntity.hzxm = vo.xm;
                    newCardEntity.OrganizeId = orgId;
                    newCardEntity.accountattr = vo.accountattr;
                }
                
                if (patiententity == null)
                {
                    //病历号重复使用判断
                    var blhExist = _SysPatBasicInfoRepository.GetInfoByblh(orgId, vo.blh.Trim());
                    if (blhExist != null)
                    {
                        //新增时判断病历号重复
                        throw new FailedException("病历号已存在");
                    }
                    patiententity = new SysPatientBasicInfoEntity();
                    patiententity.blh = vo.blh;

                }
            }
            //通过身份证获取真实出生日期和性别 
            var BirthdayAdnSix = vo.zjlx== ((int)EnumZJLX.sfz).ToString()? CommmHelper.GetBirthdayAdnSix(vo.zjh):null;
            var sd = ((int)EnumZJLX.sfz).ToString();
            //修改病人基本信息
            patiententity.xm = vo.xm;
            patiententity.zjlx = vo.zjlx;
            patiententity.zjh = vo.zjh;
            patiententity.csny = BirthdayAdnSix == null ? Convert.ToDateTime(vo.csny) : Convert.ToDateTime(BirthdayAdnSix.Item1);
            patiententity.xb = vo.xb;
            patiententity.wechat = vo.wechat;
            patiententity.zjlxfs = vo.zjlxfs?? patiententity.zjlxfs;
            patiententity.phone = vo.phone?? patiententity.phone;
            patiententity.dh = vo.dh ?? patiententity.dh;
            patiententity.email = vo.email?? patiententity.email;
            patiententity.cs_sheng = vo.cs_sheng?? patiententity.cs_sheng;
            patiententity.cs_shi = vo.cs_shi?? patiententity.cs_shi;
            patiententity.cs_xian = vo.cs_xian?? patiententity.cs_xian;
            patiententity.cs_dz = vo.cs_dz ?? patiententity.cs_dz;
            patiententity.xian_sheng = vo.xian_sheng ?? patiententity.xian_sheng;
            patiententity.xian_shi = vo.xian_shi?? patiententity.xian_shi;
            patiententity.xian_xian = vo.xian_xian?? patiententity.xian_xian;
            patiententity.xian_dz = vo.xian_dz?? patiententity.xian_dz;
            patiententity.hu_sheng = vo.hu_sheng?? patiententity.hu_sheng;
            patiententity.hu_shi = vo.hu_shi ?? patiententity.hu_shi;
            patiententity.hu_xian = vo.hu_xian ?? patiententity.hu_xian;
            patiententity.hu_dz = vo.hu_dz ?? patiententity.hu_dz;
            patiententity.py = vo.py ?? patiententity.py;
            patiententity.xl = vo.xl ?? patiententity.xl;
            patiententity.gj = vo.gj.ToString()?? patiententity.gj;
            patiententity.hf = vo.hf ?? patiententity.hf;
            patiententity.zt = "1";
            patiententity.gj = vo.gj2 ?? patiententity.gj;
            patiententity.mz = vo.mz2 ?? patiententity.mz;
            patiententity.jg = vo.jg ?? patiententity.jg;
            patiententity.OrganizeId = orgId;
            patiententity.brly = vo.brly ?? patiententity.brly;
            patiententity.jjlldh = vo.jjlldh ?? patiententity.jjlldh;
            patiententity.jjllr = vo.jjllr ?? patiententity.jjllr;
            patiententity.jjllrgx = vo.jjllrgx ?? patiententity.jjllrgx;
            patiententity.zy = vo.zy?? patiententity.zy;
            patiententity.dwmc = vo.dwmc ?? patiententity.dwmc;
            patiententity.dwdz = vo.dwdz ?? patiententity.dwdz;
            patiententity.bz = vo.bz ?? patiententity.bz;
            patiententity.dybh = vo.dybh ?? patiententity.dybh;
            patiententity.jjlxr_sheng = vo.jjlxr_sheng?? patiententity.jjlxr_sheng;
            patiententity.jjlxr_shi = vo.jjlxr_shi ?? patiententity.jjlxr_shi;
            patiententity.jjlxr_xian = vo.jjlxr_xian?? patiententity.jjlxr_xian;
            patiententity.jjlxr_dz = vo.jjlxr_dz?? patiententity.jjlxr_dz;
            patiententity.xian_yb = vo.xian_yb ?? patiententity.xian_yb;
            patiententity.dwyb = vo.dwyb ?? patiententity.dwyb;
            patiententity.dwdh = vo.dwdh ?? patiententity.dwdh;
            patiententity.hu_yb = vo.hu_yb ?? patiententity.hu_yb;
            patiententity.cs_yb = vo.cs_yb ?? patiententity.cs_yb;
            patiententity.jbs = vo.jbs ?? patiententity.jbs;
            patiententity.gms = vo.gms ?? patiententity.gms;

            xtbrjbxxlog = new SysxtbrjbxxlogEntity();
            xtbrjbxxlog.xm = vo.xm;
            xtbrjbxxlog.zjlx = vo.zjlx;
            xtbrjbxxlog.zjh = vo.zjh;
            xtbrjbxxlog.csny = BirthdayAdnSix == null ? Convert.ToDateTime(vo.csny) : Convert.ToDateTime(BirthdayAdnSix.Item1);
            xtbrjbxxlog.xb = vo.xb;
            xtbrjbxxlog.wechat = vo.wechat;
            xtbrjbxxlog.zjlxfs = vo.zjlxfs;
            xtbrjbxxlog.phone = vo.phone;
            xtbrjbxxlog.dh = vo.dh;
            xtbrjbxxlog.email = vo.email;
            xtbrjbxxlog.cs_sheng = vo.cs_sheng;
            xtbrjbxxlog.cs_shi = vo.cs_shi;
            xtbrjbxxlog.cs_xian = vo.cs_xian;
            xtbrjbxxlog.cs_dz = vo.cs_dz;
            xtbrjbxxlog.xian_sheng = vo.xian_sheng;
            xtbrjbxxlog.xian_shi = vo.xian_shi;
            xtbrjbxxlog.xian_xian = vo.xian_xian;
            xtbrjbxxlog.xian_dz = vo.xian_dz;
            xtbrjbxxlog.hu_sheng = vo.hu_sheng;
            xtbrjbxxlog.hu_shi = vo.hu_shi;
            xtbrjbxxlog.hu_xian = vo.hu_xian;
            xtbrjbxxlog.hu_dz = vo.hu_dz;
            xtbrjbxxlog.py = vo.py;
            xtbrjbxxlog.xl = vo.xl;
            xtbrjbxxlog.gj = vo.gj.ToString();
            xtbrjbxxlog.hf = vo.hf;
            xtbrjbxxlog.zt = "1";
            xtbrjbxxlog.gj = vo.gj2;
            xtbrjbxxlog.mz = vo.mz2;
            xtbrjbxxlog.jg = vo.jg;
            xtbrjbxxlog.OrganizeId = orgId;
            xtbrjbxxlog.brly = vo.brly;
            xtbrjbxxlog.jjlldh = vo.jjlldh;
            xtbrjbxxlog.jjllr = vo.jjllr;
            xtbrjbxxlog.jjllrgx = vo.jjllrgx;
            xtbrjbxxlog.zy = vo.zy;
            xtbrjbxxlog.dwmc = vo.dwmc;
            xtbrjbxxlog.dwdz = vo.dwdz;
            xtbrjbxxlog.bz = vo.bz;
            xtbrjbxxlog.dybh = vo.dybh;
            xtbrjbxxlog.jjlxr_sheng = vo.jjlxr_sheng;
            xtbrjbxxlog.jjlxr_shi = vo.jjlxr_shi;
            xtbrjbxxlog.jjlxr_xian = vo.jjlxr_xian;
            xtbrjbxxlog.jjlxr_dz = vo.jjlxr_dz;
            xtbrjbxxlog.xian_yb = vo.xian_yb;
            xtbrjbxxlog.dwyb = vo.dwyb;
            xtbrjbxxlog.dwdh = vo.dwdh;
            xtbrjbxxlog.hu_yb = vo.hu_yb;
            xtbrjbxxlog.cs_yb = vo.cs_yb;
            xtbrjbxxlog.jbs = vo.jbs;
            xtbrjbxxlog.gms = vo.gms;
            xtbrjbxxlog.blh = vo.blh;


            #region 数据比较
            System.Reflection.PropertyInfo[] mPi = typeof(SysPatientBasicInfoEntity).GetProperties();
            var data_name = "";      //差别字段名
            var datavalue_old = "";  //老字段值
            var datavalue_new = "";  //新字段值
            var sqlvalue = "";
            if (Oldpatient != null)
            {
                for (int i = 0; i < mPi.Length; i++)
                {
                    System.Reflection.PropertyInfo pi = mPi[i];
                    var get_old = pi.GetValue(Oldpatient, null) == null ? "" : pi.GetValue(Oldpatient, null).ToString().Trim();
                    var get_new = pi.GetValue(patiententity, null) == null ? "" : pi.GetValue(patiententity, null).ToString().Trim();
                    sqlvalue += get_new == "" ? "NULL," : "'" + get_new + "',";
                    if (get_old != get_new)
                    {
                        //差别数据统计
                        data_name += (pi.Name == "" ? "NULL" : pi.Name) + ",";
                        datavalue_old += (get_old == "" ? "NULL" : get_old) + ",";
                        datavalue_new += (get_new == "" ? "NULL" : get_new) + ",";
                    }

                }

                if (data_name != "")
                {
                    var CreatorCodes = CreatorCode;
                    var CreateTime = DateTime.Now;
                    data_name = (data_name != "" ? data_name.Substring(0, data_name.LastIndexOf(",")) : "");
                    datavalue_old = (datavalue_old != "" ? datavalue_old.Substring(0, datavalue_old.LastIndexOf(",")) : "");
                    datavalue_new = (datavalue_new != "" ? datavalue_new.Substring(0, datavalue_new.LastIndexOf(",")) : "");
                    string strSql = string.Format(@"insert into [NewtouchHIS_Sett].[dbo].[xt_brjbxxLOG] " +
                        " select newid(),*,@CreatorCodes,@CreateTime,NULL,NULL,@data_name data_name,@datavalue_old datavalue_old,@datavalue_new datavalue_new from [NewtouchHIS_Sett].[dbo].[xt_brjbxx] with(nolock) where patid=@patid and blh=@blh and zt='1' and OrganizeId=@orgId ");
                    var param = new DbParameter[] {
                            new SqlParameter("@data_name",data_name),
                            new SqlParameter("@datavalue_old",datavalue_old),
                            new SqlParameter("@datavalue_new",datavalue_new),
                            new SqlParameter("@patid",vo.patid),
                            new SqlParameter("@blh",vo.blh),
                            new SqlParameter("@orgId",orgId),
                            new SqlParameter("@CreatorCodes",CreatorCode),
                            new SqlParameter("@CreateTime",CreateTime),
                        };
                    this.ExecuteSqlCommand(strSql, param);
                }
                
                xtbrjbxxlog.data_name = data_name;
                xtbrjbxxlog.datavalue_old = datavalue_old;
                xtbrjbxxlog.datavalue_new = datavalue_new;
            }


            #endregion





            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (Oldpatient != null||vo.yktbz== "yktcardregister")
                {
                    if (vo.isdsfly!="Y")
                    {
                        patiententity.Modify();
                        db.Update(patiententity);
                    }
                }
                else
                {
                    newPatId = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("xt_brjbxx");
                    patiententity.Create(true, newPatId);
                    #region 照顾自助机/小程序接口，获取不到登录用户
                    if (string.IsNullOrEmpty(patiententity.CreatorCode))
                    {
                        patiententity.CreatorCode = "zzjadmin";//如果为空，设置为自助机挂号（接口）
                    }
                    if (string.IsNullOrEmpty(patiententity.CreateTime.ToString()))
                    {
                        patiententity.CreateTime = DateTime.Now;
                    }
                    #endregion
                    db.Insert(patiententity);

                    xtbrjbxxlog.patid = newPatId;
                    xtbrjbxxlog.Create(true);
                    #region 照顾自助机/小程序接口，获取不到登录用户
                    if (string.IsNullOrEmpty(xtbrjbxxlog.CreatorCode))
                    {
                        xtbrjbxxlog.CreatorCode = "zzjadmin";//如果为空，设置为自助机挂号（接口）
                    }
                    if (string.IsNullOrEmpty(xtbrjbxxlog.CreateTime.ToString()))
                    {
                        xtbrjbxxlog.CreateTime = DateTime.Now;
                    }
                    #endregion
                    db.Insert(xtbrjbxxlog);
                }

                if (newCardEntity != null)
                {
                    //新卡
                    newCardEntity.patid = newPatId;
                    newCardEntity.Create(true);
                    #region 照顾自助机/小程序接口，获取不到登录用户
                    if (string.IsNullOrEmpty(newCardEntity.CreatorCode))
                    {
                        newCardEntity.CreatorCode = "zzjadmin";//如果为空，设置为自助机挂号（接口）
                    }
                    if (string.IsNullOrEmpty(newCardEntity.CreateTime.ToString()))
                    {
                        newCardEntity.CreateTime = DateTime.Now;
                    }
                    #endregion
                    db.Insert(newCardEntity);

                    
                }

                try
                {
                    db.Commit();
                }
                catch (Exception e)
                {
                    throw;
                }


                if (Oldpatient != null)
                {
                    //基本信息变更日志
                    AppLogger.WriteEntityChangeRecordLog(Oldpatient, patiententity, SysPatientBasicInfoEntity.GetTableName(), vo.patid.ToString());
                }
            }
        }

        /// <summary>
        /// 住院登记 根据patid获取新登记入院病人基本信息/在院病人住院基本信息
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="result"></param>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        public List<SysHosBasicInfoVO> GetPatBasicCardInfo(string patid, out bool result, string OrganizeId)
        {
            StringBuilder strSql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            inSqlParameterList = new List<SqlParameter>();
            inSqlParameterList = new List<SqlParameter>();
            strSql.AppendFormat(@"SELECT  c.patid ,
                                    c.CardNo AS kh ,
                                    zy.zyh ,
                                    zy.zyh AS zyh2 ,
                                    jb.xm ,
                                    jb.zjh ,
                                    jb.py,
                                    CONVERT(VARCHAR(100), jb.csny, 23) csny ,
                                    jb.xb ,
                                    CAST( (CASE  WHEN zy.zyh IS NULL THEN FLOOR(datediff(DY,jb.csny,getdate())/365.25) ELSE zy.nl END) as SMALLINT) nl,
                                    jb.brly ,
                                    jb.blh ,
                                    jb.dh ,
                                    jb.hf ,
                                    staff2.staffName jkjlmc,
                                    zy.cw ,
                                    ( SELECT    zdmc
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 1   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) zdmc1 ,
                                    ( SELECT    zdmc
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 2   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) zdmc2 ,
                                    ( SELECT    zdmc
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 3   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) zdmc3 ,
                                    CONVERT(VARCHAR(100), ryrq, 20) ryrq ,
                                    CONVERT(VARCHAR(100), cyrq, 23) cyrq ,
                                    ks.Name ksmc ,
                                    ks.Code ks,
                                    bq.bqmc ,
                                    ( ISNULL(bq.bqCode, '') ) bq,
                                    ISNULL(zy.gms, jb.gms) gms,
                                    zy.rybq ,
                                    ISNULL(zy.zy,jb.zy) zy ,
                                    ISNULL(zy.cs_sheng, jb.cs_sheng) cs_sheng ,
                                    ISNULL(zy.cs_shi, jb.cs_shi) cs_shi ,
                                    ISNULL(zy.cs_xian, jb.cs_xian) cs_xian ,
                                    ISNULL(zy.cs_dz, jb.cs_dz) cs_dz ,
                                    ISNULL(zy.xian_dz, jb.xian_dz) xian_dz ,
                                    ISNULL(zy.xian_sheng, jb.xian_sheng) xian_sheng ,
                                    ISNULL(zy.xian_shi, jb.xian_shi) xian_shi ,
                                    ISNULL(zy.xian_xian, jb.xian_xian) xian_xian ,
                                    ISNULL(zy.hu_dz, jb.hu_dz) hu_dz ,
                                    ISNULL(zy.hu_sheng, jb.hu_sheng) hu_sheng ,
                                    ISNULL(zy.hu_shi, jb.hu_shi) hu_shi ,
                                    ISNULL(zy.hu_xian, jb.hu_xian) hu_xian ,
                                    ISNULL(zy.jjlxr_sheng, jb.jjlxr_sheng) jjlxr_sheng ,
                                    ISNULL(zy.jjlxr_shi, jb.jjlxr_shi) jjlxr_shi ,
                                    ISNULL(zy.jjlxr_xian, jb.jjlxr_xian) jjlxr_xian ,
                                    ISNULL(zy.jjlxr_dz, jb.jjlxr_dz) jjlxr_dz ,
                                    ( ISNULL(zy.hy, jb.hf) ) hy ,
                                    gj.gjmc ,
                                    gj.gjCode ,
                                    mz.mzmc ,
                                    mz.mzCode ,
                                    ( ISNULL(zy.bje, 0) ) bje ,
                                    ISNULL(zy.lxr,jb.jjllr) lxr ,
                                    zy.lxr2 ,
                                    jb.dybh,
                                    (ISNULL(zy.lxrdh,jb.jjlldh)) lxrdh ,
                                    zy.lxryddh2 ,
                                    zy.lxrdz ,
                                    zy.lxrdz2 ,
                                    zy.lxrEmail ,
                                    zy.lxrEmail2 ,
                                    ( ISNULL(zy.lxrgx,jb.jjllrgx) ) lxrgx ,
                                    ( ISNULL(zy.lxrgx2, '') ) lxrgx2 ,
                                    zy.lxrjtdh ,
                                    zy.lxrjtdh2 ,
                                    zy.lxrWebchat ,
                                    zy.lxrWebchat2 ,
                                    staff.staffName doctormc ,
                                    zy.rytj ,
                                    case when zy.zyh is not null then zyxz.brxzmc else jbxz.brxzmc end brxzmc,
                                    case when zy.zyh is not null then zyxz.brxzbh else jbxz.brxzbh end brxzbh,
                                    case when zy.zyh is not null then zyxz.brxz else jbxz.brxz end brxz,
                                    staff2.staffGh jkjl ,
                                    staff.staffgh doctor ,
                                    staff.staffName doctormc ,
                                    ( SELECT    CAST(zdCode AS VARCHAR(10)) + '-'
                                                + CAST(zdmc AS VARCHAR(30))
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 1   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) ryzd1 ,
                                    ( SELECT    CAST(zdCode AS VARCHAR(10)) + '-'
                                                + CAST(zdmc AS VARCHAR(30))
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 2   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) ryzd2 ,
                                    ( SELECT    CAST(zdCode AS VARCHAR(10)) + '-'
                                                + CAST(zdmc AS VARCHAR(30))
                                      FROM      zy_rydzd
                                      WHERE     zdpx = 3   AND zt=1
                                                AND zyh = zy.zyh AND zy_rydzd.OrganizeId= @OrganizeId
                                    ) ryzd3 ,
                                    ks.Code ks ,
                                    CAST(bq.bqId AS VARCHAR(10)) + '-' + CAST(bq.bqCode AS VARCHAR(10)) bq ,
                                    jb.zjlx ,
                                    zy.ys,
                                    jb.phone,jb.jjlldh,jb.jjllr,jb.bz,jb.dwdz dz,jb.dwmc,zy.zcyy ,c.cblb,
                                    zy.ssczdm,zy.ssczmc,zy.syfwzh,zy.sylb,zy.sysslb,zy.wybz,zy.yzs,zy.tc,zy.tes,zy.zcbz,zy.syrq
                            FROM   xt_brjbxx jb
                                    LEFT JOIN dbo.zy_brjbxx zy ON zy.patid = jb.patid AND zy.zybz  NOT IN ({0}) AND  zy.OrganizeId=@OrganizeId 
                                    LEFT JOIN xt_card c  ON zy.kh = c.cardno and jb.OrganizeId= @OrganizeId
                                    LEFT JOIN dbo.xt_brxz jbxz ON jbxz.brxz = zy.brxz
                                                                AND jbxz.zt = '1' AND jbxz.OrganizeId=@OrganizeId
                                    LEFT JOIN dbo.xt_brxz zyxz ON zyxz.brxz = zy.brxz
                                                                AND zyxz.zt = '1' AND zyxz.OrganizeId=@OrganizeId
                                    LEFT JOIN dbo.zy_rydzd zd ON zd.zyh = zy.zyh AND zd.OrganizeId=@OrganizeId
                                    LEFT JOIN [NewtouchHIS_Base]..V_S_Sys_Department ks ON ks.Code = zy.ks
                                                                                          AND ks.OrganizeId = @OrganizeId
                                    LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON zy.bq = bq.bqCode AND bq.OrganizeId=@OrganizeId
                                    LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_gj gj ON zy.gj = gj.gjCode 
                                    LEFT JOIN NewtouchHIS_Base.dbo.V_C_Sys_StaffDuty staff ON staff.StaffGh = zy.doctor  AND staff.DutyCode='Doctor' AND staff.OrganizeId=@OrganizeId
                                    LEFT JOIN NewtouchHIS_Base.dbo.V_C_Sys_StaffDuty staff2 ON staff2.StaffGh = zy.jkjl AND staff2.DutyCode='RehabDoctor' AND staff2.OrganizeId=@OrganizeId
                                    LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_mz mz ON mz.mzCode = zy.mz where jb.patid=@patid"
                                    , ((int)EnumZYBZ.Wry + "," + (int)EnumZYBZ.Ycy)
                                    );
            inSqlParameterList.Add(new SqlParameter("@OrganizeId", OrganizeId));
            inSqlParameterList.Add(new SqlParameter("@patid", patid));
            result = true;
            return FindList<SysHosBasicInfoVO>(strSql.ToString(), inSqlParameterList.ToArray());
        }

        /// <summary>
        /// 病人管理获取form窗体
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        public SysHosBasicInfoVO GetSyspatientFormJson(string patid, string OrganizeId,string zjh)
        {
            StringBuilder strSql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            inSqlParameterList = new List<SqlParameter>();
            var csnyFormat = _sysConfigRepo.GetBoolValueByCode("PatientBasicInfo_Birthday_withTime", OrganizeId, false).Value ? 120 : 23;
            strSql.AppendFormat(@"SELECT  jb.patid ,
        jb.xm ,
        jb.zjh ,
        jb.py ,
        CONVERT(VARCHAR(100), jb.csny, {1}) csny ,
        jb.xb ,
        CAST(( CASE WHEN jb.csny IS NOT NULL
                    THEN FLOOR(DATEDIFF(DY, jb.csny, GETDATE()) / 365.25)
                    ELSE 0
               END ) AS SMALLINT) nl ,
        jb.brly ,
        jb.blh ,
        jb.dh ,
        jb.hf ,
        gj.gjmc ,
        gj.gjCode ,
        mz.mzmc ,
        mz.mzCode ,
        jb.dybh ,
        jb.zjlx ,
        jb.phone ,
        jb.jjlldh ,
        jb.jjllr ,
		jb.jjlxr_sheng,
		jb.jjlxr_shi,
		jb.jjlxr_xian,
		jb.jjlxr_dz,
        jb.bz ,
        jb.dwdz ,
        jb.dwmc ,
		jb.dwdh,
		jb.dwyb,
        jb.xl ,
        jb.zy,
        jb.dh ,
        jb.wechat ,
        jb.email ,
        jb.phone ,
        jb.cs_sheng ,
        jb.cs_shi ,
        jb.cs_xian ,
		jb.cs_dz,
		jb.cs_yb,
        jb.brly ,
        jb.xian_sheng ,
        jb.xian_shi ,
        jb.xian_xian ,
        jb.xian_dz ,
		jb.xian_yb,
        jb.hu_sheng ,
        jb.hu_shi ,
        jb.hu_xian ,
        jb.hu_dz,hu_yb,jb.zjlxfs,
        jb.jjllrgx, jb.gms,jbs,jg
FROM     xt_brjbxx  jb
        --LEFT JOIN  xt_card c  ON jb.patid = c.patid
        --                          AND jb.OrganizeId = @orgId
        --LEFT JOIN dbo.xt_brxz jbxz ON jbxz.brxz = c.brxz
                                   --   AND jbxz.zt = '1'
                                   --   AND jbxz.OrganizeId = @orgId
        --LEFT JOIN dbo.xt_brxz zyxz ON zyxz.brxz = c.brxz
                            --          AND zyxz.zt = '1'
                               --       AND zyxz.OrganizeId = @orgId
        LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_gj gj ON jb.gj = gj.gjCode
        LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_mz mz ON mz.mzCode = jb.mz
        LEFT JOIN dbo.zy_brjbxx zy ON zy.patid = jb.patid
                                      AND zy.zybz NOT IN ( {0} )
                                      AND zy.OrganizeId = @orgId
        WHERE jb.zt=1 and (jb.patid=@patid or jb.zjh=@zjh)", ((int)EnumZYBZ.Wry + "," + (int)EnumZYBZ.Ycy), csnyFormat
                                    );

            inSqlParameterList.Add(new SqlParameter("@orgId", OrganizeId));
            inSqlParameterList.Add(new SqlParameter("@patid", patid ?? ""));
            inSqlParameterList.Add(new SqlParameter("@zjh", zjh ?? ""));
            var data = FindList<SysHosBasicInfoVO>(strSql.ToString(), inSqlParameterList.ToArray());

            if (data != null && data.Count > 0)
            {
                return data.FirstOrDefault();
            }
            return null;
        }


        /// <summary>
        /// 同时保存住院基本信息//和预交账户信息
        /// </summary>
        /// <param name="VO"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="res"></param>
        public string SaveSysBasicAccountInfo(SysHosBasicInfoVO VO, string OrganizeId, out string res)
        {
            //修改住院病人信息时 同步给CIS
            InpatientPatientInfoDTO syncInPatientDTO = null;
            if (_sysConfigRepo.GetBoolValueByCode("HOSP_INTERFACE_WITH_CPOE", OrganizeId) == true)
            {
                syncInPatientDTO = new InpatientPatientInfoDTO();
            }

            res = "添加成功";
            if ((VO.patid ?? 0) == 0)
            {
                throw new FailedException("patid不能为空");
            }
            var zyhAutoGene = _sysConfigRepo.GetBoolValueByCode("xt_zyh", OrganizeId, true).Value;
            //当前数据库中的住院病人基本信息   //这是要记日志 比较的    //修改时，才有值
            HospPatientBasicInfoEntity oldHospPatientBasicInfo = null;
            //新入院，会产生新住院号 要返回到页面上
            string newZyh = null;
            var hospPatientBasicInfo = _HosPatBasicInfoRepository.GetFirstPatiInfoByPatid(VO.patid);
            if (hospPatientBasicInfo != null)
            {
                oldHospPatientBasicInfo = hospPatientBasicInfo.Clone();
                //sync to cis
                //新入院的不需要同步
                if (syncInPatientDTO != null
                    && hospPatientBasicInfo.zybz != ((int)EnumZYBZ.Xry).ToString())
                {
                    syncInPatientDTO.zyh = hospPatientBasicInfo.zyh;
                }
            }
            else
            {
                //当前数据库没有对应的住院 则视为 新增住院
                hospPatientBasicInfo = new HospPatientBasicInfoEntity();
                if (!zyhAutoGene)
                {
                    //住院号配置不自动生成，则判断住院号，并且赋值
                    if (string.IsNullOrWhiteSpace(VO.zyh))
                    {
                        throw new FailedCodeException("缺少住院号");
                    }
                    //判重
                    GetBasicInfoByZyh(VO.zyh.Trim(), OrganizeId);
                    hospPatientBasicInfo.zyh = VO.zyh;
                }
            }

            #region 住院病人基本信息 赋值

            if (oldHospPatientBasicInfo == null && zyhAutoGene)
            {
                newZyh = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_zyh", OrganizeId, initFormat: "{0:D5}"); //新入院 生成住院号
                hospPatientBasicInfo.zyh = newZyh;
            }
            if (string.IsNullOrWhiteSpace(VO.ryrq))
            {
                throw new FailedException("入院日期不能为空");
            }
            if (string.IsNullOrWhiteSpace(VO.ks))
            {
                throw new FailedException("科室不能为空");
            }
            if (string.IsNullOrWhiteSpace(VO.bq))
            {
                throw new FailedException("病区不能为空");
            }
            hospPatientBasicInfo.patid = VO.patid.Value;
            if (oldHospPatientBasicInfo == null)
            {
                //是入院登记
                hospPatientBasicInfo.zybz = ((int)EnumZYBZ.Xry).ToString();
                var isToDjz = _sysConfigRepo.GetBoolValueByCode(
                    "HOSP_Register_To_DJZ", OrganizeId);
                //默认是新入院，只有配了才直接待结账
                if (isToDjz == true)
                {
                    hospPatientBasicInfo.zybz = ((int)EnumZYBZ.Djz).ToString();
                }
            }
            hospPatientBasicInfo.ks = VO.ks;
            hospPatientBasicInfo.bq = VO.bq;
            hospPatientBasicInfo.ryrq = Convert.ToDateTime(VO.ryrq);
            hospPatientBasicInfo.rytj = VO.rytj;
            hospPatientBasicInfo.jkjl = VO.jkjl;
            hospPatientBasicInfo.doctor = VO.doctor ?? "";
            hospPatientBasicInfo.cw = VO.cw;
            hospPatientBasicInfo.brxz = VO.brxz;
            //主诊断 sync to cis
            if (syncInPatientDTO != null && !string.IsNullOrWhiteSpace(syncInPatientDTO.zyh) && !string.IsNullOrWhiteSpace(VO.brxz))
            {
                syncInPatientDTO.brxz = VO.brxz;
                syncInPatientDTO.brxzmc = VO.brxzmc;
            }
            hospPatientBasicInfo.gms = VO.gms;
            hospPatientBasicInfo.rybq = VO.rybq;
            hospPatientBasicInfo.zy = VO.zy;    //职业
            hospPatientBasicInfo.ys = VO.ys;
            hospPatientBasicInfo.cs_sheng = VO.cs_sheng;
            hospPatientBasicInfo.cs_shi = VO.cs_shi;
            hospPatientBasicInfo.cs_xian = VO.cs_xian;
            hospPatientBasicInfo.cs_dz = VO.cs_dz;
            hospPatientBasicInfo.xian_sheng = VO.xian_sheng;
            hospPatientBasicInfo.xian_shi = VO.xian_shi;
            hospPatientBasicInfo.xian_xian = VO.xian_xian;
            hospPatientBasicInfo.xian_dz = VO.xian_dz;
            hospPatientBasicInfo.hu_sheng = VO.hu_sheng;
            hospPatientBasicInfo.hu_shi = VO.hu_shi;
            hospPatientBasicInfo.hu_xian = VO.hu_xian;
            hospPatientBasicInfo.hu_dz = VO.hu_dz;
            hospPatientBasicInfo.jjlxr_sheng = VO.jjlxr_sheng;
            hospPatientBasicInfo.jjlxr_shi = VO.jjlxr_shi;
            hospPatientBasicInfo.jjlxr_xian = VO.jjlxr_xian;
            hospPatientBasicInfo.jjlxr_dz = VO.jjlxr_dz;
            hospPatientBasicInfo.mz = VO.mzCode;
            hospPatientBasicInfo.gj = VO.gjCode;
            if (VO.hy.HasValue)
            {
                hospPatientBasicInfo.hy = VO.hy.Value;
            }
            hospPatientBasicInfo.bje = VO.bje;
            hospPatientBasicInfo.lxr = VO.lxr;
            hospPatientBasicInfo.lxrdh = VO.lxrdh;
            hospPatientBasicInfo.lxrdz = VO.lxrdz;
            hospPatientBasicInfo.lxrEmail = VO.lxrEmail;
            hospPatientBasicInfo.lxrgx = VO.lxrgx;
            hospPatientBasicInfo.lxrWebchat = VO.lxrWebchat;
            hospPatientBasicInfo.lxr2 = VO.lxr2;
            hospPatientBasicInfo.lxryddh2 = VO.lxryddh2;
            hospPatientBasicInfo.lxrdz2 = VO.lxrdz2;
            hospPatientBasicInfo.lxrEmail2 = VO.lxrEmail2;
            hospPatientBasicInfo.lxrgx2 = VO.lxrgx2;
            hospPatientBasicInfo.lxrWebchat2 = VO.lxrWebchat2;
            //主诊断 sync to cis
            if (syncInPatientDTO != null && !string.IsNullOrWhiteSpace(syncInPatientDTO.zyh) && !string.IsNullOrWhiteSpace(VO.lxr))
            {
                syncInPatientDTO.lxr = VO.lxr;
                syncInPatientDTO.lxrdh = VO.lxrdh;
                syncInPatientDTO.lxrgx = VO.lxrgx;
            }
            //卡
            hospPatientBasicInfo.kh = VO.kh;
            VO.cardtype = string.IsNullOrWhiteSpace(VO.cardtype) ? ((int)EnumCardType.XNK).ToString() : VO.cardtype;
            hospPatientBasicInfo.CardType =VO.cardtype ;
            hospPatientBasicInfo.CardTypeName = ((EnumCardType)(Convert.ToInt32(VO.cardtype))).GetDescription();

            hospPatientBasicInfo.lxrjtdh = VO.lxrjtdh;
            hospPatientBasicInfo.lxrjtdh2 = VO.lxrjtdh2;
            hospPatientBasicInfo.xm = VO.xm;
            hospPatientBasicInfo.xb = VO.xb;
            hospPatientBasicInfo.blh = VO.blh;
            hospPatientBasicInfo.OrganizeId = OrganizeId;
            hospPatientBasicInfo.zjlx = VO.zjlx;
            hospPatientBasicInfo.zjh = VO.zjh;
            hospPatientBasicInfo.zcyy = VO.zcyy;
            hospPatientBasicInfo.jzlx = VO.jzlx;
            hospPatientBasicInfo.bzbm = VO.bzbm;
            hospPatientBasicInfo.bzmc = VO.bzmc;
            hospPatientBasicInfo.ssczdm = VO.ssczdm;
            hospPatientBasicInfo.ssczmc = VO.ssczmc;
            hospPatientBasicInfo.syfwzh = VO.syfwzh;
            hospPatientBasicInfo.sylb = VO.sylb;
            hospPatientBasicInfo.sysslb = VO.sysslb;
            hospPatientBasicInfo.wybz = VO.wybz;
            hospPatientBasicInfo.yzs = VO.yzs;
            hospPatientBasicInfo.tc = VO.tc;
            hospPatientBasicInfo.tes = VO.tes;
            hospPatientBasicInfo.zcbz = VO.zcbz;
            hospPatientBasicInfo.syrq = VO.syrq;

            if (!string.IsNullOrWhiteSpace(VO.csny))
            {
                hospPatientBasicInfo.csny = Convert.ToDateTime(VO.csny);
                var nage = DateTimeHelper.getAgeFromBirthTime(hospPatientBasicInfo.csny);
                hospPatientBasicInfo.nl = nage.nl;
                hospPatientBasicInfo.nlshow = nage.text;
            }
            #endregion

            #region 入院多诊断
            var zdCodeList = new string[] { VO.ryzd1, VO.ryzd2, VO.ryzd3 }.Distinct().ToList();
            var zdList = new List<SysDiagnosisVEntity>();
            for (var i = 0; i < zdCodeList.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(zdCodeList[i]))
                {
                    var zdEntity = _SysDiagnoseRepo.GetEntityByCode(OrganizeId, zdCodeList[i]);
                    if (zdEntity != null)
                    {
                        zdList.Add(zdEntity);

                        if (i == 0)
                        {
                            //主诊断 sync to cis
                            if (syncInPatientDTO != null && !string.IsNullOrWhiteSpace(syncInPatientDTO.zyh) && zdEntity != null)
                            {
                                syncInPatientDTO.ryzd = zdEntity.zdCode;
                                syncInPatientDTO.ryzdmc = zdEntity.zdmc;
                            }
                        }
                    }
                }
            }
            var oldHmdUpdateList = new List<HospMultiDiagnosisEntity>();
            //数据库中当前住院号的入院多诊断（可以包括无效的）
            List<HospMultiDiagnosisEntity> hmdUpdateList = null;
            if (oldHospPatientBasicInfo != null) //Update
            {
                hmdUpdateList = _HosMoreDiagnoseRepository.IQueryable().Where(p => p.zyh == hospPatientBasicInfo.zyh).ToList();

                foreach (var item in hmdUpdateList)
                {
                    oldHmdUpdateList.Add(item.Clone());
                    item.zt = "0";  //更新为无效
                }
            }
            var hmdInsertList = new List<HospMultiDiagnosisEntity>();
            //if (zdList.Count - hmdUpdateList.Count < 0)
            //{
            //    var minus = hmdUpdateList.Count - zdList.Count;
            //    hmdUpdateList.RemoveRange(0, minus);
            //}
            for (var i = 0; i < zdList.Count; i++)   //i是从1开始的
            {

                if (hmdUpdateList != null && hmdUpdateList.Count > i)
                {
                    hmdUpdateList[i].zdCode = zdList[i].zdCode;
                    hmdUpdateList[i].zdmc = zdList[i].zdmc;
                    hmdUpdateList[i].icd10 = zdList[i].icd10;
                    hmdUpdateList[i].zdpx = (i + 1).ToString();
                    hmdUpdateList[i].zt = "1";
                    hmdUpdateList[i].OrganizeId = OrganizeId;
                }
                else
                {
                    //做Insert
                    hmdInsertList.Add(new HospMultiDiagnosisEntity
                    {
                        zyh = hospPatientBasicInfo.zyh,
                        zdCode = zdList[i].zdCode,
                        zdmc = zdList[i].zdmc,
                        icd10 = zdList[i].icd10,
                        zdpx = (i + 1).ToString(),
                        zt = "1",
                        OrganizeId = OrganizeId
                    });
                }
            }
            #endregion

            #region 系统病人基本信息
            var xtbrxx = _SysPatBasicInfoRepository.FindEntity(p => p.patid == VO.patid && p.OrganizeId == OrganizeId && p.zt == "1");
            if (xtbrxx!=null)
            {
                xtbrxx.jjllr = VO.lxr;
                xtbrxx.jjlldh = VO.lxrdh;
                xtbrxx.jjllrgx = VO.lxrgx;
                xtbrxx.gms = VO.gms;
                xtbrxx.zy = VO.zy;    //职业
                xtbrxx.mz = VO.mzCode;
                xtbrxx.gj = VO.gjCode;
                xtbrxx.cs_sheng = VO.cs_sheng;
                xtbrxx.cs_shi = VO.cs_shi;
                xtbrxx.cs_xian = VO.cs_xian;
                xtbrxx.cs_dz = VO.cs_dz;
                xtbrxx.xian_sheng = VO.xian_sheng;
                xtbrxx.xian_shi = VO.xian_shi;
                xtbrxx.xian_xian = VO.xian_xian;
                xtbrxx.xian_dz = VO.xian_xian;
                xtbrxx.hu_sheng = VO.hu_sheng;
                xtbrxx.hu_shi = VO.hu_shi;
                xtbrxx.hu_xian = VO.hu_xian;
                xtbrxx.hu_dz = VO.hu_dz;
                xtbrxx.jjlxr_sheng = VO.jjlxr_sheng;
                xtbrxx.jjlxr_shi = VO.jjlxr_shi;
                xtbrxx.jjlxr_xian = VO.jjlxr_xian;
                xtbrxx.jjlxr_dz = VO.jjlxr_dz;

            }
            if (VO.hy.HasValue)
            {
                xtbrxx.hf = (byte?)VO.hy;  //住院提交过来的是hy
            }
            #endregion

            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //住院病人基本信息
                if (oldHospPatientBasicInfo != null)
                {
                    //Update
                    res = "修改成功";
                    hospPatientBasicInfo.Modify();
                    db.Update(hospPatientBasicInfo);
                }
                else
                {
                    //Insert
                    hospPatientBasicInfo.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt(HospPatientBasicInfoEntity.GetTableName()));
                    db.Insert(hospPatientBasicInfo);
                }
                //入院诊断
                if (hmdUpdateList != null)
                {
                    foreach (var item in hmdUpdateList)
                    {
                        item.Modify();
                        db.Update(item);
                    }
                }
                if (hmdInsertList != null)
                {
                    foreach (var item in hmdInsertList)
                    {
                        item.Create(true);
                        db.Insert(item);
                    }
                }
                //系统病人基本信息
                if (xtbrxx != null)
                {
                    xtbrxx.Modify();
                    db.Update(xtbrxx);
                }
                db.Commit();
            }
            //保存实体变更日志
            if (oldHospPatientBasicInfo != null)
            {
                AppLogger.WriteEntityChangeRecordLog(oldHospPatientBasicInfo, hospPatientBasicInfo, HospPatientBasicInfoEntity.GetTableName(), oldHospPatientBasicInfo.syxh.ToString());
            }
            if (hmdUpdateList != null)
            {
                for (var i = 0; i < hmdUpdateList.Count; i++)
                {
                    AppLogger.WriteEntityChangeRecordLog(oldHmdUpdateList[i], hmdUpdateList[i], SysPatientAccountEntity.GetTableName(), oldHmdUpdateList[i].zdCode.ToString());
                }
            }

            if (syncInPatientDTO != null && !string.IsNullOrWhiteSpace(syncInPatientDTO.zyh))
            {
                SiteCISAPIHelper.UpdateInpatientBasicInfo(syncInPatientDTO);
            }

            return hospPatientBasicInfo.zyh;
        }

        /// <summary>
        /// 根据zyh判断是否存在病人基本信息
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private void GetBasicInfoByZyh(string zyh, string orgId)
        {
            var list = _HosPatBasicInfoRepository.IQueryable().Where(p => p.OrganizeId == orgId && p.zyh == zyh && p.zt == "1").ToList();
            if (list.Count == 0)
            {
                return;
            }
            else if (list.Count == 1)
            {
                if (list[0].zybz == ((int)EnumZYBZ.Wry).ToString())
                {
                    throw new FailedException("该住院号已登记，且已取消入院");
                }
                //住院号已存在
                throw new FailedCodeException("MOREZYH_IS_INVALID");
            }
            else if (list.Count > 1)
            {
                //当前住院号存在多条住院记录
                throw new FailedCodeException("MOREINFO_IS_INVALID");
            }
        }

        public DateTime? IFCQRQISJZSJ(string zyh, string orgId)
        {
            var sql = "select cqrq from [Newtouch_CIS]..zy_brxxk where organizeid=@orgId and zyh=@zyh and zt='1'";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@zyh", zyh));
            pars.Add(new SqlParameter("@orgId", orgId));
            var cqrq = this.FirstOrDefault<DateTime?>(sql, pars.ToArray());
            return cqrq;
        }
        /// <summary>
        /// check zyh是否存在有效计费
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public bool CheckHasFee(string zyh, string orgId)
        {
            var sql = @"select 1
from
(
select (select isnull(sum(sl),0) sl from zy_xmjfb(nolock) where zt = '1' and zyh = @zyh and OrganizeId = @orgId ) sl
union all
select (select isnull(sum(sl),0) sl from zy_ypjfb(nolock) where zt = '1' and zyh = @zyh and OrganizeId = @orgId ) sl
) assss
where sl <> 0";
            return this.FirstOrDefault<int?>(sql, new[] { new SqlParameter("@zyh", zyh), new SqlParameter("@orgId", orgId) }) != null;
        }

        public bool BoolRuQu(string zyh, string orgid)
        {
            var sql = @"select 1 from [Newtouch_CIS]..zy_brxxk where zt='1' and zybz<>'9' and zybz<>'2' and  zyh=@zyh and OrganizeId=@orgId";
            return this.FirstOrDefault<int?>(sql, new[] { new SqlParameter("@zyh", zyh), new SqlParameter("@orgId", orgid) }) != null;
        }

        #region 病人管理

        /// <summary>
        /// 病人管理查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="orgId"></param>
        /// <param name="blh"></param>
        /// <param name="xm"></param>
        /// <param name="xlbrbz"></param>
        /// <returns></returns>
        public List<SysPatientManageSelectVO> GetList(Pagination pagination, string orgId,string zjh)
        {
            StringBuilder strsql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            strsql.Append(@"SELECT  brjbxx.patId ,
                            brjbxx.blh ,
                            brjbxx.xm ,
                           -- brxz.brxzmc ,
                            brjbxx.xb ,
                            brjbxx.csny ,
                            ( SELECT TOP 1
                                        ItemsDetail.Name brly
                              FROM      mz_gh gh
                                        LEFT JOIN [NewtouchHIS_Base]..V_C_Sys_ItemsDetail ItemsDetail ON ItemsDetail.Code = gh.brly
                                                                                  AND ItemsDetail.CateCode = 'PatientSource'
                                                                                  AND (ItemsDetail.OrganizeId = @orgId  OR ItemsDetail.OrganizeId = '*')
                              WHERE     brjbxx.blh = gh.blh
                                        AND gh.OrganizeId = @orgId
                                        AND gh.zt = 1
                                ORDER BY  gh.CreateTime DESC
                            ) brly ,
                            brjbxx.CreateTime ,
                            brjbxx.LastModifyTime,brjbxx.zt,brjbxx.zjh--,cd.CardId ,cd.cardTypeName,cd.cardType
                    FROM    xt_brjbxx brjbxx
                            --LEFT JOIN xt_card cd on cd.patid=brjbxx.patid AND cd.OrganizeId = brjbxx.OrganizeId
                            --LEFT JOIN xt_brxz brxz ON brxz.brxz = cd.brxz
                           --       AND brxz.OrganizeId = cd.OrganizeId
                    WHERE   --brjbxx.zt = '1' AND
                             brjbxx.OrganizeId = @orgId");
            inSqlParameterList = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(zjh))
            {
                strsql.Append(" AND ( brjbxx.zjh like @zjh or  brjbxx.blh like @zjh or brjbxx.xm like @zjh or brjbxx.py like @zjh)");
                inSqlParameterList.Add(new SqlParameter("@zjh", "%" + zjh.Trim() + "%"));
            }
            inSqlParameterList.Add(new SqlParameter("@orgId", orgId));
            return this.QueryWithPage<SysPatientManageSelectVO>(strsql.ToString(), pagination, inSqlParameterList.ToArray()).ToList();
        }
        #endregion

        #region 医保交易单边帐数据查询
        public List<YibaoDataVo> GetYbCancelList(Pagination pagination, string isjs, string lx, string keyword)
        {
            StringBuilder strsql = new StringBuilder();
            IList<SqlParameter> sqlparam = null;
            sqlparam = new List<SqlParameter>();
            if (isjs == "yj")//医保已结算 HIS未结算
            {
                if (lx == "zy")
                {
                    strsql.Append(@"SELECT 0 jsnm,0.00 xjzf,'---' mdtrt_id,a.jylsh setl_id,'---' psn_no,zyxx.xm psn_name
                                    ,a.zxjssj setl_time,a.mzzyh zymzh,a.medfee_sumamt ,'' medins_setl_id,'' cbdbm,'离休医保单边' dbz  
                                    FROM drjk_output05 a WITH(NOLOCK)
                                    JOIN zy_brjbxx zyxx WITH(NOLOCK) on zyxx.zyh=a.mzzyh and zyxx.zt=1
                                    LEFT JOIN cqyb_OutPut05 b WITH(NOLOCK) on b.jylsh=a.jylsh and b.zt='1'
                                    WHERE a.zt='1'  and a.zxjssj>CONVERT(varchar(5),GETDATE(),121)+'01-01'  
                                    and b.jylsh is null ");
                    if (!string.IsNullOrEmpty(keyword))
                        strsql.Append(" and (zyxx.xm like @keyword or zyh like @keyword)");
                    strsql.Append(@" union all
                                    SELECT 0 jsnm,0.00 xjzf,mdtrt_id,setl_id,psn_no,psn_name,setl_time,a.zyh zymzh,medfee_sumamt,medins_setl_id
                                           ,c.cbdbm,'医保单边' dbz
                                    FROM drjk_zyjs_output a WITH(NOLOCK)
                                    LEFT JOIN zy_js b WITH(NOLOCK) on b.ybjslsh=a.setl_id and b.zt='1'
                                    LEFT JOIN (select distinct grbh,cbdbm from  xt_card WITH(NOLOCK)) c on a.psn_no=c.grbh 
                                    WHERE a.zt='1' and b.ybjslsh is null ");

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        strsql.Append(" and (psn_name like @keyword or zyh like @keyword)");
                        sqlparam.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
                    }
                }
                else
                {
                    strsql.Append(@"SELECT 0 jsnm,0.00 xjzf,'---' mdtrt_id,a.jylsh setl_id,'---' psn_no,gh.xm psn_name,a.zxjssj setl_time,a.mzzyh zymzh,
                                    a.medfee_sumamt ,'' medins_setl_id,'' cbdbm,'离休医保单边' dbz  
                                    FROM drjk_output05 a WITH(NOLOCK)
                                    JOIN mz_gh gh WITH(NOLOCK) on gh.mzh=a.mzzyh and gh.zt=1
                                    LEFT JOIN cqyb_OutPut05 b WITH(NOLOCK) on b.jylsh=a.jylsh and b.zt='1'
                                    WHERE a.zt='1'  and a.zxjssj>CONVERT(varchar(5),GETDATE(),121)+'01-01'  
                                    and b.jylsh is null ");

                    if (!string.IsNullOrEmpty(keyword))
                        strsql.Append(" and (gh.xm like @keyword or mzh like @keyword)");

                    strsql.Append(@" union all
                                    SELECT 0 jsnm,0.00 xjzf,mdtrt_id,setl_id,psn_no,psn_name,setl_time,mzh zymzh,medfee_sumamt,medins_setl_id
                                    ,c.cbdbm,'医保单边' dbz  
                                    FROM  drjk_mzjs_output a  WITH(NOLOCK)
                                    LEFT JOIN mz_js b WITH(NOLOCK) on b.ybjslsh=a.setl_id and b.zt='1'
                                    LEFT JOIN (select distinct grbh,cbdbm from  xt_card WITH(NOLOCK)) c on a.psn_no=c.grbh 
                                    WHERE a.zt='1'  and a.setl_time>CONVERT(varchar(5),GETDATE(),121)+'01-01' and b.ybjslsh is null");

                    if (!string.IsNullOrEmpty(keyword))
                        strsql.Append(" and (psn_name like @keyword or mzh like @keyword)");

                    strsql.Append(@" union all
                                     SELECT a.jsnm,a.xjzf,ybjs.mdtrt_id,ybjs.setl_id,ybjs.psn_no,ybjs.psn_name,ybjs.setl_time,ybjs.mzh zymzh,
                                            ybjs.medfee_sumamt,ybjs.medins_setl_id,cd.cbdbm,'HIS单边' dbz  
                                     FROM mz_js a WITH(NOLOCK)
                                     JOIN xt_brjbxx b WITH(NOLOCK) on b.patid=a.patid and b.organizeid=a.organizeid 
                                     JOIN drjk_mzjs_output ybjs WITH(NOLOCK) on ybjs.setl_id=a.ybjslsh and ybjs.zt=0
                                     LEFT JOIN drjk_mzjs_output d WITH(NOLOCK) on a.ybjslsh=d.setl_id and d.zt=1
                                     LEFT JOIN xt_card cd on cd.patid=b.patid and cd.organizeid=b.organizeid  and cd.zt='1'
                                     where a.zt=1 and cd.cblb!='3' and a.brxz!='0' 
                                     and a.CreateTime>='2021-08-01 00:00:00'
                                     and a.jszt=1 and a.jsnm not in (select cxjsnm from mz_js WITH(NOLOCK) where jszt=2 )
                                     and d.setl_id is null
									 and ybjs.setl_id  is null  ");
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        strsql.Append(" and (ybjs.psn_name like @keyword or ybjs.mzh like @keyword)");
                        sqlparam.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
                    }
                }
            }
            else if (isjs == "wj")
            {
                strsql.Append(@"select l.tradiNumber,l.hisId,a.zyh,d.mdtrt_id mdtrt_id,'' setl_id,cd.grbh psn_no,b.xm psn_name,a.zyh zymzh,
SUBSTRING(l.inHead,CHARINDEX(',',l.inHead)+6,CHARINDEX(',',l.inHead,CHARINDEX(',',l.inHead)+1)-CHARINDEX(',',l.inHead)-6 ) medins_setl_id,
cd.cbdbm,l.errorMsg errormsg,l.tradiNumber infno 
from zy_brjbxx a with(nolock)
left join xt_brjbxx b with(nolock) on a.patid=b.patid and a.OrganizeId=b.OrganizeId and b.zt='1'
left join drjk_rybl_input d with(nolock) on d.zyh=a.zyh and d.zt='1'
left join ybjk_logcontent l with(nolock) on l.hisId=a.zyh   
LEFT JOIN xt_card cd with(nolock) on cd.cardno=a.kh and cd.CardType=a.CardType and cd.organizeid=a.organizeid  and cd.zt='1'
where   a.zt='1' and zybz not in('3','9')  and cd.CardType<>'1' and l.tradiNumber='2401' and (l.errorMsg='' or l.errorMsg='success') ");

                if (!string.IsNullOrEmpty(keyword))
                {
                    strsql.Append(" and (b.xm like @keyword or a.zyh like @keyword)");
                    sqlparam.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
                }
            }
            else if (isjs == "cz")//国家平台医保交易单边 只能采用冲正交易 门诊：存在网络等特殊情况导致日志表无记录但日志文件有记录，需新增日志记录到日志表 请将 errorMsg 值赋值未0 在冲正
            {
                if (lx == "zy")
                {
                    strsql.Append(@"  select ry.mdtrt_id,'' setl_id,cd.grbh psn_no,b.xm psn_name, a.zyh zymzh,
SUBSTRING(l.inHead,CHARINDEX(',',l.inHead)+6,CHARINDEX(',',l.inHead,CHARINDEX(',',l.inHead)+1)-CHARINDEX(',',l.inHead)-6 ) medins_setl_id,
 cd.cbdbm,l.errormsg errormsg,l.tradiNumber infno
from zy_brjbxx a with(nolock)
left join xt_brjbxx b with(nolock) on a.patid=b.patid and a.OrganizeId=b.OrganizeId and b.zt='1'
left join drjk_rybl_input ry with(nolock) on ry.zyh=a.zyh  --and ry.psn_no is null
inner join ybjk_logcontent l with(nolock) on l.hisId=a.zyh
LEFT JOIN xt_card cd with(nolock) on cd.cardno=a.kh and cd.CardType=a.CardType and cd.organizeid=a.organizeid  and cd.zt='1'
where   a.zt='1'  and zybz not in('3','9') and cd.CardType<>'1'   and l.tradiNumber='2304'  and l.errorMsg<>''");
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        strsql.Append(" and (b.xm like @keyword or a.zyh like @keyword)");
                        //sqlparam.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
                    }
                }
                else {
                    strsql.Append(@" select  a.jzid mdtrt_id,'' setl_id,cd.grbh psn_no,b.xm psn_name, a.mzh zymzh,
SUBSTRING(l.inHead,CHARINDEX(',',l.inHead)+6,CHARINDEX(',',l.inHead,CHARINDEX(',',l.inHead)+1)-CHARINDEX(',',l.inHead)-6 ) medins_setl_id,
 cd.cbdbm,l.errormsg errormsg,l.tradiNumber infno 
from mz_gh a with(nolock)
left join xt_brjbxx b with(nolock) on a.patid=b.patid and a.OrganizeId=b.OrganizeId and b.zt='1'
inner join ybjk_logcontent l with(nolock) on l.hisId=a.mzh 
LEFT JOIN xt_card cd with(nolock) on cd.CardNo=a.kh and cd.CardType=a.CardType and cd.organizeid=a.organizeid  and cd.zt='1'
where  a.zt='1' and cd.CardType<>'1' and l.tradiNumber='2207' and l.errorMsg='0' ");
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        strsql.Append(" and (b.xm like @keyword or a.mzh like @keyword)");
                        sqlparam.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
                    }
                }
            }
            return this.QueryWithPage<YibaoDataVo>(strsql.ToString(), pagination, sqlparam.ToArray()).ToList();

        }
        #endregion

        /// <summary>
        /// 根据住院号查询有效住院病人基本信息
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public List<SysHosBasicInfoVO> GetSysBasicByZHY(string kh, string zyh, string orgid)
        {
            StringBuilder strSql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            try
            {
                strSql.Append(@"SELECT  a.zyh ,
        a.patid ,
        ( CASE a.zybz
            WHEN 0 THEN '入院登记'
            WHEN 1 THEN '病区中'
            WHEN 2 THEN '病区出院'
            WHEN 3 THEN '病人出院'
            WHEN 9 THEN '取消入院'
            ELSE ''
          END ) zybz ,
        CONVERT(VARCHAR(100), a.ryrq, 23) ryrq ,
                                                                            --  a.ryrq ,
        a.rybq ,
        a.xm ,
        a.xb ,                                                     -- b.csny ,
        CONVERT(VARCHAR(100), a.csny, 23) csny ,
        --b.dwmc ,
        --b.pzh ,
        --b.ylxm ,
        --b.pzksrq ,
        --b.pzzzrq ,
        --b.pzzd ,
        a.kh ,
        c.brxzbh ,
        c.brxz ,
        c.brxzmc ,
        d.Code ks ,
        d.Name ksmc ,
                                --e.bqId bqbh ,
        e.bqCode bq ,
        e.bqmc ,
        f.cwCode cw ,
        f.cwmc ,
        g.zh ,
        g.zhye , --预交金
        h.zdCode ,
        h.icd10 ,
        h.zdmc
FROM    zy_brjbxx a
        LEFT JOIN xt_brxz c ON a.brxz = c.brxz
                               AND c.OrganizeId = @OrganizeId
        INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department d ON a.ks = d.Code
                                                              AND d.OrganizeId = @OrganizeId
        INNER JOIN [NewtouchHIS_Base]..V_S_xt_bq e ON a.bq = e.bqCode
                                                      AND e.OrganizeId = @OrganizeId
        LEFT JOIN [NewtouchHIS_Base]..V_S_xt_cw f ON a.cw = f.cwCode
                                                     AND f.OrganizeId = @OrganizeId
        INNER JOIN xt_brzh g ON a.zyh = g.zyh
                                AND a.patid = g.patid
                                AND g.zhxz = 3
                                AND g.zt = 1
                                AND g.OrganizeId = @OrganizeId
        LEFT JOIN [NewtouchHIS_Base]..V_S_xt_zd AS h ON a.ryzd = h.zdId
                                                        AND ( h.OrganizeId = @OrganizeId
                                                              OR h.OrganizeId = '*'
                                                            )
WHERE   zybz NOT IN ( 0, 4 ) ");
                inSqlParameterList = new List<SqlParameter>();
                if (!string.IsNullOrEmpty(zyh))
                {
                    strSql.AppendLine("and a.zyh='" + zyh + "' ");
                    //inSqlParameterList.Add(new SqlParameter("@zyh", zyh));
                }
                if (!string.IsNullOrWhiteSpace(kh))
                {
                    strSql.AppendLine("and a.kh='" + kh + "' ");
                    //inSqlParameterList.Add(new SqlParameter("@kh", kh));
                }
                inSqlParameterList.Add(new SqlParameter("@OrganizeId", orgid));
                strSql.AppendLine("order by a.zyh desc");
                return this.FindList<SysHosBasicInfoVO>(strSql.ToString(), inSqlParameterList.ToArray());
            }
            catch (Exception e)
            {
                throw new FailedException("获取住院信息失败！");
            }
        }


        /// <summary>
        /// 费用一日清 住院号浮层搜索用
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
        public List<SysHosBasicInfoVO> GetSysBasicVagueByZHY(string zyh, string orgid)
        {
            StringBuilder strSql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            try
            {
                strSql.Append(@"SELECT  a.zyh ,
        a.patid ,
        ( CASE a.zybz
            WHEN 0 THEN '入院登记'
            WHEN 1 THEN '病区中'
            WHEN 2 THEN '病区出院'
            WHEN 3 THEN '病人出院'
            WHEN 9 THEN '取消入院'
            ELSE ''
          END ) zybz ,
        CONVERT(VARCHAR(100), a.ryrq, 23) ryrq ,
        CONVERT(VARCHAR(100), a.cyrq, 23) cyrq,
        a.rybq ,
        a.xm ,
        a.xb ,                                                   
        CONVERT(VARCHAR(100), a.csny, 23) csny ,
        a.kh ,
        c.brxzbh ,
        c.brxz ,
        c.brxzmc ,
        d.Code ks ,
        d.Name ksmc , 
        e.bqCode bq ,
        e.bqmc ,
        f.cwCode,
        f.cwmc cw--,
         --//g.zh ,
        --//g.zhye , --预交金
        --//h.zdCode ,
        --//h.icd10 ,
        --//h.zdmc
FROM    zy_brjbxx a
        LEFT JOIN xt_brxz c ON a.brxz = c.brxz
                               AND c.OrganizeId = @OrganizeId
        INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department d ON a.ks = d.Code
                                                              AND d.OrganizeId = @OrganizeId
        INNER JOIN [NewtouchHIS_Base]..V_S_xt_bq e ON a.bq = e.bqCode
                                                      AND e.OrganizeId = @OrganizeId
        LEFT JOIN [NewtouchHIS_Base]..V_S_xt_cw f ON a.cw = f.cwCode
                                                     AND f.OrganizeId = @OrganizeId
         --//INNER JOIN xt_brzh g ON a.zyh = g.zyh
        --//                        AND a.patid = g.patid
        --//                        AND g.zhxz = 3
        --//                        AND g.zt = 1
        --//                        AND g.OrganizeId = @OrganizeId
        --//LEFT JOIN [NewtouchHIS_Base]..V_S_xt_zd AS h ON a.ryzd = h.zdId
        --//                                                AND ( h.OrganizeId = @OrganizeId
        --//                                                      OR h.OrganizeId = '*'
        --//                                                    )
WHERE   zybz NOT IN ( 0, 9 ) and a.OrganizeId=@OrganizeId");
                inSqlParameterList = new List<SqlParameter>();
                if (!string.IsNullOrEmpty(zyh))
                {
                    strSql.AppendLine(" and a.zyh like '%" + zyh + "%' ");
                }

                inSqlParameterList.Add(new SqlParameter("@OrganizeId", orgid));
                strSql.AppendLine(" order by a.zyh desc");
                return this.FindList<SysHosBasicInfoVO>(strSql.ToString(), inSqlParameterList.ToArray());
            }
            catch (Exception e)
            {
                throw new FailedException("获取住院信息失败！");
            }
        }
        /// <summary>
        /// 门诊病人挂号查询
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="flag"></param>
        /// <param name="msg"></param>
        /// <param name="lastUpdateTime"></param>
        /// <param name="outpatientNumber"></param>
        /// <returns></returns>
        public List<OutPatientRegistrationInfoDTO> OutPatientRegistrationQuery(
            string orgId, ref string flag, ref string msg
            , DateTime? lastUpdateTime = null, string outpatientNumber = null
            , string ksCode = null, string ysgh = null
            , string mjzbz = null
            , string jiuzhenbz = null
            , string keyword = null
            , Pagination pagination = null)
        {
            var paraList = new List<SqlParameter>() { };
            paraList.Add(new SqlParameter("@orgId", orgId ?? ""));
            paraList.Add(new SqlParameter("@lastUpdateTime", lastUpdateTime.HasValue ? lastUpdateTime.Value.ToString() : ""));
            paraList.Add(new SqlParameter("@outpatientNumber", outpatientNumber ?? ""));
            paraList.Add(new SqlParameter("@ksCode", ksCode ?? ""));
            paraList.Add(new SqlParameter("@ysgh", ysgh ?? ""));
            paraList.Add(new SqlParameter("@mjzbz", mjzbz ?? ""));
            paraList.Add(new SqlParameter("@jiuzhenbz", jiuzhenbz ?? ""));
            paraList.Add(new SqlParameter("@keyword", "%" + (keyword ?? "") + "%"));
            paraList.Add(new SqlParameter("@zzhz",  "0"));
            //
            pagination = pagination ?? new Pagination();
            pagination.page = pagination.page <= 0 ? 1 : pagination.page;
            pagination.rows = pagination.rows <= 0 ? 100000 : pagination.rows;  //不分页
            pagination.sidx = string.IsNullOrWhiteSpace(pagination.sidx) ? "UpdateTime" : pagination.sidx;

            paraList.Add(new SqlParameter("@page", pagination.page));
            paraList.Add(new SqlParameter("@rows", pagination.rows));
            paraList.Add(new SqlParameter("@sidx", pagination.sidx ?? ""));
            paraList.Add(new SqlParameter("@sord", pagination.sord ?? ""));

            var outParameter1 = new SqlParameter("@flag", System.Data.SqlDbType.VarChar, 10);
            outParameter1.Direction = ParameterDirection.Output;
            paraList.Add(outParameter1);
            var outParameter2 = new SqlParameter("@msg", System.Data.SqlDbType.VarChar, 128);
            outParameter2.Direction = ParameterDirection.Output;
            paraList.Add(outParameter2);
            //
            var outParameter3 = new SqlParameter("@records", System.Data.SqlDbType.Int);
            outParameter3.Direction = ParameterDirection.Output;
            paraList.Add(outParameter3);
            var outParameter4 = new SqlParameter("@total", System.Data.SqlDbType.Int);
            outParameter4.Direction = ParameterDirection.Output;
            paraList.Add(outParameter4);

            var list = this.FindList<OutPatientRegistrationInfoDTO>(@"exec usp_interface_OutPatientRegistrationQuery @orgId, @lastUpdateTime
, @outpatientNumber, @ksCode, @ysgh, @mjzbz, @jiuzhenbz, @keyword,@zzhz
--
,@page ,@rows ,@sidx ,@sord
--
,@records out ,@flag out ,@msg out", paraList.ToArray());

            pagination.records = Convert.ToInt32(outParameter3.Value);

            return list;
        }

        /// <summary>
        /// 住院患者查询（接口用）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="flag"></param>
        /// <param name="msg"></param>
        /// <param name="lastUpdateTime"></param>
        /// <param name="zyh"></param>
        /// <param name="bqCode"></param>
        /// <param name="zybz"></param>
        /// <param name="keyword"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        public List<InPatientInfoDTO> InPatientQuery(
            string orgId, ref string flag, ref string msg
            , DateTime? lastUpdateTime = null, string zyh = null
            , string bqCode = null, string zybz = null
            , string keyword = null
            , Pagination pagination = null)
        {
            var paraList = new List<SqlParameter>() { };
            paraList.Add(new SqlParameter("@orgId", orgId ?? ""));
            paraList.Add(new SqlParameter("@lastUpdateTime", lastUpdateTime.HasValue ? lastUpdateTime.Value.ToString() : ""));
            paraList.Add(new SqlParameter("@zyh", zyh ?? ""));
            paraList.Add(new SqlParameter("@bqCode", bqCode ?? ""));
            paraList.Add(new SqlParameter("@zybz", zybz ?? ""));
            paraList.Add(new SqlParameter("@keyword", keyword ?? ""));
            //
            pagination = pagination ?? new Pagination();
            pagination.page = pagination.page <= 0 ? 1 : pagination.page;
            pagination.rows = pagination.rows <= 0 ? 100000 : pagination.rows;  //不分页
            pagination.sidx = string.IsNullOrWhiteSpace(pagination.sidx) ? "UpdateTime" : pagination.sidx;

            paraList.Add(new SqlParameter("@page", pagination.page));
            paraList.Add(new SqlParameter("@rows", pagination.rows));
            paraList.Add(new SqlParameter("@sidx", pagination.sidx ?? ""));
            paraList.Add(new SqlParameter("@sord", pagination.sord ?? ""));

            var outParameter1 = new SqlParameter("@flag", System.Data.SqlDbType.VarChar, 10);
            outParameter1.Direction = ParameterDirection.Output;
            paraList.Add(outParameter1);
            var outParameter2 = new SqlParameter("@msg", System.Data.SqlDbType.VarChar, 128);
            outParameter2.Direction = ParameterDirection.Output;
            paraList.Add(outParameter2);
            //
            var outParameter3 = new SqlParameter("@records", System.Data.SqlDbType.Int);
            outParameter3.Direction = ParameterDirection.Output;
            paraList.Add(outParameter3);
            var outParameter4 = new SqlParameter("@total", System.Data.SqlDbType.Int);
            outParameter4.Direction = ParameterDirection.Output;
            paraList.Add(outParameter4);

            var list = this.FindList<InPatientInfoDTO>(@"exec usp_interface_InPatientQuery @orgId, @lastUpdateTime
, @zyh, @bqCode, @zybz, @keyword
--
,@page ,@rows ,@sidx ,@sord
--
,@records out ,@flag out ,@msg out", paraList.ToArray());

            pagination.records = Convert.ToInt32(outParameter3.Value);

            return list;
        }

        /// <summary>
        /// 住院患者查询（患者查询用）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="flag"></param>
        /// <param name="msg"></param>
        /// <param name="lastUpdateTime"></param>
        /// <param name="zyh"></param>
        /// <param name="bqCode"></param>
        /// <param name="zybz"></param>
        /// <param name="keyword"></param>
        /// <param name="pagination"></param>
        /// <param name="ryrqkssj"></param>
        /// <param name="ryrqjssj"></param>
        /// <returns></returns>
        public IList<InPatientInfoVO> GetInPatientList(
            string orgId, ref string flag, ref string msg
            , DateTime? lastUpdateTime = null, string zyh = null
            , string bqCode = null, string zybz = null
            , string keyword = null
            , Pagination pagination = null, DateTime? ryrqkssj = null, DateTime? ryrqjssj = null,string ksmc=null,string bqmc=null)
        {
            var sb = new StringBuilder();
            sb.Append(@"select zyxx.zyh, isnull(zyxx.xm, xx.xm) xm,cbdbm
	    --else 年龄 出生年月与入院日期作比较 实时计算一下
    ,case when isnull(zyxx.nl,0) <> 0 then zyxx.nl else (case when zyxx.csny is not null and zyxx.ryrq is not null then datediff(yy,zyxx.csny,zyxx.ryrq) else 0 end) end nl
    ,zyxx.brxz,
    xz.brxzmc,xz.brxzlb
    ,isnull(zyxx.blh, xx.blh) blh
    ,zd.zdCode AS zzdCode,zd.icd10 AS zzdicd10
	,(select ls.zdmc from zy_rydzd ls where ls.zyh=zd.zyh and ls.zdpx= '1' and zt='1' )zzdmc
	,(select ls.zdmc from zy_rydzd ls where ls.zyh=zd.zyh  and ls.zdpx= '2' and zt='1' ) zzdmc2
	,(select ls.zdmc from zy_rydzd ls where ls.zyh=zd.zyh  and ls.zdpx= '3' and zt='1' ) zzdmc3
    ,CONVERT(varchar(100), zyxx.csny, 23) AS csny
    ,CONVERT(varchar(100), zyxx.ryrq, 23) AS ryrq
    ,CONVERT(varchar(100), zyxx.cyrq, 23) AS cyrq
    ,zyxx.bq bqCode,bq.bqmc bqmc,cw.cwmc
    ,zyxx.zybz zybz
    ,zdb.Name zy
    ,(zyxx.xian_sheng+zyxx.xian_shi+zyxx.xian_xian+zyxx.xian_dz) xzz
    ,case zyxx.zybz when '0' then '入院登记' when '1' then '病区中' when '2' then '病区出院' when '3' then '病人出院' when '9' then '作废记录' else '' end AS zybzmc
    ,isnull(zyxx.LastModifyTime, zyxx.CreateTime) UpdateTime
    ,xx.py, '' wb
    ,zyxx.zjlx zjlx
    ,(CASE zyxx.zjlx WHEN '1'THEN '身份证' WHEN '2' THEN '护照' WHEN '3' THEN '军官证' ELSE '其他' END ) zjlxValue
    ,zyxx.zjh zjh
    ,(CASE zyxx.zjlx WHEN 1 THEN zyxx.zjh ELSE '' END) AS idCardNo
    ,(CASE zyxx.xb WHEN '1' THEN '1' WHEN '2' THEN '2' ELSE '3' END ) as sex
    ,(CASE zyxx.xb WHEN '1' THEN '男' WHEN '2' THEN '女' ELSE '不详' END ) as sexValue
	,xx.cyzdmc cyzd  
	,(CASE xx.cyfs WHEN '1'THEN '治愈' WHEN '2' THEN '好转' WHEN '3' THEN '转院' ELSE '' END ) cyfs 
	,CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, zyxx.ryrq,ISNULL(cyrq,CONVERT(varchar,GETDATE(),120))) WHEN 0 THEN 1 else  DATEDIFF(DAY, zyxx.ryrq,ISNULL(cyrq,CONVERT(varchar,GETDATE(),120)))END )+'天' inHosDays
    ,(zyxx.lxr+'('+
	(CASE zyxx.lxrgx WHEN '1' THEN '夫妻'WHEN '2' THEN '父子'WHEN '3' THEN '母子'WHEN '4' THEN '父女'WHEN '5' THEN '母女'WHEN '6' THEN '兄弟'WHEN '7' THEN '姐弟'WHEN '8' THEN '姐妹'WHEN '9' THEN '祖孙'
	WHEN 'A' THEN '公媳' WHEN 'B' THEN '婆媳'WHEN 'C' THEN '岳婿'WHEN 'D' THEN '连襟'WHEN 'E' THEN '妯娌'WHEN 'F' THEN '朋友'WHEN 'G' THEN '兄妹'ELSE '其他' END)+')')
	AS contPerName 
    ,zyxx.lxrdh AS contPerPhoneNum 
    ,lxrItem.Code AS contPerRel 
    ,lxrItem.Name AS contPerRelValue
    ,zyxx.ks, ks.Name ksmc, ysStaff.Name doctor,'' jzDoctor, '' pkbz,ysStaff.Name ysxm
    ,zyxx.kh, zyxx.CardType, zyxx.CardTypeName,zyxx.jzh,zyxx.lxrdh,mz.mzmc,zyxx.rybq
    ,case isnull(ybjs.jsqd_sclsh,'0') when '0' then '未上传' else '已上传' end isscm
	,ybjs.setl_id,
    ybjs.psn_no,ybjs.mdtrt_id
from zy_brjbxx(nolock) zyxx
left join [Newtouch_CIS].[dbo].[zy_brxxk] xx
    on zyxx.zyh = xx.zyh AND zyxx.OrganizeId = xx.OrganizeId  AND xx.zt = '1'
left join xt_brxz(nolock) xz
    ON xz.brxz = zyxx.brxz and xz.OrganizeId = zyxx.OrganizeId
left join zy_rydzd(nolock) zd
    ON zd.zyh = zyxx.zyh AND zd.OrganizeId = zyxx.OrganizeId and zd.zdpx = 1 AND zd.zt = '1'
left join Newtouch_CIS..zy_PatDxInfo(nolock) cyzd 
	ON cyzd.zyh = zyxx.zyh AND cyzd.OrganizeId = zyxx.OrganizeId and cyzd.zdlb=2 and cyzd.zdlx=0 AND cyzd.zt = '1'
left join [NewtouchHIS_Base]..V_S_xt_bq(nolock) bq
    ON bq.bqCode = zyxx.bq AND bq.OrganizeId = zyxx.OrganizeId  AND bq.zt = '1'
left join [NewtouchHIS_Base].[dbo].[xt_cw] cw with(nolock) ON xx.OrganizeId=cw.OrganizeId and xx.BedCode=cw.bfCode  and cw.zt='1'
LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail lxrItem 
    ON ( lxrItem.OrganizeId = zyxx.OrganizeId OR lxrItem.OrganizeId = '*')
    AND lxrItem.Code = zyxx.lxrgx
    AND lxrItem.CateCode = 'RelativeType'
left join NewtouchHIS_Base..V_S_Sys_Department ks
    on ks.Code = zyxx.ks and ks.OrganizeId = zyxx.OrganizeId
left join NewtouchHIS_Base..V_S_Sys_Staff ysStaff
    on ysStaff.gh = zyxx.doctor and ysStaff.OrganizeId = zyxx.OrganizeId
left join xt_card kh on kh.CardNo=zyxx.kh and kh.OrganizeId=zyxx.OrganizeId and kh.zt=1
left join NewtouchHIS_Base..xt_mz mz on mz.mzCode=zyxx.mz and mz.zt=1
left join drjk_zyjs_output ybjs on ybjs.zyh=zyxx.zyh and ybjs.zt=1 
left join [NewtouchHIS_Base].[dbo].[Sys_ItemsDetail] zdb on zdb.Code=zyxx.zy and zdb.zt=1
where zyxx.zt = '1'
and zyxx.OrganizeId = @orgId
");

            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.Append(@" and (zyxx.xm like @keyword or zyxx.blh like @keyword or zyxx.zyh like @keyword)");
                pars.Add(new SqlParameter("@keyword", "%" + (keyword ?? "") + "%"));
            }
            if (!string.IsNullOrWhiteSpace(zybz))
            {
                sb.Append(@"  and zyxx.zybz in (select col from [dbo].f_split(@zybz,','))");
                pars.Add(new SqlParameter("@zybz", zybz));
            }
            if (ryrqkssj.HasValue)
            {
                sb.Append(@" and zyxx.ryrq >= @ryrqkssj");
                pars.Add(new SqlParameter("@ryrqkssj", ryrqkssj.Value.Date));
            }
            if (ryrqjssj.HasValue)
            {
                sb.Append(@" and zyxx.ryrq < @ryrqjssj");
                pars.Add(new SqlParameter("@ryrqjssj", ryrqjssj.Value.AddDays(1).Date));
            }
            if (!string.IsNullOrWhiteSpace(ksmc)) {
                sb.Append(@" and zyxx.ks like @ksmc ");
                pars.Add(new SqlParameter("@ksmc", "%" + ksmc + "%"));
            }
            if (!string.IsNullOrWhiteSpace(bqmc))
            {
                sb.Append(@" and zyxx.bq like @bqmc ");
                pars.Add(new SqlParameter("@bqmc", "%" + bqmc + "%"));
            }
            var zybrlist= this.QueryWithPage<InPatientInfoVO>(sb.ToString(), pagination, pars.ToArray()).ToList();
            if (zybrlist.Count >0)
            {
                StringBuilder sbZyh = new StringBuilder();
                zybrlist.ForEach(m => {
                    if (sbZyh.Length > 0)
                    {
                        sbZyh.Append(",'" + m.zyh + "'");
                    }
                    else
                    {
                        sbZyh.Append("'" + m.zyh + "'");
                    }
                });
                String zyhIns = "and zyh in (" + sbZyh.ToString() + ") ";

                var patFee = GetPatFee(orgId, zyhIns);
                zybrlist.ForEach(m => {
                    var data = patFee.FirstOrDefault(n => n.zyh == m.zyh);
                    if (data != null)
                    {
                        m.zyfy = data.je;
                    }
                });

                var patyjjFee = GetPatyjjFee(orgId, zyhIns);
                zybrlist.ForEach(m => {
                    var data = patyjjFee.FirstOrDefault(n => n.zyh == m.zyh);
                    if (data != null)
                    {
                        m.yjj = data.je;
                    }
                });
            }
            return zybrlist;
        }

        private List<patFeeVo> GetPatFee(string orgId, string zyhIns)
        {
            string sql = "select zyh,sum(je) je from (" +
                        "select zyh, je from [dbo].[V_C_Sys_HbtfZyYpjfb] nolock where zt=1 and OrganizeId=@orgId " + zyhIns +
                        "union all " +
                        "select zyh, je from [dbo].V_C_Sys_HbtfZyXmjfb nolock where zt=1 and OrganizeId=@orgId " + zyhIns +
                        ") a " + 
                        "group by zyh";
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<patFeeVo>(sql.ToString(), par.ToArray());
        }

        private List<patFeeVo> GetPatyjjFee(string orgId, string zyhIns)
        {
            string sql = "select zyh,sum(szje) je from zy_zhszjl where szxz=1 and zt=1 and OrganizeId=@orgId " + zyhIns +
                        "group by zyh";
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<patFeeVo>(sql.ToString(), par.ToArray());
        }

        #region 住院相关
        /// <summary>
        /// 更新住院患者多诊断之主诊断
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="zddm"></param>
        public void UpdatePatInpMultiDiagnosis(string orgId, string zyh, string zddm, string user)
        {
            if (!string.IsNullOrWhiteSpace(zddm))
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    string sql = @"select [rydzdId],a.[OrganizeId],[zyh],b.[zdCode],b.[icd10],b.[zdmc],[zdpx],[CreatorCode],
                                [CreateTime],[LastModifyTime],[LastModifierCode],[px],a.[zt] 
                                from zy_rydzd a with(nolock)
                                left join [NewtouchHIS_Base].[dbo].[V_S_xt_zd] b with(nolock) on b.organizeid in(@orgId,'*') and b.zdcode=@zddm and b.zt=1 
                                where a.zyh=@zyh and a.organizeid=@orgId and a.zdpx=1 and a.zt=1
                                ";
                    SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@zyh",zyh),
                        new SqlParameter("@zddm",zddm)
                    };

                    var patzd = db.FindList<HospMultiDiagnosisEntity>(sql, para).FirstOrDefault();
                    if (patzd != null)
                    {
                        var patold = db.FindEntity<HospMultiDiagnosisEntity>(patzd.rydzdId);
                        if (!string.IsNullOrWhiteSpace(patzd.zdCode))
                        {
                            patold.zdCode = patzd.zdCode;
                            patold.icd10 = patzd.icd10;
                            patold.zdmc = patzd.zdmc;
                            patold.LastModifierCode = user;
                            patold.LastModifyTime = DateTime.Now;

                            db.Update(patold);
                            db.Commit();
                        }
                    }
                    else
                    {
                        throw new FailedCodeException("HOSP_INPATIENT_BASICINFO_IS_NOT_EXIST");
                    }


                }
            }


        }

        /// <summary>
        /// 住院病人信息 分页数据
        /// </summary>
        /// <param name="pagination">分页信息</param>
        /// <param name="zyh">住院号</param>
        /// <param name="xm">姓名</param>
        /// <returns></returns>
        public IList<HospPatientBasicInfoEntity> GetPatSearchList(Pagination pagination, string orgId, string zyh, string xm, string brzybzType = null)
        {
            var sql = @"select zyxx.*,cw.BedNo from zy_brjbxx zyxx
left join xt_brjbxx xtbrxx on xtbrxx.patid = zyxx.patid and xtbrxx.zt = '1' and xtbrxx.OrganizeId = zyxx.OrganizeId
left join [Newtouch_CIS].[dbo].[zy_cwsyjlk] cw  on zyxx.OrganizeId=cw.OrganizeId and zyxx.zyh=cw.zyh and cw.zt='1'
where zyxx.OrganizeId = @orgId and zyxx.zt = '1'
and (@zyh = '%%' or zyxx.zyh like @zyh)
and (@xm = '%%' or zyxx.xm like @xm or xtbrxx.py like @xm)";
            if (string.IsNullOrWhiteSpace(brzybzType))
            {
                sql += " and zyxx.zybz <> '" + ((int)EnumZYBZ.Wry).ToString() + "'";
            }
            else
            {
                sql += " and zyxx.zybz in ('" + brzybzType.Replace(",", "','") + "')";
            }
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@zyh", "%" + (zyh ?? "") + "%"));
            pars.Add(new SqlParameter("@xm", "%" + (xm ?? "") + "%"));
            return this.QueryWithPage<HospPatientBasicInfoEntity>(sql, pagination, pars.ToArray());
        }

        #endregion

        #region 取消入区登记

        /// <summary>
        /// 取消入院
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        public void CancelAdmission(string zyh, string orgId)
        {
            var entity = _HosPatBasicInfoRepository.IQueryable().Where(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").OrderByDescending(p => p.CreateTime).FirstOrDefault();

            if (entity != null)
            {
                if (_sysConfigRepo.GetBoolValueByCode("HOSP_INTERFACE_WITH_CPOE", orgId) == true)
                {
                    //仅新入院可以取消入院登记
                    if (entity.zybz == ((int)EnumZYBZ.Xry).ToString())
                    {
                        entity.zybz = ((int)EnumZYBZ.Wry).ToString();
                        entity.Modify();
                        _HosPatBasicInfoRepository.Update(entity);
                    }
                    else
                    {
                        if (entity.zybz == ((int)EnumZYBZ.Bqz).ToString())
                        {
                            throw new FailedException("请先在医生站取消入区，才能取消入院");
                        }
                        else
                        {
                            throw new FailedException("当前为" + ((EnumZYBZ)Convert.ToInt32(entity.zybz)).GetDescription() + "状态，不能取消入院");
                        }
                    }
                }
                else
                {
                    //没有费用的都已取消入院
                    entity.zybz = ((int)EnumZYBZ.Wry).ToString();
                    entity.Modify();
                    _HosPatBasicInfoRepository.Update(entity);
                }
            }
        }

        #endregion

        /// <summary>
        /// 自费转医保
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="patid"></param>
        /// <param name="cardInfo"></param>
        /// <param name="ryblInfo"></param>
        public void InpatientZFchangetoYB(string orgId, string zyh, int patid, GACardReadInfoDTO cardInfo, GuianRybl21OutInfoEntity ryblInfo)
        {
            var medicalInsurance = _sysConfigRepo.GetValueByCode("Inpatient_MedicalInsurance", orgId);
            if (medicalInsurance == "guian")
            {
                if (string.IsNullOrWhiteSpace(cardInfo.prm_aac001))
                {
                    throw new FailedException("参数异常，cardInfo.prm_aac001");
                }
                if (string.IsNullOrWhiteSpace(ryblInfo.prm_aac001))
                {
                    throw new FailedException("参数异常，ryblInfo.prm_aac001");
                }
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    var zybrxxEntity = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId).First();
                    if (zybrxxEntity.patid != patid)
                    {
                        throw new FailedException("异常：patid错误");
                    }
                    //更新zy_brjbxx
                    zybrxxEntity.brxz = "1";    //普通医保
                    zybrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                    zybrxxEntity.zjh = cardInfo.prm_aac002;
                    zybrxxEntity.kh = cardInfo.prm_aac001;
                    zybrxxEntity.CardType = ((int)EnumCardType.YBJYK).ToString();
                    zybrxxEntity.CardTypeName = EnumCardType.YBJYK.GetDescription();
                    //...
                    zybrxxEntity.Modify();
                    db.Update(zybrxxEntity);

                    var xtbrxxEntity = db.IQueryable<SysPatientBasicInfoEntity>(p => p.patid == patid && p.OrganizeId == orgId && p.zt == "1").First();

                    //if (string.IsNullOrWhiteSpace(xtbrxxEntity.sbbh) || xtbrxxEntity.sbbh != ryblInfo.prm_aac001)
                    //{
                    //    //更新卡信息
                    //    var cardEnitty = db.IQueryable<SysCardEntity>(p => p.patid == patid).First();
                    //    cardEnitty.CardNo = cardInfo.prm_aac001;
                    //    cardEnitty.CardType = ((int)EnumCardType.YBJYK).ToString();
                    //    cardEnitty.CardTypeName = EnumCardType.YBJYK.GetDescription();
                    //    cardEnitty.Modify();
                    //    db.Update(cardEnitty);
                    //}
                    //else 否则不需要更新卡信息

                    //更新xt_brjbxx
                    //xtbrxxEntity.brxz = "1";    //普通医保
                    xtbrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                    xtbrxxEntity.zjh = cardInfo.prm_aac002;
                    //xtbrxxEntity.sbbh = ryblInfo.prm_aac001;
                    //xtbrxxEntity.cbdbm = cardInfo.prm_yab003;
                    //xtbrxxEntity.zxsbbf = ryblInfo.prm_ykb065;
                    //...
                    xtbrxxEntity.Modify();
                    db.Update(xtbrxxEntity);

                    var ga21List = db.IQueryable<GuianRybl21OutInfoEntity>(p => p.prm_ykc010 == zyh && p.OrganizeId == orgId).ToList();
                    if (ga21List.Count > 1)
                    {
                        throw new FailedException("医保入院办理落地数据异常，一对多");
                    }
                    else if (ga21List.Count == 0)
                    {
                        //ryblInfo.Create(true);
                        ryblInfo.OrganizeId = orgId;
                        db.Insert(ryblInfo);
                    }
                    else
                    {
                        //更新逻辑
                        ryblInfo.MapperTo(ga21List[0]);
                        //ga21List[0].Modify(true);
                        ga21List[0].OrganizeId = orgId;
                        db.Update(ga21List[0]);
                    }

                    db.Commit();
                }
            }

        }

        /// <summary>
        /// 医保转自费
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="patid"></param>
        public void InpatientYBchangetoZF(string orgId, string zyh, int patid)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var zybrxxEntity = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").First();
                if (zybrxxEntity.patid != patid)
                {
                    throw new FailedException("异常：patid错误");
                }
                //更新zy_brjbxx
                zybrxxEntity.brxz = "0";    //自费
                //...
                zybrxxEntity.Modify();
                db.Update(zybrxxEntity);
                CqybMedicalReg02Entity Cq02Entity = db.IQueryable<CqybMedicalReg02Entity>(p => p.zymzh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                if (Cq02Entity != null)
                {
                    Cq02Entity.zt = "0";
                    Cq02Entity.Modify();
                    db.Update(Cq02Entity);
                }
                GuianXnhS04InfoEntity S04InfoEntity =
                    db.IQueryable<GuianXnhS04InfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                if (S04InfoEntity != null)
                {
                    S04InfoEntity.zt = "0";
                    S04InfoEntity.Modify();
                    db.Update(S04InfoEntity);
                }
                db.Commit();
            }

        }

        #region 贵安新农合
        public S04RequestDTO ComposeS04par(SysHosBasicInfoVO vo, string orgId)
        {
            if (string.IsNullOrWhiteSpace(vo.doctor))
            {
                throw new FailedException("新农合入院办理接口医生参数不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.xnhgrbm))
            {
                throw new FailedException("xt_brjbxx表的xnhgrbm字段为空");
            }
            if (string.IsNullOrWhiteSpace(vo.ks))
            {
                throw new FailedException("新农合入院办理接口科室参数不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.ryzd1))
            {
                throw new FailedException("新农合入院办理入院诊断1参数不可为空");
            }
            S04RequestDTO dto = new S04RequestDTO();
            dto.memberId = vo.xnhgrbm;
            dto.inpatientNo = vo.zyh;
            dto.admissionDate = DateTime.Parse(vo.ryrq).ToString("yyyy-MM-dd");
            allTT = _ttCataloguesComparisonDmnService.GetTTItem(orgId, "ks", "guianxinnonghe");
            if (allTT != null && allTT.Count() > 0 && vo.ks != null && allTT.FirstOrDefault(p => p.Code == vo.ks) != null)
            {
                dto.admissionDepartments = allTT.FirstOrDefault(p => p.Code == vo.ks).TTCode;
            }
            else
            {
                throw new FailedException("新农合入院办理接口科室缺少对照");
            }
            dto.treatingPhysician = vo.doctor ?? "";
            //入院状态
            allTT = _ttCataloguesComparisonDmnService.GetTTItem(orgId, "ryzt", "guianxinnonghe");
            if (!string.IsNullOrWhiteSpace(vo.rybq))
            {
                if (allTT != null && allTT.Count() > 0 && allTT.FirstOrDefault(p => p.Code == vo.rybq) != null)
                {

                    dto.admissionStatus = allTT.FirstOrDefault(p => p.Code == vo.rybq).TTCode;
                }
                else
                {
                    throw new FailedException("新农合入院办理接口入院状态缺少对照");
                }
            }

            dto.diseaseCode = vo.ryzd1;
            dto.initialDiagnosis = "";
            dto.surgeryCode = "";
            dto.wardArea = vo.bq ?? "";
            dto.wardNo = "";
            dto.berthNo = vo.cw ?? "";
            dto.isIncrease = "";
            dto.isReferra = "0";
            dto.isReferraNo = "";
            //dto.isReferraNo = ;
            dto.inpatientTypeOflocal = "";
            dto.bxAccount = "";
            dto.tel = "";
            dto.bankCardNo = "";
            dto.accountHolder = "";
            dto.holderRelation = "";
            dto.remark = "";
            dto.bigDiseaseNo = "";
            //dto.disList =new List<valueList>();
            //valueList dis = new valueList();
            //dis.disCode = vo.ryzd1;
            //dto.disList.Add(dis);
            dto.uploadType = "";
            dto.isTransProvincial = "0";
            dto.majorDiseaseICD = "";
            dto.secondMajorDiseaseICD = "";
            dto.threeMajorDiseaseICD = "";
            dto.secondTreatCode = "";
            dto.threeTreatCode = "";
            return dto;
        }

        /// <summary>
        /// 调用S04入院办理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public Response<S04ResponseDTO> S04submit(S04RequestDTO request, string orgId)
        {
            return HospitalizationProxy.GetInstance(orgId).S04(request);

        }
        public Response<string> S05submit(S05RequestDTO request, string orgId)
        {
            return HospitalizationProxy.GetInstance(orgId).S05(request);

        }
        public Response<string> S06submit(S06RequestDTO request, string orgId)
        {
            return HospitalizationProxy.GetInstance(orgId).S06(request);

        }

        public S06RequestDTO ComposeS06par(SysHosBasicInfoVO vo, string orgId)
        {
            if (string.IsNullOrWhiteSpace(vo.inpId))
            {
                throw new FailedException("新农合入院办理信息修改接口补偿序号参数不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.ryrq))
            {
                throw new FailedException("新农合入院办理信息修改接口入院日期参数不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.ks))
            {
                throw new FailedException("新农合入院办理信息修改接口科室不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.doctor))
            {
                throw new FailedException("新农合入院办理信息修改接口医生不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.rybq))
            {
                throw new FailedException("新农合入院办理信息修改入院病情不可为空");
            }
            if (string.IsNullOrWhiteSpace(vo.ryzd1))
            {
                throw new FailedException("新农合入院办理信息修改入院诊断不可为空");
            }
            S06RequestDTO dto = new S06RequestDTO();
            dto.inpId = vo.inpId;
            dto.inpatientNo = vo.zyh;
            dto.admissionDate = DateTime.Parse(vo.ryrq).ToString("yyyy-MM-dd");
            allTT = _ttCataloguesComparisonDmnService.GetTTItem(orgId, "ks", "guianxinnonghe");
            if (allTT != null && allTT.Count() > 0 && vo.ks != null && allTT.FirstOrDefault(p => p.Code == vo.ks) != null)
            {
                dto.admissionDepartments = allTT.FirstOrDefault(p => p.Code == vo.ks).TTCode;
            }
            else
            {
                throw new FailedException("新农合入院办理接口科室缺少对照");
            }
            dto.treatingPhysician = vo.doctor;
            //入院状态
            allTT = _ttCataloguesComparisonDmnService.GetTTItem(orgId, "ryzt", "guianxinnonghe");
            if (!string.IsNullOrWhiteSpace(vo.rybq))
            {
                if (allTT != null && allTT.Count() > 0 && allTT.FirstOrDefault(p => p.Code == vo.rybq) != null)
                {

                    dto.admissionStatus = allTT.FirstOrDefault(p => p.Code == vo.rybq).TTCode;
                }
                else
                {
                    throw new FailedException("新农合入院办理接口入院状态缺少对照");
                }
            }
            dto.diseaseCode = vo.ryzd1;
            dto.initialDiagnosis = "";
            dto.surgeryCode = "";
            dto.wardArea = vo.bq ?? "";
            dto.wardNo = "";
            dto.berthNo = vo.cw ?? "";
            dto.isIncrease = "1";
            dto.isReferra = "0";
            dto.inpatientTypeOflocal = "";
            dto.bxAccount = "";
            dto.tel = "";
            dto.bankCardNo = "";
            dto.accountHolder = "";
            dto.holderRelation = "";
            dto.remark = "";
            return dto;
        }

        /// <summary>
        /// 验证新农合接口对照
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool validateTTentity(string orgId, string code)
        {
            allTT = _ttCataloguesComparisonDmnService.GetTTItem(orgId, code, "guianxinnonghe");
            return allTT != null && allTT.Count() > 0 ? true : false;
        }

        public bool InpatientZFchangetoXNH(string orgId, string zyh, HosPatZFToXNHVO cardInfo, GuianXnhS04InfoEntity ryblInfo, out string msg)
        {
            if (string.IsNullOrWhiteSpace(cardInfo.xnhgrbm))
            {
                msg = "参数异常，cardInfo.xnhgrbm";
                return false;
            }
            if (string.IsNullOrWhiteSpace(cardInfo.xnhylzh))
            {
                msg = "参数异常，cardInfo.xnhylzh";
                return false;
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var zybrxxEntity = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId).First();
                //if (zybrxxEntity.patid != patid)
                //{
                //    msg = "参数异常，patid错误";
                //    return false;
                //}
                //更新zy_brjbxx
                zybrxxEntity.brxz = "8";    //农合
                zybrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                zybrxxEntity.zjh = cardInfo.zjh;
                zybrxxEntity.xian_dz = string.IsNullOrEmpty(cardInfo.xian_dz) ? zybrxxEntity.xian_dz : cardInfo.xian_dz;
                zybrxxEntity.CardType = ((int)EnumCardType.XNHJYK).ToString();
                zybrxxEntity.CardTypeName = EnumCardType.XNHJYK.GetDescription();
                //...
                zybrxxEntity.Modify();
                db.Update(zybrxxEntity);

                var xtbrxxEntity = db.IQueryable<SysPatientBasicInfoEntity>(p => p.patid == zybrxxEntity.patid && p.OrganizeId == orgId && p.zt == "1").First();

                //if (string.IsNullOrWhiteSpace(xtbrxxEntity.xnhgrbm) || xtbrxxEntity.xnhgrbm != cardInfo.xnhgrbm)
                //{
                //    //更新卡信息
                //    var cardEnitty = db.IQueryable<SysCardEntity>(p => p.patid == zybrxxEntity.patid).First();
                //    //cardEnitty.CardNo = cardInfo.prm_aac001;
                //    cardEnitty.CardType = ((int)EnumCardType.XNHJYK).ToString();
                //    cardEnitty.CardTypeName = EnumCardType.XNHJYK.GetDescription();
                //    cardEnitty.Modify();
                //    db.Update(cardEnitty);
                //}
                //else 否则不需要更新卡信息

                //更新xt_brjbxx
                //xtbrxxEntity.brxz = "8";    //农合
                xtbrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                xtbrxxEntity.zjh = cardInfo.zjh;
                //xtbrxxEntity.xnhgrbm = cardInfo.xnhgrbm;
                //xtbrxxEntity.xnhylzh = cardInfo.xnhylzh;
                //...
                xtbrxxEntity.Modify();
                db.Update(xtbrxxEntity);

                var ga21List = db.IQueryable<GuianXnhS04InfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.inpId == ryblInfo.inpId).ToList();
                if (ga21List.Count > 1)
                {
                    msg = "医保入院办理落地数据异常，一对多";
                    return false;
                }
                else if (ga21List.Count == 0)
                {
                    ryblInfo.Create(true);
                    ryblInfo.OrganizeId = orgId;
                    db.Insert(ryblInfo);
                }
                else
                {
                    //更新逻辑
                    ryblInfo.MapperTo(ga21List[0]);
                    //ga21List[0].Modify(true);
                    ga21List[0].OrganizeId = orgId;
                    db.Update(ga21List[0]);
                }

                db.Commit();
            }

            msg = "";
            return true;
        }


        public bool InpatXnhInsertS04data(GuianXnhS04InfoEntity ryblInfo, out string msg)
        {
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    GuianXnhS04InfoEntity oldEntity =
                        db.IQueryable<GuianXnhS04InfoEntity>(p => p.zyh == ryblInfo.zyh && p.zt == "1").FirstOrDefault();
                    if (oldEntity != null)
                    {
                        oldEntity.zt = "0";
                        oldEntity.Modify();
                        db.Update(oldEntity);
                    }

                    ryblInfo.Create(true);
                    db.Insert(ryblInfo);
                    db.Commit();
                }
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }

        }

        public SysHosBasicInfoVO GetZfToXnhPatInfo(string zyh, string orgId)
        {
            var strSql = new StringBuilder(@"
SELECT  a.doctor ,
        b.xnhgrbm ,
        a.ks ,
        c.zdCode ,
        c.zdCode ryzd1,
        a.zyh ,
        CONVERT(VARCHAR(20), a.ryrq ,120) ryrq ,
        a.rybq ,
        a.bq ,
        a.cw
FROM    [NewtouchHIS_Sett].[dbo].[zy_brjbxx] a
        LEFT JOIN NewtouchHIS_Sett..xt_brjbxx b ON a.patid = b.patid
                                                   AND a.OrganizeId = b.OrganizeId
                                                   AND b.zt = '1'
        LEFT JOIN ( SELECT TOP 1
                            *
                    FROM    [NewtouchHIS_Sett].[dbo].[zy_rydzd]
                    WHERE   zt = '1'
                            AND zyh = @zyh
                    ORDER BY px
                  ) c ON a.zyh = c.zyh
                         AND a.OrganizeId = c.OrganizeId
WHERE   a.zt = '1'
        AND a.OrganizeId =@OrganizeId
        AND a.zyh = @zyh ");
            DbParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@OrganizeId", orgId)

            };
            return FirstOrDefault<SysHosBasicInfoVO>(strSql.ToString(), par.ToArray());
        }

        public string GetUpqdScData(string kssj, string jssj)
        {
            var strSql = new StringBuilder(@"select distinct stuff((select distinct ','+zyh from drjk_zyjs_output where zt='1' and jsqd_scrq is null and jsqd_sclsh is null
and  czrq>=@kssj and czrq<=@jssj FOR XML Path('')),1,1,'') as zyh  from  drjk_zyjs_output");
            DbParameter[] par =
            {
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj)

            };
            return FirstOrDefault<string>(strSql.ToString(), par.ToArray());
        }
        #endregion

        #region 重庆医保
        public Input_Bbrxx GetCQjzdjInfo(string zyh, string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
 SELECT  a.zyh hisId ,b.jylsh mdtrt_id,
        case when rytj is not null  then rytj else '21' end med_type ,e.cbdbm insuplc_admdvs,e.grbh psn_no,xzlx insutype,
        '' operatorId,'' operatorName,jzlx mdtrt_cert_type,kh mdtrt_cert_no,isnull(bzbm,'') dise_codg,isnull(bzmc,'') dise_name
FROM    NewtouchHIS_Sett..zy_brjbxx a
        left join [cqyb_OutPut02] b on a.zyh=b.zymzh and a.OrganizeId=b.OrganizeId and b.zt='1'
        LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] c ON c.OrganizeId = a.OrganizeId
                                                              AND c.gh = a.doctor
                                                              AND c.zt = '1'
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[xt_card] e ON e.cardno = a.kh
                                                            AND e.OrganizeId = a.OrganizeId
                                                            AND e.zt = '1'
       
WHERE   a.zyh = @zyh
        AND a.OrganizeId = @OrganizeId
        AND a.zt = '1' ");
            SqlParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@OrganizeId", orgId)
            };
            return this.FirstOrDefault<Input_Bbrxx>(strSql.ToString(), par);
        }

        /// <summary>
        /// 门诊自费转医保
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="patid"></param>
        /// <param name="cardInfo"></param>
        /// <param name="ryblInfo"></param>
        public void OutPatZFchangetoYB(string orgId, string mzh, int patid, ZYToYBDto patInfo)
        {
            //卡是否存在 不存在新增医保基础信息
            var brxzlist = _sysPatientNatureRepo.IQueryable().Where(p=>p.OrganizeId==orgId && p.zt=="1");
            var list = _SysCardRepository.IQueryable().Where(p => p.OrganizeId == orgId && p.patid == patid && p.zt == "1").ToList();
            var isExist = list.Where(m=>m.CardType==patInfo.jzpzlx).FirstOrDefault();
            SysCardEntity entity = new SysCardEntity();
            if (isExist==null)
            {
                entity.zt = "1";
                if (patInfo.jzpzlx != ((int)EnumCardType.YBJYK).ToString())
                {
                    entity.CardNo = _SysCardRepository.GetCardSerialNo(orgId); 
                }
                else {
                    entity.CardNo = patInfo.kh; 
                }
                entity.CardType = patInfo.jzpzlx;
                entity.CardTypeName= ((EnumCardType)(Convert.ToInt32(patInfo.jzpzlx))).GetDescription();
                entity.cbdbm = patInfo.cbdbm;
                entity.cblb = patInfo.cblb;
                entity.grbh = patInfo.grbh;
                entity.xzlx = patInfo.xzlx;
                entity.CardId = Guid.NewGuid().ToString();
                entity.patid = patid;
                entity.hzxm = list.FirstOrDefault().hzxm;
                entity.accountattr = patInfo.cblb;
                string zhbz = patInfo.ybver != "shanghaiV5" ? patInfo.xzlx: (string.IsNullOrWhiteSpace(entity.accountattr) ? "" : entity.accountattr.Substring(11,1));
                if (string.IsNullOrWhiteSpace(zhbz))
                {
                    entity.brxz = ((int)EnumBrxz.pt).ToString();
                }
                else {
                    var brlx = brxzlist.Where(p => p.insutype == zhbz).FirstOrDefault();
                    entity.brxz = brlx == null? ((int)EnumBrxz.pt).ToString() : brlx.brxz;
                }
                _SysCardRepository.SubmitForm(entity,null,orgId);
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var ghxxEntity = db.IQueryable<OutpatientRegistEntity>(p => p.mzh == mzh && p.OrganizeId == orgId).First();
                
                //更新挂号
                ghxxEntity.brxz = string.IsNullOrWhiteSpace(entity.CardNo) ? isExist.brxz : entity.brxz;    //普通医保
                ghxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                ghxxEntity.zjh = patInfo.sfzh;
                ghxxEntity.kh = string.IsNullOrWhiteSpace(entity.CardNo) ? isExist.CardNo : entity.CardNo; //patInfo.kh;
                ghxxEntity.CardType = string.IsNullOrWhiteSpace(entity.CardNo)? isExist.CardType: entity.CardType;
                ghxxEntity.CardTypeName = string.IsNullOrWhiteSpace(entity.CardNo) ? isExist.CardTypeName : entity.CardTypeName;
                ghxxEntity.patid = patid;
                ghxxEntity.jzid = patInfo.jzid;
                ghxxEntity.jzpzlx = patInfo.jzpzlx;
                ghxxEntity.Modify();
                db.Update(ghxxEntity);

                var xtbrxxEntity = db.IQueryable<SysPatientBasicInfoEntity>(p => p.patid == patid && p.OrganizeId == orgId && p.zt == "1").First();

                //更新xt_brjbxx
                xtbrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                xtbrxxEntity.zjh =string.IsNullOrWhiteSpace(patInfo.sfzh)? xtbrxxEntity.zjh :patInfo.sfzh;
                xtbrxxEntity.Modify();
                db.Update(xtbrxxEntity);

                db.Commit();
            }

        }

        public void InPatZFchangetoYB(string orgId, string zyh, int patid, ZYToYBDto patInfo, CqybMedicalReg02Entity ryblInfo)
        {

            if (string.IsNullOrWhiteSpace(patInfo.sfzh))
            {
                throw new FailedException("参数异常，身份证号为空");
            }
            //if (string.IsNullOrWhiteSpace(patInfo.kh))
            //{
            //    throw new FailedException("参数异常，卡号为空");
            //}
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //if (patInfo.kh.Count() > 15)
                //{
                // var sfsobj = _sysPatientBasicInfoRepo.IQueryable().Where(p => p.zjh == patInfo.kh && p.OrganizeId == orgId && p.brxz == "1" && p.zt == "1").FirstOrDefault();
                //patInfo.kh = sfsobj.sbbh;
                // }
                var cardEnitty = db.IQueryable<SysCardEntity>(p => p.patid == patid && p.OrganizeId==orgId && p.CardType==patInfo.jzpzlx).First();
                var oldpatid = 0;
                var zybrxxEntity = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId).First();
                //if (zybrxxEntity.patid != patid)
                //{
                //	throw new FailedException("异常：patid错误");
                //}
                oldpatid = zybrxxEntity.patid;
                //更新zy_brjbxx
                zybrxxEntity.brxz = cardEnitty.brxz;    //普通医保
                zybrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                zybrxxEntity.zjh = patInfo.sfzh;
                zybrxxEntity.kh = cardEnitty.CardNo;//patInfo.kh;
                zybrxxEntity.CardType = patInfo.jzpzlx;
                zybrxxEntity.CardTypeName = ((EnumCardType)(Convert.ToInt32(patInfo.jzpzlx))).GetDescription();
                zybrxxEntity.patid = patid;
                zybrxxEntity.jzh = patInfo.jzh;

                zybrxxEntity.rytj = patInfo.ryfs;
                //...
                zybrxxEntity.Modify();
                db.Update(zybrxxEntity);

                //预交金账户处理  自费预交金账户费用转到医保预交金账户上，当前预交金账户zt作废(历史记录)，
                //收支明细zhcod,patid变更为预交金账户相应
                try
                {
                    if (patid != oldpatid)
                    {
                        // oldpatid(自费信息ID)  patid(医保信息ID)
                        var yjjzhEntity = db.IQueryable<SysAccountEntity>(p => p.patid == oldpatid && p.OrganizeId == orgId && p.zhxz == ((int)EnumXTZHXZ.ZYYJKZH) && p.zt == "1").ToList();
                        var ybyjjzhEntity = db.IQueryable<SysAccountEntity>(p => p.patid == patid && p.OrganizeId == orgId && p.zhxz == ((int)EnumXTZHXZ.ZYYJKZH) && p.zt == "1").ToList();
                        var szjlentity = db.IQueryable<SysAccountRevenueAndExpenseEntity>(p => p.patid == patid && p.OrganizeId == orgId && p.zt == "1").OrderByDescending(p => p.CreateTime).FirstOrDefault();
                        var zfdataInfo = db.IQueryable<SysAccountRevenueAndExpenseEntity>(p => p.patid == oldpatid && p.OrganizeId == orgId && p.zt == "1").ToList();

                        if (ybyjjzhEntity.Count == 1)//存在医保预交金账户
                        {
                            if (yjjzhEntity.Count == 1)
                            {
                                SysAccountEntity zhentity = new SysAccountEntity();
                                zhentity = yjjzhEntity[0];
                                //zhentity.patid = patid;
                                zhentity.zt = "0";
                                zhentity.Modify();
                                db.Update(zhentity);
                                //转移预交金金额
                                SysAccountEntity ybentity = new SysAccountEntity();
                                ybentity = ybyjjzhEntity[0];
                                ybentity.zhye += zhentity.zhye;
                                ybentity.Modify();
                                db.Update(ybentity);
                                var ybye = szjlentity == null ? Convert.ToDecimal(0.00) : szjlentity.zhye;
                                foreach (var item in zfdataInfo)
                                {
                                    item.zhye += ybye;
                                    item.zhCode = ybyjjzhEntity[0].zhCode;
                                    item.patid = ybyjjzhEntity[0].patid;
                                    db.Update(item);
                                }

                            }
                        }
                        else if (yjjzhEntity.Count == 1)
                        {
                            SysAccountEntity zhentity = new SysAccountEntity();
                            zhentity = yjjzhEntity[0];
                            zhentity.patid = patid;
                            zhentity.Modify();
                            db.Update(zhentity);

                            foreach (var item in zfdataInfo)
                            {
                                item.patid = patid;
                                db.Update(item);
                            }
                        }
                    }


                }
                catch (Exception e)
                {
                }

                var xtbrxxEntity = db.IQueryable<SysPatientBasicInfoEntity>(p => p.patid == patid && p.OrganizeId == orgId && p.zt == "1").First();

                // if (string.IsNullOrWhiteSpace(xtbrxxEntity.sbbh) || xtbrxxEntity.sbbh != patInfo.kh)
                //  {
                //更新卡信息
                //var cardEnitty = db.IQueryable<SysCardEntity>(p => p.patid == patid).First();
                //cardEnitty.CardNo = patInfo.kh;
                //cardEnitty.CardType = ((int)EnumCardType.YBJYK).ToString();
                //cardEnitty.CardTypeName = EnumCardType.YBJYK.GetDescription();
                //cardEnitty.Modify();
                //db.Update(cardEnitty);
                //  }
                //else 否则不需要更新卡信息

                //更新xt_brjbxx
                //xtbrxxEntity.brxz = "1";    //普通医保
                xtbrxxEntity.zjlx = ((int)EnumZJLX.sfz).ToString();
                xtbrxxEntity.zjh = patInfo.sfzh;
                //xtbrxxEntity.sbbh = patInfo.kh;
                //xtbrxxEntity.cblb = patInfo.cblb;
                //xtbrxxEntity.zftoybId = oldpatid;
                //...
                xtbrxxEntity.Modify();
                db.Update(xtbrxxEntity);

                var ga21List = db.IQueryable<CqybMedicalReg02Entity>(p => p.zymzh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                if (ga21List.Count > 1)
                {
                    throw new FailedException("医保入院办理落地数据异常，一对多");
                }
                else if (ga21List.Count == 0)
                {
                    ryblInfo.Create();
                    ryblInfo.zymzh = zyh;
                    ryblInfo.jytype = "2";
                    ryblInfo.OrganizeId = orgId;
                    db.Insert(ryblInfo);
                }
                else
                {
                    //更新逻辑
                    ga21List[0].jylsh = ryblInfo.jylsh;
                    ga21List[0].zhye = ryblInfo.zhye;
                    ga21List[0].yzcyymc = ryblInfo.yzcyymc;
                    ga21List[0].OrganizeId = orgId;
                    db.Update(ga21List[0]);
                }

                db.Commit();
            }

        }
        public void UpdateCqybOut02(string zyh, string orgId)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                CqybMedicalReg02Entity Cq02Entity = db.IQueryable<CqybMedicalReg02Entity>(p => p.zymzh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                if (Cq02Entity != null)
                {
                    Cq02Entity.zt = "0";
                    Cq02Entity.Modify();
                    db.Update(Cq02Entity);
                    db.Commit();
                }
            }
        }
        #endregion

        #region 秦皇岛医保
        public Input_Bbrxx GetQHDjzdjInfo(string zyh, string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
 SELECT  a.zyh hisId ,b.mdtrt_id mdtrt_id,a.OrganizeId orgid,
        case when rytj is not null  then rytj else '21' end med_type ,e.cbdbm insuplc_admdvs,e.grbh psn_no,e.xzlx insutype,
        c.gh operatorId,c.name operatorName,'03' mdtrt_cert_type,kh mdtrt_cert_no,isnull('','') dise_codg,isnull('','') dise_name
FROM    NewtouchHIS_Sett..zy_brjbxx a
        left join [drjk_rybl_input] b on a.zyh=b.zyh and b.zt='1'
        LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] c ON c.OrganizeId = a.OrganizeId
                                                              AND c.gh = a.doctor
                                                              AND c.zt = '1'
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[xt_brjbxx] e ON e.patid = a.patid
                                                            AND e.OrganizeId = a.OrganizeId
                                                            AND e.zt = '1'
       
WHERE   
a.zyh = @zyh
        and e.brxz!='0' 
        AND a.OrganizeId = @OrganizeId
        AND a.zt = '1' ");
            SqlParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@OrganizeId", orgId)
            };
            return this.FirstOrDefault<Input_Bbrxx>(strSql.ToString(), par);
        }
        public string GetQHDSzshData(string zyh, string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@" select jr_id from [dbo].[Drjk_sqsh_Output] where issqsz='1'  and zt='1' and zyh=@zyh ");
            SqlParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@OrganizeId", orgId)
            };
            return this.FirstOrDefault<string>(strSql.ToString(), par);
        }
        public string GetQHDTFDate(string zyh, string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
select distinct
 stuff((
select feedetl_sn+',' from(
select mxsc.feedetl_sn from zy_xmjfb xmjfb1
left join zy_xmjfb xmjfb2 on xmjfb1.cxzyjfbbh=xmjfb2.jfbbh
left join [drjk_zyfymxsc_input] mxsc on mxsc.feedetl_sn='XM'+convert(varchar(20),xmjfb2.jfbbh)
 where xmjfb1.zyh=@zyh and xmjfb1.cxzyjfbbh!='0' and xmjfb1.zt='1' and mxsc.zt!='0'  and (xmjfb1.sl+xmjfb2.sl)!=mxsc.cnt
 AND xmjfb1.OrganizeId = @OrganizeId
 union all
 select mxsc.feedetl_sn from zy_ypjfb ypjfb1
left join zy_ypjfb ypjfb2 on ypjfb1.cxzyjfbbh=ypjfb2.jfbbh
left join [drjk_zyfymxsc_input] mxsc on mxsc.feedetl_sn='YP'+convert(varchar(20),ypjfb2.jfbbh)
 where ypjfb1.zyh=@zyh and ypjfb1.cxzyjfbbh!='0' and ypjfb1.zt='1' and mxsc.zt!='0' and (ypjfb1.sl+ypjfb2.sl)!=mxsc.cnt
AND ypjfb1.OrganizeId = @OrganizeId
union all 
select mxsc.feedetl_sn from zy_xmjfb xmjfb
left join [drjk_zyfymxsc_input] mxsc on mxsc.feedetl_sn='XM'+convert(varchar(20),xmjfb.jfbbh)
 where xmjfb.zyh=@zyh  and mxsc.zt!='0'  and xmjfb.zt='0' 
 AND xmjfb.OrganizeId = @OrganizeId
 union all
 select mxsc.feedetl_sn from zy_ypjfb ypjfb
left join [drjk_zyfymxsc_input] mxsc on mxsc.feedetl_sn='XM'+convert(varchar(20),ypjfb.jfbbh)
 where ypjfb.zyh=@zyh  and mxsc.zt!='0'  and ypjfb.zt='0'
 AND ypjfb.OrganizeId =@OrganizeId
) a for xml path('')),1,0,'') feedetl_sn ");
            SqlParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@OrganizeId", orgId)
            };
            return this.FirstOrDefault<string>(strSql.ToString(), par);
        }
        #endregion

        #region   医保业务
        public SysPatBasicInfoVo ValidateFirstVisit(string sfzh, string xm,string orgId,string kh=null,string jzpzlx=null)
        {
            StringBuilder strSql = new StringBuilder();
            IList<SqlParameter> inSqlParameterList = null;
            inSqlParameterList = new List<SqlParameter>();
            strSql.AppendFormat(@"select  xtxx.patid,xtxx.zjh,
                                   kh.CardNo sbbh from xt_brjbxx xtxx
                                  inner join xt_card kh 
                                   on xtxx.patid=kh.patid and xtxx.OrganizeId=kh.OrganizeId and kh.zt=1
                                  where ((CardNo=@kh and CardType=@jzpzlx)
                                     or (xtxx.zjh=@zjh and CardType=@jzpzlx)) 
                                    and xtxx.OrganizeId=@orgId
                                    and xtxx.zt='1' ");//and xm=@xm and kh.brxz<>'0'

            inSqlParameterList.Add(new SqlParameter("@orgId", orgId));
            inSqlParameterList.Add(new SqlParameter("@xm", xm ?? ""));
            inSqlParameterList.Add(new SqlParameter("@zjh", sfzh ?? ""));
            inSqlParameterList.Add(new SqlParameter("@kh", kh ?? ""));
            inSqlParameterList.Add(new SqlParameter("@jzpzlx", jzpzlx ?? ""));
            var data = FindList<SysPatBasicInfoVo>(strSql.ToString(), inSqlParameterList.ToArray());

            if (data != null && data.Count > 0)
            {
                return data.FirstOrDefault();
            }
            return null;
        }
        #endregion

        public bool SignInStateUpdate(string mzh, string calledstu,string yhcode,string orgId)
        {
            var paraList = new DbParameter[]
            {
                new SqlParameter("@mzh", mzh),
                new SqlParameter("@calledstu", calledstu),
                new SqlParameter("@yhcode", yhcode),
                new SqlParameter("@orgId", orgId)
            };
            var strSql = new StringBuilder();
            strSql.Append(@"EXEC dbo.rpt_PatientSignInStateUpdate @Calledstu = @calledstu, @Mzh = @mzh, @Yhcode = @yhcode, @OrgId = @orgId");
            _dataContext.Database.ExecuteSqlCommand(strSql.ToString(), paraList.ToArray());
            return true;
        }
        public bool PatientAppointment(string mzh, string orgId)
        {
            var paraList = new DbParameter[]
            {
                new SqlParameter("@mzh", mzh),
                new SqlParameter("@orgId", orgId)
            };
            var strSql = new StringBuilder();
            strSql.Append(@"EXEC dbo.rpt_PatientAppointment @Mzh = @mzh, @OrgId = @orgId");
            _dataContext.Database.ExecuteSqlCommand(strSql.ToString(), paraList.ToArray());
            return true;
        }
        

        public IList<InPatientDTO> InPatientInfoQuery(InPatientInfoQueryRequest dto)
        {

            List<SqlParameter> para = new List<SqlParameter>();
            para.Add(new SqlParameter("@orgId", ConfigurationManager.AppSettings[dto.HospitalID]));
            
            string sql = @"select zyxx.rytj ryfs,zyxx.zyh, isnull(zyxx.xm, xx.xm) xm
	--else 年龄 出生年月与入院日期作比较 实时计算一下
,case when isnull(zyxx.nl,0) <> 0 then zyxx.nl else (case when zyxx.csny is not null and zyxx.ryrq is not null then datediff(yy,zyxx.csny,zyxx.ryrq) else 0 end) end nl
,case when isnull(zyxx.nlshow,'0') <> '0' then zyxx.nlshow else '0' end nlshow
,zyxx.brxz,xz.brxzmc,isnull(zyxx.blh, xx.blh) blh
--,cyzd.zddm AS zzdCode,cyzd.zdmc zzdmc
--,case cyzd.cyqk when 1 then '治愈' when 2 then '好转' when 3 then '未愈' when 4 then '死亡' when 5 then '其他' when 6 then '转院' end cyqk
,CONVERT(varchar(100), zyxx.csny, 23) AS csny
,CONVERT(varchar(100), zyxx.ryrq, 23) AS ryrq
,CONVERT(varchar(100), zyxx.cyrq, 23) AS cyrq
,zyxx.bq bqCode,bq.bqmc bqmc
,zybz zybz
,case zyxx.zybz when '0' then '入院登记' when '1' then '病区中' when '2' then '病区出院' when '3' then '病人出院' when '9' then '作废记录' else '' end AS zybzmc
,isnull(zyxx.LastModifyTime, zyxx.CreateTime) UpdateTime
,xx.py, '' wb
,zyxx.zjlx zjlx
,(CASE zyxx.zjlx WHEN '1'THEN '身份证' WHEN '2' THEN '护照' WHEN '3' THEN '军官证' ELSE '其他' END ) zjlxValue
,zyxx.zjh zjh
,(CASE zyxx.zjlx WHEN 1 THEN zyxx.zjh ELSE '' END) AS idCardNo
,(CASE zyxx.xb WHEN '1' THEN '1' WHEN '2' THEN '2' ELSE '3' END ) as sex
,(CASE zyxx.xb WHEN '1' THEN '男' WHEN '2' THEN '女' ELSE '不详' END ) as sexValue
,zyxx.lxr AS contPerName 
,zyxx.lxrdh AS contPerPhoneNum 
,lxrItem.Code AS contPerRel 
,lxrItem.Name AS contPerRelValue
,zyxx.ks, ks.Name ksmc, zyxx.doctor ys, ysStaff.Name ysxm
,zyxx.kh, zyxx.CardType, zyxx.CardTypeName
from zy_brjbxx(nolock) zyxx
left join xt_brjbxx(nolock) xx
on zyxx.patid = xx.patid AND zyxx.OrganizeId = xx.OrganizeId  AND xx.zt = '1'
left join xt_brxz(nolock) xz
ON xz.brxz = zyxx.brxz and xz.OrganizeId = zyxx.OrganizeId
left join zy_rydzd(nolock) zd
ON zd.zyh = zyxx.zyh AND zd.OrganizeId = zyxx.OrganizeId and zd.zdpx = 1 AND zd.zt = '1'
left join [NewtouchHIS_Base]..V_S_xt_bq(nolock) bq
ON bq.bqCode = zyxx.bq AND bq.OrganizeId = zyxx.OrganizeId  AND bq.zt = '1'
LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail lxrItem 
ON ( lxrItem.OrganizeId = zyxx.OrganizeId OR lxrItem.OrganizeId = '*')
AND lxrItem.Code = zyxx.lxrgx
AND lxrItem.CateCode = 'RelativeType'
left join NewtouchHIS_Base..V_S_Sys_Department ks
on ks.Code = zyxx.ks and ks.OrganizeId = zyxx.OrganizeId
left join NewtouchHIS_Base..V_S_Sys_Staff ysStaff
on ysStaff.gh = zyxx.doctor and ysStaff.OrganizeId = zyxx.OrganizeId
--left join  [Newtouch_CIS].[dbo].[zy_PatDxInfo] cyzd on cyzd.zdlx=0 and cyzd.zdlb=2 and cyzd.zt=1 and cyzd.zyh=zyxx.zyh and cyzd.organizeId=zyxx.organizeId
where zyxx.zt = '1' and zyxx.OrganizeId=@orgId";


            if (!string.IsNullOrEmpty(dto.xm))
            {
                sql += " and zyxx.xm like @xm ";
                para.Add(new SqlParameter("@xm","%"+ dto.xm+"%"));
            }
            if (!string.IsNullOrEmpty(dto.sfzh))
            {
                sql += " and zyxx.zjh like @sfzh ";
                para.Add(new SqlParameter("@sfzh", "%" + dto.sfzh + "%"));
            }
            if (!string.IsNullOrEmpty(dto.zyh))
            {
                sql += " and zyxx.zyh =@zyh ";
                para.Add(new SqlParameter("@zyh", dto.zyh));
            }
            return this.FindList<InPatientDTO>(sql, para.ToArray());

        }

        public IList<OutPatientRegistrationInfoDTO> OutPatientConsultationQuery(OutPatientConsultationQueryRequest dto)
        {

            List<SqlParameter> para = new List<SqlParameter>();
            para.Add(new SqlParameter("@orgId", ConfigurationManager.AppSettings[dto.HospitalID]));

            //para.Add(new SqlParameter("@orgId", "6d5752a7-234a-403e-aa1c-df8b45d3469f"));
            string sql = @"SELECT  gh.mzh AS mzh ,gh.blh as blh
		,gh.jzbz jiuzhenbz
		,(CASE gh.jzbz WHEN '1' THEN '待就诊' WHEN '2' THEN '就诊中' WHEN '3' THEN '结束就诊' ELSE '不详' END ) as jiuzhenbzValue
		,gh.ys ,ysxmgl.Name ysxm, gh.ks, ksmcgl.Name ksmc
		,xz.brxz brxz ,xz.brxzmc brxzmc ,gh.xm AS brxm
		,(CASE xx.hf WHEN '0'THEN '0' WHEN '1' THEN '1' ELSE '2' END ) maritalStatus
		,(CASE xx.hf WHEN '0'THEN '未婚' WHEN '1' THEN '已婚' ELSE '不详' END ) maritalStatusValue
		,gh.zjlx zjlx
		,(CASE gh.zjlx WHEN '1'THEN '身份证' WHEN '2' THEN '护照' WHEN '3' THEN '军官证' ELSE '其他' END ) zjlxValue
		,gh.zjh zjh
		,(CASE gh.zjlx WHEN 1 THEN gh.zjh ELSE '' END) AS idCardNo
		,xx.dh AS ContactNum
		,(CASE gh.xb WHEN '1' THEN '1' WHEN '2' THEN '2' ELSE '3' END ) as sex
		,(CASE gh.xb WHEN '1' THEN '男' WHEN '2' THEN '女' ELSE '不详' END ) as sexValue
		,CONVERT(VARCHAR(100), xx.csny, 23) AS birth
		,case when gh.zt = '1' and isnull(xx.zt,'1') = '1' and isnull(xz.zt,'1') = '1' then '1' else '9' end AS [status]
		,case when gh.zt = '1' and isnull(xx.zt,'1') = '1' and isnull(xz.zt,'1') = '1' then '正常挂号' else '作废记录' end AS statusValue
		,xx.xian_sheng AS province,xx.xian_shi AS city ,xx.xian_xian AS county 
		,xx.xian_dz AS [ADDRESS] 
		,CONVERT(varchar(100), isnull(gh.ghrq, gh.CreateTime), 20) AS ghsj
		,gh.lxr AS contPerName 
		,gh.lxrdh AS contPerPhoneNum 
		,Citem.Code AS contPerRel 
		,Citem.Name AS contPerRelValue 
		,CONVERT(VARCHAR(100) 
		,(case when isnull( xx.LastModifyTime, xx.CreateTime) > isnull( gh.LastModifyTime, gh.CreateTime)
			then isnull( xx.LastModifyTime, xx.CreateTime) else isnull( gh.LastModifyTime, gh.CreateTime)
			end)
			,20) AS UpdateTime
		,CONVERT(VARCHAR(100) ,gh.CreateTime,20) as operatingTime
		,isnull(gh.mjzbz,'1') mjzbz
		,gh.ybjsh,gh.kh sbbh,kh.cbdbm, case when gh.fzbz = 1 then '1' else '' end fzbz
		,(CASE isnull(gh.mjzbz,'1') WHEN '1'THEN '普通门诊' WHEN '2' THEN '急诊' WHEN '3' THEN '专家门诊' ELSE '' END ) mjzbzValue
		,xx.py
		,gh.kh
		,gh.nlshow
		from mz_gh gh
		left join xt_brjbxx xx
		on gh.patId = xx.patId and gh.OrganizeId = xx.OrganizeId
		left join xt_card kh on kh.cardno=gh.kh and kh.OrganizeId=gh.OrganizeId and kh.zt=1
		LEFT JOIN xt_brxz xz
		ON xz.brxz = gh.brxz	AND xz.OrganizeId = gh.OrganizeId
		LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail Citem ON ( Citem.OrganizeId = gh.OrganizeId
                                                    OR citem.OrganizeId = '*'
                                                    )
                                                    AND citem.Code = gh.lxrgx
                                                    AND citem.CateCode = 'RelativeType'
		LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff ysxmgl
		on ysxmgl.gh = gh.ys and ysxmgl.OrganizeId = gh.OrganizeId
		LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department ksmcgl
		on ksmcgl.Code = gh.ks and ksmcgl.OrganizeId = gh.OrganizeId

		WHERE gh.zt = '1' and isnull(gh.ghzt,'') <> '2' AND xx.zt = '1' AND xz.zt = '1' and isnull(gh.mzh, '') <> '' and gh.OrganizeId ==@orgId";


            if (!string.IsNullOrEmpty(dto.xm))
            {
                sql += " and zyxx.xm like @xm ";
                para.Add(new SqlParameter("@xm", "%" + dto.xm + "%"));
            }
            if (!string.IsNullOrEmpty(dto.sfzh))
            {
                sql += " and zyxx.zjh like @sfzh ";
                para.Add(new SqlParameter("@sfzh", "%" + dto.sfzh + "%"));
            }
            return this.FindList<OutPatientRegistrationInfoDTO>(sql, para.ToArray());

        }
        public IList<InpatientDayFeeDTO> InpatientDayFee(InpatientDayFeeRequest dto)
        {

            List<SqlParameter> para = new List<SqlParameter>();
            para.Add(new SqlParameter("@orgId", ConfigurationManager.AppSettings[dto.HospitalID]));
            para.Add(new SqlParameter("@zyh", dto.zyh ?? ""));
            para.Add(new SqlParameter("@kssj", dto.kssj ));
            para.Add(new SqlParameter("@jssj", dto.jssj));
            para.Add(new SqlParameter("@pageSize", dto.pageSize ));
            para.Add(new SqlParameter("@pageNum", dto.pageNum));

            var list = this.FindList<InpatientDayFeeDTO>(@"exec rpt_InpatientDayFeeCqkr_api @zyh,@orgId,@kssj,@jssj,@pageSize,@pageNum", para.ToArray());
            return list;
        }


        //入院诊断
        public IList<ryzd> getRyzdByZyh(string zyh,string HospitalID)
        {

            List<SqlParameter> para = new List<SqlParameter>();
            var orgId = ConfigurationManager.AppSettings[HospitalID];
            para.Add(new SqlParameter("@orgId", orgId));
            
            string sql = @"select zdCode zzdCode ,icd10 zzdicd10,zdmc zzdmc ,zdpx from zy_rydzd 
		    WHERE  zt=1 and organizeId =@orgId ";

            if (!string.IsNullOrEmpty(zyh))
            {
                sql += " and zyh=@zyh ";
                para.Add(new SqlParameter("@zyh", zyh));
            }
            sql += " order by zdpx";
            return this.FindList<ryzd>(sql, para.ToArray());

        }
        /// <summary>
        /// 出院诊断
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="HospitalID"></param>
        /// <returns></returns>
        public IList<cyzd> getCyzdByZyh(string zyh, string HospitalID)
        {

            List<SqlParameter> para = new List<SqlParameter>();
            var orgId = ConfigurationManager.AppSettings[HospitalID];
            para.Add(new SqlParameter("@orgId", orgId));

            string sql = @"select 
cyzd.zddm AS zzdCode,cyzd.zdmc zzdmc
,case cyzd.cyqk when 1 then '治愈' when 2 then '好转' when 3 then '未愈' when 4 then '死亡' when 5 then '其他' when 6 then '转院' end cyqk 
from [Newtouch_CIS].[dbo].[zy_PatDxInfo]  cyzd
where cyzd.zdlx=0 and cyzd.zdlb=2 and cyzd.zt=1  and organizeId =@orgId ";

            if (!string.IsNullOrEmpty(zyh))
            {
                sql += " and zyh=@zyh ";
                para.Add(new SqlParameter("@zyh", zyh));
            }
            return this.FindList<cyzd>(sql, para.ToArray());

        }

        /// <summary>
        /// 门诊历史账单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public IList<MZhistorybillDTO> MZhistorybill(MZhistorybillRequest req)
        {
            string sql = @"select a.jsnm,(case a.jslx when '0' then '挂号账单' when'2' then '收费账单' else '' end )jslx,a.zje,(case a.jszt when 1 then '已结' when 2 then '已退' else '' end)jszt,a.CreateTime jsrq,a.LastModifyTime tfrq from mz_js a
left join mz_gh b on a.ghnm=b.ghnm and b.zt='1'
where a.zt='1' and a.zje>0
and b.mzh=@mzh
and a.CreateTime>=@kssj and a.CreateTime<=@jssj";
            List<SqlParameter> para = new List<SqlParameter>();
            para.Add(new SqlParameter("@mzh", req.mzh ));
            para.Add(new SqlParameter("@kssj", req.kssj));
            para.Add(new SqlParameter("@jssj", req.jssj));
            return this.FindList<MZhistorybillDTO>(sql, para.ToArray());
        }

        /// <summary>
        /// 账单明细
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public IList<MZhistorybillMXDTO> MZhistorybillMX(MZhistorybillMXRequest req)
        {
            string sql = @"select a.jsnm,a.jsmxnm,isnull(d.sfxmmc,e.ypmc)sfxm,isnull(b.dw,c.dw)dw,isnull(b.dj,c.dj)dj,isnull(b.sl,c.sl)sl,isnull(b.je,c.je)je from mz_jsmx a
left join mz_xm b on a.mxnm=b.xmnm and a.OrganizeId=b.OrganizeId and b.zt='1'
left join mz_cfmx c on a.cf_mxnm=c.cfmxid and a.OrganizeId=c.OrganizeId and c.zt='1'
left join NewtouchHIS_Base..xt_sfxm d on b.sfxm=d.sfxmCode and b.OrganizeId=d.OrganizeId and d.zt='1'
left join NewtouchHIS_Base..xt_yp e on c.yp=e.ypcode and c.OrganizeId=e.OrganizeId and e.zt='1'
where a.zt='1' and a.jsnm=@jsnm";
            List<SqlParameter> para = new List<SqlParameter>();
            para.Add(new SqlParameter("@jsnm", req.jsnm));
            return this.FindList<MZhistorybillMXDTO>(sql, para.ToArray());
        }

        public bool updateZybrxxkExpandBje(string zyh, string organizeId,decimal? bje) {
            var strSql = new StringBuilder();
            strSql.Append(@"    UPDATE    [Newtouch_CIS].dbo.zy_brxxk_expand
                      SET       bje = @bje 
                      WHERE     zyh=@zyh 
                                and organizeId=@organizeId
                                and zt=1
            ");
            var paraList = new object[]
            {
                new SqlParameter("@bje", bje),
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@organizeId", organizeId)
            };
            var count = _dataContext.Database.ExecuteSqlCommand(strSql.ToString(), paraList);
            return count == 1;
        }

  public bool updateZybrxxkExpandZhye(string zyh, string organizeId,decimal? zhye) {
            var strSql = new StringBuilder();
            strSql.Append(@"    UPDATE    [Newtouch_CIS].dbo.zy_brxxk_expand
                      SET       zhye = @zhye 
                      WHERE     zyh=@zyh 
                                and organizeId=@organizeId
                                and zt=1
            ");
            var paraList = new object[]
            {
                new SqlParameter("@zhye", zhye),
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@organizeId", organizeId)
            };
            var count = _dataContext.Database.ExecuteSqlCommand(strSql.ToString(), paraList);
            return count == 1;
        }
        public List<HisKsZdVO> GetksZzdList(string orgid)
        {
            string sqlstr = @"select code,name,py,convert(varchar(10),mzzybz)mzzybz,zxks,ybksbm from NewtouchHIS_Base..[Sys_Department] where zt='1' and OrganizeId=@orgid and zlks='1'";



            DbParameter[] par =
            {
                new SqlParameter("@orgid", orgid)

            };
            return this.FindList<HisKsZdVO>(sqlstr, par.ToArray());
        }

        #region 基金结算清单上传内容查询
        public IList<InSettlementInfoVO> GetSettlementList(string orgId, string sczt, string tjzt, string keyword = null, Pagination pagination = null, DateTime? cykssj = null, DateTime? cyjssj = null)
        {
            var sb = new StringBuilder();
            sb.Append(@"select zyxx.zyh, isnull(zyxx.xm, xx.xm) xm
    ,case when isnull(zyxx.nl,0) <> 0 then zyxx.nl else (case when zyxx.csny is not null and zyxx.ryrq is not null then datediff(yy,zyxx.csny,zyxx.ryrq) else 0 end) end nl
    ,zyxx.brxz, xz.brxzmc,isnull(zyxx.blh, xx.blh) blh
    ,zd.zdCode AS zzdCode,zd.icd10 AS zzdicd10,zd.zdmc zzdmc
    ,CONVERT(varchar(100), zyxx.csny, 23) AS csny
    ,CONVERT(varchar(100), zyxx.ryrq, 23) AS ryrq
    ,CONVERT(varchar(100), zyxx.cyrq, 23) AS cyrq
    ,zyxx.bq bqCode,bq.bqmc bqmc
    ,zybz zybz
    ,case zyxx.zybz when '0' then '入院登记' when '1' then '病区中' when '2' then '病区出院' when '3' then '病人出院' when '9' then '作废记录' else '' end AS zybzmc
    ,isnull(zyxx.LastModifyTime, zyxx.CreateTime) UpdateTime
    ,xx.py, '' wb
    ,zyxx.zjlx zjlx
    ,(CASE zyxx.zjlx WHEN '1'THEN '身份证' WHEN '2' THEN '护照' WHEN '3' THEN '军官证' ELSE '其他' END ) zjlxValue
    ,zyxx.zjh zjh
    ,(CASE zyxx.zjlx WHEN 1 THEN zyxx.zjh ELSE '' END) AS idCardNo
    ,(CASE zyxx.xb WHEN '1' THEN '1' WHEN '2' THEN '2' ELSE '3' END ) as sex
    ,(CASE zyxx.xb WHEN '1' THEN '男' WHEN '2' THEN '女' ELSE '不详' END ) as sexValue
    ,zyxx.lxr AS contPerName 
    ,zyxx.lxrdh AS contPerPhoneNum 
    ,lxrItem.Code AS contPerRel 
    ,lxrItem.Name AS contPerRelValue
    ,zyxx.ks, ks.Name ksmc, zyxx.doctor, ysStaff.Name ysxm
    ,zyxx.kh, zyxx.CardType, zyxx.CardTypeName,zyxx.jzh,zyxx.patid,
    case isnull(ybjs.jsqd_sclsh,'0') when '0' then '未上传' else '已上传' end sczt,
    case isnull(ybjs.jsqd_tjzt,'0') when '0' then '未提交' else '已提交' end tjzt,
    ybjs.setl_id,
    ybjs.psn_no
from zy_brjbxx(nolock) zyxx
left join xt_brjbxx(nolock) xx
    on zyxx.patid = xx.patid AND zyxx.OrganizeId = xx.OrganizeId  AND xx.zt = '1'
left join xt_brxz(nolock) xz
    ON xz.brxz = zyxx.brxz and xz.OrganizeId = zyxx.OrganizeId
left join drjk_rybl_input rybl on rybl.zyh=zyxx.zyh  and rybl.zt='1'
left join zy_rydzd(nolock) zd
    ON zd.zyh = zyxx.zyh AND zd.OrganizeId = zyxx.OrganizeId and zd.zdpx = 1 AND zd.zt = '1'
left join [NewtouchHIS_Base]..V_S_xt_bq(nolock) bq
    ON bq.bqCode = zyxx.bq AND bq.OrganizeId = zyxx.OrganizeId  AND bq.zt = '1'
LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail lxrItem 
    ON ( lxrItem.OrganizeId = zyxx.OrganizeId OR lxrItem.OrganizeId = '*')
    AND lxrItem.Code = zyxx.lxrgx AND lxrItem.CateCode = 'RelativeType'
left join NewtouchHIS_Base..V_S_Sys_Department ks
    on ks.Code = zyxx.ks and ks.OrganizeId = zyxx.OrganizeId
left join NewtouchHIS_Base..V_S_Sys_Staff ysStaff
    on ysStaff.gh = zyxx.doctor and ysStaff.OrganizeId = zyxx.OrganizeId
left join drjk_zyjs_output ybjs
    on ybjs.zyh = zyxx.zyh and ybjs.zt = '1'
where zyxx.zt = '1'
    and zyxx.brxz in ('1','2','3','11','12','13')
    and zyxx.OrganizeId = @orgId
");

            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sb.Append(@" and (zyxx.xm like @keyword or zyxx.blh like @keyword or zyxx.zyh like @keyword)");
                pars.Add(new SqlParameter("@keyword", "%" + (keyword ?? "") + "%"));
            }
            if (cykssj.HasValue)
            {
                sb.Append(@" and zyxx.cyrq >= @cykssj");
                pars.Add(new SqlParameter("@cykssj", cykssj.Value.Date));
            }
            if (cyjssj.HasValue)
            {
                sb.Append(@" and zyxx.cyrq < @cyjssj");
                pars.Add(new SqlParameter("@cyjssj", cyjssj.Value.AddDays(1).Date));
            }
            if (!string.IsNullOrWhiteSpace(sczt))
            {
                if (sczt == "1")
                {
                    sb.Append(@" and ybjs.jsqd_sclsh is null");
                }
                else
                {
                    sb.Append(@" and ybjs.jsqd_sclsh is not null");
                }
            }
            if (!string.IsNullOrWhiteSpace(tjzt))
            {
                if (tjzt == "1")
                {
                    sb.Append(@" and ybjs.jsqd_tjzt is null");
                }
                else
                {
                    sb.Append(@" and ybjs.jsqd_tjzt is not null");
                }
            }
            return this.QueryWithPage<InSettlementInfoVO>(sb.ToString(), pagination, pars.ToArray());
        }
        #endregion

    }

}
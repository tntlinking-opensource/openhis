using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;

namespace Newtouch.Domain.Entity
{
    /// <summary>
    /// 创 建：张冬林
    /// 日 期：2018-06-28 15:37
    /// 描 述：临时医嘱
    /// </summary>
    [Table("zy_lsyz")]
    public class InpatientSTATOrderEntity : IEntity<InpatientSTATOrderEntity>
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <returns></returns>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// OrganizeId
        /// </summary>
        /// <returns></returns>
        public string OrganizeId { get; set; }
        /// <summary>
        /// zyh
        /// </summary>
        /// <returns></returns>
        public string zyh { get; set; }
        /// <summary>
        /// 医嘱号，同一医嘱号内包含多个yzId
        /// </summary>
        /// <returns></returns>
        public string yzh { get; set; }
        /// <summary>
        /// 本地医嘱分组序号
        /// </summary>
        /// <returns></returns>
        public int? zh { get; set; }
        /// <summary>
        /// 病区代码
        /// </summary>
        /// <returns></returns>
        public string WardCode { get; set; }
        /// <summary>
        /// 科室代码
        /// </summary>
        /// <returns></returns>
        public string DeptCode { get; set; }
        /// <summary>
        /// 医生代码
        /// </summary>
        /// <returns></returns>
        public string ysgh { get; set; }
        /// <summary>
        /// 频次代码
        /// </summary>
        /// <returns></returns>
        public string pcCode { get; set; }

        /// <summary>
        /// 滴速  滴/分钟
        /// </summary>
        public int? ds { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        /// <returns></returns>
        public int? zxcs { get; set; }
        /// <summary>
        /// 执行周期
        /// </summary>
        /// <returns></returns>
        public int? zxzq { get; set; }
        /// <summary>
        /// 执行周期单位 -1:不规则周期，0：天,1：小时,2：分钟.注意：当为-1时，周代码（zdm）为数字（1234567）中的任意几个数字
        /// </summary>
        /// <returns></returns>
        public int? zxzqdw { get; set; }
        /// <summary>
        /// 周代码
        /// </summary>
        /// <returns></returns>
        public string zdm { get; set; }
        /// <summary>
        /// xmdm
        /// </summary>
        /// <returns></returns>
        public string xmdm { get; set; }
        /// <summary>
        /// xmmc
        /// </summary>
        /// <returns></returns>
        public string xmmc { get; set; }
        /// <summary>
        /// 医嘱状态 0待审，1审核，2执行，3取消(DC)，4停止
        /// </summary>
        /// <returns></returns>
        public int yzzt { get; set; }
        /// <summary>
        /// 剂量单位
        /// </summary>
        /// <returns></returns>
        public string dw { get; set; }
        /// <summary>
        /// 0 通用 1 自备
        /// </summary>
        /// <returns></returns>
        public int zbbz { get; set; }
        /// <summary>
        /// 药品，项目录入时，存每天数量，出院带药，存总量。文字录入数量为0
        /// </summary>
        /// <returns></returns>
        public int sl { get; set; }
        /// <summary>
        /// 单位类别 0剂量单位，1药库单位，2门诊单位，3住院单位，4进货单位(指示医嘱单位类别为0)
        /// </summary>
        /// <returns></returns>
        public int dwlb { get; set; }
        /// <summary>
        /// 本地医嘱类别 0：药品医嘱，1：抗生素医嘱，2：检查医嘱，3：检验医嘱，4：指示医嘱，5：普通医嘱 ， 6 停止医嘱， 7  出院带药， 8 膳食医嘱， 9 手术医嘱   10  中草药  
        /// </summary>
        /// <returns></returns>
        public int yzlx { get; set; }
        /// <summary>
        /// 停止医生代码
        /// </summary>
        /// <returns></returns>
        public string zfysgh { get; set; }
        /// <summary>
        /// zfsj
        /// </summary>
        /// <returns></returns>
        public DateTime? zfsj { get; set; }
        /// <summary>
        /// 停止操作员号
        /// </summary>
        /// <returns></returns>
        public string zfr { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        /// <returns></returns>
        public DateTime? shsj { get; set; }
        /// <summary>
        /// 审核操作员号
        /// </summary>
        /// <returns></returns>
        public string shr { get; set; }
        /// <summary>
        /// sssj
        /// </summary>
        /// <returns></returns>
        public DateTime? sssj { get; set; }
        /// <summary>
        /// ssr
        /// </summary>
        /// <returns></returns>
        public string ssr { get; set; }
        /// <summary>
        /// 开始日期
        /// </summary>
        /// <returns></returns>
        public DateTime kssj { get; set; }
        /// <summary>
        /// 执行日期(护士)
        /// </summary>
        /// <returns></returns>
        public DateTime? zxsj { get; set; }
        /// <summary>
        /// 给病人执行医嘱的护士
        /// </summary>
        /// <returns></returns>
        public string zxr { get; set; }
        /// <summary>
        /// 药品剂量
        /// </summary>
        /// <returns></returns>
        public decimal? ypjl { get; set; }
        /// <summary>
        /// 药品规格
        /// </summary>
        /// <returns></returns>
        public string ypgg { get; set; }
        /// <summary>
        /// 嘱托内容
        /// </summary>
        /// <returns></returns>
        public string ztnr { get; set; }
        /// <summary>
        /// 医嘱内容
        /// </summary>
        /// <returns></returns>
        public string yznr { get; set; }
        /// <summary>
        /// 药品用法
        /// </summary>
        /// <returns></returns>
        public string ypyfdm { get; set; }
        /// <summary>
        /// 执行科室代码
        /// </summary>
        /// <returns></returns>
        public string zxksdm { get; set; }
        /// <summary>
        /// printyznr
        /// </summary>
        /// <returns></returns>
        public string printyznr { get; set; }
        /// <summary>
        /// CreateTime
        /// </summary>
        /// <returns></returns>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// CreatorCode
        /// </summary>
        /// <returns></returns>
        public string CreatorCode { get; set; }
        /// <summary>
        /// LastModifyTime
        /// </summary>
        /// <returns></returns>
        public DateTime? LastModifyTime { get; set; }
        /// <summary>
        /// LastModifierCode
        /// </summary>
        /// <returns></returns>
        public string LastModifierCode { get; set; }
        /// <summary>
        /// zt
        /// </summary>
        /// <returns></returns>
        public string zt { get; set; }

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string hzxm { get; set; }

        public string bw { get; set; }

        public string zxsjd { get; set; }

        public string nlmddm { get; set; }
        public string ztmbId { get; set; }
        public string ztId { get; set; }
        public string ztmc { get; set; }

        /// <summary>
        /// 抗生素原因
        /// </summary>
        public string kssReason { get; set; }
        /// <summary>
        /// 申请类型，传到pacs用
        /// </summary>
        public string sqlx { get; set; }
        /// <summary>
        /// 部位方法
        /// </summary>
        public string bwff { get; set; }
        /// <summary>
        /// 申请单号，针对检验检查
        /// </summary>
        public string sqdh { get; set; }

        public string lcyx { get; set; }

        public string sqbz { get; set; }
        /// <summary>
        /// 申请单处理状态（0、已申请 1、已接收 2、已完成） 
        /// </summary>
        public int? sqdzt { get; set; }

        /// <summary>
        /// 代煎标志
        /// </summary>
        public int? djbz { get; set; }
        /// <summary>
        /// 出院带药标志
        /// </summary>
        public int? cydybz { get; set; }

        /// <summary>
        /// 转自费标志
        /// </summary>
        public int? zzfbz { get; set; }
        /// <summary>
        /// 申请状态
        /// </summary>
        public int? syncStatus { get; set; }
        /// <summary>
        ///  处方标签 JI 精I JII 精II MZ 麻醉
        /// </summary>
        public string yztag { get; set; }
        /// <summary>
        /// 是否计费(0:否 不计入费用明细 1:是)
        /// </summary>
        public int? isjf { get; set; }
        /// <summary>
        /// 执行中标志  1 正在执行
        /// </summary>
        public string zxing { get; set; }
        public string ispscs { get; set; }
        public int? ztsl { get; set; }
        /// <summary>
        /// 收费项目组套标识
        /// </summary>
        public string dcztbs { get; set; }
        /// <summary>
        /// 药品用法对应收费项目组套
        /// </summary>
        public string yfztbs { get; set; }
        /// <summary>
        /// 药品来源 1:科室库存 2：药房
        /// </summary>
        public int? yply { get; set; }
        /// <summary>
        /// 附属医嘱
        /// </summary>
        public string isfsyz { get; set; }

        public int? Px { get; set; }
    }
}
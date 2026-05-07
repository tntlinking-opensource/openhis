
using System;

namespace Newtouch.HIS.Domain.DTO.InputDto.OutpatientAccounting
{
    public class OptimaAccLeftDto
    {
        public string patientId { get; set; }
        public string admsNum { get; set; }
        public string patientName { get; set; }
        public string patientType { get; set; }
        /// <summary>
        /// 未确认标志
        /// </summary>
        public int? wqrzt { get; set; }
        /// <summary>
        /// 标识显示在院状态的数据
        /// </summary>
        public int? hidden { get; set; }
    }

    public class ChargeLeftDto
    {
        public string cfnm { get; set; }
        public string cfh { get; set; }
        public decimal zje { get; set; }
        public string ksmc { get; set; }
        public string ysmc { get; set; }
        public string cflx { get; set; }
        public string cflxmc { get; set; }
        public string klsj { get; set; }
    }

    public class ChargeRightDto
    {
        public string cfh { get; set; }
        public int? cfnm { get; set; }
        public string sfxmmc { get; set; }
        public string sfxmCode { get; set; }
        public string sfdlCode { get; set; }
        public string sfdlmc { get; set; }
        public string dlmc { get; set; }
        public int sl { get; set; }
        public string dw { get; set; }
        public decimal dj { get; set; }
        public decimal zje { get; set; }
        public decimal? zfbl { get; set; }
        public string zfxz { get; set; }
        /// <summary>
        /// 单次治疗量
        /// </summary>
        public int? dczll { get; set; }
        /// <summary>
        /// 执行次数
        /// </summary>
        public int? zxcs { get; set; }

        /// <summary>
        /// 项目内码（未关联处方）
        /// </summary>
        public int? xmnm { get; set; }

        /// <summary>
        /// 药品明细主键
        /// </summary>
        public int? cfmxId { get; set; }

        /// <summary>
        /// 类型（1药品 2项目）
        /// </summary>
        public string yzlx { get; set; }

        /// <summary>
        /// 科室编码
        /// </summary>
        public string ks { get; set; }

        /// <summary>
        /// 医生编码
        /// </summary>
        public string ys { get; set; }

        /// <summary>
        /// 医生姓名
        /// </summary>
        public string ysmc { get; set; }

        public DateTime klsj { get; set; }

        /// <summary>
        /// 医保代码
        /// </summary>
        public string ybdm { get; set; }
        /// <summary>
        /// 医保标志 1：医保  0自费
        /// </summary>
        public string ybbz { get; set; }

        /// <summary>
        /// 明细ID cfmxId或xmnm
        /// </summary>
        public int mxId { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 新农合医保代码
        /// </summary>
        public string xnhybdm { get; set; }
        public int? cflx { get; set; }
        
        ///医保限制用药提醒
        public string yybz { get; set; }
    }

    public class GuiAnChargeRightDto : ChargeRightDto
    {
        public int? jsmxnm { get; set; }

    }
    public class TbbzmlDto
    {
        /// <summary>
        /// 病种目录代码
        /// </summary>
        public string mtbbzmldm { get; set; }
        /// <summary>
        /// 病种目录大类名称
        /// </summary>
        public string mtbbzdlmc { get; set; }
        /// <summary>
        /// 病种分类名称
        /// </summary>
        public string mtbbzflmc { get; set; }
    }
}

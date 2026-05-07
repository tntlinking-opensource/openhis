using System;

namespace Newtouch.HIS.Domain.ValueObjects.HospitalizationManage
{
    public class DrugFeeDetailVO
    {
        /// <summary>
        /// 是否是药品 0：非 1：是
        /// </summary>
        public string isYP { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int jfbbh { get; set; }
        public string yzwym { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal dj { get; set; }
        
        /// <summary>
        /// 金额
        /// </summary>
        public decimal je { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal sl { get; set; }

        public decimal ylsl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? tsl { get; set; }

        /// <summary>
        /// 收费项目
        /// </summary>
        public string sfxm { get; set; }

        /// <summary>
        /// 收费项目名称
        /// </summary>
        public string sfxmmc { get; set; }

        /// <summary>
        /// 收费大类
        /// </summary>
        public string dl { get; set; }

        /// <summary>
        /// 收费大类 名称
        /// </summary>
        public string dlmc { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 减免金额（不 乘 数量）
        /// </summary>
        public decimal jmje { get; set; }

        /// <summary>
        /// 减免比例
        /// </summary>
        public decimal jmbl { get; set; }

        /// <summary>
        /// 自负比例
        /// </summary>
        public decimal zfbl { get; set; }

        /// <summary>
        /// 自负性质
        /// </summary>
        public string zfxz { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string ybbm { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime? tdrq { get; set; }

    }
}

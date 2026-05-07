using System;

namespace Newtouch.Domain.ValueObjects.API
{
    public class RegisteredInfoRespVO
    {
        /// <summary>
        /// 
        /// </summary>
        public string mzh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string blh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string brxm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sexValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? birth { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string brxzmc { get; set; }

        public string brxz { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? zjlx { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zjh { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ksmc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ysxm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ghsj { get; set; }
        /// <summary>
        /// 就诊类别 1普通门诊 2急诊 3专家门诊
        /// </summary>
        public string mjzbz { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime operatingTime { get; set; }
        /// <summary>
        /// 医生代码
        /// </summary>
        public string ys { get; set; }
        /// <summary>
        /// 科室代码
        /// </summary>
        public string ks { get; set; }
        /// <summary>
        /// 医保结算号（医保返回）
        /// </summary>
        public string ybjsh { get; set; }
        /// <summary>
        /// 复诊标志 1复诊
        /// </summary>
        public string fzbz { get; set; }
        /// <summary>
        /// 社保编号
        /// </summary>
        public string sbbh { get; set; }
        /// <summary>
        /// 参保地编码
        /// </summary>
        public string cbdbm { get; set; }
        /// <summary>
        /// 姓名拼音
        /// </summary>
        public string py { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        public string kh { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactNum { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 县
        /// </summary>
        public string county { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string address { get; set; }

        public string nlshow { get; set; }
        /// <summary>
        /// 挂号来源标志
        /// </summary>
        public string ghlybz { get; set; }
        /// <summary>
        /// 医保个人编号
        /// </summary>
        public string grbh { get; set; }
        public short? queno { get; set; }
    }
}

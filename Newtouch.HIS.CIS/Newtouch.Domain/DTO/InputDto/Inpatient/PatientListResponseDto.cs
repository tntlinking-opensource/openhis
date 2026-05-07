using System;

namespace Newtouch.Domain.DTO.InputDto
{
    /// <summary>
    /// 住院患者一览 在病区的病人返回对象集合
    /// </summary>
   public class PatientzbqResponseDto: PatientyzxxResponseDto
    {
        public string id { get; set; }
        public string cwmc { get; set; }
        public string xm { get; set; }
        public string sex { get; set; }
        public int? age { get; set; }
        //public string zyh { get; set; }
        public DateTime ryrq { get; set; }
        public string hljb { get; set; }
        public string wzjb { get; set; }
        public string brzt { get; set; }
        public string brxzmc { get; set; }
        public string ysmc { get; set; }
        public string zdmc { get; set; }
        public string brxzdm { get; set; }
        public int zyts { get; set; }
        public string nlshow { get; set; }
        public string ps { get; set; }
        public string blh { get; set; }
        public string deptCode { get; set; }
    }

    public class PatientyzxxResponseDto
    {
        public string zyh { get; set; }
        public DateTime? kssj { get; set; }
        public string ztnr { get; set; }
        /// <summary>
        /// 是否显示预出院图标 (有kssj 显示:null / 不显示:none)
        /// </summary>
        public string display { get; set; }
        /// <summary>
        /// 待审核医嘱条数
        /// </summary>
        public int? cnt { get; set; }
    }
    /// <summary>
    /// 住院患者一览 已出区的病人返回对象集合
    /// </summary>
    public class PatientycqResponseDto
    {
        public string id { get; set; }
        public string xm { get; set; }
        public string zyh { get; set; }
        public string cardno { get; set; }
        public string cwmc { get; set; }
        public string sex { get; set; }
        public int age { get; set; }
        public DateTime ryrq { get; set; }
        public DateTime? cqrq { get; set; }
        public string ysmc { get; set; }
        public string zdmc { get; set; }
        public string brxzmc { get; set; }
        public string brxzdm { get; set; }
        public string  nlShow { get; set; }
    }
}

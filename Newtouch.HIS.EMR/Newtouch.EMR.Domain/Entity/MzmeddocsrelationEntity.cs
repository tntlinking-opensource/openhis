using FrameworkBase.MultiOrg.Domain.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.EMR.Domain.Entity
{
    /// <summary>
    /// 创 建：chl
    /// 日 期：2018-09-11 10:46
    /// 描 述：患者病历文书关系表-门诊
    /// </summary>
    [Table("mz_meddocs_relation")]
    public class MzmeddocsrelationEntity : IEntity<MzmeddocsrelationEntity>
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
        /// 患者标识
        /// </summary>
        /// <returns></returns>
        public string patid { get; set; }
        /// <summary>
        /// 门诊号
        /// </summary>
        /// <returns></returns>
        public string mzh { get; set; }
        /// <summary>
        /// blId
        /// </summary>
        /// <returns></returns>
        public string blId { get; set; }
        /// <summary>
        /// 病历类型Id
        /// </summary>
        /// <returns></returns>
        public string mbId { get; set; }
        /// <summary>
        /// 病历名称
        /// </summary>
        /// <returns></returns>
        public string blmc { get; set; }
        /// <summary>
        /// 病历标题
        /// </summary>
        /// <returns></returns>
        public string blbt { get; set; }
        /// <summary>
        /// 是否已签名 0 未签 1 已签
        /// </summary>
        /// <returns></returns>
        public int? blzt { get; set; }
        /// <summary>
        /// 科室
        /// </summary>
        /// <returns></returns>
        public string ks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ksmc { get; set; }
        /// <summary>
        /// ysgh
        /// </summary>
        /// <returns></returns>
        public string ysgh { get; set; }
        /// <summary>
        /// ysxm
        /// </summary>
        /// <returns></returns>
        public string ysxm { get; set; }
        /// <summary>
        /// blrq
        /// </summary>
        /// <returns></returns>
        public DateTime? blrq { get; set; }
        /// <summary>
        /// 父节点
        /// </summary>
        /// <returns></returns>
        public string ParentId { get; set; }
        /// <summary>
        /// 是否父节点 0 否 1 是
        /// </summary>
        /// <returns></returns>
        public int? IsParent { get; set; }
        /// <summary>
        /// Memo
        /// </summary>
        /// <returns></returns>
        public string Memo { get; set; }
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
        public string bllx { get; set; }
        /// <summary>
        /// 医保上传标志 0 未上传 1 已上传
        /// </summary>
        public string YbUploadFlag { get; set; }
        /// <summary>
        /// 文书格式类型-xml/html
        /// </summary>
        public string doctype { get; set; }
    }
}
using FrameworkBase.MultiOrg.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.Entity
{
    /// <summary>
    /// 检验检查报告上传
    /// </summary>
    [Table("xt_JyjcFileUpload")]
    public class XtjyjcFileUploadEntity : IEntity<XtjyjcFileUploadEntity>
    {
        [Key]
        public string Id { get; set; }
        public string OrganizeId { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// 申请单号
        /// </summary>
        public string Sqdh { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件后缀
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreatorCode { get; set; }
        public DateTime? LastModifyTime { get; set; }
        public string LastModifierCode { get; set; }
        public string zt { get; set; }
    }
}

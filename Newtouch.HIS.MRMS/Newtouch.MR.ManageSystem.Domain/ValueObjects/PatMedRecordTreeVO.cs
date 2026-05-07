using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.MR.ManageSystem.Domain.ValueObjects
{
    public class PatMedRecordTreeVO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Blzt { get; set; }
        public string Doccode { get; set; }
        public string Docname { get; set; }
        public DateTime? Blrq { get; set; }

        public string parentId { get; set; }
        public DateTime? LastModifierCode { get; set; }
        public string BllxId { get; set; }
        public string zyh { get; set; }
        public string BlId { get; set; }
        /// <summary>
        /// 病历类型 EnumBllx
        /// </summary>
        public string bllx { get; set; }
        /// <summary>
        /// 是否允许添加文件标志
        /// </summary>
        public int? addPermit { get; set; }
        /// <summary>
        /// 文件读取编辑权限控制标志 EnummbqxFp
        /// </summary>
        public int? ctrlLevel { get; set; }
        public string doctype { get; set; }
    }
}

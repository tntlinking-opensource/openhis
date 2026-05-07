using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
namespace Newtouch.EMR.Domain.Entity
{
    /// <summary>
    /// 创 建：hyj
    /// 日 期：2018-10-30 11:24
    /// 描 述：元素
    /// </summary>
    [Table("bl_ys")]
    public class BlysEntity : IEntity<BlysEntity>
    {
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
        /// OrganizeId
        /// </summary>
        /// <returns></returns>
        public string OrganizeId { get; set; }
        /// <summary>
        /// yssjid
        /// </summary>
        /// <returns></returns>
        public string yssjid { get; set; }
        /// <summary>
        /// ysmc
        /// </summary>
        /// <returns></returns>
        public string ysmc { get; set; }
        /// <summary>
        /// 元素编码 唯一标识
        /// </summary>
        public string yscode { get; set; }
        /// <summary>
        /// 背景文字
        /// </summary>
        /// <returns></returns>
        public string BackgroundText { get; set; }
        /// <summary>
        /// 数据源
        /// </summary>
        /// <returns></returns>
        public string DataSource { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        /// <returns></returns>
        public string BindingPath { get; set; }
        /// <summary>
        /// AutoUpdate
        /// </summary>
        /// <returns></returns>
        public string AutoUpdate { get; set; }
        /// <summary>
        /// 是否只读
        /// </summary>
        /// <returns></returns>
        public string Readonly { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        /// <returns></returns>
        public string TypeName { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        /// <returns></returns>
        public string Description { get; set; }
        /// <summary>
        /// 是否是下来
        /// </summary>
        /// <returns></returns>
        public int? ListSource { get; set; }
        /// <summary>
        /// UserEditable
        /// </summary>
        /// <returns></returns>
        public string UserEditable { get; set; }
        /// <summary>
        /// DropdownList
        /// </summary>
        /// <returns></returns>
        public string EditStyle { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        /// <returns></returns>
        public string Value { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        /// <returns></returns>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// Style
        /// </summary>
        /// <returns></returns>
        public string Style { get; set; }
        /// <summary>
        /// Format
        /// </summary>
        /// <returns></returns>
        public string Format { get; set; }
        public int? yslx { get; set; }
        public int? px { get; set; }
    }
}
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
    /// 医技科室执行记录
    /// </summary>
    [Table("xt_jyjcexec")]
    public class XtjyjcExecEntity:IEntity<XtjyjcExecEntity>
    {
        [Key]
        public string Id { get; set; }
        public string OrganizeId { get; set; }
        public string mzzyh { get; set; }
        public string hzlx { get; set; }
        public string xm { get; set; }
        public string fylx { get; set; }
        public string sqdlx { get; set; }
        public string sqdh { get; set; }
        public int sl { get; set; }
        public decimal dj { get; set; }
        public string zxr { get; set; }
        public DateTime zxrq { get; set; }
        public string zxks { get; set; }
        public string kdks { get; set; }
        public decimal je { get; set; }
        public string ztId { get; set; }
        public string ztmc { get; set; }
        public string kdys { get; set; }
        public string shr { get; set; }
        public DateTime kdrq { get; set; }
        public string dw { get; set; }
        public string gg { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreatorCode { get; set; }
        public DateTime? LastModifyTime { get; set; }
        public string LastModifierCode { get; set; }
        public string zt { get; set; }
    }
}

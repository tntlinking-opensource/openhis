using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects
{
    public class OrderAuditVO
    {
        /// <summary>
        /// 医嘱id
        /// </summary>
        public string yzid { get; set; }
        /// <summary>
        /// 医嘱性质
        /// </summary>
        public int? yzxz { get; set; }
        /// <summary>
        /// 医嘱性质说明
        /// </summary>
        public string yzxzsm { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? kssj { get; set; }
        /// <summary>
        /// 项目类型
        /// </summary>
        public string xmmc { get; set; }
        /// <summary>
        /// 药品剂量
        /// </summary>
        public Decimal? ypjl { get; set; }
        /// <summary>
        /// 医嘱内容
        /// </summary>
        public string yzmc { get; set; }
        public string yzjl { get; set; }
        public string yfmc { get; set; }
        public string yzpcmc { get; set; }
        /// <summary>
        /// 停止/作废时间
        /// </summary>
        public DateTime? tzsj { get; set; }
        /// <summary>
        /// 停止/作废医生工号
        /// </summary>
        public string tzysgh { get; set; }
        /// <summary>
        /// 停止/作废操作员
        /// </summary>
        public string tzr { get; set; }

        public string Creator { get; set; }
        /// <summary>
        /// 医嘱组号
        /// </summary>
        public int? zh { get; set; }
        public string OrganizeId { get; set; }
        /// <summary>
        /// 0 待审 1已审核 2已执行 3DC 4停止
        /// </summary>
        public int yzzt { get; set; }
        /// <summary>
        /// 医嘱类型
        /// </summary>
        public int? yzlx { get; set; }
        public string zyh { get; set; }
        public string hzxm { get; set; }
        public string yzlxmc { get; set; }
        public string yztag { get; set; }
        public string yztagName { get; set; }
        public int? isjf { get; set; }
        public string zh1 { get; set; }
        public string ispscs { get; set; }
        public string yzh { get; set; }
        public string psjg { get; set; }
        public string psjgms { get; set; }
        public string iszt { get; set; }
        public string yfztbs { get; set; }
        /// <summary>
        /// 药品开立库存来源 1:科室备药库存 2：药房库存
        /// </summary>
        public int? yply { get; set; }
        public decimal? dj { get; set; }
        public decimal? je { get; set; }
        public int? sl { get; set; }
        public int? Px { get; set; }
        public string slstr { get; set; }
    }

    public class yzinfoVo
    {
        public string Id { get; set; }
    }
}

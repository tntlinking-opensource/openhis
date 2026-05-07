using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects
{
    public class OrderExecutionVO
     {
        /// <summary>
        /// 医嘱id
        /// </summary>
        public string yzid { get; set; }
        /// <summary>
        /// 领药序号
        /// </summary>
        public int lyxh { get; set; }
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
        /// /医嘱类型
        /// </summary>
        public int yzlx { get; set; }
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
        /// 执行时间
        /// </summary>
        public DateTime? zxsj { get; set; }
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
        /// 患者姓名
        /// </summary>
        public string hzxm { get; set; }
        /// <summary>
        /// 住院号
        /// </summary>
        public string zyh { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string shr { get; set; }
        /// <summary>
        /// 收费项目单价
        /// </summary>
        public decimal? dj { get; set; }
        /// <summary>
        /// 组号
        /// </summary>
         public int? zh { get; set; }
        /// <summary>
        /// 执行人
        /// </summary>
        public string zxr { get; set; }
        /// <summary>
        /// 是否计费
        /// </summary>
        public int? isjf { get; set; }
        public string yztag { get; set; }
        public string yztagName { get; set; }
        public string zh1 { get; set; }
        public string yfztbs { get; set; }
        /// <summary>
        /// 药品开立库存来源 1：医生站科室备药库存 2：药房库存
        /// </summary>
        public int? yply { get; set; }
        public decimal? je { get; set; }
        public int? sl { get; set; }
        public string yzh { get; set; }
        public string isfsyz { get; set; }
        public string slstr { get; set; }
        public string deptCode { get; set; }
        public string xmdm { get; set; }
    }

    public class ApiResponseVO
    {
        
        /// <summary>
        /// 住院号
        /// </summary>
        public string zyh { get; set; }
        /// <summary>
        /// 医嘱id
        /// </summary>
        public string yzid { get; set; }
        /// <summary>
        /// 医嘱性质
        /// </summary>
        public int yzxz { get; set; }
        /// <summary>
        /// 医嘱类型，药品Or项目
        /// </summary>
        public int yzlx { get; set; }
        /// <summary>
        /// 领药序号
        /// </summary>
        public int lyxh { get; set; }
        /// <summary>
        /// 收费项目单价
        /// </summary>
        public Decimal dj { get; set; }
        /// <summary>
        /// 药品/项目名称
        /// </summary>
        public string ypmc { get; set; }
        /// <summary>
        /// Table执行时间
        /// </summary>
         public DateTime? zxsj { get; set; }
        /// <summary>
        /// 执行人
        /// </summary>
        public string zxr { get; set; }
        /// <summary>
        /// 组号
        /// </summary>
        public int ? zh { get; set; }
        /// <summary>
        /// 是否计费 1:是 0:否
        /// </summary>
        public string isjf { get; set; }
        public string zh1 { get; set; }
        public string yfztbs { get; set; }
        /// <summary>
        /// 药品来源 1：医生站科室备药库存  2：药房库存
        /// </summary>
        public string yply { get; set; }
        /// <summary>
        /// 科室
        /// </summary>
        public string deptCode { get; set; }
        public string xmdm { get; set; }
        public int sl { get; set; }
    }

    public class HospItemFeeVO {
        public string ks { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public int kykc { get; set; }
    }
}

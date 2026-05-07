using System;
using System.ComponentModel.DataAnnotations;

namespace Newtouch.HIS.Domain.ValueObjects
{
    /// <summary>
    /// 隶属角色的用户信息
    /// </summary>
    public class SysMedicineVO : IEntity<SysMedicineVO>
    {
        /// <summary>
        /// 组织主键
        /// </summary>
        [Key]
        public int ypId { get; set; }

        /// <summary>
        /// 药品代码
        /// </summary>

        public string ypCode { get; set; }

        /// <summary>
        /// 药品名称
        /// </summary>

        public string ypmc { get; set; }

        /// <summary>
        /// 顶级组织主键
        /// </summary>
        public string TopOrganizeId { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string ypqz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string yphz { get; set; }

        /// <summary>
        /// 商品名
        /// </summary>

        public string spm { get; set; }

        /// <summary>
        /// 首拼
        /// </summary>

        public string py { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal cfl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string cfdw { get; set; }

        /// <summary>
        /// 2018.04.18 意 剂量转换系数
        /// </summary>

        public decimal jl { get; set; }

        /// <summary>
        /// 剂量单位
        /// </summary>

        public string jldw { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal bzs { get; set; }

        /// <summary>
        /// 包装单位
        /// </summary>

        public string bzdw { get; set; }

        /// <summary>
        /// 门诊拆零数
        /// </summary>

        public decimal mzcls { get; set; }

        /// <summary>
        /// 门诊拆零单位
        /// </summary>

        public string mzcldw { get; set; }

        /// <summary>
        /// 最小单位
        /// </summary>
        public string zxdw { get; set; }

        /// <summary>
        /// 住院拆零数
        /// </summary>

        public decimal zycls { get; set; }

        /// <summary>
        /// 住院拆零单位
        /// </summary>

        public string zycldw { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal cls { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string cldw { get; set; }

        /// <summary>
        /// 定价单位
        /// </summary>

        public string djdw { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal lsj { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal pfj { get; set; }

        /// <summary>
        /// 自负比例
        /// </summary>

        public decimal zfbl { get; set; }

        /// <summary>
        /// 自负性质
        /// </summary>

        public string zfxz { get; set; }

        /// <summary>
        /// 大类
        /// </summary>

        public string dlCode { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string jx { get; set; }

        /// <summary>
        /// 药厂名称
        /// </summary>

        public string ycmc { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public int? medid { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public int? medextid { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string ypbzdm { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string yfbmCode { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string nbdl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string mzzybz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string LastModifierCode { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public string zt { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public int? px { get; set; }

        /// <summary>
        /// 药品随ID
        /// </summary>
        [Key]
        public int ypsxId { get; set; }

        public string shbz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string tsbz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string jsbz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string gzy { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string mzy { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string yljsy { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string zbbz { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string zlff { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string sjap { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>

        public decimal? yl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string yldw { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string ypgg { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string ybdm { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public int? syts { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public decimal? dczdjl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public decimal? dczdsl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public decimal? ljzdjl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public decimal? ljzdsl { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string pzwh { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string yptssx { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string ypflCode { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string jzlx { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public int mrbzq { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public DateTime? zjtzsj { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string xglx { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public string ghdw { get; set; }

        /// <summary>
        /// 组织主键
        /// </summary>
        public int ypcd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sfdlMc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string jxmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypflMc { get; set; }

        /// <summary>
        /// 医保最后同步时间
        /// </summary>
        public DateTime? LastYBUploadTime { get; set; }

        /// <summary>
        /// 是否已同步医保
        /// </summary>
        public string isSynch { get; set; }
        
        /// <summary>
        /// 默认剂量
        /// </summary>
        public decimal? mrjl { get; set; }
        
        /// <summary>
        /// 默认频次
        /// </summary>
        public string mrpc { get; set; }

        /// <summary>
        /// 是否抗生素
        /// </summary>
        public string isKss { get; set; }
        
        /// <summary>
        /// 抗生素Id
        /// </summary>
        public string kssId { get; set; }

        /// <summary>
        /// 基药标识
        /// </summary>
        public string jybz { get; set; }
        
        /// <summary>
        /// 医保标志
        /// </summary>
        public string ybbz { get; set; }

        /// <summary>
        /// 新农合医保代码
        /// </summary>
        public string xnhybdm { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string bz { get; set; }
        /// <summary>
        /// 国家医保代码
        /// </summary>
        public string gjybdm { get; set; }
        public string gjybmc { get; set; }
        /// <summary>
        /// 超限价金额
        /// </summary>
        public decimal ?cxjje { get; set; }


		/// <summary>
		/// 药品种类
		/// </summary>
		public string tsypbz { get; set; }
        /// <summary>
        /// 单次限量
        /// </summary>
        public decimal? dcxl { get; set; }
        /// <summary>
        /// 慢病限量
        /// </summary>
        public decimal? mbxl { get; set; }

        /// <summary>
        /// 默认用法
        /// </summary>
        public string mryf { get; set; }
        /// <summary>
        /// 默认用法名称
        /// </summary>
        public string mryfmc { get; set; }
        /// <summary>
        /// 医保规格
        /// </summary>
        public string ybgg { get; set; }
        
        //暂为国药准字
        public string ypzsm { get; set; }
        public int? kcyjz { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.Entity.V
{
	public class CrkMxAll
	{
		/// <summary>
		/// 出入库明细ID
		/// </summary>
		public string crkmxId { get; set; }

		/// <summary>
		/// 来自表 XT_YP_CRKDJK的主键
		/// </summary>
		public string crkId { get; set; }

		/// <summary>
		/// 申领单明细ID
		/// </summary>
		public string sldmxId { get; set; }

		/// <summary>
		/// 药品代码
		/// </summary>
		public string ypdm { get; set; }

		/// <summary>
		/// 发票号
		/// </summary>
		public string fph { get; set; }

		/// <summary>
		/// 开票日期
		/// </summary>
		public DateTime? Kprq { get; set; }

		/// <summary>
		/// 到票日期
		/// </summary>
		public DateTime? Dprq { get; set; }

		/// <summary>
		/// 批号
		/// </summary>
		public string ph { get; set; }
		
		/// <summary>
		/// 有效期
		/// </summary>
		public DateTime? yxq { get; set; }

		/// <summary>
		/// 批发价 与zhyz和sl对应
		/// </summary>
		public decimal pfj { get; set; }

		/// <summary>
		/// 零售价 与zhyz和sl对应
		/// </summary>
		public decimal lsj { get; set; }

		/// <summary>
		/// 药库批发价
		/// </summary>
		public decimal ykpfj { get; set; }

		/// <summary>
		/// 药库零售价
		/// </summary>
		public decimal yklsj { get; set; }

		/// <summary>
		/// 总金额
		/// </summary>
		public decimal Zje { get; set; }

		/// <summary>
		/// 数量 部门单位数量
		/// </summary>
		public int sl { get; set; }

		/// <summary>
		/// 入库转换因子
		/// </summary>
		public int Rkzhyz { get; set; }

		/// <summary>
		/// 入库部门库存
		/// </summary>
		public int Rkbmkc { get; set; }

		/// <summary>
		/// 入库单位（单位名称）
		/// </summary>
		public string rkdw { get; set; }

		/// <summary>
		/// 出库转换因子
		/// </summary>
		public int Ckzhyz { get; set; }

		/// <summary>
		/// 出库部门库存
		/// </summary>
		public int Ckbmkc { get; set; }

		/// <summary>
		/// 出库单位（单位名称）
		/// </summary>
		public string ckdw { get; set; }

		/// <summary>
		/// 外观
		/// </summary>
		public string Wg { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int? zbbz { get; set; }

		/// <summary>
		/// 进库注册证
		/// </summary>
		public string jkzcz { get; set; }

		/// <summary>
		/// 合格证明
		/// </summary>
		public string hgzm { get; set; }

		/// <summary>
		/// 验收结果
		/// </summary>
		public string ysjg { get; set; }

		/// <summary>
		/// 退货原因
		/// </summary>
		public string thyy { get; set; }

		/// <summary>
		/// 处理结果
		/// </summary>
		public string Cljg { get; set; }

		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? scrq { get; set; }

		/// <summary>
		/// 扣率
		/// </summary>
		public decimal? kl { get; set; }

		/// <summary>
		/// 进价 药库单位进价
		/// </summary>
		public decimal? jj { get; set; }

		/// <summary>
		/// 产地
		/// </summary>
		public int? cd { get; set; }

		/// <summary>
		/// 批次
		/// </summary>
		public string pc { get; set; }

		/// <summary>
		/// 状态
		/// </summary>
		public string zt { get; set; }

		/// <summary>
		/// 排序
		/// </summary>
		public int? px { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string CreatorCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? LastModifyTime { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string LastModifierCode { get; set; }
		/// <summary>
		/// 进价总金额 药库单位进价
		/// </summary>
		public decimal? pfjze { get; set; }
		

		/// <summary>
		/// 药品名称
		/// </summary>
		public string ypmc { get; set; }

		/// <summary>
		/// 大类名称
		/// </summary>
		public string dlmc { get; set; }

		/// <summary>
		/// 药品规格
		/// </summary>
		public string ypgg { get; set; }

		/// <summary>
		/// 生产厂家
		/// </summary>
		public string ycmc { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects
{
    public class PrescriptionDetailVO
    {
        #region 处方明细

        /// <summary>
        /// 
        /// </summary>
        public string xmCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string xmmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypmc { get; set; }

        /// <summary>
        /// 每次治疗量  针对康复项目
        /// </summary>
        public int? mczll { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? mcjl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string mcjldw { get; set; }

        /// <summary>
        /// 用法代码
        /// </summary>
        public string yfCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pcCode { get; set; }

        /// <summary>
        /// 药品中的数量=总量   康复中的总量=总量
        /// </summary>
        public int? zl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal dj { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal je { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zh { get; set; }

        /// <summary>
        /// 部位
        /// </summary>
        public string bw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string urgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string purpose { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zxks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? zxsj { get; set; }
        /// <summary>
        /// 医保唯一码
        /// </summary>
        public string ybwym { get; set; }
        /// <summary>
        /// 限制使用标志
        /// </summary>
        public string xzsybz { get; set; }

        public decimal? ts { get; set; }
        public string ztmbId { get; set; }
        public string ztId { get; set; }
        public string ztmc { get; set; }
        public string ztCode { get; set; }

        /// <summary>
        /// 抗生素原因
        /// </summary>
        public string kssReason { get; set; }
        public string bwff { get; set; }

        public string sqlx { get; set; }
        public int? px { get; set; }
        /// <summary>
        /// 滴速
        /// </summary>
        public int? ds { get; set; }
        /// <summary>
        /// 转自费标志
        /// </summary>
        public int? zzfbz { get; set; }

        public string ispsbz { get; set; }
        public string islgbz { get; set; }
        public int? ztsl { get; set; }

        public int? sfzt { get; set; }
        public string zysm { get; set; }

        /// <summary>
        /// 处方附带生成的常规处方
        /// </summary>
        public string syncfbz { get; set; }
		#endregion


		/// <summary>
		/// 处方id
		/// </summary>
		public string cfId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ghlybz { get; set; }
        public string gjybdm { get; set; }

	}
}

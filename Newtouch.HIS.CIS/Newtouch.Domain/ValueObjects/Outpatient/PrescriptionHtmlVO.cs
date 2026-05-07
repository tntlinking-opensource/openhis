using System;

namespace Newtouch.Domain.ValueObjects
{
    public class PrescriptionHtmlVO
    {
        #region 处方主表
        /// <summary>
        /// 
        /// </summary>
        public string cfId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? sfbz { get; set; }

        /// <summary>
        /// 枚举 1 西药处方 2 中药处方 3 康复处方 4 检验处方 5 检查处方 6 输液处方
        /// </summary>
        public int cflx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string cfh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal zje { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ys { get; set; }

        /// <summary>
        /// 贴数
        /// </summary>
        public int? tieshu { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string cfyf { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? djbz { get; set; }
        /// <summary>
        /// 处方标签 JI 精I JII 精II MZ 麻醉
        /// </summary>
        public string cftag { get; set; }
        /// <summary>
        /// 代煎方式
        /// </summary>
        public string djfs { get; set; }
        // <summary>
        /// 代煎付数
        /// </summary>
        public string djts { get; set; }
        // <summary>
        /// 嘱托
        /// </summary>
        public string cfzt { get; set; }
        public string isdzcf { get; set; }//电子处方 1 是  0 否
        #endregion



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
        /// 用法
        /// </summary>
        public string yfCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pcCode { get; set; }

        /// <summary>
        /// 药品中的数量=总量   康复中的总量（不是数量）=总量
        /// </summary>
        public int? zl { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 非康复处方 暂时使用
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
        /// 组号（药品的组号）
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
        /// 执行科室（药品的药房部门、检验检查的执行科室）（康复处方的执行科室）
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
        /// <summary>
        /// 天数
        /// </summary>
        public decimal? ts { get; set; }
        public string ztmbId { get; set; }
        public string ztId { get; set; }
        public string ztmc { get; set; }

        /// <summary>
        /// 抗生素原因
        /// </summary>
        public string kssReason { get; set; }
        /// <summary>
        /// 部位方法
        /// </summary>
        public string bwff { get; set; }

        public string sqlx { get; set; }
        /// <summary>
        /// 排序 add by huangshanshan 2019.5.17 
        /// </summary>
        public int? px { get; set; }
        /// <summary>
        /// 滴速 add by huangshanshan 2019.6.3
        /// </summary>
        public int? ds { get; set; }
        /// <summary>
        /// 转自费标志
        /// </summary>
        public int? zzfbz { get; set; }


        public string yfztbm { get; set; }

        public string yfztmc { get; set; }

        public string sfzt { get; set; }
        /// <summary>
        /// 中药处方说明
        /// </summary>
        public string zysm { get; set; }

        #endregion
        #region 处方明细2
        /// <summary>
        /// 
        /// </summary>
        public string ypCode2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypmc2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? mcjl2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string mcjldw2 { get; set; }



        /// <summary>
        ///
        /// </summary>
        public int? sl2 { get; set; }

        /// <summary>
        /// 非康复处方 暂时使用
        /// </summary>
        public string dw2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? dj2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? je2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Remark2 { get; set; }

        /// <summary>
        /// 执行科室（药品的药房部门、检验检查的执行科室）（康复处方的执行科室）
        /// </summary>
        public string zxks2 { get; set; }

        /// <summary>
        /// 转自费标志
        /// </summary>
        public int? zzfbz2 { get; set; }
        /// <summary>
        /// 中药处方说明
        /// </summary>
        public string zysm2 { get; set; }

        #endregion

        /// <summary>
        /// 临床原因
        /// </summary>
        public string lcyx { get; set; }
        /// <summary>
        /// 申请备注
        /// </summary>
        public string sqbz { get; set; }
        /// <summary>
        /// 药品规格
        /// </summary>
        public string ypgg { get; set; }

        public string ispsbz { get; set; }

        public string islgbz { get; set; }
        public int? ztsl { get; set; }

		/// <summary>
		/// 处方有效天数
		/// </summary>
		public int? yxts { get; set; }
        public string gjybdm { get; set; }
    }
}

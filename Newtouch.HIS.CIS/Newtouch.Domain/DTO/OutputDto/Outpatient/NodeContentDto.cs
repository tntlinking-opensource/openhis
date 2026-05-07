using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Newtouch.Domain.DTO.OutputDto
{
    public class NodeContentDto
    {
        #region 患者就诊信息

        public string jzysmc { get; set; }
        public string jzys { get; set; }
        public string xb { get; set; }
        public string nlshow { get; set; }
        
        public int mjzbz { get; set; }
        public string xm { get; set; }
        public string jzks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mzh { get; set; }
        /// <summary>
        /// 挂号医生
        /// </summary>
        public string ghys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? zlkssj { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ghksmc { get; set; }
        
   

        /// <summary>
        /// 
        /// </summary>
        public decimal? tizhong { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? tiwen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? maibo { get; set; }
       
        /// <summary>
        /// 
        /// </summary>
        public decimal? xueya { get; set; }

        /// <summary>
        /// 血糖测量方式
        /// </summary>
        public string xuetangclfs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? xuetang { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fzbz { get; set; }

        public decimal? shengao { get; set; }
        public decimal? shousuoya { get; set; }
        public decimal? shuzhangya { get; set; }
        public decimal? huxi { get; set; }
        public string cfzbz { get; set; }
        /// <summary>
        /// 社保编号
        /// </summary>
        public string sbbh { get; set; }
        public lastzcfDto lastzcfDto { get; set; }
        #endregion

        #region 病历内容
        /// <summary>
        /// 主诉
        /// </summary>
        public string zs { get; set; }

        /// <summary>
        /// 发病时间
        /// </summary>
        public DateTime? fbsj { get; set; }

        /// <summary>
        /// 现病史
        /// </summary>
        public string xbs { get; set; }

        /// <summary>
        /// 既往史
        /// </summary>
        public string jws { get; set; }
        /// <summary>
        /// 月经史
        /// </summary>
        public string yjs { get; set; }
        /// <summary>
        /// 过敏史
        /// </summary>
        public string gms { get; set; }
        /// <summary>
        /// 婚姻
        /// </summary>
        public string hy { get; set; }
        /// <summary>
        /// 婚姻
        /// </summary>
        public string hymc { get; set; }
        /// <summary>
        /// 查体
        /// </summary>
        public string ct { get; set; }

        public string clfa { get; set; }

        public string fzjc { get; set; }
        #endregion

        #region 病历诊断
        /// <summary>
        /// 
        /// </summary>
        public List<WMDiagnosisHtmlVO> xyzdList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<TCMDiagnosisHtmlVO> zyzdList { get; set; }
        #endregion

        #region 处方内容
        /// <summary>
        /// 
        /// </summary>
        public List<NodeContentPresBO> cfBoList { get; set; }

        #endregion


    }

    public class lastzcfDto {
        /// <summary>
        /// 最后一次收取诊查费时间
        /// </summary>
        public string zcfsj { get; set; }
        /// <summary>
        /// 最后一次收取诊查费收费项目详情
        /// </summary>
        public string zcfsfxm { get; set; }
    }
}

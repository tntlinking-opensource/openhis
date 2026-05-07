using System;

namespace Newtouch.HIS.Domain.DO
{
    /// <summary>
    /// 资源预定
    /// </summary>
    public class BookItemDo
    {
        /// <summary>
        /// 药品代码
        /// </summary>
        public string YpCode { get; set; }

        /// <summary>
        /// 最小单位数量
        /// </summary>
        public int Sl { get; set; }

        /// <summary>
        /// 药房部门
        /// </summary>
        public string Yfbm { get; set; }

        /// <summary>
        /// 成组号
        /// </summary>
        public string czh { get; set; }

        /// <summary>
        /// 处方号
        /// </summary>
        public string Cfh { get; set; }
        /// <summary>
        /// 处方明细Id
        /// </summary>
        public string CfmxId { get; set; }

        /// <summary>
        /// 医嘱执行ID
        /// </summary>
        public string Yzzxid { get; set; }

        /// <summary>
        /// 医嘱执行ID
        /// </summary>
        public string zxId { get; set; }

        /// <summary>
        /// 组织结构ID
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 提档日期/(或CreateTime)
        /// </summary>
        public DateTime AdtTdrq { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public string Res { get; set; }

        /// <summary>
        /// 组号
        /// </summary>
        public int? zh { get; set; }
    }

    /// <summary>
    /// 取消排药
    /// </summary>
    public class CancelArrangement
    {
        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 药品代码
        /// </summary>
        public string ypCode { get; set; }

        /// <summary>
        /// 医嘱ID
        /// </summary>
        public string yzId { get; set; }

        /// <summary>
        /// 医嘱执行ID
        /// </summary>
        public string zxId { get; set; }

        /// <summary>
        /// 药房部门代码
        /// </summary>
        public string yfbmCode { get; set; }

        /// <summary>
        /// 组织机构Id
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 登录用户
        /// </summary>
        public string userCode { get; set; }
    }
}

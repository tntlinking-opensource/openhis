namespace Newtouch.HIS.Domain.DTO
{
    /// <summary>
    /// 门诊挂号查询 返回DTO
    /// </summary>
    public class OutPatientRegistrationInfoDTO
    {
        /// <summary>
        /// 门诊号
        /// </summary>
        public string mzh { get; set; }
        /// <summary>
        /// 病历号
        /// </summary>
        public string blh { get; set; }
        /// <summary>
        /// 医生
        /// </summary>
        public string ys { get; set; }
        /// <summary>
        /// 医生
        /// </summary>
        public string ysxm { get; set; }
        /// <summary>
        /// 科室
        /// </summary>
        public string ks { get; set; }
        /// <summary>
        /// 科室
        /// </summary>
        public string ksmc { get; set; }
        /// <summary>
        /// 病人性质
        /// </summary>
        public string brxz { get; set; }
        /// <summary>
        /// 病人性质
        /// </summary>
        public string brxzmc { get; set; }
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string brxm { get; set; }
        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string maritalStatus { get; set; }
        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string maritalStatusValue { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string zjlx { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string zjlxValue { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        public string zjh { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string idCardNo { get; set; }
        /// <summary>
        /// 联系号码
        /// </summary>
        public string contactNum { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string sexValue { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public string birth { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string statusValue { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 县
        /// </summary>
        public string county { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 挂号时间
        /// </summary>
        public string ghsj { get; set; }
        /// <summary>
        /// 紧急联系人
        /// </summary>
        public string contPerName { get; set; }
        /// <summary>
        /// 紧急联系人电话
        /// </summary>
        public string contPerPhoneNum { get; set; }
        /// <summary>
        /// 紧急联系人关系
        /// </summary>
        public string contPerRel { get; set; }
        /// <summary>
        /// 紧急联系人关系
        /// </summary>
        public string contPerRelValue { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string updateTime { get; set; }

        /// <summary>
        /// 门急诊标志
        /// </summary>
        public string mjzbz { get; set; }

        /// <summary>
        /// 门急诊标志
        /// </summary>
        public string mjzbzValue { get; set; }

        /// <summary>
        /// 就诊标志
        /// </summary>
        public string jiuzhenbz { get; set; }

        /// <summary>
        /// 就诊标志
        /// </summary>
        public string jiuzhenbzValue { get; set; }

        /// <summary>
        /// 挂号操作时间
        /// </summary>
        public string operatingTime { get; set; }

        /// <summary>
        /// 医保结算号（前置）
        /// </summary>
        public string ybjsh { get; set; }

        /// <summary>
        /// 复诊标志 1复诊
        /// </summary>
        public string fzbz { get; set; }

        /// <summary>
        /// 社保编号
        /// </summary>
        public string sbbh { get; set; }

        /// <summary>
        /// 参保地编码
        /// </summary>
        public string cbdbm { get; set; }

        /// <summary>
        /// 拼音（患者姓名首拼）
        /// </summary>
        public string py { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        public string kh { get; set; }

        public string nlshow { get; set; }
        /// <summary>
        /// 挂号来源标签 2020-3-4 新增
        /// </summary>
        public string ghlybz { get; set; }
        public short? queno { get; set; }
    }
}

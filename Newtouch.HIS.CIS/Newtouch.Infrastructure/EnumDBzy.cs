using System.ComponentModel;

namespace Newtouch.Infrastructure
{
    /// <summary>
    /// 在院标志
    /// </summary>
    public enum EnumZYBZ
    {
        /// <summary>
        /// 入院登记
        /// </summary>
        Xry = 0,
        /// <summary>
        /// 病区中
        /// </summary>
        Bqz = 1,
        /// <summary>
        /// 病区出院（出病区）（待结账）
        /// </summary>
        Djz = 2,
        /// <summary>
        /// 已出院（出院结算）
        /// </summary>
        Ycy = 3,
        /// <summary>
        /// 转区
        /// </summary>
        Zq = 7,

        /// <summary>
        /// 作废记录/取消入院
        /// </summary>
        Wry = 9,
    }

    /// <summary>
    /// 医嘱类型
    /// </summary>
    public enum EnumYzlx
    {
        ///// <summary>
        ///// 通用
        ///// </summary>
        //[Description("通用")]
        //Ty = 1,
        /// <summary>
        /// 药品
        /// </summary>
        [Description("药品")]
        Yp = 2,
        /// <summary>
        /// 文字
        /// </summary>
        [Description("文字")]
        Wz = 3,
        /// <summary>
        /// 出院带药
        /// </summary>
        [Description("出院带药")]
        Cydy = 4,
        /// <summary>
        /// 项目
        /// </summary>
        [Description("项目")]
        sfxm = 5,
        /// <summary>
        /// 检验
        /// </summary>
        [Description("检验")]
        jy = 6,
        /// <summary>
        /// 检查
        /// </summary>
        [Description("检查")]
        jc = 7,
        /// <summary>
        /// 膳食医嘱
        /// </summary>
        [Description("膳食医嘱")]
        ssyz = 8,
        /// <summary>
        /// 手术
        /// </summary>
        [Description("手术")]
        oper = 9,
        /// <summary>
        /// 中草药
        /// </summary>
        [Description("中草药")]
        zcy = 10,
	    /// <summary>
	    /// 康复
	    /// </summary>
	    [Description("康复")]
	    rehab = 11,
        /// <summary>
	    /// 物资耗材
	    /// </summary>
	    [Description("耗材")]
        yyhc = 12

    }
    /// <summary>
    /// 医嘱状态
    /// </summary>
    public enum EnumYzzt
    {
        /// <summary>
        /// 待审 
        /// </summary>
        [Description("待审")]
        Ds = 0,
        /// <summary>
        /// 审核
        /// </summary>
        [Description("审核")]
        Sh = 1,
        /// <summary>
        /// 执行
        /// </summary>
        [Description("执行")]
        Zx = 2,
        /// <summary>
        /// 撤DC
        /// </summary>
        [Description("DC")]
        DC = 3,
        /// <summary>
        /// 停止或作废
        /// </summary>
        [Description("停止")]
        TZ = 4 
    }
    /// <summary>
    /// 药品单位类别
    /// </summary>
    public enum EnumYPdwlb
    {
        /// <summary>
        /// 剂量单位
        /// </summary>
        Jldw = 1,
        /// <summary>
        /// 药库单位
        /// </summary>
        Ykdw = 2,
        /// <summary>
        /// 门诊单位
        /// </summary>
        Mzdw = 3,
        /// <summary>
        /// 住院单位
        /// </summary>
        Zydw = 4,
        /// <summary>
        /// 进货单位
        /// </summary>
        Jhdw = 5,
        /// <summary>
        /// 最小单位
        /// </summary>
        Zxdw = 6
    }
    /// <summary>
    /// 套餐范围
    /// </summary>
    public enum EnumTcfw
    {
        [Description("个人")]
        Person = 1,
        [Description("科室")]
        Dept = 2,
        [Description("病区")]
        Ward = 3,
        [Description("全院")]
        Hosp = 4
    }
    /// <summary>
    /// 护理病人状态（护理生命体征录入用）
    /// </summary>
    public enum EnumHlbrzt
    {
        [Description("入院")]
        Ry = 1,
        [Description("在院")]
        Zy = 2,
        [Description("出院")]
        Cy = 3,
        [Description("请假")]
        Qj = 4,
        [Description("转入")]
        Zr = 5,
        [Description("转出")]
        Zc = 6,
        [Description("拒测")]
        Jc = 7,
        [Description("返院")]
        Fy = 8,
        [Description("复测")]
        Fc = 9,
		[Description("卧床")]
		Wc = 10,
		[Description("死亡")]
		Sw = 11,
		[Description("手术")]
		ss = 12,
        [Description("回室")]
        hs = 13
    }
    /// <summary>
    /// 执行周期单位
    /// </summary>
    public enum EnumZxzqdw
    {
        /// <summary>
        /// 天
        /// </summary>
        Day = 1,
        /// <summary>
        /// 小时
        /// </summary>
        Hour = 2,
        /// <summary>
        /// 分钟
        /// </summary>
        Minute = 3,
        /// <summary>
        /// 不规则周期
        /// </summary>
        Unruly = 4
    }
    /// <summary>
    /// 发药退药标志
    /// </summary>
    public enum EnumMedSTflag
    {
        [Description("发药")]
        Receive = 1,
        [Description("全退")]
        AllReturn = 2,
        [Description("部分退")]
        PartReturn = 3
    }
    /// <summary>
    /// 医嘱性质
    /// </summary>
    public enum EnumYzxz
    {
        [Description("临时")]
        Ls = 1,
        [Description("长期")]
        Cq = 2
    }
    /// <summary>
    /// 床位记录状态
    /// </summary>
    public enum EnumCwjlzt
    {
        [Description("当前")]
        Dq = 1,
        [Description("转床")]
        Zc = 2,
        [Description("转区")]
        Zq = 3,
        [Description("出区")]
        Cq = 4,
        [Description("取消入区")]
        QxRq = 5
    }
    /// <summary>
    /// 医生类型
    /// </summary>
    public enum EnumYslx
    {
        [Description("住院医生")] //Resident
        ZyDoc = 1,
        [Description("主治医生")] //Attending
        ZzDoc = 2,
        [Description("主任医生")] //chief
        ZrDoc = 3
    }
    /// <summary>
    /// 护理级别
    /// </summary> 
    public enum EnumHljb
    {
        /// <summary>
        /// I级
        /// </summary>
        [Description("I级")]
        I = 1,
        /// <summary>
        /// II级
        /// </summary>
        [Description("II级")]
        II = 2,
        /// <summary>
        /// III级
        /// </summary>
        [Description("III级")]
        III = 3,
        /// <summary>
        /// 特级
        /// </summary>
        [Description("特级")]
        T = 4
    }
    /// <summary>
    /// 结算状态
    /// </summary>
    public enum EnumJszt
    {
        [Description("已结算")]
        Yjs = 1,
        [Description("未结算")]
        Wjs = 2
    }
    /// <summary>
    /// 固定执行标准
    /// </summary>
    public enum EnumGdzxbz
    {
        /// <summary>
        /// 固定
        /// </summary>
        Fixed = 1,
        /// <summary>
        /// 不固定
        /// </summary>
        Nonfixed = 0
    }
    /// <summary>
    /// 危重级别
    /// </summary>
    public enum EnumWzjb
    {
        /// <summary>
        /// 一般
        /// </summary>
        [Description("一般")]
        Normal =1,
        /// <summary>
        /// 病重
        /// </summary>
        [Description("病重")]
        Serious =2,
        /// <summary>
        /// 病危
        /// </summary>
        [Description("病危")]
        Bedying =3
    }
    //EnumDB已包含
    //public enum EnumZdlx
    //{
    //    MainDiag=1,
    //    SedDiag=2
    //}

    /// <summary>
    /// 体温测量说明
    /// </summary>
    public enum EnumTwclfs
    {
        /// <summary>
        /// 口温
        /// </summary>
        [Description("口温")]
        kw = 1,
        /// <summary>
        /// 腋温
        /// </summary>
        [Description("腋温")]
        yw = 2,
        /// <summary>
        /// 肛温
        /// </summary>
        [Description("肛温")]
        gw = 1,
    }
    /// <summary>
    /// 膳食类别
    /// </summary>
    public enum EnumSSLB
    {
        /// <summary>
        /// 类型
        /// </summary>
        [Description("类型")]
        lx = 1,
        /// <summary>
        /// 特殊要求
        /// </summary>
        [Description("特殊要求")]
        tsyq = 2,
        /// <summary>
        /// 膳食型号
        /// </summary>
        [Description("膳食型号")]
        ssxh = 3,
    }

    /// <summary>
    /// 模板使用权限
    /// </summary>
    public enum Enummbqx
    {
        /// <summary>
        /// 通用
        /// </summary>
        [Description("通用")]
        pub = 0,
        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")]
        prv = 1,
        /// <summary>
        /// 科室
        /// </summary>
        [Description("科室")]
        dept = 2
    }

    /// <summary>
    /// 病历类型
    /// </summary>
    public enum EnumBllx
    {
        /// <summary>
        /// 住院病历
        /// </summary>
        [Description("住院病历")]
        zybl = 1,

        /// <summary>
        /// 病程记录
        /// </summary>
        [Description("病程记录")]
        bcjl = 2,

        /// <summary>
        /// 医疗文书
        /// </summary>
        [Description("医疗文书")]
        ylws = 3,

        /// <summary>
        /// 护理记录
        /// </summary>
        [Description("护理记录")]
        hljl = 4,

        /// <summary>
        /// 病案首页
        /// </summary>
        /// 
        [Description("病案首页")]
        basy = 5
    }

    public enum EnumCYQK
    {
        /// <summary>
        /// 治愈
        /// </summary>
        [Description("治愈")]
        zy = 1,
        /// <summary>
        /// 好转
        /// </summary>
        [Description("好转")]
        hz = 2,
        /// <summary>
        /// 未愈
        /// </summary>
        [Description("未愈")]
        wy = 3,
        /// <summary>
        /// 死亡
        /// </summary>
        [Description("死亡")]
        sw = 4,
        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        qt = 5,
        /// <summary>
        /// 转院
        /// </summary>
        [Description("转院")]
        zhuanyuan = 6

    }
    public enum EnumSF
    {
        /// <summary>
        /// 否
        /// </summary>
        [Description("否")]
        f = 0,
        /// <summary>
        /// 是
        /// </summary>
        [Description("是")]
        s = 1,
    }

    //皮试测试
    public enum EnumPSCS {
        /// <summary>
        /// 否
        /// </summary>
        [Description("否")]
        f = 0,
        /// <summary>
        /// 是
        /// </summary>
        [Description("是")]
        s = 1,
    }
    /// <summary>
    /// 皮试结果
    /// </summary>
    public enum EnumPsResult {
        [Description("阴性")]
        negative = 0,
        [Description("阳性")]
        positive = 1,
    }

    /// <summary>
    /// 科室备药状态
    /// </summary>
    public enum EnumKSBYZT
    {
        [Description("已申请")]
        sq=0,
        [Description("已提交")]
        sh= 1,
        [Description("已审核")]
        tj= 2,
        [Description("已发药")]
        fy= 3,
        [Description("已退回")]
        th= 4,
        [Description("已作废")]
        zf= 5,
    }

    /// <summary>
    /// 科室备药退回状态
    /// </summary>
    public enum EnumKSBYTHZT
    {
        [Description("保存")]
        sq = 1,
        [Description("审核")]
        sh = 2,
        [Description("提交")]
        tj = 3,
        [Description("发药")]
        fy = 4,
        [Description("撤回")]
        ch = 5,
        [Description("退回")]
        th = 6,
        [Description("作废")]
        zf = 7,
    }
    /// <summary>
    /// 药品来源
    /// </summary>
    public enum EnumYply
    {
        [Description("科室备药")]
        ksby = 1,
        [Description("药房")]
        yf = 2
    }

    /// <summary>
    /// 皮试
    /// </summary>
    public enum EnumPs
    {
        /// <summary>
        /// 阴
        /// </summary>
        [Description("阴")]
        yin = 0,
        /// <summary>
        /// 阳
        /// </summary>
        [Description("阳")]
        yang = 1,
    }
}

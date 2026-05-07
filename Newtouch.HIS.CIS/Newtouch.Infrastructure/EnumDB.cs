using System.ComponentModel;

namespace Newtouch.Infrastructure
{
    /// <summary>
    /// 日志记录类型
    /// </summary>
    public enum DbLogType_Ex
    {

    }

    /// <summary>
    /// 证件类型
    /// </summary>
    public enum EnumZjlx
    {
        /// <summary>
        /// 身份证
        /// </summary>
        [Description("身份证")]
        sfz = 1,
        /// <summary>
        /// 护照
        /// </summary>
        [Description("护照")]
        hz,
        /// <summary>
        /// 军官证
        /// </summary>
        [Description("军官证")]
        jgz,
        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        other = 9
    }

    /// <summary>
    /// 自负性质（药品、项目字典的属性）
    /// </summary>
    public enum EnumZiFuXingZhi
    {
        /// <summary>
        /// 可报
        /// </summary>
        [Description("可报")]
        KB = 0,
        /// <summary>
        /// 自费
        /// </summary>
        [Description("自费")]
        ZF = 1,
        /// <summary>
        /// 分类自负
        /// </summary>
        [Description("分类自负")]
        FLZF = 2,
        /// <summary>
        /// 绝对自理
        /// </summary>
        [Description("绝对自理")]
        JueDuiZiLi = 3,
        /// <summary>
        /// 甲类
        /// </summary>
        [Description("甲类")]
        Jia,
        /// <summary>
        /// 乙类
        /// </summary>
        [Description("乙类")]
        Yi,
        /// <summary>
        /// 丙类
        /// </summary>
        [Description("丙类")]
        Bing,
    }

	/// <summary>
	/// 药品类别
	/// </summary>
	public enum EnumYpsx
	{
		[Description("麻精药品（麻醉、精一、精二）")]
		Mzyp = 9,
		[Description("毒性药品")]
		Dxyp = 4,
		[Description("血液制品")]
		Xyzp = 5,
		[Description("激素药品")]
		Jsyp = 6,
		[Description("贵重药品")]
		Gzyp = 7,
		[Description("抗肿瘤药物药品")]
		Kzlywyp = 8,
	}
	/// <summary>
	/// 抗生素级别
	/// </summary>
	public enum EnumKss
	{

		[Description("非限制使用药物")]
		Fxzsyyw = 0,
		[Description("限制使用药物")]
		Xzsyyw = 1,
		[Description("特殊使用药物")]
		Tssyyw = 2,
	}

	/// <summary>
	/// 就诊状态
	/// </summary>
	public enum EnumJzzt
    {
        /// <summary>
        /// 未就诊
        /// </summary>
        [Description("未就诊")]
        NotYetTreate = 1,
        /// <summary>
        /// 就诊中
        /// </summary>
        [Description("就诊中")]
        Treating = 2,
        /// <summary>
        /// 就诊结束
        /// </summary>
        [Description("就诊结束")]
        Treated = 3,
    }

    /// <summary>
    /// 门诊类型 mjzbz
    /// </summary>
    public enum EnumOutPatientType
    {
        /// <summary>
        /// 普通门诊
        /// </summary>
        [Description("普通门诊")]
        generalOutpat = 1,
        /// <summary>
        /// 急诊
        /// </summary>
        [Description("急诊")]
        emerDiagnosis = 2,
        /// <summary>
        /// 专家门诊
        /// </summary>
        [Description("专家门诊")]
        expertOutpat = 3,
        /// <summary>
        /// 专家门诊
        /// </summary>
        [Description("特病门诊")]
        SpecialOutpat = 4,
        /// <summary>
        /// 重大疾病门诊
        /// </summary>
        [Description("重大疾病门诊")]
        MajorDiseases = 5,
        /// <summary>
        /// 慢性病门诊
        /// </summary>
        [Description("慢性病门诊")]
        ChronicDisease = 6,
        /// <summary>
        /// 居民两病
        /// </summary>
        [Description("居民两病")]
        jmlb = 7,
        /// <summary>
        /// 意外伤害门诊
        /// </summary>
        [Description("意外伤害门诊")]
        ywshmz = 8,
        /// <summary>
        /// 生育门诊
        /// </summary>
        [Description("生育门诊")]
        symz = 9,
        /// <summary>
        /// 耐多药结核门诊
        /// </summary>
        [Description("耐多药结核门诊")]
        ndyjhmz = 10,
        /// <summary>
        /// 儿童两病门诊
        /// </summary>
        [Description("儿童两病门诊")]
        etlbmz = 11,
        /// <summary>
        /// 简易门诊
        /// </summary>
        [Description("简易门诊")]
        jymz = 12
    }

    /// <summary>
    /// 处方类型
    /// </summary>
    public enum EnumCflx
    {
        /// <summary>
        /// 西药处方
        /// </summary>
        [Description("西药处方")]
        WMPres = 1,
        /// <summary>
        /// 中药处方
        /// </summary>
        [Description("中药处方")]
        TCMPres = 2,
        /// <summary>
        /// 康复处方
        /// </summary>
        [Description("康复处方")]
        RehabPres = 3,
        /// <summary>
        /// 检验处方
        /// </summary>
        [Description("检验处方")]
        InspectionPres = 4,
        /// <summary>
        /// 检查处方
        /// </summary>
        [Description("检查处方")]
        ExaminationPres = 5,
        /// <summary>
        /// 常规项目处方
        /// </summary>
        [Description("常规项目处方")]
        RegularItemPres = 6,
        /// <summary>
        /// 电子处方
        /// </summary>
		[Description("电子处方")]
		Dzcf =7,
        /// <summary>
        /// 医用耗材
        /// </summary>
        [Description("医用耗材")]
        Yyhc = 8
    }

    public enum EnumYpyf
    {
        [Description("西医")]
        WM=1,
        [Description("中医")]
        TCM=2
    }

    /// <summary>
    /// 诊断类型
    /// </summary>
    public enum EnumZdlx
    {
        /// <summary>
        /// 主诊断
        /// </summary>
        [Description("主诊断")]
        Main = 1,
        /// <summary>
        /// 副诊断
        /// </summary>
        [Description("副诊断")]
        Deputy = 2
    }

    /// <summary>
    /// 处方模板类型
    /// （病历模板类型也是用此枚举）
    /// </summary>
    public enum EnumCfMbLx
    {
        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")]
        personal = 1,
        /// <summary>
        /// 科室
        /// </summary>
        [Description("科室")]
        department = 2,
        /// <summary>
        /// 全院
        /// </summary>
        [Description("全院")]
        hospital = 3,
    }

    /// <summary>
    /// 检验检查组套类型  （检验检查共用一个页面，只是参数（即type）不同）
    /// </summary>
    public enum EnumjyjczutaoLx
    {
        /// <summary>
        /// 检验
        /// </summary>
        [Description("检验")]
        jy = 1,
        /// <summary>
        /// 检查
        /// </summary>
        [Description("检查")]
        jc = 2
    }

    /// <summary>
    /// 检验检查模板类型  （检验检查共用一个页面，只是参数（即type）不同）
    /// </summary>
    public enum EnumjyjcmbLx
    {
        /// <summary>
        /// 检验
        /// </summary>
        [Description("检验")]
        jy = 1,
        /// <summary>
        /// 检查
        /// </summary>
        [Description("检查")]
        jc = 2
    }

    /// <summary>
    /// 收费项目计价策略
    /// </summary>
    public enum EnumSfxmJjcl
    {
        /// <summary>
        /// 按时长
        /// </summary>
        [Description("按时长")]
        Time = 1,
        /// <summary>
        /// 按数量
        /// </summary>
        [Description("按数量")]
        Amount,
        /// <summary>
        /// 按面积
        /// </summary>
        [Description("按面积")]
        Acreage,
    }

    /// <summary>
    /// 历史病历来源
    /// </summary>
    public enum Enumlsblly
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("本系统")]
        self = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("api")]
        api,
    }

    /// <summary>
    /// 登录标志
    /// </summary>
    public enum EnumLoginFlag
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("单点登录")]
        SSO = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("系统登录")]
        App = 2
    }

    /// <summary>
    /// 饮食类别
    /// </summary>
    public enum Enumyslb {
        /// <summary>
        /// 
        /// </summary>
        [Description("回民普食")]
        hmps = 1,
        /// <summary>
        /// 
        /// </summary>
        [Description("回民半流")]
        hmbl = 2,
        /// <summary>
        /// 
        /// </summary>
        [Description("回民流质")]
        hmlz = 3,
        /// <summary>
        /// 
        /// </summary>
        [Description("一般普食")]
        ybps = 4,
        /// <summary>
        /// 
        /// </summary>
        [Description("一般半流")]
        ybbl = 5,
        /// <summary>
        /// 
        /// </summary>
        [Description("一般流质")]
        yblz = 6,
        /// <summary>
        /// 
        /// </summary>
        [Description("膳食自备")]
        sszb = 7,
        /// <summary>
        /// 
        /// </summary>
        [Description("禁食")]
        js = 8,
        /// <summary>
        /// 
        /// </summary>
        [Description("肠内营养")]
        cnyy = 9,
        /// <summary>
        /// 
        /// </summary>
        [Description("营养补充剂")]
        yybcj = 10,
    }

    /// <summary>
    /// 门诊挂号来源
    /// </summary>
    public enum EnumMzghly
    {
        [Description("His")]
        his = 0,
        [Description("预约")]
        yy = 1,
        [Description("银医通")]
        yyt = 3,
        [Description("微信")]
        wechat = 4,
        [Description("支付宝")]
        alipay = 5,
        [Description("体检")]
        tj = 6,
    }

    /// <summary>
    /// 排队叫号状态
    /// </summary>
    public enum EmunQueueCalledStu
    {
        [Description("已签到")]
        sign = 1,
        [Description("已叫")]
        call = 2,
        [Description("过号")]
        pass = 3,
        [Description("应答")]
        reply = 4,
    }

    /// <summary>
    /// 门诊住院标志  0 通用 1 仅门诊 2 仅住院 3 系统（门诊住院都不可用）例如：家床
    /// </summary>
    public enum Enummzzybz
    {

        /// <summary>
        /// 仅门诊
        /// </summary>
        [Description("门诊")]
        mz = 1,
        /// <summary>
        /// 仅住院
        /// </summary>
        [Description("住院")]
        zy = 2,
        ///// <summary>
        ///// 通用
        ///// </summary>
        //[Description("通用")]
        //ty = 9,
    }
    public enum Enumzxzt
    {
        /// <summary>
        /// 未执行
        /// </summary>
        [Description("未执行")]
        wzx = 0,
        /// <summary>
        /// 已执行
        /// </summary>
        [Description("已执行")]
        yzx = 1
       
    } 
    /// <summary>
    /// 分诊叫号状态
    /// </summary>
    public enum EmunConsultCalledStu
    {
        [Description("待叫号")]
        wait = 1,
        [Description("已叫")]
        call = 2,
        [Description("过号")]
        pass = 3,
        [Description("应答")]
        reply = 4,
        [Description("重新叫号")]
        recall = 5,
    }
    /// <summary>
    /// 远程诊疗状态
    /// </summary>
    public enum Emunzlzt
    {
        [Description("待确认")]
        dqr = 1,
        [Description("就诊中")]
        jzz = 2,
        [Description("已结束")]
        yjs = 3,
        [Description("已退回")]
        yth = 4,
        [Description("已撤销")]
        ycx = 5,
        [Description("已发药")]
        yfy = 6,
    }
    /// <summary>
    /// 损益标志
    /// </summary>
    public enum EnumSybz
    {
        /// <summary>
        /// 报损
        /// </summary>
        [Description("报损")]
        Loss = 0,

        /// <summary>
        /// 报溢
        /// </summary>
        [Description("报溢")]
        Profit = 1,
    }
    public enum EnumOutOrInStorageBillType
    {
        /// <summary>
        /// 外部入库
        /// </summary>
        [Description("外部入库")]
        Wbrk = 1,

        /// <summary>
        /// 外部出库
        /// </summary>
        [Description("外部出库")]
        Wbck = 2,

        /// <summary>
        /// 直接出库
        /// </summary>
        [Description("直接出库")]
        Zjck = 3,

        /// <summary>
        /// 申领出库
        /// </summary>
        [Description("申领出库")]
        Slck = 4,

        /// <summary>
        /// 内部退货
        /// </summary>
        [Description("内部退货")]
        Nbth = 5,

        /// <summary>
        /// 报损报溢
        /// </summary>
        [Description("报损报溢")]
        Bsby = 6,

        /// <summary>
        /// 配送至科室
        /// </summary>
        [Description("出库至科室")] //单据号ckZKS111111111111
        chukuzhikeshi = 7,

        /// <summary>
        /// 科室申领
        /// </summary>
        [Description("科室申领")]
        kssl = 8,

        /// <summary>
        /// 科室申领出库
        /// </summary>
        [Description("科室申领出库")]
        ksSlck = 9,

        /// <summary>
        /// 采购计划
        /// </summary>
        [Description("采购计划")]
        purchasingPlan = 10,

        /// <summary>
        /// 采购订单
        /// </summary>
        [Description("采购订单")]
        purchasingOrder = 11,
    }
    /// <summary>
    /// 有效/无效
    /// </summary>
    public enum Enumzt
    {
        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Disable = 0,

        /// <summary>
        /// 有效
        /// </summary>
        [Description("有效")]
        Enable = 1

    }
    /// <summary>
    /// 审核状态
    /// </summary>
    public enum EnumAuditState
    {

        /// <summary>
        /// 待处理
        /// </summary>
        [Description("待处理")]
        Waiting = 0,

        /// <summary>
        /// 审核通过
        /// </summary>
        [Description("审核通过")]
        Adopt = 1,

        /// <summary>
        /// 审核不通过
        /// </summary>
        [Description("审核不通过")]
        Refuse = 2,

        /// <summary>
        /// 已作废
        /// </summary>
        [Description("已作废")]
        Cancelled = 3,

        /// <summary>
        /// 暂存
        /// </summary>
        [Description("暂存")]
        Temporary = 4,
    }
    /// <summary>
    /// 库存显示
    /// </summary>
    public enum EnumKCXS
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("全部")]
        None = -1,
        /// <summary>
        /// 显示零库存
        /// </summary>
        [Description("显示零库存")]
        Xslkc = 0,
        /// <summary>
        /// 不显示理论数量为零
        /// </summary>
        [Description("不显示理论数量为零")]
        Bxsllslwl = 1,
        /// <summary>
        /// 不显示实际数量为零
        /// </summary>
        [Description("不显示实际数量为零")]
        Bxssjslwl = 2,
        /// <summary>
        /// 不显示实际数量为零
        /// </summary>
        [Description("不显示两者都为零")]
        Bxslzdwl = 4
    }
}

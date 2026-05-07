var keyValue = $.request("keyValue");
var fromly = $.request("fromly");
var readbz = $.request("readbz");//读卡标识 读卡类取消身份证验证是否存在一卡通信息
var cz = "";//确认操作
var sfzh;
var cityref;
$(function () {
    if ($.request("parent") === 'cisframe') {
        fromly = "yktmodify";
    }
    if (fromly === "yktmodify") {
        $('#divYktRegister').remove();
        $('#divYktModify').show();
    }
    else {
        $('#divYktRegister').show();
        $('#divYktModify').remove();
    }
   
    initControl();
    gridInit();
    $('#myTab a:first').tab('show');
    //根据姓名获得拼音
    $('#xm').keyup(function () {
        $('#py').val($(this).toShouPin());

    });

    $("#nlshow").keyup(function () {
        var nlshowText = $(this).val();
        if (!isNaN(nlshowText) && parseInt(nlshowText) > 0) {
            //是数字
            date = new Date();
            if ($('#nlshowdw').val() == '1') {
                //年
                var tempGetYearDate = function (year) {
                    var time = new Date();
                    time.setYear(time.getFullYear() + year);
                    return $('#csny').attr('data-dateFmt') == 'yyyy-MM-dd' ? $.getDate({ date: time }) : $.getTime({ date: time });
                }
                age = 0 - parseInt(nlshowText);
                $("#csny").val(tempGetYearDate(age));
            }
            else if ($('#nlshowdw').val() == '2') {
                //月
                var tempGetMonthDate = function (month) {
                    var time = new Date();
                    time.setMonth(time.getMonth() + month);
                    return $('#csny').attr('data-dateFmt') == 'yyyy-MM-dd' ? $.getDate({ date: time }) : $.getTime({ date: time });
                }
                age = 0 - parseInt(nlshowText);
                $("#csny").val(tempGetMonthDate(age));
            }
            else if ($('#nlshowdw').val() == '3') {
                //日
                var tempGetDayDate = function (day) {
                    var time = new Date();
                    time.setDate(time.getDate() + day);//获取Day天后的日期 
                    return $('#csny').attr('data-dateFmt') == 'yyyy-MM-dd' ? $.getDate({ date: time }) : $.getTime({ date: time });
                }
                age = 0 - parseInt(nlshowText);
                $("#csny").val(tempGetDayDate(age));
            }
            else if ($('#nlshowdw').val() == '4') {
                //时
                var tempGetHourDate = function (hour) {
                    var date1 = new Date().getTime();
                    var date2 = date1 + hour * 60 * 60 * 1000;
                    var newDate = new Date(date2);
                    return $('#csny').attr('data-dateFmt') == 'yyyy-MM-dd' ? $.getDate({ date: newDate }) : $.getTime({ date: newDate });
                }
                age = 0 - parseInt(nlshowText);
                $("#csny").val(tempGetHourDate(age));
            }
        }
    });

    $('#zjh').bind("keydown blur",

        function (e) {
            if (((e.keyCode === 13 && e.type === "keydown") || e.type === "blur") && $('#zjlx').val() === "1") {
                //获取输入的身份证号
                var sfzh = $(this).val();
                var len = $(this).val().length;
                //if (len == 18 || len == 15) {//checkCard()
                if (len === 18 || len === 15) {
                    var csrq;
                    csrq = sfzh.substr(6, 8);
                    if ((len === 15 && csrq) || len === 18) {
                        csrq = csrq.replace(/(.{4})(.{2})/, "$1-$2-");
                        //获取性别
                        var xb;
                        if (parseInt(sfzh.charAt(16) / 2) * 2 != sfzh.charAt(16))
                            xb = 1;
                        else
                            xb = 0;
                        //$('#zjh').val(sfzh);
                        $('#csny').val($('#csny').attr('data-dateFmt') == 'yyyy-MM-dd' ? $.getDate({ date: csrq }) : $.getTime({ date: csrq }));
                        $("input[name=xb]").parent().removeClass('active');
                        $("input[name=xb]").removeAttr('checked');
                        if (xb == 0) {
                            //$("input[name=xb]:eq(1)").attr("checked", 'checked'); //女
                            //$("input[name=xb]:eq(1)").parent().addClass('active');
                            //$('#xb').val("2");
                            $("input[name=xb]:eq(1)").trigger('click'); //女

                        }
                        else {
                            //$("input[name=xb]:eq(0)").attr("checked", 'checked'); //男
                            //$("input[name=xb]:eq(0)").parent().addClass('active');
                            //$('#xb').val("1");

                            $("input[name=xb]:eq(0)").trigger('click'); //男
                        }
                        setAge($('#csny').val());
                    }
                }
            }
        });
    $("#hu_dz").keyupEnterEvent(function () {
        submitForm();
    });
    $('#nlshowdw').change(function () {
        $('#nlshow').trigger('keyup');
    });
});

function setAge(strBirthday) {
    var theAge = getAgeFromBirthTime({ begin: strBirthday });
    if (!!theAge.text && !!theAge.nldw) {
        $("#nlshow").val(theAge.nl);
        $('#nlshowdw').val(theAge.nldw); //
    }
}

function initControl() {
    ///报销政策
    $("#brxzmc").newtouchBatchFloatingSelector({
        height: 200,
        width: 400,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysPatientNatureList, function (idx, val) {
                if ((val.py && val.py.toLowerCase().indexOf(keyword.toLowerCase()) >= 0) ||
                    keyword.trim() == "" ||
                    (val.brxzmc && val.brxzmc.toLowerCase().indexOf(keyword) >= 0)) {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "病人性质",
        colModel: [
            { label: '编码', name: 'brxz', widthratio: 25 },
            { label: '名称', name: 'brxzmc', widthratio: 25 },
            { label: '拼音', name: 'py', widthratio: 25 }
        ],
        itemdbclickhandler: function ($thistr) {
            if ($thistr.attr('data-brxz') === "8") {
                //判断是否需要读卡
                if ($("#xnhylzh").val() === "" || $("#xnhgrbm").val() === "") {
                    $.modalAlert("新农合病人请先读卡！", 'error');
                    return;
                }
            }
            $("#brxzmc").attr("data-label", $thistr.attr('data-brxz'));
            $("#brxzmc").val($thistr.attr('data-brxzmc'));
            return;
        }
    });
    $("#xl").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('Degree'), function () {
                $('#xl').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });
    //最佳联系方式
    $("#zjlxfs").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('ContactWay'), function () {
                $('#zjlxfs').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });
    //国籍
    $("#gj2").newtouchBindSelect({
        //id: "gjCode",
        //text: "gjmc",
        datasource: function () {
            var resultObjArr = new Array();
            if (top.window.clients.sysNationalityList) {
                $.each(top.window.clients.sysNationalityList, function (idx, val) {
                    
                    if (val.gjCode == "cn") {
                        $("#gj2").append('<option value="' + val.gjCode + '" selected>' + val.gjmc + '</option>');
                    } else {
                        $("#gj2").append('<option value="' + val.gjCode + '">' + val.gjmc + '</option>');
                    }
                    //resultObjArr.push(val);
                });
            }
            return resultObjArr;
        }
    });
    //民族
    $("#mz2").newtouchBindSelect({
        //id: "mzCode",
        //text: "mzmc",
        datasource: function () {
            var resultObjArr = new Array();
            if (top.window.clients.sysNationList) {
                $.each(top.window.clients.sysNationList, function (idx, val) {
                    if (val.mzCode == "00000001") {
                        $("#mz2").append('<option value="' + val.mzCode + '" selected>' + val.mzmc + '</option>');
                    } else {
                        $("#mz2").append('<option value="' + val.mzCode + '">' + val.mzmc + '</option>');
                    }

                    //resultObjArr.push(val);
                });
            }
            return resultObjArr;
        }
    });
    //职业
    $("#zy").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('Profession'), function () {
                $('#zy').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });
    //病人来源
    $("#brly").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('PatientSource'), function () {
                $('#brly').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });
    //婚姻
    //$("#hf").newtouchBindSelect({
    //    datasource: function () {
    //        var resultObjArr = new Array();
    //        $.each($.itemDetails.getItems('hy'), function () {
    //            $('#hf').append('<option value="' + this.Code + '">' + this.Name + '</option>');
    //        });
    //        return resultObjArr;
    //    }
    //});
    //紧急联系人关系
    $("#jjllrgx").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('RelativeType'), function () {
                $('#jjllrgx').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });
    if (zjpz == 'OFF') {
        $(".zjclass").text('');
    }

    //
    $('#zjlx').bindSelect();
    $('#hf').bindSelect();
    $('#brxzmc').attr('autocomplete', 'off');

    $('#citylist_JJLXRDZ').citys({
        required: false,
        nodata: 'disabled',
        //province: ' ',
        //city: '',
        //area: '',
        onChange: function (info) {
            //townFormat(info);
        }
    }, function (api) {
        var info = api.getInfo();
        //townFormat(info);
    });

    $('#citylist_HKDZ').citys({
        required: false,
        nodata: 'disabled',
        //province: ' ',
        //city: '',
        //area: '',
        onChange: function (info) {
            //townFormat(info);
        }
    }, function (api) {
        var info = api.getInfo();
        //townFormat(info);
    });

    $('#citylist_CSD').citys({
        required: false,
        nodata: 'disabled',
        //province: '',
        //city: '',
        //area: '',
        onChange: function (info) {
            //townFormat(info);
        }
    }, function (api) {
        var info = api.getInfo();
        //townFormat(info);
    });
    $('#citylist_XDZ').citys({
        required: false,
        nodata: 'disabled',
        //province: '',
        //city: '',
        //area: '',
        onChange: function (info) {
            //townFormat(info);
        }
    }, function (api) {
        var info = api.getInfo();
        //townFormat(info);
    });


}

function gridInit()
{   
    if (fromly) { //非门诊挂号住院登记新建弹出 ，取消就诊卡列表初始化
        return;
    }
    var $gridList = $("#patGridList");
    $gridList.dataGrid({
        //url: "/OutpatientManage/OutpatientAccounting/PatSearchInfo",
        //postData: { zjh: $("#zjh").val(), },
        height: 100,
        //width: $("#divPatCard").width() - 10,
        caption: '患者就诊卡列表',
        colModel: [
                    { label: '主键', name: 'patid', hidden: true },
                    { label: '病历号', name: 'blh', width: 80, align: 'left' },
                    { label: '姓名', name: 'xm', width: 80, align: 'left' },
                    { label: '出生年月', name: 'csny', hidden: true, width: 120, align: 'left' },
                    {
                        label: '性别', name: 'xb', width: 80, align: 'left', formatter: function (cellvalue) {
                            return $.getGender(cellvalue);
                        }
                    },
                    {
                        label: '年龄', name: 'nlshow', width: 80, align: 'left', formatter: function (cellvalue, options, rowObject) {
                            return getAgeFromBirthTime({ begin: rowObject.csny }).text;
                        }
                    },
                    { label: '证件号', name: 'zjh', width: 120, align: 'left' },
                    { label: '卡号', name: 'kh', width: 80, align: 'left' },
                    { label: '卡类型', name: 'CardTypeName', width: 80, align: 'left' },
                    { label: '病人性质', name: 'brxzmc', width: 80, align: 'left' },
                        { label: 'CardType', name: 'CardType', width: 80, align: 'left', hidden: true, },
                    { label: 'CardId', name: 'CardId', width: 80, align: 'left', hidden: true, },
                        { label: '', name: 'CreateTime', hidden: true }
        ],
        pager: "#gridPager",
        sortname: 'CreateTime desc',
        viewrecords: true,
        gridComplete: function () {
            var rowIds = $gridList.jqGrid('getDataIDs');
            var exsistzf = false;
            if (rowIds.length > 0) {
                $.each(rowIds, function (index, value) {
                    var cardtype = $gridList.jqGrid('getRowData', rowIds[index]).CardType;
                    if (cardtype==="1")//是否已存在虚拟卡
                    {
                        exsistzf = true;
                        return;
                    }
                });
               
                var rowData = $gridList.jqGrid('getRowData', rowIds[0]);
                var refkh = '';var refblh='';
                $.najax({
                    url: "/PatientManage/HospiterRes/GetFormJson?T=" + new Date(),
                    data: { "keyValue": rowData.patid },
                    dataType: 'json',
                    async: false,
                    success: function (rep) {
                        if (rep.state !== 'error') {
                            refkh = $("#kh").val(); refblh = $("#blh").val();
                            $("#form1").formSerialize(rep);
                            brxzinit();
                            setTimeout(function () {
                                getcitycode(rep);
                            }, 2000);
                        }
                    }
                });
                if (exsistzf) {
                    $("#divPatCard").css('display', 'block');
                    $.modalAlert("当前证件号已存在一卡通信息,请选中下方就诊卡号列表", "error");
                } else {
                    cz = "SaveFrom";
                    $('#kh').val(refkh);
                    $('#blh').val(refblh);
                }
            }
            else {
                $("#divPatCard").css('display', 'none');
                //$("#form1")[0].reset();//初始化
                $("#patid").val("");
                //患者性质对照赋值
                brxzinit();
                $.ajax({
                    url: "/PatientManage/HospiterRes/GetNewCardNoAndBLH",
                    data: { khsc: khsc, blhsc: blhsc },
                    dataType: "json",
                    async: false,
                    cache: false,
                    success: function (data) {
                        $('#kh').val(data.kh);
                        $('#blh').val(data.blh);
                    }
                });
               
            }
        },
    });
    $("#divPatCard").css('display', 'none');
}

function brxzinit()
{
    //患者性质对照赋值
    var ybfyxzCompResult = {
        Code: '0',
        Name: '自费'
    };
    $("#brxzmc").val(ybfyxzCompResult.Name).attr("data-label", ybfyxzCompResult.Code);
    $('#cardtype').val(xnkCardType);//默认虚拟卡
}

function submitForm(callback) {
    debugger
    var result = checkNotNull();
    if (result) {
        var data = $("#form1").formSerialize();
        data = savecitycode(data);
        data.brxz = $("#brxzmc").attr("data-label");
        data.yktbz = data.patid && !fromly ? "yktcardregister" : fromly; //yktregister 更新基本信息同时新增卡信息
        if (openYbSett == 'True') {
            //开启了医保交易 判断‘非医保卡，不能选择医保费用性质’·
            if (data.cardtype !== ybkCardType
                && ($("#brxzmc").val().indexOf('医保') != -1 || $("#brxzmc").val().indexOf('工伤') != -1 || $("#brxzmc").val().indexOf('生育') != -1)) {
                $.modalAlert("非医保患者，不能选择医保费用性质", 'error');
                return false;
            }
            //开启了医保交易 判断‘医保卡，只能选择医保费用性质’
            if (data.cardtype === ybkCardType
                && ($("#brxzmc").val().indexOf('医保') == -1 && $("#brxzmc").val().indexOf('工伤') == -1 && $("#brxzmc").val().indexOf('生育') == -1)) {
                $.modalAlert("医保患者，只能选择医保费用性质", 'error');
                return false;
            }
        }
        if (!data.xb) {
            $.modalAlert("请选中性别！", 'warning');
            return false;
        }
        $.submitForm({
            url: "/PatientManage/HospiterRes/SubmitForm",
            param: { PatientBasicAndCardInfoVO: data },
            success: function () {
                
                var parent = $.request("parent");
                if (parent && parent === 'patientlist') {
                    //来自患者列表页面触发
                    $.currentWindow().$("#gridList").resetSelection();
                    $.currentWindow().$("#gridList").trigger("reloadGrid");
                    return;
                }
                else if ((parent && parent === 'cisframe') || parent==="OutChargeRegister") {
                    //来自CIS嵌套该修改页面
                    // return;
                    callback({ kh: data.kh, cardtype: data.cardtype,brxz:data.brxz,zjh:data.zjh});
                }
                else if (parent && parent === 'ZYZfToYb') {
                    ///住院自费转医保
                    $.currentWindow().CallbackPatientQuery(ajaxref.data);
                }
                else {
                    //来自登记页面触发
                    
                    $.currentWindow().CallbackPatientQuery({ kh: data.kh, cardtype: data.cardtype });
                }
            }
        });
    }
}

function valiPatZjh()
{   
    if ($("#zjh").val())
    {
        if (fromly || readbz!=="1") { //患者一卡通页面及医保读卡取消就诊卡列表加载
            return;
        }
        var $gridList = $("#patGridList");
        $gridList.jqGrid('setGridParam', {
            url: "/OutpatientManage/OutpatientAccounting/PatSearchInfo",
            postData: { zjh: $("#zjh").val(),zjlx:'1' },
        }).trigger('reloadGrid');
    }
}
var theAcceptClickCallBack = null;
function AcceptClick(callBack) {

    theAcceptClickCallBack = callBack;
    
    var $gridList = $("#patGridList");
    var rowIds = $gridList.jqGrid('getDataIDs');
    if (rowIds.length == 0||cz=="SaveFrom") {
        submitForm();
    }
    else { 
    var blh = $gridList.jqGridRowValue().blh;
    if (!!!blh) {
        $.modalAlert("尚未选择一条就诊卡记录", "error");
        return;
    }
    var obj = new Object();
    obj.blh = blh;
    obj.nlshow = $gridList.jqGridRowValue().nlshow;
    obj.xm = $gridList.jqGridRowValue().xm;
    obj.patid = $gridList.jqGridRowValue().patid;
    obj.startTime = $.currentWindow().$("#ks").val();
    obj.endTime = $.currentWindow().$("#js").val();
    obj.CardId = $gridList.jqGridRowValue().CardId;
    obj.CardType = $gridList.jqGridRowValue().CardType;
    callBack(obj);
    }
}

//确定 主页面调用
function PatSearchConfirm() {
    AcceptClick();
}
//jqGrid 双击选中某行
function btn_edit() {
    PatSearchConfirm();
}
function checkNotNull() {
    //卡号
    var kh = $("#kh").val();
    if (!kh && fromly != "yktmodify") {
        $.modalAlert("请填写卡号！", 'warning');
        return false;
    }
    //病历号
    var blh = $("#blh").val();
    if (!blh && fromly != "yktmodify") {
        $.modalAlert("请填写病历号！", 'warning');
        return false;
    }
    //姓名
    var xm = $("#xm").val();
    if (!xm) {
        $.modalAlert("请填写姓名！", 'warning');
        return false;
    }
    //身份证
    var zjh = $("#zjh").val();
    if (!zjh) {
        $.modalAlert("请填写身份证号！", 'warning');
        return false;
    }

    //病人性质
    var brxzmc = $("#brxzmc").val();
    if (!brxzmc && fromly != "yktmodify") {
        $.modalAlert("请选择病人性质！", 'warning');
        return false;
    }

    //证件类型
    var zjlx = $("#zjlx").val();
    if ((zjlx === "" || !zjlx) && zjpz == 'ON') {
        $.modalAlert("请选择证件类型！", 'warning');
        return false;
    }
    var zjh = $("#zjh").val();
    if (!!!zjh && zjpz == 'ON') {
        $.modalAlert("请填写证件号！", 'warning');
        return false;
    }
    ////暂时去除身份验证----lixin 20190816
    //if (!checkCard()) {
    //    return false;
    //}
    //出生年月
    var csny = $("#csny").val();
    if (csny == null || csny === "" || csny == undefined) {
        $.modalAlert("请填写出生年月!", 'warning');
        return false;
    }
    //拼音
    var py = $("#py").val();
    if (!py) {
        if (!!$("#xm").val()) {
            py = $("#xm").toShouPin();
        }
        if (!py) {
            $.modalAlert("拼音为空！", 'warning');
            return false;
        }
        else {
            $("#py").val(py);
        }
    }
    //性别
    var xb = false;
    $('input[name="xb"]').each(function () {
        var $this = $(this);
        if ($this.parent().hasClass("active")) {
            xb = true;
            $("#xb").val();
        }
    });
    if (!xb) {
        $.modalAlert("请选中性别！", 'warning');
        return false;
    }
    //职业
    var zy = $("#zy").val();
    //if (zy === "" || !zy) {
    //    $.modalAlert("请选择职业 ！", 'warning');
    //    return false;
    //}
    //地址
    //if (!$('#xian_dz').val()) {
    //    $.modalAlert("请填写现地址！", 'warning');
    //    return false;
    //}

    //紧急联络人
    var nlshowdw = $('#nlshowdw option:selected').val();
    var nlshow = $('#nlshow').val();
    if (nlshowdw === '1' && nlshow < 14) {
        if (!$('#jjllr').val()) {
            $.modalAlert("14岁以下请填写紧急联络人 ！", 'warning');
            return false;
        }
        if (!$('#jjlldh').val()) {
            $.modalAlert("14岁以下请填写紧急联络电话 ！", 'warning');
            return false;
        }
    }

    var validator = $('#form1').validate();
    validator.settings = {
        rules: {
            //hf: { required: true },
            dh: { isPhone: true },
            email: { email: true },
            phone: { isMobile: true },
            jjlldh: { isPhone: true }
        },
        messages: {
            //hf: { required: "请选择婚否！" },
            dh: { isPhone: "电话格式不正确！" },
            email: { email: "邮箱格式不正确！" },
            phone: { isMobile: "手机格式不正确！" },
            jjlldh: { isPhone: "电话格式不正确！" }
        },
        showErrors: function (errorMap, errorList) {
            if (!$.isEmptyObject(errorList)) {
                if (Array.isArray(errorList) && errorList.length == 0) {
                    return;
                }
                $.modalAlert(errorList[0].message, 'warning');
            }
        }
    }
    if (!validator.form()) {
        return false;
    }

    return true;
}
var vcity = {
    11: "北京", 12: "天津", 13: "河北", 14: "山西", 15: "内蒙古",
    21: "辽宁", 22: "吉林", 23: "黑龙江", 31: "上海", 32: "江苏",
    33: "浙江", 34: "安徽", 35: "福建", 36: "江西", 37: "山东", 41: "河南",
    42: "湖北", 43: "湖南", 44: "广东", 45: "广西", 46: "海南", 50: "重庆",
    51: "四川", 52: "贵州", 53: "云南", 54: "西藏", 61: "陕西", 62: "甘肃",
    63: "青海", 64: "宁夏", 65: "新疆", 71: "台湾", 81: "香港", 82: "澳门", 91: "国外"
};

//check身份证
checkCard = function () {
    if ($('#zjlx').val() !== "1") {
        //证件类型 非身份证
        return true;
    }
    var card = $("#zjh").val().toUpperCase();
    if (!!!card) {
        if (zjpz == 'ON') {
            $.modalAlert("请填写证件号！", 'warning');
            return false;
        }
        else if (zjpz == 'OFF') {
            return true;
        }
    }
    $("#zjh").val(card);
    //是否为空  
    if (!!!card) {
        $.modalAlert('请输入身份证号，身份证号不能为空', 'warning');
        // $("#zjh").focus;
        return false;
    }
    //校验长度，类型  
    if (isCardNo(card) === false) {
        $.modalAlert('您输入的身份证号码不正确，请重新输入', 'warning');
        //$("#zjh").focus;
        return false;
    }
    //检查省份  
    if (checkProvince(card) === false) {
        $.modalAlert('您输入的身份证前两位不正确,请重新输入', 'warning');
        //$("#zjh").focus;
        return false;
    }
    //校验生日  
    if (checkBirthday(card) === false) {
        $.modalAlert('您输入的身份证号码生日不正确,请重新输入', 'warning');
        //$("#zjh").focus();
        return false;
    }
    //检验位的检测  
    if (checkParity(card) === false) {
        $.modalAlert('您的身份证校验位不正确,请重新输入', 'warning');
        // $("#zjh").focus();
        return false;
    }
    return true;
};


//检查号码是否符合规范，包括长度，类型  
isCardNo = function (card) {
    //身份证号码为15位或者18位，15位时全为数字，18位前17位为数字，最后一位是校验位，可能为数字或字符X  
    var reg = /(^\d{15}$)|(^\d{17}(\d|X)$)/;
    if (reg.test(card) === false) {
        return false;
    }

    return true;
};

//取身份证前两位,校验省份  
checkProvince = function (card) {
    var province = card.substr(0, 2);
    if (vcity[province] == undefined) {
        return false;
    }
    return true;
};

//检查生日是否正确  
checkBirthday = function (card) {
    var len = card.length;
    //身份证15位时，次序为省（3位）市（3位）年（2位）月（2位）日（2位）校验位（3位），皆为数字  
    if (len == '15') {
        var re_fifteen = /^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$/;
        var arr_data = card.match(re_fifteen);
        var year = arr_data[2];
        var month = arr_data[3];
        var day = arr_data[4];
        var birthday = new Date('19' + year + '/' + month + '/' + day);
        return verifyBirthday('19' + year, month, day, birthday);
    }
    //身份证18位时，次序为省（3位）市（3位）年（4位）月（2位）日（2位）校验位（4位），校验位末尾可能为X  
    if (len == '18') {
        var re_eighteen = /^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9]|X)$/;
        var arr_data = card.match(re_eighteen);
        var year = arr_data[2];
        var month = arr_data[3];
        var day = arr_data[4];
        var birthday = new Date(year + '/' + month + '/' + day);
        return verifyBirthday(year, month, day, birthday);
    }
    return false;
};

//校验日期  
verifyBirthday = function (year, month, day, birthday) {
    var now = new Date();
    var now_year = now.getFullYear();
    //年月日是否合理  
    if (birthday.getFullYear() == year && (birthday.getMonth() + 1) == month && birthday.getDate() == day) {
        //判断年份的范围（3岁到100岁之间)  
        //判断年份的范围（1岁到150岁之间)  
        var time = now_year - year;
        //if (time >= 3 && time <= 100) {
        if (time >= 1 && time <= 150) {
            return true;
        }
        return false;
    }
    return false;
};

//校验位的检测  
checkParity = function (card) {
    return true;
    //15位转18位  
    card = changeFivteenToEighteen(card);
    sfzh = card;
    var len = card.length;
    if (len == '18') {
        var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
        var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
        var cardTemp = 0, i, valnum;
        for (i = 0; i < 17; i++) {
            cardTemp += card.substr(i, 1) * arrInt[i];
        }
        valnum = arrCh[cardTemp % 11];
        if (valnum == card.substr(17, 1)) {
            return true;
        }

        return false;
    }
    return false;
};

//15位转18位身份证号  
changeFivteenToEighteen = function (card) {
    if (card.length == '15') {
        var arrInt = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2);
        var arrCh = new Array('1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2');
        var cardTemp = 0, i;
        card = card.substr(0, 6) + '19' + card.substr(6, card.length - 6);
        for (i = 0; i < 17; i++) {
            cardTemp += card.substr(i, 1) * arrInt[i];
        }
        card += arrCh[cardTemp % 11];
        return card;
    }
    return card;
};

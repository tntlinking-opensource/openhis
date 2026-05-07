
var keyValue = $.request("keyValue");
var zyhpz = "";
var boolsearchwithkh = $("#searchwithkh").val() === "ON";
var patInfoObj = {};
var ybnhlx = "";
var IsSuccess = true;//异常标示,false标示有异常
$(function () {
    $("#ryrq").val($.getTime());
    $("#bje").val("");
    zyhpz = $("#zyhpz").val();
    $("#zyh,#kh").keydown(function (e) {
        var ev = document.all ? window.event : e;
        var v_id = $(ev.target).attr('id');
        if ((e.keyCode === 13 && boolsearchwithkh) || (v_id === "zyh" && e.keyCode === 13 && $.isEmptyObject(patInfoObj))) {
            var kh = $("#kh").val();
            $.ajax({
                url: "/PatientManage/HospiterRes/CheckCardState",
                data: { "kh": kh, "zyh": $("#zyh").val() },
                dataType: 'json',
                async: false,
                success: function (rep) {
                    if (rep && rep.data) {
                        ClearAll();
                        CallbackPatientQuery(rep.data);
                        return;
                        //if (rep === 0) {
                        //    //无卡做免卡登记 
                        //    btn_NocardRes();
                        //} else {
                        //    if (rep !== undefined) {
                        //        fillInfo(rep);
                        //    }
                        //}
                    }

                    //无卡做免卡登记 
                    btn_NocardRes();
                }
            });
        }
    });


    initControl();
    $("#lxrdz2").keyupEnterEvent(function () {
        btn_Save();
    });

    if (!boolsearchwithkh) {
        //病历号，姓名，拼音关键字搜索
        $('#kh').newtouchBatchFloatingSelector({
            width: 500,
            height: 200,
            caption: "选择患者",
            url: "/PatientManage/HospiterRes/PatSearchInfo",
            ajaxparameters: function ($thisinput) {
                var keyword = $thisinput.val().trim();
                return "keyword=" + keyword;
            },
            itemdbclickhandler: function ($thistr, $thisinput) {
                CallbackPatientQuery($thistr.attr('data-blh'));
            },
            colModel: [
                { label: '主键', name: 'patid', hidden: true },
                { label: '病历号', name: 'blh', width: 130, align: 'left' },
                { label: '姓名', name: 'xm', width: 120, align: 'left' },
                { label: '出生年月', name: 'csny', hidden: true, width: 100, align: 'left' },
                {
                    label: '性别', name: 'xb', width: 70, align: 'left', formatter: function (cellvalue) {
                        return $.getGender(cellvalue);
                    }
                },
                {
                    label: '年龄', name: 'nl', width: 100, align: 'left', formatter: function (cellvalue, a, b) {
                        return getAgeFromBirthTime({ begin: b.csny }).text;
                    }
                },
                { label: 'brly', name: 'brly', align: 'left', hidden: true },
                { label: 'zjh', name: 'zjh', align: 'left', hidden: true },
                { label: 'kh', name: 'kh', align: 'left', hidden: true },
                { label: 'phone', name: 'phone', align: 'left', hidden: true },
                { label: 'dy', name: 'dy', align: 'left', hidden: true },
                { label: 'zjlx', name: 'zjlx', align: 'left', hidden: true },
                { label: 'sycs', name: 'sycs', align: 'left', hidden: true },
                { label: 'dybh', name: 'dybh', align: 'left', hidden: true },
                { label: 'lxr', name: 'lxr', align: 'left', hidden: true },
                { label: 'lxrgx', name: 'lxrgx', align: 'left', hidden: true },
                { label: 'lxrdh', name: 'lxrdh', align: 'left', hidden: true },
            ]
        });
    }
});

function fillInfo(rep) {
    //有卡显示基本信息
    $("#form1").formSerialize(rep);
    getcitycode(rep);
    if (!rep.zyh) {
        //是新登记
        if (rep.hf || rep.hf == 0) {
            $("#hy").val(rep.hf).trigger('change');
        }
        else {
            $("#hy").val('').trigger('change');
        }
    }
    if ((rep.zyh !== null && rep.hasOwnProperty("zyh")) || zyhpz == "ON") {
        $("#zyh").attr("disabled", "disabled").css("background-color", "#f1f4f6");
        $('#zyh').attr("placeholder", "由系统自动生成");
        $('#zyhlog').html("");
    }
    if (rep["jkjl"]) {
        $("#jkjlmc").attr("data-label", rep["jkjl"]);
    }
    if (rep["doctor"]) {
        $("#doctormc").attr("data-label", rep["doctor"]);
    }
    if (rep["brxz"]) {
        $("#brxzmc").attr("data-label", rep["brxz"]);
    }
    if (rep["ryzd1"]) {
        $("#zdmc1").attr("data-label", rep["ryzd1"]);
    }
    if (rep["ryzd2"]) {
        $("#zdmc2").attr("data-label", rep["ryzd2"]);
    }
    if (rep["ryzd3"]) {
        $("#zdmc3").attr("data-label", rep["ryzd3"]);
    }
    if (rep["ks"]) {
        $("#ksmc").attr("data-label", rep["ks"]);
    }
    if (rep["bq"]) {
        $("#bqmc").attr("data-label", rep["bq"]);
    }
    if (rep["mzCode"]) {
        $("#mzmc").attr("data-label", rep["mzCode"]);
    }
    if (rep["gjCode"]) {
        $("#gjmc").attr("data-label", rep["gjCode"]);   //会：显示无，值有
    }
    if (rep["zjlx"]) {
        var type = rep["zjlx"].trim();
        switch (type) {
            case "1": type = "身份证："; break;
            case "2": type = "护照："; break;
            case "3": type = "军官证："; break;
            default:
                type = "其他："; break;

        }
        $("#zjlx").html(type);
    }

    var patModel = rep;
    $("#xb").html($.getGender(patModel.xb));
    $("#csny").html((patModel.csny && patModel.csny.length >= 10 ? patModel.csny.substring(0, 10) : ""));
    $("#zjh").html(patModel.zjh);
    $("#jsr").html(patModel.jsr);
    $("#xm").html(patModel.xm);
    $("#nlshow").html(getAgeFromBirthTime({ begin: patModel.csny }).text);
    $("#phone").html(patModel.phone);
    //$("#dy").html(patModel.dybh === 1 ? '外地' : (patModel.dybh === 0 ? '本地' : '未知'));
    $("#kh").val(boolsearchwithkh ? patModel.kh : patModel.blh);
    $("#blh").html(boolsearchwithkh ? patModel.blh : patModel.kh);
    $("#hiddenzjlx").val(patModel.zjlx);
    //加载病人信息
    patInfoObj.xm = patModel.xm; //姓名
    patInfoObj.xb = patModel.xb; //性别
    patInfoObj.csny = patModel.csny;
    patInfoObj.patid = patModel.patid; //病人内码
    patInfoObj.blh = patModel.blh; //病历号
    patInfoObj.zjh = patModel.zjh; //证件号
    patInfoObj.jsr = patModel.jsr; //介绍人
    patInfoObj.brxz = patModel.brxz;//病人性质
    patInfoObj.zjlx = patModel.zjlx; //证件类型
    patInfoObj.csny = patModel.csny;//出生年月
    //禁用系统病人基本信息
    DisableSysBasicInfo();
}
function loadPatiInfo(rep) {
    getcitycode(rep);
    $("#gms").val(rep.gms);
    $("#cs_dz").val(rep.cs_dz);
    $("#xian_dz").val(rep.xian_dz);
    $("#hu_dz").val(rep.hu_dz);
    $("#jjlxr_dz").val(rep.jjlxr_dz);
}

function btn_NocardRes(t) {
    if (t != null && t === 1) {
        localStorage.removeItem("patientform");
    }
    $.modalOpen({
        id: "Form",
        title: "一卡通办理",
        url: "/PatientManage/HospiterRes/PatientBasic?T=" + new Date() + "&readbz=" + t,
        width: "1000px",
        height: "450px",
        callBack: function (iframeId) {
            //top.frames[iframeId].submitForm();
            $.currentWindow(iframeId).AcceptClick(function (obj) {
                if (obj.CardType === '2') {
                    if (IsOpenSfzYbJz !== "ON") {
                        $.modalAlert("未开启医保可按身份证就诊动态配置!", 'warning');
                        $.modalClose("Form");
                        return;
                    }
                    //身份证调用医保接口
                    //...
                }
                GetQueryFphAjax(obj);
                $.modalClose("Form");
            });
        }
    });
}

function initControl() {
    ///健康教练
    $("#jkjlmc").newtouchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysStaffDutyList, function (idx, val) {
                if (((val.StaffPY && val.StaffPY.indexOf(keyword.toLowerCase()) >= 0)
                    || (val.StaffName && val.StaffName.indexOf(keyword.toLowerCase()) >= 0)
                    || keyword.trim() == "")
                    && val.DutyCode == "RehabDoctor") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "健康教练",
        colModel: [
            {
                label: '代码', name: 'StaffGh', widthratio: 25
            },
            {
                label: '名称', name: 'StaffName', widthratio: 25
            },
            { label: '拼音', name: 'StaffPY', widthratio: 25 }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#jkjlmc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#jkjlmc").val($thistr.find('td:eq(1)').html());
            return;
        }
    });
    ///医生
    $("#doctormc").newtouchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();

            $.each(top.window.clients.sysStaffDutyList, function (idx, val) {
                if (((val.StaffPY && val.StaffPY.toLowerCase().indexOf(keyword.toLowerCase()) >= 0)
                    || (val.StaffName && val.StaffName.indexOf(keyword.toLowerCase()) >= 0)
                    || keyword.trim() == "")
                    && val.DutyCode == "Doctor") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "医生",
        colModel: [
            {
                label: '工号', name: 'StaffGh', widthratio: 25
            },
            {
                label: '名称', name: 'StaffName', widthratio: 25
            },
            { label: '拼音', name: 'StaffPY', widthratio: 25 }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#doctormc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#doctormc").val($thistr.find('td:eq(1)').html());
            return;
        }
    });
    ///报销政策
    $("#brxzmc").newtouchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysPatientNatureList, function (idx, val) {
                if ((val.py && val.py.toLowerCase().indexOf(keyword.toLowerCase()) >= 0)
                    || (val.brxzmc && val.brxzmc.indexOf(keyword) >= 0)
                    || keyword.trim() == "") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "病人性质",
        colModel: [
            {
                label: '编号', name: 'brxzbh', widthratio: 25
            },
            {
                label: '代码', name: 'brxz', widthratio: 25
            },
            {
                label: '名称', name: 'brxzmc', widthratio: 25
            },
            { label: '拼音', name: 'py', widthratio: 25 },
            { label: 'brxzlb', name: 'brxzlb', widthratio: 25,hidden:true }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#brxzmc").attr("data-brxzmc", $thistr.find("td:eq(2)").html())
                .attr("data-label", $thistr.find("td:eq(1)").html()).attr("data-brxzlb", $thistr.find("td:eq(4)").html());
            $("#brxzmc").val($thistr.find('td:eq(2)').html());
            if (openYbSett && medicalInsurance == "guian" && $("#brxzmc").attr("data-label") === "1") {
                ybnhlx = "yb";
            }
            if (openXnhSett && medicalInsurance == "guian" && $("#brxzmc").attr("data-label") === "8") {
                ybnhlx = "nh";
            }
            $('#zdmc1').trigger('newtouchBatchFloatingSelector');
            $('#zdmc2').trigger('newtouchBatchFloatingSelector');
            $('#zdmc3').trigger('newtouchBatchFloatingSelector');
            //hss 2019.7.19 同个病人存在多个交易类型时，根据报销政策加载卡号
            $.ajax({
                type: "POST",
                url: "/PatientManage/HospiterRes/GetkhInfoBybrxz",
                data: { patid: $('#patid').val(), brxz: $("#brxzmc").attr("data-label") },
                dataType: "json",
                async: false,
                success: function (r) {
                    if (!!r.data && !!r.data.CardNo) {
                        if (!boolsearchwithkh) {
                            //启用了病历号搜索
                            $("#blh").html(r.data.CardNo);

                        } else {
                            $("#kh").val(r.data.CardNo);
                        }
                    }
                }
            });
            return;
        }
    });
    ///入院诊断 zdmc1
    $("#zdmc1").newtouchBatchFloatingSelector({
        height: 200,
        width: 600,
        clickautotrigger: true,
        url: "/PatientManage/HospiterRes/GetryzdSelect",
        ajaxparameters: function () {
            return "ryzd=" + $("#zdmc1").val() + "&ybnhlx=" + ybnhlx;
        },
        caption: "入院诊断",
        colModel: [
            { label: '名称', name: 'zdmc', widthratio: 60 },
            { label: '拼音', name: 'py', widthratio: 20 },
            { label: 'icd10', name: 'icd10', widthratio: 20 },
            { label: '编号', name: 'zdbh', hidden: true },
            { label: '内码', name: 'zdnm', hidden: true }
        ],
        itemdbclickhandler: function (data) {
            $("#zdmc1").attr("data-label", data.attr('data-zdbh') + "-" + data.attr('data-zdnm'));
            $("#zdmc1").val(data.attr('data-zdmc'));
            return;
        }
    });
    ///入院诊断 zdmc2
    $("#zdmc2").newtouchBatchFloatingSelector(
        {
            height: 200,
            width: 600,
            url: "/PatientManage/HospiterRes/GetryzdSelect",
            clickautotrigger: true,
            ajaxparameters: function () {
                return "ryzd=" + $("#zdmc2").val() + "&ybnhlx=" + ybnhlx;
            },
            caption: "入院诊断",
            colModel: [
                { label: '名称', name: 'zdmc', widthratio: 60 },
                { label: '拼音', name: 'py', widthratio: 20 },
                { label: 'icd10', name: 'icd10', widthratio: 20 },
                { label: '编号', name: 'zdbh', hidden: true },
                { label: '内码', name: 'zdnm', hidden: true }
            ],
            itemdbclickhandler: function (data) {
                $("#zdmc2").attr("data-label", data.attr('data-zdbh') + "-" + data.attr('data-zdnm'));
                $("#zdmc2").val(data.attr('data-zdmc'));
                return;
            }
        });
    ///入院诊断 zdmc3
    $("#zdmc3").newtouchBatchFloatingSelector({
        height: 200,
        width: 600,
        clickautotrigger: true,
        url: "/PatientManage/HospiterRes/GetryzdSelect",
        ajaxparameters: function () {
            return "ryzd=" + $("#zdmc3").val() + "&ybnhlx=" + ybnhlx;
        },
        caption: "入院诊断",
        colModel: [
            { label: '名称', name: 'zdmc', widthratio: 60 },
            { label: '拼音', name: 'py', widthratio: 20 },
            { label: 'icd10', name: 'icd10', widthratio: 20 },
            { label: '编号', name: 'zdbh', hidden: true },
            { label: '内码', name: 'zdnm', hidden: true }
        ],
        itemdbclickhandler: function (data) {
            $("#zdmc3").attr("data-label", data.attr('data-zdbh') + "-" + data.attr('data-zdnm'));
            $("#zdmc3").val(data.attr('data-zdmc'));
            return;
        }
    });
    ///科室
    $("#ksmc").newtouchBatchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysDepartList, function (idx, val) {
                if (((val.py && val.py.toLowerCase().indexOf(keyword) >= 0)
                    || (val.Name && val.Name.indexOf(keyword) >= 0)
                    || keyword.trim() == "") && val.mzzybz != '1' && val.Code != 'KS17') {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "科室",
        colModel: [
            { label: '编号', name: 'Code', widthratio: 30 },
            { label: '名称', name: 'Name', widthratio: 50 },
            { label: '拼音', name: 'py', hidden: true }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#ksmc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#ksmc").val($thistr.find('td:eq(1)').html());
            return;
        },
    });
    ///病区
    $("#bqmc").newtouchBatchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            var ks = $("#ksmc").attr("data-label");
            var bqlist = top.window.clients.sysWardDeptRelation.filter((item) => item.ks == ks);
            $.each(bqlist, function (idx, val) {
                if ((val.bqpy && val.bqpy.toLowerCase().indexOf(keyword.toLowerCase()) >= 0)
                    || (val.bqmc && val.bqmc.indexOf(keyword) >= 0)
                    || keyword.trim() == "") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "病区",
        colModel: [
            {
                label: '编号', name: 'bq', widthratio: 25
            },
            {
                label: '名称', name: 'bqmc', widthratio: 50
            }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#bqmc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#bqmc").val($thistr.find('td:eq(1)').html());
            return;
        },
    });
    ///民族
    $("#mzmc").newtouchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysNationList, function (idx, val) {
                if ((val.py && val.py.toLowerCase().indexOf(keyword.toLowerCase()) >= 0)
                    || (val.mzmc && val.mzmc.indexOf(keyword) >= 0)
                    || keyword.trim() == "") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "民族",
        colModel: [
            {
                label: '代码', name: 'mzCode', widthratio: 30
            },
            {
                label: '名称', name: 'mzmc', widthratio: 30
            },
            { label: '拼音', name: 'py', widthratio: 30 }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#mzmc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#mzmc").val($thistr.find('td:eq(1)').html());
            return;
        },
    });
    ///国籍
    $("#gjmc").newtouchFloatingSelector({
        height: 200,
        width: 300,
        clickautotrigger: true,
        filter: function (keyword) {
            //遍历数据源，用keyword来筛选出结果
            var resultObjArr = new Array();
            $.each(top.window.clients.sysNationalityList, function (idx, val) {
                if ((val.py && val.py.indexOf(keyword.toLowerCase()) >= 0)
                    || (val.gjmc && val.gjmc.indexOf(keyword) >= 0)
                    || keyword.trim() == "") {
                    resultObjArr.push(val);
                }
            });
            return resultObjArr;

        },
        caption: "国籍",
        colModel: [
            {
                label: '代码', name: 'gjCode', widthratio: 25
            },
            {
                label: '名称', name: 'gjmc', widthratio: 25
            },
            { label: '拼音', name: 'py', widthratio: 25 }
        ],
        itemdbclickhandler: function ($thistr) {
            $("#gjmc").attr("data-label", $thistr.find("td:eq(0)").html());
            $("#gjmc").val($thistr.find('td:eq(1)').html());
            return;
        },
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
    //转出医院
    //$("#zcyy").newtouchBindSelect({
    //    datasource: function () {
    //		var resultObjArr = new Array();
    //		$.each($.itemDetails.getItems('zcyy'), function () {
    //			$('#zcyy').append('<option value="' + this.Code + '">' + this.Name + '</option>');
    //		});
    //		return resultObjArr;
    //	}
    //});
    //紧急联系人关系
    $("#lxrgx").newtouchBindSelect({
        datasource: function () {
            var resultObjArr = new Array();
            $.each($.itemDetails.getItems('RelativeType'), function () {
                $('#lxrgx').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr;
        }
    });

    //紧急联系人关系
    $("#lxrgx2").newtouchBindSelect({
        datasource: function () {
            var resultObjArr2 = new Array(); $.each($.itemDetails.getItems('RelativeType'), function () {
                $('#lxrgx2').append('<option value="' + this.Code + '">' + this.Name + '</option>');
            });
            return resultObjArr2;
        }
    });

    if (!boolsearchwithkh) {
        //启用了病历号搜索
        $("#lbl_blh").html("卡号：");
        $("#lbl_kh").html("病历号/姓名：");
    }

    //入院诊断必填
    if ($('#isDiagnosisRequired').val() == 'True') {
        $('#spanZyzd').html('*').addClass('rema');
    }

    //
    $('#rytj').bindSelect();
    //
    $('#hy').bindSelect();
}
var errorMsg = "";//存放出错信息
function btn_Save() {
    var result = checkNotNull();
    if (result) {
        var data = $("#form1").formSerialize();
        data = savecitycode(data);
        data["csny"] = patInfoObj.csny;
        data["jkjl"] = $("#jkjlmc").attr("data-label");
        data["doctor"] = $("#doctormc").attr("data-label");
        if ($("#brxzmc").val()) {
            data["brxz"] = $("#brxzmc").attr("data-label");
        }
        else {
            data["brxz"] = null;
        }
        if (!data["brxz"]) {
            $.modalAlert("报销政策必填", 'warning');
            return;
        }
        if ($("#rytj").val() == "3" && !$("#zcyy").val()) {
            $.modalAlert("如果是其他医院转入，请选择转出医院", 'warning');
            return;
        }
        var zd1 = $("#zdmc1").attr("data-label");
        data["ryzd1"] = null;
        if (zd1 && $("#zdmc1").val().trim() != "" && $("#zdmc1").val() != null) {
            data["ryzd1"] = (zd1.split('-', 1))[0];
        } else {
            data["ryzd1"] = null;
        }
        var zd2 = $("#zdmc2").attr("data-label");
        data["ryzd2"] = null;
        if (zd2 && $("#zdmc2").val().trim() != "" && $("#zdmc2").val() != null) {
            data["ryzd2"] = (zd2.split('-', 1))[0];
        } else {
            data["ryzd2"] = null;
        }
        var zd3 = $("#zdmc3").attr("data-label");
        data["ryzd3"] = null;
        if (zd3 && $("#zdmc3").val().trim() != "" && $("#zdmc3").val() != null) {
            data["ryzd3"] = (zd3.split('-', 1))[0];
        } else {
            data["ryzd3"] = null;
        }
        data["ks"] = $("#ksmc").attr("data-label");
        data["bq"] = $("#bqmc").attr("data-label");
        data["mzCode"] = $("#mzmc").attr("data-label");
        data["gjCode"] = $("#gjmc").attr("data-label");
        data["xm"] = $("#xm").html();
        data["blh"] = $("#blh").html();
        if (!boolsearchwithkh) {
            tempp = data["blh"];
            data["blh"] = data["kh"];
            data["kh"] = tempp;
        }
        data["xb"] = $.getGenderCode($("#xb").html());
        data["zjlx"] = $("#hiddenzjlx").val();
        data["zjh"] = $("#zjh").html();
        data["xnhgrbm"] = $("#xnhgrbm").val();
        data["xnhylzh"] = $("#xnhylzh").val();
        data["inpId"] = $("#inpId").val();

        data["bzbm"] = $("#sel_tsbbz").val();
        data["bzmc"] = $("#sel_tsbbz").find("option:selected").text();
        data["cardtype"] = $("#readCardCardType").val();
        data["CardTypeName"] = $("#readCardCardType option:selected").text();

        //入院诊断必填
        if ($('#isDiagnosisRequired').val() == 'True'
            && !!!data["ryzd1"]) {
            $.modalAlert("入院诊断必须填写", 'warning');
            return;
        }


        var jylsh = "";//存放交易流水号
        var jyyzm = "";//存放交易校验码

        if (openYbSett && $("#brxzmc").attr("data-brxzlb") === "1") //启用医保,医保性质
        {
            if (cqPatInfo.ybVer != "shanghaiV5") {
                if (cqPatInfo.ybVer == undefined) {
                    $.modalAlert("报销政策为【医保病人】，请先读卡！", 'warning');
                    return;
                }
            }
            if (!data.zyh) //(无住院号)入院登记
            {
                if (isZYZT) {
                    $.modalAlert("该患者在医保中尚未出院，不能办理入院！", 'warning');
                    return;
                }
            } else { //(有住院号)入院登记修改
                //获取入院登记反馈信息

            }
        }
        var jzlx = cqPatInfo.mdtrt_cert_type;
        data["jzlx"] = cqPatInfo.jzlx;//$('#readCardCardType').val();
        data["cardtype"] = cqPatInfo.jzlx;
        if (IsSuccess) {
            $.submitForm({
                url: "/PatientManage/HospiterRes/SaveSysHosBasicInfo",
                loading: "正在提交数据...",
                param: { vo: data, isybjy: openYbSett },
                successwithtipmsg: false,
                success: function (datareq) {
                    if (datareq.message === "添加成功") {

                        if (openYbSett && $("#brxzmc").attr("data-brxzlb") === "1") {

                            if (cqPatInfo.ybVer == "shanghaiV5") {
                                shanghaiV5ybrydj(datareq, data, jzlx);//上海五期医保入院登记 SJ11
                            } else if (cqPatInfo.ybVer == "gjyb") {
                                gjybrydj(datareq, jzlx);//国家医保程序调用2401 住院登记
                            }
                        }
                        if (!IsSuccess) {
                            $.najax({
                                url: "/PatientManage/HospiterRes/CancelAdmission?zyh=" + datareq.data.zyh,
                                dataType: 'json',
                                async: false,
                                success: function (data) {
                                    if (data.message == "取消入院成功！") {
                                        $.modalAlert(errorMsg, 'error');
                                    }
                                    $.modalAlert(errorMsg, "error");
                                }
                            });
                        } else {
                            $.modalAlert("住院登记成功！住院号为:" + datareq.data.zyh, 'success');
                        }

                    }
                    else if (datareq.message === "修改成功") {
                        $.ajax({
                            type: "POST",
                            url: "/PatientManage/HospiterRes/GetCQjzdjInfo",
                            data: { zyh: data.zyh },
                            dataType: "json",
                            cache: false,
                            async: false,
                            success: function (payoptype) {
                                if (!payoptype) {
                                    $.modalAlert("获取住院就诊登记失败", 'error');
                                    return;
                                }
                                if (cqPatInfo.ybVer == "shanghaiV5") {
                                    shanghaiV5djcx(jzlx, $('#zyh').val());//上海五期医保入院登记撤销
                                    shanghaiV5ybrydj(datareq, data, jzlx);//上海五期医保入院登记 SJ11

                                } else {
                                    gjybrydjxg(payoptype);//国家医保程序调用2401 住院登记
                                }
                            }
                        });
                        if (!IsSuccess) {
                            errorMsg = "医保入院登记修改失败！" + errorMsg;
                        } else {
                            $.modalAlert("住院号为:" + datareq.data.zyh + "，修改成功", 'warning');
                        }
                    } else {
                        //失败，重庆医保登记红冲
                        if (openYbSett && $("#brxzmc").attr("data-brxzlb") === "1") {
                            $.najax({
                                type: "GET",
                                url: "/OutpatientManage/OutpatCharge/GetCQjzdjDataInfo?zymzh=" + datareq.data.zyh,
                                dataType: "json",
                                loadingtext: "正在请求HIS取消入院登记，请稍后…",
                                success: function (ajaxresp) {
                                    if (ajaxresp && ajaxresp.jylsh || cqPatInfo.ybVer == "shanghaiV5") {
                                        if (cqPatInfo.ybVer == "shanghaiV5") {
                                            shanghaiV5djcx(jzlx, $('#zyh').val());//上海五期医保入院登记撤销
                                        }
                                        else {
                                            var payoptype = { hisId: datareq.data.zyh, 'mdtrt_id': ajaxresp.jylsh, 'operatorId': curUserCode, "operatorName": curUserName, 'insuplc_admdvs': $("#xzqh").val(), 'psn_no': $("#rybh").val() };
                                            $.ajax({
                                                type: "POST",
                                                url: "http://127.0.0.1:33333/api/YiBao/HospitaUpMdtrtinfo_2404",
                                                dataType: "json",
                                                data: payoptype,
                                                async: false,
                                                success: function (data) {
                                                    var cqybjyDenySettleReturn = eval('(' + data + ')');
                                                    if (cqybjyDenySettleReturn.infcode == "0") {
                                                        //////就诊登记取消，不需要落地
                                                    } else {
                                                        $.modalAlert('取消住院登记失败：' + cqybjyDenySettleReturn.err_msg, 'success');
                                                    }
                                                }
                                            });
                                        }
                                    }
                                }
                            });
                        }
                        $.modalAlert("入院登记保存失败", 'warning');
                    }
                    ClearAll();
                    $.loading(false);
                }
            });
        } else
            $.modalAlert(errorMsg, 'error');
    }
}
function gjybrydjxg(payoptype) {
    payoptype.mdtrt_cert_type = cqPatInfo.mdtrt_cert_type;
    payoptype.orgId = orgId;
    payoptype.med_type = ($("#rytj").val() == "" ? "21" : $("#rytj").val());//($("#rytj").val() == "3" ? "22" : "21");
    payoptype.czlb = 1;
    $.ajax({
        type: "POST",
        url: "http://127.0.0.1:33333/api/YiBao/HospitaMdtrtinfo_2403",
        dataType: "json",
        data: payoptype,
        async: false,
        success: function (data) {
            var medicalReg = eval('(' + data + ')');
            if (medicalReg) {
                if (medicalReg.infcode == "0") {
                }
                else {
                    IsSuccess = false;
                    errorMsg = medicalReg.err_msg;
                }
            }
        }
    });
}
function shanghaiV5djcx(jzlx, zyh) {
    var payoptype = {
        cardtype: jzlx,
        carddata: jzlx == "3" ? cqPatInfo.ecToken : cqPatInfo.sbkh,
        hisId: zyh,//住院号
        operatorId: curUserCode,
        operatorName: curUserName,
        insuplc_admdvs: $("#xzqh").val()
    };
    $.ajax({
        type: "POST",
        url: "http://127.0.0.1:33333/api/FifthPhaseYiBao/ybInterface_SJ21",
        dataType: "json",
        data: payoptype,
        async: false,
        success: function (data) {
            var medicalReg = eval('(' + data + ')');
            if (medicalReg) {
                if (medicalReg.xxfhm === "P001") {

                }
                else {
                    //$.modalAlert("医保门诊登记失败：" + medicalReg.ErrorMsg, 'error');
                    IsSuccess = false;
                    errorMsg = "医保住院登记撤销失败：" + medicalReg.err_msg;
                }
            }
        }
    });
}
function gjybrydj(datareq, jzlx) {
    var payoptype = {
        mdtrt_cert_type: jzlx,
        mdtrt_cert_no: jzlx == "01" ? cqPatInfo.ecToken : (jzlx == "02" ? $("#sfzh").val() : patModel.kh),
        hisId: datareq.data.zyh,//住院号
        med_type: ($("#rytj").val() == "" ? "21" : $("#rytj").val()),//21普通住院，22转入住院
        operatorId: curUserCode,
        operatorName: curUserName,
        insuplc_admdvs: $("#xzqh").val(),
        orgId: orgId,
        psn_no: $("#rybh").val(),
        dise_codg: $("#sel_tsbbz").val(),
        dise_name: $("#sel_tsbbz").find("option:selected").text()
    };
    $.ajax({
        type: "POST",
        url: "http://127.0.0.1:33333/api/YiBao/HospitaMdtrtinfo_2401",
        dataType: "json",
        data: payoptype,
        async: false,
        success: function (data) {
            var medicalReg = eval('(' + data + ')');
            if (medicalReg) {
                if ((medicalReg.infcode == "0" || medicalReg.infcode == 0) && medicalReg.output.result.mdtrt_id) {
                    $.najax({
                        type: "POST",
                        data: {
                            patid: $("#patid").val(),
                            zymzh: datareq.data.zyh,
                            jylsh: medicalReg.output.result.mdtrt_id,
                            jytype: "2",
                            medicalInList: null
                        },
                        url: "/OutpatientManage/OutpatientReg/SaveCqybMedicalReg",
                        dataType: "json",
                        loading: false,
                        success: function (ajaxresp) {
                        }
                    });
                }
                else {
                    //$.modalAlert("医保门诊登记失败：" + medicalReg.ErrorMsg, 'error');
                    IsSuccess = false;
                    errorMsg = "医保住院登记失败：" + medicalReg.err_msg;
                }
            }
        }
    });
}
function shanghaiV5ybrydj(datareq, data, jzlx) {
    var payoptype = {
        cardtype: cqPatInfo.jzlx,
        carddata: cqPatInfo.carddata,//jzlx == "3" ? cqPatInfo.ecToken : cqPatInfo.sbkh,
        operatorId: curUserCode,
        operatorName: curUserName,
        insuplc_admdvs: $("#xzqh").val(),
        orgId: orgId,
        hisId: datareq.data.zyh//住院号
    };
    $.ajax({
        type: "POST",
        url: "http://127.0.0.1:33333/api/FifthPhaseYiBao/ybInterface_SJ11",
        dataType: "json",
        data: payoptype,
        async: false,
        success: function (data) {
            var medicalReg = eval('(' + data + ')');
            if (medicalReg) {
                if (medicalReg.xxfhm === "P001") {
                    $.najax({
                        type: "POST",
                        data: {
                            patid: $("#patid").val(),
                            zymzh: datareq.data.zyh,
                            jylsh: medicalReg.xxnr.jzdyh,
                            jytype: "2",
                            medicalInList: null
                        },
                        url: "/OutpatientManage/OutpatientReg/SaveCqybMedicalReg",
                        dataType: "json",
                        loading: false,
                        success: function (ajaxresp) {
                        }
                    });
                }
                else {
                    IsSuccess = false;
                    errorMsg = "医保服务【SJ11住院登记失败】：" + medicalReg.fhxx;
                }
            } else {
                IsSuccess = false;
                errorMsg = "医保服务【SJ11住院登记未返回信息】：" + medicalReg.fhxx;
            }
        }, error: function (request, error, ex) {
            success = false;
            errMsg = "医保服务【SJ11】住院登记不可访问：[" + ex + "]";
        }
    });
}
function PrinyZYInfo() {
    if (!$("#patid").val()) {
        $.modalAlert("病人基本信息不全，无法打印住院信息", 'error');
        return;
    } else {
        $.modalAlert("功能正在开发中", 'warning');
        return;
    }
    var data = $("#form1").formSerialize();
    data["ryzd1"] = $("#zdmc1").attr("data-label");
    data["ryzd2"] = $("#zdmc2").attr("data-label");
    data["ryzd3"] = $("#zdmc3").attr("data-label");
    $.ajax({
        url: "/PatientManage/HospiterRes/PrinyZYInfo",
        data: data,
        dataType: 'json',
        async: false,
        success: function (data) {
            if (data.message = "打印成功！") {
                $.modalAlert(data.message, "success");
            }
        }
    });
}

function PrintWDInfo() {
    if (!$("#patid").val()) {
        $.modalAlert("病人基本信息不全，无法打印腕带", 'error');
        return;
    } else {
        $.modalAlert("功能正在开发中", 'warning');
        return;
    }
}

function Cancel() {
    debugger;
    if (!$("#patid").val()) {
        $.modalAlert("病人基本信息不全，无法打印取消入院", 'error');
        return;
    }

    var IsSuccess = true;//异常标示,false标示有异常
    var errorMsg = "";//存放出错信息
    var jylsh = "";//存放交易流水号
    var jyyzm = "";//存放交易校验码

    if (openYbSett && $("#brxzmc").attr("data-brxzlb") === "1") //启用医保,贵安医保,医保性质
    {
        $.najax({
            type: "GET",
            url: "/PatientManage/HospiterRes/CheckCancelAdmission?zyh=" + $("#zyh").val(),
            dataType: "json",
            async: false,
            loadingtext: "正在请求HIS取消住院登记，请稍后…",
            success: function () {
                $.najax({
                    type: "GET",
                    url: "/OutpatientManage/OutpatCharge/GetCQjzdjDataInfo?zymzh=" + $('#zyh').val(),
                    dataType: "json",
                    async: false,
                    loadingtext: "正在请求HIS取消住院登记，请稍后…",
                    success: function (ajaxresp) {
                        if (ajaxresp && (ajaxresp.jylsh || cqPatInfo.ybVer == "shanghaiV5")) {
                            if (cqPatInfo.ybVer == "shanghaiV5") {
                                var jzlx = $('#readCardCardType').val();
                                shanghaiV5djcx(jzlx, $('#zyh').val());//上海五期医保入院登记撤销
                            } else {
                                var payoptype = { hisId: $('#zyh').val(), 'mdtrt_id': ajaxresp.jylsh, 'operatorId': curUserCode, "operatorName": curUserName, 'insuplc_admdvs': $("#xzqh").val(), 'psn_no': $("#rybh").val() };

                                $.ajax({
                                    type: "POST",
                                    url: "http://127.0.0.1:33333/api/YiBao/HospitaUpMdtrtinfo_2404",
                                    dataType: "json",
                                    data: payoptype,
                                    async: false,
                                    success: function (data) {
                                        var cqybjyDenySettleReturn = eval('(' + data + ')');
                                        if (cqybjyDenySettleReturn.infcode == "0") {
                                        } else {
                                            $.modalAlert('取消住院登记失败：' + cqybjyDenySettleReturn.err_msg, 'error');
                                            IsSuccess = false;
                                        }
                                    }
                                });
                            }
                        }
                    }
                });
            }
        });


    }
    if (IsSuccess) {
        $.najax({
            url: "/PatientManage/HospiterRes/CancelAdmission?zyh=" + $("#zyh").val(),
            dataType: 'json',
            async: false,
            success: function (data) {
                if (data.message == "取消入院成功！") {
                    if (openYbSett && medicalInsurance == "guian" && $("#brxzmc").attr("data-brxzlb") === "1") {
                        var ybReq = {
                            astr_jylsh: jylsh,  //医保返回信息
                            astr_jyyzm: jyyzm  //医保返回信息
                        }
                        var ybjySettReturn;// = $.guianyibao.YibaoConfirm(ybReq);//贵安医保确认提交
                        $.ajax({
                            url: "http://127.0.0.1:12345/api/YiBao/YibaoConfirm",
                            data: ybReq,
                            dataType: "json",
                            async: false,
                            success: function (data) {
                                ybjySettReturn = eval('(' + data + ')');
                            }
                        });
                        if (!(ybjySettReturn.Code == 0)) {
                            $.modalAlert("医保取消入院登记，流水号【" + jylsh + "】提交失败，请前往“不确定交易”处理", 'error');
                        }
                    }
                    $.modalAlert("住院号为:" + $("#zyh").val() + "，取消成功", 'warning');
                    AbledSysBasicInfo();
                    newtouch_globalevent_f4(event);
                }
                $.modalAlert(data.message, "success");
            }
        });
    }


}

function DisableSysBasicInfo() {
    $("#csny").attr("disabled", "disabled");
    $('input[name="xb"]').each(
        function () { $(this).parent().attr("disabled", "disabled"); });
    $("#blh").attr("disabled", "disabled");
    $("#phone").attr("disabled", "disabled");
    //$("#dy").attr("disabled", "disabled");
    $("#hf").attr("disabled", "disabled");
}

function AbledSysBasicInfo() {

    $("#mzmc").removeAttr("data-label");
    $("#gjmc").removeAttr("data-label");
    $("#jkjlmc").removeAttr("data-label");
    $("#xm").html("");
    $("#zjh").html("");
    $("#xb").html("");
    $("#nlshow").html("");
    $("#blh").html("");
    $("#phone").html("");
    $("#cblb").val("");
    //$("#dy").html("");
    $("#zyh").removeAttr("disabled").removeAttr("style");
    $('#zyh').attr("placeholder", "");
    patInfoObj = {};
}

function ClearAll() {

    AbledSysBasicInfo();
    newtouch_globalevent_f4();
}

function newtouch_globalevent_f6() {
    PrintWDInfo();
}

function newtouch_globalevent_f7() {
    PrinyZYInfo();
}

function newtouch_globalevent_f8() {
    btn_Save();
}

function newtouch_globalevent_f9() {
    var yjj = 0;
    $.najax({
        url: "/SystemManage/InpatientAccountManage/GetZyAccount?zyh=" + $("#zyh").val() + "&zhxz=3",
        cache: false,
        async: false,
        alertbierror: false,
        success: function (zhyeData) {
            if (zhyeData && zhyeData.data) {
                yjj = zhyeData.data.zhye;
            }
        }
    });
    if (yjj > 0) {
        $.modalAlert("预交金为：" + yjj + "，请先退预交金在取消住院", 'error');
        return;
    } else {
        Cancel();
    }
   
}

function newtouch_event_f4() {
    AbledSysBasicInfo();
    $("#patid").val("");
}

function checkNotNull() {
    var patid = $("#patid").val();
    if (!patid) {
        $.modalAlert("病人基本信息不存在，无法保存", 'warning');
        return false;
    }
    if ($("#rytj").val() == "9921") {
        if ($('#syrq').val().length == 0) {
            $.modalAlert("新生儿随母入院请输入生育日期", 'warning');
            return false;
        }
        if ($('#syfwzh').val().length == 0) {
            $.modalAlert("新生儿随母入院请输入生育服务证号", 'warning');
            return false;
        }
    }
    var validator = $('#form1').validate();
    validator.settings = {
        rules: {
            doctormc: { required: true },
            zdmc1: { required: true },
            ryrq: { required: true },
            ksmc: { required: true },
            bqmc: { required: true },
            kh: { required: true },
            zyh: { required: true }
        },
        messages: {
            doctormc: { required: "医生必须填写" },
            zdmc1: { required: "入院诊断一必须填写" },
            ryrq: { required: "入院时间必须选择" },
            ksmc: { required: "科室必须填写" },
            bqmc: { required: "病区必须填写" },
            kh: { required: (boolsearchwithkh ? "卡号" : "病历号") + "必须填写" },
            zyh: { required: "住院号必须填写" }
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

    if ($.getDate({ date: $('#ryrq').val() }) > $.getDate()) {
        $.modalAlert('入院时间不能大于当天', 'warning');
        return false;
    }
    return true;
}

//人员查询
function GetPatSerarchView(blh) {
    $.modalOpen({
        id: "patSearch",
        title: "患者查询",
        url: "/OutpatientManage/OutpatientAccounting/SysPatEntitiesblhView?t=" + Math.random() + "&blh=" + blh,
        width: "700px",
        height: "600px",
        callBack: function (iframeId) {
            top.frames[iframeId].PatDbGrid(); //在弹出窗口事件
        }
    });
}

//按病历号检索病人基本信息
function CallbackPatientQuery(obj) {
    GetQueryFphAjax(obj);
}



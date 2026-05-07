//频次 通用 浮层选择器
$.fn.pcFloatingSelector = function (options) {
	//默认options
	var defaults = {
		width: 280,
		height: 280,
		//minlength: 2,
		focusautotrigger: true,
		className: "ui-pc-container",
		caption: "选择频次",
		url: '/SystemManage/BaseData/GetOrderFrequencyList',
		ajaxparameters: function ($thisinput) {
			return "";
		}
	};
	var options = $.extend(defaults, options);

	$(this).frequencyNewtouchFloatingSelector(options);
}

//药品用法 通用 浮层选择器
$.fn.yfFloatingSelector = function (options) {
	//默认options
	var defaults = {
		width: 250,
		height: 200,
		//minlength: 2,
		focusautotrigger: true,
		caption: "选择用法",
		url: '/SystemManage/SysBaseData/GetMedicineUsageList',
		ajaxparameters: function ($thisinput) {
			return "keyword=" + $.trim($thisinput.val());
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: '代码', name: 'yfCode', widthratio: 60 },
			{ label: '名称', name: 'yfmc', widthratio: 35 }
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//收费项目 浮层（包括药品、项目，也可以非治疗项目）
$.fn.sfxmFloatingSelector = function (options) {
	//默认options
	var defaults = {
	    width: 1550,
		height: 380,
		clickautotrigger: true,
		url: '/SystemManage/BaseData/SelectSfxmYp',
		ajaxparameters: null,   //请指定
		itemdbclickhandler: null,   //请指定
		djDecimalPlaces: 2,  //单价精度 默认2
		searchType: null,   //仅控制列显隐，不参与筛选//检索目标 "yp,sfxm"、"yp"、"sfxm"、"yp.kc"、"sfxm.dwjls"、"yp,sfxm.dwjls"、"yp.kc,sfxm"、"yp.kc,sfxm.dwjls"
		showColModel: null,   //可指定 [{name:;widthratio:;},{name:;widthratio:;}] widthratio可选
		ypkccbjc: true,    //药品库存初步检查   是否被控制 是否库存0

		colModel: [
			{ label: '代码', name: 'sfxmCode', widthratio: 6 },
			{ label: '名称', name: 'sfxmmc', widthratio: 6 },
			{
				label: '规格', name: 'gg', widthratio: 6
				, hidden: true
			},
			{ label: '医嘱类型', name: 'yzlx', hidden: true },
			{ label: '首拼', name: 'py', widthratio: 5, hidden: true },
			{ label: '收费大类', name: 'sfdlCode', hidden: true },
			{
				label: '收费大类', name: 'sfdlmc'
				, widthratio: 6
				, hidden: true
			},
			{ label: '单位', name: 'dw', widthratio: 3 },
			{
				label: '单价', name: 'dj', widthratio: 6, formatter: function (cellvalue) {
					return (!!cellvalue || cellvalue == 0) ? (parseFloat(cellvalue).toFixed(options.djDecimalPlaces ? options.djDecimalPlaces : 2)) : '';
				}
			},
			{ label: '计价策略', name: 'jjcl', hidden: true },
			{
				label: '计量数', name: 'dwjls', widthratio: 8, formatter: function (cellvalue) {
					return !!cellvalue ? cellvalue : '';
				}
				, hidden: true
			},
			//{ label: '报销政策', name: 'zfxz', hidden: true },
			//{
			//    label: '报销政策', name: 'zfxzmc', widthratio: 8, formatter: function (cellvalue, a, b) {
			//        return $.enum.getDescByValue("EnumZiFuXingZhi", b.zfxz);
			//    }
			//},
			{ label: '报销政策', name: 'zfxz', hidden: true },
			{
				label: '报销政策', name: 'zfxzmc', widthratio: 5, formatter: function (cellvalue, a, b) {
					return $.enum.getDescByValue("EnumZiFuXingZhi", b.zfxz);
				}
			},
			{ label: '自负比例', name: 'zfbl', widthratio: 3,hidden: true },
			{ label: '特殊药品标志', name: 'tsypbz', hidden: true },
			{
				label: '特殊药品标志', name: 'tsypbzmc', hidden: true, formatter: function (cellvalue, a, b) {
					return $.enum.getDescByValue("EnumYpsx", b.tsypbz);
				}
			},
			{ label: 'ypjxCode', name: 'ypjxCode', hidden: true },
			{ label: 'duration', name: 'duration', hidden: true },
			{ label: 'jldw', name: 'jldw', hidden: true },
			{ label: 'jldwzhxs', name: 'jldwzhxs', hidden: true },
			{ label: 'cls', name: 'cls', hidden: true },
			{ label: 'zyqzlx', name: 'zyqzlx', hidden: true },
			{ label: 'zxks', name: 'zxks', hidden: true },
			{ label: 'zxksmc', name: 'zxksmc', hidden: true },
			{ label: 'ybdm', name: 'ybdm', hidden: true },
			{ label: 'xzyy', name: 'xzyy', hidden: true },
			{ label: 'xzyysm', name: 'xzyysm', hidden: true },
			{ label: 'mrjl', name: 'mrjl', hidden: true },
			{ label: 'mrpc', name: 'mrpc', hidden: true },
			{ label: 'mrpcmc', name: 'mrpcmc', hidden: true },
			{ label: 'mryf', name: 'mryf', hidden: true },
			{ label: 'mryfmc', name: 'mryfmc', hidden: true },
			{ label: 'yfbmCode', name: 'yfbmCode', hidden: true },
			{ label: 'jlfwBegin', name: 'jlfwBegin', hidden: true },
			{ label: 'jlfwEnd', name: 'jlfwEnd', hidden: true },
			{ label: 'pcfwBegin', name: 'pcfwBegin', hidden: true },
			{ label: 'pcfwEnd', name: 'pcfwEnd', hidden: true },
			{ label: 'kssKy', name: 'kssKy', hidden: true },
			{ label: 'kssqxjb', name: 'kssqxjb', hidden: true },
			{
				label: 'kssmc', name: 'kssmc', hidden: true, formatter: function (cellvalue, a, b) {
					return $.enum.getDescByValue("EnumKss", b.kssqxjb);
				} },
			{
			    label: '医保限价', name: 'cxjje', formatter: function (cellvalue, a, b) {
					if (b.cxjje == null) {
						return "0.00"
					}
					else return b.cxjje
				}, widthratio: 4, hidden: true
			},
			{
				label: '药房', name: 'yfbmmc', formatter: function (cellvalue, a, b) {
					return b.yzlx === '1' && !!cellvalue ? cellvalue : '';
				}, widthratio: 4, hidden: true
			},
			{
				label: '库存', name: 'kcsl', formatter: function (cellvalue, a, b) {
					return b.yzlx === '1'&&!!cellvalue ? cellvalue : '';
				}, widthratio: 4, hidden: true
			},
			{
				label: '库存', name: 'clkcsl', formatter: function (cellvalue, a, b) {
					//给的kcsl是最小单位的，这里要显示门诊/住院单位的数量
					return b.yzlx === '1' ? (!!b.kcsl && !!b.cls ? (parseInt(b.kcsl / b.cls)) : '无') : '';
				}, widthratio: 4, hidden: true
			},
			{
				label: '抗生素', name: 'isKss', formatter: function (cellvalue, a, b) {
					return cellvalue == '1' ? '是' : '否';
				}, widthratio: 3, hidden: true
			},
			{
				label: '单次剂量', name: 'jlfw', formatter: function (cellvalue, a, b) {
					if (b.jlfwBegin || b.jlfwEnd) {
						return b.jlfwBegin + "-" + b.jlfwEnd;
					}
					else return ''
				}, widthratio: 7, hidden: true
			},
			{
				label: '频次范围', name: 'pcfw', formatter: function (cellvalue, a, b) {
					if (b.pcfwBegin || b.pcfwEnd) {
						return b.pcfwBegin + "-" + b.pcfwEnd;
					}
					else return ''
				}, widthratio: 7, hidden: true
			},
			{
				label: '控制', name: 'kzbz', formatter: function (cellvalue, a, b) {
					return cellvalue == '1' ? '控制' : '';
				}, widthratio: 3, hidden: true
			},
            {
                label: '生产厂家', name: 'sccj'/*, hidden: true*/, formatter: function (cellvalue, a, b) {
                    return cellvalue == undefined ? '' : cellvalue;
                }, widthratio: 8
            },
            { label: '国家医保代码', name: 'gjybdm', widthratio: 8 },
			{ label: '备注', name: 'bz' }
		]
	};
	var options = $.extend(defaults, options);

	if (!!!options.searchType) {

		options.searchType = "yp,sfxm"; //默认
	}
	if (options.searchType && !!!options.showColModel) {
	    var colsfxmmc = null;
	    var colsfxmCode = null;
		var colgg = null;
		var colsfdlmc = null;
		var coldwjls = null;
		var colyfbmmc = null;
		var colclkcsl = null;
		var colkzbz = null;
		var colisKss = null;
		var coljlfw = null;
		var colpcfw = null;
		var colzfbl = null;
		var colbz = null;
		var colcxjje = null;
		var coltsypbz = null;
		var colsccj = null;
		var colgjybdm = null;
		$.each(options.colModel, function () {
			if (this.name === 'bz') {
				colbz = this;
			}
			if (this.name === 'zfbl') {
				colzfbl = this;
			}
			if (this.name === 'sfxmmc') {
				colsfxmmc = this;
			}
			if (this.name === 'zfxzmc') {
			    colzfxzmc = this;
			}
			if (this.name === 'dw') {
			    coldw = this;
			}
			if (this.name === 'sfxmCode') {
			    colsfxmCode=this;
			}
			else if (this.name === 'gg') {
				colgg = this;
			}
			else if (this.name === 'sfdlmc') {
				colsfdlmc = this;
			}
			else if (this.name === 'dwjls') {
				coldwjls = this;
			}
			else if (this.name === 'yfbmmc') {
				colyfbmmc = this;
			}
			else if (this.name === 'clkcsl') {
				colclkcsl = this;
			}
			else if (this.name === 'kzbz') {
				colkzbz = this;
			}
			else if (this.name === 'isKss') {
				colisKss = this;
			}
			else if (this.name === 'jlfw') {
				coljlfw = this;
			}
			else if (this.name === 'pcfw') {
				colpcfw = this;
			}
			else if (this.name == 'cxjje') {
				colcxjje = this;
			}
			else if (this.name === 'sccj') {
			    colsccj = this;
			}
			else if (this.name === 'gjybdm') {
			    colgjybdm = this;
			}
			if (this.name === 'tsypbzmc') {
				coltsypbz = this;
			}
		});
		if (!!colsfxmmc && !!colgg && !!colsfdlmc && !!coldwjls && !!colyfbmmc && !!colclkcsl && !!colkzbz) {
			//已被占用50宽
			switch (options.searchType) {
				case "yp": 
					colsfxmmc.widthratio = 10;
					colgg.hidden = undefined;
					colgg.widthratio = 8;
					colsfdlmc.hidden = undefined;
					colsfdlmc.widthratio = 10;
					colisKss.hidden = undefined;
					colisKss.widthratio = 6;
					colzfbl.hidden = undefined;
					colzfbl.widthratio = 10;
					colbz.hidden = undefined;
					colbz.widthratio = 5;
					colsccj.hidden = undefined;
					colsccj.widthratio = 10;
					colgjybdm.hidden = undefined;
					colgjybdm.widthratio = 5;
					//coljlfw.hidden = undefined;
					//coljlfw.widthratio = 10;
					//colpcfw.hidden = undefined;
					//colpcfw.widthratio = 10;
					break;
				case "sfxm": 
					colgg.hidden = undefined;
					colgg.widthratio = 10;
					colsfxmmc.widthratio = 20;
					colsfdlmc.hidden = undefined;
					colsfdlmc.widthratio = 12;
					colcxjje.hidden = undefined;
					colcxjje.widthratio = 10;
					//colclkcsl.hidden = undefined;
					//colclkcsl.widthratio = 10;
					break;
				case "yp,sfxm":
			    case "sfxm,yp":
			        colsfxmCode.widthratio = 9;
			        colsfxmmc.widthratio = 20;
			        coldw.widthratio = 8;
			        colgjybdm.hidden = true;
                    colzfxzmc.widthratio = 8;
					colgg.hidden = undefined;
					colgg.widthratio = 12;
					//colisKss.hidden = undefined;
					//colisKss.widthratio = 6;
					colzfbl.hidden = undefined;
					colzfbl.widthratio = 10;
					colbz.hidden = undefined;
					colbz.widthratio = 19;
					//coljlfw.hidden = undefined;
					//coljlfw.widthratio = 10;
					//colpcfw.hidden = undefined;
					//colpcfw.widthratio = 10;
					break;
			    case "yp.kc":
			    case "yyhc":
				case "yp.kc,sfxm":
				case "sfxm,yp.kc": //cxjje 
					coltsypbz.hidden = undefined;
					coltsypbz.widthratio = 8;
					colsfxmmc.widthratio = 15;
					colgg.hidden = undefined;
					colgg.widthratio = 8;
					colyfbmmc.hidden = undefined;
					colyfbmmc.widthratio = 5;
					colclkcsl.hidden = undefined;
					colclkcsl.widthratio = 4;
					colkzbz.hidden = undefined;
					colkzbz.widthratio = 4;
					colisKss.hidden = undefined;
					colisKss.widthratio = 4;
					colzfbl.hidden = undefined;
					colzfbl.widthratio = 5;
					colbz.hidden = undefined;
					colbz.widthratio = 5;
					colbz.cx = undefined;
					colbz.widthratio = 5;
					colcxjje.hidden = undefined;
					colcxjje.widthratio = 6;
					colsccj.widthratio = 8;
					colgjybdm.hidden = undefined;
					colgjybdm.widthratio = 8;
					//coljlfw.hidden = undefined;
					//coljlfw.widthratio = 10;
					//colpcfw.hidden = undefined;
					//colpcfw.widthratio = 10;
					break;
				case "sfxm.dwjls": 
					colgg.hidden = undefined;
					colgg.widthratio = 10;
					colsfxmmc.widthratio = 25;
					coldwjls.hidden = undefined;
					coldwjls.widthratio = 10;
					colcxjje.hidden = undefined;
					colcxjje.widthratio = 10;
					break;
				case "yp,sfxm.dwjls":
				case "sfxm.dwjls,yp": 
					colsfxmmc.widthratio = 12;
					colgg.hidden = undefined;
					colgg.widthratio = 10;
					coldwjls.hidden = undefined;
					coldwjls.widthratio = 8;
					colisKss.hidden = undefined;
					colisKss.widthratio = 7;
					colzfbl.hidden = undefined;
					colzfbl.widthratio = 10;
					colbz.hidden = undefined;
					colbz.widthratio = 5;
					//coljlfw.hidden = undefined;
					//coljlfw.widthratio = 10;
					//colpcfw.hidden = undefined;
					//colpcfw.widthratio = 10;
					break;
				case "yp.kc,sfxm.dwjls":
				case "sfxm.dwjls,yp.kc": 
					colyfbmmc.hidden = undefined;
					coltsypbz.widthratio = 9;
					colsfxmmc.widthratio = 12;
					colgg.hidden = undefined;
					colgg.widthratio = 8;
					colyfbmmc.hidden = undefined;
					colyfbmmc.widthratio = 7;
					colclkcsl.hidden = undefined;
					colclkcsl.widthratio = 5;
					colkzbz.hidden = undefined;
					colkzbz.widthratio = 5;
					coldwjls.hidden = undefined;
					coldwjls.widthratio = 6;
					colisKss.hidden = undefined;
					colisKss.widthratio = 6;
					colzfbl.hidden = undefined;
					colzfbl.widthratio = 10;
					colbz.hidden = undefined;
					colbz.widthratio = 5;
					//coljlfw.hidden = undefined;
					//coljlfw.widthratio = 10;
					//colpcfw.hidden = undefined;
					//colpcfw.widthratio = 10;
					break;
				default:
					break;
			}
		}
	}

	if (options.showColModel) {
		//[{name:;widthratio:;},{name:;widthratio:;}] widthratio可选
		$.each(options.colModel, function () {
			var thisCol = this;
			var matchedShow = $.jsonWhere(options.showColModel, function (v) {
				return v.name == thisCol.name;
			});
			if (matchedShow && matchedShow.length) {
				thisCol.hidden = undefined;
				if (matchedShow[0].widthratio) {
					thisCol.widthratio = matchedShow[0].widthratio;
				}
			}
			else {
				thisCol.hidden = true;
			}
		});
	}

	if (options.searchType.indexOf('yp.kc') >= 0 && options.ypkccbjc === true) {

		options.itemdbclickhandleBefore = function ($iiithis) {
			
			if ($iiithis.attr('data-yzlx') == '1') {
				if ((!isNaN($iiithis.attr('data-clkcsl')) && parseInt($iiithis.attr('data-clkcsl')) <= 0) || $iiithis.attr('data-clkcsl') == '无') {
					$.modalAlert("不能开立无库存药品", 'warning');
					return false;
				}
				if ($iiithis.attr('data-kzbz') == '控制') {
					$.modalAlert("不能开立控制药品", 'warning');
					return false;
				}
			}
			return;
		}
	}
	$(this).newtouchFloatingSelector(options);
}

//系统科室 浮层选择器
$.fn.ksFloatingSelector = function (options) {
	//默认options
	var defaults = {
		width: 300,
		height: 180,
		//minlength: 2,
		focusautotrigger: true,
		caption: "选择科室",
		url: '/SystemManage/SysBaseData/SelectDepartmentList',
		ajaxparameters: function ($thisinput) {
		    if (!!!options.iszlks) {
		        return "keyword=" + $.trim($thisinput.val());
		    } else {
		        return "keyword=" + $.trim($thisinput.val()) + "&zlks=" + options.iszlks;
		    }
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: '代码', name: 'Code', widthratio: 45 },
			{ label: '名称', name: 'Name', widthratio: 50 }
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}
//处方模板浮层
$.fn.cfzutaoFloatingSelector12 = function (options) {
	//默认options
	var defaults = {
		width: 350,
		height: 280,
		focusautotrigger: true,
		caption: "选择组套",
		url: '/PresTemplate/GetCfmbList',
		ajaxparameters: function ($thisinput) {
			return "mblx="+ options.mblx + "&cflx="+options.cflx+"&expandCflx=null&mbKeyword=" + $.trim($thisinput.val());
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: 'cfmbId', name: 'mbId', hidden: true },
			{ label: '模板名称', name: 'mbmc', widthratio: 30 },
			{
				label: '处方类型',
				name: 'cflx',
				widthratio: 20,
				formatter: function(value, options, row) {
					// 将数字值转换为汉字
					const prescriptionTypes = {
						1: "西药处方",
						2: "中药处方",
						3: "检验处方",
						4: "检查处方",
						5: "康复处方",
						6: "常规项目处方",
					};
					return prescriptionTypes[value] || "未知类型"; // 默认值为 "未知类型"
				}
			},
			{ label: '描述', name: 'Description', widthratio: 30, hidden: true},
			{ label: '注意事项', name: 'Remark', widthratio: 30 ,hidden: true}
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//组套 浮层选择器
$.fn.zutaoFloatingSelector = function (options) {
	//默认options
	var defaults = {
		width: 350,
		height: 280,
		focusautotrigger: true,
		caption: "选择组套",
		url: '/TemplateManage/GroupPackage/SelectGroupPackageList',
		ajaxparameters: function ($thisinput) {
			return "type=" + options.type + "&keyword=" + $.trim($thisinput.val());
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: 'ztId', name: 'ztId', hidden: true },
			{ label: '名称', name: 'ztmc', widthratio: 30 },
			{
				label: '类型', name: 'Type', widthratio: 20, hidden: true
			},
			{ label: '描述', name: 'Description', widthratio: 30 },
			{ label: '注意事项', name: 'Remark', widthratio: 30 }
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//系统部位 浮层选择器
$.fn.bwFloatingSelector = function (options, data) {
	//默认options
	var defaults = {
		width: 300,
		height: 250,
		focusautotrigger: true,
		isinputchangetriggered: false,
		mutiselect: true,
        bwlx:"pt",
		caption: "选择部位",
		url: '/SystemManage/BodyParts/GetSysBodyParts',
		ajaxparameters: function ($thisinput) {
			return "";
		}
	};
	var options = $.extend(defaults, options);
	$(this).multiquencyNewtouchFloatingSelector(options, data);
}

//系统部位 浮层选择器
$.fn.yxbwFloatingSelector = function (options, data) {
	//默认options
	var defaults = {
		width: 300,
		height: 200,
		focusautotrigger: true,
		isinputchangetriggered: false,
		bwlx: "jc",
		mutiselect: true,
		caption: "选择部位",
		url: '/SystemManage/BodyParts/GetYxBodyParts',
		ajaxparameters: function ($thisinput) {
			return "";
		}
	};
	var options = $.extend(defaults, options);
	$(this).multiquencyNewtouchFloatingSelector(options, data);
}


//系统嘱托 浮层选择器
$.fn.ztFloatingSelector = function (options) {
	//默认options
	var defaults = {
		width: 300,
		height: 280,
		focusautotrigger: true,
		caption: "选择嘱托",
		url: '/SystemManage/DoctorRemark/GetSysDoctorRemark',
		ajaxparameters: function ($thisinput) {
			return "keyword=" + $.trim($thisinput.val());
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: '代码', name: 'ztCode', widthratio: 45 },
			{ label: '名称', name: 'ztmc', widthratio: 50 }
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//医生站--》文字录入--》指示文本框按钮
//$.fn.zsFloatingSelector = function (options) {
//    var defaults = {
//        width: 400,
//        height: 400,
//        focusautotrigger: true,
//        caption: "手术内容",
//        url: '/SystemManage/DoctorRemark/surgery_record',
//        ajaxparameters: function ($thisinput) {
//            return "keyword=" + $.trim($thisinput.val());
//        },
//        itemdbclickhandler: null,
//        colModel: [
//            { label: '手术名称', name: 'ssmc', widthratio: 33 },
//            { label: '手术时间', name: 'sssj', widthratio: 33 },
//            { label: '主刀医生', name: 'Name', widthratio: 33 }
//        ]
//    };
//    var options = $.extend(defaults, options);

//    $(this).newtouchFloatingSelector(options);
//}



//诊断
$.fn.zdFloatingSelector = function (options) {
	var defaults = {
		url: '/SystemManage/SysBaseData/GetDiagnosisList',
		width: 600,
		height: 200,
		clickautotrigger: true,
		caption: "选择诊断",
		ajaxparameters: function ($thisinput) {
			if (!!!options.ybnhlx) {
				return "keyword=" + $thisinput.val() + "&zdlx=" + options.zdlx;
			} else {
				return "keyword=" + $thisinput.val() + "&zdlx=" + options.zdlx + "&ybnhlx=" + options.ybnhlx;
			}

		},
		itemdbclickhandler: null,
		colModel: [
			{ label: '代码', name: 'zdCode', hidden: true},
			{ label: '名称', name: 'zdmc', widthratio: 60 },
			{ label: '拼音', name: 'py', widthratio: 20 },
			{ label: 'icd10', name: 'icd10', widthratio: 20 },
			{ label: 'icd10附加码', name: 'icd10fjm', hidden: true }
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//症候
$.fn.zyzhFloatingSelector = function (options) {
	var defaults = {
		url: '/SystemManage/SysBaseData/GetTCMSymptomsList',
		width: 400,
		height: 200,
		clickautotrigger: true,
		caption: "选择症候",
		ajaxparameters: function ($thisinput) {
			return "keyword=" + $thisinput.val();
		},
		itemdbclickhandler: null,
		colModel: [
			{ label: '代码', name: 'zhCode', hidden: true },
			{ label: '名称', name: 'zhmc', widthratio: 80 },
			{ label: '拼音', name: 'py', widthratio: 20 },
		]
	};
	var options = $.extend(defaults, options);

	$(this).newtouchFloatingSelector(options);
}

//岗位人员选择器
$.fn.dutyStaffFloatingSelector = function (options) {
	$(this).newtouchFloatingSelector({
		url: '/SystemManage/SysStaff/GetStaffListByDutyCode',
		width: 300,
		height: 200,
		clickautotrigger: true,
		ajaxparameters: function ($thisinput) {
			return "dutyCode=" + options.dutyCode + "&keyword=" + $thisinput.val();
		},
		colModel: [
			{ label: 'StaffGh', name: 'StaffGh', hidden: true },
			{ label: 'ks', name: 'ks', hidden: true },
			{ label: '医生名称', name: 'StaffName', widthratio: 48 },
			{ label: '科室名称', name: 'ksmc', widthratio: 50 },
		],
		itemdbclickhandler: function ($thistr, $thisinput) {
			//保存时验证val和data-Name一致
			$thisinput.attr('data-StaffGh', $thistr.attr('data-StaffGh'));
			$thisinput.attr('data-ks', $thistr.attr('data-ks'));
			$thisinput.attr('data-ksmc', $thistr.attr('data-ksmc'));
			$thisinput.val($thistr.attr('data-StaffName'));
		}
	});
}

//普通膳食指示
$.fn.yszsFloatingSelector = function (options, data) {
	//默认options
	var defaults = {
		width: 300,
		height: 280,
		focusautotrigger: true,
		isinputchangetriggered: false,
		mutiselect: true,
		caption: "选择指示",
		url: '/NurseManage/DietaryAdvice/GetyszsList',
		ajaxparameters: function ($thisinput) {
			return "keyword=" + options.searchdl;
		}
	};
	var options = $.extend(defaults, options);
	$(this).multiFoodFloatingSelector(options, data);
}

//医嘱类别（长临） 浮层选择器
$.fn.yzlbFloatingSelector = function (options, data) {
	//默认options
	var defaults = {
		width: 100,
		height: 40,
		focusautotrigger: true,
		isinputchangetriggered: false,
		mutiselect: true,
		caption: "选择医嘱类别",
		url: '/SystemManage/SysBaseData/GetYzlb',
		ajaxparameters: function ($thisinput) {
			return "";
		}
	};
	var options = $.extend(defaults, options);
	$(this).frequencyNewtouchFloatingSelector(options, data);
}


//系统诊室 浮层选择器
$.fn.zsFloatingSelector = function (options) {
    //默认options
    var defaults = {
        width: 300,
        height: 180,
        //minlength: 2,
        focusautotrigger: true,
        caption: "选择诊室",
        url: '/SystemManage/SysBaseData/SelectConsultList',
        ajaxparameters: function ($thisinput) {
            return "ksCode=" + options.ksCode + "&keyword=" + $.trim($thisinput.val());
        },
        itemdbclickhandler: null,
        colModel: [
            { label: '代码', name: 'Code', widthratio: 45 },
            { label: '名称', name: 'Name', widthratio: 50 }
        ]
    };
    var options = $.extend(defaults, options);

    $(this).newtouchFloatingSelector(options);
}

$(function () {
    gridList();
})
function gridList() {
    var $gridList = $("#gridList");
    $gridList.dataGrid({
        treeGrid: true,
        treeGridModel: "adjacency",
        url: "/SystemManage/SysForCashPay/GetfsGridJson",
        postData: { keyword: $("#txt_keyword").val() },
        height: $(window).height() - 96,
        colModel: [
            { label: "主键", name: "xjzffsbh", hidden: true, key: true },
            { label: '代码', name: 'xjzffs', width: 100, align: 'left' },
            { label: '现金支付方式', name: 'xjzffsmc', width: 220, align: 'left' },
            {
                label: '有效', name: 'zt', width: 50, align: 'left',
                formatter: function (cellvalue) {
                    if (cellvalue == 0) {
                        return "无效"
                    } else {
                        return "有效"
                    }
                }
            },
            { label: '创建人', name: 'CreatorCode', width: 100, align: 'left' },
            {
                label: '创建日期', name: 'CreateTime', width: 100, align: 'left',
                formatter: "date", formatoptions: { srcformat: 'Y-m-d', newformat: 'Y-m-d' }
            }
        ]
    });
    $("#btn_search").click(function () {
        $gridList.jqGrid('setGridParam', {
            postData: { keyword: $("#txt_keyword").val() },
        }).trigger('reloadGrid');
    });
}
function btn_add() {
    $.modalOpen({
        id: "Form",
        title: "系统收费项目特殊标志维护",
        url: "/SystemManage/SysForCashPay/Form",
        width: "500px",
        height: "300px",
        callBack: function (iframeId) {
            top.frames[iframeId].submitForm();
        }
    });
}
function btn_edit() {
    var keyValue = $("#gridList").jqGridRowValue().xjzffsbh;
    $.modalOpen({
        id: "Form",
        title: "修改信息",
        url: "/SystemManage/SysForCashPay/Form?keyValue=" + keyValue,
        width: "500px",
        height: "300px",
        callBack: function (iframeId) {
            top.frames[iframeId].submitForm();
        }
    });
}
function btn_delete() {
    $.deleteForm({
        url: "/SystemManage/SysForCashPay/DeleteForm",
        param: { keyValue: $("#gridList").jqGridRowValue().xjzffsbh },
        success: function () {
            $.currentWindow().$("#gridList").resetSelection();
            $.currentWindow().$("#gridList").trigger("reloadGrid");
        }
    })
}
function btn_details() {
    var keyValue = $("#gridList").jqGridRowValue().id;
    $.modalOpen({
        id: "Details",
        title: "查看机构",
        url: "/SystemManage/SysForCashPay/Form?keyValue=" + keyValue,
        width: "500px",
        height: "300px",
        btn: null,
    });
}
function submitForm() {
    if (!$('#form1').formValid()) {
        return false;
    }
    var keyValue = $("#gridList").jqGridRowValue().xjzffsbh;
    $.submitForm({
        url: "/SystemManage/SysForCashPay/SubmitForm?keyValue=" + keyValue,
        param: $("#form1").formSerialize(),
        success: function () {
            $.currentWindow().$("#gridList").resetSelection();
            $.currentWindow().$("#gridList").trigger("reloadGrid");
            $.loading(false);
        }
    })
}
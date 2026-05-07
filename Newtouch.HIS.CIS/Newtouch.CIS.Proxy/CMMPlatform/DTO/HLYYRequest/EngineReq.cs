using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace Newtouch.CIS.Proxy.CMMPlatform.DTO.HLYYRequest
{
    [XmlRoot("root")]
    public class EngineReq
    {
        public bool ShouldSerializeMyProperty() => true;
        public string Type { get; set; } // 处方类型
        public PatientRecord Patient { get; set; } // 病人信息
        public Operation Operation { get; set; } // 手术信息
        public List<Prescription> Prescriptions { get; set; } // 药品处方列表

    }
     public class PatientRecord
    {
        [XmlElement("departID")]
        public string DepartID { get; set; } // 科室编号
        
        [XmlElement("department")]
        public string Department { get; set; } // 科室名称
        
        [XmlElement("bedNo")]
        public string BedNo { get; set; } // 住院病人床号
        
        [XmlElement("presType")]
        public string PresType { get; set; } // 处方类型
        
        [XmlElement("presSource")]
        public string PresSource { get; set; } // 来源
        
        [XmlElement("presDateTime")]
        public string PresDatetime { get; set; } // 处方/医嘱开具时间
        
        [XmlElement("payType")]
        public string PayType { get; set; } // 费别
        
        [XmlElement("patientNo")]
        public string PatientNo { get; set; } // 就诊卡号
        
        [XmlElement("presNo")]
        public string PresNo { get; set; } // 处方/医嘱号
        
        [XmlElement("name")]
        public string Name { get; set; } // 病人姓名
        
        [XmlElement("diagnoseid")]
        public string DiagnoseID { get; set; } // 诊断id
        
        [XmlElement("diagnose")]
        public string Diagnose { get; set; } // 诊断
        
        [XmlElement("IDCard")]
        public string IDCard { get; set; } // 身份证号码
        
        [XmlElement("address")]
        public string Address { get; set; } // 病人地址
        
        [XmlElement("phoneNo")]
        public string PhoneNo { get; set; } // 联系电话
        
        [XmlElement("age")]
        public string Age { get; set; } // 年龄
        
        [XmlElement("sex")]
        public string Sex { get; set; } // 性别
        
        [XmlElement("height")]
        public string Height { get; set; } // 身高
        
        [XmlElement("weight")]
        public string Weight { get; set; } // 体重
        
        [XmlElement("birthWeight")]
        public string BirthWeight { get; set; } // 出生时体重
        
        [XmlElement("previousHistory")]
        public string PreviousHistory { get; set; } // 既往史
        
        [XmlElement("nowMedicalHistory")]
        public string NowMedicalHistory { get; set; } // 现病史
        
        [XmlElement("ccr")]
        public string Ccr { get; set; } // 内生肌酐清除率
        
        [XmlElement("anaphylactogen")]
        public string Anaphylactogen { get; set; } // 过敏史
        
        [XmlElement("allergicHistory")]
        public string AllergicHistory { get; set; } // 过敏源
        
        [XmlElement("pregnancy")]
        public string Pregnancy { get; set; } // 是否怀孕
        
        [XmlElement("timeOfPreg")]
        public string TimeOfPreg { get; set; } // 孕期
        
        [XmlElement("disease")]
        public string Disease { get; set; } // 是否慢性病
        
        [XmlElement("breastFeeding")]
        public string BreastFeeding { get; set; } // 是否哺乳
        
        [XmlElement("dialysis")]
        public string Dialysis { get; set; } // 是否透析
        
        [XmlElement("proxName")]
        public string ProxName { get; set; } // 代办人姓名
        
        [XmlElement("proxIDCard")]
        public string ProxIDCard { get; set; } // 代办人身份证号
        
        [XmlElement("docID")]
        public string DocID { get; set; } // 医生工号
        
        [XmlElement("docName")]
        public string DocName { get; set; } // 医生姓名
        
        [XmlElement("totalAmount")]
        public string TotalAmount { get; set; } // 处方金额
        // Add any additional fields as necessary
    }

    public class Operation
    {
        [XmlElement("operationCode")]
        public string OperationCode { get; set; } // 手术代码
        
        [XmlElement("operationName")]
        public string OperationName { get; set; } // 手术名称
        
        [XmlElement("operationStartTime")]
        public string OperationStartTime { get; set; } // 手术开始时间
        
        [XmlElement("operationEndTime")]
        public string OperationEndTime { get; set; } // 手术结束时间
        
        [XmlElement("incisionType")]
        public string IncisionType { get; set; } // 切口类型
        
        [XmlElement("incisionStatus")]
        public string IncisionStatus { get; set; } // 愈合类型
        
        [XmlElement("inplant")]
        public string Inplant { get; set; } // 是否有植入物
    }

    [XmlRoot("Prescription")]
    public class Prescription
    {
        [XmlElement("drug")]
        public string Drug { get; set; } // 药品ID
        
        [XmlElement("drugName")]
        public string DrugName { get; set; } // 药品名称
        
        [XmlElement("regName")]
        public string RegName { get; set; } // 商品名
        
        [XmlElement("specification")]
        public string Specification { get; set; } // 含量规格
        
        [XmlElement("package")]
        public string Package { get; set; } // 包装规格
        
        [XmlElement("quantity")]
        public float Quantity { get; set; } // 数量
        
        [XmlElement("packUnit")]
        public string PackUnit { get; set; } // 包装单位
        
        [XmlElement("unitPrice")]
        public string UnitPrice { get; set; } // 单价
        
        [XmlElement("amount")]
        public string Amount { get; set; } // 总价
        
        [XmlElement("groupNo")]
        public string GroupNo { get; set; } // 组号
        
        [XmlElement("firstUse")]
        public string FirstUse { get; set; } // 首剂使用
        
        [XmlElement("prepForm")]
        public string PrepForm { get; set; } // 剂型
        
        [XmlElement("adminRoute")]
        public string AdminRoute { get; set; } // 给药途径
        
        [XmlElement("adminArea")]
        public string AdminArea { get; set; } // 给药部位
        
        [XmlElement("adminFrequency")]
        public string AdminFrequency { get; set; } // 给药频率
        
        [XmlElement("adminDose")]
        public string AdminDose { get; set; } // 给药剂量
        
        [XmlElement("adminMethod")]
        public string AdminMethod { get; set; } // 给药时机
        
        [XmlElement("type")]
        public string Type { get; set; } // 类型
        
        [XmlElement("adminGoal")]
        public string AdminGoal { get; set; } // 给药目的
        
        [XmlElement("docID")]
        public string DocID { get; set; } // 医生工号
        
        [XmlElement("docName")]
        public string DocName { get; set; } // 医生姓名
        
        [XmlElement("docTitle")]
        public string DocTitle { get; set; } // 医生职称
        
        [XmlElement("departID")]
        public string DepartID { get; set; } // 科室ID
        
        [XmlElement("department")]
        public string Department { get; set; } // 科室名称
        
        [XmlElement("nurseName")]
        public string NurseName { get; set; } // 护士名称
        
        [XmlElement("startTime")]
        public string StartTime { get; set; } // 开始时间
        
        [XmlElement("endTime")]
        public string EndTime { get; set; } // 结束时间
        
        [XmlElement("specialPromote")]
        public string SpecialPromote { get; set; } // 煎煮方法
        
        [XmlElement("continueDays")]
        public string ContinueDays { get; set; } // 持续时间
    }
}

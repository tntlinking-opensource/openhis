using System;
using System.Collections.Generic;
using Newtouch.Domain.Entity;

namespace Newtouch.Domain.DTO.InputDto.Inpatient
{
    /// <summary>
    /// 保存医嘱时，view传到后台的对象
    /// </summary>
    public class DoctorServiceRequestDto
    {
        public string Id { get; set; }
        public string zyh { get; set; }

        public int? zh { get; set; }

        public string pcCode { get; set; }
        public string pcmc { get; set; }

        public int? zxcs { get; set; }

        public int? zxzq { get; set; }

        public int? zxzqdw { get; set; }

        public string zdm { get; set; }

        public decimal? idm { get; set; }

        public string xmdm { get; set; }

        public string xmmc { get; set; }

        public string dwwwwwww { get; set; }//单位
        public int zbbzzzzzzz { get; set; }//自备标志
        public int? iszzffffff { get; set; }//转自费标志
        public int? ispscsfffff { get; set; }//皮试测试标志
        public int? isjffffff { get; set; }//是否计费
        public string yztag { get; set; }//精麻医嘱

        public decimal? ypjl { get; set; }

        public string yfmc { get; set; }

        public string ypyfdm { get; set; }

        public int sl { get; set; }

        public int? dwlb { get; set; }

        public string ypgg { get; set; }

        public string ztnr { get; set; }

        public string yznr { get; set; }

        public string hzxm { get; set; }

        public string WardCode { get; set; }

        public string DeptCode { get; set; }

        public string ysgh { get; set; }

        public int yzlx { get; set; }

        public int? ts { get; set; }

        public string kssj { get; set; }
        public string zxsj { get; set; }//24小时执行的时间
        /// <summary>
        /// 剂量转换系数
        /// </summary>
        public decimal? jlzhxs { get; set; }
        /// <summary>
        /// 住院转换系数
        /// </summary>
        public decimal? zyzhxs { get; set; }
        public string bw { get; set; }
        public string zxksdm { get; set; }
        public string zydw { get; set; }

        //膳食医嘱
        public string nlmd { get; set; }//能量密度
        public string nlmddm { get; set; }//能量密度
        public string ssyzcfId { get; set; }

        public string yslb { get; set; }//膳食类别
        public string yslbdm { get; set; }//膳食类别val

        public string yszs { get; set; }//膳食指示
        public string yszsval { get; set; }//膳食Ids

        public string ssxhdm { get; set; }//膳食型号
        public string ssxhval { get; set; }//膳食型号
        public string ssxh { get; set; }//膳食型号
        public string ztmbId { get; set; }//组套所在模板id
        public string ztId { get; set; }//组套id
        public string ztmc { get; set; }//组套名称

        /// <summary>
        /// 抗生素原因
        /// </summary>
        public string kssReason { get; set; }

        public string sqlx { get; set; }

        public string bwff { get; set; }

        public string sqdh { get; set; }

        /// <summary>
        /// 医嘱类别  临-临时医嘱；长-长期医嘱
        /// </summary>
        public string yzlb { get; set; }

        /// <summary>
        /// 医嘱号，同一医嘱号内包含多个yzId
        /// </summary>
        public string yzh { get; set; }

        /// <summary>
        /// 滴速  滴/分钟
        /// </summary>
        public int? ds { get; set; }
        /// <summary>
        /// 临床印象
        /// </summary>
        public string lcyx { get; set; }
        /// <summary>
        /// 申请备注
        /// </summary>
        public string sqbz { get; set; }

        /// <summary>
        /// 代煎标志
        /// </summary>
        public int? djbzzzzzzz { get; set; }
        /// <summary>
        /// 出院带药标志
        /// </summary>
        public int? cydybzzzzzzz { get; set; }
        /// <summary>
        /// 医嘱诊断icd10
        /// </summary>
        public string yzzdid { get; set; }
        /// <summary>
        /// 医嘱诊断名称
        /// </summary>
        public string yzzdmc { get; set; }
        /// <summary>
        /// 医嘱诊断时间
        /// </summary>
        public string yzzdsj { get; set; }
        /// <summary>
        /// 是否项目组套
        /// </summary>
        public string sfxmzt { get; set; }
        /// <summary>
        /// 组套数量
        /// </summary>
        public int? ztsl { get; set; }
        /// <summary>
        /// 药品用法对应组套
        /// </summary>
        public string yfztbm { get; set; }
        public string yfztbs { get; set; }
        public string dcztbs { get; set; }
        /// <summary>
        /// 药品来源 1:科室库存 2药房
        /// </summary>
        public int? yply { get; set; }
        public int? Px { get; set; }
    }

    public class DoctorServiceUIRequestDto : DoctorServiceRequestDto
    {
        public string action { get; set; }
        public string redundant_jldw { get; set; }
        public string dw { get; set; }
        public string tcmc { get; set; }
        public string yfmcval { get; set; }
        public string jxCode { get; set; }
        public string qzfs { get; set; }
        public string zxksmc { get; set; }
        public int? kcsl { get; set; }
        public string kzbz { get; set; }
        public int? dwjls { get; set; }
        public string yzlb { get; set; }
        public int tcfw { get; set; }
        public decimal? dj { get; set; }
        public string zfxz { get; set; }
        public string sqdh { get; set; }
        public string ispscs { get; set; }
        public int? isjf { get; set; }
        public int? iszzf { get; set; }
        
    }
    public class HistoricalOrdersRequestDto : DoctorServiceRequestDto
    {
        public string action { get; set; }
        public string redundant_jldw { get; set; }
        public string dw { get; set; }
        public string yfmcval { get; set; }
        public string jxCode { get; set; }
        public string qzfs { get; set; }
        public string zxksmc { get; set; }
        public int? kcsl { get; set; }
        public string kzbz { get; set; }
        public int? dwjls { get; set; }
        public string yzlb { get; set; }
        public decimal? dj { get; set; }
        public string zfxz { get; set; }
        public string ispscs { get; set; }
        public int? isjf { get; set; }
        public int? zzfbz { get; set; }
        public string CreateTime { get; set; }
    }
    public class PatientDto
    {
        public string zyh { get; set; }
        public string xm { get; set; }
        public string xb { get; set; }
        public string ryrq { get; set; }
        public string cyrq { get; set; }
        public string ryzd { get; set; }
        public string cyzd { get; set; }
    }
    /// <summary>
    /// 住院医嘱管理，药品预备数据时
    /// </summary>
    public class MedicineInfoDto
    {
        public string dw { get; set; }
        public string jx { get; set; }
        public decimal? jlzhxs { get; set; }
        public decimal? zyzhxs { get; set; }
        public string qzfs { get; set; }
    }

    /// <summary>
    /// 住院患者一览 已出区病人的浮层筛选条件
    /// </summary>
    public class patientInfoDto
    {
        public string Id { get; set; }
        public string zyh { get; set; }
        public string blh { get; set; }
        public string xm { get; set; }
        public string birth { get; set; }
        public string sex { get; set; }
        public int age { get; set; }
        public string ksdm { get; set; }
        public string ksmc { get; set; }
        public string zdmc { get; set; }
        public string ysgh { get; set; }
        public string ysxm { get; set; }
        public DateTime ryrq { get; set; }
        public string brxzmc { get; set; }
        public string bqmc { get; set; }
        public string cwmc { get; set; }
        public DateTime? rqrq { get; set; }
        public DateTime? cqrq { get; set; }
        public int? zybz { get; set; }
    }
    public class DoctorServiceparentRequestDto
    {
        public patientInfoDto patientInfo { get; set; }
        public List<DoctorServiceUIRequestDto> DoctorServiceUIRequestDto { get; set; }
        public string DrugStockInfo { get; set; }
    }

    public class SaveDoctorServiceDto
    {
        public List<InpatientSTATOrderEntity> lsyzList { get; set; }
        public List<InpatientLongTermOrderEntity> cqyzList { get; set; }
        public List<InpatientSTATOrderEntity> EditlsyzList { get; set; }
        public List<InpatientLongTermOrderEntity> EditcqyzList { get; set; }
        public List<InpatientDietDetailSplitEntity> ssyzList { get; set; }
        public List<InpatientDiagnosisEntity> zyyzzdList { get; set; }
        public List<string> DelIds { get; set; }
        public List<string> DelssyzIds { get; set; }
    }

    /// <summary>
    /// 判断医嘱是否重复
    /// </summary>
    public class DSrepeatRequestDto
    {
        public string Id { get; set; }
        //0 临时医嘱 1 长期医嘱
        public int clbz { get; set; }
        public DateTime kssj { get; set; }
        public string xmdm { get; set; }
    }

    public class SfxmztDto
    {
        public string ztId { get; set; }
        public string ztmc { get; set; }
        public string sfxm { get; set; }
        public string sfxmmc { get; set; }
        public string dw { get; set; }
        public decimal dj { get; set; }
        public int sl { get; set; }
        public string zxks { get; set; }
        public string zxksmc { get; set; }
        public string sfmb { get; set; }
    }
    public class yfsfxmdyDto
    {
        public string sfmb { get; set; }
        public string sfmbmc { get; set; }
    }

    public class ypyfdataDto
    {
        public string yfbmCode { get; set; }
        public string yfbmmc { get; set; }
        public string mzzybz { get; set; }
    }
    public class base_SfxmDto
    {
        public decimal? dj { get; set; }
        public int? dwjls { get; set; }
    }
}

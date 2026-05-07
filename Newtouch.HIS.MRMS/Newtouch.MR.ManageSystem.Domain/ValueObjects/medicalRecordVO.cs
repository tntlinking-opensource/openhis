using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.MR.ManageSystem.Domain.ValueObjects
{
    public class medicalRecordVO
    {
        public string ID { get; set; }
        public string bllx { get; set; }

        public string blxtml { get; set; }

        public string mbbh { get; set; }
        public string zyh { get; set; }

        public string blxtmc_yj { get; set; }
        public string blxtmc_hj { get; set; }
        public string ksdm { get; set; }
        public string ksmc { get; set; }

        public string OrganizeId { get; set; }
        public string CreatorCode { get; set; }
        public string LastModifierCode { get; set; }
        public int? IsLock { get; set; }
    }
    public class FileItem
    {
        public string FullName { get; set; }

        public string Name { get; set; }
        public bool Selected { get; set; }
    }
    public class BlTemplateVo
    {
        /// <summary>
        /// 文书、模板Id
        /// </summary>
        public string templateId { get; set; }
        /// <summary>
        /// 文书、模板名称
        /// </summary>
        public string templateName { get; set; }
        /// <summary>
        /// 文书内容
        /// </summary>
        public string xmldata { get; set; }

    }
    //[Serializable]
    //public partial class ZybrjbxxVO
    //{

    //    #region Model
    //    private string _id;

    //    private string _zyh;
    //    private string _blh;
    //    private string _xm;
    //    private string _py;
    //    private string _wb;
    //    private string _sfzh;
    //    private string _sex;
    //    private DateTime? _birth;
    //    private int _zybz;
    //    private bool? _sfqj;
    //    private string _deptcode;
    //    private string _wardcode;
    //    private string _ysgh;
    //    private string _bedcode;
    //    private DateTime _ryrq;
    //    private DateTime? _rqrq;
    //    private DateTime? _cqrq;
    //    private string _wzjb;
    //    private string _hljb;
    //    private string _ryfs;
    //    private string _cyfs;
    //    private DateTime? _gdxmzxrq;
    //    private string _brxzdm;
    //    private string _brxzmc;
    //    private string _cardno;
    //    private string _cardtype;
    //    private string _lxr;
    //    private string _lxrgx;
    //    private string _lxrdh;
    //    private string _zddm;
    //    private string _zdmc;
    //    private string _cyzddm;
    //    private string _cyzdmc;
    //    private int? _nl;
    //    private string _DeptName;
    //    private string _WardName;
    //    private string _ysxm;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string Id
    //    {
    //        set { _id = value; }
    //        get { return _id; }
    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string zyh
    //    {
    //        set { _zyh = value; }
    //        get { return _zyh; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string blh
    //    {
    //        set { _blh = value; }
    //        get { return _blh; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string xm
    //    {
    //        set { _xm = value; }
    //        get { return _xm; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string py
    //    {
    //        set { _py = value; }
    //        get { return _py; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string wb
    //    {
    //        set { _wb = value; }
    //        get { return _wb; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string sfzh
    //    {
    //        set { _sfzh = value; }
    //        get { return _sfzh; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string sex
    //    {
    //        set { _sex = value; }
    //        get { return _sex; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public DateTime? birth
    //    {
    //        set { _birth = value; }
    //        get { return _birth; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int zybz
    //    {
    //        set { _zybz = value; }
    //        get { return _zybz; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public bool? sfqj
    //    {
    //        set { _sfqj = value; }
    //        get { return _sfqj; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string DeptCode
    //    {
    //        set { _deptcode = value; }
    //        get { return _deptcode; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string WardCode
    //    {
    //        set { _wardcode = value; }
    //        get { return _wardcode; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string ysgh
    //    {
    //        set { _ysgh = value; }
    //        get { return _ysgh; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string BedCode
    //    {
    //        set { _bedcode = value; }
    //        get { return _bedcode; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public DateTime ryrq
    //    {
    //        set { _ryrq = value; }
    //        get { return _ryrq; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public DateTime? rqrq
    //    {
    //        set { _rqrq = value; }
    //        get { return _rqrq; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public DateTime? cqrq
    //    {
    //        set { _cqrq = value; }
    //        get { return _cqrq; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string wzjb
    //    {
    //        set { _wzjb = value; }
    //        get { return _wzjb; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string hljb
    //    {
    //        set { _hljb = value; }
    //        get { return _hljb; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string ryfs
    //    {
    //        set { _ryfs = value; }
    //        get { return _ryfs; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string cyfs
    //    {
    //        set { _cyfs = value; }
    //        get { return _cyfs; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public DateTime? gdxmzxrq
    //    {
    //        set { _gdxmzxrq = value; }
    //        get { return _gdxmzxrq; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string brxzdm
    //    {
    //        set { _brxzdm = value; }
    //        get { return _brxzdm; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string brxzmc
    //    {
    //        set { _brxzmc = value; }
    //        get { return _brxzmc; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string cardno
    //    {
    //        set { _cardno = value; }
    //        get { return _cardno; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string cardtype
    //    {
    //        set { _cardtype = value; }
    //        get { return _cardtype; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string lxr
    //    {
    //        set { _lxr = value; }
    //        get { return _lxr; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string lxrgx
    //    {
    //        set { _lxrgx = value; }
    //        get { return _lxrgx; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string lxrdh
    //    {
    //        set { _lxrdh = value; }
    //        get { return _lxrdh; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string zddm
    //    {
    //        set { _zddm = value; }
    //        get { return _zddm; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string zdmc
    //    {
    //        set { _zdmc = value; }
    //        get { return _zdmc; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string cyzddm
    //    {
    //        set { _cyzddm = value; }
    //        get { return _cyzddm; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string cyzdmc
    //    {
    //        set { _cyzdmc = value; }
    //        get { return _cyzdmc; }
    //    }

    //    public int? nl
    //    {
    //        set { _nl = value; }
    //        get { return _nl; }
    //    }
    //    public string DeptName
    //    {
    //        set { _DeptName = value; }
    //        get { return _DeptName; }
    //    }
    //    public string WardName
    //    {
    //        set { _WardName = value; }
    //        get { return _WardName; }
    //    }
    //    public string ysxm
    //    {
    //        set { _ysxm = value; }
    //        get { return _ysxm; }
    //    }
    //    #endregion Model

    //}
}

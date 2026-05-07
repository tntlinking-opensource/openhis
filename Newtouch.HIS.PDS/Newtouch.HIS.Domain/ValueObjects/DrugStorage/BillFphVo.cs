using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.ValueObjects.DrugStorage
{
    public class BillFphVo
    {
        public string Pdh { get; set; }
        public string Rkbm { get; set; }
        public string Rkmc { get; set; }
        public string Ckbm { get; set; }
        public string Ckmc { get; set; }
        public DateTime Rksj { get; set; }
        public string Fph { get; set; }
        public string pc { get; set; }
        public decimal Zje { get; set; }
    }

    public class BillFphMxVo
    {
        public string ypdm { get; set; }
        public string ypmc { get; set; }
        public string dlmc { get; set; }
        public string pc { get; set; }
        public string fph { get; set; }
        public string slStr { get; set; }
        public decimal sl { get; set; }
        public string dw { get; set; }
        public string gg { get; set; }
        public string ph { get; set; }
        public string yxq { get; set; }
        public string sccj { get; set; }
        public string lsjdjdw { get; set; }
        public decimal lsze { get; set; }
        public decimal bzs { get; set; }
        public string bzdw { get; set; }
        public string zxdw { get; set; }
        public decimal kykc { get; set; }
        public decimal pfj { get; set; }
        public decimal lsj { get; set; }
        public decimal zxdwlsj { get; set; }
        public decimal ykpfj { get; set; }
        public decimal yklsj { get; set; }
        public decimal zxdwjj { get; set; }
        public decimal bzdwjj { get; set; }

    }
}

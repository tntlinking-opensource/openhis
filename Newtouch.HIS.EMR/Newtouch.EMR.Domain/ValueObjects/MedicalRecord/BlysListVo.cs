using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.EMR.Domain.ValueObjects.MedicalRecord
{
    public class BlysListVo
    {
        public string ysdl { get; set; }
        public string ysId { get; set; }
        public string ysdm { get; set; }
        public string ysmc { get; set; }
        public int yslx { get; set; }
        public string yslxmc { get; set; }
        public string BindingPath { get; set; }

    }

    public class TableColumnVo
    {
        public int column_id { get; set; }
        public string columnName { get; set; }
        public string columnType { get; set; }
        public short max_length { get; set; }
        public byte scale { get; set; }
    }

    public class BlysDeatilVo
    {
        public string ysId { get; set; }
        public string ysmxcode { get; set; }
        public string ysmxName { get; set; }
    }

    public class BlysGridVo
    {
        public int Id { get; set; }
        public int ElementId { get; set; }
        public string Table_Name { get; set; }
        public int Table_Column_No { get; set; }

        public string Table_Column_Code { get; set; }
        public string Table_Colunn_Name { get; set; }

        public string Table_Column_Type { get; set; }

        public string Element_ID { get; set; }
        public string Element_Name { get; set; }

        public int Element_Type { get; set; }
        public string Element_Type_Name { get; set; }
        public int Sybz { get; set; }
        public string ysmxId { get; set; }
        public string AreValuemc { get; set; }
        public string AreValue { get; set; }
        public int Px { get; set; }
    }
    public class byysjg
    {
        public string ysid { get; set; }
        public string ysvalue { get; set; }
    }

    public class BlyyjghDto
    {
        public List<Blyyjgh> data { get; set; }
    }

    public class Blyyjgh
    {
        public string keyCode { get; set; }
        public string keyValue { get; set; }
        public string keyId { get; set; }
        public string keyName { get; set; }
    }
}

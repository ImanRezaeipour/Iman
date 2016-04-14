using System;

namespace AtlasTrafficReader.Classes
{
    internal class Traffics
    {
        public string BarCode { get; set; }

        public DateTime Date { get; set; }

        public int FirstIn { get; set; }

        public int LastOut { get; set; }
    }
    internal class Traficsdata
    {
        public string barcode { get; set; }
        public DateTime date { get; set; }        
        public int time { get; set; }
        public string inout { get; set; }
    }
    internal class Traficsdata1
    {
        public Int32 barcode { get; set; }
        public DateTime date { get; set; }
        public Int32 time { get; set; }
        public String inout { get; set; }
    }

}
       
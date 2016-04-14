using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtlasTrafficReader.Classes
{
    public static class Info
    {
        private static int _Progress=0;
        public static int Progress
        {
            get { return _Progress; }
            set { _Progress = value; }
        }

        private static string _Message="";
        public static string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        private static string _File = "";
        public static string File
        {
            get { return _File; }
            set { _File = value; }
        }

        private static int _SheetRemain = 0;
        public static int SheetRemain
        {
            get { return _SheetRemain; }
            set { _SheetRemain = value; }
        }
    }
}

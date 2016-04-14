using System;

namespace AtlasTrafficReader.Classes
{
    public class Converter
    {
        public static DateTime ConvertToPersian(string dateTime)
        {
            if (dateTime == "") dateTime = "1300/01/01";
            string[] time = new string[3];
            time = dateTime.Split('/');
            int day = Convert.ToInt32(time[2]);
            int month = Convert.ToInt32(time[1]);
            int year = Convert.ToInt32(/*"13" + */time[0]);
            System.Globalization.PersianCalendar dc = new System.Globalization.PersianCalendar();            
            return dc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        public static DateTime ConvertToDateTime(string dateTime)
        {
            if (dateTime == "") dateTime = "1300/01/01";
            string[] time = new string[3];
            time = dateTime.Split('-');
            int day = Convert.ToInt32(time[2]);
            int month = Convert.ToInt32(time[1]);
            int year = Convert.ToInt32(/*"13" + */time[0]);
            System.Globalization.PersianCalendar dc = new System.Globalization.PersianCalendar();
            return dc.ToDateTime(year, month, day, 0, 0, 0, 0);
            
        }
     
        public static int ConvertToMinute(string time)
        {
            if (time == "")
                return 9999; //9999 = null
            string[] t = new string[2];
            t = time.Split(':');
            int hour = Convert.ToInt32(t[0]);
            int minute = Convert.ToInt32(t[1]);
           
            if (time.Substring(time.Length - 3, 3) == "ق.ظ" || time.Substring(time.Length - 2, 2) == "AM" || time.Substring(time.Length - 2, 2) == "am")
            {
                if (hour == 12)
                    return minute;
                else
                    return (hour * 60) + minute;
            }
            else if (time.Substring(time.Length - 3, 3) == "ب.ظ" || time.Substring(time.Length - 2, 2) == "PM" || time.Substring(time.Length - 2, 2) == "pm")
            {
                if (hour == 12)
                    return (hour * 60) + minute;
                return ((hour + 12) * 60) + minute;
            }
            return 0;
        }        
    }
}

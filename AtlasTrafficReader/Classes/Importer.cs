using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading;
//using System.Transactions;
//using Excel;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Globalization;
using Library35.Globalization;


namespace AtlasTrafficReader.Classes
{
    internal class Impoerter
    {

        //private byte[] buffer;	
        private FalatGTSEntities falatGTS = new FalatGTSEntities();
        private List<Traficsdata> result = new List<Traficsdata>();
        private List<decimal> NeedUpdate = new List<decimal>();
   
        ~Impoerter()
        {
            if (falatGTS != null) falatGTS.Dispose();
        }
     
        public void NewFiles(string file)
        {
            //if (Directory.Exists(ConfigurationManager.AppSettings["FolderPath"]))
            //{
            //    string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["FolderPath"]);
            //    foreach (string file in files)
            //    {
                    try
                    {
                       if (!IsRegistredFile(file))
                        {
                           
                            Classes.Info.File = file;
                            //GetExcelSheetNames(file);                           

                            List<string> data = ReadFile(file);

                            List<Traficsdata> trafics = Convertstringtotrafic(data);
                            string YearFolder = CreateFolderInImportedPath();
                            ProcessTrafics(file, trafics, YearFolder);
                            // WriteDatainExcel(result, ConfigurationManager.AppSettings["ExcelPath"]);                                                   
                            Classes.Info.Progress = 0;

                            // ثبت در فایل
                            using (StreamWriter stream = new StreamWriter(ConfigurationManager.AppSettings["ImportedPath"] + ConfigurationManager.AppSettings["LogFile"], true, System.Text.Encoding.UTF8))
                            {
                                stream.WriteLine(file);
                            }
                            // انتقال فایل
                            try
                            {                                                               
                                 System.IO.File.Copy(file, ConfigurationManager.AppSettings["ImportedPath"] +  YearFolder + "\\" + new FileInfo(file).Name, true);
                                 System.IO.File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                LogError("Action: MoveFile; Error: " + ex, file);
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Method: NewFiles; Error: " + ex.Message, file);
                        //Info.Message+="Method: NewFiles; Error: " + ex.Message+"; FileName: "+ file+"\n";
                    }
                //}
           // }
        }
        /// <summary>
        /// اگر نام یکی از فایل ها در فایل لاگ باشد بررسی نمی شود
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsRegistredFile(string fileName)
        {
            
            try
            {
                using (StreamReader reader = new StreamReader(ConfigurationManager.AppSettings["ImportedPath"] + ConfigurationManager.AppSettings["LogFile"], System.Text.Encoding.UTF8))
                {
                    List<string> files = new List<string>();
                    while (!reader.EndOfStream)
                    {                        
                        files.Add(reader.ReadLine());
                       
                    }
                    if (files.Contains(fileName))
                        return true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                //var logDB = new LogDBEntities();
                //if (logDB.TA_TrafficLog.Count(m => m.FileName == fileName) > 0) return true;
                return false;
            }
        }
        private string CreateFolderInImportedPath()
        {
            try
            {
                DateTime ? nowdate = DateTime.Now;
                string year = null;  //folder name
                //Get Persian Date
                if (nowdate != null && nowdate.HasValue)
                {                   
                    PersianDateTime ShamsiDate = PersianDateTime.ParseEnglish(Convert.ToString(nowdate.Value));                   
                    year = Convert.ToString(ShamsiDate.Year);
                    //if not exists current year folder then create thats.
                     if (Directory.Exists(ConfigurationManager.AppSettings["ImportedPath"]))
                     {
                         string[] directories = Directory.GetDirectories(ConfigurationManager.AppSettings["ImportedPath"]);
                         if(directories.Contains(ConfigurationManager.AppSettings["ImportedPath"]+year))
                             return year;
                          else                        
                            //create folder
                            System.IO.Directory.CreateDirectory(ConfigurationManager.AppSettings["ImportedPath"] + year);                         
                     }                   
                }
                return year;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private List<string> ReadFile(string filename)
        {
            List<string> data = new List<string>();
            try
            {

                //using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                //{                   
                //    buffer = new byte[fs.Length];
                //    fs.Read(buffer,0,(int)fs.Length);

                //    fs.Close();
                //}              
                using (StreamReader reader = new StreamReader(filename, System.Text.Encoding.UTF8))                                   
                    while (!reader.EndOfStream)
                        data.Add(reader.ReadLine());                
            }
            catch (Exception ex)
            {
                // return false;
            }
            return data;
        }
        private List<Traficsdata> Convertstringtotrafic(List<string> indata)
        {
            List<Traficsdata> trafics = new List<Traficsdata>();
                try
                {  
                  
                foreach (var data in indata)
                {
                   
                    Traficsdata traficdata = new Traficsdata();
                   
                    if (data.Substring(0, 2) != "ID")
                    if (data.Length == 23 || data.Length == 24 /*|| data != "\n"*/)
                    {
                        //1001-1005 for Melli Bank Personel
                        if (data.Substring(0, 4) != "1001" && data.Substring(0, 4) != "1002" && data.Substring(0, 4) != "1003" && data.Substring(0, 4) != "1004" && data.Substring(0, 4) != "1005" && data.Substring(0, 4) != "1006")
                        {
                            traficdata.barcode = data.Substring(0, 5) != null ? data.Substring(0, 5) : "";                            
                            traficdata.date = Convert.ToDateTime(data.Substring(6, 4) + "/" + data.Substring(10, 2) + "/" + data.Substring(12, 2));
                            traficdata.time = Convert.ToInt32(data.Substring(15, 2)) * 60 + Convert.ToInt32(data.Substring(18, 2));
                            if (data.Substring(21) == "in" || data.Substring(21) == "In" || data.Substring(21) == "IN" || data.Substring(21) == "iN")
                                traficdata.inout = "In";
                            if (data.Substring(21) == "out" || data.Substring(21) == "Out" || data.Substring(21) == "OUT")
                                traficdata.inout = "Out";
                            //23:59 in minute =1439
                            if (traficdata.time >= 0 && traficdata.time <= 1439)                            
                                trafics.Add(traficdata);                            
                        }
                    }
                    
                   
                }
                }
                catch (Exception ex)
                {                   
                    //LogError("Method: NewFiles; Error: " + ex.Message, file);
                    //Info.Message+="Method: NewFiles; Error: " + ex.Message+"; FileName: "+ file+"\n";
                }
          
            //sort list based on barcode 
            //trafics = trafics.OrderBy(x => x.barcode).ThenBy(x => x.date).ThenBy(x => x.inout).ThenBy(x => x.time).ToList();
            trafics = trafics.OrderBy(x => x.barcode).ThenBy(x => x.date).ToList();
           // WriteDatainExcel(trafics, "D:\\Taraddod\\Src_ImportData\\New\\output\\in.xls");
            return trafics;
        }
        private bool ProcessTrafics(string filename , List<Traficsdata> trafics, string YearFolder)
        {
            //Deleteta_needupdatecfp(filename);
            Classes.Info.SheetRemain = trafics.Count;
            // BackgroundWorker Progress value
            int sheetPercent = 1, progressValue = 1;

            int resstartforperson = 0, resendforperson=0;

            int startperson = 0; int endperson = trafics.Count;
            int rep = 0;
            try
            {
                do
                {
                    rep = 0;
                    int j;
                    for (j = startperson; j < trafics.Count - 1; j++)
                    {
                        if (trafics[j].barcode == trafics[j + 1].barcode)
                            rep++;
                        else
                            break;
                    }
                    endperson = j;
                    //if (startperson == trafics.Count) break;
                    if (rep == 0)
                    {
                        result.Add(trafics[startperson]);  //one data in month
                        WriteTraficsInDatabaseTable(filename, result.Count - 1, result.Count - 1, YearFolder);
                        //Classes.Info.SheetRemain--;                        
                        if (trafics.Count < 100)
                            sheetPercent = 1;
                        else sheetPercent = trafics.Count / 100;
                        progressValue = (j) / sheetPercent;
                        Info.Progress = progressValue;
                    }
                    if (rep != 0)
                    {
                        trafics = RemoveRepinoutPerson(trafics, startperson, ref endperson);
                        resstartforperson = result.Count;                        
                        Check20MinutesPerson(trafics, startperson, endperson);

                        if (endperson == startperson)
                            resstartforperson = result.Count - 1;                                                   
                        resendforperson = result.Count - 1;
                        WriteTraficsInDatabaseTable(filename, resstartforperson, resendforperson, YearFolder);

                       //Classes.Info.SheetRemain--;

                        sheetPercent = trafics.Count / 100;
                        progressValue = (j) / sheetPercent;
                        Info.Progress = progressValue;
                    }
                    startperson = endperson + 1;
                } while (startperson < trafics.Count);
                //DateTime mindate = trafics.Skip(0).Take(trafics.Count-1).Min(m => m.date);
                //AddtoNeedUpdate(filename);
                //UpdateCfpByTable(mindate,filename);
            }
            catch (Exception ex)
            {
                return false;
            }
            //falatGTS.SaveChanges();
            /*
             * Update Cfp By table 
             */                      
            return true;
        }
        private List<Traficsdata> RemoveRepinoutPerson(List<Traficsdata> trafics, int start, ref int end)
        {
            int rep = start;
            int count;
            do
            {
                count = 0;
                int i;
                //int repbeforevalue = rep;
                for (i = rep; i < end; i++)
                {
                    //if (trafics[i + 1] != null)

                    if (trafics[i].date == trafics[i + 1].date && trafics[i].inout == trafics[i + 1].inout)
                    {
                        rep++;
                        count++;
                    }
                    else break;
                }                
                if (count != 0)
                {
                    int j = start;
                    if (trafics[j].inout == "In")
                    {
                        //delete
                        int index = 0;
                        for (j = start; j < rep; j++)
                        {
                            //in  
                            trafics.RemoveAt(j - index);
                            index++;
                        }
                        rep -= (index);
                        //rep = repbeforevalue;
                        end -= (index);
                    }
                    else if (trafics[j].inout == "Out")
                    {
                        //delete
                        int index = 0;
                        for (j = start + 1; j <= rep; j++)
                        {
                            //in                           
                            trafics.RemoveAt(j - index);
                            index++;
                        }
                        //
                        rep -= (index);
                        //rep = repbeforevalue;
                        end -= (index);
                    }
                }
                rep++;
                start = rep;
            } while (rep <= end);
            //if (i != trafics.Count)
            //{
            //    trafics = RemoveRepinoutPerson(trafics, rep + 1, end);
            //}
            return trafics;
        }
        /// <summary>
        /// check 20 minuate and add trafic to result
        /// </summary>
        /// <param name="trafics"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Check20MinutesPerson(List<Traficsdata> trafics, int start, int end)
        {
            // List<Traficsdata> result = new List<Traficsdata>();
            //Traficsdata tdata = new Traficsdata(); 
          
            int endlist = end;            
            int rep = 0;
            bool prev;
            do
            {
                //if (start == trafics.Count)
                //{
                //    result.Add(trafics[start]);
                //    break;
                //}
                rep = 0;
                int j;
                for (j = start; j < endlist; j++)
                {
                    if (trafics[j].date == trafics[j + 1].date)
                        rep++;
                    else
                        break;
                }
                end = j;
                result.Add(trafics[start]);
                if (rep > 1)
                {
                    if (trafics[start].inout == "Out")
                    {
                        result.Add(trafics[start + 1]);
                        start++;
                    }
                    prev = false;
                    while (start <= end)
                    {
                        
                        if (start == end)
                        {
                            if (!prev)
                            {

                                if (trafics[start].inout == "In" && result[result.Count - 2] != trafics[start - 1])
                                {
                                    result.Add(trafics[start - 1]);
                                }
                                result.Add(trafics[start]);
                                break;
                            }
                                //prev= true
                            else
                            {                               
                               // result.Add(trafics[start]);
                                break;
                            }
                        }
                        else if (trafics[start].inout == "Out" && trafics[start + 1].time - trafics[start].time > 20)
                        {
                            result.Add(trafics[start]);
                            result.Add(trafics[start + 1]);
                            start++;
                            if(start==end)
                                prev = true;
                        }
                        //if (trafics[start].inout == "In") or else of above condition
                        else
                        {
                            start++;
                        }
                    }
                }
                //next date
                start++;
            }while(start<=endlist);           
        }                 

        private bool WriteTraficsInDatabaseTable(string filename, int start , int end, string YearFolder)
        {    
            //var falatGTS = new FalatGTSEntities();
            bool IsRep = true;
            string bCode = result[start].barcode;
          
            decimal personId = 0;        
            try
            {
                personId =
                   (from items in falatGTS.TA_Person
                    where (items.Prs_Barcode == bCode || items.Prs_CardNum.Contains(bCode) || items.Prs_EmploymentNum == bCode)
                    select (decimal)items.Prs_ID).Single();              
                                            
            }
            catch (Exception ex)
            {
                // ثبت در فایل
                
                using (StreamWriter stream = new StreamWriter(ConfigurationManager.AppSettings["ImportedPath"] + YearFolder +"\\"+ "NOT-FOUND-BARCODE-IN-" + new FileInfo(filename).Name, true, System.Text.Encoding.UTF8))
                {
                    stream.WriteLine(bCode);
                }
                LogError("Method: ReadSheet; Barcode:   " + bCode +"  "+ex.Message+ "; Error: Barcode or CardNum not found.", filename);                    
                return false;
            }          
            if(!NeedUpdate.Contains(personId))
                NeedUpdate.Add(personId);                     
            DateTime mindate;
            DateTime maxdate;
            //for only one data
            if (start == end)
            {
                mindate = result[start].date;
                maxdate = mindate;
            }
            else
            {                      
                mindate = result.Skip(start).Take(end).Min(m => m.date);
                maxdate = result.Skip(start).Take(end).Max(m => m.date);
            }

            //List<Traficsdata> res = result.GetRange(start, end - start);
            //DateTime mindate = result.GetRange(start, end - start).Min(m => m.date);
            //DateTime maxdate = result.GetRange(start, end - start).Max(m => m.date);
            IQueryable<AtlasTrafficReader.TA_BaseTraffic> allPersonTraffics = falatGTS.TA_BaseTraffic.
                Where(w => w.BasicTraffic_PersonID == personId && w.BasicTraffic_Date >= mindate && w.BasicTraffic_Date <= maxdate);
            TA_BaseTraffic baseTraffic = null;
            int time;
            DateTime date;

            for (int i = start; i <= end; i++)
            {
                Classes.Info.SheetRemain--; 
                time = result[i].time;
                date = result[i].date;
                try
                {
                    // رکورد غیر  تکراری
                    if (
                        allPersonTraffics.Count(
                            w =>
                            w.BasicTraffic_PersonID == personId && w.BasicTraffic_Date == date
                            && (w.BasicTraffic_Time == time))== 0)                           
                        
                    {
                        IsRep = false;
                        // ذخیره رکورد
                        baseTraffic = null;
                        if (result[i].time != 9999) // 9999 = null
                        {
                            baseTraffic = new TA_BaseTraffic();
                            if (result[i].time == 0)
                            {
                                baseTraffic.BasicTraffic_Time = result[i].time + 1;
                            }
                            else
                            {
                                baseTraffic.BasicTraffic_Time = result[i].time;
                            }
                            baseTraffic.BasicTraffic_Date = result[i].date;
                            baseTraffic.BasicTraffic_PrecardId = 8832;
                            baseTraffic.BasicTraffic_PersonID = personId;
                            baseTraffic.BasicTraffic_Used = false;
                            baseTraffic.BasicTraffic_Active = true;
                            baseTraffic.BasicTraffic_Manual = false;
                            baseTraffic.BasicTraffic_State = true;
                            baseTraffic.BasicTraffic_ReportsListId = 0;
                            baseTraffic.BasicTraffic_OperatorPersonID = null;
                            baseTraffic.BasicTraffic_Description = "تردد مرکز";
                            baseTraffic.BasicTraffic_ClockCustomCode = null;
                            //insert in db table
                            falatGTS.TA_BaseTraffic.AddObject(baseTraffic);
                            //falatGTS.SaveChanges();                          
                        }
                    }
                        
                }
                catch (Exception ex)
                {
                    LogError("Method: RegisterRecord; Barcode:  " + result[i].barcode +"  "+ex.Message + "; Error: " + ex, filename);
                    //Info.Message += "Method: RegisterRecord; Barcode: " + traffic.BarCode + "; Error: " + ex.Message + "; FileName: " + fileName + "\n";
                    return false;
                }
            } //for

            //var selectedpersonfalatGTS.TA_Calculation_Flag_Persons
                //.Select(w => new { w.CFP_Date }).FirstOrDefault();       
            //CFPCALCulation Update
            if (!IsRep)
            {
                try
                {
                    falatGTS.SaveChanges();
                    TA_Calculation_Flag_Persons selectedpersonCFPDate = falatGTS.TA_Calculation_Flag_Persons
                    .Where(w => w.CFP_PrsId == personId).FirstOrDefault();

                    if (selectedpersonCFPDate.CFP_Date > mindate)
                    {
                        selectedpersonCFPDate.CFP_Date = mindate;
                        selectedpersonCFPDate.CFP_CalculationIsValid = false;
                        falatGTS.SaveChanges();                       
                    }
               
                }
                catch (Exception ex)
                {
                    // ثبت در فایل
                    using (StreamWriter stream = new StreamWriter(ConfigurationManager.AppSettings["ImportedPath"] + YearFolder +"\\"+ "DataBaseSaveChenageError-" + new FileInfo(filename).Name, true, System.Text.Encoding.UTF8))
                    {
                        stream.WriteLine(bCode);                            
                    }
                    LogError("Database SaveChange Error in  Barcode: " + bCode + ex.Message, filename);
                    return false;
                }
            }        
            return true;
        }
        private bool WriteDatainExcel(List<Traficsdata> traficsdata, string excelfilename)
        {
            Microsoft.Office.Interop.Excel.Workbook mWorkBook;
            Microsoft.Office.Interop.Excel.Sheets mWorkSheets;
            Microsoft.Office.Interop.Excel.Worksheet mWSheet1;
            Microsoft.Office.Interop.Excel.Application oXL;
            object misValue = System.Reflection.Missing.Value;
            oXL = new Microsoft.Office.Interop.Excel.Application();
            oXL.Visible = true;
            oXL.DisplayAlerts = false;
            oXL.UserControl = true;
            int index = 2;
            try
            {
                if (!File.Exists(excelfilename))             
                    // File.Create(excelfilename);                    
                    mWorkBook = oXL.Workbooks.Add();                
                else
                {
                    mWorkBook = oXL.Workbooks.Open(excelfilename);
                    mWorkBook = oXL.Workbooks.Open(excelfilename, 0, false, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true,
                       false, 0, true, false, false);
                }

                //Get all the sheets in the workbook
                mWorkSheets = mWorkBook.Worksheets;
                //Get the allready exists sheet
                mWSheet1 = (Microsoft.Office.Interop.Excel.Worksheet)mWorkSheets.get_Item("Sheet1");

                Microsoft.Office.Interop.Excel.Range range = mWSheet1.UsedRange;
                //int colCount = range.Columns.Count;
                //int rowCount = range.Rows.Count;

                //header
                mWSheet1.Cells[1, 1] = "barcode";
                mWSheet1.Cells[1, 2] = "tdate";
                mWSheet1.Cells[1, 3] = "ttime";
                mWSheet1.Cells[1, 4] = "inout";
                foreach (var trafic in traficsdata)
                {
                    mWSheet1.Cells[index, 1] = trafic.barcode;
                    mWSheet1.Cells[index, 2] = Convert.ToString(trafic.date);
                    mWSheet1.Cells[index, 3] = trafic.time;
                    mWSheet1.Cells[index, 4] = trafic.inout;
                    index++;
                }
                //save in  excel file
                mWorkBook.SaveAs(excelfilename, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value);
                mWorkBook.Close(true, Missing.Value, Missing.Value);
                mWSheet1 = null;
                mWorkBook = null;

                oXL.Quit();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        
        private void LogError(string ex, string fileName)
        {
            try
            {
                var logDB = new LogDBEntities();
                TA_TrafficLog trafficLogError = new TA_TrafficLog();
                trafficLogError.FileName = fileName;
                DateTime dateTime = DateTime.Now;
                trafficLogError.Date = dateTime;
                trafficLogError.Message = "Error";
                trafficLogError.Exception = ex;
                logDB.TA_TrafficLog.AddObject(trafficLogError);
                logDB.SaveChanges();
            }                
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Method: LogError; Error: " + exception.Message + "\n", "DATABASE ERROR!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //Info.Message += "DATABASE ERROR! Method: LogError; Error: " + exception.Message + "\n";
            }
        }

        private void Deleteta_needupdatecfp(string filename)
        {
            //delete all record of table
            //var allrecord = falatGTS.TA_NeedUpdateCFP.ToList();    
            try
            {
                falatGTS.TA_NeedUpdateCFP.Context.ExecuteStoreCommand("Delete from TA_NeedUpdateCFP");
                falatGTS.TA_NeedUpdateCFP.Context.SaveChanges();       
            }
            catch (Exception ex)
            {
                LogError("delete command exception from TA_NeedUpdateCFP", filename);                    
               
            }
               
        }
        private void AddtoNeedUpdate(string filename)
        {
            //Insert into TA_NeedUpdate
            try
            {
                for (int i = 0; i < NeedUpdate.Count(); i++ )
                {
                    TA_NeedUpdateCFP needupdatecfp = null;
                    needupdatecfp = new TA_NeedUpdateCFP();
                    needupdatecfp.PersonId = NeedUpdate[i];
                    falatGTS.TA_NeedUpdateCFP.AddObject(needupdatecfp);
                }
                falatGTS.SaveChanges();
            }
            catch (Exception ex)
            {              
                LogError("TA_NeedUpdateCFP Error: "  + ex.Message, filename);
                return;
            }
        }
        private void UpdateCfpByTable(DateTime date,string filename)
        {
            //exec spr_UpdateCFP_BYTable '2014-05-22'
            try
            {
                falatGTS.spr_UpdateCFP_ByTable(date);
                LogError("SPr Update CFP Completion in txt Import ", filename);
            }
            catch (Exception ex)
            {
                LogError("SPr Update CFP Error in txt Import" + ex, filename);
            }
        }
    
        private bool WriteDatainExcel(List<Traficsdata1> traficsdata, string excelfilename)
        {

            Microsoft.Office.Interop.Excel.Workbook mWorkBook;
            Microsoft.Office.Interop.Excel.Sheets mWorkSheets;
            Microsoft.Office.Interop.Excel.Worksheet mWSheet1;             
            Microsoft.Office.Interop.Excel.Application oXL;
            object misValue = System.Reflection.Missing.Value;
            oXL = new Microsoft.Office.Interop.Excel.Application();
            oXL.Visible = true;
            oXL.DisplayAlerts = false;
            oXL.UserControl = true;
            int index = 2;
            try
            {
                if (!File.Exists(excelfilename))
                {
                    // File.Create(excelfilename);                    
                    mWorkBook = oXL.Workbooks.Add();

                }
                else
                {
                    //mWorkBook = oXL.Workbooks.Open(excelfilename);
                    mWorkBook = oXL.Workbooks.Open(excelfilename, 0, false, 5, "", "", false, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true,
                       false, 0, true, false, false);
                }

                //Get all the sheets in the workbook
                mWorkSheets = mWorkBook.Worksheets;                
                //Get the allready exists sheet
                mWSheet1 = (Microsoft.Office.Interop.Excel.Worksheet)mWorkSheets.get_Item("Sheet1");
                
                Microsoft.Office.Interop.Excel.Range range = mWSheet1.UsedRange;

                //int colCount = range.Columns.Count;
                //int rowCount = range.Rows.Count;

                //header
                mWSheet1.Cells[1, 1] = "barcode";
                mWSheet1.Cells[1, 2] = "tdate";
                mWSheet1.Cells[1, 3] = "ttime";
                mWSheet1.Cells[1, 4] = "inout";
                foreach (var trafic in traficsdata)
                {
                    mWSheet1.Cells[index, 1] = trafic.barcode;
                    mWSheet1.Cells[index, 2] = Convert.ToString(trafic.date);
                    mWSheet1.Cells[index, 3] = trafic.time;
                    mWSheet1.Cells[index, 4] = trafic.inout;
                    index++;
                }
                //save in  excel file
                mWorkBook.SaveAs(excelfilename, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value);
                mWorkBook.Close(true, Missing.Value, Missing.Value);
                mWSheet1 = null;
                mWorkBook = null;

                oXL.Quit();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}

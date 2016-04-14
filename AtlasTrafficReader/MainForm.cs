using System;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using AtlasTrafficReader.Classes;
using System.IO;

namespace AtlasTrafficReader
{
    public partial class MainForm : Form
    {
        private Thread Txtthread;
        //private Thread Elsthread;
        private bool Txtthreadrun = false;
        //private bool Elsthreadrun = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblFileName.Text = "";
            lblPercent.Text = "";
            Scheduler();
        }
        public void Progress(int value)
        {
            prgImport.Value += value;
        }

        private void DoTxtImport(string filename)
        {
            Impoerter import = new Impoerter();
            import.NewFiles(filename);
            
            //timerInfo.Stop();
            rtbInfo.Text += "filename finished.\n";
            //prgImport.Value = 0;
            //lblFileName.Text = "";
            //lblPercent.Text = "";
            //btnImport.Enabled = true;
            //Txtthreadrun = false;
            //btnCancel.Visible = false;
            ////timer.Interval = 500000000;
            ////timer.Tick += new EventHandler(timer_Tick);
            //Thread.Sleep(1200000);   
            //timer.Start();
            //Application.Exit();                                 
        }
        private void DoExcelImport(string filename)
        {
            Classes.XLS xls = new Classes.XLS();
            xls.NewFiles(filename);
            
            //timerInfo.Stop();
            rtbInfo.Text += "filename finished.\n";
            //prgImport.Value = 0;
            //lblFileName.Text = "";
            //lblPercent.Text = "";
            //btnImport.Enabled = true;
            btnCancel.Visible = false;
        }
        private void Scheduler()
        {
                CheckForIllegalCrossThreadCalls = false;
                prgImport.Value = 0;
                lblFileName.Text = "";
                lblPercent.Text = "";
                rtbInfo.Text = "";
                timerInfo.Start();
                Txtthread = new Thread(GetFiles);
                //Txtthread = new Thread(()=>DoTxtImport(filename));
                Txtthread.Start();
                Txtthreadrun = true;                              
        }
    
        private void GetFiles()
        {
            try
            {
                if (Directory.Exists(ConfigurationManager.AppSettings["FolderPath"]))
                {
                    string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["FolderPath"]);
                    foreach (string file in files)
                    {
                        string filetype = Path.GetExtension(file);
                        if (filetype == ".txt")
                            DoTxtImport(file);
                        else if (filetype == ".xls" || filetype == ".xlsx")
                            DoExcelImport(file);
                    }
                }
                timerInfo.Stop();
                Txtthreadrun = false;                
                Thread.Sleep(120000);
                //Thread.Sleep(1200000); 
            }
            catch (Exception ex)
            {
                
            }
            Application.Exit();     
        }
        private void AutoDo()
        {
            //Classes.XLS xls = new Classes.XLS();
            //xls.NewFiles();
            //Impoerter import = new Impoerter();
            //import.NewFiles();
        }     
        private void timer_Tick(object sender, EventArgs e)
        {
            AutoDo();
        }

        //private void btnStop_Click(object sender, EventArgs e)
        //{
        //    timer.Stop();
        //    btnStop.Enabled = false;
        //    btnAutomatic.Enabled = true;
        //}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Txtthread.Abort();
            //DialogResult dr = MessageBox.Show(
            //    "Are you sure?",
            //    "Exit",
            //    MessageBoxButtons.YesNo,
            //    MessageBoxIcon.Question,
            //    MessageBoxDefaultButton.Button2);
            //if (dr == DialogResult.No)
            //{
            //    thread.Abort();
            //    e.Cancel = true;
            //}
           
        }

        private void timerInfo_Tick(object sender, EventArgs e)
        {
            //rtbInfo.Text = Classes.Info.Message;

            rtbInfo.Text = "Traffics Count: " + Classes.Info.SheetRemain.ToString() + "\n";

            if (Classes.Info.Progress > 100)
                prgImport.Value = 100;
            else
                prgImport.Value = Classes.Info.Progress;
            lblPercent.Text = "";
            lblPercent.Text = Classes.Info.Progress.ToString() + "%";
            lblFileName.Text = Classes.Info.File;
        }                   

        //private void btnExcelImport_Click(object sender, EventArgs e)
        //{
        //    CheckForIllegalCrossThreadCalls = false;
        //    rtbInfo.Text = "";
        //    btnImport.Enabled = false;
        //    btnCancel.Visible = true;
        //    btnCancel.Text = "Pause Importing";
        //    timerInfo.Start();
        //    thread = new Thread(DoExcelImport);
        //    thread.Start();

        //    threadrun = true;
        //}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();   
            //if (Txtthreadrun == true)
            //{
            //    Txtthread.Suspend();
            //    timerInfo.Stop();
            //    btnCancel.Text = "Resume Importing";
            //    Txtthreadrun = false;
            //}
            //else if (Txtthreadrun == false)
            //{
            //    Txtthread.Resume();
            //    timerInfo.Start();
            //    btnCancel.Text = "Pause Importing";
            //    Txtthreadrun = true;
            //}
        }
    }
}

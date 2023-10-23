using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace iClipsBrain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process IClipsMonitorModule = new Process();

       MoodView mood = new MoodView();

        public MainWindow()
        {
            //var dllDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

            InitializeComponent();
            StringBuilder sb = new StringBuilder();

             using (StreamReader reader = new StreamReader("commands.txt"))
             {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.First()!='#')
                        sb.AppendLine(line);
                }
             }
              
            
            

//            sb.AppendLine("");
//            sb.AppendLine("(reset)");
             //string nomeFile = "ecoexp";
            //int count = 1;
            //while (true) 
            //{
                
            //    if (File.Exists(nomeFile + count+".txt"))
            //        count += 1;
            //    else
            //        break;
              
            //}
            //sb.AppendLine("(open "+nomeFile+count+".txt mydata \"a\")");
            //sb.AppendLine("(open ecoexp.txt mydata \"a\")");

            //sb.AppendLine("(close)");
            //sb.AppendLine("(facts)");
            //sb.AppendLine("(watch rules)");
            //sb.AppendLine("------------");
            //sb.AppendLine("(reject \"AttentionModule\")");
            //sb.AppendLine("(reset)");


            ClipsEnv.Editor.Text = sb.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClipsEnv.EmbedEditor = false;
            var w = new Window();
            w.Closing += (o, evt) =>
            {
                w.Content = null;
                ClipsEnv.EmbedEditor = true;
            };
            w.Content = ClipsEnv.Editor;
            
           
           
            w.Show();
        }

        private void cbRun_Checked(object sender, RoutedEventArgs e)
        {
            ClipsEnv.RunTimer.Start();
        }

        private void cbRun_Unchecked(object sender, RoutedEventArgs e)
        {
            ClipsEnv.RunTimer.Stop();
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            mood.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Process proc in Process.GetProcessesByName("IClipsMonitor"))
            {
                proc.CloseMainWindow();
                proc.Kill();

            }
            mood.Close();

            ClipsEnv.Dispose();

        }

        private void ClipsEnv_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Timers;

namespace InteractiveCLIPS
{
    /// <summary>
    /// Interaction logic for iClips.xaml
    /// </summary>
    public partial class iClips : UserControl
    {
        private System.Timers.Timer runTimer = new System.Timers.Timer();
        public System.Timers.Timer RunTimer
        {
            get { return runTimer; }
        }
        
        private CLIPSNet.Environment env;        
        public CLIPSNet.Environment Env
        {
            get { return env; }
        }

        private Object envLock = new Object();
        public Object EnvLock
        {
            get { return envLock; }
        }

        private ClipsRouter router;        
        internal ClipsRouter Router
        {
            get { return router; }
        }

        private ModuleManager manager;
        internal ModuleManager Manager
        {
            get { return manager; }
        }

        private ClipsEditor editor;
        public ClipsEditor Editor
        {
            get { return editor; }
        }

        private bool embedEditor = false;
        public bool EmbedEditor
        {
            get { return embedEditor; }
            set
            {
                embedEditor = value;
                if (embedEditor)
                    this.Content = editor;
                else
                    this.Content = new Grid();
            }
        }

        public string Text
        {
            get { return editor.Text; }
            set { editor.Text = value; }
        }
              

        public iClips()
        {
            InitializeComponent();

            editor = new ClipsEditor(envLock);
            editor.ClipsEnv = this;

            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            Init();
        }

        private void Init()
        {
            env = new CLIPSNet.Environment();
            var dir = new System.IO.FileInfo((typeof(iClips)).Assembly.Location).DirectoryName;

            manager = new ModuleManager(env, envLock);
            
            manager.LoadedModule += (o, e) =>
            {
                foreach (var a in manager.LoadedModules[e.ModuleName].actions)
                    editor.completionData.Add(new FunctionCompletionData(a.Name, "", a.Name, null));
            };

            new CLIPSNet.UserFunction(env, new Func<CLIPSNet.DataTypes.String, CLIPSNet.DataTypes.String, CLIPSNet.DataTypes.Boolean>((name, desc) =>
            {
                var d = new FunctionCompletionData(desc.Value, null, name.Value, null);
                editor.completionData.Add(d);
                return true;
            }), "iclips-register-function");

            new CLIPSNet.UserFunction(env, new Func<CLIPSNet.DataTypes.String>(() => { var s = editor.openFileDialog(); return new CLIPSNet.DataTypes.String(s); }), "pick-file-name");

            int res = env.Load(dir + @"\Modules\core.clp");
            env.BatchStar(dir + @"\Modules\init.clp");
            router = new ClipsRouter(env, "RouterTest", 10);

            runTimer.Interval = 300;
            runTimer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (manager.envLockObj)
            {
                try
                {
                    env.Run();
                    editor.testBlockOutput.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(() => editor.testBlockOutput.Text = router.Output.ToString())
                    );

                    editor.txtMemory.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                        new Action(() => editor.txtMemory.Text = env.MemUsed.ToString())
                    );

                    //editor.testBlockOutput.Dispatcher.Invoke(
                    //    System.Windows.Threading.DispatcherPriority.Background,
                    //    new Action(() => editor.testBlockOutput.Text = router.Output.ToString())
                    //);
                }
                catch (ArgumentException argExc)
                {
                    System.Diagnostics.Debug.WriteLine(argExc.Message);
                }
            }
        }

        

        public void Dispose() 
        {
             
            runTimer.Stop();
            Manager.Dispose();
            env.Dispose();
        }

        public void Reset()
        {
            env.Dispose();
            Init();
        }

    }
}

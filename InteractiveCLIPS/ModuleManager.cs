using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace InteractiveCLIPS
{
    public class ModuleManager : IDisposable
    {
        internal Dictionary<string, Module> LoadedModules;
        
        private CLIPSNet.UserFunction loadModule;
        private CLIPSNet.UserFunction unloadModule;


        public CLIPSNet.Environment clipsEnv;
        public event EventHandler<LoadedModuleEventArgs> LoadedModule;
        private Dictionary<string, CLIPSNet.Fact> assertedFactIds;
        public Object envLockObj = new Object();

        internal ModuleManager(CLIPSNet.Environment clips, Object lockObj)
        {
            clipsEnv = clips;
            LoadedModules = new Dictionary<string, Module>();
            envLockObj = lockObj;
            
            loadModule = new CLIPSNet.UserFunction(clips, new Func<CLIPSNet.DataTypes.String, CLIPSNet.DataTypes.Boolean>(ClipsLoadModule), "load-module");
            unloadModule = new CLIPSNet.UserFunction(clips, new Func<CLIPSNet.DataTypes.String, CLIPSNet.DataTypes.Boolean>(ClipsUnLoadModule), "unload-module");

            assertedFactIds = new Dictionary<string, CLIPSNet.Fact>();
        }

        public CLIPSNet.DataTypes.Boolean ClipsLoadModule(CLIPSNet.DataTypes.String name)
        {
            return new CLIPSNet.DataTypes.Boolean(LoadModule(name.Value));
        }
        public CLIPSNet.DataTypes.Boolean ClipsUnLoadModule(CLIPSNet.DataTypes.String name)
        {
            return new CLIPSNet.DataTypes.Boolean(UnloadedModule(name.Value));
        }

        public string ModulesDir
        {
            get { return string.Format("{0}\\Modules", new FileInfo(this.GetType().Assembly.Location).DirectoryName); }
        }

        private Module FindModule(string dir)
        {
            var md = new DirectoryInfo(dir);
            if (!md.Exists) return null;
            foreach (var f in md.GetFiles("*.dll"))
            {
                try
                {
                    var a = Assembly.LoadFrom(f.FullName);
                    
                    foreach (var t in a.GetTypes())
                    {
                        if (t.IsSubclassOf(typeof(Module)))
                        {
                             return (Module)a.CreateInstance(t.FullName);
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        private Type GetFunc(int count)
        {
            if (count < 1 || count > 16) throw new ArgumentException("Max 15 args allowed!");
            return typeof(Func<int>).Assembly.GetType("System.Func`" + count);
        }

        public bool LoadModule(string name)
        {
            if (LoadedModules.ContainsKey(name)) 
                return false;

            var md = string.Format("{0}\\{1}", ModulesDir, name);
            var m = FindModule(md);
            m.ModuleManager = this;

            if (m == null) 
                return false;

            var t = m.GetType();
            foreach (var f in t.GetMethods())
            {
                var a = f.GetCustomAttributes(typeof(ClipsActionAttribute), true);
                if (a.Length > 0)
                {
                    var ca = a[0] as ClipsActionAttribute;
                    var pars = f.GetParameters();
                    var delegateType = GetFunc(pars.Length + 1);

                    var types = new Type[pars.Length + 1];
                    for (int i = 0; i < pars.Length; i++)
                        types[i] = pars[i].ParameterType;
                    types[pars.Length] = f.ReturnType;
                    var funcd = delegateType.MakeGenericType(types);
                    Delegate d = Delegate.CreateDelegate(funcd, m, f);
                    var clipsFun = new CLIPSNet.UserFunction(clipsEnv, d, ca.Name);
                    m.actions.Add(clipsFun);
                }
            }

            m.initModule();

            if (!m.LoadCLP(clipsEnv))
            {
                m.Dispose();
                return false;
            }

            lock (envLockObj)
            {
                clipsEnv.AssertString(string.Format("(iclips-module (name \"{0}\"))", name));
               // m.AssertString(string.Format("(iclips-module (name \"{0}\"))", name));


            }
            LoadedModules.Add(name, m);
            if (LoadedModule != null) 
                LoadedModule(this, new LoadedModuleEventArgs { ModuleName = name }); 
            
            return true;
        }

        public bool UnloadedModule(string name)
        {
            //if (loadModule != null) loadModule.Dispose();
            if (LoadedModules.ContainsKey(name)) LoadedModules[name].Dispose();

            LoadedModules.Remove(name);

            return true;
        }


        public void AssertTemplate(string template, string search_key)
        {
            lock (envLockObj)
            {
                if (assertedFactIds.ContainsKey(search_key))
                {
                    assertedFactIds[search_key].Retract();
                    assertedFactIds[search_key].Dispose();
                }

                CLIPSNet.Fact f = clipsEnv.AssertString(template);
                if (f != null)
                {
                    //System.Diagnostics.Debug.WriteLine(f.Index);
                    assertedFactIds[search_key] = f;
                }
            }
        }

        public void Dispose()
        {
            foreach (var m in LoadedModules.Values) m.Dispose();
        }

    }
}

//   (fun_makeexp ?ev ?ea)

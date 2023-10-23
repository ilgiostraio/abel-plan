using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Documents;

using Sense.Lib.FACELibrary;
using CLIPSNet.DataTypes;
using CLIPSNet;

namespace InteractiveCLIPS
{
    #region Attribute definition
    public class ModuleDefinitionAttribute : Attribute
    {
        private string clpfname = null;
        public string ClpFileName
        {
            get { return clpfname; }
            set { clpfname = value; }
        }
        private Type[] deftemplates = null;
        public Type[] Deftemplates
        {
            get { return deftemplates; }
            set { deftemplates = value; }
        }


        public ModuleDefinitionAttribute() { }
    }

    public class ClipsActionAttribute : Attribute
    {
        public string Name;

        public ClipsActionAttribute(string name)
        {
            Name = name;
        }
    }

    #endregion

    public class LoadedModuleEventArgs : EventArgs
    {
        private string moduleName;
        public string ModuleName
        {
            get { return moduleName; }
            set { moduleName = value; }
        }
    }


    public abstract class DefTemplateBase
    {
        internal string ToFact()
        {
            var sb = new StringBuilder();
            var t = this.GetType();

            sb.AppendFormat("({0} ", t.Name);
            foreach (var f in t.GetFields())
            {
                sb.AppendFormat(" ({0} {1})", f.Name, f.GetValue(this));
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

    public abstract class Module : IDisposable
    {
        ModuleDefinitionAttribute moduleInfo = null;
        internal List<CLIPSNet.UserFunction> actions = new List<CLIPSNet.UserFunction>();

        private ModuleManager moduleManager;
        public ModuleManager ModuleManager
        {
            get { return moduleManager; }
            internal set { moduleManager = value; }
        }

        public CLIPSNet.Environment ClipsEnv 
        {
            get { return moduleManager.clipsEnv; } 
        }

        private CLIPSNet.Fact fact;

        public CLIPSNet.Fact Fact 
        {
            get { return fact; }
        }


        public Module()
        {
            var a = (ModuleDefinitionAttribute[])this.GetType().GetCustomAttributes(typeof(ModuleDefinitionAttribute), true);
            if (a.Length > 0) 
                moduleInfo = a[0];
        }

        public string ClpFileName
        {
            get
            {
                if (moduleInfo == null || moduleInfo.ClpFileName == null)
                    return (this.GetType().Name + ".clp");
                else
                    return moduleInfo.ClpFileName;
            }
        }

        internal bool LoadCLP(CLIPSNet.Environment clips)
        {
            var fn = ClpFileName;
            var mwd = new FileInfo(this.GetType().Assembly.Location).DirectoryName;


            ///Returns an integer; Zero if the file couldnt be opened, -1 if the file
            /// was opened but an error occurred while loading, and 1 if the file 
            /// was opened an no errors occurred while loading. If syntactic errors 
            /// are in the constructs, Load still will attempt to read the entire file 
            /// and error notices will be sent to werror. 
            if (clips.Load(string.Format("{0}\\{1}", mwd, fn)) !=1)
                return false;
            
            return true;


            //ClipsEnv.AssertString("(subj)");
        }

        protected CLIPSNet.Fact Assert(DefTemplateBase t)
        {
            lock (moduleManager.envLockObj)
            {
                return ClipsEnv.AssertString(t.ToFact());
            }
        }

        public void AssertString(string t)
        {
            lock (moduleManager.envLockObj)
            {
                fact = ClipsEnv.AssertString(t);
            }
        }

        
        protected void AssertFact(string key, string value) 
        {
            lock (moduleManager.envLockObj)
            {
                ClipsEnv.AssertString(string.Format("({0} {1})", key, value));
            }
        }

        protected void AssertTemplate(string s, string search_key) 
        {
            moduleManager.AssertTemplate(s, search_key);
        }
        

        private void defTemplate(Type t)
        {
            var s = new StringBuilder();
            s.AppendFormat("(deftemplate {0} (", t.Name);
            foreach (var f in t.GetFields())
            {
                if (f.FieldType.IsSubclassOf(typeof(CLIPSNet.DataType)))
                {
                    var typename = "";
                    if (f.FieldType == typeof(CLIPSNet.DataTypes.String)) typename = "STRING";
                    else if (f.FieldType == typeof(CLIPSNet.DataTypes.Integer)) typename = "INTEGER";
                    else if (f.FieldType == typeof(CLIPSNet.DataTypes.Symbol)) typename = "SYMBOL";
                    s.AppendFormat(" (slot {0} (type {1}))", f.Name, typename);
                }
            }
            s.AppendFormat("))");
            //System.Diagnostics.Debug.WriteLine(s);
        }

        internal void initModule() 
        {
            var def = (ModuleDefinitionAttribute)this.GetType().GetCustomAttributes(typeof(ModuleDefinitionAttribute), true)[0];
            if (def.Deftemplates != null)
            {
                foreach (var t in def.Deftemplates)
                {
                    defTemplate(t);
                }
            }
            this.Init();
        }

        protected virtual void Init() { }

        public void Dispose() 
        {
            YarpClose();
            foreach (var f in actions) f.Dispose();

            //*************************
            // non fuziona perchè il template non è cancellabile 
            //*************************
            string name = this.GetType().Assembly.GetName().Name;
           
            CLIPSNet.Defmodule m = ClipsEnv.FindDefmodule(name);
            //var mm = ClipsEnv.GetDefmoduleList();
            //var s = ClipsEnv.GetDeftemplateList();

            Multifield sc = ClipsEnv.GetDeftemplateList(m);
            //Multifield sc = ClipsEnv.GetDeftemplateList();
            List<DataType> list = sc.ToList();

            foreach (DataType l in list)
            {
              Deftemplate df =  ClipsEnv.FindDeftemplate(l.ToString());
                if (df != null)
                    if (df.Undeftemplate())
                        Console.WriteLine("ok");
                        
            }

            
            
        }

        public abstract void YarpClose();

    }
}


//protected void AssertTemplate(Type t, object obj, string search_key)
//{
//    var convertedObject = Convert.ChangeType(obj, t);
//    StringBuilder s = new StringBuilder();

//    s.Append("(" + t.Name.ToString().ToLower() + " \n");

//    if (t.Name == "Subject")
//    {
//        #region sub
//        foreach (System.Reflection.PropertyInfo prop in t.GetProperties())
//        {
//            object val = typeof(Subject).GetProperty(prop.Name).GetValue(convertedObject, null);
//            if (val != null)
//            {
//                StringBuilder sbGeneric = new StringBuilder();
//                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
//                {

//                    System.Collections.IList l = (System.Collections.IList)val;
//                    sbGeneric.AppendFormat("({0}", prop.Name);
//                    foreach (object elem in l)
//                    {
//                        if (elem.ToString() != null)
//                            sbGeneric.AppendFormat(" {0}", elem.ToString());
//                    }
//                    sbGeneric.AppendFormat(")\n");

//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Limb")
//                {


//                    Limb li = (Limb)val;

//                    sbGeneric.AppendFormat(" ({0}Left", prop.Name);
//                    sbGeneric.AppendFormat(" ");
//                    sbGeneric.AppendFormat(" {0}", li.left.X.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", li.left.Y.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", li.left.Z.ToString("F3"));
//                    sbGeneric.AppendFormat(")\n");

//                    sbGeneric.AppendFormat(" ({0}Right", prop.Name);
//                    sbGeneric.AppendFormat(" {0}", li.right.X.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", li.right.Y.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", li.right.Z.ToString("F3"));
//                    sbGeneric.AppendFormat(")\n");

//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Position")
//                {
//                    Position pos = (Position)val;
//                    sbGeneric.AppendFormat("({0}", prop.Name);

//                    sbGeneric.AppendFormat(" {0}", pos.X.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", pos.Y.ToString("F3"));
//                    sbGeneric.AppendFormat(" {0}", pos.Z.ToString("F3"));
//                    sbGeneric.AppendFormat(")\n");
//                }
//                else
//                {
//                    sbGeneric.AppendFormat("({0} {1})\n", prop.Name, val.ToString());
//                }
//                s.Append(sbGeneric.ToString());
//            }

//        }
//        #endregion
//    }
//    else if (t.Name == "Surroundings")
//    {
//        #region surroundings
//        foreach (System.Reflection.PropertyInfo prop in t.GetProperties())
//        {
//            object val = typeof(Surroundings).GetProperty(prop.Name).GetValue(convertedObject, null);
//            if (val != null)
//            {
//                StringBuilder sbGen = new StringBuilder();
//                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
//                {
//                    System.Collections.IList l = (System.Collections.IList)val;
//                    sbGen.AppendFormat(" ({0} ", prop.Name);
//                    foreach (object elem in l)
//                    {
//                        if (elem.ToString() != null)
//                        {
//                            if (elem.ToString().Length > 4)
//                                sbGen.AppendFormat(" {0}", elem.ToString().Substring(0, 4));
//                            else
//                                sbGen.AppendFormat(" {0}", elem.ToString());
//                        }
//                    }
//                    sbGen.AppendFormat(")\n");
//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "SOME")
//                {
//                    SOME some = (SOME)val;

//                    sbGen.AppendFormat(" ({0}Mic", prop.Name);
//                    sbGen.AppendFormat(" {0})\n", some.mic.ToString());
//                    sbGen.AppendFormat(" ({0}Lux", prop.Name);
//                    sbGen.AppendFormat(" {0})\n", some.lux.ToString());
//                    sbGen.AppendFormat(" ({0}Temp", prop.Name);
//                    sbGen.AppendFormat(" {0})\n", some.temp.ToString());
//                    sbGen.AppendFormat(" ({0}IR", prop.Name);
//                    sbGen.AppendFormat(" {0})\n", some.IR.ToString());
//                    sbGen.AppendFormat(" ({0}Touch", prop.Name);
//                    sbGen.AppendFormat(" {0})\n", some.touch.ToString());


//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "")
//                {
//                    Saliency sal = (Saliency)val;
//                    sbGen.AppendFormat(" ({0}", prop.Name);
//                    if (sal.position.Count != 0)
//                    {
//                        sbGen.AppendFormat(" {0}", sal.position[0].ToString("F2"));
//                        sbGen.AppendFormat(" {0}", sal.position[1].ToString("F2"));
//                        sbGen.AppendFormat(" {0}", sal.saliencyWeight.ToString("F1"));
//                    }
//                    sbGen.AppendFormat(")\n ");
//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Ambience")
//                {
//                    sbGen.AppendFormat(" ({0}", prop.Name);
//                    sbGen.AppendFormat(")\n");
//                }
//                else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Resolution")
//                {
//                    Resolution res = (Resolution)val;
//                    sbGen.AppendFormat(" ({0}", prop.Name);
//                    sbGen.AppendFormat(" {0}", res.Width.ToString("F1"));
//                    sbGen.AppendFormat(" {0}", res.Height.ToString("F1"));
//                    sbGen.AppendFormat(")\n ");
//                }
//                else
//                {
//                    sbGen.AppendFormat(" ({0} {1})\n", prop.Name, val.ToString());
//                }
//                s.Append(sbGen.ToString());


//            }
//        }
//        #endregion
//    }
//    else
//    {
//        s.AppendFormat("({0} (", t.Name);
//        foreach (var f in t.GetFields())
//        {
//            if (f.FieldType.IsSubclassOf(typeof(CLIPSNet.DataType)))
//            {
//                var typename = "";
//                if (f.FieldType == typeof(CLIPSNet.DataTypes.String)) typename = "STRING";
//                else if (f.FieldType == typeof(CLIPSNet.DataTypes.Integer)) typename = "INTEGER";
//                else if (f.FieldType == typeof(CLIPSNet.DataTypes.Symbol)) typename = "SYMBOL";
//                s.AppendFormat(" (slot {0} (type {1}))", f.Name, typename);
//            }
//        }
//        s.AppendFormat("))");
//    }


//    s.AppendFormat(")");
//    //System.Diagnostics.Debug.WriteLine(s);
//    moduleManager.AssertTemplate(s.ToString(), search_key);         

//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLIPSNet;
using InteractiveCLIPS;

namespace TestModule
{

    public class Person : DefTemplateBase
    {
        public CLIPSNet.DataTypes.String Pippo;
    }

    [ModuleDefinition(ClpFileName = "init.clp", Deftemplates=new Type[]{typeof(Person)})]
    public class Class : Module
    {
        protected override void Init()
        {
        }

        [ClipsAction("Kinect-Run")]
        public CLIPSNet.DataTypes.Integer startApp()
        {
            return new CLIPSNet.DataTypes.Integer(0);
        }        

        [ClipsAction("AddPerson")]
        public CLIPSNet.DataTypes.String testPerson(CLIPSNet.DataTypes.Integer i, CLIPSNet.DataTypes.Integer j)
        {
            Person p = new Person { Pippo = new CLIPSNet.DataTypes.String("Ciao") };
            Assert(p);
            var f = ClipsEnv.AssertString("(nadia beve)");
            return p.Pippo;
        }

        [ClipsAction("Add")]
        public CLIPSNet.DataTypes.Integer test(CLIPSNet.DataTypes.Integer i, CLIPSNet.DataTypes.Integer j)
        {
            ClipsEnv.AssertString("(nadiaAddFact " + i + " " + j + ")");
            return new CLIPSNet.DataTypes.Integer(i.Value + j.Value);
        }
    }
}

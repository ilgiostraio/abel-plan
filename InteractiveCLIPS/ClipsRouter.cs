using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InteractiveCLIPS
{
    class ClipsRouter : CLIPSNet.Router
    {
        private StringBuilder outputClips;
        public StringBuilder Output { 
            get { return outputClips; } 
        }

        private int numlines = 0;

        private int limitLines = 100000;
        public int LimitLines
        {
            get { return limitLines; }
            set { limitLines = value; }
        }

        public ClipsRouter(CLIPSNet.Environment env, string name, int priority) : base(env, name, priority) 
        {
            
            outputClips = new StringBuilder();
        }
        
        protected override bool Print(string name, string str)
        {
            //if (str != "\n" && str.Trim() != string.Empty)
            //    outputClips.AppendLine();



            if (numlines >= limitLines)
            {
                while (numlines > limitLines)
                {
                    //int i = Convert.ToString(outputClips).Split('\n').FirstOrDefault().Length + 1; 
           
                    outputClips.Remove(0, Convert.ToString(outputClips).Split('\n').FirstOrDefault().Length + 1);
                    numlines= numlines-1;
                }
            
            }

            outputClips.Append(str);
            if (str == "\n")
                numlines++;

            return true;
        }
        
        protected override bool Query(string name)
        {
            /*
            We want to intercept any I/O requests that the standard interface would handle. In
            addition, we also need to handle requests for the logical name top. The recognizer
            function for our router is defined below.
            */
            if ((name.CompareTo("stdin") == 0) || (name.CompareTo("stdout") == 0) || (name.CompareTo("wprompt") == 0)
                || (name.CompareTo("wdisplay") == 0) || (name.CompareTo("wdialog") == 0) || (name.CompareTo("werror") == 0)
                || (name.CompareTo("wwarning") == 0) || (name.CompareTo("wtrace") == 0) || (name.CompareTo("wclips") == 0)               
                || (name.CompareTo("t") == 0) || (name.CompareTo("RouterTest") == 0))
            { 
                return true; 
            } 
            else
            { 
                return false; 
            }
        }

        protected override int Getc(string name)
        {
            return base.Getc(name);
        }

        protected override void Exit(int exitCode)
        {
            base.Exit(exitCode);
        }
    }
}
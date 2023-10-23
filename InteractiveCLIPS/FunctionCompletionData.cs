using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace InteractiveCLIPS
{
    public class FunctionCompletionData : ICompletionData
    {
        public double Priority { 
            get; 
            private set; 
        }

        public string Text { 
            get; 
            private set; 
        }

        public FunctionCompletionData(string descritption, string returnType, string name, string[] args)
        {
            this.Text = name;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }        

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return "Description for " + this.Text; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
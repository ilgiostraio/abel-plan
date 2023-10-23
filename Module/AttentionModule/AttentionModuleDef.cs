using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CLIPSNet;
using InteractiveCLIPS;

using System.Timers;
using System.Threading;
using System.Runtime.InteropServices;

using Sense.Lib.FACELibrary;
using YarpManagerCS;

using System.Threading.Tasks;
using System.Drawing;

namespace AttentionModule
{
    [ModuleDefinition(ClpFileName = "init.clp")]
    public class AttentionModuleDef : Module, IDisposable
    {
        private YarpPort lookAtEyesPort;  //porta per orientare gli occhi rispetto al piano della testa (si aggancia a NetView.cs di ACT_FACE)      ALTILIA

        private YarpPort lookAtPort;  //porta per orientare la testa rispetto al piano della kinect (si aggancia a ROS tramite yarpROSbridge)
        private YarpPort expressionPort;
        private YarpPort SceneReceiverPort;
        private YarpPort speechPort;
        private YarpPort feedBackSpeechPort;
        private YarpPort moodPort;
        private YarpPort posturePort;
        private YarpPort neckPort;   //porta per inviare posture (string) via yarpROSbridge al collo
        private YarpPort sentenceTxtPort;
        private YarpPort sentenceAudioPort;
        private YarpPort sentenceEmotionPort;
        private YarpPort sentencePosePort;
        private YarpPort peoplePort;
        private YarpPort contextPort;
        private YarpPort speechStatusPort;
        private YarpPort sentenceChangedPort;  //porta per inviare la frase modificata dal brain

        private Scene sceneData;
        private string received = "";
        private string receivedFeedBack = "";
        private string receivedSentenceEmo = "";
        private string receivedSentencePose = "";
        private string receivedSentenceText = "";
        private string receivedSentenceAudio64 = "";

        //private string context = "";
        //private string sentenceChanged = "";


        private Winner WinnerSub;
        private Winner WinnerSubOld;

        private FaceExpression exp;
        private FaceExpression expOld;

        private peopleInfo peopleInfo;

        private Thread _worker;

        public static CLIPSNet.UserFunction uf_lookat;
        public static CLIPSNet.UserFunction uf_makeexp;
        public static CLIPSNet.UserFunction uf_speech;
        public static CLIPSNet.UserFunction uf_mood;
        public static CLIPSNet.UserFunction uf_posture;
        public static CLIPSNet.UserFunction uf_neck;
        public static CLIPSNet.UserFunction uf_people;
        public static CLIPSNet.UserFunction uf_context;
        public static CLIPSNet.UserFunction uf_speechstatus;
        public static CLIPSNet.UserFunction uf_sentencechanged;

        delegate CLIPSNet.DataTypes.Integer lookAtDelegate(CLIPSNet.DataTypes.Integer id, CLIPSNet.DataTypes.Double x, CLIPSNet.DataTypes.Double y, CLIPSNet.DataTypes.Double z);        //aggiunta anche zCoord       ALTILIA
        delegate CLIPSNet.DataTypes.Integer makeExpressionDelegate(CLIPSNet.DataTypes.Double v, CLIPSNet.DataTypes.Double a);
        delegate CLIPSNet.DataTypes.Integer speechDelegate(CLIPSNet.DataTypes.Integer id);
        delegate CLIPSNet.DataTypes.Integer moodDelegate(CLIPSNet.DataTypes.Double v, CLIPSNet.DataTypes.Double a);
        delegate CLIPSNet.DataTypes.Integer postureDelegate(CLIPSNet.DataTypes.String posture, CLIPSNet.DataTypes.Integer duration);
        delegate CLIPSNet.DataTypes.Integer neckDelegate(CLIPSNet.DataTypes.String neck, CLIPSNet.DataTypes.Integer duration);
        delegate CLIPSNet.DataTypes.Integer peopleDelegate(CLIPSNet.DataTypes.Integer numberOfPeople, CLIPSNet.DataTypes.Integer WinnerID, CLIPSNet.DataTypes.String Exp, CLIPSNet.DataTypes.Integer Age, CLIPSNet.DataTypes.String Gender);  // per lo speech
        delegate CLIPSNet.DataTypes.Integer contextDelegate(CLIPSNet.DataTypes.String context);
        delegate CLIPSNet.DataTypes.Integer speechStatusDelegate(CLIPSNet.DataTypes.Integer speechstatus);
        delegate CLIPSNet.DataTypes.Integer sentenceChangedDelegate(CLIPSNet.DataTypes.String sentenceChanged);


        private System.Timers.Timer checkPortTimer = new System.Timers.Timer();
        //private bool sceneAnalyzerPortExists = false;
        private bool sceneAnalyzerConnectionExists = false;
        private bool sentenceAudioPortConnectionExists = false;

        string WinnerXml;
        string expressionXml;

        public string peopleInfoXml;

        float Xmax = 0;
        float X = 0;

        float Ymax = 0;
        float Y = 0;

        float Zmax = 4.5f;
        float Z = 0;              //      ALTILIA

        bool close = false;

        protected override void Init()
        {
            uf_lookat = new CLIPSNet.UserFunction(ClipsEnv, new lookAtDelegate(FunLookAt), "fun_lookat");
            uf_makeexp = new CLIPSNet.UserFunction(ClipsEnv, new makeExpressionDelegate(FunMakeExp), "fun_makeexp");
            uf_speech = new CLIPSNet.UserFunction(ClipsEnv, new speechDelegate(FunSpeech), "fun_speech");
            uf_mood= new CLIPSNet.UserFunction(ClipsEnv, new moodDelegate(MoodSpeech), "fun_mood");
            uf_posture = new CLIPSNet.UserFunction(ClipsEnv, new postureDelegate(FunPosture), "fun_posture");
            uf_neck = new CLIPSNet.UserFunction(ClipsEnv, new neckDelegate(FunNeckPosture), "fun_neck_posture");
            uf_people = new CLIPSNet.UserFunction(ClipsEnv, new peopleDelegate(FunPeopleInfo), "fun_people_info");
            uf_context = new CLIPSNet.UserFunction(ClipsEnv, new contextDelegate(FunContext), "fun_context");
            uf_speechstatus = new CLIPSNet.UserFunction(ClipsEnv, new speechStatusDelegate(FunSpeechStatus), "fun_speechstatus");
            uf_sentencechanged = new CLIPSNet.UserFunction(ClipsEnv, new sentenceChangedDelegate(FunSentenceChanged), "fun_sentence_changed");


            InitYarp();


            ThreadPool.QueueUserWorkItem(receiveSentenceAudio_Elapsed);
            ThreadPool.QueueUserWorkItem(receiveDataTimer_Elapsed);
            ThreadPool.QueueUserWorkItem(receiveFeedBackSpeech_Elapsed);
            ThreadPool.QueueUserWorkItem(receiveSentenceEmotion_Elapsed);
            ThreadPool.QueueUserWorkItem(receiveSentencePose_Elapsed);
            ThreadPool.QueueUserWorkItem(receiveSentenceText_Elapsed);
            //ThreadPool.QueueUserWorkItem(receiveSentenceText_Elapsed);




            WinnerSub = new Winner();
            exp = new FaceExpression();

            peopleInfo = new peopleInfo();

        }

        private void InitYarp()
        {

            lookAtEyesPort = new YarpPort();
            lookAtEyesPort.openSender("/AttentionModule/LookAtEyes:o");


            lookAtPort = new YarpPort();
            lookAtPort.openSender("/AttentionModule/LookAt:o");

            speechPort = new YarpPort();
            speechPort.openSender("/AttentionModule/Speech:o");

            expressionPort = new YarpPort();
            expressionPort.openSender("/AttentionModule/ECS:o");

            moodPort = new YarpPort();
            moodPort.openSender("/AttentionModule/mood:o");

            posturePort = new YarpPort();
            posturePort.openSender("/AttentionModule/Posture:o");

            neckPort = new YarpPort();
            neckPort.openSender("/AttentionModule/Neck:o");

            peoplePort = new YarpPort();
            peoplePort.openSender("/AttentionModule/People:o");

            contextPort = new YarpPort();
            contextPort.openSender("/AttentionModule/SendContext:o");

            sentenceChangedPort = new YarpPort();
            sentenceChangedPort.openSender("/AttentionModule/SendSentenceChanged:o");

            speechStatusPort = new YarpPort();
            speechStatusPort.openSender("/AttentionModule/SpeechStatus:o");

            feedBackSpeechPort = new YarpPort();
            feedBackSpeechPort.openReceiver("/RobotSpeech/FeedBackSpeech:o", "/InteractiveCLIPS/FeedBackSpeech:i");

            sentenceTxtPort = new YarpPort();
            sentenceTxtPort.openReceiver("/AbelServer/SentenceTxt:o", "/InteractiveCLIPS/SentenceTxt:i");

            sentenceAudioPort = new YarpPort();
            sentenceAudioPort.openReceiver("/AbelServer/SentenceAudio:o", "/InteractiveCLIPS/SentenceAudio:i");

            sentenceEmotionPort = new YarpPort();
            sentenceEmotionPort.openReceiver("/AbelServer/VoiceEmotion:o", "/InteractiveCLIPS/VoiceEmotion:i");

            sentencePosePort = new YarpPort();
            sentencePosePort.openReceiver("/AbelServer/Pose:o", "/InteractiveCLIPS/Pose:i");

            SceneReceiverPort = new YarpPort();
            SceneReceiverPort.openReceiver("/SceneAnalyzer/MetaSceneXML:o", "/InteractiveCLIPS/MetaSceneXML:i");


            if (SceneReceiverPort.PortExists("/SceneAnalyzer/MetaSceneXML:o"))
            {
                ClipsEnv.PrintRouter("RouterTest", "Connection with SENSE created! \n");
                System.Diagnostics.Debug.WriteLine("");
                sceneAnalyzerConnectionExists = true;
                                         

            }
            else 
            {

                ClipsEnv.PrintRouter("RouterTest", "Connection NOT created! /SceneAnalyzer/MetaSceneXML:o port does not exist! \n");

            }

            if (sentenceAudioPort.PortExists("/AbelServer/SentenceAudio:o")) 
            {
                ClipsEnv.PrintRouter("RouterTest", "Connection with Sentence AUDIO created! \n");
                System.Diagnostics.Debug.WriteLine("");
                sentenceAudioPortConnectionExists = true;


            }
            else
            {

                ClipsEnv.PrintRouter("RouterTest", "Connection NOT created! /AbelServer/SentenceAudio:o port do not exist! \n");

            }


        }

        //DOBBIAMO FARE IN MODO CHE ANCHE QUESTO MODULO UNA VOLTA CHIUSO CHIAMI LA DISCONNECT SULLA PORTA YARP UTILIZZATA

        [ClipsAction("fun_lookat")]
        public CLIPSNet.DataTypes.Integer FunLookAt(CLIPSNet.DataTypes.Integer id, CLIPSNet.DataTypes.Double xCoord, CLIPSNet.DataTypes.Double yCoord, CLIPSNet.DataTypes.Double zCoord)      //aggiunta anche zCoord       ALTILIA
        {
            //System.Diagnostics.Debug.WriteLine("WINNER: " + (int)id.Value + " x: " + ((float)xCoord.Value) + " - y: " + ((float)yCoord.Value));
             
                        
            if(id.Value!=1)
            {
                foreach (Subject subject in sceneData.Subjects) //calibration of the LookAt point
                {
                    if(id.Value==subject.idKinect)
                    {
                        //The field of view for the color camera is 84.1 degrees horizontally and 53.8 degrees vertically.
                        //For the depth camera it's 70.6 degrees horizontally and 60 degrees vertically.

                        Xmax = subject.head.Z * (float)Math.Tan(42.05 / 180.00 * Math.PI); //alzando l'angolo della tangente aumento l'eccentricità (42.05°)
                        X = subject.head.X / Xmax / 2 + (float)0.5; //aggiustamento per rappresentazione grafica

                        Ymax = subject.head.Z * (float)Math.Tan(26.9 / 180.00 * Math.PI); //alzando l'angolo della tangente aumento l'eccentricità (26.9°)
                        Y = subject.head.Y / Ymax / 2 + (float)0.5; //aggiustamento per rappresentazione grafica
                        //Y_round = Math.Round(Y, 2);

                        if (Z <= Zmax)
                            Z = subject.head.Z / Zmax; // coord Z per convergenza oculare    
                        else                            //condizione max range + normalizzazione 
                            Z = 1.0f;               

                        
                        WinnerSub.id = (int)id.Value;
                        WinnerSub.spinX = X;
                        WinnerSub.spinY = Y;
                        WinnerSub.spinZ = Z;     
                    }
                }

                
            }         
            else
            {
                WinnerSub.id = (int)id.Value;
                WinnerSub.spinX = (float)xCoord.Value;
                WinnerSub.spinY = (float)yCoord.Value;
                WinnerSub.spinZ = (float)zCoord.Value;   
            }

            if (WinnerSubOld != null)
            {
                if (!(WinnerSub.spinX == WinnerSubOld.spinX && WinnerSub.spinY == WinnerSubOld.spinY && WinnerSub.spinZ == WinnerSubOld.spinZ))      // aggiunta stessa condizione anche su Z      ALTILIA
                {
                    WinnerXml = ComUtils.XmlUtils.Serialize<Winner>(WinnerSub);
                    lookAtPort.sendData(WinnerXml);
                    lookAtEyesPort.sendData(WinnerXml);
                    WinnerSubOld = (Winner)WinnerSub.Clone();
                    WinnerXml = "";
                }
            }
            else
            {
                WinnerSubOld = new Winner();
                WinnerSubOld = (Winner)WinnerSub.Clone();
            }
           
          

            return id;
        }

        int n = 0;
        [ClipsAction("fun_makeexp")]
        public CLIPSNet.DataTypes.Integer FunMakeExp(CLIPSNet.DataTypes.Double v, CLIPSNet.DataTypes.Double a)
        {
            //System.Diagnostics.Debug.WriteLine("ECS -> Valence = " + v.ToString() + " - Arousal = " + a.ToString());

            if (!close)
                if (expOld != null)
                {
                    if (!((float)v.Value == expOld.valence && a.Value == expOld.arousal))
                    {
                        exp.valence = (float)v.Value;
                        exp.arousal = (float)a.Value;

                        expressionXml = ComUtils.XmlUtils.Serialize<FaceExpression>(exp);
                        expressionPort.sendData(expressionXml);
                        expOld = (FaceExpression)exp.Clone();
                        expressionXml = "";
                    }
                }
                else
                {
                    expOld = new FaceExpression();
                    expOld = (FaceExpression)exp.Clone();
                }

            

            return new CLIPSNet.DataTypes.Integer(0);
        }

        [ClipsAction("fun_speech")]
        public CLIPSNet.DataTypes.Integer FunSpeech(CLIPSNet.DataTypes.Integer id)
        {
            if (!close)
                speechPort.sendData(id.ToString());

            return id;
        }

        [ClipsAction("fun_speechstatus")]
        public CLIPSNet.DataTypes.Integer FunSpeechStatus(CLIPSNet.DataTypes.Integer speechStatus)
        {
            if (!close)
                if (speechStatus.ToString() == "1")
                {
                    speechStatusPort.sendData("end");
                }

            return new CLIPSNet.DataTypes.Integer(101);
        }

        [ClipsAction("fun_context")]
        public CLIPSNet.DataTypes.Integer FunContext(CLIPSNet.DataTypes.String context)
        {
            if (!close)
                contextPort.sendData(Convert.ToString(context));

            return new CLIPSNet.DataTypes.Integer(101);
        }

        [ClipsAction("fun_sentence_changed")]
        public CLIPSNet.DataTypes.Integer FunSentenceChanged(CLIPSNet.DataTypes.String sentenceChanged)
        {
            if (!close)
                sentenceChangedPort.sendData(Convert.ToString(sentenceChanged));

            return new CLIPSNet.DataTypes.Integer(10221);
        }

        [ClipsAction("fun_mood")]
        public CLIPSNet.DataTypes.Integer MoodSpeech(CLIPSNet.DataTypes.Double v, CLIPSNet.DataTypes.Double a)
        {
            if(!close)
                moodPort.sendData(new List<float>() { (float)v.Value, (float)a.Value });

            return new CLIPSNet.DataTypes.Integer(0);
        }

        [ClipsAction("fun_posture")]
        public CLIPSNet.DataTypes.Integer FunPosture(CLIPSNet.DataTypes.String posture, CLIPSNet.DataTypes.Integer duration)
        {
            if (!close)
                posturePort.sendData(Convert.ToString(posture));   

            return new CLIPSNet.DataTypes.Integer(1000);
        }


        [ClipsAction("fun_neck_posture")]
        public CLIPSNet.DataTypes.Integer FunNeckPosture(CLIPSNet.DataTypes.String neck, CLIPSNet.DataTypes.Integer duration)
        {
            if (!close)
                neckPort.sendData(Convert.ToString(neck));   

            return new CLIPSNet.DataTypes.Integer(50);
        }

        [ClipsAction("fun_people_info")]
        public CLIPSNet.DataTypes.Integer FunPeopleInfo(CLIPSNet.DataTypes.Integer numberOfPeople, CLIPSNet.DataTypes.Integer WinnerID, 
                                    CLIPSNet.DataTypes.String Exp, CLIPSNet.DataTypes.Integer Age, CLIPSNet.DataTypes.String Gender)
        {
            if (!close)
            {
                peopleInfo.numberOfPeople = (int)numberOfPeople.Value;
                peopleInfo.WinnerID = (int)WinnerID.Value;
                peopleInfo.Exp = (string)Exp.Value;
                peopleInfo.Age = (int)Age.Value;
                peopleInfo.Gender = (string)Gender.Value;

                peopleInfoXml = ComUtils.XmlUtils.Serialize<peopleInfo>(peopleInfo);

                peoplePort.sendData(peopleInfoXml);
            }
               
            return new CLIPSNet.DataTypes.Integer(50);
        }



        #region Yarp


        /* Asserts managed by a timer */
        void receiveDataTimer_Elapsed(object sender)
        {
            while (!close)
            {
                
                SceneReceiverPort.receivedData(out received);
                if (received != null && received != "")
                {

                    sceneData = ComUtils.XmlUtils.Deserialize<Scene>(received);

                    Parallel.ForEach(sceneData.Subjects, (subject) =>
                    {
                        StringBuilder sObj = convertedObject(typeof(Subject), subject, (subject.idKinect).ToString());
                        AssertTemplate(sObj.ToString(), (subject.idKinect).ToString());
                        //AssertTemplate(subject.ToStringClips(), (subject.idKinect).ToString());

                    });



                    //StringBuilder s = convertedObject(typeof(Surroundings), sceneData.Environment, "0");
                    //System.Diagnostics.Debug.WriteLine(s.ToString());
                    AssertTemplate(convertedObject(typeof(Surroundings), sceneData.Environment, "0").ToString(), "0");
                    //AssertTemplate(typeof(Surroundings), sceneData.Environment, "0");
                    //AssertFact("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //sceneData = null;


                }
            }
        }

        void receiveFeedBackSpeech_Elapsed(object sender)
        {
            string start_value = "start";
            string end_value = "end";
            while (!close)
            {
                feedBackSpeechPort.receivedData(out receivedFeedBack);
                if (receivedFeedBack.Contains(start_value))
                {
                    AssertFact("speak", "start");
                }
                else if (receivedFeedBack.Contains(end_value))
                {
                    AssertFact("speak", "end");
                }
            }
        }

        void receiveSentenceEmotion_Elapsed(object sender)
        {
    
            while (!close)
            {
                sentenceEmotionPort.receivedData(out receivedSentenceEmo);
                AssertFact("sentence-emotion-is", receivedSentenceEmo);
            }
        }

        void receiveSentencePose_Elapsed(object sender)
        {

            while (!close)
            {
                sentencePosePort.receivedData(out receivedSentencePose);
                AssertFact("sentence-pose-is", receivedSentencePose);
            }
        }

        void receiveSentenceText_Elapsed(object sender)
        {
            while (!close)
            {
                sentenceTxtPort.receivedData(out receivedSentenceText);
                if (receivedSentenceText != null && receivedSentenceText != "")
                    {
                    //AssertFact("sentence-text-is", receivedSentenceText);
                    sentenceChangedPort.sendData(Convert.ToString(receivedSentenceText));

                }  
            }
        }

        void receiveSentenceAudio_Elapsed(object sender)
        {
            while (!close)
            {
                sentenceAudioPort.receivedData(out receivedSentenceAudio64);
                if (receivedSentenceAudio64 != null && receivedSentenceAudio64 != "")
                    {
                    receivedSentenceAudio64 = receivedSentenceAudio64.Remove(0, 1);
                    receivedSentenceAudio64 = receivedSentenceAudio64.Remove(receivedSentenceAudio64.Length - 1, 1);  
                    //File.WriteAllText("C:\\Users\\FACETeam\\Desktop\\WriteText.txt", receivedSentenceAudio64);
                    byte[] data = Convert.FromBase64String(receivedSentenceAudio64);
                    File.WriteAllBytes("C:\\Users\\FACETeam\\Documents\\GitRepo\\Speech\\audio\\0.wav", data);
                    AssertFact("dai", "parla-cazzo");
                        
                }
            }
        }
        #endregion


        protected StringBuilder convertedObject(Type t, object obj, string search_key)
        {
            var convertedObject = Convert.ChangeType(obj, t);
            StringBuilder s = new StringBuilder();

            s.Append("(" + t.Name.ToString().ToLower() + " \n");

            if (t.Name == "Subject")
            {
                #region sub
                foreach (System.Reflection.PropertyInfo prop in t.GetProperties())
                {
                    object val = typeof(Subject).GetProperty(prop.Name).GetValue(convertedObject, null);
                    if (val != null)
                    {
                        StringBuilder sbGeneric = new StringBuilder();
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {

                            System.Collections.IList l = (System.Collections.IList)val;
                            sbGeneric.AppendFormat("({0}", prop.Name);
                            foreach (object elem in l)
                            {
                                if (elem.ToString() != null)
                                    sbGeneric.AppendFormat(" {0}", elem.ToString());
                            }
                            sbGeneric.AppendFormat(")\n");

                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Limb")
                        {


                            Limb li = (Limb)val;

                            sbGeneric.AppendFormat(" ({0}Left", prop.Name);
                            sbGeneric.AppendFormat(" ");
                            sbGeneric.AppendFormat(" {0}", li.left.X.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", li.left.Y.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", li.left.Z.ToString("F3"));
                            sbGeneric.AppendFormat(")\n");

                            sbGeneric.AppendFormat(" ({0}Right", prop.Name);
                            sbGeneric.AppendFormat(" {0}", li.right.X.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", li.right.Y.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", li.right.Z.ToString("F3"));
                            sbGeneric.AppendFormat(")\n");

                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Position")
                        {
                            Position pos = (Position)val;
                            sbGeneric.AppendFormat("({0}", prop.Name);

                            sbGeneric.AppendFormat(" {0}", pos.X.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", pos.Y.ToString("F3"));
                            sbGeneric.AppendFormat(" {0}", pos.Z.ToString("F3"));
                            sbGeneric.AppendFormat(")\n");
                        }
                        else
                        {
                            sbGeneric.AppendFormat("({0} {1})\n", prop.Name, val.ToString());
                        }
                        s.Append(sbGeneric.ToString());
                    }

                }
                #endregion
            }
            else if (t.Name == "Surroundings")
            {
                #region surroundings
                foreach (System.Reflection.PropertyInfo prop in t.GetProperties())
                {
                    object val = typeof(Surroundings).GetProperty(prop.Name).GetValue(convertedObject, null);
                    if (val != null)
                    {
                        StringBuilder sbGen = new StringBuilder();
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            System.Collections.IList l = (System.Collections.IList)val;
                            sbGen.AppendFormat(" ({0} ", prop.Name);
                            foreach (object elem in l)
                            {
                                if (elem.ToString() != null)
                                {
                                    if (elem.ToString().Length > 4)
                                        sbGen.AppendFormat(" {0}", elem.ToString().Substring(0, 4));
                                    else
                                        sbGen.AppendFormat(" {0}", elem.ToString());
                                }
                            }
                            sbGen.AppendFormat(")\n");
                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "SOME")
                        {
                            SOME some = (SOME)val;

                            sbGen.AppendFormat(" ({0}Mic", prop.Name);
                            sbGen.AppendFormat(" {0})\n", some.mic.ToString());
                            sbGen.AppendFormat(" ({0}Lux", prop.Name);
                            sbGen.AppendFormat(" {0})\n", some.lux.ToString());
                            sbGen.AppendFormat(" ({0}Temp", prop.Name);
                            sbGen.AppendFormat(" {0})\n", some.temp.ToString());
                            sbGen.AppendFormat(" ({0}IR", prop.Name);
                            sbGen.AppendFormat(" {0})\n", some.IR.ToString());
                            sbGen.AppendFormat(" ({0}Touch", prop.Name);
                            sbGen.AppendFormat(" {0})\n", some.touch.ToString());


                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Saliency")
                        {
                            Saliency sal = (Saliency)val;
                            sbGen.AppendFormat(" ({0}", prop.Name);
                            if (sal.position.Count != 0)
                            {
                                sbGen.AppendFormat(" {0}", sal.position[0].ToString("F2"));
                                sbGen.AppendFormat(" {0}", sal.position[1].ToString("F2"));
                                sbGen.AppendFormat(" {0}", sal.saliencyWeight.ToString("F1"));
                            }
                            sbGen.AppendFormat(")\n ");
                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Ambience")
                        {
                            sbGen.AppendFormat(" ({0}", prop.Name);
                            sbGen.AppendFormat(")\n");
                        }
                        else if (prop.PropertyType.IsClass && prop.PropertyType.Name == "Resolution")
                        {
                            Resolution res = (Resolution)val;
                            sbGen.AppendFormat(" ({0}", prop.Name);
                            sbGen.AppendFormat(" {0}", res.Width.ToString("F1"));
                            sbGen.AppendFormat(" {0}", res.Height.ToString("F1"));
                            sbGen.AppendFormat(")\n ");
                        }
                        else
                        {
                            sbGen.AppendFormat(" ({0} {1})\n", prop.Name, val.ToString());
                        }
                        s.Append(sbGen.ToString());


                    }
                }
                #endregion
            }
            else
            {
                s.AppendFormat("({0} (", t.Name);
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
            }


            s.AppendFormat(")");
            //System.Diagnostics.Debug.WriteLine(s);
            return s;

        }

        public override void YarpClose() 
        {
            close = true;

            if (lookAtEyesPort != null)
                lookAtEyesPort.Close();
            if (lookAtPort != null)
                lookAtPort.Close();
            if (expressionPort != null)
                expressionPort.Close();
            if (SceneReceiverPort != null)
                SceneReceiverPort.Close();
            if (speechPort != null)
                speechPort.Close();
            if (feedBackSpeechPort != null)
                feedBackSpeechPort.Close();
            if (moodPort != null)
                moodPort.Close();
            if (neckPort != null)
                neckPort.Close();
            if (posturePort != null)
                posturePort.Close();
            if (sentenceAudioPort != null)
                sentenceAudioPort.Close();
            if (sentenceTxtPort != null)
                sentenceTxtPort.Close();
            if (sentenceEmotionPort != null)
                sentenceEmotionPort.Close();

        }

        


    }
}




/*
 //(assert-string "(primary color is red)")
        //private void assertTemplate(Type t)
        private List<string> assertTemplate(Scene t)
        {
            List<string> assertSubjects = new List<string>();
            for (int i = 0; i < t.Subjects.Count; i++)
            {
                Subject currSubject = t.Subjects[i];
                StringBuilder s = new StringBuilder();
                s.Append("(subj ");
                foreach (System.Reflection.PropertyInfo prop in typeof(Subject).GetProperties())
                {
                    //Console.WriteLine("{0} = {1}", prop.Name, prop.GetValue(t, null));
                    //if (prop.GetType() == typeof(String))
                    //    s.AppendFormat("({0} \"{1}\")", prop.Name, currSubject.GetType().GetProperty(prop.Name).GetValue(currSubject, null));
                    //else
                    object val = currSubject.GetType().GetProperty(prop.Name).GetValue(currSubject, null);
                    if ( val.ToString() == "")                        
                        s.AppendFormat("({0} {1})", prop.Name, "-");
                    else
                        s.AppendFormat("({0} {1})", prop.Name, val);
                }
                s.AppendFormat(")");
                assertSubjects.Add(s.ToString());
            }
            return assertSubjects;
        }
 */


//if (xCoord is CLIPSNet.DataTypes.GenericDataType<float> && yCoord is CLIPSNet.DataTypes.GenericDataType<float>)
//{
//    var x = (CLIPSNet.DataTypes.GenericDataType<float>)xCoord;
//    var y = (CLIPSNet.DataTypes.GenericDataType<float>)yCoord;
//    try
//    {
//        System.Diagnostics.Debug.WriteLine("x: " + x.ToString() + " - y: " + y.ToString()); 
//    }
//    catch (Exception e) { Console.WriteLine(e.ToString()); }
//}
//else 
//{ 
//    System.Diagnostics.Debug.WriteLine("x: " + xCoord.ToString() + " - y: " + yCoord.ToString()); 
//}
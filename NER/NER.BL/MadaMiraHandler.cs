using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;

namespace NER.BL
{
    public class MadaMiraHandler
    {

        public static List<WordInfoItem> Analyse(string sentence)
        {
            string ApplicationPath = @"C:\Users\Shaza\Desktop\a\";
            string text = System.IO.File.ReadAllText(ApplicationPath + "Template.txt");
            text = text.Replace("@Word", sentence);
            System.IO.File.WriteAllText(ApplicationPath + @"Test.xml", text);
            ExecuteCommandSync(@"java -Xmx2500m -Xms2500m -XX:NewRatio=3 -jar " + ApplicationPath + @"MADAMIRA.jar -i " + ApplicationPath + @"Test.xml  -o  " + ApplicationPath + @"Out.xml");

            var xmldoc = new XmlDataDocument();
            XmlNodeList xmlnode;
            int i = 0;
            FileStream fs = new FileStream(ApplicationPath + "Out.xml", FileMode.Open, FileAccess.Read);
            try
            {
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName("word");

                var result = new List<WordInfoItem>();
                for (i = 0; i <= xmlnode.Count - 1; i++)
                {
                    var InfoElement = xmlnode[i].ChildNodes[0].ChildNodes[0];

                    var item = new WordInfoItem();
                    item.ID = int.Parse(xmlnode[i].Attributes["id"].Value);
                    item.word = xmlnode[i].Attributes["word"].Value;
                    item.diac = InfoElement.Attributes["diac"] != null ? InfoElement.Attributes["diac"].Value : string.Empty;
                    item.lemma = InfoElement.Attributes["lemma"] != null ? InfoElement.Attributes["lemma"].Value : string.Empty;
                    item.bw = InfoElement.Attributes["bw"] != null ? InfoElement.Attributes["bw"].Value : string.Empty;
                    item.gloss = InfoElement.Attributes["gloss"] != null ? InfoElement.Attributes["gloss"].Value : string.Empty;
                    item.pos = InfoElement.Attributes["pos"] != null ? InfoElement.Attributes["pos"].Value : string.Empty;
                    item.prc3 = InfoElement.Attributes["prc3"] != null ? InfoElement.Attributes["prc3"].Value : string.Empty;
                    item.prc2 = InfoElement.Attributes["prc2"] != null ? InfoElement.Attributes["prc2"].Value : string.Empty;
                    item.prc1 = InfoElement.Attributes["prc1"] != null ? InfoElement.Attributes["prc1"].Value : string.Empty;
                    item.prc0 = InfoElement.Attributes["prc0"] != null ? InfoElement.Attributes["prc0"].Value : string.Empty;
                    item.per = InfoElement.Attributes["per"] != null ? InfoElement.Attributes["per"].Value : string.Empty;
                    item.asp = InfoElement.Attributes["asp"] != null ? InfoElement.Attributes["asp"].Value : string.Empty;
                    item.vox = InfoElement.Attributes["vox"] != null ? InfoElement.Attributes["vox"].Value : string.Empty;
                    item.mod = InfoElement.Attributes["mod"] != null ? InfoElement.Attributes["mod"].Value : string.Empty;
                    item.gen = InfoElement.Attributes["gen"] != null ? InfoElement.Attributes["gen"].Value : string.Empty;
                    item.num = InfoElement.Attributes["num"] != null ? InfoElement.Attributes["num"].Value : string.Empty;
                    item.stt = InfoElement.Attributes["stt"] != null ? InfoElement.Attributes["stt"].Value : string.Empty;
                    item.cas = InfoElement.Attributes["cas"] != null ? InfoElement.Attributes["cas"].Value : string.Empty;
                    item.enc0 = InfoElement.Attributes["enc0"] != null ? InfoElement.Attributes["enc0"].Value : string.Empty;
                    item.source = InfoElement.Attributes["source"] != null ? InfoElement.Attributes["source"].Value : string.Empty;
                    item.stem = InfoElement.Attributes["stem"] != null ? InfoElement.Attributes["stem"].Value : string.Empty;
                    result.Add(item);

                }

                return result;
            }
            catch {return  new List<WordInfoItem>(); }


        }
        public static void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        public static void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }



    }
}

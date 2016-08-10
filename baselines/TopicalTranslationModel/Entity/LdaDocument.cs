using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace GibbsLDA.Entity
{
    [Serializable]
    public class LdaDocument
    {
        public int[] doc;
        /// <summary>
        /// queryWordTopic[i] denotes the topic that query[i] is assigned
        /// </summary>
        public int[] wordTopic;
        public bool[] iswordbackground;
        public int[] tag;

        public void BinarySerialize(string outputFilePath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (fs != null)
            {
                fs.Close();
            }
        }

        public static LdaDocument BinaryDeSerialize(string inputFilePath)
        {
            FileStream fs = null;
            LdaDocument qaDocument = null;

            try
            {
                fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFormatter bf = new BinaryFormatter();
                qaDocument = (LdaDocument)bf.Deserialize(fs);
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (fs != null) { fs.Close(); }
            return qaDocument;
        }

        public void XMLSerialize(string outputFilePath)
        {
            FileStream fs = null;

            try
            {
                fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                XmlSerializer xs = new XmlSerializer(typeof(LdaDocument));
                xs.Serialize(fs, this);
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (fs != null) { fs.Close(); }
        }

        public static LdaDocument XMLDeSerialize(string inputFilePath)
        {
            FileStream fs = null;
            LdaDocument qaDocument = null;

            try
            {
                fs = new FileStream(inputFilePath, FileMode.Create, FileAccess.Read, FileShare.Read);
                XmlSerializer xs = new XmlSerializer(typeof(LdaDocument));
                qaDocument = (LdaDocument)xs.Deserialize(fs);
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (fs != null) { fs.Close(); }
            return qaDocument;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QA.IR.Basic.Entity
{
    public interface ITransDict
    {
        double TransPropBgivenA(string a, string b);
    }
    public class TransDict : ITransDict
    {
        const double ZERO_LOW = -0.00000001;
        const double ZERO_UPPER = 0.00000001;

        Dictionary<string, double>[] transDict = null;
       
        public bool Init(string transDictFilePath)
        {
            bool succ = true;

            if (transDict != null)
                return true;

            if (!File.Exists(transDictFilePath))
                return false;
            transDict = new Dictionary<string, double>[27];
            for (int i = 0; i < 27; i++)
                transDict[i] = new Dictionary<string, double>();
            Console.WriteLine("Loading trans data...");
            StreamReader sr = null;
            try
            {
                string[] splitStr1 = new string[] { "|||" };
                string[] splitStr2 = new string[] { "\t", " " };
                sr = new StreamReader(transDictFilePath);
                string curline = null;
                int count = 0;
                while ((curline = sr.ReadLine()) != null)
                {
                    string[] fields = curline.Split(splitStr1, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 3)
                        continue;
                    fields[0] = fields[0].Trim();
                    fields[1] = fields[1].Trim();
                    string[] values = fields[2].Split(splitStr2, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length < 2)
                        continue;
                    double score = 0;
                    if (double.TryParse(values[0], out score) && !(score > ZERO_LOW && score < ZERO_UPPER))
                    {
                        string key = fields[0] + " " + fields[1];
                        int index = ((int)key[0]) % 27;
                        transDict[index][key] = score;
                    }
                    score = 0;
                    if (double.TryParse(values[1], out score) && !(score > ZERO_LOW && score < ZERO_UPPER))
                    {
                        string key = fields[1] + " " + fields[0];
                        int index = ((int)key[0]) % 27;
                        transDict[index][key] = score;
                    }
                    count++;
                    if (count % 100000 == 0)
                    {
                        Console.Write("{0}\r", count);
                    }
                }
            }
            catch (Exception e)
            {
                transDict = null;
                succ = false;
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Success Loading trans data...");
            if (sr != null)
                sr.Close();
            return succ;
        }

        public double TransPropBgivenA(string a, string b)
        {
            string key = a + " " + b;
            int index = ((int)key[0]) % 27;
            double ret = 0;
            if (!transDict[index].TryGetValue(key, out ret))
                ret = ZERO_UPPER;
            return ret;
        }
    }

    public class TransDict2Dihe : ITransDict
    {
        Dictionary<string, double> mTransDict = null;

        public TransDict2Dihe(string filepath, bool firstVale)
        {
            int labelID = 2;
            if (!firstVale)
                labelID = 3;
            mTransDict = new Dictionary<string, double>();
            try
            {
                HashSet<string> set = new HashSet<string>();
                int maxValue = int.MinValue;
                using (StreamReader sr = new StreamReader(filepath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().ToLower();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        string[] fields = line.Split('\t');
                        if (fields.Length != 4)
                            continue;
                        int value = 0;
                        if (int.TryParse(fields[labelID], out value))
                        {
                            string key = null;
                            if (fields[0].CompareTo(fields[1]) <= 0)
                                key = string.Format("{0}|||{1}", fields[0], fields[1]);
                            else
                                key = string.Format("{0}|||{1}", fields[1], fields[0]);
                            mTransDict[key] = value;
                            if (value > maxValue)
                                maxValue = value;
                            set.Add(key);
                        }
                    }
                }

                foreach (string key in set)
                    mTransDict[key] = mTransDict[key] / (double)maxValue;
            }
            catch (IOException)
            { }
        }

        public double TransPropBgivenA(string a, string b)
        {
            string key = null;
            if (a.CompareTo(b) <= 0)
                key = string.Format("{0}|||{1}", a, b);
            else
                key = string.Format("{0}|||{1}", b, a);
            double value = 0;
            if (!mTransDict.TryGetValue(key, out value))
                return 0;
            return value;
        }
    }
}

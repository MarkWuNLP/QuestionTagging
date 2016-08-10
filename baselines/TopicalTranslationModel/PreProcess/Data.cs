using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GibbsLDA.PreProcess
{
    class MQuestion
    {
        //public string que = null;
        public string[] queTerm = null;
        public string[] desTerm = null;
        public List<string[]> ansTerm = null;

        public MQuestion(string line)
        {
            char[] strSplitChar = new char[] { '\t' };
            char[] wordSplitChar = new char[] { ' ' };
            char[] ansPairSplit = new char[] { ':' };

            string[] terms = line.Split(strSplitChar);
            if (terms.Length < 5)
                return;
            //que
            queTerm = terms[2].Split(wordSplitChar);

            //des
            if (terms[3] != "N/A")
                desTerm = terms[3].Split(wordSplitChar);

            ansTerm = new List<string[]>();
            for (int t = 4; t < terms.Length; t++)
            {
                string[] pairs = terms[t].Split(ansPairSplit, 2);
                if (pairs.Length > 1)
                    ansTerm.Add(pairs[1].Split(wordSplitChar));
            }
            if (ansTerm.Count <= 0)
                ansTerm = null;
        }
    }

    class Data
    {
        public static void PreProcess(string inputDir, string vocabularyFile, string outputfile)
        {
            DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
            FileInfo[] termFiles = inputDirInfo.GetFiles("*Terms.dat");

            PreProcess(termFiles, vocabularyFile, outputfile);
        }
        public static void PreProcess(string inputDir, DataInfo dataInfo, string vocabularyFile, string outputfile)
        {
            List<FileInfo> info = new List<FileInfo>();
            for (int root = dataInfo.startRootID; root <= dataInfo.endRootID; root++)
            {
                for (int leaf = dataInfo.startLeafID; leaf <= dataInfo.endLeafID; leaf++)
                {
                    string fileName = string.Format("cate{0}_C{1}QuestionAnswersTerms.dat", root, leaf);
                    string fullpath = Path.Combine(inputDir, fileName);
                    if (File.Exists(fullpath))
                        info.Add(new FileInfo(fullpath));
                }
            }
            PreProcess(info.ToArray(), vocabularyFile, outputfile);
        }
        public static void PreProcess(FileInfo[] termFiles, string vocabularyFile, string outputfile, int start = -1 , int end = -1)
        {
            int queCount = 0;
            Dictionary<string, string> vocDict = null;
            GetVocabularyDic(vocabularyFile, out vocDict);

            StreamReader sr = null;
            StreamReader sr2 = null;

            StreamWriter sw = null;

            StreamWriter sourceFileWriter = null;

            try
            {
                char[] strSplitChar = new char[] { '\t' };
                char[] wordSplitChar = new char[] { ' ' };
                FileStream fs = new FileStream(outputfile, FileMode.Create);
                sw = new StreamWriter(fs);

                sourceFileWriter = new StreamWriter(new FileStream(outputfile+"_source", FileMode.Create));

                foreach (FileInfo fileinfo in termFiles)
                {
                    Console.WriteLine("[Begin] process {0}", fileinfo.Name);
                    try
                    {
                        sr = new StreamReader(fileinfo.FullName);
                        sr = new StreamReader(fileinfo.FullName + "_source");

                        int mCount = 0;
                        string curLine = null;
                        string curSourceLine = null;
                        while ((curLine = sr.ReadLine()) != null && (curSourceLine = sr2.ReadLine()) != null)
                        {
                            mCount++;
                            if (start > 0 && end > 0 && (mCount < start || mCount > end))
                                continue;
                            MQuestion mQuetion = new MQuestion(curLine);
                            if (mQuetion.ansTerm == null)
                                continue;
                            //que
                            string que = null;
                            foreach (string word in mQuetion.queTerm)
                            {
                                if (vocDict.ContainsKey(word))
                                {
                                    if (que == null)
                                        que = vocDict[word];
                                    else
                                        que += ("|" + vocDict[word]);
                                }
                            }
                            if (que == null)
                                continue;
                            //des
                            string des = "";
                            if (mQuetion.desTerm != null)
                            {
                                foreach (string word in mQuetion.desTerm)
                                {
                                    if (vocDict.ContainsKey(word))
                                        des += ("|" + vocDict[word]);
                                }
                            }
                            if (des != "")
                                que += des;
                            string answers = null;
                            int count = mQuetion.ansTerm.Count;
                            for (int a = 0; a < count; a++)
                            {
                                string answer = null;
                                string[] ansWords = mQuetion.ansTerm[a];
                                foreach (string word in ansWords)
                                {
                                    if (vocDict.ContainsKey(word))
                                    {
                                        if (answer == null)
                                            answer = vocDict[word];
                                        else
                                            answer += ("|" + vocDict[word]);
                                    }
                                }

                                if (answer != null)
                                {
                                    if (answers != null)
                                        answers += ("|" + answer);
                                    else
                                        answers = answer;
                                }
                            }
                            if (answers != null)
                            {
                                sw.WriteLine("{0}|{1}", que, answers);
                                sourceFileWriter.WriteLine(curSourceLine);
                                queCount++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    if (sr != null)
                    {
                        sr.Close();
                        sr = null;
                    }
                    if (sr2 != null)
                        sr2.Close();
                    Console.WriteLine("[End] process {0}", fileinfo.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (sw != null)
                sw.Close();
            if (sourceFileWriter != null)
                sourceFileWriter.Close();

            Console.WriteLine("Total que : {0}", queCount);
        }

        public static int GetVocabularySize(string vocabularyFile)
        {
            //Dictionary<string, string> vocDict = new Dictionary<string, string>();
            int lable = 0;
            StreamReader sr = null;

            try
            {
                char[] splitChar = new char[] { '\t' };
                sr = new StreamReader(vocabularyFile);
                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                    lable++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (sr != null)
                sr.Close();

            return lable;
        }

        public static void GetVocabularyDic(string vocabularyFile, out Dictionary<string, string> vocDict)
        {
            vocDict = new Dictionary<string, string>();
            StreamReader sr = null;

            try
            {
                char[] splitChar = new char[] { '\t' };
                sr = new StreamReader(vocabularyFile);
                string curLine = null;
                while ((curLine = sr.ReadLine()) != null)
                {
                    string[] terms = curLine.Split(splitChar);
                    if (terms.Length < 2)
                        continue;
                    vocDict[terms[0]] = terms[1];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (sr != null)
                sr.Close();
        }

        public struct DataInfo
        {
            public int startRootID;
            public int endRootID;
            public int startLeafID;
            public int endLeafID;
        }
    }
}

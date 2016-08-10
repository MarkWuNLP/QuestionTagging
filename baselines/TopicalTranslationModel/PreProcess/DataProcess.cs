using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using WordBreaker;
using GibbsLDA.Entity;

namespace GibbsLDA.PreProcess
{
    public class DataPreProcess
    {
        Dictionary<string, int> wordIndexDic;
        public  Dictionary<string, int> WordIndexDic { get { return wordIndexDic; } }
        public  int GetVocabularySize() { return wordIndexDic.Count; }

        EnNLPUtility enNLPUtility;
        int currentMaxWordIndex;

        public bool Init(string sourceFileDir, string wordIndexFile = null)
        {
            bool isSuccess = true;

            wordIndexDic = new Dictionary<string, int>();
            currentMaxWordIndex = -1;

            StreamReader sr = null;
            try
            {
                enNLPUtility = new EnNLPUtility(sourceFileDir);
                if (wordIndexFile != null)
                {
                    sr = new StreamReader(wordIndexFile, Encoding.UTF8);
                    string wordIndexLine = null;
                    while ((wordIndexLine = sr.ReadLine()) != null)
                    {
                        var terms = wordIndexLine.Split(new char[] { '\t' });
                        try
                        {
                            int index = int.Parse(terms[1]);
                            if (index > currentMaxWordIndex) { currentMaxWordIndex = index; }
                            wordIndexDic[terms[0]] = index;
                        }
                        catch (Exception e) { Console.WriteLine(e); }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                isSuccess = false;
            }
            if (sr != null)
            {
                sr.Close();
            }

            return isSuccess;
        }

        public bool ConvertWordToInteger(string inputDataDir, string outputDataDir)
        {
            DirectoryInfo inputDirInfo = new DirectoryInfo(inputDataDir);
            FileInfo[] inputFilesInfo = inputDirInfo.GetFiles("*Question.dat");
            foreach (var fileInfo in inputFilesInfo)
            {
                string fileFullName = fileInfo.FullName;
                string answerFullName = fileFullName.Replace("Question.dat", "Answer.dat");
                string encodeName = fileInfo.Name.Replace("Question", "Encode");
                string endoeFullName = Path.Combine(outputDataDir, encodeName);

                if (!ConvertWordToInteger(fileFullName, answerFullName, endoeFullName))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ConvertWordToInteger(string inputQuestionFile, string inputAnswerFile, string outputfile)
        {
            bool isSuccess = true;
            StreamReader srQue = null;
            StreamReader srAns = null;
            StreamWriter sw = null;
            Console.WriteLine("[Begin] encode {0}", inputQuestionFile);
            try
            {
                srQue = new StreamReader(inputQuestionFile, Encoding.UTF8);
                srAns = new StreamReader(inputAnswerFile, Encoding.UTF8);

                FileStream fs = new FileStream(outputfile, FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);

                string questionLine = null;
                string answerLine = null;
                while ((questionLine = srQue.ReadLine()) != null && (answerLine = srAns.ReadLine()) != null)
                {
                    var terms = questionLine.Split(new char[] { '\t' });
                    if (terms.Length < 4) { continue; }
                    string question = terms[2];
                    string des = terms[3].Trim();
                    if (des != "" && des != "N/A") { question += (" " + des); }
                    var queWords = enNLPUtility.WordStemming(question);
                    string outputLine1 = null;
                    foreach (var word in queWords)
                    {
                        int index = -1;
                        if (wordIndexDic.ContainsKey(word))
                        {
                            index = wordIndexDic[word];
                        }
                        else
                        {
                            currentMaxWordIndex++;
                            wordIndexDic[word] = currentMaxWordIndex;
                            index = currentMaxWordIndex;
                        }
                        if (outputLine1 == null)
                        {
                            outputLine1 = index.ToString();
                            continue;
                        }
                        outputLine1 += ("|" + index.ToString());
                    }
                    if (outputLine1 == null)
                    {
                        Console.WriteLine("question is empty");
                        continue;
                    }
                    string outputLine2 = null;
                    var answers = answerLine.Split(new string[] { "|`|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var answer in answers)
                    {
                        var autherAnswer = answer.Split(new char[] { '\t' });
                        //if (autherAnswer == null) { continue; }
                        if (autherAnswer.Length > 1)
                        {
                            var ansWords = enNLPUtility.WordStemming(autherAnswer[1]);
                            if (ansWords == null) { continue; }
                            foreach (var word in ansWords)
                            {
                                int index = -1;
                                if (wordIndexDic.ContainsKey(word))
                                {
                                    index = wordIndexDic[word];
                                }
                                else
                                {
                                    currentMaxWordIndex++;
                                    wordIndexDic[word] = currentMaxWordIndex;
                                    index = currentMaxWordIndex;
                                }
                                if (outputLine2 == null)
                                {
                                    outputLine2 = index.ToString();
                                    continue;
                                }
                                outputLine2 += ("|" + index.ToString());
                            }
                        }
                    }
                    if (outputLine2 == null)
                    {
                        Console.WriteLine("answer is empty");
                        continue;
                    }
                    sw.Write("{0}", outputLine1);
                    if (outputLine2 != "")
                    {
                        sw.Write("|{0}", outputLine2);
                    }
                    sw.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                isSuccess = false;
            }

            if (srQue != null)
            {
                srQue.Close();
            }
            if (srAns != null)
            {
                srAns.Close();
            }
            if (sw != null)
            {
                sw.Close();
            }
            if (isSuccess)
            {
                Console.WriteLine("[Success] encode file {0}", inputQuestionFile);
            }
            return isSuccess;
        }

        //public bool ConvertWordToInteger(string inputDataDir, string outputDataDir)
        //{
        //    DirectoryInfo inputDirInfo = new DirectoryInfo(inputDataDir);
        //    FileInfo[] inputFilesInfo = inputDirInfo.GetFiles("*Question.dat");
        //    foreach (var fileInfo in inputFilesInfo)
        //    {
        //        string fileFullName = fileInfo.FullName;
        //        string encodeName = fileInfo.Name.Replace("Question", "Encode");
        //        string endoeFullName = Path.Combine(outputDataDir, encodeName);

        //        if (!ConvertWordToInteger2(fileFullName, endoeFullName))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        private bool ConvertWordToInteger2(string inputQuestionFile, string outputfile)
        {
            bool isSuccess = true;
            StreamReader srQue = null;
            StreamWriter sw = null;
            Console.WriteLine("[Begin] encode {0}", inputQuestionFile);
            try
            {
                srQue = new StreamReader(inputQuestionFile, Encoding.UTF8);

                FileStream fs = new FileStream(outputfile, FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);

                string questionLine = null;
                while ((questionLine = srQue.ReadLine()) != null)
                {
                    var terms = questionLine.Split(new char[] { '\t' });
                    if (terms.Length < 3) { continue; }
                    string question = terms[2];
                    var queWords = enNLPUtility.WordStemming(question);
                    string outputLine1 = null;
                    foreach (var word in queWords)
                    {
                        int index = -1;
                        if (wordIndexDic.ContainsKey(word))
                        {
                            index = wordIndexDic[word];
                        }
                        else
                        {
                            currentMaxWordIndex++;
                            wordIndexDic[word] = currentMaxWordIndex;
                            index = currentMaxWordIndex;
                        }
                        if (outputLine1 == null)
                        {
                            outputLine1 = index.ToString();
                            continue;
                        }
                        outputLine1 += ("|" + index.ToString());
                    }
                    if (outputLine1 == null)
                    {
                        Console.WriteLine("question is empty");
                        continue;
                    }
                    
                    sw.WriteLine("{0}", outputLine1);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                isSuccess = false;
            }

            if (srQue != null)
            {
                srQue.Close();
            }
            if (sw != null)
            {
                sw.Close();
            }
            if (isSuccess)
            {
                Console.WriteLine("[Success] encode file {0}", inputQuestionFile);
            }
            return isSuccess;
        }

        public void WriteWordIndexDicToFile(string outputfile)
        {
            StreamWriter sw = null;

            try
            {
                FileStream fs = new FileStream(outputfile, FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);

                foreach (var wordIndex in wordIndexDic)
                {
                    sw.WriteLine("{0}\t{1}", wordIndex.Key, wordIndex.Value);
                }
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (sw != null)
            {
                sw.Close();
            }
        }

       
        public bool LoadData(string[] inputfiles, out LdaDocument[] qaDocuments)
        {
            StreamReader sr = null;
            int count = 0;
            List<LdaDocument> documents = new List<LdaDocument>();
            foreach (string inputfile in inputfiles) 
            {
                Console.WriteLine("[Begin] read {0}", inputfile);
                try
                {
                    sr = new StreamReader(inputfile, Encoding.UTF8);
                    string queLine = null;
                    while ((queLine = sr.ReadLine()) != null)
                    {
                        if (count++ < 50000)
                            continue;
                        string[] tmp = queLine.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tmp.Length < 2)
                            continue;


                        LdaDocument document = new LdaDocument();
                        List<int> queWordList = new List<int>();
                        var questionWords = tmp[0].Split(new char[] { '|' },StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in questionWords)
                        {
                            try
                            {
                                int index = int.Parse(word);
                                queWordList.Add(index);
                            }
                            catch (Exception e){ Console.WriteLine(e); }
                        }

                        List<int> quetags = new List<int>();
                        var tags = tmp[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tag in tags)
                        {
                            quetags.Add(int.Parse(tag));
                        }

                        document.tag = quetags.ToArray();
                        document.doc = queWordList.ToArray();
                        documents.Add(document);
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
                if (sr != null)
                {
                    sr.Close();
                }
                Console.WriteLine("[End] read {0}", inputfile);
            }
            qaDocuments = documents.ToArray();
            documents = null;
            return true;
        }
    }
}

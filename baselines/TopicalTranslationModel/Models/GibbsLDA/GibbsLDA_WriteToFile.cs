using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace GibbsLDA.Models
{
    public partial class GibbsLDA
    {
        const string splitStr = " ";
        public bool SaveParameter(string modelFile)
        {
            bool success = false;
            StreamWriter sw = null;
            StreamWriter sw2 = null;
            StreamWriter sw3= null;
            StreamWriter sw4 = null;
            try
            {
               // FileStream fs = new FileStream(modelFile, FileMode.Create);
                sw = new StreamWriter(modelFile+".setting");
                sw2 = new StreamWriter(modelFile + ".phi");
                sw3 = new StreamWriter(modelFile + ".theta");
                sw4 = new StreamWriter(modelFile + "");

                sw.WriteLine("DocCount:\t" + docCount);
                ///// <summary>
                ///// vocabulry size
                ///// </summary>
                //public int vocbulrySize;
                sw.WriteLine("VocabularySize:\t" + vocbulrySize);

                ///// <summary>
                ///// the total number of topics
                ///// </summary>
                //public int topicSum_K;
                sw.WriteLine("TopicSum_K:\t" + topicSum_K);

                ///// <summary>
                ///// document--topic associations dirichlet parameter
                ///// </summary>
                //double alpha;
                sw.WriteLine("Alpha:\t"+ alpha);

                ///// <summary>
                ///// topic--word associations dirichlet parameter
                ///// </summary>
                //double beta;
                sw.WriteLine("beta:\t"+beta);

                //int numstats;
                sw.WriteLine("numstats:\t"+numstats);
                //int BURN_IN = 100;
                sw.WriteLine("Burn_in:\t" + BURN_IN);
                //int ITERATIONS = 1000;
                sw.WriteLine("Iterations:\t" + ITERATIONS);
                //public int SAMPLE_LAG = 10;
                sw.WriteLine("Sample_lag:\t" + SAMPLE_LAG);

                sw2.WriteLine("{0}\t{1}", backgroundWordToWrite, nonBackgroundWordToWrite);
                for (int j = 0; j < vocbulrySize; j++)
                {
                    StringBuilder sb=  new StringBuilder();
                    sb.Append(j);sb.Append(' ');
                    
                    for (int i = 0; i < topicSum_K; i++)
                    {
                        sb.Append(phiSum[i, j] + splitStr);
                    }
                    sb.Append(phiB[j]);
                    sw2.WriteLine(sb.ToString());
                }

                 for (int i = 0; i < topicSum_K; i++)
                {
                    sw3.WriteLine(i);
                }
                 for (int j = 0; j < vocbulrySize; j++)
                {
                    sw4.Write(j+"\t");
                    foreach(var v in WordTagTopicSum[j])
                    {
                        sw4.Write(v.Key + ":");
                        for(int i=0;i<topicSum_K;i++)
                        {
                            sw4.Write(v.Value[i] + " ");
                        }
                        sw4.Write("\t");
                    }
                    sw4.WriteLine();
                }
            }
            catch(Exception e)
            {
                return success;
            }
            sw.Close();
            sw2.Close();
            sw3.Close();
            sw4.Close();
            success = true;
            return success;
        }
        
        public bool SaveModel(string modelFile)
        {
            bool success = true;
            StreamWriter sw = null;
            try
            {
                FileStream fs = new FileStream(modelFile, FileMode.Create);
                sw = new StreamWriter(fs);
                //LdaQADocument[] qaDocuments;

                //int docCount;
                sw.WriteLine(docCount);
                ///// <summary>
                ///// vocabulry size
                ///// </summary>
                //public int vocbulrySize;
                sw.WriteLine(vocbulrySize);

                ///// <summary>
                ///// the total number of topics
                ///// </summary>
                //public int topicSum_K;
                sw.WriteLine(topicSum_K);

                ///// <summary>
                ///// document--topic associations dirichlet parameter
                ///// </summary>
                //double alpha;
                sw.WriteLine(alpha);

                ///// <summary>
                ///// topic--word associations dirichlet parameter
                ///// </summary>
                //double beta;
                sw.WriteLine(beta);

                //int numstats;
                sw.WriteLine(numstats);
                //int BURN_IN = 100;
                sw.WriteLine(BURN_IN);
                //int ITERATIONS = 1000;
                sw.WriteLine(ITERATIONS);
                //public int SAMPLE_LAG = 10;
                sw.WriteLine(SAMPLE_LAG);

                ///// <summary>
                ///// docTopicSum[i,j] denotes the number of words in document i assigned to topic j
                ///// </summary>
                //int[,] docTopicSum;
                string line = "";
                for (int i = 0; i < docCount; i++)
                {
                    line = docTopicSum[i, 0].ToString();
                    for (int j = 1; j < topicSum_K; j++)
                    {
                        line += (splitStr + docTopicSum[i, j]);
                    }
                    sw.WriteLine(line);
                }

                ///// <summary>
                ///// wordTopicSum[i,j] denotes the number of instance of word i assigned to topic j
                ///// </summary>
                //int[,] wordTopicSum;
                for (int i = 0; i < vocbulrySize; i++)
                {
                    line = wordTopicSum[i, 0].ToString();
                    for (int j = 1; j < topicSum_K; j++)
                    {
                        line += (splitStr + wordTopicSum[i, j]);
                    }
                    sw.WriteLine(line);
                }

                ///// <summary>
                ///// topicWordSum[i] denotes the total number of words assigned to topic i
                ///// </summary>
                //int[] topicWordSum;
                line = topicWordSum[0].ToString();
                for (int i = 1; i < topicSum_K; i++)
                {
                    line += (splitStr + topicWordSum[i]);
                }
                sw.WriteLine(line);

                ///// <summary>
                ///// docWordSum[i] denotes the total number of words in document i
                ///// </summary>
                //int[] docWordSum;
                line = docWordSum[0].ToString();
                for (int i = 1; i < docCount; i++)
                {
                    line += (splitStr + docWordSum[i]);
                }
                sw.WriteLine(line);

                ///// <summary>
                ///// theataSum[i,j] denotes the probability of document i assigned to topic j
                ///// </summary>
                //double[,] thetaSum;
                for (int i = 0; i < docCount; i++)
                {
                    line = (thetaSum[i, 0]).ToString();
                    for (int j = 1; j < topicSum_K; j++)
                    {
                        line += (splitStr + thetaSum[i, j]);
                    }
                    sw.WriteLine(line);
                }

                //[NonSerialized]
                //double[,] theta;
                //public double[,] Theta { get { return theta; } }

                ///// <summary>
                ///// phiSum[i,j] denotes the probability of topic i assigned to word j
                ///// </summary>
                //double[,] phiSum;
                for (int j = 0; j < vocbulrySize; j++)
                {
                    line = (phiSum[0, j]).ToString();
                    for (int i = 1; i < topicSum_K; i++)
                    {
                        line += (splitStr + phiSum[i, j]);
                    }
                    sw.WriteLine(line);
                }

                //[NonSerialized]
                //double[,] phi

                //[NonSerialized]
                //public Random random;

                ////HMM parameter
                //int[][] topicToTopicSum;
                for (int i = 0; i < topicSum_K; i++)
                {
                    var curTopicToTopic = topicToTopicSum[i];
                    line = curTopicToTopic[0].ToString();
                    for (int j = 1; j < topicSum_K; j++)
                    {
                        line += (splitStr + curTopicToTopic[j]);
                    }
                    sw.WriteLine(line);
                }
                //int[][] topicToTopicLAG;
                for (int i = 0; i < topicSum_K; i++)
                {
                    var curTopicToTopic = topicToTopicLAG[i];
                    line = curTopicToTopic[0].ToString();
                    for (int j = 1; j < topicSum_K; j++)
                    {
                        line += (splitStr + curTopicToTopic[j]);
                    }
                    sw.WriteLine(line);
                }

                //[NonSerialized]
                //double[][] topicToTopic;
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine(e);
            }
            if (sw != null)
            {
                sw.Close();
            }
            return success;
        }

        //static char[] splitChars = new char[] { ' ' };
        public static GibbsLDA LoadModel(string modelFile)
        {
            Console.WriteLine("[Begin] get load {0}...", modelFile);
            char[] splitChars = new char[] { ' ' };
            GibbsLDA lda = new GibbsLDA();
            lda.random = new Random((int)DateTime.Now.Ticks);
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(modelFile);
                //LdaQADocument[] qaDocuments;

                //int qaDocCount;
                int _docCount = int.Parse(sr.ReadLine());
                lda.docCount = _docCount;
                ///// <summary>
                ///// vocabulry size
                ///// </summary>
                //public int vocbulrySize;
                int _vocbulrySize = int.Parse(sr.ReadLine());
                lda.vocbulrySize = _vocbulrySize;

                ///// <summary>
                ///// the total number of topics
                ///// </summary>
                //public int topicSum_K;
                int _topicSum_K = int.Parse(sr.ReadLine());
                lda.topicSum_K = _topicSum_K;

                ///// <summary>
                ///// document--topic associations dirichlet parameter
                ///// </summary>
                //double alpha;
                lda.alpha = double.Parse(sr.ReadLine());
                if (lda.alpha < 0)
                    lda.alpha = 1.0 / _topicSum_K;

                ///// <summary>
                ///// topic--word associations dirichlet parameter
                ///// </summary>
                //double beta;
                lda.beta = double.Parse(sr.ReadLine());

                //int numstats;
                lda.numstats = int.Parse(sr.ReadLine());
                //int BURN_IN = 100;
                lda.BURN_IN = int.Parse(sr.ReadLine());
                //int ITERATIONS = 1000;
                lda.ITERATIONS = int.Parse(sr.ReadLine());
                //public int SAMPLE_LAG = 10;
                lda.SAMPLE_LAG = int.Parse(sr.ReadLine());

                string[] terms = null;

                ///// <summary>
                ///// docTopicSum[i,j] denotes the number of words in document i assigned to topic j
                ///// </summary>
                //int[,] docTopicSum;
                int[,] _docTopicSum = new int[_docCount, _topicSum_K];
                for (int i = 0; i < _docCount; i++)
                {
                    terms = sr.ReadLine().Split(splitChars);
                    for (int j = 0; j < _topicSum_K; j++)
                    {
                        _docTopicSum[i, j] = int.Parse(terms[j]);
                    }
                }
                lda.docTopicSum = _docTopicSum;

                ///// <summary>
                ///// wordTopicSum[i,j] denotes the number of instance of word i assigned to topic j
                ///// </summary>
                //int[,] wordTopicSum;
                int[,] _wordTopicSum = new int[_vocbulrySize, _topicSum_K];
                for (int i = 0; i < _vocbulrySize; i++)
                {
                    terms = sr.ReadLine().Split(splitChars);
                    for (int j = 0; j < _topicSum_K; j++)
                    {
                        _wordTopicSum[i, j] = int.Parse(terms[j]);
                    }
                }
                lda.wordTopicSum = _wordTopicSum;

                ///// <summary>
                ///// topicWordSum[i] denotes the total number of words assigned to topic i
                ///// </summary>
                //int[] topicWordSum;
                int[] _topicWordSum = new int[_topicSum_K];
                terms = sr.ReadLine().Split(splitChars);
                for (int i = 0; i < _topicSum_K; i++)
                {
                    _topicWordSum[i] = int.Parse(terms[i]);
                }
                lda.topicWordSum = _topicWordSum;

                ///// <summary>
                ///// docWordSum[i] denotes the total number of words in document i
                ///// </summary>
                //int[] docWordSum;
                int[] _docWordSum = new int[_docCount];
                terms = sr.ReadLine().Split(splitChars);
                for (int i = 0; i < _docCount; i++)
                {
                    _docWordSum[i] = int.Parse(terms[i]);
                }
                lda.docWordSum = _docWordSum;

                ///// <summary>
                ///// theataSum[i,j] denotes the probability of document i assigned to topic j
                ///// </summary>
                //double[,] thetaSum;
                double[,] _thetaSum = new double[_docCount, _topicSum_K];
                for (int i = 0; i < _docCount; i++)
                {
                    terms = sr.ReadLine().Split(splitChars);
                    for (int j = 0; j < _topicSum_K; j++)
                    {
                        _thetaSum[i, j] = double.Parse(terms[j]);
                    }
                }
                lda.thetaSum = _thetaSum;

                //[NonSerialized]
                //double[,] theta;
                //public double[,] Theta { get { return theta; } }

                ///// <summary>
                ///// phiSum[i,j] denotes the probability of topic i assigned to word j
                ///// </summary>
                //double[,] phiSum;
                double[,] _phiSum = new double[_topicSum_K, _vocbulrySize];
                for (int j = 0; j < _vocbulrySize; j++)
                {
                    terms = sr.ReadLine().Split(splitChars);
                    for (int i = 0; i < _topicSum_K; i++)
                    {
                        _phiSum[i, j] = double.Parse(terms[i]);
                    }
                }
                lda.phiSum = _phiSum;

                //[NonSerialized]
                //double[,] phiQuery;

                //[NonSerialized]
                //public Random random;

                ////HMM parameter
                //int[][] queryTopicToTopicSum;
                int[][] _topicToTopicSum = new int[_topicSum_K][];
                for (int i = 0; i < _topicSum_K; i++)
                {
                    int[] cur = new int[_topicSum_K];
                    terms = sr.ReadLine().Split(splitChars);
                    for (int j = 0; j < _topicSum_K; j++)
                    {
                        cur[j] = int.Parse(terms[j]);
                    }
                    _topicToTopicSum[i] = cur;
                }
                lda.topicToTopicSum = _topicToTopicSum;
                //int[][] queryTopicToTopicLAG;
                int[][] _topicToTopicLAG = new int[_topicSum_K][];
                for (int i = 0; i < _topicSum_K; i++)
                {
                    int[] cur = new int[_topicSum_K];
                    terms = sr.ReadLine().Split(splitChars);
                    for (int j = 0; j < _topicSum_K; j++)
                    {
                        cur[j] = int.Parse(terms[j]);
                    }
                    _topicToTopicLAG[i] = cur;
                }
                lda.topicToTopicLAG = _topicToTopicLAG;

                //[NonSerialized]
                //double[][] queryTopicToTopic;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (sr != null)
            {
                sr.Close();
            }
            Console.WriteLine("[End] get load {0}...", modelFile);
            return lda;
        }

        public static GibbsLDA LoadModel(string modelfile, string docfile)
        {
            GibbsLDA lda = LoadModel(modelfile);
            lda.BinaryDeserializeDocuments(docfile);
            return lda;
        }

        public static GibbsLDA LoadModelPredict(string modelfile1, string modelfile2)
        {
            LdaModel ldaModel1 = new LdaModel(modelfile1);
            LdaModel ldaModel2 = new LdaModel(modelfile2);

            Thread thread1 = new Thread(LoadModelThread);
            Thread thread2 = new Thread(LoadModelThread);

            thread1.Start(ldaModel1);
            thread2.Start(ldaModel2);

            thread1.Join();
            thread2.Join();

            GibbsLDA lda1 = null;
            GibbsLDA lda2 = null;
            if (ldaModel1.lda.numstats > ldaModel2.lda.numstats)
            {
                lda2 = ldaModel1.lda;
                lda1 = ldaModel2.lda;
            }
            else
            {
                lda2 = ldaModel2.lda;
                lda1 = ldaModel1.lda;
            }

            lda2.numstats = lda2.numstats - lda1.numstats;
            for (int k = 0; k < lda2.topicSum_K; k++)
            {
                for (int v = 0; v < lda2.vocbulrySize; v++)
                    lda2.phiSum[k, v] = lda2.phiSum[k, v] - lda1.phiSum[k, v];
            }

            for (int k1 = 0; k1 < lda2.topicSum_K; k1++)
            {
                for (int k2 = 0; k2 < lda2.topicSum_K; k2++)
                    lda2.topicToTopicLAG[k1][k2] = lda2.topicToTopicLAG[k1][k2] - lda1.topicToTopicLAG[k1][k2];
            }

            return lda2;
        }

        private static void LoadModelThread(object ldamodel)
        {
            LdaModel mLdaModel = (LdaModel)ldamodel;
            mLdaModel.lda = LoadModel(mLdaModel.modelfile);
        }
    }

    class LdaModel
    {
        public string modelfile;
        public GibbsLDA lda;

        public LdaModel(string modelFile)
        {
            modelfile = modelFile;
        }
    }
}

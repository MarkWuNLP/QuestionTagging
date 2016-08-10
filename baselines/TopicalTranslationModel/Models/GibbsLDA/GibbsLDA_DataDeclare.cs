using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

using GibbsLDA.Entity;

namespace GibbsLDA.Models
{
    [Serializable]
    public partial class GibbsLDA
    {
        //[NonSerialized]
        /// <summary>
        /// document data
        /// </summary>
        [NonSerialized]
        LdaDocument[] docs;

        int docCount;

        /// <summary>
        /// vocabulry size
        /// </summary>
        public int vocbulrySize;
        public int TagvocabularySize;
        /// <summary>
        /// the total number of topics
        /// </summary>
        public int topicSum_K;

        /// <summary>
        /// document--topic associations dirichlet parameter
        /// </summary>
        double alpha;
        public double Alpha { get { return alpha; } }
        /// <summary>
        /// topic--word associations dirichlet parameter
        /// </summary>
        double beta;
        public double Beta { get { return beta; } }
        ///// <summary>
        ///// topic assignments for each word
        ///// </summary>
        //List<int[]> z_question;
        //List<int[]> z_answer;
        int[] topicDocSum;
        
        /// <summary>
        /// denotes document i word j background property 
        /// </summary>
        bool[][] isBackgroundWord;
        int[] wordBackgroundSum;

        //[NonSerialized]
        /// <summary>
        /// docTopicSum[i,j] denotes the number of words in document i assigned to topic j
        /// </summary>
        [NonSerialized]
        int[,] docTopicSum;
        int[] docTopic;
        //Dictionary<int, List<int>> docTopicSumDic;// use to serialize
        List<int[]> docTopicSumList;

        //[NonSerialized]
        /// <summary>
        /// wordTopicSum[i,j] denotes the number of instance of word i assigned to topic j
        /// </summary>
        [NonSerialized]
        int[,] wordTopicSum;
        List<int[]> wordTopicSumList;

        /// <summary>
        /// topicWordSum[i] denotes the total number of words assigned to topic i
        /// </summary>
        int[] topicWordSum;

        /// <summary>
        /// docWordSum[i] denotes the total number of words in document i
        /// </summary>
        int[] docWordSum;


        /// <summary>
        /// theataSum[i,j] denotes the probability of document i assigned to topic j
        /// </summary>
        [NonSerialized]
        double[,] thetaSum;
        List<double[]> thetaSumList;

        [NonSerialized]
        double[,] theta;
        public double[,] Theta { get { return theta; } }

        /// <summary>
        /// phiSum[i,j] denotes the probability of topic i assigned to word j
        /// </summary>
        [NonSerialized]
        double[,] phiSum;
        double[] phiB;
        List<double[]> phiSumList;
        [NonSerialized]
        double[,] phi;
        public double[,] Phi { get { return phi; } }

        //[NonSerialized]
        int numstats;

        //[NonSerialized]
        //int THIN_INTERVAL = 20;
        //[NonSerialized]
        int BURN_IN = 100;
        //[NonSerialized]
        int ITERATIONS = 1000;
        //[NonSerialized]
        public int SAMPLE_LAG = 10;

        double backgroundWordToWrite, nonBackgroundWordToWrite; 
        int BackgroundSum, NonBackgroundSum;

        Dictionary<int,int[]>[] WordTagTopicSum;
        int[,] WordTagSum;        
        
        //[NonSerialized]
        public Random random;
        public Random backgroundrandom;

        //HMM parameter
        int[][] topicToTopicSum;
        int[][] topicToTopicLAG;
        [NonSerialized]
        double[][] topicToTopic;

        public void BinarySerialize(string outputFilePath)
        {
            FileStream fs = null;
            Console.WriteLine("[Begin] serialize {0}", outputFilePath);
            try
            {
                fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                InitListFormData();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, this);
                DisposeList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (fs != null)
            {
                fs.Close();
            }
            Console.WriteLine("[End] serialize {0}", outputFilePath);
        }

        public static GibbsLDA BinaryDeSerialize(string inputFilePath)
        {
            FileStream fs = null;
            GibbsLDA ldaModel = null;
            Console.WriteLine("[Begin] desserialize {0}", inputFilePath);
            try
            {
                fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFormatter bf = new BinaryFormatter();
                ldaModel = (GibbsLDA)bf.Deserialize(fs);
                ldaModel.InitDataFromList();
                ldaModel.DisposeList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (fs != null)
            {
                fs.Close();
            }
            Console.WriteLine("[End] desserialize {0}", inputFilePath);
            return ldaModel;
        }

        public void BinarySerializeDocuments(string outputFilePath)
        {
            if (docs == null) { return; }
            FileStream fs = null;
            Console.WriteLine("[Begin] serialize {0}", outputFilePath);
            try
            {
                fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, docs);
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (fs != null) { fs.Close(); }

            Console.WriteLine("[End] serialize {0}", outputFilePath);
        }

        public void BinaryDeserializeDocuments(string inputFilePath)
        {
            FileStream fs = null;
            Console.WriteLine("[Begin] deserialize {0}", inputFilePath);
            try
            {
                fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFormatter bf = new BinaryFormatter();
                LdaDocument[] documents = (LdaDocument[])bf.Deserialize(fs);
                if (documents != null) { docs = documents; }
            }
            catch (Exception e) { Console.WriteLine(e); }
            if (fs != null) { fs.Close(); }
            Console.WriteLine("[End] deserialize {0}", inputFilePath);
        }

        private void XMLSerialize(string outputFilePath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                InitListFormData();
                XmlSerializer xs = new XmlSerializer(typeof(GibbsLDA));
                xs.Serialize(fs, this);
                DisposeList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (fs != null)
            {
                fs.Close();
            }
        }

        private static GibbsLDA XMLDeSerialize(string inputFilePath)
        {
            FileStream fs = null;
            GibbsLDA ldaModel = null;
            try
            {
                fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlSerializer xs = new XmlSerializer(typeof(GibbsLDA));
                ldaModel = (GibbsLDA)xs.Deserialize(fs);
                ldaModel.InitDataFromList();
                ldaModel.DisposeList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (fs != null)
            {
                fs.Close();
            }
            return ldaModel;
        }

        private void InitListFormData()
        {
            docTopicSumList = new List<int[]>();
            thetaSumList = new List<double[]>();
            for (int i = 0; i < docs.Length; i++)
            {
                int[] docTopicISum = new int[topicSum_K];
                double[] docTopicRate = new double[topicSum_K];
                for (int j = 0; j < topicSum_K; j++)
                {
                    docTopicISum[j] = docTopicSum[i, j];
                    docTopicRate[j] = thetaSum[i, j];
                }
                docTopicSumList.Add(docTopicISum);
                thetaSumList.Add(docTopicRate);
            }

            wordTopicSumList = new List<int[]>();
            for (int i = 0; i < vocbulrySize; i++)
            {
                int[] queWordITopicSum = new int[topicSum_K];
                int[] ansWordITopicSum = new int[topicSum_K];
                for (int j = 0; j < topicSum_K; j++)
                {
                    queWordITopicSum[j] = wordTopicSum[i, j];
                }
                wordTopicSumList.Add(queWordITopicSum);
            }

            phiSumList = new List<double[]>();
            for (int i = 0; i < topicSum_K; i++)
            {
                double[] quePhiISum = new double[vocbulrySize];
                double[] ansPhiISum = new double[vocbulrySize];
                for (int j = 0; j < vocbulrySize; j++)
                {
                    quePhiISum[j] = phiSum[i, j];
                }
                phiSumList.Add(quePhiISum);
            }
        }

        private void InitDataFromList()
        {
            if (docCount <= 0) { docCount = thetaSumList.Count; }
            docTopicSum = new int[docCount, topicSum_K];
            thetaSum = new double[docCount, topicSum_K];
            for (int i = 0; i < docCount; i++)
            {
                for (int j = 0; j < topicSum_K; j++)
                {
                    docTopicSum[i, j] = docTopicSumList[i][j];
                    thetaSum[i, j] = thetaSumList[i][j];
                }
            }

            wordTopicSum = new int[vocbulrySize, topicSum_K];
            for (int i = 0; i < vocbulrySize; i++)
            {
                for (int j = 0; j < topicSum_K; j++)
                {
                    wordTopicSum[i, j] = wordTopicSumList[i][j];
                }
            }

            phiSum = new double[topicSum_K, vocbulrySize];
            for (int i = 0; i < topicSum_K; i++)
            {
                for (int j = 0; j < vocbulrySize; j++)
                {
                    phiSum[i, j] = phiSumList[i][j];
                }
            }
        }

        private void DisposeList()
        {
            docTopicSumList = null;
            thetaSumList = null;

            wordTopicSumList = null;
            phiSumList = null;
        }
    }
}

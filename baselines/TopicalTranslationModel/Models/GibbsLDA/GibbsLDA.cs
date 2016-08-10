using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GibbsLDA.Entity;
using System.IO;

namespace GibbsLDA.Models
{
    public partial class GibbsLDA
    {
        static System.Object writelock = new object(); 
        double sigma = 0.01;
        public void Gibbs(int topicSum, double alpha, double beta, string tempModelFile = null, int startIteration = 0)
        {
            if (docs == null) 
            {
                Console.WriteLine("please init documents first.");
                return;
            }

            if (startIteration == 0)
            {
                this.topicSum_K = topicSum;
                this.alpha = alpha;
                this.beta = beta;

                InitState(topicSum);
            }

            int count = 0;
            int topic = 0;
            int word = 0;

            LdaDocument document = null;
            int docIndex = 0;
            int wordIndex = 0;

            int wordTopic;
            double[] p = new double[topicSum_K];
            double u = 0;
            for (int i = startIteration; i < ITERATIONS; i++)
            {
                double pos = 0, neg = 0;
            
                for (docIndex = 0; docIndex < docs.Length; docIndex++)
                {
                    document =docs[docIndex];
                   // if (document.wordTopic != null)
                    {
                        wordTopic = docTopic[docIndex];

                        #region SampleTopic
                        topicDocSum[wordTopic]--;

                        for (wordIndex = 0; wordIndex < document.doc.Length; wordIndex++)
                        {
                            if (!isBackgroundWord[docIndex][wordIndex])
                            {
                                for (int t = 0; t < document.tag.Length; t++)
                                {
                                    try
                                    {
                                        WordTagSum[document.doc[wordIndex],wordTopic]--;
                                        WordTagTopicSum[document.doc[wordIndex]][document.tag[t]][wordTopic]--;

                                        if(WordTagTopicSum[document.doc[wordIndex]][document.tag[t]][wordTopic]<0)
                                        {
                                            ;
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                 
                                }

                                wordTopicSum[document.doc[wordIndex], wordTopic]--;
                                topicWordSum[wordTopic]--;
                            }

                        }

                        for (topic = 0; topic < topicSum_K; topic++)
                        {
                            p[topic] = (topicDocSum[topic] + alpha);
                            for (int dociter = 0; dociter < document.doc.Length; dociter++)
                            {
                                if (!isBackgroundWord[docIndex][dociter])
                                {
                                    p[topic] *= 10 * (wordTopicSum[document.doc[dociter], topic] + beta) 
                                        / (topicWordSum[topic] + vocbulrySize * beta);

                                    double tagprobability = 0;
                                    for(int t=0;t<document.tag.Length;t++)
                                    {
                                        if (WordTagTopicSum[document.doc[dociter]].ContainsKey(document.tag[t]))
                                            tagprobability += (WordTagTopicSum[document.doc[dociter]][document.tag[t]][topic] + beta)
                                                / (WordTagSum[document.doc[dociter],topic] + beta * TagvocabularySize);
                                    }
                                    p[topic] *= 100 * tagprobability;
                                }
                            }
                        }

                        for (topic = 1; topic < topicSum_K; topic++)
                        {
                            p[topic] += p[topic - 1];
                        }
                        u = random.NextDouble() * p[topicSum_K - 1];
                        for (topic = 0; topic < topicSum_K; topic++)
                        {
                            if (p[topic] > u) { break; }
                        }
                        if (topic == 30)
                        {
                            if(numstats>1)
                            {
                                ;
                            }

                            topic = (int)(random.NextDouble() * 30);
                            Console.Write("!");
                        }

                        topicDocSum[topic]++;
                        docTopic[docIndex] = topic;

                        for (wordIndex = 0; wordIndex < document.doc.Length; wordIndex++)
                        {
                            if (!isBackgroundWord[docIndex][wordIndex])
                            {
                               

                                wordTopicSum[document.doc[wordIndex], topic]++;
                                topicWordSum[topic]++;
                            }
                        }

                        #endregion             
                        wordTopic = topic;


                        #region SampleBackground
                        for (wordIndex = 0; wordIndex < document.doc.Length; wordIndex++)
                        {

                            bool isbackground = isBackgroundWord[docIndex][wordIndex];
                            word = document.doc[wordIndex];
                            if (isbackground)
                            {
                                wordBackgroundSum[word]--;
                                BackgroundSum--;
                                //firstterm = (BackgroundSum + sigma);// / (BackgroundSum + topicSum + 2 * sigma);
                            }
                            else
                            {
                                NonBackgroundSum--;
                                wordTopicSum[word, wordTopic]--;
                                topicWordSum[wordTopic]--;
                                //firstterm = (topicSum + sigma) / (BackgroundSum + topicSum + 2 * sigma);
                            }


                            double background_prob = (BackgroundSum + sigma) * ((wordBackgroundSum[word] + beta) / (BackgroundSum + vocbulrySize * beta));
                            double topicz_prob = (NonBackgroundSum + sigma) * ((wordTopicSum[word, wordTopic] + beta) / (topicWordSum[wordTopic] + vocbulrySize * beta));

                            double sum = topicz_prob + background_prob;
                            u = random.NextDouble() * sum;
                            if (u < topicz_prob)
                            {
                                wordTopicSum[word, wordTopic]++;
                                topicWordSum[wordTopic]++;
                                NonBackgroundSum++;

                                for (int t = 0; t < document.tag.Length; t++)
                                {
                                    WordTagSum[word, topic]++;

                                    if (WordTagTopicSum[word].ContainsKey(document.tag[t]))
                                    {
                                        WordTagTopicSum[word][document.tag[t]][wordTopic]++;
                                    }
                                }



                                isBackgroundWord[docIndex][wordIndex] = false;
                            }
                            else
                            {
                                wordBackgroundSum[word]++;
                                BackgroundSum++;
                                isBackgroundWord[docIndex][wordIndex] = true;
                            }
                        }
                        #endregion

                    }
                    count++;
                    if (count == 10000)
                    {
                        count = 0;
                        Console.Write(".");
                    }
                }
                #region UpdatePara
                if (i > BURN_IN)
                {
                    if ((SAMPLE_LAG > 0) && (i % SAMPLE_LAG == 0))
                    {
                        //UpdateTopicToTotopic();
                        UpdateParams();
                    }
                    if (tempModelFile != null && i % 100 == 0)//i % (SAMPLE_LAG * 5) == 0)
                    {
                        //BinarySerialize(tempModelFile + "_temp" + i);
                        lock (writelock)
                        {
                            try
                            {
                               // BinarySerializeDocuments(tempModelFile + "_temp_doc_" + i);
                            }
                            catch (Exception e) 
                            {
                                Console.WriteLine(e);
                            }
                            SaveParameter(Path.Combine("model","_temp" + i));
                            //SaveModel(tempModelFile + "_temp" + i);
                        }
                    }
                }
                #endregion


                Console.WriteLine();
                Console.WriteLine(i);
                //Console.WriteLine("{0}\t{1}",pos,neg);
                Console.WriteLine("{0}\t{1}\t{2}", DateTime.Now, BackgroundSum, NonBackgroundSum); 
            }

        }

       
        private void UpdateParams()
        {
        //for (int m = 0; m < docs.Length; m++)
        //{
        //    for (int topic = 0; topic < topicSum_K; topic++)
        //    {
        //        thetaSum[m, topic] += ((docTopicSum[m, topic] + alpha) / (docWordSum[m] + topicSum_K * alpha));
        //    }
        //}

            for (int topic = 0; topic < topicSum_K; topic++)
            {
                for (int word = 0; word < vocbulrySize; word++)
                {
                    phiSum[topic, word] += (wordTopicSum[word, topic] + beta);
                }
            }

            for (int word = 0; word < vocbulrySize; word++)
            {
                phiB[word] += wordBackgroundSum[word];
            }

            backgroundWordToWrite += BackgroundSum;

            nonBackgroundWordToWrite += NonBackgroundSum;

            numstats++;

        }

        private void UpdateTopicToTopic()
        {
            for (int i = 0; i < topicSum_K; i++)
            {
                var temp = topicToTopicSum[i];
                for (int j = 0; j < topicSum_K; j++)
                {
                    temp[j] = 0;
                }
                topicToTopicSum[i] = temp;
            }

            for (int i = 0; i < docs.Length; i++)
            {
                LdaDocument document = docs[i];
                if (document.wordTopic != null)
                {
                    int n_length = document.wordTopic.Length;
                    int preTopic = document.wordTopic[0];
                    int curTopic = 0;
                    for (int j = 1; j < n_length; j++)
                    {
                        curTopic = document.wordTopic[j];
                        topicToTopicSum[preTopic][curTopic]++;
                        preTopic = curTopic;
                    }
                }
            }
        }

        public void ComputeTheta()
        {
            theta = new double[docs.Length, topicSum_K];
            if (SAMPLE_LAG > 0)
            {
                for (int m = 0; m < docs.Length; m++)
                {
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        theta[m, topic] = thetaSum[m, topic] / numstats;
                    }
                }
            }
            else
            {
                for (int m = 0; m < docs.Length; m++)
                {
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        theta[m, topic] = ((docTopicSum[m, topic] + alpha) / (docWordSum[m] + topicSum_K * alpha));
                    }
                }
            }
        }

        public void SavedTheta(string inputFilePath, string outFilePath)
        {
            ComputeTheta();
            System.IO.StreamReader sr = null;
            System.IO.StreamWriter sw = null;

            try {
                sr = new System.IO.StreamReader(inputFilePath);
                sw = new System.IO.StreamWriter(new System.IO.FileStream(outFilePath, System.IO.FileMode.Create));
                string curLine = null;
                for (int i = 0; i < docs.Length && (curLine = sr.ReadLine()) != null; i++)
                {
                    string topic = theta[i, 0].ToString();
                    for (int j = 1; j < topicSum_K; i++)
                        topic += ("|" + theta[i, j]);
                    sw.WriteLine("{0}\t{1}", curLine, topic);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (sr != null)
                sr.Close();
            if (sw != null)
                sw.Close();
        }

        public double[,] ComputePhi()
        {
            phi = new double[topicSum_K, vocbulrySize];
            if (SAMPLE_LAG > 0)
            {
                for (int topic = 0; topic < topicSum_K; topic++)
                {
                    for (int word = 0; word < vocbulrySize; word++)
                    {
                        phi[topic, word] = phiSum[topic, word] / numstats;
                    }
                }
            }
            else
            {
                for (int topic = 0; topic < topicSum_K; topic++)
                {
                    for (int word = 0; word < vocbulrySize; word++)
                    {
                        phi[topic, word] = (wordTopicSum[word, topic] + beta) / (topicWordSum[topic] + vocbulrySize * beta);
                    }
                }
            }
            return phi;
        }

        public double[][] ComputeTopicToTopic()
        {
            topicToTopic = new double[topicSum_K][];
            for (int i = 0; i < topicSum_K; i++) 
            {
                topicToTopic[i] = new double[topicSum_K];
            }

            if (SAMPLE_LAG > 0)
            {
                for (int preTopic = 0; preTopic < topicSum_K; preTopic++) 
                {
                    double _total = topicSum_K * 0.001;
                    double ansTotal = topicSum_K * 0.001;
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        _total += ((double)topicToTopicLAG[preTopic][topic] / numstats);
                    }
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        topicToTopic[preTopic][topic] = ((double)topicToTopicLAG[preTopic][topic] / numstats + 0.001) / _total;
                    }
                }
            }
            else
            {
                for (int preTopic = 0; preTopic < topicSum_K; preTopic++)
                {
                    double _total = topicSum_K * 0.001;
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        _total += topicToTopicSum[preTopic][topic];
                    }
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        topicToTopic[preTopic][topic] = (topicToTopicSum[preTopic][topic] + 0.001) / _total;
                    }
                }
            }
            return topicToTopic;
        }
    }
}

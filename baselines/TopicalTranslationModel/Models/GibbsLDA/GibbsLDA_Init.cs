using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GibbsLDA.Entity;

namespace GibbsLDA.Models
{
    public partial class GibbsLDA
    {
        private GibbsLDA() { random = new Random((int)DateTime.Now.Ticks); }
        public GibbsLDA(LdaDocument[] documents, int vocbulrySize)
        {
            this.docs = documents;
            this.docCount = documents.Length;
            this.vocbulrySize = vocbulrySize;
        }

        private void InitState(int topicSum)
        {
            random = new Random((int)DateTime.Now.Ticks);
            backgroundrandom =  new Random((int)DateTime.Now.Ticks);
            docTopic = new int[docs.Length];
            wordTopicSum = new int[vocbulrySize, topicSum];
            wordBackgroundSum = new int[vocbulrySize];
            topicWordSum = new int[topicSum];
            isBackgroundWord = new bool[docs.Length][];
            topicDocSum = new int[topicSum];
            phiSum = new double[topicSum, vocbulrySize];
            phiB = new double[vocbulrySize];
            WordTagTopicSum = new Dictionary<int,int[]>[vocbulrySize];
            WordTagSum = new int[vocbulrySize, topicSum];
            //wordtagtopic = new short[vocbulrySize,69534,topicSum_K];
            ////docTopicSum[i, topic]++;

            //wordtagsum = new short[69534, topicSum_K];
            TagvocabularySize = 69534;

            for (int i = 0; i < vocbulrySize;i++ )
            {
                WordTagTopicSum[i] = new Dictionary<int, int[]>();
            }

            for (int i = 0; i < docs.Length; i++)
            {
                LdaDocument document = docs[i];
                int topic = random.Next(topicSum);
                docTopic[i] = topic;
                topicDocSum[topic]++;


                if (document.doc != null)
                {
                    int _docLength = document.doc.Length;
                    isBackgroundWord[i] = new bool[_docLength];
                    //initial state of Markov chain by random
                    for (int j = 0; j < _docLength; j++)
                    {

                        int background = 0;
                        double ran = random.NextDouble();
                        if (ran > 0.5)
                        {
                            background = 0;
                        }
                        else
                        {
                            background = 1;
                        }
                        if (background == 0)
                        {
                            NonBackgroundSum++;
                            wordTopicSum[document.doc[j], topic]++;
                            topicWordSum[topic]++;
                            isBackgroundWord[i][j] = false;


                            for (int t = 0; t < document.tag.Length; t++)
                            {
                                //string wordTagKey;
                                //StringBuilder sb = new StringBuilder();
                                //sb.Append(document.tag[t].ToString());
                                //sb.Append("##");
                                //sb.Append(document.doc[j].ToString());
                                //wordTagKey = sb.ToString();

                                WordTagSum[document.doc[j], topic]++;


                                if (WordTagTopicSum[document.doc[j]].ContainsKey(document.tag[t]))
                                {
                                    WordTagTopicSum[document.doc[j]][document.tag[t]][topic]++;
                                }
                                else
                                {
                                    WordTagTopicSum[document.doc[j]][document.tag[t]] = new int[topicSum_K];
                                    WordTagTopicSum[document.doc[j]][document.tag[t]][topic]++;
                                }
                            }
                        }
                        else
                        {
                            BackgroundSum++;
                            wordBackgroundSum[document.doc[j]]++;
                            for (int t = 0; t < document.tag.Length; t++)
                            {
                                if (!WordTagTopicSum[document.doc[j]].ContainsKey(document.tag[t]))
                                {
                                    WordTagTopicSum[document.doc[j]][document.tag[t]] = new int[topicSum_K];
                                }
                            }


                            isBackgroundWord[i][j] = true;
                        }
                    }
                }
            }
        }

        public void Configure(int iterations, int burnIn, int sampleLag)
        {
            ITERATIONS = iterations;
            BURN_IN = burnIn;
            //THIN_INTERVAL = thinInterval;
            SAMPLE_LAG = sampleLag;
        }
    }
}

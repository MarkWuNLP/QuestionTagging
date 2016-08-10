using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GibbsLDA.Entity;

namespace GibbsLDA.Models
{
    partial class GibbsLDA
    {
        public double TopicLikelihood(LdaDocument question, LdaDocument candidate)
        {  
            var questionTheta = GetRankDocumentTheta(question);
            var candidateTheta = GetRankDocumentTheta(candidate);

            double score = 0;
            for (int i = 0; i < topicSum_K; i++)
            {
                score += (questionTheta[i] * candidateTheta[i] / topicWordSum[i]);
            }
            return score * question.doc.Length;
        }

        public double TopicLikelihood(double[] questionTheta, double[] candidateTheta, double questionLength = 1)
        {
            double score = 0;
            try
            {
                for (int i = 0; i < topicSum_K; i++)
                {
                    score += (questionTheta[i] * candidateTheta[i] / topicWordSum[i]);
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            return score * questionLength;
        }

        public double WordTopicScore(int word, double[] docTheta)
        {
            double score = 0;
            try
            {
                for (int i = 0; i < topicSum_K; i++)
                {
                    score += (docTheta[i] * phi[i, word]);
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            return score;
        }

        [NonSerialized]
        double[] topicDis = null;
        public double[] GetRankDocHMMTrans(LdaDocument rankDoc) 
        {
            if (rankDoc.doc == null || rankDoc.doc.Length == 0) { return new double[topicSum_K]; }
            if (topicDis == null) { InitTopicDis(); }
            if (phi == null) { ComputePhi(); }
            if (topicToTopic == null) { ComputeTopicToTopic(); }

            int queryLength = rankDoc.doc.Length;
            rankDoc.wordTopic = new int[queryLength];
            double[,] dis = new double[queryLength, topicSum_K];
            int[,] preIndex = new int[queryLength, topicSum_K];
            for (int i = 0; i < topicSum_K; i++)
            {
                double maxDis = -1;
                for (int j = 0; j < topicSum_K; j++)
                {
                    double curDis = topicDis[j] *  topicToTopic[j][i];
                    if (curDis > maxDis)
                    {
                        maxDis = curDis;
                    }
                }
                dis[0, i] = maxDis * phi[i, rankDoc.doc[0]];
                preIndex[0, i] = -1;
            }

            for (int i = 1; i < queryLength; i++)
            {
                for (int j = 0; j < topicSum_K; j++)
                {
                    double maxDis = -1;
                    int index = -1;
                    for (int s = 0; s < topicSum_K; s++)
                    {
                        double curDis = dis[i - 1, s] * topicToTopic[s][j];
                        if (curDis > maxDis)
                        {
                            maxDis = curDis;
                            index = s;
                        }
                    }
                    dis[i, j] = maxDis *  phi[j, rankDoc.doc[i]];
                    preIndex[i, j] = index;
                }
            }

            int max = 0;
            for (int i = 0; i < topicSum_K; i++) 
            {
                if (dis[queryLength - 1, i] > dis[queryLength - 1, max]) { max = i; }
            }
            int[] rankTopicSum = new int[topicSum_K];
            for (int i = queryLength - 1; i >= 0; i--)
            {
                rankDoc.wordTopic[i] = max;
                rankTopicSum[max]++;
                max = preIndex[i, max];
            }
            double[] rankTheta = new double[topicSum_K];
            for (int i = 0; i < topicSum_K; i++) { rankTheta[i] = (rankTopicSum[i] + alpha) / (rankDoc.doc.Length + topicSum_K * alpha); }
            return rankTheta;
        }

        private void InitTopicDis()
        {
            topicDis = new double[topicSum_K];
            for (int i = 0; i < topicSum_K; i++) { topicDis[i] = 1.0 / topicSum_K; }
            for (int i = 0; i < docCount; i++)
            {
                for (int topic = 0; topic < topicSum_K; topic++)
                {
                    topicDis[topic] += docTopicSum[i, topic];
                }
            }
            double total = 0;
            for (int topic = 0; topic < topicSum_K; topic++) { total += topicDis[topic]; }
            for (int topic = 0; topic < topicSum_K; topic++) { topicDis[topic] /= total; }
        }

        public double[] GetRankDocumentTheta(LdaDocument rankDoc)
        {
            if (rankDoc.doc == null) { return null; }

            double[] rankDocTheta = new double[topicSum_K];
            # region InitRankDocumentState
            int[] rankDocTopicSum = new int[topicSum_K];
            int length = rankDoc.doc.Length;
            int[,] rankDocWordTopicSum = new int[length, topicSum_K];

            int[] wordTopic = new int[length];
            for (int i = 0; i < length; i++)
            {
                int topic = random.Next(topicSum_K);
                wordTopic[i] = topic;
                rankDocTopicSum[topic]++;
                rankDocWordTopicSum[i, topic]++;
            }
            rankDoc.wordTopic = wordTopic;

            int rankNumstats = 0;
            #endregion InitRankDocumentState

            for (int it = 0; it < ITERATIONS; it++)
            {
                #region RankDocSample
                if (rankDoc.wordTopic != null)
                {
                    double[] p = new double[topicSum_K];
                    for (int i = 0; i < rankDoc.doc.Length; i++)
                    {

                        int word = rankDoc.doc[i];
                        int topic = rankDoc.wordTopic[i];
                        rankDocTopicSum[topic]--;
                        rankDocWordTopicSum[i, topic]--;

                        for (int j = 0; j < topicSum_K; j++)
                        {
                            p[j] = (wordTopicSum[word, j] + rankDocWordTopicSum[i, j] + beta)
                                / (topicWordSum[j] + rankDocTopicSum[j] + vocbulrySize * beta) * (rankDocTopicSum[j] + alpha);
                        }
                        for (int k = 1; k < topicSum_K; k++) { p[k] += p[k - 1]; }
                        double u = random.NextDouble() * p[topicSum_K - 1];
                        for (topic = 0; topic < topicSum_K; topic++)
                        {
                            if (p[topic] > u) { break; }
                        }

                        rankDocWordTopicSum[i, topic]++;
                        rankDocTopicSum[topic]++;
                        rankDoc.wordTopic[i] = topic;
                    }
                }
                #endregion RankDocSample

                if ((it > BURN_IN) && (SAMPLE_LAG > 0) && (it % SAMPLE_LAG == 0))
                {
                    #region UpdateRankDocParams
                    for (int topic = 0; topic < topicSum_K; topic++)
                    {
                        rankDocTheta[topic] += ((rankDocTopicSum[topic] + alpha) / (rankDoc.doc.Length + topicSum_K * alpha));
                    }

                    rankNumstats++;
                    #endregion UpdateRankDocParams
                }
            }

            #region ComputRankDocTheta
            if (SAMPLE_LAG > 0)
            {
                for (int topic = 0; topic < topicSum_K; topic++)
                {
                    rankDocTheta[topic] = rankDocTheta[topic] / rankNumstats;
                }
            }
            else
            {
                for (int topic = 0; topic < topicSum_K; topic++)
                {
                    rankDocTheta[topic] = ((rankDocTopicSum[topic] + alpha) / (rankDoc.doc.Length + topicSum_K * alpha));
                }
            }
            #endregion ComputRankDocTheta
            Console.Write(".");
            return rankDocTheta;
        }
    }
}

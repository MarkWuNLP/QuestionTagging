using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class Training
    {
        int TAGFEATURENUM = 3;
        int QUESTIONFEATURENUM = 3;
        double loss = 0;
        double lastLoss = double.MaxValue;
        double learningrate = 0.01;
        double lamda = 0.001;
        double alpha = 0.5;
        int MAX_Iter = 0;
        double Stop_Gap = 0;

        Matrix[] tagSimFeatures; //xi
        Vector[] questionSimFeatures; //theta
        Vector tagSimWeights; //denoted by w in paper
        Vector questionSimWeights; // denoted by v in paper
        Matrix[] DeriviationH;
        Vector DerivateForPi;
        IFeatureExtractor featureExtractor;
        Matrix tagSim;
        Vector questionsim; 
        Vector tagsignificance;
        Matrix TranslationMatrix;

        Dictionary<string, double> tagScore = new Dictionary<string, double>();
        Dictionary<string, Vector> tagQuestionFeature = new Dictionary<string, Vector>();
        Dictionary<string, Vector[]> tagTagFeature = new Dictionary<string, Vector[]>();

        Dictionary<string, int> tagFrequency = new Dictionary<string, int>();
        List<Dictionary<string, int>> instanceCandidates = new List<Dictionary<string, int>>();
        List<Matrix[]> instanceTagSimFeatures = new List<Matrix[]>();
        List<Vector[]> instanceQuestionSimFeatures = new List<Vector[]>();
        Dictionary<string, double> tagQuestionSim = new Dictionary<string, double>();

        Dictionary<string, int> candidates = new Dictionary<string,int>();
        List<Question> questions;
        List<List<Question>> questionNeighbours;
        public void TrainingInstancesInit(List<Question> questions, List<List<Question>> questionNeighbours)
        {
            this.questions = questions;
            this.questionNeighbours = questionNeighbours;
            featureExtractor = new GoldenFeatureExtractor();
            for (int i = 0; i < questions.Count;i++ )
            {
                featureExtractor.GetTruth(questions[i].RelatedTags);
                ConstructCandidateTable(questionNeighbours[i]);
                SampleUnrelatedTag(questions[i],questionNeighbours[i]);
                ComputeQuestionSimFeature(questions[i],questionNeighbours[i]);
                ComputeTagSimFeature(instanceCandidates[instanceCandidates.Count-1]);
            }
           
        }
        /// <summary>
        /// this is the implementation of algorithm 1 in the paper
        /// </summary>
        /// <param name="QFeatureNum"></param>
        /// <param name="TFeatureNum"></param>
        /// <param name="MaxIter"></param>
        /// <param name="LearningRate"></param>
        /// <param name="Lamda"></param>
        /// <param name="StopGap"></param>
        public void Train(int QFeatureNum, int TFeatureNum, int MaxIter, double LearningRate, double Lamda, double StopGap)
        {
            this.QUESTIONFEATURENUM = QFeatureNum;
            this.TAGFEATURENUM = TFeatureNum;
            this.learningrate = LearningRate;
            this.lamda = Lamda;
            this.MAX_Iter = MaxIter;
            this.Stop_Gap = StopGap;

            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            tagSimWeights = (Vector)Vector.Build.Random(TAGFEATURENUM, normal);
            questionSimWeights = (Vector)Vector.Build.Random(QUESTIONFEATURENUM, normal);
            for (int t = 0; t < this.MAX_Iter; t++)
            {
                loss = 0;
                int incorrect_pair = 0;
                DenseVector Deriviate_W = new DenseVector(QUESTIONFEATURENUM);
                DenseVector Deriviate_V = new DenseVector(TAGFEATURENUM);
                for (int i = 0; i < questions.Count; i++)
                {
                    tagFrequency.Clear();
                    var neighbours = questionNeighbours[i];
                    var question = questions[i];
                    candidates = instanceCandidates[i];

                    if(t==0)
                        Init(question, neighbours);
                    else
                    {
                        tagSim = new DenseMatrix(candidates.Count, candidates.Count);
                        questionsim = new DenseVector(neighbours.Count);
                        tagSimFeatures = instanceTagSimFeatures[i];
                        questionSimFeatures = instanceQuestionSimFeatures[i];

                        tagQuestionFeature = new Dictionary<string, Vector>();
                        foreach (var candidate in candidates)
                        {
                            Vector tmp_v = new DenseVector(QUESTIONFEATURENUM);
                            for (int k = 0; k < QUESTIONFEATURENUM; k++)
                            {
                                for (int j = 0; j < neighbours.Count; j++)
                                {
                                    if (neighbours[j].RelatedTags.Contains(candidate.Key))
                                        tmp_v[k] = Math.Max(tmp_v[k], questionSimFeatures[k][j]);// We implement function g() by Max function
                                }
                            }
                            tagQuestionFeature.Add(candidate.Key, tmp_v);
                        }
                    }
                    ComputeTagSim(); 
                    DerivateForPi = null;
                    ComputeTagSignificance(neighbours);
                    ComputeQuestionSim(question, neighbours);
                    ComputeQuestionTagSimHeriusticlly(question);
                    ComputeDerivateofH();
                    RankingTags(question, neighbours);

                    foreach(var postag in question.RelatedTags)
                    {
                        if (!tagScore.ContainsKey(postag))
                            continue;
                        foreach(var negtag in question.UnRelatedTags)
                        {
                            if(tagScore[postag]<tagScore[negtag])
                            {
                                incorrect_pair += 1;
                                loss += -tagScore[postag] + tagScore[negtag];
                                Deriviate_W += DenseVector.OfVector(ComputeDeriviateOfW(postag, negtag));
                                Deriviate_V += DenseVector.OfVector(ComputeDeriviateOfV(postag, negtag));
                            }
                        }
                    }
                }
                for (int i = 0; i < QUESTIONFEATURENUM;i++)
                {
                    questionSimWeights[i] -= learningrate * (Deriviate_W[i]);
                }
                for (int i = 0; i < TAGFEATURENUM;i++)
                {
                    tagSimWeights[i] -= learningrate * 0.1 * (Deriviate_V[i]);
                }
                if(lastLoss-loss<StopGap)
                {
                    break;
                }
                else
                {
                    lastLoss = loss;
                }
                Console.Write("loss:{0}\tIncorrect_Pair{1}\t", loss,incorrect_pair);
                for (int i = 0; i < QUESTIONFEATURENUM; i++)
                { Console.Write("QuestionFeature{1}:{0}\t", questionSimWeights[i],i); }
                for (int i = 0; i < TAGFEATURENUM; i++)
                { Console.Write("TagFeature{1}:{0}\t", tagSimWeights[i],i); }
                Console.WriteLine();

            }
            PrintResult();
        }
        private void PrintResult()
        {
            StreamWriter sw = new StreamWriter("result.txt");
            for(int i=0;i<QUESTIONFEATURENUM;i++)
            {
                sw.Write("QFeature{0}:{1}\t", i, questionSimWeights[i]);
            }
            sw.WriteLine();
            for(int j=0;j<TAGFEATURENUM;j++)
            {
                sw.Write("TFeature{0}:{1}\t", j, tagSimWeights[j]);
            }
            sw.Close();
        }
        /// <summary>
        /// equation 10 in my paper
        /// </summary>
        /// <param name="postag"></param>
        /// <param name="negtag"></param>
        /// <returns></returns>
        private Vector ComputeDeriviateOfV(string postag, string negtag)
        {
            Vector posderivation = DenseVector.OfVector(tagsignificance[candidates[postag]] * ComputeDerivateOfVForTag(postag)
                + tagScore[postag]/tagsignificance[candidates[postag]] * ComputeDerivateOfPiForTag(postag));
            Vector negderivation = DenseVector.OfVector(tagsignificance[candidates[negtag]] * ComputeDerivateOfVForTag(negtag)
                + tagScore[negtag] / tagsignificance[candidates[negtag]] * ComputeDerivateOfPiForTag(negtag));

            return (Vector)(negderivation - posderivation);
        }
        /// <summary>
        /// This is euqation 12 in my paper
        /// </summary>
        /// <param name="postag"></param>
        /// <returns></returns>
        private Vector ComputeDerivateOfPiForTag(string postag)
        {
            var DerivateOfA = ComputeDerivateOfA();
            Vector res = new DenseVector(TAGFEATURENUM);
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                //if(DerivateForPi==null)
                {
                    DerivateForPi = DenseVector.OfVector(alpha * (SparseMatrix.CreateIdentity(candidates.Count) - alpha * TranslationMatrix).Inverse() * DerivateOfA[i] * tagsignificance);
                }
                res[i] = DerivateForPi[candidates[postag]];
            }
            return res;
        }

        /// <summary>
        /// equation 13 in the paper. This function gets the derivative of A respect to V
        /// </summary>
        /// <returns></returns>
        private Matrix[] ComputeDerivateOfA()
        {
            Matrix[] derivateofA = new Matrix[TAGFEATURENUM];
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                derivateofA[i] = new DenseMatrix(candidates.Count, candidates.Count);
                for (int row = 0; row < candidates.Count; row++)
                {
                    for (int col = 0; col < candidates.Count; col++)
                    {
                        double firstterm = this.DeriviationH[i][row, col] * tagSim.Column(col).Sum();
                        double secondterm = tagSim[row, col] * this.DeriviationH[i].Column(col).Sum();
                        derivateofA[i][row,col] = firstterm-secondterm;
                    }
                }
            }
            return derivateofA;
        }

        private void ComputeDerivateofH()
        {
            this.DeriviationH = new Matrix[TAGFEATURENUM];
            List<string> tags = candidates.Keys.ToList();
            double weightsum = 0;
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                weightsum += Math.Exp(tagSimWeights[i]);
            }
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                this.DeriviationH[i] = new DenseMatrix(tags.Count, tags.Count);
                for(int row=0;row<tags.Count;row++)
                {
                    for(int col = 0;col<tags.Count;col++)
                    {
                        double firstterm = 0, secondterm = 0;
                        firstterm += Math.Exp(tagSimWeights[i]) * tagTagFeature[tags[row]][i][candidates[tags[col]]] * weightsum;
                        double tmp = 0;
                        for (int j = 0; j < TAGFEATURENUM; j++)
                        {
                            tmp += Math.Exp(tagSimWeights[j]) * tagTagFeature[tags[row]][j][candidates[tags[col]]];
                        }
                        secondterm += tmp * Math.Exp(tagSimWeights[i]);
                        this.DeriviationH[i][row, col] = (firstterm - secondterm) / (weightsum * weightsum);
                    }
                }
            }
        }

        private Vector ComputeDerivateOfVForTag(string tag)
        {
            Vector res = new DenseVector(QUESTIONFEATURENUM);
            double weightsum = 0;
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                weightsum += Math.Exp(tagSimWeights[i]);
            }
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                double firstterm = 0, secondterm = 0;
                foreach(var tag2 in tagQuestionSim)
                {
                    if(tag2.Value!=0)
                    {
                        firstterm += Math.Exp(tagSimWeights[i]) * tagTagFeature[tag][i][candidates[tag2.Key]] *weightsum;
                        double tmp = 0;
                        for (int j = 0; j < TAGFEATURENUM; j++)
                        {
                            tmp += Math.Exp(tagSimWeights[j]) * tagTagFeature[tag][j][candidates[tag2.Key]];
                        }
                        secondterm += tmp * Math.Exp(tagSimWeights[i]);
                    }
                }
                res[i] = (firstterm - secondterm)/(weightsum*weightsum);
            }
            return res;
        }


        /// <summary>
        /// Equation 7 in paper
        /// </summary>
        /// <param name="postag"></param>
        /// <param name="negtag"></param>
        /// <returns></returns>
        private Vector ComputeDeriviateOfW(string postag, string negtag)
        {
            Vector posderivation = ComputeDerivateOfWForTag(postag);
            Vector negderivation = ComputeDerivateOfWForTag(negtag);

            return (Vector)(tagsignificance[candidates[negtag]] * negderivation 
                - tagsignificance[candidates[postag]] * posderivation);
        }
        /// <summary>
        /// equation 6 in my paper
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private Vector ComputeDerivateOfWForTag(string tag)
        {
            Vector res = new DenseVector(QUESTIONFEATURENUM);
            double weightsum = 0;
            for (int i = 0; i < QUESTIONFEATURENUM; i++)
            {
                weightsum += Math.Exp(questionSimWeights[i]);
            }
            for (int i = 0; i < QUESTIONFEATURENUM; i++)
            {
                double firstterm=0;
                if(tagQuestionFeature.ContainsKey(tag))
                    firstterm = Math.Exp(questionSimWeights[i]) * tagQuestionFeature[tag][i] * weightsum;
                else
                {
                    //Console.WriteLine(tag);
                }
                double tmp = 0;
                for (int j = 0; j < QUESTIONFEATURENUM; j++)
                {
                    tmp += Math.Exp(questionSimWeights[j]) * tagQuestionFeature[tag][j];
                }
                var secondterm = Math.Exp(questionSimWeights[i]) * tmp;
                res[i] = firstterm - secondterm;
            }
            return res;
        }

        private void SampleUnrelatedTag(Question question, List<Question> neighbours)
        {
            tagFrequency.Clear();
            foreach (var neighbour in neighbours)
            {
                foreach (var tag in neighbour.RelatedTags)
                {
                    if (tagFrequency.ContainsKey(tag))
                        tagFrequency[tag]++;
                    else
                        tagFrequency.Add(tag, 1);
                }
            }

            question.UnRelatedTags = new List<string>();
            var orderedtag = tagFrequency.OrderByDescending(x => x.Value).ToArray();
            for(int i=0;i<question.RelatedTags.Count;i++)
            {
                question.UnRelatedTags.Add(orderedtag[i].Key);
            }
        }

        private void ComputeTagSimFeature(Dictionary<string, int> dictionary)
        {
            this.instanceTagSimFeatures.Add(this.featureExtractor.ExtractTagSim(dictionary.Keys.ToList()));
        }

        private void ComputeQuestionSimFeature(Question question, List<Question> list)
        {
            this.instanceQuestionSimFeatures.Add(this.featureExtractor.ExtractQuestionSim(question, list));
        }

        private void Init(Question question, List<Question> neighbours)
        {
            //Console.WriteLine("!!!");
      
            tagSimFeatures = instanceTagSimFeatures[this.questions.IndexOf(question)];
            questionSimFeatures = instanceQuestionSimFeatures[this.questions.IndexOf(question)];

            tagQuestionFeature = new Dictionary<string, Vector>();
            foreach(var candidate in candidates)
            {
                Vector tmp_v = new DenseVector(QUESTIONFEATURENUM);
                for (int i = 0; i < QUESTIONFEATURENUM;i++)
                {
                    for(int j=0;j<neighbours.Count;j++)
                    {
                        if(neighbours[j].RelatedTags.Contains(candidate.Key))
                            tmp_v[i] = Math.Max(tmp_v[i], questionSimFeatures[i][j]);
                    }
                }
                tagQuestionFeature.Add(candidate.Key, tmp_v);
            }

            tagSim = new DenseMatrix(candidates.Count, candidates.Count);
            questionsim = new DenseVector(neighbours.Count);
        }

        private void ComputeTagSim()
        {
            List<string> tags = candidates.Keys.ToList();
            tagTagFeature.Clear();
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                for (int rowindex = 0; rowindex < candidates.Count; rowindex++)
                {
                    Vector vector = new DenseVector(candidates.Count);
                    for (int columnindex = 0; columnindex < candidates.Count; columnindex++)
                    {
                        tagSim[rowindex, columnindex] += tagSimFeatures[i][rowindex, columnindex] * tagSimWeights[i];
                        vector[columnindex] = tagSimFeatures[i][rowindex, columnindex];
                    }
                    if(i==0)
                    {
                        tagTagFeature.Add(tags[rowindex],new Vector[TAGFEATURENUM]);
                    }
                    tagTagFeature[tags[rowindex]][i] = vector;
                }

            }
        }
        /// <summary>
        /// this is function f in the paper
        /// </summary>
        /// <param name="question"></param>
        private void ComputeQuestionTagSimHeriusticlly(Question question)
        {
            tagQuestionSim = new Dictionary<string, double>();
            foreach (var candidate in candidates)
            {
                tagQuestionSim.Add(candidate.Key,
                    Utils.LanguageModel(question.StemWords, candidate.Key));

            }
        }
        /// <summary>
        /// equation 2 in the paper
        /// </summary>
        /// <param name="question"></param>
        /// <param name="neighbours"></param>
        private void ComputeQuestionSim(Question question, List<Question> neighbours)
        {
            double sum = 0;
            for (int i = 0; i < QUESTIONFEATURENUM; i++)
            {
                sum += Math.Exp(questionSimWeights[i]);
            }
            for (int j = 0; j < neighbours.Count; j++)
            {
                for (int i = 0; i < QUESTIONFEATURENUM; i++)
                {
                    questionsim[j] += Math.Exp(questionSimWeights[i]) * questionSimFeatures[i][j] / sum;
                }
            }
        }
        /// <summary>
        /// equation 1 in the paper
        /// </summary>
        /// <param name="question"></param>
        /// <param name="neighbours"></param>
        private void RankingTags(Question question, List<Question> neighbours)
        {
            tagScore.Clear();
            foreach (var candidate in candidates)
            {
                double q_t = 0;
                double q_q_t = 0;
                double q_t_t = 0;

                int tagindex = candidates[candidate.Key];
                q_t = tagQuestionSim[candidate.Key];
                foreach (var neighbour in neighbours)
                {
                    if (neighbour.RelatedTags.Contains(candidate.Key))
                    {
                        if(q_q_t < questionsim[neighbours.IndexOf(neighbour)])
                            q_q_t = questionsim[neighbours.IndexOf(neighbour)];
                    }
                }
                foreach (var tag in tagQuestionSim)
                {
                    double f_qt = tag.Value;
                    int t_index = candidates[tag.Key];
                    if (tagindex != t_index)
                    {
                        q_t_t += tagSim[tagindex, t_index]*tag.Value;
                    }
                }
                tagScore.Add(candidate.Key, tagsignificance[candidate.Value] * (q_q_t + q_t + q_t_t));
            }
        }

        private void ConstructCandidateTable(List<Question> neighbours)
        {
            candidates = new Dictionary<string, int>();
            foreach (var neighbour in neighbours)
            {
                foreach (var tag in neighbour.RelatedTags)
                {
                    if (!candidates.ContainsKey(tag))
                    {
                        candidates.Add(tag, candidates.Count);
                    }
                }
            }
            instanceCandidates.Add(candidates);
        }
        /// <summary>
        /// pagerank score for each tag equation 4 in the paper
        /// </summary>
        /// <param name="neighbours"></param>
        private void ComputeTagSignificance(List<Question> neighbours)
        {
            var restartprob = Vector.Build.Dense(candidates.Count);
            TranslationMatrix = new DenseMatrix(candidates.Count, candidates.Count);

            foreach (var neighbour in neighbours)
            {
                foreach (var tag in neighbour.RelatedTags)
                {
                    if (tagFrequency.ContainsKey(tag))
                        tagFrequency[tag]++;
                    else
                        tagFrequency.Add(tag, 1);
                }
            }

            int i = 0;
            foreach (var candidate in candidates)
            {
                restartprob[i++] = tagFrequency[candidate.Key];
            }
            restartprob = restartprob.Normalize(1);

            i = 0;
            for (int rowindex = 0; rowindex < TranslationMatrix.RowCount; rowindex++)
                for (int columnindex = 0; columnindex < TranslationMatrix.ColumnCount; columnindex++)
                {
                    TranslationMatrix[rowindex, columnindex] += tagSim[rowindex, columnindex];
                }

            TranslationMatrix = DenseMatrix.OfMatrix(TranslationMatrix.NormalizeColumns(1));
            tagsignificance = Utils.PageRank(TranslationMatrix, restartprob);
        }
    }
}

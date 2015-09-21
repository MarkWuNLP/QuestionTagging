using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class Training
    {
        const int TAGFEATURENUM = 3;
        const int QUESTIONFEATURENUM = 3;

        Matrix[] tagSimFeatures;
        Vector[] questionSimFeatures;
        double[] tagSimWeights;
        double[] questionSimWeights;

        Matrix tagSim;
        Vector questionsim;
        Vector tagsignificance;
        FeatureExtractor featureExtractor = new FeatureExtractor();

        Dictionary<string, double> tagScore = new Dictionary<string, double>();
        Dictionary<string, int> tagFrequency = new Dictionary<string, int>();
        List<Dictionary<string, int>> instanceCandidates = new List<Dictionary<string, int>>();
        List<Matrix[]> instanceTagSimFeatures = new List<Matrix[]>();
        List<Vector[]> instanceQuestionSimFeatures = new List<Vector[]>();
        Dictionary<string, double> tagQuestionSim = new Dictionary<string, double>();

        Dictionary<string, int> candidates;

        public void TrainingInstancesInit(List<Question> questions, List<List<Question>> questionNeighbours)
        {
            for (int i = 0; i < questions.Count;i++ )
            {
                ConstructCandidateTable(questionNeighbours[i]);
                SampleUnrelatedTag(questions[i],questionNeighbours[i]);
                ComputeQuestionSimFeature(questions[i],questionNeighbours[i]);
                ComputeTagSimFeature(instanceCandidates[instanceCandidates.Count]);
            }
            for(int t= 0; t<300;t++)
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    var neighbours = questionNeighbours[i];
                    var question = questions[i];   
                    candidates = instanceCandidates[i];
                    ComputeTagSim();
                    ComputeTagSignificance(neighbours);
                    ComputeQuestionSim(question, neighbours);
                    ComputeQuestionTagSimHeriusticlly(question);
                    RankingTags(question, neighbours);
                }
            }
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
            var orderedtag = tagFrequency.OrderByDescending(x => x.Value).ToArray();
            for(int i=0;i<question.RelatedTags.Count;i++)
            {
                question.UnRelatedTags.Add(orderedtag[i].Key);
            }
        }

        private void ComputeTagSimFeature(Dictionary<string, int> dictionary)
        {
            this.instanceTagSimFeatures.Add(featureExtractor.ExtractTagSim(dictionary.Keys.ToList()));
        }

        private void ComputeQuestionSimFeature(Question question, List<Question> list)
        {
            this.instanceQuestionSimFeatures.Add(featureExtractor.ExtractQuestionSim(question, list));
        }

        private void ComputeTagSim()
        {
            //var candidates = instanceCandidates[instanceCandidates.Count];
            tagSim = new DenseMatrix(candidates.Count, candidates.Count);
            for (int i = 0; i < TAGFEATURENUM; i++)
            {
                for (int rowindex = 0; rowindex < candidates.Count; rowindex++)
                    for (int columnindex = 0; columnindex < candidates.Count; columnindex++)
                    {
                        tagSim[rowindex, columnindex] += tagSimFeatures[i][rowindex, columnindex] * tagSimWeights[i];
                    }
            }
        }

        private void ComputeQuestionTagSimHeriusticlly(Question question)
        {
            foreach (var candidate in candidates)
            {
                tagQuestionSim.Add(candidate.Key,
                    Utils.LanguageModel(question.StemWords, candidate.Key));

            }
        }

        private void ComputeQuestionSim(Question question, List<Question> neighbours)
        {
            for (int j = 0; j < neighbours.Count; j++)
            {
                for (int i = 0; i < QUESTIONFEATURENUM; i++)
                {
                    questionsim[j] += questionSimWeights[i] * questionSimFeatures[i][j];
                }
            }
        }

        private void RankingTags(Question question, List<Question> neighbours)
        {
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
                        q_q_t += questionsim[neighbours.IndexOf(neighbour)];
                    }
                }
                foreach (var tag in tagQuestionSim)
                {
                    double f_qt = tag.Value;
                    int t_index = candidates[tag.Key];
                    if (tagindex != t_index)
                    {
                        q_t_t += tagSim[tagindex, t_index];
                    }
                }
                tagScore.Add(candidate.Key, tagsignificance[candidate.Value] * (q_q_t + q_t + q_t_t));
            }
        }

        private void ConstructCandidateTable(List<Question> neighbours)
        {
            Dictionary<string, int> candidates = new Dictionary<string, int>();
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
        /// pagerank score for each tag
        /// </summary>
        /// <param name="neighbours"></param>
        private void ComputeTagSignificance(List<Question> neighbours)
        {
            tagFrequency.Clear();
            var restartprob = Vector.Build.Dense(candidates.Count);
            var TranslationMatrix = Matrix.Build.Dense(candidates.Count, candidates.Count);

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
                restartprob[i] = tagFrequency[candidate.Key];
            }
            restartprob = restartprob.Normalize(1);


            for (int rowindex = 0; rowindex < TranslationMatrix.RowCount; rowindex++)
                for (int columnindex = 0; columnindex < TranslationMatrix.ColumnCount; columnindex++)
                {
                    TranslationMatrix[rowindex, columnindex] += tagSim[rowindex, columnindex] * tagSimWeights[i];
                }

            TranslationMatrix = TranslationMatrix.NormalizeColumns(1);
            tagsignificance = Utils.PageRank(TranslationMatrix, restartprob);
        }
    }
}

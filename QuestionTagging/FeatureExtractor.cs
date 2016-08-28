using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    interface IFeatureExtractor
    {
        Matrix[] ExtractTagSim(List<string> tags);
        Vector[] ExtractQuestionSim(Question q, List<Question> neighbour);
        void GetTruth(List<string> tags);
    }
    class GoldenFeatureExtractor:IFeatureExtractor
    {
        int QFeatureNum = int.Parse(ConfigurationManager.AppSettings["QFeatureNum"]);
        int TFeatureNum = int.Parse(ConfigurationManager.AppSettings["TFeatureNum"]);
        List<string> TruthTags = new List<string>();
        public void GetTruth(List<string> tags)
        {
            TruthTags = tags;
        }

        public Matrix[] ExtractTagSim(List<string> tags)
        {
            Matrix[] tagsimefeature = new Matrix[TFeatureNum];
            for (int i = 0; i < TFeatureNum; i++)
            {
                tagsimefeature[i] = RandomTagFeature(tags.Count, tags.Count);
            }

            for(int i=0;i<tags.Count;i++)
            {
                for (int j=0;j<tags.Count;j++)
                {
                    if(TruthTags.Contains(tags[i]))
                        tagsimefeature[0][i, j] = 1;
                }
            }

            return tagsimefeature;
        }

        private Matrix RandomTagFeature(int rowcount, int columncount)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Matrix)Matrix.Build.Random(rowcount, columncount, normal);
        }




        public Vector[] ExtractQuestionSim(Question q, List<Question> neighbour)
        {
            Vector[] questionsimfeature = new Vector[QFeatureNum];

            for (int i = 0; i < QFeatureNum; i++)
            {
                Vector v = RandomQuestionFeature(neighbour.Count);
                questionsimfeature[i] = v;
            }

            for (int i = 0; i < neighbour.Count; i++)
            {
                foreach (var t in TruthTags)
                {
                    if (neighbour[i].RelatedTags.Contains(t))
                        questionsimfeature[0][i] = 1;
                }
            }

            return questionsimfeature;
        }

        private Vector RandomQuestionFeature(int count)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Vector)Vector.Build.Random(count, normal);
        }
    }

    class RandomFeatureExtractor:IFeatureExtractor
    {
        int QFeatureNum = int.Parse(ConfigurationManager.AppSettings["QFeatureNum"]);
        int TFeatureNum = int.Parse(ConfigurationManager.AppSettings["TFeatureNum"]);
        public Matrix[] ExtractTagSim(List<string> tags)
        {
            Matrix[] tagsimefeature = new Matrix[TFeatureNum];
            for (int i = 0; i < TFeatureNum;i++)
            {
                tagsimefeature[i] = RandomTagFeature(tags.Count,tags.Count);
            }

            return tagsimefeature;
        }

        private Matrix RandomTagFeature(int rowcount,int columncount)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Matrix) Matrix.Build.Random(rowcount,columncount,normal);
        }
        public void GetTruth(List<string> tags)
        {
            ;
        }




        public Vector[] ExtractQuestionSim(Question q, List<Question> neighbour)
        {
            Vector[] questionsimfeature = new Vector[QFeatureNum];

            for (int i = 0; i < QFeatureNum ;i++)
            {
                Vector v = RandomQuestionFeature(neighbour.Count);
                 questionsimfeature[i] = v;
            }

            return questionsimfeature;
        }

        private Vector RandomQuestionFeature(int count)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Vector)Vector.Build.Random(count, normal);
        }
    }
}

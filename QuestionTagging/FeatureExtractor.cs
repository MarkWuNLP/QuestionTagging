using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    interface IFeatureExtractor
    {
        Matrix[] ExtractTagSim(List<string> tags);
        Vector[] ExtractQuestionSim(Question q, List<Question> neighbour);
    }
    class RandomFeatureExtractor:IFeatureExtractor
    {
        public Matrix[] ExtractTagSim(List<string> tags)
        {
            Matrix[] tagsimefeature = new Matrix[3];
            for (int i = 0; i < 3;i++)
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




        public Vector[] ExtractQuestionSim(Question q, List<Question> neighbour)
        {
            Vector[] questionsimfeature = new Vector[3];

            for (int i = 0; i < 3;i++)
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

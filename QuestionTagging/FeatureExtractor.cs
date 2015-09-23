using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class FeatureExtractor
    {
        public static Matrix[] ExtractTagSim(List<string> tags)
        {
            Matrix[] tagsimefeature = new Matrix[3];
            for (int i = 0; i < 3;i++)
            {
                tagsimefeature[i] = RandomTagFeature(tags.Count,tags.Count);
            }

            return tagsimefeature;
        }

        private static Matrix RandomTagFeature(int rowcount,int columncount)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Matrix) Matrix.Build.Random(rowcount,columncount,normal);
        }




        public static Vector[] ExtractQuestionSim(Question q, List<Question> neighbour)
        {
            Vector[] questionsimfeature = new Vector[3];

            for (int i = 0; i < 3;i++)
            {
                Vector v = RandomQuestionFeature(neighbour.Count);
                 questionsimfeature[i] = v;
            }

            return questionsimfeature;
        }

        private static Vector RandomQuestionFeature(int count)
        {
            MathNet.Numerics.Distributions.ContinuousUniform normal = new MathNet.Numerics.Distributions.ContinuousUniform();
            return (Vector)Vector.Build.Random(count, normal);
        }
    }
}

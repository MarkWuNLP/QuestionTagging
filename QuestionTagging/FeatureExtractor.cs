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
        public Matrix[] ExtractTagSim(List<string> tags)
        {
            Matrix[] tagsimefeature = new Matrix[3];

            return tagsimefeature;
        }

        public Vector[] ExtractQuestionSim(Question q, List<Question> neighbour)
        {
            Vector[] questionsimfeature = new Vector[3];

            return questionsimfeature;
        }

     
    }
}

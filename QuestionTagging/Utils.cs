using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class Utils
    {
        const double beta = 0.5;

        public static Vector PageRank(MathNet.Numerics.LinearAlgebra.Matrix<double> A, MathNet.Numerics.LinearAlgebra.Vector<double> z)
        {
            Vector pi;
            var inversematrix = (SparseMatrix.CreateIdentity(z.Count) - beta * A).Inverse();
            pi = (Vector)(inversematrix * (1 - beta) * z);
            return pi;
        }

        /// <summary>
        /// The function is a simplier language model~ You can replace it with any similarity function
        /// </summary>
        /// <param name="QuestionStem"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static double LanguageModel(List<string> QuestionStem, string tag)
        {
              double res = 0;
     
            List<string> list = tag.Split(' ').ToList();
            for (int i = 0; i < list.Count;i++)
            {
                bool b = true;
                if (!QuestionStem.Contains(list[i]))
                {
                    b = false;
                    break;
                }
                if (b == true)
                {
                    res = 1 / (double)QuestionStem.Count;
                }
            }

            return res ;
        }
    }
}

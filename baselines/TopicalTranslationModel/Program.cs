using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicalTranslationModel
{
    class Program
    {
        static string trainingDataDir = @"E:\v-wuyu\Data\ZhihuLDA\RawText";
        static void Main(string[] args)
        {
            int iter = 1000, burnIn = 100, sampleLag = 10;
            int topicNum_K = 30;
            string modelDir = @"E:\v-wuyu\Data\ZhihuLDA\Model";

            string wordDictFile = @"E:\v-wuyu\Data\ZhihuLDA\LDADic.txt";

            string[] inpudFiles = null;
            DirectoryInfo encodeInfo = new DirectoryInfo(trainingDataDir);
            FileInfo[] filesInfo = encodeInfo.GetFiles();
            inpudFiles = new string[filesInfo.Length];
            for (int i = 0; i < filesInfo.Length; i++)
                inpudFiles[i] = filesInfo[i].FullName;

            GibbsLDA.Entity.LdaDocument[] documents = null;
            GibbsLDA.PreProcess.DataPreProcess process = new GibbsLDA.PreProcess.DataPreProcess();
            process.LoadData(inpudFiles, out documents);
            process = null;
            inpudFiles = null;

            int v = GibbsLDA.PreProcess.Data.GetVocabularySize(wordDictFile);
            GibbsLDA.Models.GibbsLDA lda = new GibbsLDA.Models.GibbsLDA(documents, v);
            lda.Configure(iter, burnIn, sampleLag);
            double alpha = 30 / (double)topicNum_K;
            double beta = 0.01;
            string modelfile = Path.Combine(modelDir, "model.dat");
            string docfile = Path.Combine(modelDir, "doc.dat");
            lda.Gibbs(topicNum_K, alpha, beta, modelfile);
        }
    }
}

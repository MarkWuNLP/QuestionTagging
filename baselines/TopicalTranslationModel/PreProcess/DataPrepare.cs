using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GibbsLDA.PreProcess
{
    public class DataPrepare
    {
        public int COPYNUM = 100;

        public bool PrepareSourceData(string inputDataRootDir, string outputDataDir)
        {
            if (!Directory.Exists(inputDataRootDir)) { return false; }
            if (!Directory.Exists(outputDataDir)) { Directory.CreateDirectory(outputDataDir); }

            DirectoryInfo inputDirInfo = new DirectoryInfo(inputDataRootDir);
            var cateDirsInfo = inputDirInfo.GetDirectories("cate*");
            foreach (var cateInfo in cateDirsInfo)
            {
                string fullName = cateInfo.FullName;
                string name = cateInfo.Name;
                for (int i = 0; i < COPYNUM; i++)
                {
                    string queFileName = "C" + i + "Question.dat";
                    string fromQueFilePath = Path.Combine(fullName, queFileName);
                    string ansFileName = "C" + i + "Answer.dat";
                    string fromAnsFilePath = Path.Combine(fullName, ansFileName);

                    if (!File.Exists(fromQueFilePath) || !File.Exists(fromAnsFilePath))
                    {
                        Console.WriteLine("file {0} or file {1} not exist", fromQueFilePath, fromAnsFilePath);
                        continue;
                    }

                    string toQueFilePath = Path.Combine(outputDataDir, name + "_" + queFileName);
                    File.Copy(fromQueFilePath, toQueFilePath, true);
                    string toAnsFilePath = Path.Combine(outputDataDir, name + "_" + ansFileName);
                    File.Copy(fromAnsFilePath, toAnsFilePath, true);
                }
                Console.WriteLine("{0} complete", name);
            }

            return true;
        }

        //public bool PrepareSourceData(string inputDataRootDir, string outputDataDir)
        //{
        //    if (!Directory.Exists(inputDataRootDir)) { return false; }
        //    if (!Directory.Exists(outputDataDir)) { Directory.CreateDirectory(outputDataDir); }

        //    DirectoryInfo inputDirInfo = new DirectoryInfo(inputDataRootDir);
        //    var cateDirsInfo = inputDirInfo.GetDirectories("cate*");
        //    foreach(var cateInfo in cateDirsInfo)
        //    {
        //        string fullName = cateInfo.FullName;
        //        string name = cateInfo.Name;
        //        for (int i = 0; i < COPYNUM; i++)
        //        {
        //            string queFileName = "C" + i + "Question.dat";
        //            string fromQueFilePath = Path.Combine(fullName, queFileName);

        //            if (!File.Exists(fromQueFilePath))
        //            {
        //                Console.WriteLine("file {0} or file {1} not exist", fromQueFilePath);
        //                continue;
        //            }

        //            string toQueFilePath = Path.Combine(outputDataDir, name + "_" + queFileName);
        //            File.Copy(fromQueFilePath, toQueFilePath, true);
        //        }
        //        Console.WriteLine("{0} complete", name);
        //    }

        //    return true;
        //}
    }
}

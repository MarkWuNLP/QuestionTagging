﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class Program
    {
        static void Test()
        {
            StreamReader sr = new StreamReader(@"..\..\..\resource\test.txt");
            while(!sr.EndOfStream)
            {
                Question q=null;
                List<Question> neighbours = new List<Question>();
                string question = sr.ReadLine();
                string[] q_tmp = question.Split('\t');
                q = new Question(q_tmp[0]
                    ,q_tmp[2].Split('|').ToList()
                    ,q_tmp[1].Split(new string[]{"; "},StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                Console.WriteLine(q_tmp[0]);
                while(true)
                {
                    Question n;
                    string neighbour = sr.ReadLine();
                    if (neighbour == "###")
                        break;
                    else
                    {
                        string[] tmp = neighbour.Split('\t');
                        if(tmp.Length<3)
                        {
                            //Console.WriteLine(neighbour);
                            continue;
                        }
                        n = new Question(tmp[0], tmp[2].Split('|').ToList(),
                            tmp[1].Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList());
                        
                    }
                    neighbours.Add(n);
                }
                QuestionTagging tagger = new QuestionTagging();
                Console.WriteLine("Tags:");
                tagger.TagQuestion(q,neighbours.GetRange(0,50));
            }
        }

        static void Train()
        {
            List<Question> TrainingInstances = new List<Question>();
            List<List<Question>> InstancesNeighbours = new List<List<Question>>();
            StreamReader sr = new StreamReader(@"..\..\..\resource\train.txt");
            while (!sr.EndOfStream)
            {
                Question q = null;
                List<Question> neighbours = new List<Question>();
                string question = sr.ReadLine();
                string[] q_tmp = question.Split('\t');
                q = new Question(q_tmp[0]
                    , q_tmp[2].Split('|').ToList()
                    , q_tmp[1].Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
                while (true)
                {
                    Question n;
                    string neighbour = sr.ReadLine();
                    if (neighbour == "###")
                        break;
                    else
                    {
                        string[] tmp = neighbour.Split('\t');
                        if (tmp.Length < 3)
                            continue;
                        n = new Question(tmp[0], tmp[2].Split('|').ToList(),
                            tmp[1].Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList());

                    }
                    neighbours.Add(n);
                }
                TrainingInstances.Add(q);
                InstancesNeighbours.Add(neighbours.GetRange(0,Math.Min(neighbours.Count,50)));
                if (TrainingInstances.Count > 100)
                    break;
                Console.WriteLine("Training Instance:" + TrainingInstances.Count);
               // QuestionTagging tagger = new QuestionTagging();
               // tagger.TagQuestion(q, neighbours.GetRange(0, 50));
            }

            Training t = new Training();
            t.TrainingInstancesInit(TrainingInstances, InstancesNeighbours);
            int QFeatureNum = int.Parse(ConfigurationManager.AppSettings["QFeatureNum"]);
            int TFeatureNum = int.Parse(ConfigurationManager.AppSettings["TFeatureNum"]);
            int Max_Iter = int.Parse(ConfigurationManager.AppSettings["Max_Iter"]);
            double LearningRate = double.Parse(ConfigurationManager.AppSettings["LearningRate"]);
            double Lamda = double.Parse(ConfigurationManager.AppSettings["Lamda"]);
            double StopGap = double.Parse(ConfigurationManager.AppSettings["StopGap"]);
            double decay = double.Parse(ConfigurationManager.AppSettings["decay"]);

            t.Train(QFeatureNum, TFeatureNum, Max_Iter, LearningRate, Lamda, StopGap,decay);

            StreamWriter sw = new StreamWriter("questionfeature.weight");
            for(int i=0;i<QFeatureNum;i++)
            {
                sw.WriteLine(t.questionSimWeights[i]);
                Console.WriteLine(t.questionSimWeights[i]);
            }
            sw.Close();
            sw = new StreamWriter("tagfeature.weight");
            for (int i = 0; i < QFeatureNum; i++)
            {
                sw.WriteLine(t.questionSimWeights[i]);
                Console.WriteLine(t.tagSimWeights[i]);
            }
            sw.Close();
        }

        static void Main(string[] args)
        {            
            //Program.Train();

            Program.Test();
        }
    }
}

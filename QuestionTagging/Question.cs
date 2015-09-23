using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionTagging
{
    class Question
    {
        public string QuestionText { get; set; }
        public List<string> StemWords { get; set; }
        public List<string> RelatedTags { get; set; }
        public List<string> UnRelatedTags { get; set; }

        public Question(string qtext,List<string> stemwords,List<string> tags)
        {
            this.QuestionText = qtext;
            this.StemWords = stemwords;
            this.RelatedTags = tags;
        }
    }
}

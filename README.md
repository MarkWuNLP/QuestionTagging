# QuestionTagging
Source code of our AAAI 2016 paper

This is a Question tagging algorithm based on Supervised PageRank FrameWork. The users should extracted the question similarity
and tag similarity first. Then the project provides weight training component and tag predict component. The users should train feature weights using training.cs and set the parameter tagsimweighs and questionsimweights in Questiontagging.cs when tagging questions.  

## Input
* Input file format should follows the files format in the resource directory, which is  
  raw question \t tags \t question stem word
* The users should implement feature extractor part for quesiton tagging, and also could replace the original ComputeQuestionTagSimHeriusticlly() function as want he wants.

## Usage
* I provide training and test component in this project. You could call Progrom.Train() to learn question and tag weights based on the training corpus. After acquiring weights, you could modify weights in QuestionTagging file and Run Tagging algorithm.

##Output
  In training process, the learning weights result will be printed on the Console. 
  In question tagging process, the predicted tags for a question are shown ordered by their score

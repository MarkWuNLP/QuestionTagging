# QuestionTagging
Source code of AAAI 2016 submitted paper

This is a Question tagging algorithm based on Supervised PageRank FrameWork. The users should extracted the question similarity
and tag similarity first. Then the project provides weight training component and tag predict component. The users should train feature weights using training.cs and set the parameter tagsimweighs and questionsimweights in Questiontagging.cs when tagging questions.  

## Input
* Input file format should follows the files format in the resource directory, which is  
  raw question \t tags \t question stem word
* The users should implement feature extractor part for quesiton tagging, and also could replace the original ComputeQuestionTagSimHeriusticlly() function as want he wants.
 

Now The Author is busy writting comments for the algorithm to correspond with the equations in our paper.

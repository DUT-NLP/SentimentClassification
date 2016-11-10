using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace wordExtraction
{
    public class SerializerForBigLSTM
    {   
        public static int dimension = 100;
        public static String type = ConfigurationManager.AppSettings["dataType"];
        public static int wordNum = 0;
        public static String sentiDim = "100d/";
        public static int num = 1;

        public static void Serialize()
        {

            //String corpusPath = "G:/liuzhuang/corpus_newDict_AddMoreNegativeWords/";
            //String vectorTablePath = "G:/liuzhuang/corpus/";

            String corpusPath = "I:/liuzhuang/sentimentClassification_corpus_addAbstraction/";
            String vectorTablePath = "G:/liuzhuang/corpus/";

            String IndexEmbedPath = corpusPath + "data/";
            String corpusPath_en = corpusPath + "en/";
            String corpusPath_cn = corpusPath + "cn/";
            String SeriOutput =corpusPath + "Serializer/";
            // extract dict，vector = word vector + sentence vectro.
            String EnEmbedPath = vectorTablePath + "en_vectorTable/en_vectors_"+dimension.ToString()+".txt";
            String CnEmbedPath = vectorTablePath + "cn_vectorTable/cn_vectors_"+dimension.ToString()+".txt";
            String wordPath = SeriOutput+type+"_wordList_"+dimension.ToString()+".txt";
            String enDictPath = corpusPath +"en/SentiWordList_en.txt";
            String cnDictPath = corpusPath +"cn/SentiWordList_cn.txt";


            int tempNum = 0;
            StreamWriter wordWriter = File.CreateText(wordPath);
            //train_en
            String trainEnSentIndexPath = IndexEmbedPath+sentiDim+type+"/"+type+"_train_index_EN.sent";
            String trainEnSentEmbedPath = IndexEmbedPath+sentiDim+type+"/"+type+"_train_embed_EN.sent";
            //test_en
            String testEnSentIndexPath = IndexEmbedPath+sentiDim+type+"/"+type+"_test_index_EN.sent";
            String testEnSentEmbedPath = IndexEmbedPath+sentiDim+type+"/"+type+"_test_embed_EN.sent";
            //train_cn
            String trainCnSentIndexPath = IndexEmbedPath+sentiDim+type+"/"+type+"_train_index_CN.sent";
            String trainCnSentEmbedPath = IndexEmbedPath+sentiDim+type+"/"+type+"_train_embed_CN.sent";
            //test_cn
            String testCnSentIndexPath = IndexEmbedPath+sentiDim+type+"/"+type+"_test_index_CN.sent";
            String testCnSentEmbedPath = IndexEmbedPath+sentiDim+type+"/"+type+"_test_embed_CN.sent";

            String srcTrainEnPath = corpusPath_en + "label_"+type+"_new.txt";
            String srcTestEnPath = corpusPath_en + "test_"+type+"_new.txt";
            //      String srcTrainCnPath = "E:/workspace/bi_lingual_preprocess/data/train_cn_0122/label_"+type+"_new.txt";
            //      String srcTestCnPath = "E:/workspace/bi_lingual_preprocess/data/test_cn_0122/test_"+type+"_new.txt";

            String srcTrainCnPath = corpusPath_cn+"label_"+type+"_new.txt";
            String srcTestCnPath = corpusPath_cn+"test_"+type+"_new.txt";

            String desTrainEnPath = SeriOutput +"semantic_sentiment_train_"+type+"_en_"+dimension.ToString()+".txt";
            String desTestEnPath = SeriOutput + "semantic_sentiment_test_"+type+"_en_"+dimension.ToString()+".txt";
            String desTrainCnPath = SeriOutput + "semantic_sentiment_train_"+type+"_cn_"+dimension.ToString()+".txt";
            String desTestCnPath = SeriOutput + "semantic_sentiment_test_"+type+"_cn_"+dimension.ToString()+".txt";

            String dictPath = SeriOutput + "semantic_sentiment_"+type+"_dict_"+dimension.ToString()+".txt";

            List<String> enSentiWord = loadSentiList(enDictPath);
            List<String> cnSentiWord = loadSentiList(cnDictPath);

            Dictionary<String, String> gloveEmbedding= loadEmbeddings(EnEmbedPath);
            Dictionary<String, String> word2VecEmbedding= loadEmbeddings(CnEmbedPath);
            //train_en
            Dictionary<String, String> trainEnSentEmbedding = mapSentEmbedding(trainEnSentIndexPath, trainEnSentEmbedPath);
            //test_en
            Dictionary<String, String> testEnSentEmbedding = mapSentEmbedding(testEnSentIndexPath, testEnSentEmbedPath);
            //train_cn
            Dictionary<String, String> trainCnSentEmbedding = mapSentEmbedding(trainCnSentIndexPath, trainCnSentEmbedPath);
            //test_cn
            Dictionary<String, String> testCnSentEmbedding = mapSentEmbedding(testCnSentIndexPath, testCnSentEmbedPath);

            List<String> dict = new List<String>();
            List<String> wordIndex = new List<String>();
            Dictionary<String,String> wordList = new Dictionary<String, String>();
            wordNum = wordIndex.Count;
            extract(srcTrainEnPath, desTrainEnPath, gloveEmbedding, trainEnSentEmbedding, dict, wordList, wordIndex, enSentiWord);
            System.Console.WriteLine(wordNum-tempNum);
            tempNum = wordNum; 
            extract(srcTestEnPath, desTestEnPath, gloveEmbedding, testEnSentEmbedding, dict, wordList, wordIndex,enSentiWord);
            System.Console.WriteLine(wordNum-tempNum);
            tempNum = wordNum; 
            extract(srcTrainCnPath, desTrainCnPath, word2VecEmbedding, trainCnSentEmbedding, dict, wordList, wordIndex, cnSentiWord);
            System.Console.WriteLine(wordNum-tempNum);
            tempNum = wordNum; 
            extract(srcTestCnPath, desTestCnPath, word2VecEmbedding, testCnSentEmbedding, dict, wordList, wordIndex, cnSentiWord);
            System.Console.WriteLine(wordNum-tempNum);
            tempNum = wordNum; 

            StreamWriter dictWriter = File.CreateText(dictPath);
            System.Console.WriteLine("Writing...");
            foreach(String embedding in dict){
                dictWriter.WriteLine(embedding);
            }

            foreach(String word in wordIndex){
                wordWriter.WriteLine(word);
            }

            wordWriter.Close();
            dictWriter.Close();
        }


        public static List<String> loadSentiList(String dictPath){
            //load sentiment word list.
            StreamReader reader = File.OpenText(dictPath);
            List<String> wordList = new List<String>();
            String line = "";
            while(!reader.EndOfStream){
                line = reader.ReadLine();
                line = line.Trim();
                wordList.Add(line);
            }

            reader.Close();
            return wordList;
        }
        public static Dictionary<String, String> mapSentEmbedding(String sentIndexPath, String sentEmbedPath){
            // 将句子情感标识和index匹配
            System.Console.WriteLine("Mapping...");
            Dictionary<String, String> sentMap = new Dictionary<String, String>();
            StreamReader indexReader = File.OpenText(sentIndexPath);
            StreamReader embedReader = File.OpenText(sentEmbedPath);
            String indexLine = "";
            String embedLine = "";
            while( !indexReader.EndOfStream && !embedReader.EndOfStream)
            {
                indexLine = indexReader.ReadLine();
                embedLine = embedReader.ReadLine();

                embedLine = embedLine.Replace("  ", " ");
                embedLine = embedLine.Trim();
                sentMap.Add(indexLine, embedLine);
            }
            indexReader.Close();
            embedReader.Close();
            return sentMap;
        }

        public static Dictionary<String, String> loadEmbeddings(String embedPath){
            // 加载GloVe词典
            System.Console.WriteLine("Loading...");
            Dictionary<String, String> srcMap = new Dictionary<String, String>();
            StreamReader reader = File.OpenText(embedPath);
            String line = "";
            while(!reader.EndOfStream)
            {
                line = reader.ReadLine();
                String word = line.Substring(0, line.IndexOf(" "));
                String embed = line.Substring(line.IndexOf(" ")+1);
                embed = embed.Trim();
                srcMap.Add(word, embed);
            }
            reader.Close();
            return srcMap;
        }
        public static void extract(String srcPath,
            String desPath, Dictionary<String, String> gloveEmbedding,
            Dictionary<String, String> sentEmbedding, List<String> dict, Dictionary<String, String> wordList, List<String> list, List<String> sentiList){

            int rowNum = 0;
            System.Console.WriteLine("Extracting...");
            StreamReader reader = File.OpenText(srcPath);
            StreamWriter writer = File.CreateText(desPath);
            String line = "";
            String outputLine = "";
            int docNum = 0;
            int sentNum = 0;
            Boolean first = true;
            while(!reader.EndOfStream)
            {
                line = reader.ReadLine();
                //          System.out.println(line);
                if(line.Length == 0){
                    continue;
                }
                if(line.Equals("<p>")||line.Equals("<n>")||line.Contains("< N >")||line.Contains("< P >")){
                    docNum++;
                    sentNum = 0;
                    if(first == true){
                        first = false;
                    }else{
                        outputLine = outputLine.Trim();
                        //                  System.out.println(outputLine);
                        writer.WriteLine(outputLine);
                        outputLine = "";
                    }
                    continue;
                }
                if(line.StartsWith("[") && line.EndsWith("]")){
                    line = line.Substring(2, line.Length-4).Trim();
                }
                line = line.Replace("  ", " ");
                line = line.Replace("   ", " ");
                //          if(srcPath.equals("E:/workspace/bi_lingual_preprocess/data/train_cn_0122/label_book_new.txt")){
                //              System.out.println(line);
                //          }
                String [] array = line.Split(' ');
                sentNum++;
                for(int i = 0; i < array.Length; i++){
                    outputLine = outputLine + (++wordNum).ToString() + " ";
                    if(wordList.ContainsKey(array[i])){
                        String isSentiWord = "0";
                        if(sentiList.Contains(array[i])){
                            isSentiWord = "1";
                        }
                        list.Add(wordList[array[i]] + " " +isSentiWord);
                    }else{
                        wordList.Add(array[i], "***"+(num++).ToString());
                        String isSentiWord = "0";
                        if(sentiList.Contains(array[i])){
                            isSentiWord = "1";
                        }
                        list.Add(wordList[array[i]] + " " +isSentiWord);
                    }

                    String embedding = "";
                    if(gloveEmbedding.ContainsKey(array[i])){
                        embedding = gloveEmbedding[array[i]];
                        //get the embedding from gloveEmbeddings
                        embedding = embedding + " " + sentEmbedding[docNum+" "+sentNum];
                    }else{
                        //not get the embedding from gloveEmbeddings
                        embedding = "";
                        Random a = new Random(DateTime.Now.Second);
                        for(int j = 0; j < dimension; j++){
                            double value = a.NextDouble()*2-1;
                            embedding = embedding + " "+ value.ToString();
                        }
                        embedding = embedding + " " + sentEmbedding[docNum+" "+sentNum];
                    }
                    embedding = embedding.Trim();
                    dict.Add(embedding);
                    rowNum++;
                    //String []testArray = embedding.Split(' ');
                    //              if(testArray.length != 150){
                    //                  System.out.println(rowNum);
                    //                  System.out.println(docNum+"_"+sentNum);
                    //              }
                }
            }
            writer.WriteLine(outputLine.Trim());

            writer.Close();
            reader.Close();
        }

    }
}


using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace wordExtraction
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            SerializerWSR.Serialize();
            //SerializerForBigLSTM.Serialize();
            //DictionaryBuilder.GenerateSentiWordList("/home/laboratory/corpus/en/SentiWordList_en.txt");
            //DictionaryBuilder.GenerateSentiWordList("/home/laboratory/corpus/cn/SentiWordList_cn.txt");
            List<String> filesPath = new List<string>(ConfigurationManager.AppSettings["CorpusPath"].Split(','));
            Exactor exactor = new Exactor(filesPath);
            exactor.Do();

        }
    }
}
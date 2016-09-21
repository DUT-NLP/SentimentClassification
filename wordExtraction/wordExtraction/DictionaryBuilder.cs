using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;

namespace wordExtraction
{
    public class DictionaryBuilder
    {       //Config
        private static String mDictPath = ConfigurationManager.AppSettings["DictionaryPath"];
        public static Dictionary<String, Record> WordDict{ get{ return mWordDict; } }
        private static  Dictionary<String, Record> mWordDict;
        private static  Boolean mNotInitialized = true;
        public static  void InitializeDictionary(String dictPath = null)
        {
            if (mNotInitialized) 
            {            
                //No need to Initialize dictionary more than once,
                //unless you want to change another dictionary, you may want to call the function "ReInitializeDictionary"
                mNotInitialized = false;
                Init(dictPath);
            }
        }
        public static void ReInitializeDictionary(String dictPath = null)
        {
            Init(dictPath);
        }
        private static void Init(String dictPath)
        {
            //Default, the dictionary path in configuration file will be used.
            if (!String.IsNullOrEmpty(dictPath))
            {
                mDictPath = dictPath;
            }
            mWordDict = new Dictionary<string, Record> ();
            StreamReader sr = File.OpenText (mDictPath);
            while (!sr.EndOfStream) 
            {
                String recordStr = sr.ReadLine ();
                Record record = new Record (recordStr);
                try
                {
                    if(record.priorpolarity != Priorpolarity.neutral && record.priorpolarity != Priorpolarity.both)
                    {
                        mWordDict.Add (record.word1, record);
                    }
                }
                catch (Exception)
                {
                    //System.Console.WriteLine (ex.ToString ());
                }
            }
            sr.Close ();
        }
        static public void WriteEnWordList(String path)
        {
            InitializeDictionary();
            StreamWriter sw = File.CreateText(path);
            foreach (String word in mWordDict.Keys)
            {
                sw.WriteLine(word);
            }
            sw.Close();
        }
        public static void DropNeutralRecord(String newDictPath)
        {
            InitializeDictionary();
            StreamWriter sw = File.CreateText(newDictPath);
            foreach (Record record in mWordDict.Values)
            {
                if (record.priorpolarity != Priorpolarity.neutral)
                {
                    sw.WriteLine(record.ToString());
                }
            }
            sw.Close();
        }
        public static void BuildDictionaryForZh(String dictPath)
        {
			InitializeDictionary();
            String ZhWordListPath = ConfigurationManager.AppSettings["WordListPathZh"];
            StreamReader sr_zh = File.OpenText(ZhWordListPath);
            StreamWriter sw_zh = File.CreateText(dictPath);
            foreach(String word in mWordDict.Keys)
            {
                String zhWord = sr_zh.ReadLine();

                Record zhRecord = new Record(mWordDict[word]);
                zhRecord.word1 = zhWord;

                sw_zh.WriteLine(zhRecord.ToString() + " " + mWordDict[word].word1);
            }

            sr_zh.Close();
            sw_zh.Close();
        }
        public static void Deduplication(String newPath, String oldPath)
        {
            ReInitializeDictionary(oldPath);
            StreamWriter sw = File.CreateText(newPath);
            foreach (KeyValuePair<String, Record> keyvalue in mWordDict)
            {
                sw.WriteLine(keyvalue.Value.ToString());
            }
            sw.Close();
        }

        public static void GenerateSentiWordList(String SentWordListpath)
        {
            StreamWriter sw = File.CreateText(SentWordListpath);
            Init(mDictPath);
            foreach (KeyValuePair<String, Record> keyvalue in mWordDict)
            {
                sw.WriteLine(keyvalue.Key.ToString());
            }
            sw.Close();
        }

    }
}


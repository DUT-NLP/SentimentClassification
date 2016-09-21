using System;
using System.Collections.Generic;
using System.Configuration;

namespace wordExtraction
{
	public enum LevelType
	{
        none = 0,
		weaksubj = 1,
		strongsubj = 2,
	}
	public enum Priorpolarity
	{
        negative = 0,
        positive = 1,
        neutral = 2,
        both = 3,
        weakneg = 4,
	}
    public enum PartofSpeech
    {
        none = 0,
        adj = 1,
        verb = 2,
        anypos = 3,
        noun = 4,
        adverb = 5,
    }
	//i.e type=weaksubj len=1 word1=abandoned pos1=adj stemmed1=n priorpolarity=negative
	public class Record
	{
        private static Boolean IsDictFormatNormal = Boolean.Parse(ConfigurationManager.AppSettings["IsDictFormatNormal"]);
        private static Dictionary<String, Int32> logicTable = new Dictionary<string, int>();
        public LevelType type;
        public Int32 len;
		public String word1 { get; set; }
        public PartofSpeech pos1;
        public Boolean stemmed1;
		public Priorpolarity priorpolarity { get; set; }

        private static Boolean IsNotInit = true;
        private static void InitLogicTable()
        {
            if (IsNotInit)
            {
                IsNotInit = false;
                logicTable = new Dictionary<string, int>();
                logicTable.Add(LevelType.strongsubj.ToString(), (Int32)LevelType.strongsubj);
                logicTable.Add(LevelType.weaksubj.ToString(), (Int32)(LevelType.weaksubj));

                logicTable.Add(Priorpolarity.negative.ToString(), (Int32)Priorpolarity.negative);
                logicTable.Add(Priorpolarity.positive.ToString(), (Int32)Priorpolarity.positive);
                logicTable.Add(Priorpolarity.neutral.ToString(), (Int32)Priorpolarity.neutral);
                logicTable.Add(Priorpolarity.both.ToString(), (Int32)Priorpolarity.both);
                logicTable.Add(Priorpolarity.weakneg.ToString(), (Int32)Priorpolarity.weakneg);

                logicTable.Add(PartofSpeech.adj.ToString(), (Int32)(PartofSpeech.adj));
                logicTable.Add(PartofSpeech.verb.ToString(), (Int32)(PartofSpeech.verb));
                logicTable.Add(PartofSpeech.anypos.ToString(), (Int32)(PartofSpeech.anypos));
                logicTable.Add(PartofSpeech.noun.ToString(), (Int32)(PartofSpeech.noun));
                logicTable.Add(PartofSpeech.adverb.ToString(), (Int32)(PartofSpeech.adverb));
            }
        }
        public Record(){}
        public Record(Record record)
        {
            type = record.type;
            len = record.len;
            word1 = record.word1;
            pos1 = record.pos1;
            stemmed1 = record.stemmed1;
            priorpolarity = record.priorpolarity;

        }
		public Record(String record)
        {
            InitLogicTable();
            String[] properties = record.Split(' ');
            if (IsDictFormatNormal)
            {
                foreach (String str in properties)
                {
                    String[] keyValue = str.Split('=');
                    if (keyValue[0].Equals("word1"))
                    {
                        word1 = keyValue[1];
                    }
                    else if (keyValue[0].Equals("len"))
                    {
                        len = Int32.Parse(keyValue[1]);
                    }
                    else if (keyValue[0].Equals("type"))
                    {
                        type = (LevelType)logicTable[keyValue[1]];
                    }
                    else if (keyValue[0].Equals("pos1"))
                    {
                        pos1 = (PartofSpeech)logicTable[keyValue[1]];
                    }
                    else if (keyValue[0].Equals("priorpolarity"))
                    {
                        priorpolarity = (Priorpolarity)logicTable[keyValue[1]];
                    }
                    else if (keyValue[0].Equals("stemmed1"))
                    {
                        if (keyValue[0].Equals("n", StringComparison.OrdinalIgnoreCase))
                        {
                            stemmed1 = false;
                        }
                        else
                        {
                            stemmed1 = true;
                        }
                    }
                }
            }
            else
            {
                word1 = properties[0];
                priorpolarity = (Priorpolarity)logicTable[properties[1]];
            }
		}

        public override string ToString()
        {
            return string.Format("type={0} len={1} word1={2} pos1={3} stemmed1={4} priorpolarity={5}", 
                type.ToString(),
                len.ToString(), 
                word1, 
                pos1.ToString(), 
                stemmed1 ? "y" : "n",
                priorpolarity);
        }
	}
}

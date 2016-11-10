using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace wordExtraction
{

	public class Exactor
	{
        private static List<String> mNegativeWords = new List<String>(ConfigurationManager.AppSettings["NegativeWords"].Split(','));
        private static Boolean addAbstraction = Boolean.Parse(ConfigurationManager.AppSettings["addAbstraction"]);
        private static Int32 window = Int32.Parse(ConfigurationManager.AppSettings["WindowSize"]);
        private static String BLANK = "BLANK";
		private List<String> mDocumentsPath;
		public Exactor (List<String> documentsPath)
		{
            DictionaryBuilder.InitializeDictionary ();
			mDocumentsPath = documentsPath;
		}
		public void Do()
		{
			foreach (String path in mDocumentsPath)
			{
				StreamWriter sw = null;
                StreamWriter swNumber = null;
                StreamWriter swLabel = null;
				try
				{
					sw = File.CreateText(path + ".extract");
                    swNumber = File.CreateText(path + ".number");
                    swLabel =  File.CreateText(path + ".label");
				}
				catch (Exception) 
				{
					//System.Console.WriteLine(ex.ToString());
					continue;
				}
				Document doc = new Document (path);
				doc.Open ();
				while (!doc.EndOfFile) 
				{
					Paragraph prag = doc.NextParagraph ();
					Int32 LineNumberInPrag = 0;
                    Boolean hasNoSentimentWord = true;
					foreach (String str in prag.Lines) 
					{
                        hasNoSentimentWord = true;
                        LineNumberInPrag++;
                        List<String> words = NomalizeLineToWords (str);
						for(int i = window; i < words.Count - window; ++i)
						{
							try
                            {
                                //If current word is not in the dictionary, then an exception would be thrown.
                                //And in exception handle function, we do nothing but continue to process next word.
                                Record record = DictionaryBuilder.WordDict[words[i]];
                                //for example, let the current word be the center, and give a window which length is 2
                                //then the fragment would contain 5 words
                                List<String> fragmentstr = words.GetRange(i - window, window * 2 + 1);
                                String fragment = ToFragment(fragmentstr);
                                Priorpolarity fragPriorpolarity = GetNewPriorpolarity(fragmentstr, record.priorpolarity);
                                sw.WriteLine((new FragmentSenitment(fragment, fragPriorpolarity).ToString()));
                                swNumber.WriteLine(String.Format("{0} {1}", doc.CurrentParagraphNumber, LineNumberInPrag));
                                swLabel.WriteLine(GetLabel(fragPriorpolarity));
                                hasNoSentimentWord = false;
							}
							catch (Exception) 
							{
								//System.Console.WriteLine (ex.ToString ());
                                continue;
							}
						}
                        if (LineNumberInPrag == 1 && hasNoSentimentWord && addAbstraction)
                        {
                            words.Remove("[");
                            words.Remove("]");
                            for (int i = words.Count; i < window * 2 + 1; i++)
                            {
                                words.Add("BLANK");
                            }                                
                            List<String> fragmentstr = words.GetRange(0, window * 2 + 1);
                            String fragment = ToFragment(fragmentstr);
                            sw.WriteLine((new FragmentSenitment(fragment, prag.Priorpolarity).ToString()));
                            swNumber.WriteLine(String.Format("{0} {1}", doc.CurrentParagraphNumber, LineNumberInPrag));
                            swLabel.WriteLine(GetLabel(prag.Priorpolarity));
                        }
					}
                    if(prag.Length != 0)
                        swNumber.WriteLine("end " + prag.Length);
                }
				doc.Close();
				sw.Close();
                swNumber.Close();
                swLabel.Close();
			}
            mDocumentsPath.Clear();
		}
        private static String GetLabel(Priorpolarity oldPriop)
        {
            if (oldPriop == Priorpolarity.positive)
            {
                return "1";
            }
            else if (oldPriop == Priorpolarity.negative)
            {
                return "0";
            }
            else if (oldPriop == Priorpolarity.weakneg)
            {
                return "0";
            }
            return "1";
        }
        private static Priorpolarity GetNewPriorpolarity(List<String> words, Priorpolarity oldPriop)
        {
            foreach(String word in words)
            {
                if(mNegativeWords.Contains(word))
                {
                    if (oldPriop == Priorpolarity.negative)
                    {
                        return Priorpolarity.positive;
                    }
                    else if (oldPriop == Priorpolarity.positive)
                    {
                        return Priorpolarity.negative;
                    }
                    else if (oldPriop == Priorpolarity.weakneg)
                    {
                        return Priorpolarity.positive;
                    }
                }
            }
            return oldPriop;
        }
		private static String ToFragment(List<String> words)
		{
            //we want a string like this: "this is an example"
			String frag = "";
			foreach (String str in words)
			{
				frag = frag + str + " ";
			}
			return frag.TrimEnd (' ');
		}
        private static List<String> NomalizeLineToWords(String str)
		{
            List<String> words = new List<String>(str.Split(' '));
            //when we exact words from one line, there would be a special case that the word is at head or tail of the whole line.
            //in this case, we add "BLANK" insread.
            //Here, we add two "BLANK" words on head and tail respectively, then the special case is not special any more.
			for (Int32 i = 0; i < window; ++i) 
			{
				words.Insert (0, BLANK);
				words.Insert (words.Count,  BLANK);
			}
            words.RemoveAll(node =>
                {
                return String.IsNullOrWhiteSpace(node);// || node.Equals("[") || node.Equals("]") ;
                });
			return words;
		}
	}
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace wordExtraction
{
	public class Paragraph
	{
        public Int32 Length{ get { return Lines.Count; } }
		public List<String> Lines;
		public Priorpolarity Priorpolarity;
		public Paragraph()
		{
			Lines = new List<String> ();
		}
	}
	public class Document: IDisposable
	{
		static private List<String> mParagraphTags = new List<string> ();
		static private Boolean mIsNotInitialized = true;
        static public void initializeParagraphTags()
		{
			if (mIsNotInitialized) 
			{
				mIsNotInitialized = false;
                mParagraphTags.Add (String.Format("<{0}>", ConfigurationManager.AppSettings["ParagraphTagn"].ToUpper()));
                mParagraphTags.Add (String.Format("<{0}>", ConfigurationManager.AppSettings["ParagraphTagn"].ToUpper()));
				mParagraphTags.Add (String.Format("<{0}>", ConfigurationManager.AppSettings["ParagraphTagp"]));
                mParagraphTags.Add (String.Format("<{0}>", ConfigurationManager.AppSettings["ParagraphTagn"]));
                mParagraphTags.Add (String.Format("< {0} >", ConfigurationManager.AppSettings["ParagraphTagp"].ToUpper()));
                mParagraphTags.Add (String.Format("< {0} >", ConfigurationManager.AppSettings["ParagraphTagn"].ToUpper()));
                mParagraphTags.Add (String.Format("< {0} >", ConfigurationManager.AppSettings["ParagraphTagp"]));
                mParagraphTags.Add (String.Format("< {0} >", ConfigurationManager.AppSettings["ParagraphTagn"]));
			}
		}

		private String mPath;
		private StreamReader mDocReader;
		public Document (String path)
		{
			initializeParagraphTags ();
			mPath = path;
		}
		public void Open()
		{
			mDocReader = File.OpenText (mPath);
		}
		public void Close()
		{
			mDocReader.Close ();
		}
		private Int32 mCurrentParagraphNumber = 0;
		public Int32 CurrentParagraphNumber{ get { return mCurrentParagraphNumber; } }
		private String mTmpTag = "";
		public Boolean EndOfFile
		{
			get
			{
				return mDocReader.EndOfStream;
			}
		}
		public Paragraph NextParagraph()
		{
			Paragraph paragraph = new Paragraph();
            paragraph.Priorpolarity = mTmpTag.Trim ().Equals (mParagraphTags [0]) ? Priorpolarity.negative : Priorpolarity.positive;

			while (!mDocReader.EndOfStream)
			{
				mTmpTag = mDocReader.ReadLine();
				if (mParagraphTags.Contains(mTmpTag.Trim()))
				{
					break;
				}
				if (!mTmpTag.Trim().Equals(String.Empty))
				{
					paragraph.Lines.Add(mTmpTag);
				}
			}
			if(paragraph.Lines.Count != 0)   mCurrentParagraphNumber++;
			return paragraph;
		}

		private Boolean mIsNotDisposed = true;
		public void Dispose()
		{
			if (mIsNotDisposed)
			{
				mIsNotDisposed = false;
				mDocReader.Dispose ();
			}
		}

	}
}


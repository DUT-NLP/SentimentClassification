using System;

namespace wordExtraction
{
    public class FragmentSenitment
    {
        public Int32 ParagraphNumber { get; set; }
        public Int32 LineNumber { get; set; }
        public String Fragment { get; set; }
        public Priorpolarity Priorpolarity{ get; set; }
        public FragmentSenitment()
        {

        }
        public FragmentSenitment(String frag, Priorpolarity pri, Int32 pn = 0, Int32 ln = 0 )
        {
            ParagraphNumber = pn;
            LineNumber = ln;
            Fragment = frag;
            Priorpolarity = pri;
        }       
        public override String ToString()
        {
            return String.Format("{0}", 
                                  Fragment.ToString());
        }
    }
}


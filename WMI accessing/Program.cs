#define  W32Share //DisplaySession //DelSession  // used to choose start point for varies code files
using System;
using System.Management;
using System.Windows.Forms;

namespace WMI_accessing
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Form1 form = new Form1();
            form.textBox1.Clear();

            W32ShareSecurity.W32Share(form);
            //NOTE using searcher appears a lot faster than building a class - may want ot reimplement finding usbs but searcher works best when query can do a where clause that I cannot work where where uses object works well on strings. Typically this means when looking for one thing. If looking for multiple of parts of names via substrings the get collection and foreach works best
#if DelSession
            App.SessDel(form);
#endif            
#if DisplaySession
            DisplaySession.DispSession(form);
#endif 
        }
  
    }
}

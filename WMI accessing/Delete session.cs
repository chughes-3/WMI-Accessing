#define  DelSession//DisplaySession//DelSession  // used to remove display sessions
//found in http://bytes.com/topic/c-sharp/answers/261977-drop-users-connected-my-pc
// by Willy Denoyette [MVP] Nov 2005
//>>"pnp" <pnp.@.softlab.ntua.gr> wrote in message
//>>news:ut9vH6TCFHA.1292@TK2MSFTNGP10.phx.gbl...
//>>[color=darkred]
//>>>Hi all,
//>>> from computer management|shared folders|sessions I can see all the
//>>>users in a network that are connected to my PC and how many open files
//>>>they have. Is there a way to monitor this through C# and drop the users
//>>>that I want?
//>>>
//>>>thanks in advance,
//>>>Peter[/color]
//>>
//>>[/color]
//>
//>[/color]



//Willy Denoyette [MVP]    Posts: n/a 
//#6: Nov 16 '05  

//re: Drop users connected to my PC?

//--------------------------------------------------------------------------------


//"pnp" <pnp.@.softlab.ntua.gr> wrote in message
//news:OJAO49bCFHA.2032@tk2msftngp13.phx.gbl...[color=blue]
//> Yes that is what I want to do. How can I accomplish that?
//>[/color]


//One possibility is to use System.Management classes and the ServerSession
//and ServerConnection WMI classes.
//Note that:
//- the client cannot be stopped to re-open another session, this can happen
//automatically, for instance office applications will re-initiate and re-open
//the files it has currently open.
//- all resources associated with this client session will be closed (files
//are close), this guarantess physical consistency, but not logical
//consistancy.

//Herewith a small sample:

using System;
using System.Management;
using System.Windows.Forms;
namespace WMI_accessing
{
    class App
    {
#if DelSession
        static Form1 form;
        static string sharename = "TaxWiseServer_P";
        static bool deleteConn;
        public static void SessDel(Form1 formPassed)
        {
            form = formPassed;
            // remove session for user 'SomeUser'
            //DropConn4Share();
            //DropThisSession(@"chris");
            //either searcher or getinstances does same thing as doesinstance.delete or invoke method. Get much better return infor with.invoke. must have valid path for share before can delete it.
            Win32_SharesGetInstancesDELPshare();
            //Win32_SharesSearcher();
            form.ShowDialog();
        }
        static void Win32_SharesGetInstancesDELPshare()
        {
            ManagementObjectCollection shares = new ManagementClass("Win32_Share").GetInstances();
            foreach (ManagementObject shr in shares)
            {
                if (shr.GetPropertyValue("Name").ToString() == sharename)
                {
                    try
                    {
                        shr.Delete();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    //ManagementBaseObject outparams = shr.InvokeMethod("Delete", null, null);
                    //form.textBox1.AppendText("outparams = " + outparams.Properties["ReturnValue"].Value.ToString() + Environment.NewLine);
                }
            }
        }

        private static void Win32_SharesSearcher()
        {
            SelectQuery query = new SelectQuery("select * from Win32_Share where Name=\"" + sharename + "\"");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    form.textBox1.AppendText(string.Format("Win32ShareName: {0} Description {1} Path {2} ", mo.Properties["Name"].Value, mo.Properties["Description"].Value, mo.Properties["Path"].Value) + Environment.NewLine);
                    ManagementBaseObject outparams = mo.InvokeMethod("Delete", null, null);
                    form.textBox1.AppendText("outparams = " + outparams.Properties["ReturnValue"].Value.ToString() + Environment.NewLine);
                }

            }
        }
        static void DropConn4Share()
        {
            //CANNOT delete a server connection (Win32_ServerConnection) one can only delete a Server Session. The associators only work one way on the .get related so have to go from session to connection to validate share name and then back to session for the deletion
            string shareName = "TaxWiseServer_P";
            SelectQuery query = new SelectQuery("Select ComputerName, UserName from win32_ServerSession");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject srvSess in searcher.Get())
                {
                    form.textBox1.AppendText(string.Format("computerName = {0} and UserName = {1}", srvSess.Properties["ComputerName"].Value, srvSess.Properties["UserName"].Value) + Environment.NewLine);
                    foreach (ManagementBaseObject servConn in srvSess.GetRelated("Win32_ServerConnection"))
                    {
                        if (servConn.GetPropertyValue("ShareName").ToString() == shareName)
                        {
                            form.textBox1.AppendText("found servconn = " + servConn.GetPropertyValue("ShareName").ToString());
                            srvSess.Delete();
                        }
                    }
                }
            }
            form.ShowDialog();
        }
        static void DropThisSession(string objectQry)
        {
            //SelectQuery query = new SelectQuery("select ComputerName, UserName, ResourcesOpened from win32_serversession where username ='" + objectQry + "'");
            deleteConn = false;
            SelectQuery query = new SelectQuery("select ComputerName, UserName, ResourcesOpened from win32_serversession");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    //Console.WriteLine("Session opened by: {0} from {1} with {2} resources opened", mo.Properties["UserName"].Value, mo.Properties["ComputerName"].Value, mo.Properties["ResourcesOpened"].Value);
                    form.textBox1.AppendText(string.Format( "Session opened by: {0} from {1} with {2} resources opened", mo.Properties["UserName"].Value, mo.Properties["ComputerName"].Value, mo.Properties["ResourcesOpened"].Value) + Environment.NewLine);
                    // Optionaly - Get associated serverconnection instance
                    foreach (ManagementBaseObject b in mo.GetRelated("Win32_ServerConnection"))
                    {
                        ShowServerConnectionProperties(b.ToString());

                    }
                    // Delete the session, this will close all opened resources (files etc..)for this session
                    if (deleteConn == true)
                    {
                        mo.Delete();
                    }
                }
            }
        }
        static void ShowServerConnectionProperties(string objectClass)
        {
            using (ManagementObject process = new ManagementObject(objectClass))
            {
                process.Get();
                PropertyDataCollection processProperties = process.Properties;
                //Console.WriteLine("ConnectionID: {0,6} \tShareName: {1}", processProperties["ConnectionID"].Value, processProperties["ShareName"].Value);
                form.textBox1.AppendText(string.Format("ConnectionID: {0,6} \tShareName: {1}", processProperties["ConnectionID"].Value, processProperties["ShareName"].Value)+ Environment.NewLine);
                if (processProperties["ShareName"].Value.ToString() == "TaxWiseServer_P")
                {
                    deleteConn = true;
                    return;
                }
            }
        }
#endif
    }

}






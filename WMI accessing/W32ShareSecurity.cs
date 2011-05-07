using System;
using System.Management;
using System.Windows.Forms;
using System.Security.AccessControl;

namespace WMI_accessing
{
    class W32ShareSecurity
    {
        static Form1 form;
        static string sharename = "TaxWiseServer_P";
        public static void W32Share(Form1 formPassed)
        {
            form = formPassed;
            Win32_SharesSearcher();
            form.ShowDialog();

        }
        private static void Win32_SharesSearcher()
        {
            SelectQuery query = new SelectQuery("select * from Win32_Share where Name=\"" + sharename + "\"");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    foreach (PropertyData prop in mo.Properties)
                    {
                        form.textBox1.AppendText(prop.Name + " = " + mo[prop.Name] + Environment.NewLine);                    }
                        //form.textBox1.AppendText(string.Format("Win32ShareName: {0} Description {1} Path {2} ", mo.Properties["Name"].Value, mo.Properties["Description"].Value, mo.Properties["Path"].Value) + Environment.NewLine);
                }
            }
            ManagementObject winShareP = new ManagementObject("root\\CIMV2", "Win32_Share.Name=\"" + sharename + "\"", null);
            ManagementBaseObject outParams = winShareP.InvokeMethod("GetAccessMask", null, null);
            form.textBox1.AppendText(String.Format("access Mask = {0:x}", outParams["ReturnValue"]) + Environment.NewLine);
            ManagementBaseObject inParams = winShareP.GetMethodParameters("SetShareInfo");
            form.textBox1.AppendText("SetShareInfor in parameters" + Environment.NewLine);
            foreach (PropertyData prop in inParams.Properties)
            {
                form.textBox1.AppendText(String.Format("PROP = {0}, TYPE = {1} ", prop.Name, prop.Type.ToString()) + Environment.NewLine);
            }
            Object access = inParams.GetPropertyValue("Access");
        }

    }
}

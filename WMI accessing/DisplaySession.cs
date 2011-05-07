using System;
using System.Management;
using System.Windows.Forms;

namespace WMI_accessing
{
    class DisplaySession
    {
        static Form1 form;

        static public void DispSession(Form1 formpassed)
        {
            form = formpassed;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ServerConnection");
                form.textBox1.AppendText("Win32_ServerConnection instance" + Environment.NewLine + Environment.NewLine);
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    foreach (PropertyData item in queryObj.Properties)
                    {
                        form.textBox1.AppendText(item.Name + " = " + queryObj[item.Name] + Environment.NewLine);
                    }
                    form.textBox1.AppendText(Environment.NewLine);
                }

                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ServerSession");
                form.textBox1.AppendText("Win32_ServerSession instance" + Environment.NewLine + Environment.NewLine);
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    foreach (PropertyData item in queryObj.Properties)
                    {
                        form.textBox1.AppendText(item.Name + " = " + queryObj[item.Name] + Environment.NewLine);
                    }
                    ManagementObjectCollection serConn = queryObj.GetRelated("Win32_ServerConnection"); //related works its way through dependent/antecedent relationship ALso see below 2 different ways of getting property value
                    foreach (ManagementObject item in serConn)
                    {
                        form.textBox1.AppendText(string.Format("ServerConn for this session: {0} from {1} with {2} ShareName", item.GetPropertyValue("UserName"), item.GetPropertyValue("ComputerName"), item.Properties["ShareName"].Value) + Environment.NewLine);
                    }
                    form.textBox1.AppendText(Environment.NewLine);
                }
                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ConnectionShare");
                ManagementObjectCollection connShares = searcher.Get();
                form.textBox1.AppendText("Win32_ConnectionShare instance Count = " + connShares.Count.ToString() + Environment.NewLine + Environment.NewLine);
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj.GetPropertyValue("Antecedent").ToString().Contains("TaxWiseServer_P"))
                    {
                        string str = queryObj.GetPropertyValue("Dependent").ToString();
                        form.textBox1.AppendText("Hooray TaxWiseServer_P from " + str.Substring(str.IndexOf("ComputerName"), str.IndexOf("ShareName") - str.IndexOf("ComputerName")) + Environment.NewLine);
                    }
                    foreach (PropertyData item in queryObj.Properties)
                    {
                        form.textBox1.AppendText(item.Name + " = " + queryObj[item.Name] + Environment.NewLine);
                    }
                    form.textBox1.AppendText(Environment.NewLine);
                }
            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
            form.ShowDialog();
        }
    }
}

using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Security.Principal;
using System.Text;
using System.Security.Cryptography;
HWIDGen.Main();
public class HWIDGen
{
    public static void Main()
    {
        if(!IsUserAdministrator())
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = System.AppDomain.CurrentDomain.FriendlyName;
            proc.Verb = "runas";

            Process.Start(proc);
            return;
        }
        Console.WriteLine("HWID: " + GenerateHWID());
            Console.ReadLine();

    }
    public static string GenerateHWID()
    {
        string param1, param2, param3, param4, param5, param6;
        param1 = WmicGetter("Win32_BIOS", "SerialNumber");
        param2 = WmicGetter("Win32_DiskDrive", "SerialNumber");
        param3 = WmicGetter("Win32_BaseBoard", "SerialNumber");
        param4 = WmicGetter("Win32_Processor", "ProcessorId");
        param5 = WmicGetter("Win32_ComputerSystem ", "SystemType");
        param6 = WmicGetter("Win32_OperatingSystem", "BuildNumber");
        string combinedParams = param1 + param2 + param3 + param4 + param5 + param6;

        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedParams));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
    public static bool IsUserAdministrator()
    {
        bool isAdmin;
        try
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException ex)
        {
            isAdmin = false;
        }
        catch (Exception ex)
        {
            isAdmin = false;
        }
        return isAdmin;
    }
    public static string WmicGetter(string wmiClass, string wmiProperty)
    {
        using (PowerShell powerShell = PowerShell.Create())
        {
            powerShell.AddCommand("Get-WmiObject")
                      .AddParameter("Class", wmiClass)
                      .AddParameter("Property", wmiProperty);

            foreach (PSObject result in powerShell.Invoke())
            {
                return result.Members[wmiProperty].Value.ToString();
            }
            return "";
        }
    }
}

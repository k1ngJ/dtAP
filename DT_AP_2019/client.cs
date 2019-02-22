using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace DT_AP_2019
{
    public class ROClient
    {
        public int clientVersion { get; set; }
        public int currentHpBaseAddress { get; set; }
        public int mouseFixAddress { get; set; }

        public int statusBufferAddress { get; set; }

        private process_memory_rw PMR { get; set; }
        private int _num = 0;

        public ROClient(Process roProcess)
        {
            PMR = new process_memory_rw();
            try
            {
                PMR.ReadProcess = roProcess;
                PMR.OpenProcess();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error on openprocess! Additional details: {0}", ex.Message));
                Environment.Exit(0);
            }
        }

        public uint ReadMemory(int address)
        {
            return BitConverter.ToUInt32(PMR.ReadProcessMemory((IntPtr)address, 4u, out _num), 0);
        }

        public byte[] ReadMemory(int address, uint bytesToRead)
        {
            return PMR.ReadProcessMemory((IntPtr)address, bytesToRead, out _num);
        }

        public void WriteMemory(int address, uint intToWrite)
        {
            PMR.WriteProcessMemory((IntPtr)address, BitConverter.GetBytes(intToWrite), out _num);
        }

        public void WriteMemory(int address, byte[] bytesToWrite)
        {
            PMR.WriteProcessMemory((IntPtr)address, bytesToWrite, out _num);
        }
    }
}

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

        private const int STATUS_BUFFER_RELATIVE = 1148;

        public ROClient(Process roProcess)
        {
            PMR = new process_memory_rw();
            try
            {
                PMR.ReadProcess = roProcess;
                this.currentHpBaseAddress = 0x00E4CAF4;
                this.mouseFixAddress = 0x00C77578;
                this.statusBufferAddress = this.currentHpBaseAddress + 1148;

                PMR.OpenProcess();

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error on openprocess! Additional details: {0}", ex.Message));
                Environment.Exit(0);
            }
        }

        public bool IsHpBelow(int percent) {
          return ReadCurrentHp() * 100 < percent * ReadMaxHp();
        }

        public bool IsSpBelow(int percent) {
          return ReadCurrentSp() * 100 < percent * ReadMaxSp();
        }

        public string HpLabel() {
          return string.Format("{0} / {1}", ReadCurrentHp(), ReadMaxHp());
        }

        public string SpLabel() {
          return string.Format("{0} / {1}", ReadCurrentSp(), ReadMaxSp());
        }

        private uint ReadCurrentHp() {
          return ReadMemory(this.currentHpBaseAddress);
        }

        private uint ReadCurrentSp() {
          return ReadMemory(this.currentHpBaseAddress + 8);
        }

        private uint ReadMaxHp() {
          return ReadMemory(this.currentHpBaseAddress + 4);
        }

        private uint ReadMaxSp() {
          return ReadMemory(this.currentHpBaseAddress + 12);
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Input;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace DT_AP_2019
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // PINVOKES
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, Keys wParam, int lParam);

        #region vars

        private uint maxHp { get; set; }
        private uint curHp { get; set; }

        private uint maxSp { get; set; }
        private uint curSp { get; set; }

        private int hpPercent { get; set; }
        private int spPercent { get; set; }

        Dictionary<string, Keys> dictionary = new Dictionary<string, Keys>();
        List<Key> all_keys = new List<Key>(); // mouse click ahk
        List<Key> all_keysB = new List<Key>(); // no mouse click ahk

        private ROClient roClient { get; set; }
        private Process roProc { get; set; }

        private bool dictLoaded { get; set; }
        #endregion

        #region UI Functions
        private void resizeWindow(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            this.CenterToScreen();
        }

        public void loadDict()
        {
            dictLoaded = false;
            dictionary.Add("F1", Keys.F1);
            dictionary.Add("F2", Keys.F2);
            dictionary.Add("F3", Keys.F3);
            dictionary.Add("F4", Keys.F4);
            dictionary.Add("F5", Keys.F5);
            dictionary.Add("F6", Keys.F6);
            dictionary.Add("F7", Keys.F7);
            dictionary.Add("F8", Keys.F8);
            dictionary.Add("F9", Keys.F9);
            dictionary.Add("1", Keys.D1);
            dictionary.Add("2", Keys.D2);
            dictionary.Add("3", Keys.D3);
            dictionary.Add("4", Keys.D4);
            dictionary.Add("5", Keys.D5);
            dictionary.Add("6", Keys.D6);
            dictionary.Add("7", Keys.D7);
            dictionary.Add("8", Keys.D8);
            dictionary.Add("9", Keys.D9);
            dictionary.Add("0", Keys.D0);
            dictionary.Add("A", Keys.A);
            dictionary.Add("B", Keys.B);
            dictionary.Add("C", Keys.C);
            dictionary.Add("D", Keys.D);
            dictionary.Add("E", Keys.E);
            dictionary.Add("F", Keys.F);
            dictionary.Add("G", Keys.G);
            dictionary.Add("H", Keys.H);
            dictionary.Add("I", Keys.I);
            dictionary.Add("J", Keys.J);
            dictionary.Add("K", Keys.K);
            dictionary.Add("L", Keys.L);
            dictionary.Add("M", Keys.M);
            dictionary.Add("N", Keys.N);
            dictionary.Add("O", Keys.O);
            dictionary.Add("P", Keys.P);
            dictionary.Add("Q", Keys.Q);
            dictionary.Add("R", Keys.R);
            dictionary.Add("S", Keys.S);
            dictionary.Add("T", Keys.T);
            dictionary.Add("U", Keys.U);
            dictionary.Add("V", Keys.V);
            dictionary.Add("W", Keys.W);
            dictionary.Add("X", Keys.X);
            dictionary.Add("Y", Keys.Y);
            dictionary.Add("Z", Keys.Z);

            // ap

            cmb_hp.DataSource = new BindingSource(dictionary, null);
            cmb_hp.ValueMember = "Value";
            cmb_hp.DisplayMember = "Key";
            cmb_hp.SelectedIndex = 7;
            cmb_sp.DataSource = new BindingSource(dictionary, null);
            cmb_sp.ValueMember = "Value";
            cmb_sp.DisplayMember = "Key";
            cmb_sp.SelectedIndex = 8;


            // ahk

            cmb_ahk.DataSource = new BindingSource(dictionary, null);
            cmb_ahk.ValueMember = "Value";
            cmb_ahk.DisplayMember = "Key";
            cmb_ahk.SelectedIndex = 0;

            // buff

            cmb_aspd.DataSource = new BindingSource(dictionary, null);
            cmb_aspd.ValueMember = "Value";
            cmb_aspd.DisplayMember = "Key";
            cmb_aspd.SelectedIndex = 41;

            cmb_gloom.DataSource = new BindingSource(dictionary, null);
            cmb_gloom.ValueMember = "Value";
            cmb_gloom.DisplayMember = "Key";
            cmb_gloom.SelectedIndex = 35;


            // scrolls

            cmb_bless.DataSource = new BindingSource(dictionary, null);
            cmb_bless.ValueMember = "Value";
            cmb_bless.DisplayMember = "Key";
            cmb_bless.SelectedIndex = 41;

            cmb_agi.DataSource = new BindingSource(dictionary, null);
            cmb_agi.ValueMember = "Value";
            cmb_agi.DisplayMember = "Key";
            cmb_agi.SelectedIndex = 35;
            dictLoaded = true;
        }

        public void list_process()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                cb_process.Items.Clear();
            });
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle != "")
                {
                    cb_process.Items.Add(string.Format("{0}.exe-{1}", p.ProcessName, p.Id));
                }
            }
        }

        #endregion

        #region settings

        // vars

        private int autopotDelay { get; set; }
        private int autobuffReadDelay { get; set; }
        private int autobuffDelay { get; set; }
        private int ahkDelay { get; set; }

        // autobuff
        private bool foundGloom { get; set; }
        private bool foundAspd { get; set; }

        // ahk
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        public void read_settings()
        {
            KeyConverter k = new KeyConverter();
            if (File.Exists("ap_settings.txt"))
            {
                string[] settings = File.ReadAllLines("ap_settings.txt");
                string ahkwhole = "";
                foreach (string st in settings)
                {
                    if (st.Contains("hp_button"))
                    {
                        cmb_hp.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("sp_button"))
                    {
                        cmb_sp.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("hp_percent"))
                    {
                        textBox1.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("sp_percent"))
                    {
                        textBox2.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("status_button"))
                    {
                        // cmb_status.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("hold_button"))
                    {
                        // cmb_hold.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("gloom_button"))
                    {
                        cmb_gloom.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("aspd_button"))
                    {
                        cmb_aspd.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    /*else if (st.Contains("rcx_color"))
                    {
                        tb_rcx.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }*/

                    // foods
                    else if (st.Contains("agiscroll_button"))
                    {
                        cmb_agi.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("blesscroll_button"))
                    {
                        cmb_bless.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("ap_delay"))
                    {
                        tb_apDelay.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("autobuff_delay"))
                    {
                        tb_abuffDelay.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("autobuff_read_delay"))
                    {
                        tb_abuffReadDelay.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("spam_delay"))
                    {
                        tb_spamDelay.Text = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                    }
                    else if (st.Contains("ahk_list"))
                    {
                        all_keys.Clear();
                        all_keysB.Clear();
                        ahkwhole = st.Split(new string[] { " = " }, StringSplitOptions.None)[1];
                        foreach (string a in ahkwhole.Split(','))
                        {
                            if (a.Length != 0)
                            {
                                if (a.Contains("mc"))
                                {
                                    Key _ahk = (Key)k.ConvertFromString(a.Split(':')[0]);
                                    all_keys.Add(_ahk);
                                }
                                else
                                {
                                    Key _ahk = (Key)k.ConvertFromString(a.Split(':')[0]);
                                    all_keysB.Add(_ahk);
                                }
                                lb_ahk.Items.Add(a);
                            }
                        }
                    }
                }
                readSettingsFromUI();
            }
            else
            {
                string allahk = "";
                all_keys.Clear();
                all_keysB.Clear();
                foreach (string ahkl in lb_ahk.Items)
                {
                    allahk += string.Format("{0},", ahkl);
                    if (ahkl.Contains("mc"))
                    {
                        Key _ahk = (Key)k.ConvertFromString(ahkl.Split(':')[0]);
                        all_keys.Add(_ahk);
                    }
                    else
                    {
                        Key _ahk = (Key)k.ConvertFromString(ahkl.Split(':')[0]);
                        all_keysB.Add(_ahk);
                    }

                }


                string[] settings = {
                                            string.Format("hp_button = {0}", cmb_hp.Text),
                                            string.Format("sp_button = {0}", cmb_sp.Text),
                                            string.Format("ahk_list = {0}", allahk),
                                            string.Format("hp_percent = {0}", textBox1.Text),
                                            string.Format("sp_percent = {0}", textBox2.Text),
                                            string.Format("gloom_button = {0}", cmb_gloom.Text),
                                            string.Format("aspd_button = {0}", cmb_aspd.Text),
                                            string.Format("agiscroll_button = {0}", cmb_agi.Text),
                                            string.Format("blesscroll_button = {0}", cmb_agi.Text),
                                            string.Format("ap_delay = {0}", tb_apDelay.Text),
                                            string.Format("autobuff_delay = {0}", tb_abuffDelay.Text),
                                            string.Format("autobuff_read_delay = {0}", tb_abuffReadDelay.Text),
                                            string.Format("spam_delay = {0}", tb_spamDelay.Text),
                                            string.Format("", "")
                                        };

                File.WriteAllLines("ap_settings.txt", settings);
                readSettingsFromUI();
            }
        }

        public void update_settings()
        {
            if (!dictLoaded)
                return;
            KeyConverter k = new KeyConverter();
            string allahk = "";
            all_keys.Clear();
            all_keysB.Clear();
            foreach (string ahkl in lb_ahk.Items)
            {
                allahk += string.Format("{0},",ahkl);
                Key _ahk = (Key)k.ConvertFromString(ahkl.Split(':')[0]);
                all_keys.Add(_ahk);
            }

            string[] settings = {
                                  string.Format("hp_button = {0}", cmb_hp.Text),
                                  string.Format("sp_button = {0}", cmb_sp.Text),
                                  string.Format("ahk_list = {0}", allahk),
                                  string.Format("hp_percent = {0}", textBox1.Text),
                                  string.Format("sp_percent = {0}", textBox2.Text),
                                  string.Format("gloom_button = {0}", cmb_gloom.Text),
                                  string.Format("aspd_button = {0}", cmb_aspd.Text),
                                  string.Format("agiscroll_button = {0}", cmb_agi.Text),
                                  string.Format("blesscroll_button = {0}", cmb_bless.Text),
                                  string.Format("ap_delay = {0}", tb_apDelay.Text),
                                  string.Format("autobuff_delay = {0}", tb_abuffDelay.Text),
                                  string.Format("autobuff_read_delay = {0}", tb_abuffReadDelay.Text),
                                  string.Format("spam_delay = {0}", tb_spamDelay.Text),
                                  string.Format("", "")
                                };

            File.WriteAllLines("ap_settings.txt", settings);
            readSettingsFromUI();
        }

        public void readSettingsFromUI()
        {
            try
            {
                autopotDelay      = int.Parse(tb_apDelay.Text);
                autobuffDelay     = int.Parse(tb_abuffDelay.Text);
                autobuffReadDelay = int.Parse(tb_abuffReadDelay.Text);
                ahkDelay          = int.Parse(tb_spamDelay.Text);
                hpPercent         = int.Parse(textBox1.Text);
                spPercent         = int.Parse(textBox2.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings error! Please delete ap_settings.txt to build new settings file.");
            }
        }
        #endregion

        #region Threads

        // process
        private void waitForProcess()
        {
            while (true)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    if (cb_process.Text != "")
                    {
                        // found proc
                        foreach (Process pc1 in Process.GetProcessesByName(cb_process.Text.Split(new string[] { ".exe-" }, StringSplitOptions.None)[0]))
                        {
                            if (pc1.Id == int.Parse(cb_process.Text.Split(new string[] { ".exe-" }, StringSplitOptions.None)[1]))
                            {
                                // pId matched
                                roProc = pc1;
                                roClient = new ROClient(roProc);
                                startAutoPotThread();
                                startBuffThread();
                                startAhkThread();
                                statusBufferSize = 100;

                                break;
                            }
                        }

                    }
                });

                if (roProc != null)
                    break;
                Thread.Sleep(1000);

            }
        }

        private void startAutoPotThread() {
            Thread apThread = new Thread(() => autopotThread());
            apThread.SetApartmentState(ApartmentState.STA);
            apThread.Start();
        }

        private void startBuffThread() {
            Thread buffThread = new Thread(() => autoBuffThread());
            buffThread.SetApartmentState(ApartmentState.STA);
            buffThread.Start();
        }

        private void startAhkThread() {
            Thread ahkThread = new Thread(() => spammerThread());
            ahkThread.SetApartmentState(ApartmentState.STA);
            ahkThread.Start();
        }


        // autopot
        private void autopotThread()
        {
            uint hp_pot_count = 0;
            while (true)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    // check hp first
                    if (roClient.IsHpBelow(hpPercent))
                    {
                        potHp();
                        hp_pot_count++;

                        if (hp_pot_count == 3) {
                          hp_pot_count = 0;

                          if (roClient.IsSpBelow(spPercent)) {
                            potSp();
                          }
                        }
                    }

                    // check sp
                    if (roClient.IsSpBelow(spPercent))
                    {
                      potSp();
                    }

                    // update UI
                    lbl_hp.Text = roClient.HpLabel();
                    lbl_sp.Text = roClient.SpLabel();

                });

                Thread.Sleep(autopotDelay);
            }
        }

        private void potSp() {
          PostMessage(roProc.MainWindowHandle, 0x100, (Keys)cmb_sp.SelectedValue, 0); // keydown
          PostMessage(roProc.MainWindowHandle, 0x101, (Keys)cmb_sp.SelectedValue, 0); // keyup
        }

        private void potHp() {
          PostMessage(roProc.MainWindowHandle, 0x100, (Keys)cmb_hp.SelectedValue, 0); // keydown
          PostMessage(roProc.MainWindowHandle, 0x101, (Keys)cmb_hp.SelectedValue, 0); // keyup
        }

        // autobuff
        private void autoBuffThread()
        {
            uint currentBuffValue = 0;
            while (true)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    foundGloom = false;
                    foundAspd  = false;
                    for (int i = 0; i <= statusBufferSize - 1; i++)
                    {
                        currentBuffValue = roClient.ReadMemory(roClient.statusBufferAddress + i * 4);

                        if (currentBuffValue == 3)
                            foundGloom = true;
                        if (currentBuffValue == 39 || currentBuffValue == 38 || currentBuffValue == 37)
                            foundAspd = true;
                        if (foundAspd && foundGloom )
                            break;
                        if (currentBuffValue == 0xFFFFFFFF)
                            break;
                    }

                    if (!foundGloom && cb_gloom.Checked)
                    {
                        useBoxOfGloom();
                        Thread.Sleep(autobuffDelay);
                    }

                    if (!foundAspd && cb_aspd.Checked)
                    {
                        useAspdPotion();
                        Thread.Sleep(autobuffDelay);
                    }
                });

                Thread.Sleep(autobuffReadDelay);
            }
        }

        private void useBoxOfGloom() {
          PostMessage(roProc.MainWindowHandle, 0x100, (Keys)cmb_gloom.SelectedValue, 0);
        }

        private void useAspdPotion() {
          PostMessage(roProc.MainWindowHandle, 0x100, (Keys)cmb_aspd.SelectedValue, 0);
        }

        // spammer
        private void spammerThread()
        {
            while (true)
            {
                try
                {
                    foreach (Key ak in all_keys)
                    {
                        // with mouse click
                        if (Keyboard.IsKeyDown(ak))
                        {
                            while (Keyboard.IsKeyDown(ak))
                            {
                                roClient.WriteMemory(roClient.mouseFixAddress, 0xFFFFFFFF);
                                Keys thisk = (Keys)Enum.Parse(typeof(Keys), ak.ToString());
                                PostMessage(roProc.MainWindowHandle, 0x100, thisk, 0);
                                PostMessage(roProc.MainWindowHandle, WM_LBUTTONDOWN, 0, 0);
                                PostMessage(roProc.MainWindowHandle, WM_LBUTTONUP, 0, 0);
                                Thread.Sleep(ahkDelay);
                            }
                            roClient.WriteMemory(roClient.mouseFixAddress, 500);
                        }
                    }
                    foreach (Key ak in all_keysB)
                    {
                        // without mouse click
                        if (Keyboard.IsKeyDown(ak))
                        {
                            while (Keyboard.IsKeyDown(ak))
                            {
                                Keys thisk = (Keys)Enum.Parse(typeof(Keys), ak.ToString());
                                PostMessage(roProc.MainWindowHandle, 0x100, thisk, 0);
                                Thread.Sleep(ahkDelay);
                            }
                        }

                    }
                }
                catch
                {

                }
                Thread.Sleep(ahkDelay);
            }
        }

        #endregion

        private int statusBufferSize { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            list_process();
            loadDict();
            read_settings();
            resizeWindow(202, 242);

            Thread waitForProcThread = new Thread(() => waitForProcess());
            waitForProcThread.SetApartmentState(ApartmentState.STA);
            waitForProcThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1) // autopot
                resizeWindow(202, 242);
            else if (tabControl1.SelectedTab == tabPage2) // spammer
                resizeWindow(373, 367);
            else if (tabControl1.SelectedTab == tabPage3)
                resizeWindow(249, 287);
            else
                resizeWindow(249, 287);
        }

        private void cb_process_Click(object sender, EventArgs e)
        {
            list_process();
        }

        private void btn_ahk_add_Click(object sender, EventArgs e)
        {
            string ahk_key = cmb_ahk.SelectedValue.ToString();
            int found_ahk = lb_ahk.FindString(ahk_key);
            if (found_ahk == -1)
            {
                if (cb_mouseclick.Checked)
                    lb_ahk.Items.Add(string.Format("{0}:mc", ahk_key));
                else
                    lb_ahk.Items.Add(string.Format("{0}", ahk_key));
                lb_ahk.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Hotkey has already been added. Please delete it first or choose another key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            update_settings();
        }

        private void cmb_hp_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void cmb_sp_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void cmb_gloom_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void cmb_aspd_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void btn_ahk_remove_Click(object sender, EventArgs e)
        {
            try
            {
                string lbhk = lb_ahk.SelectedItem.ToString();
                lb_ahk.Items.Remove(lb_ahk.Items[lb_ahk.SelectedIndex]);
                KeyConverter k = new KeyConverter();
                Key _ahk = (Key)k.ConvertFromString(lbhk);
                foreach (var _k in all_keys)
                {
                    if (_k == _ahk)
                    {
                        all_keys.Remove(_k);
                        break;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            update_settings();
        }

        private void tb_apDelay_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void tb_abuffDelay_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void tb_abuffReadDelay_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void tb_spamDelay_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            update_settings();
        }
    }
}

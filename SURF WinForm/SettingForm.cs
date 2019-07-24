using System;
using System.Windows.Forms;

namespace WindowsFormsApplication4
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public bool getDecriptors()
        {
            if (radioButton1.Checked == true)
            { return true; }
            else
            { return false; }
        }

        public int getThreshold()
        {
            return (int)numericUpDown1.Value;
        }

        public int getOctaves()
        {
            return (int)numericUpDown2.Value;
        }

        public int getOctavesLayer()
        {
            return (int)numericUpDown3.Value;
        }

        public double getMinDinst()
        {
            return (double)numericUpDown4.Value;
        }
    }
}

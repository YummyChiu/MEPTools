using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEPTools.FireHyrantLink
{
    public partial class FireHydrantLinkForm : Form
    {
        public bool IsBottom
        {
            get
            {
                return radioButton2.Checked;
            }
        }

        public double Offset
        {
            get
            {
                return double.Parse(textBox1.Text);
            }
        }

        public FireHydrantLinkForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

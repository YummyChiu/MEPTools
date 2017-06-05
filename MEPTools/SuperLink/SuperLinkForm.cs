using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEPTools.SuperLink
{
    public partial class SuperLinkForm : Form
    {
        private bool partial;

        public double Offset
        {
            get { return double.Parse(textBox1.Text); }
        }

        public double Angle
        {
            get
            {
                foreach (RadioButton rb in groupBox1.Controls)
                {
                    if (rb.Checked)
                    {
                        return double.Parse(rb.Text.TrimEnd('°'));
                    }
                }
                return 90.0;
            }
        }

        public bool Partial
        {
            get
            {
                return partial;
            }
        }

        public SuperLinkForm()
        {
            InitializeComponent();
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
            partial = false;
            Close();
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            partial = true;
            Close();
        }
    }
}

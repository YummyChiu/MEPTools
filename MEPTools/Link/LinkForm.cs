using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEPTools.Link
{
    public partial class LinkForm : Form
    {
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

        public LinkForm()
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

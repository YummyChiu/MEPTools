using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEPTools.Bend
{
    public partial class BendForm : Form
    {
        public double Offset
        {
            get { return double.Parse(textBoxHeightOffset.Text); }
        }

        public BendCommand.Direction Direction
        {
            get
            {
                foreach (RadioButton rb in groupBoxDirection.Controls)
                {
                    string checkedDirection = "";
                    if (rb.Checked)
                    {
                        checkedDirection = rb.Text;

                        if (checkedDirection == "向上")
                           return  BendCommand.Direction.Up;
                        else if (checkedDirection == "向下")
                            return  BendCommand.Direction.Down;
                        else if (checkedDirection == "向左")
                            return  BendCommand.Direction.Left;
                        else if (checkedDirection == "向右")
                            return  BendCommand.Direction.Right;
                    }

                }
                return BendCommand.Direction.Up;
            }
        }

        public double Angle
        {
            get
            {
                foreach (RadioButton rb in groupBoxAngle.Controls)
                {
                    if (rb.Checked)
                    {
                        return double.Parse(rb.Text.TrimEnd('°'));
                    }
                }
                return 90.0;
            }
        }
        bool isOneSideBend = false;
        public bool IsOneSideBend
        {
            get
            {
                return isOneSideBend;
            }
            set
            {
                isOneSideBend = value;
            }
        }
        public BendForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isOneSideBend = false;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isOneSideBend = true;
            Close();
        }
    }
}

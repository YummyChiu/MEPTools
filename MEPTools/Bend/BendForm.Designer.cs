namespace MEPTools.Bend
{
    partial class BendForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bendTwoSideBtn = new System.Windows.Forms.Button();
            this.bendOneSizeBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxDirection = new System.Windows.Forms.GroupBox();
            this.radioButtonRight = new System.Windows.Forms.RadioButton();
            this.radioButtonLeft = new System.Windows.Forms.RadioButton();
            this.radioButtonDown = new System.Windows.Forms.RadioButton();
            this.radioButtonUp = new System.Windows.Forms.RadioButton();
            this.groupBoxAngle = new System.Windows.Forms.GroupBox();
            this.radioButton90 = new System.Windows.Forms.RadioButton();
            this.radioButton60 = new System.Windows.Forms.RadioButton();
            this.radioButton45 = new System.Windows.Forms.RadioButton();
            this.radioButton30 = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxHeightOffset = new System.Windows.Forms.TextBox();
            this.groupBoxDirection.SuspendLayout();
            this.groupBoxAngle.SuspendLayout();
            this.SuspendLayout();
            // 
            // bendTwoSideBtn
            // 
            this.bendTwoSideBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bendTwoSideBtn.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bendTwoSideBtn.Location = new System.Drawing.Point(13, 280);
            this.bendTwoSideBtn.Name = "bendTwoSideBtn";
            this.bendTwoSideBtn.Size = new System.Drawing.Size(134, 40);
            this.bendTwoSideBtn.TabIndex = 0;
            this.bendTwoSideBtn.Text = "两边翻弯";
            this.bendTwoSideBtn.UseVisualStyleBackColor = true;
            this.bendTwoSideBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // bendOneSizeBtn
            // 
            this.bendOneSizeBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bendOneSizeBtn.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bendOneSizeBtn.Location = new System.Drawing.Point(179, 280);
            this.bendOneSizeBtn.Name = "bendOneSizeBtn";
            this.bendOneSizeBtn.Size = new System.Drawing.Size(134, 40);
            this.bendOneSizeBtn.TabIndex = 1;
            this.bendOneSizeBtn.Text = "一边翻弯";
            this.bendOneSizeBtn.UseVisualStyleBackColor = true;
            this.bendOneSizeBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "起翻高度：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(245, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "mm";
            // 
            // groupBoxDirection
            // 
            this.groupBoxDirection.Controls.Add(this.radioButtonRight);
            this.groupBoxDirection.Controls.Add(this.radioButtonLeft);
            this.groupBoxDirection.Controls.Add(this.radioButtonDown);
            this.groupBoxDirection.Controls.Add(this.radioButtonUp);
            this.groupBoxDirection.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxDirection.Location = new System.Drawing.Point(13, 98);
            this.groupBoxDirection.Name = "groupBoxDirection";
            this.groupBoxDirection.Size = new System.Drawing.Size(300, 76);
            this.groupBoxDirection.TabIndex = 5;
            this.groupBoxDirection.TabStop = false;
            this.groupBoxDirection.Text = "起翻方向";
            // 
            // radioButtonRight
            // 
            this.radioButtonRight.AutoSize = true;
            this.radioButtonRight.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButtonRight.Location = new System.Drawing.Point(236, 34);
            this.radioButtonRight.Name = "radioButtonRight";
            this.radioButtonRight.Size = new System.Drawing.Size(50, 21);
            this.radioButtonRight.TabIndex = 3;
            this.radioButtonRight.Text = "向右";
            this.radioButtonRight.UseVisualStyleBackColor = true;
            // 
            // radioButtonLeft
            // 
            this.radioButtonLeft.AutoSize = true;
            this.radioButtonLeft.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButtonLeft.Location = new System.Drawing.Point(166, 34);
            this.radioButtonLeft.Name = "radioButtonLeft";
            this.radioButtonLeft.Size = new System.Drawing.Size(50, 21);
            this.radioButtonLeft.TabIndex = 2;
            this.radioButtonLeft.Text = "向左";
            this.radioButtonLeft.UseVisualStyleBackColor = true;
            // 
            // radioButtonDown
            // 
            this.radioButtonDown.AutoSize = true;
            this.radioButtonDown.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButtonDown.Location = new System.Drawing.Point(82, 34);
            this.radioButtonDown.Name = "radioButtonDown";
            this.radioButtonDown.Size = new System.Drawing.Size(50, 21);
            this.radioButtonDown.TabIndex = 1;
            this.radioButtonDown.Text = "向下";
            this.radioButtonDown.UseVisualStyleBackColor = true;
            // 
            // radioButtonUp
            // 
            this.radioButtonUp.AutoSize = true;
            this.radioButtonUp.Checked = true;
            this.radioButtonUp.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButtonUp.Location = new System.Drawing.Point(7, 34);
            this.radioButtonUp.Name = "radioButtonUp";
            this.radioButtonUp.Size = new System.Drawing.Size(50, 21);
            this.radioButtonUp.TabIndex = 0;
            this.radioButtonUp.TabStop = true;
            this.radioButtonUp.Text = "向上";
            this.radioButtonUp.UseVisualStyleBackColor = true;
            // 
            // groupBoxAngle
            // 
            this.groupBoxAngle.Controls.Add(this.radioButton90);
            this.groupBoxAngle.Controls.Add(this.radioButton60);
            this.groupBoxAngle.Controls.Add(this.radioButton45);
            this.groupBoxAngle.Controls.Add(this.radioButton30);
            this.groupBoxAngle.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxAngle.Location = new System.Drawing.Point(13, 190);
            this.groupBoxAngle.Name = "groupBoxAngle";
            this.groupBoxAngle.Size = new System.Drawing.Size(300, 76);
            this.groupBoxAngle.TabIndex = 6;
            this.groupBoxAngle.TabStop = false;
            this.groupBoxAngle.Text = "起翻角度";
            // 
            // radioButton90
            // 
            this.radioButton90.AutoSize = true;
            this.radioButton90.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton90.Location = new System.Drawing.Point(237, 34);
            this.radioButton90.Name = "radioButton90";
            this.radioButton90.Size = new System.Drawing.Size(45, 21);
            this.radioButton90.TabIndex = 3;
            this.radioButton90.Text = "90°";
            this.radioButton90.UseVisualStyleBackColor = true;
            // 
            // radioButton60
            // 
            this.radioButton60.AutoSize = true;
            this.radioButton60.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton60.Location = new System.Drawing.Point(167, 34);
            this.radioButton60.Name = "radioButton60";
            this.radioButton60.Size = new System.Drawing.Size(45, 21);
            this.radioButton60.TabIndex = 2;
            this.radioButton60.Text = "60°";
            this.radioButton60.UseVisualStyleBackColor = true;
            // 
            // radioButton45
            // 
            this.radioButton45.AutoSize = true;
            this.radioButton45.Checked = true;
            this.radioButton45.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton45.Location = new System.Drawing.Point(83, 34);
            this.radioButton45.Name = "radioButton45";
            this.radioButton45.Size = new System.Drawing.Size(45, 21);
            this.radioButton45.TabIndex = 1;
            this.radioButton45.TabStop = true;
            this.radioButton45.Text = "45°";
            this.radioButton45.UseVisualStyleBackColor = true;
            // 
            // radioButton30
            // 
            this.radioButton30.AutoSize = true;
            this.radioButton30.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton30.Location = new System.Drawing.Point(8, 34);
            this.radioButton30.Name = "radioButton30";
            this.radioButton30.Size = new System.Drawing.Size(45, 21);
            this.radioButton30.TabIndex = 0;
            this.radioButton30.Text = "30°";
            this.radioButton30.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(13, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(144, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "提示：请  点击  翻弯点！";
            // 
            // textBoxHeightOffset
            // 
            this.textBoxHeightOffset.Location = new System.Drawing.Point(87, 51);
            this.textBoxHeightOffset.Name = "textBoxHeightOffset";
            this.textBoxHeightOffset.Size = new System.Drawing.Size(152, 23);
            this.textBoxHeightOffset.TabIndex = 8;
            this.textBoxHeightOffset.Text = "200";
            // 
            // BendForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 338);
            this.Controls.Add(this.textBoxHeightOffset);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBoxAngle);
            this.Controls.Add(this.groupBoxDirection);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bendOneSizeBtn);
            this.Controls.Add(this.bendTwoSideBtn);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(347, 376);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(347, 376);
            this.Name = "BendForm";
            this.ShowIcon = false;
            this.Text = "一键翻弯";
            this.groupBoxDirection.ResumeLayout(false);
            this.groupBoxDirection.PerformLayout();
            this.groupBoxAngle.ResumeLayout(false);
            this.groupBoxAngle.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bendTwoSideBtn;
        private System.Windows.Forms.Button bendOneSizeBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxDirection;
        private System.Windows.Forms.RadioButton radioButtonRight;
        private System.Windows.Forms.RadioButton radioButtonLeft;
        private System.Windows.Forms.RadioButton radioButtonDown;
        private System.Windows.Forms.RadioButton radioButtonUp;
        private System.Windows.Forms.GroupBox groupBoxAngle;
        private System.Windows.Forms.RadioButton radioButton90;
        private System.Windows.Forms.RadioButton radioButton60;
        private System.Windows.Forms.RadioButton radioButton45;
        private System.Windows.Forms.RadioButton radioButton30;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxHeightOffset;
    }
}
namespace 磁磚辨識評分
{
    partial class IdentificationForm
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnGotoUserDef = new System.Windows.Forms.Button();
            this.btnFindBest = new System.Windows.Forms.Button();
            this.lblBoxFind = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.trackBarThresholding = new System.Windows.Forms.TrackBar();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.loadImageButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.originalImageBox = new Emgu.CV.UI.ImageBox();
            this.BinImageBox = new Emgu.CV.UI.ImageBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.CatchImageBox = new Emgu.CV.UI.ImageBox();
            this.OrgPlusCatchBox = new Emgu.CV.UI.ImageBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThresholding)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.originalImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BinImageBox)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CatchImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OrgPlusCatchBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnGotoUserDef);
            this.panel1.Controls.Add(this.btnFindBest);
            this.panel1.Controls.Add(this.lblBoxFind);
            this.panel1.Controls.Add(this.btnRun);
            this.panel1.Controls.Add(this.lblThreshold);
            this.panel1.Controls.Add(this.trackBarThresholding);
            this.panel1.Controls.Add(this.fileNameTextBox);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.loadImageButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1038, 49);
            this.panel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(665, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(849, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "儲存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnGotoUserDef
            // 
            this.btnGotoUserDef.Location = new System.Drawing.Point(930, 12);
            this.btnGotoUserDef.Name = "btnGotoUserDef";
            this.btnGotoUserDef.Size = new System.Drawing.Size(92, 23);
            this.btnGotoUserDef.TabIndex = 9;
            this.btnGotoUserDef.Text = "使用辨識結果";
            this.btnGotoUserDef.UseVisualStyleBackColor = true;
            this.btnGotoUserDef.Click += new System.EventHandler(this.btnGotoUserDef_Click);
            this.btnGotoUserDef.MouseMove += new System.Windows.Forms.MouseEventHandler(this.btnGotoUserDef_MouseMove);
            // 
            // btnFindBest
            // 
            this.btnFindBest.Location = new System.Drawing.Point(746, 13);
            this.btnFindBest.Name = "btnFindBest";
            this.btnFindBest.Size = new System.Drawing.Size(75, 23);
            this.btnFindBest.TabIndex = 7;
            this.btnFindBest.Text = "btnFindBest";
            this.btnFindBest.UseVisualStyleBackColor = true;
            this.btnFindBest.Click += new System.EventHandler(this.btnFindBest_Click);
            // 
            // lblBoxFind
            // 
            this.lblBoxFind.AutoSize = true;
            this.lblBoxFind.Location = new System.Drawing.Point(610, 15);
            this.lblBoxFind.Name = "lblBoxFind";
            this.lblBoxFind.Size = new System.Drawing.Size(41, 12);
            this.lblBoxFind.TabIndex = 6;
            this.lblBoxFind.Text = "找到：";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(509, 13);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 5;
            this.btnRun.Text = "RUN";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblThreshold
            // 
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(447, 15);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(23, 12);
            this.lblThreshold.TabIndex = 4;
            this.lblThreshold.Text = "128";
            // 
            // trackBarThresholding
            // 
            this.trackBarThresholding.Location = new System.Drawing.Point(334, 6);
            this.trackBarThresholding.Maximum = 255;
            this.trackBarThresholding.Name = "trackBarThresholding";
            this.trackBarThresholding.Size = new System.Drawing.Size(98, 45);
            this.trackBarThresholding.TabIndex = 3;
            this.trackBarThresholding.Value = 128;
            this.trackBarThresholding.Scroll += new System.EventHandler(this.trackBarThreshold_Scroll);
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Location = new System.Drawing.Point(49, 13);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.ReadOnly = true;
            this.fileNameTextBox.Size = new System.Drawing.Size(215, 22);
            this.fileNameTextBox.TabIndex = 2;
            this.fileNameTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "File:";
            // 
            // loadImageButton
            // 
            this.loadImageButton.Location = new System.Drawing.Point(270, 11);
            this.loadImageButton.Name = "loadImageButton";
            this.loadImageButton.Size = new System.Drawing.Size(30, 21);
            this.loadImageButton.TabIndex = 0;
            this.loadImageButton.Text = "...";
            this.loadImageButton.UseVisualStyleBackColor = true;
            this.loadImageButton.Click += new System.EventHandler(this.loadImageButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 49);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1038, 702);
            this.splitContainer1.SplitterDistance = 530;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.originalImageBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.BinImageBox);
            this.splitContainer2.Size = new System.Drawing.Size(530, 702);
            this.splitContainer2.SplitterDistance = 359;
            this.splitContainer2.TabIndex = 0;
            // 
            // originalImageBox
            // 
            this.originalImageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.originalImageBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.originalImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.originalImageBox.Location = new System.Drawing.Point(0, 0);
            this.originalImageBox.Name = "originalImageBox";
            this.originalImageBox.Size = new System.Drawing.Size(530, 359);
            this.originalImageBox.TabIndex = 3;
            this.originalImageBox.TabStop = false;
            this.originalImageBox.OnZoomScaleChange += new System.EventHandler(this.originalImageBox_OnZoomScaleChange);
            // 
            // BinImageBox
            // 
            this.BinImageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.BinImageBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.BinImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BinImageBox.Location = new System.Drawing.Point(0, 0);
            this.BinImageBox.Name = "BinImageBox";
            this.BinImageBox.Size = new System.Drawing.Size(530, 339);
            this.BinImageBox.TabIndex = 4;
            this.BinImageBox.TabStop = false;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.CatchImageBox);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.OrgPlusCatchBox);
            this.splitContainer3.Size = new System.Drawing.Size(504, 702);
            this.splitContainer3.SplitterDistance = 361;
            this.splitContainer3.TabIndex = 0;
            // 
            // CatchImageBox
            // 
            this.CatchImageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CatchImageBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.CatchImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CatchImageBox.Location = new System.Drawing.Point(0, 0);
            this.CatchImageBox.Name = "CatchImageBox";
            this.CatchImageBox.Size = new System.Drawing.Size(504, 361);
            this.CatchImageBox.TabIndex = 4;
            this.CatchImageBox.TabStop = false;
            // 
            // OrgPlusCatchBox
            // 
            this.OrgPlusCatchBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.OrgPlusCatchBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.OrgPlusCatchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OrgPlusCatchBox.Location = new System.Drawing.Point(0, 0);
            this.OrgPlusCatchBox.Name = "OrgPlusCatchBox";
            this.OrgPlusCatchBox.Size = new System.Drawing.Size(504, 337);
            this.OrgPlusCatchBox.TabIndex = 4;
            this.OrgPlusCatchBox.TabStop = false;
            // 
            // IdentificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 751);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "IdentificationForm";
            this.Text = "Shape Detection";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThresholding)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.originalImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BinImageBox)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CatchImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OrgPlusCatchBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button loadImageButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lblThreshold;
        private System.Windows.Forms.TrackBar trackBarThresholding;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblBoxFind;
        private System.Windows.Forms.Button btnFindBest;
        private System.Windows.Forms.Button btnGotoUserDef;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Emgu.CV.UI.ImageBox originalImageBox;
        private Emgu.CV.UI.ImageBox BinImageBox;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private Emgu.CV.UI.ImageBox CatchImageBox;
        private Emgu.CV.UI.ImageBox OrgPlusCatchBox;
        private System.Windows.Forms.Button button1;

    }
}


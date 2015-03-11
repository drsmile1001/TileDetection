namespace 磁磚辨識評分
{
    partial class FormUserDef
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
            this.picboxWatchArea = new System.Windows.Forms.PictureBox();
            this.picboxThumbnail = new System.Windows.Forms.PictureBox();
            this.lbxTileList = new System.Windows.Forms.ListBox();
            this.btnDelTile = new System.Windows.Forms.Button();
            this.btnCreatTile = new System.Windows.Forms.Button();
            this.lblTileCount = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnLoad = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnGiveGradeSquare = new System.Windows.Forms.Button();
            this.btnDimSquareWorkingspace = new System.Windows.Forms.Button();
            this.lblMsg = new System.Windows.Forms.Label();
            this.btnDimRectangleWorkingspace = new System.Windows.Forms.Button();
            this.btnGiveGradeRectangle = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ckbRankTopOnly = new System.Windows.Forms.CheckBox();
            this.lblFileEdition = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudRectangularTileAdj = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAutoTrimmingAllTile = new System.Windows.Forms.Button();
            this.ckbDrawBold = new System.Windows.Forms.CheckBox();
            this.btnIdentification = new System.Windows.Forms.Button();
            this.btnRectangleTileRegularize = new System.Windows.Forms.Button();
            this.nudPixelPerCentimeter = new System.Windows.Forms.NumericUpDown();
            this.ckbShowWeb = new System.Windows.Forms.CheckBox();
            this.btnSquareTileRegularize = new System.Windows.Forms.Button();
            this.lblEditMsg = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picboxWatchArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picboxThumbnail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRectangularTileAdj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPixelPerCentimeter)).BeginInit();
            this.SuspendLayout();
            // 
            // picboxWatchArea
            // 
            this.picboxWatchArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picboxWatchArea.Location = new System.Drawing.Point(0, 0);
            this.picboxWatchArea.Name = "picboxWatchArea";
            this.picboxWatchArea.Size = new System.Drawing.Size(776, 641);
            this.picboxWatchArea.TabIndex = 4;
            this.picboxWatchArea.TabStop = false;
            this.picboxWatchArea.Click += new System.EventHandler(this.picboxWatchArea_Click);
            this.picboxWatchArea.DoubleClick += new System.EventHandler(this.picboxWatchArea_DoubleClick);
            this.picboxWatchArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picboxWatchArea_MouseDown);
            this.picboxWatchArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picboxWatchArea_MouseMove);
            this.picboxWatchArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picboxWatchArea_MouseUp);
            // 
            // picboxThumbnail
            // 
            this.picboxThumbnail.BackColor = System.Drawing.Color.Black;
            this.picboxThumbnail.Location = new System.Drawing.Point(3, 3);
            this.picboxThumbnail.Name = "picboxThumbnail";
            this.picboxThumbnail.Size = new System.Drawing.Size(160, 120);
            this.picboxThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picboxThumbnail.TabIndex = 5;
            this.picboxThumbnail.TabStop = false;
            this.picboxThumbnail.Click += new System.EventHandler(this.picboxBorse_Click);
            this.picboxThumbnail.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picboxBorse_MouseDown);
            this.picboxThumbnail.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picboxBorse_MouseMove);
            this.picboxThumbnail.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picboxBorse_MouseUp);
            // 
            // lbxTileList
            // 
            this.lbxTileList.FormattingEnabled = true;
            this.lbxTileList.ItemHeight = 12;
            this.lbxTileList.Location = new System.Drawing.Point(3, 129);
            this.lbxTileList.Name = "lbxTileList";
            this.lbxTileList.Size = new System.Drawing.Size(154, 160);
            this.lbxTileList.TabIndex = 6;
            this.lbxTileList.SelectedIndexChanged += new System.EventHandler(this.lbxTileList_SelectedIndexChanged);
            // 
            // btnDelTile
            // 
            this.btnDelTile.Location = new System.Drawing.Point(3, 295);
            this.btnDelTile.Name = "btnDelTile";
            this.btnDelTile.Size = new System.Drawing.Size(75, 23);
            this.btnDelTile.TabIndex = 7;
            this.btnDelTile.Text = "刪除";
            this.btnDelTile.UseVisualStyleBackColor = true;
            this.btnDelTile.Click += new System.EventHandler(this.btnDelTile_Click);
            // 
            // btnCreatTile
            // 
            this.btnCreatTile.Location = new System.Drawing.Point(3, 324);
            this.btnCreatTile.Name = "btnCreatTile";
            this.btnCreatTile.Size = new System.Drawing.Size(75, 23);
            this.btnCreatTile.TabIndex = 8;
            this.btnCreatTile.Text = "新增";
            this.btnCreatTile.UseVisualStyleBackColor = true;
            this.btnCreatTile.Click += new System.EventHandler(this.btnCreatTile_Click);
            // 
            // lblTileCount
            // 
            this.lblTileCount.AutoSize = true;
            this.lblTileCount.Location = new System.Drawing.Point(84, 300);
            this.lblTileCount.Name = "lblTileCount";
            this.lblTileCount.Size = new System.Drawing.Size(41, 12);
            this.lblTileCount.TabIndex = 9;
            this.lblTileCount.Text = "總數：";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(3, 607);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "存檔";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "data|*.data";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(101, 607);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "讀檔";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "data|*.data";
            // 
            // btnGiveGradeSquare
            // 
            this.btnGiveGradeSquare.Location = new System.Drawing.Point(129, 549);
            this.btnGiveGradeSquare.Name = "btnGiveGradeSquare";
            this.btnGiveGradeSquare.Size = new System.Drawing.Size(43, 23);
            this.btnGiveGradeSquare.TabIndex = 12;
            this.btnGiveGradeSquare.Text = "評分";
            this.btnGiveGradeSquare.UseVisualStyleBackColor = true;
            this.btnGiveGradeSquare.Click += new System.EventHandler(this.btnGiveGradeSquare_Click);
            // 
            // btnDimSquareWorkingspace
            // 
            this.btnDimSquareWorkingspace.Location = new System.Drawing.Point(3, 549);
            this.btnDimSquareWorkingspace.Name = "btnDimSquareWorkingspace";
            this.btnDimSquareWorkingspace.Size = new System.Drawing.Size(120, 23);
            this.btnDimSquareWorkingspace.TabIndex = 13;
            this.btnDimSquareWorkingspace.Text = "定義正方形工作區";
            this.btnDimSquareWorkingspace.UseVisualStyleBackColor = true;
            this.btnDimSquareWorkingspace.Click += new System.EventHandler(this.btnDimSquareWorkingspace_Click);
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Location = new System.Drawing.Point(84, 324);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(23, 12);
            this.lblMsg.TabIndex = 14;
            this.lblMsg.Text = "123";
            // 
            // btnDimRectangleWorkingspace
            // 
            this.btnDimRectangleWorkingspace.Location = new System.Drawing.Point(3, 578);
            this.btnDimRectangleWorkingspace.Name = "btnDimRectangleWorkingspace";
            this.btnDimRectangleWorkingspace.Size = new System.Drawing.Size(120, 23);
            this.btnDimRectangleWorkingspace.TabIndex = 13;
            this.btnDimRectangleWorkingspace.Text = "定義長方形工作區";
            this.btnDimRectangleWorkingspace.UseVisualStyleBackColor = true;
            this.btnDimRectangleWorkingspace.Click += new System.EventHandler(this.btnDimRectangleWorkingspace_Click);
            // 
            // btnGiveGradeRectangle
            // 
            this.btnGiveGradeRectangle.Location = new System.Drawing.Point(129, 578);
            this.btnGiveGradeRectangle.Name = "btnGiveGradeRectangle";
            this.btnGiveGradeRectangle.Size = new System.Drawing.Size(43, 23);
            this.btnGiveGradeRectangle.TabIndex = 12;
            this.btnGiveGradeRectangle.Text = "評分";
            this.btnGiveGradeRectangle.UseVisualStyleBackColor = true;
            this.btnGiveGradeRectangle.Click += new System.EventHandler(this.btnGiveGradeRectangle_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.picboxWatchArea);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ckbRankTopOnly);
            this.splitContainer1.Panel2.Controls.Add(this.lblFileEdition);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.nudRectangularTileAdj);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.btnAutoTrimmingAllTile);
            this.splitContainer1.Panel2.Controls.Add(this.ckbDrawBold);
            this.splitContainer1.Panel2.Controls.Add(this.btnIdentification);
            this.splitContainer1.Panel2.Controls.Add(this.btnRectangleTileRegularize);
            this.splitContainer1.Panel2.Controls.Add(this.nudPixelPerCentimeter);
            this.splitContainer1.Panel2.Controls.Add(this.ckbShowWeb);
            this.splitContainer1.Panel2.Controls.Add(this.lblMsg);
            this.splitContainer1.Panel2.Controls.Add(this.btnSquareTileRegularize);
            this.splitContainer1.Panel2.Controls.Add(this.lblEditMsg);
            this.splitContainer1.Panel2.Controls.Add(this.picboxThumbnail);
            this.splitContainer1.Panel2.Controls.Add(this.lbxTileList);
            this.splitContainer1.Panel2.Controls.Add(this.btnDimRectangleWorkingspace);
            this.splitContainer1.Panel2.Controls.Add(this.btnDelTile);
            this.splitContainer1.Panel2.Controls.Add(this.btnDimSquareWorkingspace);
            this.splitContainer1.Panel2.Controls.Add(this.btnCreatTile);
            this.splitContainer1.Panel2.Controls.Add(this.btnGiveGradeRectangle);
            this.splitContainer1.Panel2.Controls.Add(this.lblTileCount);
            this.splitContainer1.Panel2.Controls.Add(this.btnGiveGradeSquare);
            this.splitContainer1.Panel2.Controls.Add(this.btnSave);
            this.splitContainer1.Panel2.Controls.Add(this.btnLoad);
            this.splitContainer1.Size = new System.Drawing.Size(971, 643);
            this.splitContainer1.SplitterDistance = 778;
            this.splitContainer1.TabIndex = 15;
            // 
            // ckbRankTopSideOnly
            // 
            this.ckbRankTopOnly.AutoSize = true;
            this.ckbRankTopOnly.Checked = true;
            this.ckbRankTopOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbRankTopOnly.Location = new System.Drawing.Point(97, 524);
            this.ckbRankTopOnly.Name = "ckbRankTopSideOnly";
            this.ckbRankTopOnly.Size = new System.Drawing.Size(84, 16);
            this.ckbRankTopOnly.TabIndex = 26;
            this.ckbRankTopOnly.Text = "只評分上半";
            this.ckbRankTopOnly.UseVisualStyleBackColor = true;
            this.ckbRankTopOnly.CheckedChanged += new System.EventHandler(this.ckbRankTopOnly_CheckedChanged);
            // 
            // lblFileEdition
            // 
            this.lblFileEdition.AutoSize = true;
            this.lblFileEdition.Location = new System.Drawing.Point(74, 629);
            this.lblFileEdition.Name = "lblFileEdition";
            this.lblFileEdition.Size = new System.Drawing.Size(0, 12);
            this.lblFileEdition.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 494);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 24;
            this.label2.Text = "長方磚微調";
            // 
            // nudRectangularTileAdj
            // 
            this.nudRectangularTileAdj.DecimalPlaces = 1;
            this.nudRectangularTileAdj.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudRectangularTileAdj.Location = new System.Drawing.Point(85, 492);
            this.nudRectangularTileAdj.Name = "nudRectangularTileAdj";
            this.nudRectangularTileAdj.Size = new System.Drawing.Size(91, 22);
            this.nudRectangularTileAdj.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 423);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "比例參數：";
            // 
            // btnAutoTrimmingAllTile
            // 
            this.btnAutoTrimmingAllTile.Location = new System.Drawing.Point(3, 520);
            this.btnAutoTrimmingAllTile.Name = "btnAutoTrimmingAllTile";
            this.btnAutoTrimmingAllTile.Size = new System.Drawing.Size(93, 23);
            this.btnAutoTrimmingAllTile.TabIndex = 21;
            this.btnAutoTrimmingAllTile.Text = "全部自動微調";
            this.btnAutoTrimmingAllTile.UseVisualStyleBackColor = true;
            this.btnAutoTrimmingAllTile.Click += new System.EventHandler(this.btnAutoTurnAllTile_Click);
            // 
            // ckbDrawBold
            // 
            this.ckbDrawBold.AutoSize = true;
            this.ckbDrawBold.Location = new System.Drawing.Point(116, 384);
            this.ckbDrawBold.Name = "ckbDrawBold";
            this.ckbDrawBold.Size = new System.Drawing.Size(60, 16);
            this.ckbDrawBold.TabIndex = 20;
            this.ckbDrawBold.Text = "畫粗線";
            this.ckbDrawBold.UseVisualStyleBackColor = true;
            // 
            // btnIdentification
            // 
            this.btnIdentification.Location = new System.Drawing.Point(3, 377);
            this.btnIdentification.Name = "btnIdentification";
            this.btnIdentification.Size = new System.Drawing.Size(75, 23);
            this.btnIdentification.TabIndex = 19;
            this.btnIdentification.Text = "辨識新圖";
            this.btnIdentification.UseVisualStyleBackColor = true;
            this.btnIdentification.Click += new System.EventHandler(this.btnIdentification_Click);
            // 
            // btnRectangleTileRegularize
            // 
            this.btnRectangleTileRegularize.Location = new System.Drawing.Point(96, 466);
            this.btnRectangleTileRegularize.Name = "btnRectangleTileRegularize";
            this.btnRectangleTileRegularize.Size = new System.Drawing.Size(85, 23);
            this.btnRectangleTileRegularize.TabIndex = 18;
            this.btnRectangleTileRegularize.Text = "正規化長磁磚";
            this.btnRectangleTileRegularize.UseVisualStyleBackColor = true;
            this.btnRectangleTileRegularize.Click += new System.EventHandler(this.btnRectangleTileRegularize_Click);
            // 
            // nudPixelPerCentimeter
            // 
            this.nudPixelPerCentimeter.DecimalPlaces = 10;
            this.nudPixelPerCentimeter.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudPixelPerCentimeter.Location = new System.Drawing.Point(5, 438);
            this.nudPixelPerCentimeter.Name = "nudPixelPerCentimeter";
            this.nudPixelPerCentimeter.Size = new System.Drawing.Size(91, 22);
            this.nudPixelPerCentimeter.TabIndex = 17;
            // 
            // ckbShowWeb
            // 
            this.ckbShowWeb.AutoSize = true;
            this.ckbShowWeb.Location = new System.Drawing.Point(104, 408);
            this.ckbShowWeb.Name = "ckbShowWeb";
            this.ckbShowWeb.Size = new System.Drawing.Size(72, 16);
            this.ckbShowWeb.TabIndex = 16;
            this.ckbShowWeb.Text = "顯示網格";
            this.ckbShowWeb.UseVisualStyleBackColor = true;
            this.ckbShowWeb.CheckedChanged += new System.EventHandler(this.ckbShowWeb_CheckedChanged);
            // 
            // btnSquareTileRegularize
            // 
            this.btnSquareTileRegularize.Location = new System.Drawing.Point(5, 466);
            this.btnSquareTileRegularize.Name = "btnSquareTileRegularize";
            this.btnSquareTileRegularize.Size = new System.Drawing.Size(85, 23);
            this.btnSquareTileRegularize.TabIndex = 15;
            this.btnSquareTileRegularize.Text = "正規化方磁磚";
            this.btnSquareTileRegularize.UseVisualStyleBackColor = true;
            this.btnSquareTileRegularize.Click += new System.EventHandler(this.btnSquareTileRegularize_Click);
            // 
            // lblEditMsg
            // 
            this.lblEditMsg.AutoSize = true;
            this.lblEditMsg.Location = new System.Drawing.Point(84, 466);
            this.lblEditMsg.Name = "lblEditMsg";
            this.lblEditMsg.Size = new System.Drawing.Size(0, 12);
            this.lblEditMsg.TabIndex = 14;
            // 
            // FormUserDef
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(971, 643);
            this.Controls.Add(this.splitContainer1);
            this.KeyPreview = true;
            this.Name = "FormUserDef";
            this.Text = "FormUserDef";
            this.Load += new System.EventHandler(this.FormUserDef_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormUserDef_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormUserDef_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormUserDef_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picboxWatchArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picboxThumbnail)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudRectangularTileAdj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPixelPerCentimeter)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picboxWatchArea;
        private System.Windows.Forms.PictureBox picboxThumbnail;
        private System.Windows.Forms.ListBox lbxTileList;
        private System.Windows.Forms.Button btnDelTile;
        private System.Windows.Forms.Button btnCreatTile;
        private System.Windows.Forms.Label lblTileCount;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnGiveGradeSquare;
        private System.Windows.Forms.Button btnDimSquareWorkingspace;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.Button btnDimRectangleWorkingspace;
        private System.Windows.Forms.Button btnGiveGradeRectangle;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblEditMsg;
        private System.Windows.Forms.Button btnSquareTileRegularize;
        private System.Windows.Forms.CheckBox ckbShowWeb;
        private System.Windows.Forms.NumericUpDown nudPixelPerCentimeter;
        private System.Windows.Forms.Button btnRectangleTileRegularize;
        private System.Windows.Forms.Button btnIdentification;
        private System.Windows.Forms.CheckBox ckbDrawBold;
        private System.Windows.Forms.Button btnAutoTrimmingAllTile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudRectangularTileAdj;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFileEdition;
        private System.Windows.Forms.CheckBox ckbRankTopOnly;

    }
}
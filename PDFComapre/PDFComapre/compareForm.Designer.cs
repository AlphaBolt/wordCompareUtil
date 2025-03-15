using System;
namespace PDFCompare
{
    partial class compareForm
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
            System.Drawing.Drawing2D.GraphicsPath graphicsPath2 = new System.Drawing.Drawing2D.GraphicsPath();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(compareForm));
            this.materialTabControl1 = new MaterialWinforms.Controls.MaterialTabControl();
            this.selectFilesTabPage = new MaterialWinforms.Controls.MaterialTabPage();
            this.selectFilesContentPanel = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.reviewButton = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.text_OR_imageBtn = new MaterialWinforms.Controls.MaterialToggle();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.btnbrwTarget = new System.Windows.Forms.Button();
            this.btnbrwSource = new System.Windows.Forms.Button();
            this.reviewTabPage = new MaterialWinforms.Controls.MaterialTabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelResultPathDisplay = new System.Windows.Forms.Label();
            this.btnSelectResultPath = new System.Windows.Forms.Button();
            this.addSection = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.reviewContentPanel = new System.Windows.Forms.Panel();
            this.resultTabPage = new MaterialWinforms.Controls.MaterialTabPage();
            this.resultContentPanel = new System.Windows.Forms.Panel();
            this.nextResult = new System.Windows.Forms.Button();
            this.previousResult = new System.Windows.Forms.Button();
            this.materialTabSelector1 = new MaterialWinforms.Controls.MaterialTabSelector();
            this.panel6 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.labelResultPath = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelErrorMessage = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.materialTabControl1.SuspendLayout();
            this.selectFilesTabPage.SuspendLayout();
            this.selectFilesContentPanel.SuspendLayout();
            this.reviewTabPage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.resultTabPage.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.selectFilesTabPage);
            this.materialTabControl1.Controls.Add(this.reviewTabPage);
            this.materialTabControl1.Controls.Add(this.resultTabPage);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.materialTabControl1.Location = new System.Drawing.Point(0, 35);
            this.materialTabControl1.MouseState = MaterialWinforms.MouseState.HOVER;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 2;
            this.materialTabControl1.Size = new System.Drawing.Size(1260, 618);
            this.materialTabControl1.TabIndex = 5;
            this.materialTabControl1.TabsAreClosable = true;
            this.materialTabControl1.SelectedIndexChanged += new System.EventHandler(this.materialTabControl1_SelectedIndexChanged);
            // 
            // selectFilesTabPage
            // 
            this.selectFilesTabPage.Closable = false;
            this.selectFilesTabPage.Controls.Add(this.selectFilesContentPanel);
            this.selectFilesTabPage.Depth = 0;
            this.selectFilesTabPage.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectFilesTabPage.Location = new System.Drawing.Point(4, 29);
            this.selectFilesTabPage.MouseState = MaterialWinforms.MouseState.HOVER;
            this.selectFilesTabPage.Name = "selectFilesTabPage";
            this.selectFilesTabPage.Size = new System.Drawing.Size(1252, 585);
            this.selectFilesTabPage.TabIndex = 0;
            this.selectFilesTabPage.Text = "Select File(s)";
            // 
            // selectFilesContentPanel
            // 
            this.selectFilesContentPanel.Controls.Add(this.button1);
            this.selectFilesContentPanel.Controls.Add(this.reviewButton);
            this.selectFilesContentPanel.Controls.Add(this.textBox2);
            this.selectFilesContentPanel.Controls.Add(this.textBox1);
            this.selectFilesContentPanel.Controls.Add(this.text_OR_imageBtn);
            this.selectFilesContentPanel.Controls.Add(this.txtTarget);
            this.selectFilesContentPanel.Controls.Add(this.txtSource);
            this.selectFilesContentPanel.Controls.Add(this.btnbrwTarget);
            this.selectFilesContentPanel.Controls.Add(this.btnbrwSource);
            this.selectFilesContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectFilesContentPanel.Location = new System.Drawing.Point(0, 0);
            this.selectFilesContentPanel.Name = "selectFilesContentPanel";
            this.selectFilesContentPanel.Size = new System.Drawing.Size(1252, 585);
            this.selectFilesContentPanel.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(830, 460);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
            this.button1.Size = new System.Drawing.Size(106, 46);
            this.button1.TabIndex = 18;
            this.button1.Text = "Reset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // reviewButton
            // 
            this.reviewButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.reviewButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reviewButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reviewButton.Location = new System.Drawing.Point(572, 438);
            this.reviewButton.Name = "reviewButton";
            this.reviewButton.Size = new System.Drawing.Size(196, 78);
            this.reviewButton.TabIndex = 17;
            this.reviewButton.Text = "Review";
            this.reviewButton.UseVisualStyleBackColor = true;
            this.reviewButton.Click += new System.EventHandler(this.reviewButton_Click);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox2.Font = new System.Drawing.Font("Yu Gothic UI Semibold", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.ForeColor = System.Drawing.Color.Black;
            this.textBox2.Location = new System.Drawing.Point(802, 82);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(136, 54);
            this.textBox2.TabIndex = 16;
            this.textBox2.Text = "IMAGE";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.textBox1.Font = new System.Drawing.Font("Yu Gothic UI Semibold", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.Black;
            this.textBox1.Location = new System.Drawing.Point(413, 82);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 54);
            this.textBox1.TabIndex = 15;
            this.textBox1.Text = "TEXT";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // text_OR_imageBtn
            // 
            this.text_OR_imageBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.text_OR_imageBtn.Depth = 0;
            this.text_OR_imageBtn.EllipseBorderColor = "#3b73d1";
            this.text_OR_imageBtn.EllipseColor = "#508ef5";
            this.text_OR_imageBtn.Location = new System.Drawing.Point(647, 102);
            this.text_OR_imageBtn.MouseState = MaterialWinforms.MouseState.HOVER;
            this.text_OR_imageBtn.Name = "text_OR_imageBtn";
            this.text_OR_imageBtn.Size = new System.Drawing.Size(47, 19);
            this.text_OR_imageBtn.TabIndex = 14;
            this.text_OR_imageBtn.UseVisualStyleBackColor = true;
            // 
            // txtTarget
            // 
            this.txtTarget.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtTarget.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTarget.Location = new System.Drawing.Point(878, 366);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(100, 27);
            this.txtTarget.TabIndex = 13;
            this.txtTarget.Visible = false;
            // 
            // txtSource
            // 
            this.txtSource.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.txtSource.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSource.Location = new System.Drawing.Point(353, 366);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(100, 27);
            this.txtSource.TabIndex = 12;
            this.txtSource.Visible = false;
            // 
            // btnbrwTarget
            // 
            this.btnbrwTarget.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnbrwTarget.BackColor = System.Drawing.SystemColors.Control;
            this.btnbrwTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnbrwTarget.Font = new System.Drawing.Font("Calibri", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnbrwTarget.ForeColor = System.Drawing.Color.DimGray;
            this.btnbrwTarget.Location = new System.Drawing.Point(758, 205);
            this.btnbrwTarget.Name = "btnbrwTarget";
            this.btnbrwTarget.Size = new System.Drawing.Size(220, 162);
            this.btnbrwTarget.TabIndex = 11;
            this.btnbrwTarget.Text = "Select Target File(s)";
            this.btnbrwTarget.UseVisualStyleBackColor = false;
            this.btnbrwTarget.Click += new System.EventHandler(this.btnbrwTarget_Click);
            // 
            // btnbrwSource
            // 
            this.btnbrwSource.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnbrwSource.BackColor = System.Drawing.SystemColors.Control;
            this.btnbrwSource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnbrwSource.Font = new System.Drawing.Font("Calibri", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnbrwSource.ForeColor = System.Drawing.Color.DimGray;
            this.btnbrwSource.Location = new System.Drawing.Point(353, 205);
            this.btnbrwSource.Name = "btnbrwSource";
            this.btnbrwSource.Size = new System.Drawing.Size(222, 162);
            this.btnbrwSource.TabIndex = 10;
            this.btnbrwSource.Text = "Select Source File(s)";
            this.btnbrwSource.UseVisualStyleBackColor = false;
            this.btnbrwSource.Click += new System.EventHandler(this.btnbrwSource_Click);
            // 
            // reviewTabPage
            // 
            this.reviewTabPage.AutoScroll = true;
            this.reviewTabPage.Closable = false;
            this.reviewTabPage.Controls.Add(this.panel2);
            this.reviewTabPage.Depth = 0;
            this.reviewTabPage.Location = new System.Drawing.Point(4, 29);
            this.reviewTabPage.MouseState = MaterialWinforms.MouseState.HOVER;
            this.reviewTabPage.Name = "reviewTabPage";
            this.reviewTabPage.Size = new System.Drawing.Size(1252, 585);
            this.reviewTabPage.TabIndex = 1;
            this.reviewTabPage.Text = "Review";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.reviewContentPanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1252, 585);
            this.panel2.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.progressBar1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 553);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1252, 32);
            this.panel4.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BackColor = System.Drawing.Color.White;
            this.progressBar1.ForeColor = System.Drawing.Color.LawnGreen;
            this.progressBar1.Location = new System.Drawing.Point(46, 6);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1168, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.labelResultPathDisplay);
            this.panel3.Controls.Add(this.btnSelectResultPath);
            this.panel3.Controls.Add(this.addSection);
            this.panel3.Controls.Add(this.btnCompare);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1252, 165);
            this.panel3.TabIndex = 1;
            // 
            // labelResultPathDisplay
            // 
            this.labelResultPathDisplay.AutoSize = true;
            this.labelResultPathDisplay.Location = new System.Drawing.Point(42, 83);
            this.labelResultPathDisplay.Name = "labelResultPathDisplay";
            this.labelResultPathDisplay.Size = new System.Drawing.Size(278, 20);
            this.labelResultPathDisplay.TabIndex = 6;
            this.labelResultPathDisplay.Text = "Default Path: C:\\\\CompareToolsResult";
            // 
            // btnSelectResultPath
            // 
            this.btnSelectResultPath.AutoSize = true;
            this.btnSelectResultPath.Location = new System.Drawing.Point(207, 115);
            this.btnSelectResultPath.Name = "btnSelectResultPath";
            this.btnSelectResultPath.Size = new System.Drawing.Size(170, 46);
            this.btnSelectResultPath.TabIndex = 5;
            this.btnSelectResultPath.Text = "Select Folder";
            this.btnSelectResultPath.UseVisualStyleBackColor = true;
            this.btnSelectResultPath.Click += new System.EventHandler(this.btnSelectResultPath_Click);
            // 
            // addSection
            // 
            this.addSection.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.addSection.Location = new System.Drawing.Point(545, 23);
            this.addSection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.addSection.Name = "addSection";
            this.addSection.Size = new System.Drawing.Size(189, 40);
            this.addSection.TabIndex = 4;
            this.addSection.Text = "Add More Files";
            this.addSection.UseVisualStyleBackColor = true;
            this.addSection.Click += new System.EventHandler(this.addSectionButton_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCompare.BackColor = System.Drawing.Color.LightGray;
            this.btnCompare.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnCompare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCompare.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCompare.Location = new System.Drawing.Point(485, 83);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(297, 63);
            this.btnCompare.TabIndex = 3;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = false;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // reviewContentPanel
            // 
            this.reviewContentPanel.AutoScroll = true;
            this.reviewContentPanel.AutoSize = true;
            this.reviewContentPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.reviewContentPanel.Location = new System.Drawing.Point(0, 0);
            this.reviewContentPanel.MaximumSize = new System.Drawing.Size(0, 649);
            this.reviewContentPanel.Name = "reviewContentPanel";
            this.reviewContentPanel.Size = new System.Drawing.Size(1252, 0);
            this.reviewContentPanel.TabIndex = 0;
            // 
            // resultTabPage
            // 
            this.resultTabPage.Closable = false;
            this.resultTabPage.Controls.Add(this.resultContentPanel);
            this.resultTabPage.Controls.Add(this.nextResult);
            this.resultTabPage.Controls.Add(this.previousResult);
            this.resultTabPage.Depth = 0;
            this.resultTabPage.Location = new System.Drawing.Point(4, 29);
            this.resultTabPage.MouseState = MaterialWinforms.MouseState.HOVER;
            this.resultTabPage.Name = "resultTabPage";
            this.resultTabPage.Size = new System.Drawing.Size(1252, 585);
            this.resultTabPage.TabIndex = 2;
            this.resultTabPage.Text = "Result";
            // 
            // resultContentPanel
            // 
            this.resultContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultContentPanel.Location = new System.Drawing.Point(70, 0);
            this.resultContentPanel.Name = "resultContentPanel";
            this.resultContentPanel.Size = new System.Drawing.Size(1112, 585);
            this.resultContentPanel.TabIndex = 1;
            // 
            // nextResult
            // 
            this.nextResult.Dock = System.Windows.Forms.DockStyle.Right;
            this.nextResult.FlatAppearance.BorderSize = 0;
            this.nextResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nextResult.Font = new System.Drawing.Font("Cooper Black", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nextResult.Location = new System.Drawing.Point(1182, 0);
            this.nextResult.Name = "nextResult";
            this.nextResult.Size = new System.Drawing.Size(70, 585);
            this.nextResult.TabIndex = 0;
            this.nextResult.Text = ">";
            this.nextResult.UseVisualStyleBackColor = true;
            this.nextResult.Click += new System.EventHandler(this.nextResult_Click);
            // 
            // previousResult
            // 
            this.previousResult.Dock = System.Windows.Forms.DockStyle.Left;
            this.previousResult.FlatAppearance.BorderSize = 0;
            this.previousResult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.previousResult.Font = new System.Drawing.Font("Cooper Black", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previousResult.Location = new System.Drawing.Point(0, 0);
            this.previousResult.Name = "previousResult";
            this.previousResult.Size = new System.Drawing.Size(70, 585);
            this.previousResult.TabIndex = 0;
            this.previousResult.Text = "<";
            this.previousResult.UseVisualStyleBackColor = true;
            this.previousResult.Click += new System.EventHandler(this.previousResult_Click);
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.CenterTabs = false;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Dock = System.Windows.Forms.DockStyle.Top;
            this.materialTabSelector1.Elevation = 10;
            this.materialTabSelector1.Location = new System.Drawing.Point(0, 0);
            this.materialTabSelector1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 9);
            this.materialTabSelector1.MaxTabWidht = -1;
            this.materialTabSelector1.MouseState = MaterialWinforms.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            graphicsPath2.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
            this.materialTabSelector1.ShadowBorder = graphicsPath2;
            this.materialTabSelector1.Size = new System.Drawing.Size(1260, 35);
            this.materialTabSelector1.TabIndex = 0;
            this.materialTabSelector1.TabPadding = 24;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.pictureBox1);
            this.panel6.Controls.Add(this.copyrightLabel);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel6.Location = new System.Drawing.Point(0, 41);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1260, 51);
            this.panel6.TabIndex = 8;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(126, 51);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.copyrightLabel.Location = new System.Drawing.Point(357, 15);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(574, 20);
            this.copyrightLabel.TabIndex = 0;
            this.copyrightLabel.Text = "Label";
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelResultPath
            // 
            this.labelResultPath.AutoSize = true;
            this.labelResultPath.BackColor = System.Drawing.SystemColors.Control;
            this.labelResultPath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelResultPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResultPath.ForeColor = System.Drawing.Color.OrangeRed;
            this.labelResultPath.Location = new System.Drawing.Point(706, 8);
            this.labelResultPath.Name = "labelResultPath";
            this.labelResultPath.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelResultPath.Size = new System.Drawing.Size(0, 25);
            this.labelResultPath.TabIndex = 0;
            this.labelResultPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelResultPath.Visible = false;
            this.labelResultPath.Click += new System.EventHandler(this.labelResultPath_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelResultPath);
            this.panel1.Controls.Add(this.labelErrorMessage);
            this.panel1.Controls.Add(this.panel6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 653);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1260, 92);
            this.panel1.TabIndex = 1;
            // 
            // labelErrorMessage
            // 
            this.labelErrorMessage.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelErrorMessage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelErrorMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelErrorMessage.Location = new System.Drawing.Point(0, 0);
            this.labelErrorMessage.Name = "labelErrorMessage";
            this.labelErrorMessage.Size = new System.Drawing.Size(1260, 41);
            this.labelErrorMessage.TabIndex = 9;
            this.labelErrorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.background_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.background_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.background_RunWorkerCompleted);
            // 
            // compareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1260, 745);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.materialTabSelector1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "compareForm";
            this.Text = "Forms Compare Tool";
            this.materialTabControl1.ResumeLayout(false);
            this.selectFilesTabPage.ResumeLayout(false);
            this.selectFilesContentPanel.ResumeLayout(false);
            this.selectFilesContentPanel.PerformLayout();
            this.reviewTabPage.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.resultTabPage.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialWinforms.Controls.MaterialTabControl materialTabControl1;
        private MaterialWinforms.Controls.MaterialTabPage selectFilesTabPage;
        private System.Windows.Forms.Panel selectFilesContentPanel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button reviewButton;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private MaterialWinforms.Controls.MaterialToggle text_OR_imageBtn;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Button btnbrwTarget;
        private System.Windows.Forms.Button btnbrwSource;
        private MaterialWinforms.Controls.MaterialTabPage reviewTabPage;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.Panel reviewContentPanel;
        private MaterialWinforms.Controls.MaterialTabPage resultTabPage;
        private System.Windows.Forms.Panel resultContentPanel;
        private System.Windows.Forms.Button nextResult;
        private System.Windows.Forms.Button previousResult;
        private MaterialWinforms.Controls.MaterialTabSelector materialTabSelector1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Label labelResultPath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelErrorMessage;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button addSection;
        private System.Windows.Forms.Button btnSelectResultPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label labelResultPathDisplay;
    }
}
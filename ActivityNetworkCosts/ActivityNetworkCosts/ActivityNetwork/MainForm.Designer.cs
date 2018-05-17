namespace ActivityNetwork
{
    partial class MainForm
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
            this.pbMain = new System.Windows.Forms.PictureBox();
            this.dgvAct = new System.Windows.Forms.DataGridView();
            this.Название = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ПредРаботы = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.МинДлительность = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.МаксДлительность = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.МинСтоимость = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.МаксСтоимость = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tc = new System.Windows.Forms.TabControl();
            this.tpData = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tpOptRes = new System.Windows.Forms.TabPage();
            this.wb = new System.Windows.Forms.WebBrowser();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbIter = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbResult = new System.Windows.Forms.ListBox();
            this.tbTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pbMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAct)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tc.SuspendLayout();
            this.tpData.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpOptRes.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbMain
            // 
            this.pbMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbMain.Location = new System.Drawing.Point(0, 0);
            this.pbMain.Name = "pbMain";
            this.pbMain.Size = new System.Drawing.Size(288, 323);
            this.pbMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbMain.TabIndex = 0;
            this.pbMain.TabStop = false;
            this.pbMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this.pbMain.SizeChanged += new System.EventHandler(this.pbMain_SizeChanged);
            // 
            // dgvAct
            // 
            this.dgvAct.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAct.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAct.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Название,
            this.ПредРаботы,
            this.МинДлительность,
            this.МаксДлительность,
            this.МинСтоимость,
            this.МаксСтоимость});
            this.dgvAct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAct.Location = new System.Drawing.Point(3, 16);
            this.dgvAct.Name = "dgvAct";
            this.dgvAct.Size = new System.Drawing.Size(522, 261);
            this.dgvAct.TabIndex = 1;
            this.dgvAct.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellValueChanged);
            // 
            // Название
            // 
            this.Название.HeaderText = "Название";
            this.Название.Name = "Название";
            // 
            // ПредРаботы
            // 
            this.ПредРаботы.HeaderText = "Пред. работы";
            this.ПредРаботы.Name = "ПредРаботы";
            // 
            // МинДлительность
            // 
            this.МинДлительность.HeaderText = "Мин. длительность";
            this.МинДлительность.Name = "МинДлительность";
            // 
            // МаксДлительность
            // 
            this.МаксДлительность.HeaderText = "Макс. длительность";
            this.МаксДлительность.Name = "МаксДлительность";
            // 
            // МинСтоимость
            // 
            this.МинСтоимость.HeaderText = "Мин. стоимость";
            this.МинСтоимость.Name = "МинСтоимость";
            // 
            // МаксСтоимость
            // 
            this.МаксСтоимость.HeaderText = "Макс. стоимость";
            this.МаксСтоимость.Name = "МаксСтоимость";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 26);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tc);
            this.splitContainer1.Panel1MinSize = 300;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pbMain);
            this.splitContainer1.Size = new System.Drawing.Size(840, 323);
            this.splitContainer1.SplitterDistance = 548;
            this.splitContainer1.TabIndex = 4;
            // 
            // tc
            // 
            this.tc.Controls.Add(this.tpData);
            this.tc.Controls.Add(this.tpOptRes);
            this.tc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tc.Location = new System.Drawing.Point(0, 0);
            this.tc.Name = "tc";
            this.tc.SelectedIndex = 0;
            this.tc.Size = new System.Drawing.Size(548, 323);
            this.tc.TabIndex = 1;
            this.tc.SelectedIndexChanged += new System.EventHandler(this.tc_TabIndexChanged);
            // 
            // tpData
            // 
            this.tpData.Controls.Add(this.groupBox1);
            this.tpData.Location = new System.Drawing.Point(4, 27);
            this.tpData.Name = "tpData";
            this.tpData.Padding = new System.Windows.Forms.Padding(3);
            this.tpData.Size = new System.Drawing.Size(540, 292);
            this.tpData.TabIndex = 0;
            this.tpData.Text = "Данные";
            this.tpData.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dgvAct);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(528, 280);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Работы";
            // 
            // tpOptRes
            // 
            this.tpOptRes.Controls.Add(this.wb);
            this.tpOptRes.Controls.Add(this.groupBox2);
            this.tpOptRes.Controls.Add(this.groupBox3);
            this.tpOptRes.Controls.Add(this.tbTime);
            this.tpOptRes.Controls.Add(this.label2);
            this.tpOptRes.Location = new System.Drawing.Point(4, 27);
            this.tpOptRes.Name = "tpOptRes";
            this.tpOptRes.Padding = new System.Windows.Forms.Padding(3);
            this.tpOptRes.Size = new System.Drawing.Size(540, 292);
            this.tpOptRes.TabIndex = 2;
            this.tpOptRes.Text = "Оптимизация";
            this.tpOptRes.UseVisualStyleBackColor = true;
            // 
            // wb
            // 
            this.wb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wb.Location = new System.Drawing.Point(8, 197);
            this.wb.MinimumSize = new System.Drawing.Size(20, 20);
            this.wb.Name = "wb";
            this.wb.Size = new System.Drawing.Size(526, 89);
            this.wb.TabIndex = 7;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lbIter);
            this.groupBox2.Location = new System.Drawing.Point(11, 114);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(518, 77);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Итерации симплекс-метода (минимизация суммарных издержек, F = F * -1)";
            // 
            // lbIter
            // 
            this.lbIter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbIter.FormattingEnabled = true;
            this.lbIter.Location = new System.Drawing.Point(3, 16);
            this.lbIter.Name = "lbIter";
            this.lbIter.Size = new System.Drawing.Size(512, 56);
            this.lbIter.TabIndex = 4;
            this.lbIter.SelectedValueChanged += new System.EventHandler(this.lbIter_SelectedValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lbResult);
            this.groupBox3.Location = new System.Drawing.Point(11, 32);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(518, 76);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Результаты";
            // 
            // lbResult
            // 
            this.lbResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbResult.FormattingEnabled = true;
            this.lbResult.Location = new System.Drawing.Point(3, 16);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(512, 56);
            this.lbResult.TabIndex = 4;
            this.lbResult.SelectedValueChanged += new System.EventHandler(this.lbResult_SelectedValueChanged);
            // 
            // tbTime
            // 
            this.tbTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTime.Location = new System.Drawing.Point(149, 6);
            this.tbTime.Name = "tbTime";
            this.tbTime.Size = new System.Drawing.Size(385, 20);
            this.tbTime.TabIndex = 3;
            this.tbTime.Text = "1000";
            this.tbTime.TextChanged += new System.EventHandler(this.tbTime_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Директивное время:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.exampleToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(840, 26);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(49, 22);
            this.fileToolStripMenuItem.Text = "Файл";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveToolStripMenuItem.Text = "Сохранить отчет...";
            this.saveToolStripMenuItem.Visible = false;
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.exitToolStripMenuItem.Text = "Выход";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // exampleToolStripMenuItem
            // 
            this.exampleToolStripMenuItem.Name = "exampleToolStripMenuItem";
            this.exampleToolStripMenuItem.Size = new System.Drawing.Size(64, 22);
            this.exampleToolStripMenuItem.Text = "Пример";
            this.exampleToolStripMenuItem.Click += new System.EventHandler(this.exampleToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 22);
            this.helpToolStripMenuItem.Text = "Справка";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.aboutToolStripMenuItem.Text = "О программе";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 349);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Сетевое планирование проекта: учет зависимости стоимости от продолжительности";
            ((System.ComponentModel.ISupportInitialize)(this.pbMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAct)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tc.ResumeLayout(false);
            this.tpData.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tpOptRes.ResumeLayout(false);
            this.tpOptRes.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbMain;
        private System.Windows.Forms.DataGridView dgvAct;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabControl tc;
        private System.Windows.Forms.TabPage tpData;
        private System.Windows.Forms.TabPage tpOptRes;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TextBox tbTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lbResult;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Название;
        private System.Windows.Forms.DataGridViewTextBoxColumn ПредРаботы;
        private System.Windows.Forms.DataGridViewTextBoxColumn МинДлительность;
        private System.Windows.Forms.DataGridViewTextBoxColumn МаксДлительность;
        private System.Windows.Forms.DataGridViewTextBoxColumn МинСтоимость;
        private System.Windows.Forms.DataGridViewTextBoxColumn МаксСтоимость;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lbIter;
        private System.Windows.Forms.WebBrowser wb;
        private System.Windows.Forms.ToolStripMenuItem exampleToolStripMenuItem;
    }
}


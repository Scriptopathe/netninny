namespace NetNinny.UI
{
    partial class NetNinnyForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_filtersTextbox = new System.Windows.Forms.TextBox();
            this.m_browseButton = new System.Windows.Forms.Button();
            this.m_portUpdown = new System.Windows.Forms.NumericUpDown();
            this.m_toggleButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.m_statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_logTabs = new System.Windows.Forms.TabControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.m_portUpdown)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.m_logTabs.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Filters :";
            // 
            // m_filtersTextbox
            // 
            this.m_filtersTextbox.Location = new System.Drawing.Point(49, 32);
            this.m_filtersTextbox.Name = "m_filtersTextbox";
            this.m_filtersTextbox.Size = new System.Drawing.Size(124, 20);
            this.m_filtersTextbox.TabIndex = 2;
            this.m_filtersTextbox.Text = "filters.txt";
            // 
            // m_browseButton
            // 
            this.m_browseButton.Location = new System.Drawing.Point(179, 30);
            this.m_browseButton.Name = "m_browseButton";
            this.m_browseButton.Size = new System.Drawing.Size(62, 23);
            this.m_browseButton.TabIndex = 3;
            this.m_browseButton.Text = "Browse...";
            this.m_browseButton.UseVisualStyleBackColor = true;
            // 
            // m_portUpdown
            // 
            this.m_portUpdown.Location = new System.Drawing.Point(49, 4);
            this.m_portUpdown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.m_portUpdown.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.m_portUpdown.Name = "m_portUpdown";
            this.m_portUpdown.Size = new System.Drawing.Size(192, 20);
            this.m_portUpdown.TabIndex = 4;
            this.m_portUpdown.Value = new decimal(new int[] {
            5001,
            0,
            0,
            0});
            // 
            // m_toggleButton
            // 
            this.m_toggleButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_toggleButton.Location = new System.Drawing.Point(253, 3);
            this.m_toggleButton.Name = "m_toggleButton";
            this.m_toggleButton.Size = new System.Drawing.Size(702, 58);
            this.m_toggleButton.TabIndex = 5;
            this.m_toggleButton.Text = "Start";
            this.m_toggleButton.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_statusText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 364);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(964, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // m_statusText
            // 
            this.m_statusText.Name = "m_statusText";
            this.m_statusText.Size = new System.Drawing.Size(45, 17);
            this.m_statusText.Text = "Status :";
            // 
            // m_logTabs
            // 
            this.m_logTabs.Controls.Add(this.tabPage1);
            this.m_logTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_logTabs.Location = new System.Drawing.Point(3, 73);
            this.m_logTabs.Multiline = true;
            this.m_logTabs.Name = "m_logTabs";
            this.m_logTabs.SelectedIndex = 0;
            this.m_logTabs.Size = new System.Drawing.Size(958, 288);
            this.m_logTabs.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.m_logTabs.TabIndex = 7;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.m_logTabs, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(964, 364);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.m_toggleButton, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(958, 64);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.m_portUpdown);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.m_filtersTextbox);
            this.panel1.Controls.Add(this.m_browseButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(244, 58);
            this.panel1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(950, 262);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // NetNinnyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 386);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "NetNinnyForm";
            this.Text = "Net Ninny UI";
            ((System.ComponentModel.ISupportInitialize)(this.m_portUpdown)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.m_logTabs.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_filtersTextbox;
        private System.Windows.Forms.Button m_browseButton;
        private System.Windows.Forms.NumericUpDown m_portUpdown;
        private System.Windows.Forms.Button m_toggleButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel m_statusText;
        private System.Windows.Forms.TabControl m_logTabs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage1;
    }
}


namespace HTCViveDroneController
{
    partial class HTCViveDroneController
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
            this.tlpMainPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnVjoyMonitor = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnConfig = new System.Windows.Forms.Button();
            this.cmbVJoyId = new System.Windows.Forms.ComboBox();
            this.lblDeviceId = new System.Windows.Forms.Label();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tlpMainPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMainPanel
            // 
            this.tlpMainPanel.AutoSize = true;
            this.tlpMainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMainPanel.ColumnCount = 6;
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpMainPanel.Controls.Add(this.btnVjoyMonitor, 2, 2);
            this.tlpMainPanel.Controls.Add(this.lblStatus, 1, 1);
            this.tlpMainPanel.Controls.Add(this.btnConfig, 1, 2);
            this.tlpMainPanel.Controls.Add(this.cmbVJoyId, 4, 2);
            this.tlpMainPanel.Controls.Add(this.lblDeviceId, 3, 2);
            this.tlpMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMainPanel.Location = new System.Drawing.Point(0, 0);
            this.tlpMainPanel.Name = "tlpMainPanel";
            this.tlpMainPanel.RowCount = 3;
            this.tlpMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpMainPanel.Size = new System.Drawing.Size(482, 57);
            this.tlpMainPanel.TabIndex = 0;
            // 
            // btnVjoyMonitor
            // 
            this.btnVjoyMonitor.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnVjoyMonitor.Location = new System.Drawing.Point(153, 24);
            this.btnVjoyMonitor.Name = "btnVjoyMonitor";
            this.btnVjoyMonitor.Size = new System.Drawing.Size(90, 30);
            this.btnVjoyMonitor.TabIndex = 6;
            this.btnVjoyMonitor.Text = "VJoy Monitor";
            this.btnVjoyMonitor.UseVisualStyleBackColor = true;
            this.btnVjoyMonitor.Click += new System.EventHandler(this.btnVjoyMonitor_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.tlpMainPanel.SetColumnSpan(this.lblStatus, 5);
            this.lblStatus.Location = new System.Drawing.Point(13, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 11);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "status";
            // 
            // btnConfig
            // 
            this.btnConfig.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnConfig.Location = new System.Drawing.Point(13, 24);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(116, 30);
            this.btnConfig.TabIndex = 1;
            this.btnConfig.Text = "Controller Setup";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // cmbVJoyId
            // 
            this.cmbVJoyId.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmbVJoyId.FormattingEnabled = true;
            this.cmbVJoyId.Location = new System.Drawing.Point(358, 28);
            this.cmbVJoyId.Name = "cmbVJoyId";
            this.cmbVJoyId.Size = new System.Drawing.Size(121, 23);
            this.cmbVJoyId.TabIndex = 4;
            this.cmbVJoyId.SelectedIndexChanged += new System.EventHandler(this.cmbVJoyId_SelectedIndexChanged);
            // 
            // lblDeviceId
            // 
            this.lblDeviceId.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblDeviceId.AutoSize = true;
            this.lblDeviceId.Location = new System.Drawing.Point(267, 39);
            this.lblDeviceId.Margin = new System.Windows.Forms.Padding(3);
            this.lblDeviceId.Name = "lblDeviceId";
            this.lblDeviceId.Size = new System.Drawing.Size(85, 15);
            this.lblDeviceId.TabIndex = 5;
            this.lblDeviceId.Text = "VJoy Device Id";
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tlpMainPanel);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(482, 57);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(482, 82);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // HTCViveDroneController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 82);
            this.Controls.Add(this.toolStripContainer1);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.Name = "HTCViveDroneController";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "VjoyVive";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HTCViveDroneController_FormClosing);
            this.Shown += new System.EventHandler(this.HTCViveDroneController_Shown);
            this.tlpMainPanel.ResumeLayout(false);
            this.tlpMainPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMainPanel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.ComboBox cmbVJoyId;
        private System.Windows.Forms.Label lblDeviceId;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.Button btnVjoyMonitor;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
namespace HTCViveDroneController
{
    partial class ConfigPanel
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
            this.tlpConfig = new System.Windows.Forms.TableLayoutPanel();
            this.lblLeftController = new System.Windows.Forms.Label();
            this.lblRightController = new System.Windows.Forms.Label();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblConfiguration = new System.Windows.Forms.Label();
            this.cmbConfigurations = new System.Windows.Forms.ComboBox();
            this.tlpConfig.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpConfig
            // 
            this.tlpConfig.AutoSize = true;
            this.tlpConfig.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpConfig.ColumnCount = 7;
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpConfig.Controls.Add(this.lblLeftController, 1, 3);
            this.tlpConfig.Controls.Add(this.lblRightController, 4, 3);
            this.tlpConfig.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tlpConfig.Controls.Add(this.flpButtons, 1, 10);
            this.tlpConfig.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tlpConfig.Location = new System.Drawing.Point(0, 0);
            this.tlpConfig.Name = "tlpConfig";
            this.tlpConfig.RowCount = 11;
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpConfig.Size = new System.Drawing.Size(596, 179);
            this.tlpConfig.TabIndex = 0;
            // 
            // lblLeftController
            // 
            this.lblLeftController.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblLeftController.AutoSize = true;
            this.tlpConfig.SetColumnSpan(this.lblLeftController, 2);
            this.lblLeftController.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLeftController.Location = new System.Drawing.Point(90, 65);
            this.lblLeftController.Name = "lblLeftController";
            this.lblLeftController.Size = new System.Drawing.Size(123, 19);
            this.lblLeftController.TabIndex = 12;
            this.lblLeftController.Text = "Left Controller";
            // 
            // lblRightController
            // 
            this.lblRightController.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblRightController.AutoSize = true;
            this.tlpConfig.SetColumnSpan(this.lblRightController, 2);
            this.lblRightController.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRightController.Location = new System.Drawing.Point(376, 65);
            this.lblRightController.Name = "lblRightController";
            this.lblRightController.Size = new System.Drawing.Size(135, 19);
            this.lblRightController.TabIndex = 13;
            this.lblRightController.Text = "Right Controller";
            // 
            // flpButtons
            // 
            this.flpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flpButtons.AutoSize = true;
            this.flpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpConfig.SetColumnSpan(this.flpButtons, 5);
            this.flpButtons.Controls.Add(this.btnCancel);
            this.flpButtons.Controls.Add(this.btnAccept);
            this.flpButtons.Controls.Add(this.btnOK);
            this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtons.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flpButtons.Location = new System.Drawing.Point(302, 134);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(279, 33);
            this.flpButtons.TabIndex = 6;
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAccept.Location = new System.Drawing.Point(96, 3);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(87, 27);
            this.btnAccept.TabIndex = 4;
            this.btnAccept.Text = "Ok";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(189, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 27);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(3, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 27);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Visible = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpConfig.SetColumnSpan(this.flowLayoutPanel1, 3);
            this.flowLayoutPanel1.Controls.Add(this.lblConfiguration);
            this.flowLayoutPanel1.Controls.Add(this.cmbConfigurations);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(32, 15);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(251, 32);
            this.flowLayoutPanel1.TabIndex = 16;
            // 
            // lblConfiguration
            // 
            this.lblConfiguration.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblConfiguration.AutoSize = true;
            this.lblConfiguration.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfiguration.Location = new System.Drawing.Point(3, 7);
            this.lblConfiguration.Name = "lblConfiguration";
            this.lblConfiguration.Size = new System.Drawing.Size(105, 18);
            this.lblConfiguration.TabIndex = 3;
            this.lblConfiguration.Text = "Configuration:";
            this.lblConfiguration.Visible = false;
            // 
            // cmbConfigurations
            // 
            this.cmbConfigurations.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbConfigurations.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbConfigurations.FormattingEnabled = true;
            this.cmbConfigurations.Items.AddRange(new object[] {
            "Right Hand",
            "Left Hand"});
            this.cmbConfigurations.Location = new System.Drawing.Point(114, 3);
            this.cmbConfigurations.Name = "cmbConfigurations";
            this.cmbConfigurations.Size = new System.Drawing.Size(134, 26);
            this.cmbConfigurations.TabIndex = 4;
            this.cmbConfigurations.Visible = false;
            this.cmbConfigurations.SelectedIndexChanged += new System.EventHandler(this.cmbConfigurations_SelectedIndexChanged);
            // 
            // ConfigPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(624, 350);
            this.ControlBox = false;
            this.Controls.Add(this.tlpConfig);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ConfigPanel";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ConfigPanel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigPanel_FormClosing);
            this.tlpConfig.ResumeLayout(false);
            this.tlpConfig.PerformLayout();
            this.flpButtons.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpConfig;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Label lblLeftController;
        private System.Windows.Forms.Label lblRightController;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblConfiguration;
        private System.Windows.Forms.ComboBox cmbConfigurations;
        private System.Windows.Forms.Button btnOK;
    }
}
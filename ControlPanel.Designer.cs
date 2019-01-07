namespace HTCViveDroneController
{
    partial class ControlPanel
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
            this.panelXYSpace = new System.Windows.Forms.Panel();
            this.panelXY = new System.Windows.Forms.Panel();
            this.panelZSpace = new System.Windows.Forms.Panel();
            this.panelZ = new System.Windows.Forms.Panel();
            this.panelRotSpace = new System.Windows.Forms.Panel();
            this.panelRot = new System.Windows.Forms.Panel();
            this.panelXYSpace.SuspendLayout();
            this.panelZSpace.SuspendLayout();
            this.panelRotSpace.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelXYSpace
            // 
            this.panelXYSpace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelXYSpace.Controls.Add(this.panelXY);
            this.panelXYSpace.Location = new System.Drawing.Point(40, 12);
            this.panelXYSpace.Name = "panelXYSpace";
            this.panelXYSpace.Size = new System.Drawing.Size(215, 215);
            this.panelXYSpace.TabIndex = 0;
            // 
            // panelXY
            // 
            this.panelXY.BackColor = System.Drawing.Color.Maroon;
            this.panelXY.Location = new System.Drawing.Point(64, 76);
            this.panelXY.Name = "panelXY";
            this.panelXY.Size = new System.Drawing.Size(15, 15);
            this.panelXY.TabIndex = 0;
            // 
            // panelZSpace
            // 
            this.panelZSpace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelZSpace.Controls.Add(this.panelZ);
            this.panelZSpace.Location = new System.Drawing.Point(12, 12);
            this.panelZSpace.Name = "panelZSpace";
            this.panelZSpace.Size = new System.Drawing.Size(22, 215);
            this.panelZSpace.TabIndex = 1;
            // 
            // panelZ
            // 
            this.panelZ.BackColor = System.Drawing.Color.Maroon;
            this.panelZ.Location = new System.Drawing.Point(3, 76);
            this.panelZ.Name = "panelZ";
            this.panelZ.Size = new System.Drawing.Size(15, 15);
            this.panelZ.TabIndex = 0;
            // 
            // panelRotSpace
            // 
            this.panelRotSpace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRotSpace.Controls.Add(this.panelRot);
            this.panelRotSpace.Location = new System.Drawing.Point(40, 233);
            this.panelRotSpace.Name = "panelRotSpace";
            this.panelRotSpace.Size = new System.Drawing.Size(215, 22);
            this.panelRotSpace.TabIndex = 2;
            // 
            // panelRot
            // 
            this.panelRot.BackColor = System.Drawing.Color.Maroon;
            this.panelRot.Location = new System.Drawing.Point(3, 3);
            this.panelRot.Name = "panelRot";
            this.panelRot.Size = new System.Drawing.Size(15, 15);
            this.panelRot.TabIndex = 0;
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(271, 261);
            this.Controls.Add(this.panelRotSpace);
            this.Controls.Add(this.panelZSpace);
            this.Controls.Add(this.panelXYSpace);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ControlPanel";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Drone Controller Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlPanel_FormClosing);
            this.panelXYSpace.ResumeLayout(false);
            this.panelZSpace.ResumeLayout(false);
            this.panelRotSpace.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelXYSpace;
        private System.Windows.Forms.Panel panelXY;
        private System.Windows.Forms.Panel panelZSpace;
        private System.Windows.Forms.Panel panelZ;
        private System.Windows.Forms.Panel panelRotSpace;
        private System.Windows.Forms.Panel panelRot;
    }
}
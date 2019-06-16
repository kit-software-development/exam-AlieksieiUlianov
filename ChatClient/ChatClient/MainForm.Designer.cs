namespace ChatClient
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblAppartment = new System.Windows.Forms.Label();
            this.txtAppartment = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.Appart_Password = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(20, 112);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(169, 28);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(136, 14);
            this.txtIp.Margin = new System.Windows.Forms.Padding(4);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(169, 22);
            this.txtIp.TabIndex = 1;
            this.txtIp.Text = "127.0.0.1";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(17, 17);
            this.lblServer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(70, 17);
            this.lblServer.TabIndex = 2;
            this.lblServer.Text = "Server IP:";
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(417, 406);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 28);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Enabled = false;
            this.txtMessage.Location = new System.Drawing.Point(21, 408);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(4);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(386, 22);
            this.txtMessage.TabIndex = 4;
            this.txtMessage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMessage_KeyPress);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.Location = new System.Drawing.Point(21, 148);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(496, 250);
            this.txtLog.TabIndex = 5;
            // 
            // lblAppartment
            // 
            this.lblAppartment.AutoSize = true;
            this.lblAppartment.Location = new System.Drawing.Point(17, 49);
            this.lblAppartment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAppartment.Name = "lblAppartment";
            this.lblAppartment.Size = new System.Drawing.Size(102, 17);
            this.lblAppartment.TabIndex = 6;
            this.lblAppartment.Text = "Appartment ID:";
            // 
            // txtAppartment
            // 
            this.txtAppartment.Location = new System.Drawing.Point(136, 46);
            this.txtAppartment.Margin = new System.Windows.Forms.Padding(4);
            this.txtAppartment.Name = "txtAppartment";
            this.txtAppartment.Size = new System.Drawing.Size(169, 22);
            this.txtAppartment.TabIndex = 7;
            this.txtAppartment.Text = "320";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(136, 76);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(169, 22);
            this.txtPassword.TabIndex = 8;
            this.txtPassword.Text = "320Opa320";
            // 
            // Appart_Password
            // 
            this.Appart_Password.AutoSize = true;
            this.Appart_Password.Location = new System.Drawing.Point(17, 79);
            this.Appart_Password.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Appart_Password.Name = "Appart_Password";
            this.Appart_Password.Size = new System.Drawing.Size(77, 17);
            this.Appart_Password.TabIndex = 9;
            this.Appart_Password.Text = "Password: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 453);
            this.Controls.Add(this.Appart_Password);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtAppartment);
            this.Controls.Add(this.lblAppartment);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.btnConnect);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(430, 500);
            this.Name = "Form1";
            this.Text = "Chat Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblAppartment;
        private System.Windows.Forms.TextBox txtAppartment;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label Appart_Password;
    }
}


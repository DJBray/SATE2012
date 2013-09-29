namespace SkeletalTracking
{
    partial class LoginForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ErrorMessageLbl = new System.Windows.Forms.Label();
            this.HelpBtn = new System.Windows.Forms.Button();
            this.PwdLbl = new System.Windows.Forms.Label();
            this.FirstnameLBL = new System.Windows.Forms.Label();
            this.FirstNameTxtBx = new System.Windows.Forms.TextBox();
            this.PwdTxtBx = new System.Windows.Forms.TextBox();
            this.loginBtn = new System.Windows.Forms.Button();
            this.LastNameTxtBx = new System.Windows.Forms.TextBox();
            this.LastNameLbl = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LastNameLbl);
            this.groupBox1.Controls.Add(this.LastNameTxtBx);
            this.groupBox1.Controls.Add(this.ErrorMessageLbl);
            this.groupBox1.Controls.Add(this.HelpBtn);
            this.groupBox1.Controls.Add(this.PwdLbl);
            this.groupBox1.Controls.Add(this.FirstnameLBL);
            this.groupBox1.Controls.Add(this.FirstNameTxtBx);
            this.groupBox1.Controls.Add(this.PwdTxtBx);
            this.groupBox1.Controls.Add(this.loginBtn);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(379, 246);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // ErrorMessageLbl
            // 
            this.ErrorMessageLbl.AutoSize = true;
            this.ErrorMessageLbl.Location = new System.Drawing.Point(25, 215);
            this.ErrorMessageLbl.Name = "ErrorMessageLbl";
            this.ErrorMessageLbl.Size = new System.Drawing.Size(0, 17);
            this.ErrorMessageLbl.TabIndex = 7;
            this.ErrorMessageLbl.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 6);
            // 
            // HelpBtn
            // 
            this.HelpBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.HelpBtn.Location = new System.Drawing.Point(303, 179);
            this.HelpBtn.Name = "HelpBtn";
            this.HelpBtn.Size = new System.Drawing.Size(57, 31);
            this.HelpBtn.TabIndex = 6;
            this.HelpBtn.Text = "Help";
            this.HelpBtn.UseVisualStyleBackColor = true;
            // 
            // PwdLbl
            // 
            this.PwdLbl.AutoSize = true;
            this.PwdLbl.Location = new System.Drawing.Point(56, 108);
            this.PwdLbl.Name = "PwdLbl";
            this.PwdLbl.Size = new System.Drawing.Size(73, 17);
            this.PwdLbl.TabIndex = 4;
            this.PwdLbl.Text = "Password:";
            // 
            // FirstnameLBL
            // 
            this.FirstnameLBL.AutoSize = true;
            this.FirstnameLBL.Location = new System.Drawing.Point(56, 18);
            this.FirstnameLBL.Name = "FirstnameLBL";
            this.FirstnameLBL.Size = new System.Drawing.Size(84, 17);
            this.FirstnameLBL.TabIndex = 3;
            this.FirstnameLBL.Text = "First Name: ";
            // 
            // FirstNameTxtBx
            // 
            this.FirstNameTxtBx.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FirstNameTxtBx.Location = new System.Drawing.Point(59, 51);
            this.FirstNameTxtBx.Name = "FirstNameTxtBx";
            this.FirstNameTxtBx.Size = new System.Drawing.Size(115, 30);
            this.FirstNameTxtBx.TabIndex = 0;
            // 
            // PwdTxtBx
            // 
            this.PwdTxtBx.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PwdTxtBx.Location = new System.Drawing.Point(59, 140);
            this.PwdTxtBx.Name = "PwdTxtBx";
            this.PwdTxtBx.PasswordChar = '*';
            this.PwdTxtBx.Size = new System.Drawing.Size(249, 30);
            this.PwdTxtBx.TabIndex = 2;
            this.PwdTxtBx.UseSystemPasswordChar = true;
            // 
            // loginBtn
            // 
            this.loginBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.loginBtn.Location = new System.Drawing.Point(138, 178);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(100, 32);
            this.loginBtn.TabIndex = 0;
            this.loginBtn.Text = "Login";
            this.loginBtn.UseVisualStyleBackColor = true;
            this.loginBtn.Click += new System.EventHandler(this.loginBtn_Click);
            // 
            // LastNameTxtBx
            // 
            this.LastNameTxtBx.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LastNameTxtBx.Location = new System.Drawing.Point(180, 51);
            this.LastNameTxtBx.Name = "LastNameTxtBx";
            this.LastNameTxtBx.Size = new System.Drawing.Size(128, 30);
            this.LastNameTxtBx.TabIndex = 1;
            // 
            // LastNameLbl
            // 
            this.LastNameLbl.AutoSize = true;
            this.LastNameLbl.Location = new System.Drawing.Point(180, 18);
            this.LastNameLbl.Name = "LastNameLbl";
            this.LastNameLbl.Size = new System.Drawing.Size(80, 17);
            this.LastNameLbl.TabIndex = 9;
            this.LastNameLbl.Text = "Last Name:";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 270);
            this.Controls.Add(this.groupBox1);
            this.Name = "LoginForm";
            this.Text = "Login Screen";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label PwdLbl;
        private System.Windows.Forms.Label FirstnameLBL;
        private System.Windows.Forms.TextBox FirstNameTxtBx;
        private System.Windows.Forms.TextBox PwdTxtBx;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Label ErrorMessageLbl;
        private System.Windows.Forms.Button HelpBtn;
        private System.Windows.Forms.TextBox LastNameTxtBx;
        private System.Windows.Forms.Label LastNameLbl;
    }
}
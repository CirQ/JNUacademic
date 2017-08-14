namespace JNUacademic {
    partial class LoginForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.stuNumTxt = new System.Windows.Forms.TextBox();
            this.pwTxt = new System.Windows.Forms.TextBox();
            this.capTxt = new System.Windows.Forms.TextBox();
            this.capImg = new System.Windows.Forms.PictureBox();
            this.loginBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.banButtonTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.capImg)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Student Number";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(64, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "Captcha";
            // 
            // stuNumTxt
            // 
            this.stuNumTxt.Location = new System.Drawing.Point(118, 20);
            this.stuNumTxt.Margin = new System.Windows.Forms.Padding(2);
            this.stuNumTxt.MaxLength = 10;
            this.stuNumTxt.Name = "stuNumTxt";
            this.stuNumTxt.Size = new System.Drawing.Size(111, 21);
            this.stuNumTxt.TabIndex = 0;
            // 
            // pwTxt
            // 
            this.pwTxt.Location = new System.Drawing.Point(118, 44);
            this.pwTxt.Margin = new System.Windows.Forms.Padding(2);
            this.pwTxt.MaxLength = 20;
            this.pwTxt.Name = "pwTxt";
            this.pwTxt.Size = new System.Drawing.Size(111, 21);
            this.pwTxt.TabIndex = 1;
            this.pwTxt.UseSystemPasswordChar = true;
            // 
            // capTxt
            // 
            this.capTxt.Location = new System.Drawing.Point(118, 68);
            this.capTxt.Margin = new System.Windows.Forms.Padding(2);
            this.capTxt.MaxLength = 4;
            this.capTxt.Name = "capTxt";
            this.capTxt.Size = new System.Drawing.Size(61, 21);
            this.capTxt.TabIndex = 2;
            // 
            // capImg
            // 
            this.capImg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.capImg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.capImg.Location = new System.Drawing.Point(190, 68);
            this.capImg.Margin = new System.Windows.Forms.Padding(2);
            this.capImg.Name = "capImg";
            this.capImg.Size = new System.Drawing.Size(39, 15);
            this.capImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.capImg.TabIndex = 6;
            this.capImg.TabStop = false;
            this.capImg.WaitOnLoad = true;
            this.capImg.Click += new System.EventHandler(this.capImgClick);
            // 
            // loginBtn
            // 
            this.loginBtn.Location = new System.Drawing.Point(61, 94);
            this.loginBtn.Margin = new System.Windows.Forms.Padding(2);
            this.loginBtn.Name = "loginBtn";
            this.loginBtn.Size = new System.Drawing.Size(56, 19);
            this.loginBtn.TabIndex = 3;
            this.loginBtn.Text = "Login";
            this.loginBtn.UseVisualStyleBackColor = true;
            this.loginBtn.Click += new System.EventHandler(this.loginBtnClick);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(145, 94);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(2);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(56, 19);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtnClick);
            // 
            // banButtonTimer
            // 
            this.banButtonTimer.Interval = 1000;
            this.banButtonTimer.Tick += new System.EventHandler(this.banButtonTimerTick);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(255, 126);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.loginBtn);
            this.Controls.Add(this.capImg);
            this.Controls.Add(this.capTxt);
            this.Controls.Add(this.pwTxt);
            this.Controls.Add(this.stuNumTxt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.Text = "JNUacademic";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Formclose);
            this.Load += new System.EventHandler(this.FormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.capImg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox stuNumTxt;
        private System.Windows.Forms.TextBox pwTxt;
        private System.Windows.Forms.TextBox capTxt;
        private System.Windows.Forms.PictureBox capImg;
        private System.Windows.Forms.Button loginBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Timer banButtonTimer;
    }
}


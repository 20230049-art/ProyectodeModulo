namespace Vista.Iniciar_Sesion
{
    partial class frmIniciarSesion
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
            this.pnlVistaIniciarSesion = new System.Windows.Forms.Panel();
            this.lnkOlvidaste = new System.Windows.Forms.Button();
            this.btnIniciarSesion = new System.Windows.Forms.Button();
            this.chkRecordarme = new System.Windows.Forms.CheckBox();
            this.txtContrasena = new System.Windows.Forms.TextBox();
            this.txtCorreo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlVistaIniciarSesion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlVistaIniciarSesion
            // 
            this.pnlVistaIniciarSesion.BackColor = System.Drawing.Color.White;
            this.pnlVistaIniciarSesion.Controls.Add(this.pictureBox1);
            this.pnlVistaIniciarSesion.Controls.Add(this.lnkOlvidaste);
            this.pnlVistaIniciarSesion.Controls.Add(this.btnIniciarSesion);
            this.pnlVistaIniciarSesion.Controls.Add(this.chkRecordarme);
            this.pnlVistaIniciarSesion.Controls.Add(this.txtContrasena);
            this.pnlVistaIniciarSesion.Controls.Add(this.txtCorreo);
            this.pnlVistaIniciarSesion.Controls.Add(this.label4);
            this.pnlVistaIniciarSesion.Controls.Add(this.label3);
            this.pnlVistaIniciarSesion.Controls.Add(this.label2);
            this.pnlVistaIniciarSesion.Controls.Add(this.label1);
            this.pnlVistaIniciarSesion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlVistaIniciarSesion.Location = new System.Drawing.Point(0, 0);
            this.pnlVistaIniciarSesion.Name = "pnlVistaIniciarSesion";
            this.pnlVistaIniciarSesion.Size = new System.Drawing.Size(1924, 1033);
            this.pnlVistaIniciarSesion.TabIndex = 0;
            // 
            // lnkOlvidaste
            // 
            this.lnkOlvidaste.Font = new System.Drawing.Font("High Tower Text", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkOlvidaste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(166)))), ((int)(((byte)(207)))));
            this.lnkOlvidaste.Location = new System.Drawing.Point(1203, 805);
            this.lnkOlvidaste.Name = "lnkOlvidaste";
            this.lnkOlvidaste.Size = new System.Drawing.Size(457, 56);
            this.lnkOlvidaste.TabIndex = 12;
            this.lnkOlvidaste.Text = "¿Olvidaste la contraseña?";
            this.lnkOlvidaste.UseVisualStyleBackColor = true;
            // 
            // btnIniciarSesion
            // 
            this.btnIniciarSesion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(166)))), ((int)(((byte)(207)))));
            this.btnIniciarSesion.FlatAppearance.BorderSize = 0;
            this.btnIniciarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIniciarSesion.Font = new System.Drawing.Font("High Tower Text", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIniciarSesion.ForeColor = System.Drawing.Color.White;
            this.btnIniciarSesion.Location = new System.Drawing.Point(1203, 743);
            this.btnIniciarSesion.Name = "btnIniciarSesion";
            this.btnIniciarSesion.Size = new System.Drawing.Size(457, 56);
            this.btnIniciarSesion.TabIndex = 11;
            this.btnIniciarSesion.Text = "Iniciar Sesión";
            this.btnIniciarSesion.UseVisualStyleBackColor = false;
            this.btnIniciarSesion.Click += new System.EventHandler(this.btnIniciarSesion_Click);
            // 
            // chkRecordarme
            // 
            this.chkRecordarme.AutoSize = true;
            this.chkRecordarme.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRecordarme.ForeColor = System.Drawing.Color.Gray;
            this.chkRecordarme.Location = new System.Drawing.Point(1129, 607);
            this.chkRecordarme.Name = "chkRecordarme";
            this.chkRecordarme.Size = new System.Drawing.Size(123, 24);
            this.chkRecordarme.TabIndex = 9;
            this.chkRecordarme.Text = "Recordarme";
            this.chkRecordarme.UseVisualStyleBackColor = true;
            // 
            // txtContrasena
            // 
            this.txtContrasena.Font = new System.Drawing.Font("Leelawadee UI", 16.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtContrasena.Location = new System.Drawing.Point(1129, 553);
            this.txtContrasena.Name = "txtContrasena";
            this.txtContrasena.PasswordChar = '●';
            this.txtContrasena.Size = new System.Drawing.Size(584, 45);
            this.txtContrasena.TabIndex = 8;
            this.txtContrasena.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtContrasena_KeyDown);
            // 
            // txtCorreo
            // 
            this.txtCorreo.Font = new System.Drawing.Font("Leelawadee UI", 16.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCorreo.Location = new System.Drawing.Point(1129, 410);
            this.txtCorreo.Name = "txtCorreo";
            this.txtCorreo.Size = new System.Drawing.Size(584, 45);
            this.txtCorreo.TabIndex = 7;
            this.txtCorreo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCorreo_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Javanese Text", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(11)))), ((int)(((byte)(67)))));
            this.label4.Location = new System.Drawing.Point(1117, 503);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(185, 69);
            this.label4.TabIndex = 6;
            this.label4.Text = "Contraseña";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Javanese Text", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(11)))), ((int)(((byte)(67)))));
            this.label3.Location = new System.Drawing.Point(1117, 359);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 69);
            this.label3.TabIndex = 5;
            this.label3.Text = "Usuario";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DarkGray;
            this.label2.Location = new System.Drawing.Point(1125, 215);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(377, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Ingresa tus credenciales para acceder al sistema";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Javanese Text", 40.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(11)))), ((int)(((byte)(67)))));
            this.label1.Location = new System.Drawing.Point(1103, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(465, 152);
            this.label1.TabIndex = 3;
            this.label1.Text = "Iniciar Sesión";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Vista.Properties.Resources.Captura_de_pantalla_2026_10_10_113754;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(896, 1033);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 13;
            this.pictureBox1.TabStop = false;
            // 
            // frmIniciarSesion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.ClientSize = new System.Drawing.Size(1924, 1033);
            this.Controls.Add(this.pnlVistaIniciarSesion);
            this.MaximumSize = new System.Drawing.Size(1942, 1080);
            this.MinimumSize = new System.Drawing.Size(1000, 500);
            this.Name = "frmIniciarSesion";
            this.Text = "frmIniciarSesion";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmIniciarSesion_Load);
            this.pnlVistaIniciarSesion.ResumeLayout(false);
            this.pnlVistaIniciarSesion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlVistaIniciarSesion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCorreo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnIniciarSesion;
        private System.Windows.Forms.CheckBox chkRecordarme;
        private System.Windows.Forms.TextBox txtContrasena;
        private System.Windows.Forms.Button lnkOlvidaste;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
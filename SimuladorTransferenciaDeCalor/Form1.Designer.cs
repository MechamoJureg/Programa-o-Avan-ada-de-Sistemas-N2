namespace SimuladorTransferenciaDeCalor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            TamanhoDaGrade = new NumericUpDown();
            boxMaterial = new ComboBox();
            LadoCubo = new NumericUpDown();
            TemperaturaInicial = new NumericUpDown();
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            painelGrade = new Panel();
            timerSimulacao = new System.Windows.Forms.Timer(components);
            BotaoIniciar = new Button();
            BotaoPausar = new Button();
            labelTempo = new Label();
            ((System.ComponentModel.ISupportInitialize)TamanhoDaGrade).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LadoCubo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TemperaturaInicial).BeginInit();
            SuspendLayout();
            // 
            // TamanhoDaGrade
            // 
            TamanhoDaGrade.AccessibleName = "";
            TamanhoDaGrade.BackColor = Color.White;
            TamanhoDaGrade.Location = new Point(215, 187);
            TamanhoDaGrade.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            TamanhoDaGrade.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            TamanhoDaGrade.Name = "TamanhoDaGrade";
            TamanhoDaGrade.Size = new Size(120, 23);
            TamanhoDaGrade.TabIndex = 0;
            TamanhoDaGrade.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // boxMaterial
            // 
            boxMaterial.FormattingEnabled = true;
            boxMaterial.Location = new Point(215, 216);
            boxMaterial.Name = "boxMaterial";
            boxMaterial.Size = new Size(121, 23);
            boxMaterial.TabIndex = 1;
            // 
            // LadoCubo
            // 
            LadoCubo.DecimalPlaces = 2;
            LadoCubo.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            LadoCubo.Location = new Point(215, 245);
            LadoCubo.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            LadoCubo.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            LadoCubo.Name = "LadoCubo";
            LadoCubo.Size = new Size(120, 23);
            LadoCubo.TabIndex = 2;
            LadoCubo.Value = new decimal(new int[] { 1, 0, 0, 65536 });
            // 
            // TemperaturaInicial
            // 
            TemperaturaInicial.Location = new Point(215, 274);
            TemperaturaInicial.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            TemperaturaInicial.Minimum = new decimal(new int[] { 27315, 0, 0, -2147352576 });
            TemperaturaInicial.Name = "TemperaturaInicial";
            TemperaturaInicial.Size = new Size(120, 23);
            TemperaturaInicial.TabIndex = 3;
            TemperaturaInicial.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // button1
            // 
            button1.Location = new Point(245, 320);
            button1.Name = "button1";
            button1.Size = new Size(75, 28);
            button1.TabIndex = 4;
            button1.Text = "Criar Grade";
            button1.UseVisualStyleBackColor = true;
            button1.Click += BotaoCriarGrade;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.ControlLight;
            label1.Location = new Point(82, 187);
            label1.Name = "label1";
            label1.Size = new Size(111, 15);
            label1.TabIndex = 5;
            label1.Text = "Tamanho Da Grade:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = SystemColors.ControlLight;
            label2.Location = new Point(140, 219);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 6;
            label2.Text = "Material:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = SystemColors.ControlLight;
            label3.Location = new Point(85, 247);
            label3.Name = "label3";
            label3.Size = new Size(108, 15);
            label3.TabIndex = 7;
            label3.Text = "Lado Do Cubo (m):";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = SystemColors.ControlLight;
            label4.Location = new Point(95, 276);
            label4.Name = "label4";
            label4.Size = new Size(98, 15);
            label4.TabIndex = 8;
            label4.Text = "Temperatura (ºC)";
            // 
            // painelGrade
            // 
            painelGrade.AutoScroll = true;
            painelGrade.Location = new Point(577, 150);
            painelGrade.Name = "painelGrade";
            painelGrade.Size = new Size(500, 400);
            painelGrade.TabIndex = 9;
            // 
            // timerSimulacao
            // 
            timerSimulacao.Tick += timerSimulacao_Tick;
            // 
            // BotaoIniciar
            // 
            BotaoIniciar.Location = new Point(106, 476);
            BotaoIniciar.Margin = new Padding(2);
            BotaoIniciar.Name = "BotaoIniciar";
            BotaoIniciar.Size = new Size(71, 23);
            BotaoIniciar.TabIndex = 10;
            BotaoIniciar.Text = "Iniciar Sim";
            BotaoIniciar.UseVisualStyleBackColor = true;
            BotaoIniciar.Click += BotaoIniciar_Click;
            // 
            // BotaoPausar
            // 
            BotaoPausar.Location = new Point(232, 476);
            BotaoPausar.Margin = new Padding(2);
            BotaoPausar.Name = "BotaoPausar";
            BotaoPausar.Size = new Size(70, 23);
            BotaoPausar.TabIndex = 11;
            BotaoPausar.Text = "Pausar Sim";
            BotaoPausar.UseVisualStyleBackColor = true;
            BotaoPausar.Click += BotaoPausar_Click;
            // 
            // labelTempo
            // 
            labelTempo.AutoSize = true;
            labelTempo.Location = new Point(577, 107);
            labelTempo.Name = "labelTempo";
            labelTempo.Size = new Size(101, 15);
            labelTempo.TabIndex = 12;
            labelTempo.Text = "Tempo decorrido:";
            // 
            // Form1
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1149, 548);
            Controls.Add(labelTempo);
            Controls.Add(BotaoPausar);
            Controls.Add(BotaoIniciar);
            Controls.Add(painelGrade);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(TemperaturaInicial);
            Controls.Add(LadoCubo);
            Controls.Add(boxMaterial);
            Controls.Add(TamanhoDaGrade);
            ForeColor = SystemColors.ActiveCaptionText;
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)TamanhoDaGrade).EndInit();
            ((System.ComponentModel.ISupportInitialize)LadoCubo).EndInit();
            ((System.ComponentModel.ISupportInitialize)TemperaturaInicial).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown TamanhoDaGrade;
        private ComboBox boxMaterial;
        private NumericUpDown LadoCubo;
        private NumericUpDown TemperaturaInicial;
        private Button button1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Panel painelGrade;
        private System.Windows.Forms.Timer timerSimulacao;
        private Button BotaoIniciar;
        private Button BotaoPausar;
        private Label labelTempo;
    }
}

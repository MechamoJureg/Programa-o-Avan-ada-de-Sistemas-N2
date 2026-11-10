namespace SimuladorTransferenciaDeCalor
{
    partial class AlterarMaterialForm
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
            boxNovoMaterial = new ComboBox();
            NovoMaterial = new Label();
            Alterar = new Button();
            Concluir = new Button();
            label1 = new Label();
            TemperaturaCorpo = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)TemperaturaCorpo).BeginInit();
            SuspendLayout();
            // 
            // boxNovoMaterial
            // 
            boxNovoMaterial.FormattingEnabled = true;
            boxNovoMaterial.Location = new Point(371, 267);
            boxNovoMaterial.Margin = new Padding(4, 5, 4, 5);
            boxNovoMaterial.Name = "boxNovoMaterial";
            boxNovoMaterial.Size = new Size(280, 33);
            boxNovoMaterial.TabIndex = 0;
            // 
            // NovoMaterial
            // 
            NovoMaterial.AutoSize = true;
            NovoMaterial.BackColor = SystemColors.ControlLight;
            NovoMaterial.Location = new Point(220, 272);
            NovoMaterial.Margin = new Padding(4, 0, 4, 0);
            NovoMaterial.Name = "NovoMaterial";
            NovoMaterial.Size = new Size(124, 25);
            NovoMaterial.TabIndex = 1;
            NovoMaterial.Text = "Novo Material";
            // 
            // Alterar
            // 
            Alterar.Location = new Point(371, 408);
            Alterar.Margin = new Padding(4, 5, 4, 5);
            Alterar.Name = "Alterar";
            Alterar.Size = new Size(281, 38);
            Alterar.TabIndex = 2;
            Alterar.Text = "Alterar Material";
            Alterar.UseVisualStyleBackColor = true;
            Alterar.Click += Alterar_Click;
            // 
            // Concluir
            // 
            Concluir.Location = new Point(771, 408);
            Concluir.Margin = new Padding(4, 5, 4, 5);
            Concluir.Name = "Concluir";
            Concluir.Size = new Size(107, 38);
            Concluir.TabIndex = 3;
            Concluir.Text = "Concluir";
            Concluir.UseVisualStyleBackColor = true;
            Concluir.Click += Concluir_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.ControlLight;
            label1.Location = new Point(210, 513);
            label1.Name = "label1";
            label1.Size = new Size(144, 25);
            label1.TabIndex = 4;
            label1.Text = "Temperatura (ºC)";
            // 
            // TemperaturaCorpo
            // 
            TemperaturaCorpo.DecimalPlaces = 2;
            TemperaturaCorpo.Location = new Point(371, 511);
            TemperaturaCorpo.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            TemperaturaCorpo.Minimum = new decimal(new int[] { 27315, 0, 0, -2147352576 });
            TemperaturaCorpo.Name = "TemperaturaCorpo";
            TemperaturaCorpo.Size = new Size(280, 31);
            TemperaturaCorpo.TabIndex = 5;
            // 
            // AlterarMaterialForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1530, 964);
            Controls.Add(TemperaturaCorpo);
            Controls.Add(label1);
            Controls.Add(Concluir);
            Controls.Add(Alterar);
            Controls.Add(NovoMaterial);
            Controls.Add(boxNovoMaterial);
            Margin = new Padding(4, 5, 4, 5);
            Name = "AlterarMaterialForm";
            Text = "AlterarMaterialForm";
            ((System.ComponentModel.ISupportInitialize)TemperaturaCorpo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox boxNovoMaterial;
        private Label NovoMaterial;
        private Button Alterar;
        private Button Concluir;
        private Label label1;
        private NumericUpDown TemperaturaCorpo;
    }
}
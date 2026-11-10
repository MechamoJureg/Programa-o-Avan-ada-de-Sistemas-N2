namespace SimuladorTransferenciaDeCalor
{
    partial class InformacoesCorpoForm
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
            informacoesCorpo = new Label();
            AlterarMaterial = new Button();
            Fechar = new Button();
            SuspendLayout();
            // 
            // informacoesCorpo
            // 
            informacoesCorpo.AutoSize = true;
            informacoesCorpo.BackColor = SystemColors.ControlLight;
            informacoesCorpo.Location = new Point(330, 80);
            informacoesCorpo.Name = "informacoesCorpo";
            informacoesCorpo.Size = new Size(127, 15);
            informacoesCorpo.TabIndex = 0;
            informacoesCorpo.Text = "Informações do corpo:";
            // 
            // AlterarMaterial
            // 
            AlterarMaterial.Location = new Point(230, 260);
            AlterarMaterial.Name = "AlterarMaterial";
            AlterarMaterial.Size = new Size(127, 23);
            AlterarMaterial.TabIndex = 1;
            AlterarMaterial.Text = "Alterar Material";
            AlterarMaterial.UseVisualStyleBackColor = true;
            AlterarMaterial.Click += AlterarMaterial_Click;
            // 
            // Fechar
            // 
            Fechar.Location = new Point(430, 260);
            Fechar.Name = "Fechar";
            Fechar.Size = new Size(145, 23);
            Fechar.TabIndex = 2;
            Fechar.Text = "Fechar";
            Fechar.UseVisualStyleBackColor = true;
            Fechar.Click += Fechar_Click;
            // 
            // InformacoesCorpoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Fechar);
            Controls.Add(AlterarMaterial);
            Controls.Add(informacoesCorpo);
            Name = "InformacoesCorpoForm";
            Text = "InformacoesCorpoForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label informacoesCorpo;
        private Button AlterarMaterial;
        private Button Fechar;
    }
}
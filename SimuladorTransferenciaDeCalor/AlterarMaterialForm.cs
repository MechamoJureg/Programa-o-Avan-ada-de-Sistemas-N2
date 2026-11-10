using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor
{
    public partial class AlterarMaterialForm : Form
    {
        private Corpo corpo;
        private ListaDuplaEncade<Material> materiais;
        public AlterarMaterialForm(Corpo corpo, ListaDuplaEncade<Material> materiais)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.corpo = corpo;
            this.materiais = materiais;

            TemperaturaCorpo.Value = (decimal)corpo.GetTemp();

            for (int i = 0; i < materiais.GetQuantidade(); i++)
            {
                boxNovoMaterial.Items.Add(materiais.ObterViaProx(i));
            }
        }

        private void Alterar_Click(object sender, EventArgs e)
        {
            if (boxNovoMaterial.SelectedItem == null)
            {
                MessageBox.Show("Selecione um material.");
                return;
            }

            Material novoMaterial = (Material)boxNovoMaterial.SelectedItem;
            corpo.SetMaterial(novoMaterial);
            MessageBox.Show("Material alterado para: " + corpo.GetMaterial().GetNome());
        }

        private void Concluir_Click(object sender, EventArgs e)
        {
            corpo.SetTemp((double)TemperaturaCorpo.Value);
            this.Close();
        }
    }
}

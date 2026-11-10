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
    public partial class InformacoesCorpoForm : Form
    {
        private Corpo corpo;
        private ListaDuplaEncade<Material> materiais;
        public InformacoesCorpoForm(Corpo corpo, ListaDuplaEncade<Material> materiais)
        {
            InitializeComponent();

            this.corpo = corpo;
            this.materiais = materiais;
            
            MostrarInformacoes();
        }

        private void MostrarInformacoes()
        {
            informacoesCorpo.Text = "Material: " + corpo.GetMaterial().GetNome() +
                                    "\nTemperatura: " + corpo.GetTemp() + " °C" +
                                    "\nLado: " + corpo.GetLado() +
                                    "\nVolume: " + corpo.GetVolume() +
                                    "\nMassa: " + corpo.GetMassa();
        }
        private void AlterarMaterial_Click(object sender, EventArgs e)
        {
            AlterarMaterialForm janela = new AlterarMaterialForm(corpo, materiais);
            janela.ShowDialog(this);
            MostrarInformacoes();

        }

        private void Fechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

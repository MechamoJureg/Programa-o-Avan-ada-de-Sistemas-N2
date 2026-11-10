using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimuladorTransferenciaDeCalor
{
    // Painel que desenha o próprio fundo como um degradê.
    // O centro tem a cor da temperatura do corpo; cada borda tem a cor intermediária entre este corpo
    // e o vizinho daquele lado. Assim, quadrados vizinhos compartilham a mesma cor na borda de contato
    // e a grade inteira parece uma superfície contínua de calor.
    public class CelulaGradiente : Panel
    {
        private Color corEsquerda = Color.Gray;
        private Color corCentro = Color.Gray;
        private Color corDireita = Color.Gray;
        private Color corCima = Color.Gray;
        private Color corBaixo = Color.Gray;

        public CelulaGradiente()
        {
            // Evita o "piscar" ao redesenhar a cada tick da simulação.
            DoubleBuffered = true;
            // Redesenha o degradê se o tamanho mudar (por exemplo, ao mudar a escala de DPI).
            ResizeRedraw = true;
        }

        // Define as cinco cores do degradê e pede para o controle ser redesenhado.
        public void DefinirCores(Color esquerda, Color centro, Color direita, Color cima, Color baixo)
        {
            corEsquerda = esquerda;
            corCentro = centro;
            corDireita = direita;
            corCima = cima;
            corBaixo = baixo;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle area = ClientRectangle;
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            // Camada 1: degradê horizontal (esquerda -> centro -> direita).
            // O ColorBlend permite um ponto intermediário: o centro fica com a cor do próprio corpo.
            using (LinearGradientBrush horizontal = new LinearGradientBrush(area, corEsquerda, corDireita, LinearGradientMode.Horizontal))
            {
                ColorBlend mistura = new ColorBlend(3);
                mistura.Colors = new Color[] { corEsquerda, corCentro, corDireita };
                mistura.Positions = new float[] { 0f, 0.5f, 1f };
                horizontal.InterpolationColors = mistura;

                e.Graphics.FillRectangle(horizontal, area);
            }

            // Camada 2: degradê vertical (cima -> centro -> baixo) SEMI-TRANSPARENTE por cima da primeira.
            // O centro tem alpha 0 (totalmente transparente), então a cor do corpo se mantém no meio;
            // nas bordas de cima e de baixo, a cor do vizinho aparece com alpha 160 (cerca de 63%).
            using (LinearGradientBrush vertical = new LinearGradientBrush(area, corCima, corBaixo, LinearGradientMode.Vertical))
            {
                ColorBlend mistura = new ColorBlend(3);
                mistura.Colors = new Color[]
                {
                    Color.FromArgb(160, corCima),
                    Color.FromArgb(0, corCentro),
                    Color.FromArgb(160, corBaixo)
                };
                mistura.Positions = new float[] { 0f, 0.5f, 1f };
                vertical.InterpolationColors = mistura;

                e.Graphics.FillRectangle(vertical, area);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor
{
    public class Grade
    {
        private Corpo[,] corpos;

        public Grade(int tamanho)
        {
            corpos = new Corpo[tamanho, tamanho];
        }
        public int GetTamanho()
        {
            return corpos.GetLength(0);
        }

        public Corpo GetCorpo(int linha, int coluna)
        {
            return corpos[linha, coluna];
        }
        public void SetCorpo(int linha, int coluna, Corpo corpo)
        {
            corpos[linha, coluna] = corpo;
        }
    }

}
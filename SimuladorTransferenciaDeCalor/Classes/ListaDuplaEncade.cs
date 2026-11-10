using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor.Classes
{
    public class ListaDuplaEncade<T>
    {
        private No<T> Inicio;
        private No<T> Fim;
        private int Quantidade;

        public ListaDuplaEncade()
        {
            Inicio = null!;
            Fim = null!;
            Quantidade = 0;
        }

        public int GetQuantidade()
        {
            return Quantidade;
        }

        public void Adicionar(T objeto)
        {
            No<T> novo = new No<T>(objeto);
            if (Inicio == null)
            {
                Inicio = novo;
                Fim = novo;
            }
            else
            {
                novo.Ante = Fim;
                Fim.Prox = novo;
                Fim = novo;
            }
            Quantidade++;
        }

        public T ObterViaProx(int indice)
        {
            if (indice < 0 || indice >= Quantidade)
            {
                throw new IndexOutOfRangeException("Índice inválido");
            }
            No<T> atual = Inicio;
            int i = 0;
            for (i = 0; i < indice; i++)
            {
                atual = atual.Prox;
            }
            return atual.Dado;
        }

        public T ObterFim()
        {
            if (Quantidade == 0)
            {
                throw new Exception("Lista vazia");
            }
            return Fim.Dado;
        }

        public T ObterViaAnte(int indice)
        {
            if (indice < 0 || indice >= Quantidade)
            {
                throw new IndexOutOfRangeException("Índice inválido");
            }
            No<T> atual = Fim;
            int i = 0;
            for (i = 0; i < indice; i++)
            {
                atual = atual.Ante;
            }
            return atual.Dado;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor.Classes
{
    public class No<T>
    {
        public T Dado;
        public No<T> Prox;
        public No<T> Ante;

        public No(T valor)
        {
            Dado=valor;
            Prox = null!;
            Ante=null!;
        }
    }
}

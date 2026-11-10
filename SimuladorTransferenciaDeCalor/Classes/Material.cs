using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor
{
    public class Material
    {
        protected string Nome;
        protected double Densidade;
        protected double CalorEspecifico;
        protected double Condutividade;

        public Material(string Nome, double Densidade, double CalorEspecifico, double Condutividade)
        {
            this.Nome = Nome;
            this.Densidade = Densidade;
            this.CalorEspecifico = CalorEspecifico;
            this.Condutividade = Condutividade;
        }

        public override string ToString()
        {
            return Nome;  //Permite que o ComboBox mostre o nome correto do objeto material
                          //Mesmo selecionando todo o objeto.
        }

        public string GetNome()
        {
            return Nome;
        }
        public void SetNome(string nome)
        {
            Nome = nome;
        }

        public double GetDensidade()
        {
            return Densidade;  //Permite Classe Corpo acessar Densidade sem deixar variavel publica
        }

        public void SetDensidade(double valor)
        {
            Densidade = valor;
        }

        public double GetCalor()
        {
            return CalorEspecifico;
        }
        public void SetCalor(double valor)
        {
            CalorEspecifico = valor;
        }

        public double GetCondutividade()
        {
            return Condutividade;
        }
        public void SetCondutividade(double valor)
        {
            Condutividade = valor;
        }
    }

}
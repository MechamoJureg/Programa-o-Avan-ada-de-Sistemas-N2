using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor
{
    public class Corpo
    {
        protected Material material;
        protected double Lado;
        protected double Temperatura;
        protected double Volume;
        protected double Massa;

        public Corpo(Material material, double Lado, double Temperatura)
        {
            this.material = material;
            this.Lado = Lado;
            this.Temperatura = Temperatura;
            this.Volume = Lado * Lado * Lado;
            this.Massa = material.GetDensidade() * this.Volume;
        }

        public Material GetMaterial()
        {
            return material;
        }
        public void SetMaterial(Material material)
        {
            this.material = material;
            this.Massa = material.GetDensidade() * this.Volume;  //Ao modificar o Material recalcula a massa do corpo de acordo com a nova densidade.
        }

        public double GetLado()
        {
            return this.Lado;
        }
        public double GetTemp()
        {
            return Temperatura;
        }
        public void SetTemp(double Temperatura)
        {
            this.Temperatura = Temperatura;
        }

        public double GetVolume()
        {
            return Volume;
        }
        public double GetMassa()
        {
            return Massa;
        }
    }
}
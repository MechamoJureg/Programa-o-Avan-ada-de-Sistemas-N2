using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SimuladorTransferenciaDeCalor
{
    public class Simulador
    {
        private Grade grade;
        public Simulador(Grade grade)
        {
            this.grade = grade;
        }

        private double CalculoTranferenciaTemp(Corpo corpo1, Corpo corpo2)
        {
            return corpo1.GetTemp() - corpo2.GetTemp();
        }

        private double CalculoTransferenciaEnergia(Corpo corpo1, Corpo corpo2, double deltaTempo)
        {
            double DiferencaTemp = CalculoTranferenciaTemp(corpo1, corpo2);

            double k1 = corpo1.GetMaterial().GetCondutividade();
            double k2 = corpo2.GetMaterial().GetCondutividade();

            double k = Math.Min(k1, k2); //Retorna a menor condutividade

            double lado = corpo1.GetLado();
            double area = lado * lado;
            double energia = k * area * DiferencaTemp * deltaTempo;

            return energia;
        }


        private double CalcularVariacaoTemp(Corpo corpo, double energia)
        {
            double massa = corpo.GetMassa();
            double calorEspecifico = corpo.GetMaterial().GetCalor();

            double variacaoTemp = energia / (massa * calorEspecifico);

            return variacaoTemp;
        }

        private void TransferirCalor(Corpo corpo1, Corpo corpo2, double deltaTempo)
        {
            double energia = CalculoTransferenciaEnergia(corpo1, corpo2, deltaTempo);

            double variacao1 = CalcularVariacaoTemp(corpo1, -energia);   //Se o primeiro corpo estiver mais quente, a energia será positiva.
                                                                         //Ele perde calor, então sua variação de temperatura será negativa.

            double variacao2 = CalcularVariacaoTemp(corpo2, energia);  //O segundo corpo recebe essa energia, então sua variação será positiva.
                                                                       //Se o segundo corpo estiver mais quente, a energia será negativa.
                                                                       //Os sinais se invertem automaticamente.
            corpo1.SetTemp(corpo1.GetTemp() + variacao1);
            corpo2.SetTemp(corpo2.GetTemp() + variacao2);
        }

        public void SimularPasso(double deltaTempo)
        {
            int tamanho = grade.GetTamanho();

            double[,] energiaAcumulada = new double[tamanho, tamanho];

            //Calcula as transferências de energia
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpoAtual = grade.GetCorpo(linha, coluna);

                    // Verifica o vizinho da direita
                    if (coluna + 1 < tamanho)
                    {
                        Corpo vizinho = grade.GetCorpo(linha, coluna + 1);

                        double energia = CalculoTransferenciaEnergia(corpoAtual, vizinho, deltaTempo);

                        energiaAcumulada[linha, coluna] -= energia;
                        energiaAcumulada[linha, coluna + 1] += energia;
                    }
                    // Verifica o vizinho de baixo
                    if (linha + 1 < tamanho)
                    {
                        Corpo vizinho = grade.GetCorpo(linha + 1, coluna);

                        double energia = CalculoTransferenciaEnergia(corpoAtual, vizinho, deltaTempo);

                        energiaAcumulada[linha, coluna] -= energia;
                        energiaAcumulada[linha + 1, coluna] += energia;
                    }
                }
            }
            //Atualiza todas as temperaturas
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpo = grade.GetCorpo(linha, coluna);

                    double variacao = CalcularVariacaoTemp(
                        corpo,
                        energiaAcumulada[linha, coluna]
                    );

                    corpo.SetTemp(corpo.GetTemp() + variacao);
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimuladorTransferenciaDeCalor
{
    public class Simulador
    {
        private Grade grade;

        // Matriz de saldo de energia de cada célula no passo atual.
        // É um campo (e não uma variável local) para ser criada UMA vez e reaproveitada a cada passo,
        // evitando alocar N x N doubles por passo e pressionar o coletor de lixo.
        private double[,] energiaAcumulada;

        // Número máximo de threads usadas pelo Parallel.For.
        // -1 significa "deixa o .NET decidir". Usado no benchmark para testar 1, 2, 4, 8... threads.
        public int GrauParalelismo { get; set; } = -1;

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

            // Atenção: a área usa o lado do corpo1. Hoje todos os corpos da grade têm o mesmo lado,
            // então a ordem dos argumentos não altera o resultado. Se os lados pudessem diferir,
            // a fórmula deixaria de ser simétrica e a versão paralela divergiria da sequencial.
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

        // ============================================================
        // VERSÃO SEQUENCIAL (N1) - mantida como referência para o
        // teste de equivalência e para o benchmark.
        // ============================================================
        public void SimularPassoSequencial(double deltaTempo)
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

        // ============================================================
        // VERSÃO PARALELA (N2)
        // ============================================================

        // Calcula o saldo de energia de UMA célula neste passo (estratégia "puxar").
        // A célula olha seus vizinhos existentes (cima, baixo, esquerda, direita) apenas para LER
        // as temperaturas; nunca chama SetTemp. Por isso este método é seguro para rodar em várias threads.
        // Sinal: o vizinho entra como corpo1 e a célula atual como corpo2. Se o vizinho estiver mais quente,
        // o resultado é positivo (a atual ganha energia); se estiver mais frio, é negativo (a atual perde).
        private double CalcularSaldoEnergia(int linha, int coluna, double deltaTempo)
        {
            int tamanho = grade.GetTamanho();
            Corpo atual = grade.GetCorpo(linha, coluna);
            double saldo = 0;

            // Vizinho de cima: só existe se não estamos na primeira linha
            if (linha > 0)
            {
                Corpo vizinho = grade.GetCorpo(linha - 1, coluna);
                saldo += CalculoTransferenciaEnergia(vizinho, atual, deltaTempo);
            }

            // Vizinho de baixo: só existe se não estamos na última linha
            if (linha < tamanho - 1)
            {
                Corpo vizinho = grade.GetCorpo(linha + 1, coluna);
                saldo += CalculoTransferenciaEnergia(vizinho, atual, deltaTempo);
            }

            // Vizinho da esquerda: só existe se não estamos na primeira coluna
            if (coluna > 0)
            {
                Corpo vizinho = grade.GetCorpo(linha, coluna - 1);
                saldo += CalculoTransferenciaEnergia(vizinho, atual, deltaTempo);
            }

            // Vizinho da direita: só existe se não estamos na última coluna
            if (coluna < tamanho - 1)
            {
                Corpo vizinho = grade.GetCorpo(linha, coluna + 1);
                saldo += CalculoTransferenciaEnergia(vizinho, atual, deltaTempo);
            }

            return saldo;
        }

        public void SimularPassoParalelo(double deltaTempo)
        {
            int tamanho = grade.GetTamanho();

            // Cria a matriz de saldos só na primeira vez (ou se o tamanho da grade mudou).
            // Não precisa zerar: a fase 1 SOBRESCREVE cada posição (atribuição, não +=).
            if (energiaAcumulada == null || energiaAcumulada.GetLength(0) != tamanho)
            {
                energiaAcumulada = new double[tamanho, tamanho];
            }

            // Cópia local da referência: a lambda abaixo usa a variável local em vez do campo.
            double[,] saldos = energiaAcumulada;

            ParallelOptions opcoes = new ParallelOptions();
            opcoes.MaxDegreeOfParallelism = GrauParalelismo;

            // FASE 1: calcula o saldo de energia de cada célula.
            // Paralelizamos por LINHA (e não por célula) para reduzir o overhead de agendamento:
            // cada thread recebe blocos de linhas inteiras. Cada iteração escreve SOMENTE em
            // saldos[linha, coluna] (a própria posição) e apenas LÊ as temperaturas dos vizinhos,
            // que ninguém altera nesta fase. Logo não há condição de corrida e não precisa de lock.
            Parallel.For(0, tamanho, opcoes, linha =>
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    saldos[linha, coluna] = CalcularSaldoEnergia(linha, coluna, deltaTempo);
                }
            });
            // O Parallel.For só retorna quando TODAS as iterações terminam: isso funciona como uma
            // barreira, garantindo que a fase 2 só comece com todos os saldos já calculados
            // (senão uma célula poderia ter a temperatura alterada enquanto outra ainda a lê).

            // FASE 2: atualiza as temperaturas.
            // Cada célula lê apenas o próprio saldo e escreve apenas na própria temperatura,
            // então as linhas também são independentes entre si.
            Parallel.For(0, tamanho, opcoes, linha =>
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpo = grade.GetCorpo(linha, coluna);
                    double variacao = CalcularVariacaoTemp(corpo, saldos[linha, coluna]);
                    corpo.SetTemp(corpo.GetTemp() + variacao);
                }
            });
        }
    }
}

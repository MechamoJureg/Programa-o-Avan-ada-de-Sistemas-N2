using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SimuladorTransferenciaDeCalor
{
    // Mede o tempo de CÁLCULO da simulação (sem interface e sem banco de dados), comparando:
    //  - a versão sequencial (algoritmo da N1, mantido em SimularPassoSequencial); e
    //  - a versão paralela (N2) com 1, 2, 4, 8 e 16 threads.
    // Gera um arquivo CSV na Área de Trabalho para montar as tabelas e gráficos do relatório.
    // Execução (na pasta raiz do repositório, de preferência em Release):
    //   dotnet run -c Release --project SimuladorTransferenciaDeCalor -- --benchmark
    public static class Benchmark
    {
        // Tamanhos de grade testados e quantos passos simular em cada um.
        // Grades maiores usam menos passos para o teste todo terminar em poucos minutos.
        private static readonly int[] Tamanhos = { 50, 100, 200, 400, 800 };
        private static readonly int[] Passos = { 400, 200, 100, 50, 20 };

        // Graus de paralelismo testados (número máximo de threads do Parallel.For).
        private static readonly int[] Threads = { 1, 2, 4, 8, 16 };

        // Cada medição é repetida e usamos a média, para reduzir o efeito de oscilações do sistema.
        private const int Repeticoes = 3;

        // Mesmo valor de deltaTempo usado pela tela principal (0,05 s * 50).
        private const double DeltaTempo = 2.5;

        // Culture pt-BR: o CSV usa vírgula decimal e ponto e vírgula como separador, para abrir direto no Excel brasileiro.
        private static readonly CultureInfo Cultura = new CultureInfo("pt-BR");

        // Monta uma grade com materiais variados (para exercitar a regra do Math.Min das condutividades)
        // e o ponto quente de 100 °C em [0,0], como na tela principal.
        private static Grade CriarGrade(int tamanho)
        {
            Material[] materiais =
            {
                new Material("Alumínio", 2700, 900, 237),
                new Material("Aço", 8060, 480, 15.1),
                new Material("Cobre", 8930, 385, 401),
                new Material("Madeira", 610, 2386, 0.15),
                new Material("Vidro", 2500, 750, 1.4)
            };

            Grade grade = new Grade(tamanho);
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Material material = materiais[(linha + coluna) % materiais.Length];
                    grade.SetCorpo(linha, coluna, new Corpo(material, 0.1, 25));
                }
            }

            grade.GetCorpo(0, 0).SetTemp(100);
            return grade;
        }

        // Roda a simulação inteira e devolve o tempo gasto em milissegundos.
        // A criação da grade fica FORA do cronômetro: só o cálculo dos passos é medido.
        private static double Medir(int tamanho, int passos, bool paralelo, int threads)
        {
            Grade grade = CriarGrade(tamanho);
            Simulador simulador = new Simulador(grade);
            simulador.GrauParalelismo = threads;

            Stopwatch cronometro = Stopwatch.StartNew();
            for (int i = 0; i < passos; i++)
            {
                if (paralelo)
                {
                    simulador.SimularPassoParalelo(DeltaTempo);
                }
                else
                {
                    simulador.SimularPassoSequencial(DeltaTempo);
                }
            }
            cronometro.Stop();

            return cronometro.Elapsed.TotalMilliseconds;
        }

        // Média de várias repetições da mesma configuração.
        private static double MedirMedia(int tamanho, int passos, bool paralelo, int threads)
        {
            double soma = 0;
            for (int r = 0; r < Repeticoes; r++)
            {
                soma += Medir(tamanho, passos, paralelo, threads);
            }
            return soma / Repeticoes;
        }

        public static void Executar()
        {
            MessageBox.Show(
                "O benchmark vai começar e leva cerca de 1 minuto.\n" +
                "Feche outros programas pesados e não mexa no computador até aparecer a mensagem final.\n\n" +
                "Clique em OK para iniciar.",
                "Benchmark");

            // Aquecimento: a primeira execução paga o custo de compilação JIT e de criação das threads.
            // Rodamos uma vez, sem anotar o tempo, para não prejudicar a primeira medição real.
            Medir(50, 20, false, 1);
            Medir(50, 20, true, Environment.ProcessorCount);

            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Tamanho;Passos;Versao;Threads;TempoMedioMs;Speedup");

            StringBuilder resumo = new StringBuilder();
            resumo.AppendLine("Processador: " + Environment.ProcessorCount + " threads lógicas");
            resumo.AppendLine();

            for (int t = 0; t < Tamanhos.Length; t++)
            {
                int tamanho = Tamanhos[t];
                int passos = Passos[t];

                // Referência: versão sequencial (algoritmo da N1)
                double tempoSequencial = MedirMedia(tamanho, passos, false, 1);
                csv.AppendLine(string.Join(";",
                    tamanho, passos, "sequencial", 1,
                    tempoSequencial.ToString("F2", Cultura),
                    (1.0).ToString("F2", Cultura)));

                double melhorSpeedup = 0;
                int melhorThreads = 1;

                foreach (int threads in Threads)
                {
                    double tempoParalelo = MedirMedia(tamanho, passos, true, threads);

                    // Speedup = tempo sequencial / tempo paralelo. Acima de 1 significa mais rápido.
                    double speedup = tempoSequencial / tempoParalelo;

                    csv.AppendLine(string.Join(";",
                        tamanho, passos, "paralela", threads,
                        tempoParalelo.ToString("F2", Cultura),
                        speedup.ToString("F2", Cultura)));

                    if (speedup > melhorSpeedup)
                    {
                        melhorSpeedup = speedup;
                        melhorThreads = threads;
                    }
                }

                resumo.AppendLine(tamanho + "x" + tamanho + " (" + passos + " passos): sequencial " +
                                  tempoSequencial.ToString("F0", Cultura) + " ms | melhor paralelo " +
                                  melhorSpeedup.ToString("F2", Cultura) + "x com " + melhorThreads + " threads");
            }

            string caminho = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "resultados-benchmark.csv");
            File.WriteAllText(caminho, csv.ToString(), Encoding.UTF8);

            resumo.AppendLine();
            resumo.AppendLine("Arquivo salvo em:");
            resumo.AppendLine(caminho);

            MessageBox.Show(resumo.ToString(), "Benchmark concluído");
        }
    }
}

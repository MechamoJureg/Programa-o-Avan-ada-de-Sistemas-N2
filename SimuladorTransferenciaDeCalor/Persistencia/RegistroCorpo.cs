using System;

namespace SimuladorTransferenciaDeCalor.Persistencia
{
    // "Retrato" de um corpo em um instante da simulação.
    // É uma cópia dos valores (e não uma referência ao objeto Corpo), por dois motivos:
    //  1) A gravação pode rodar em outra thread sem ler objetos que o cálculo está alterando.
    //  2) Quem implementa o banco de dados não precisa conhecer as classes Grade/Corpo/Material.
    // É um struct imutável (readonly) para gerar menos lixo na memória: uma grade 500x500 produz
    // 250 mil registros por iteração, e structs em um vetor ficam contíguos, sem um objeto por registro.
    public readonly struct RegistroCorpo
    {
        public int Linha { get; }
        public int Coluna { get; }
        public string NomeMaterial { get; }   // Identifica o material (chave na tabela de materiais)
        public double Lado { get; }           // Massa e volume são derivados do lado e da densidade
        public double Temperatura { get; }    // Em °C

        public RegistroCorpo(int linha, int coluna, string nomeMaterial, double lado, double temperatura)
        {
            Linha = linha;
            Coluna = coluna;
            NomeMaterial = nomeMaterial;
            Lado = lado;
            Temperatura = temperatura;
        }

        // Tira uma "foto" de todos os corpos da grade, em ordem linha a linha.
        // O vetor devolvido é novo a cada chamada: quem recebê-lo pode guardá-lo ou
        // enfileirá-lo para gravar depois, sem risco de ele mudar durante a próxima iteração.
        public static RegistroCorpo[] CapturarDe(Grade grade)
        {
            int tamanho = grade.GetTamanho();
            RegistroCorpo[] registros = new RegistroCorpo[tamanho * tamanho];

            int posicao = 0;
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpo = grade.GetCorpo(linha, coluna);

                    registros[posicao] = new RegistroCorpo(
                        linha,
                        coluna,
                        corpo.GetMaterial().GetNome(),   // O material pode ser trocado durante a simulação, por isso é gravado a cada iteração
                        corpo.GetLado(),
                        corpo.GetTemp());

                    posicao++;
                }
            }

            return registros;
        }
    }
}

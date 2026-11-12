using System;
using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor
{
    // Verifica que a versão paralela produz o mesmo resultado da sequencial.
    public static class TesteEquivalencia
    {
        // Monta uma grade determinística com materiais MISTURADOS (para exercitar a regra do
        // Math.Min das condutividades) e um ponto quente em [0,0], como na tela principal.
        private static Grade CriarGrade(int tamanho, double lado, ListaDuplaEncade<Material> materiais)
        {
            Grade grade = new Grade(tamanho);
            int quantidade = materiais.GetQuantidade();

            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    // Escolhe o material de forma determinística, para as duas grades ficarem idênticas.
                    Material material = materiais.ObterViaProx((linha * tamanho + coluna) % quantidade);
                    Corpo corpo = new Corpo(material, lado, 25);
                    grade.SetCorpo(linha, coluna, corpo);
                }
            }

            grade.GetCorpo(0, 0).SetTemp(100);
            return grade;
        }

        // Roda a mesma simulação nas duas versões e devolve a MAIOR diferença absoluta de
        // temperatura entre as duas grades. Deve ser ~0 (ordem de 1e-10 ou menos): a ordem das
        // somas muda o último dígito do ponto flutuante, mas não o resultado físico.
        public static double Executar(int tamanho, int passos, double deltaTempo, ListaDuplaEncade<Material> materiais)
        {
            Grade gradeSeq = CriarGrade(tamanho, 0.1, materiais);
            Grade gradePar = CriarGrade(tamanho, 0.1, materiais);

            Simulador simSeq = new Simulador(gradeSeq);
            Simulador simPar = new Simulador(gradePar);

            for (int i = 0; i < passos; i++)
            {
                simSeq.SimularPassoSequencial(deltaTempo);
                simPar.SimularPassoParalelo(deltaTempo);
            }

            double maiorDiferenca = 0;
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    double diferenca = Math.Abs(gradeSeq.GetCorpo(linha, coluna).GetTemp()
                                              - gradePar.GetCorpo(linha, coluna).GetTemp());
                    // Se alguma temperatura virou NaN (simulação instável), a comparação abaixo daria falso e esconderia o erro.
                    if (double.IsNaN(diferenca))
                    {
                        return double.NaN;
                    }
                    if (diferenca > maiorDiferenca)
                    {
                        maiorDiferenca = diferenca;
                    }
                }
            }

            return maiorDiferenca;
        }
    }
}

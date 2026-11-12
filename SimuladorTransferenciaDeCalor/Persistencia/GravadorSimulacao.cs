using System;
using System.Diagnostics;
using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor.Persistencia
{
    // Coordena a gravação de uma simulação: guarda o id, conta as iterações e mede o tempo gasto
    // gravando. O Form1 só chama três métodos (Iniciar, RegistrarIteracao, Finalizar) e não precisa
    // saber nada de banco de dados.
    public class GravadorSimulacao
    {
        private IRepositorioSimulacao repositorio;
        private long idSimulacao;
        private int iteracaoAtual;
        private bool ativa;

        // Soma do tempo gasto em chamadas ao repositório. É medido À PARTE do cálculo,
        // para o relatório separar "quanto custa calcular" de "quanto custa gravar".
        private Stopwatch cronometroGravacao = new Stopwatch();

        public TimeSpan TempoTotalGravacao
        {
            get { return cronometroGravacao.Elapsed; }
        }

        public int IteracoesGravadas
        {
            get { return iteracaoAtual; }
        }

        public GravadorSimulacao(IRepositorioSimulacao repositorio)
        {
            this.repositorio = repositorio;
        }

        // Começa a gravar uma nova simulação. Se havia uma anterior em andamento, encerra ela primeiro.
        public void Iniciar(Grade grade, ListaDuplaEncade<Material> materiais, double deltaTempo, string versao)
        {
            Finalizar();

            cronometroGravacao.Reset();
            cronometroGravacao.Start();

            repositorio.GarantirMateriais(materiais);
            idSimulacao = repositorio.IniciarSimulacao(grade.GetTamanho(), deltaTempo, versao);
            repositorio.GravarEstadoInicial(idSimulacao, RegistroCorpo.CapturarDe(grade));

            cronometroGravacao.Stop();

            iteracaoAtual = 0;
            ativa = true;
        }

        // Deve ser chamado logo DEPOIS de cada passo de cálculo.
        // Se nenhuma simulação foi iniciada, não faz nada.
        public void RegistrarIteracao(Grade grade, double tempoDecorrido)
        {
            if (!ativa)
            {
                return;
            }

            iteracaoAtual++;

            cronometroGravacao.Start();
            repositorio.GravarIteracao(idSimulacao, iteracaoAtual, tempoDecorrido, RegistroCorpo.CapturarDe(grade));
            cronometroGravacao.Stop();
        }

        // Encerra a simulação em andamento (se houver) e descarrega o que estiver pendente.
        public void Finalizar()
        {
            if (!ativa)
            {
                return;
            }

            ativa = false;

            cronometroGravacao.Start();
            repositorio.FinalizarSimulacao(idSimulacao);
            cronometroGravacao.Stop();
        }
    }
}

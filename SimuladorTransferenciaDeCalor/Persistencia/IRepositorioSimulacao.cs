using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor.Persistencia
{
    // Contrato entre o simulador e o armazenamento dos resultados.
    // O simulador só conhece esta interface; quem implementa (RepositorioNulo, RepositorioMySql...)
    // decide COMO gravar. Assim o cálculo pode ser medido sem banco, e o banco pode ser trocado
    // ou melhorado (gravação em lote, fila em outra thread) sem mexer na tela nem no Simulador.
    public interface IRepositorioSimulacao
    {
        // Garante que os materiais existem no armazenamento (tabela de materiais).
        // Pode ser chamado várias vezes: a implementação deve evitar duplicar registros.
        void GarantirMateriais(ListaDuplaEncade<Material> materiais);

        // Registra uma nova simulação e devolve o identificador dela.
        // 'versao' indica qual algoritmo gerou os dados ("sequencial" ou "paralela"),
        // útil para comparar N1 x N2 no relatório.
        long IniciarSimulacao(int tamanho, double deltaTempo, string versao);

        // Grava o estado ANTES do primeiro passo (iteração 0).
        // Junto com o deltaTempo da simulação, é o ponto de partida para remontá-la.
        void GravarEstadoInicial(long idSimulacao, RegistroCorpo[] corpos);

        // Grava o estado de TODOS os corpos após uma iteração.
        // numeroIteracao começa em 1; tempoDecorrido é o tempo simulado acumulado, em segundos.
        // O vetor recebido é uma cópia exclusiva: a implementação pode guardá-lo e gravar depois.
        void GravarIteracao(long idSimulacao, int numeroIteracao, double tempoDecorrido, RegistroCorpo[] corpos);

        // Marca o fim da simulação. Se a implementação usa fila ou lote, deve descarregar
        // (gravar) tudo o que ainda estiver pendente antes de retornar.
        void FinalizarSimulacao(long idSimulacao);
    }
}

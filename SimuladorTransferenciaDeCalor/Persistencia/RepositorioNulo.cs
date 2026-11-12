using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor.Persistencia
{
    // Implementação que NÃO grava nada.
    // Serve para o programa funcionar enquanto o banco não existe e, principalmente, para o
    // benchmark medir o tempo de CÁLCULO puro, sem a interferência da gravação.
    // Segue o padrão "Null Object": em vez de espalhar 'if (repositorio != null)' pelo código,
    // usamos um objeto que simplesmente não faz nada.
    public class RepositorioNulo : IRepositorioSimulacao
    {
        public void GarantirMateriais(ListaDuplaEncade<Material> materiais)
        {
            // Nada a gravar.
        }

        public long IniciarSimulacao(int tamanho, double deltaTempo, string versao)
        {
            return 0;   // Identificador fictício
        }

        public void GravarEstadoInicial(long idSimulacao, RegistroCorpo[] corpos)
        {
            // Nada a gravar.
        }

        public void GravarIteracao(long idSimulacao, int numeroIteracao, double tempoDecorrido, RegistroCorpo[] corpos)
        {
            // Nada a gravar.
        }

        public void FinalizarSimulacao(long idSimulacao)
        {
            // Nada a gravar.
        }
    }
}

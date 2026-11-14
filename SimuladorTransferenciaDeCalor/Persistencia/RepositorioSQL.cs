using System.Globalization;
using System.Text;
using MySqlConnector;   // pacote NuGet: MySqlConnector
using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor.Persistencia
{
    /// <summary>
    /// Implementação da interface IRepositorioSimulacao que grava no MySQL.
    /// O simulador só conhece a interface: ele chama os métodos abaixo e
    /// esta classe traduz cada chamada em comandos SQL.
    /// </summary>
    public class RepositorioMySql : IRepositorioSimulacao
    {
        private readonly string _stringDeConexao;

        // Guarda "nome do material -> id na tabela Material".
        // O RegistroCorpo traz só o NomeMaterial, mas IteracaoCorpo guarda o id.
        // Carregamos o dicionário uma vez (em GarantirMateriais) para não
        // fazer um SELECT por corpo.
        private readonly Dictionary<string, int> _idPorMaterial = new Dictionary<string, int>();

        // Números sempre com ponto decimal, independente do idioma do Windows.
        // Sem isso, 0,1 seria escrito com vírgula e quebraria o SQL.
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // Linhas por INSERT. Com 400 corpos (20x20) cabe tudo em um comando.
        // Em grades maiores (benchmark) divide em lotes para não estourar
        // o max_allowed_packet do servidor.
        private const int TamanhoLote = 500;

        public RepositorioMySql(string stringDeConexao)
        {
            // Falha cedo e com mensagem clara se a variável de ambiente não foi definida.
            if (string.IsNullOrWhiteSpace(stringDeConexao))
                throw new InvalidOperationException(
                    "Defina a variável de ambiente SIMULADOR_DB com a string de conexão do MySQL.");

            _stringDeConexao = stringDeConexao;
        }

        // Cada método abre a sua própria conexão. O MySqlConnector mantém um
        // "pool": fechar a conexão só devolve ela ao pool, e abrir de novo é
        // barato. Assim não precisamos guardar uma conexão aberta na classe.

        // ---------------------------------------------------------------
        // 1) Nova simulação: garante os materiais
        // ---------------------------------------------------------------
        public void GarantirMateriais(ListaDuplaEncade<Material> materiais)
        {
            using (var con = new MySqlConnection(_stringDeConexao))
            {
                con.Open();

                for (int i = 0; i < materiais.GetQuantidade(); i++)
                {
                    Material m = materiais.ObterViaProx(i);

                    // ON DUPLICATE KEY UPDATE usa o UNIQUE de "nome": se o
                    // material já existe, atualiza em vez de duplicar.
                    using (var cmd = new MySqlCommand(
                        @"INSERT INTO Material (nome, densidade, calor_especifico, condutividade)
                          VALUES (@nome, @dens, @calor, @cond)
                          ON DUPLICATE KEY UPDATE
                              densidade = VALUES(densidade),
                              calor_especifico = VALUES(calor_especifico),
                              condutividade = VALUES(condutividade)", con))
                    {
                        // Os campos de Material são privados, então usamos os getters.
                        // GetNome() existe (usado no Form1). Os outros três seguem o
                        // mesmo padrão; se o nome real for diferente, ajuste aqui.
                        cmd.Parameters.AddWithValue("@nome", m.GetNome());
                        cmd.Parameters.AddWithValue("@dens", m.GetDensidade());
                        cmd.Parameters.AddWithValue("@calor", m.GetCalor());
                        cmd.Parameters.AddWithValue("@cond", m.GetCondutividade());
                        cmd.ExecuteNonQuery();
                    }
                }

                // Carrega o dicionário nome -> id.
                _idPorMaterial.Clear();
                using (var cmd = new MySqlCommand("SELECT id, nome FROM Material", con))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        _idPorMaterial[rd.GetString("nome")] = Convert.ToInt32(rd["id"]);
                }
            }
        }

        // ---------------------------------------------------------------
        // 2) Cria a linha em Simulacao e devolve o id gerado
        // ---------------------------------------------------------------
        public long IniciarSimulacao(int tamanho, double deltaTempo, string versao)
        {
            using (var con = new MySqlConnection(_stringDeConexao))
            {
                con.Open();
                using (var cmd = new MySqlCommand(
                    @"INSERT INTO Simulacao (tamanho, delta_tempo, versao)
                      VALUES (@tam, @delta, @versao)", con))
                {
                    cmd.Parameters.AddWithValue("@tam", tamanho);
                    cmd.Parameters.AddWithValue("@delta", deltaTempo);
                    cmd.Parameters.AddWithValue("@versao", versao);
                    cmd.ExecuteNonQuery();

                    // id gerado pelo AUTO_INCREMENT. O simulador guarda esse
                    // valor e o devolve nas chamadas seguintes.
                    return cmd.LastInsertedId;
                }
            }
        }

        // ---------------------------------------------------------------
        // 3) Estado inicial = iteração 0, tempo 0
        // ---------------------------------------------------------------
        public void GravarEstadoInicial(long idSimulacao, RegistroCorpo[] corpos)
        {
            GravarEstado(idSimulacao, 0, 0.0, corpos);
        }

        // ---------------------------------------------------------------
        // 4) A cada passo da simulação
        // ---------------------------------------------------------------
        public void GravarIteracao(long idSimulacao, int numero, double tempoDecorrido, RegistroCorpo[] corpos)
        {
            GravarEstado(idSimulacao, numero, tempoDecorrido, corpos);
        }

        // ---------------------------------------------------------------
        // 5) Encerramento
        // ---------------------------------------------------------------
        public void FinalizarSimulacao(long idSimulacao)
        {
            // Esta versão grava de forma síncrona (sem fila), então não há
            // nada pendente para descarregar. Se for adicionada uma fila
            // com thread gravadora, esperar ela esvaziar AQUI antes do UPDATE.
            using (var con = new MySqlConnection(_stringDeConexao))
            {
                con.Open();
                using (var cmd = new MySqlCommand(
                    "UPDATE Simulacao SET finalizada = TRUE WHERE id = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", idSimulacao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ---------------------------------------------------------------
        // Gravação de uma iteração (usada pelo estado inicial e pelos passos)
        // ---------------------------------------------------------------
        private void GravarEstado(long idSimulacao, int numero, double tempo, RegistroCorpo[] corpos)
        {
            using (var con = new MySqlConnection(_stringDeConexao))
            {
                con.Open();

                // Transação: ou grava a iteração inteira (linha em Iteracao +
                // todos os corpos) ou não grava nada. Também é mais rápido
                // que confirmar (commit) a cada INSERT.
                using (var tx = con.BeginTransaction())
                {
                    // Primeiro a linha pai (Iteracao): IteracaoCorpo aponta
                    // para ela por chave estrangeira.
                    using (var cmd = new MySqlCommand(
                        @"INSERT INTO Iteracao (id_simulacao, numero, tempo_decorrido)
                          VALUES (@sim, @num, @tempo)", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@sim", idSimulacao);
                        cmd.Parameters.AddWithValue("@num", numero);
                        cmd.Parameters.AddWithValue("@tempo", tempo);
                        cmd.ExecuteNonQuery();
                    }

                    // Depois os corpos, vários por INSERT (em lote).
                    // Um INSERT por corpo faria 400 viagens ao servidor por
                    // iteração; em lote é uma só.
                    for (int inicio = 0; inicio < corpos.Length; inicio += TamanhoLote)
                    {
                        int fim = Math.Min(inicio + TamanhoLote, corpos.Length);

                        var sql = new StringBuilder(
                            "INSERT INTO IteracaoCorpo " +
                            "(id_simulacao, numero_iteracao, linha, coluna, id_material, lado, temperatura) VALUES ");

                        for (int i = inicio; i < fim; i++)
                        {
                            RegistroCorpo c = corpos[i];

                            int idMaterial;
                            if (!_idPorMaterial.TryGetValue(c.NomeMaterial, out idMaterial))
                                throw new InvalidOperationException(
                                    "Material não encontrado na tabela Material: " + c.NomeMaterial);

                            if (i > inicio) sql.Append(',');

                            // Só números entram no texto do SQL (nada digitado
                            // pelo usuário), então não há risco de SQL injection.
                            sql.Append('(')
                               .Append(idSimulacao.ToString(Inv)).Append(',')
                               .Append(numero.ToString(Inv)).Append(',')
                               .Append(c.Linha.ToString(Inv)).Append(',')
                               .Append(c.Coluna.ToString(Inv)).Append(',')
                               .Append(idMaterial.ToString(Inv)).Append(',')
                               .Append(c.Lado.ToString("R", Inv)).Append(',')
                               .Append(c.Temperatura.ToString("R", Inv))
                               .Append(')');
                        }

                        using (var cmd = new MySqlCommand(sql.ToString(), con, tx))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        // ---------------------------------------------------------------
        // Leitura (fora da interface): prova que dá para remontar a simulação.
        // Devolve tuplas em vez de RegistroCorpo, porque o RegistroCorpo é
        // somente leitura (não dá para preencher as propriedades depois de criar).
        // ---------------------------------------------------------------
        public List<(int Linha, int Coluna, string Material, double Lado, double Temperatura)>
            LerIteracao(long idSimulacao, int numero)
        {
            var lista = new List<(int Linha, int Coluna, string Material, double Lado, double Temperatura)>();

            using (var con = new MySqlConnection(_stringDeConexao))
            {
                con.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT ic.linha, ic.coluna, m.nome, ic.lado, ic.temperatura
                      FROM IteracaoCorpo ic
                      JOIN Material m ON m.id = ic.id_material
                      WHERE ic.id_simulacao = @sim AND ic.numero_iteracao = @num
                      ORDER BY ic.linha, ic.coluna", con))
                {
                    cmd.Parameters.AddWithValue("@sim", idSimulacao);
                    cmd.Parameters.AddWithValue("@num", numero);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add((
                                Convert.ToInt32(rd["linha"]),
                                Convert.ToInt32(rd["coluna"]),
                                rd.GetString("nome"),
                                rd.GetDouble("lado"),
                                rd.GetDouble("temperatura")));
                        }
                    }
                }
            }

            return lista;
        }
    }
}
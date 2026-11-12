using SimuladorTransferenciaDeCalor.Classes;
using SimuladorTransferenciaDeCalor.Persistencia;

namespace SimuladorTransferenciaDeCalor
{
    public partial class Form1 : Form
    {
        private ListaDuplaEncade<Material> materiais;
        private Grade grade;
        private Label[,] labelsGrade;
        private CelulaGradiente[,] celulas;   // Painéis que desenham o degradê de cada quadrado
        private Simulador simulador;
        private double tempoDecorrido = 0;

        // Grava os resultados de cada iteração. Hoje usa o RepositorioNulo (não grava nada).
        // Quando o banco MySQL estiver pronto, basta trocar por: new GravadorSimulacao(new RepositorioMySql(...)).
        private GravadorSimulacao gravador = new GravadorSimulacao(new RepositorioNulo());

        // Indica qual algoritmo gera os dados gravados (usado para comparar N1 x N2 no banco e no relatório).
        // Se trocar a chamada no timerSimulacao_Tick para o sequencial, troque este texto também.
        private const string VersaoSimulador = "paralela";

        // Pontos de referência da escala de cores, do mais frio ao mais quente.
        // Entre dois pontos consecutivos a cor é interpolada, o que gera a transição suave.
        private static readonly Color[] coresGradiente =
        {
            Color.FromArgb(90, 130, 230),   // frio    (azul)
            Color.FromArgb(120, 210, 235),  //         (ciano)
            Color.FromArgb(250, 240, 130),  //         (amarelo)
            Color.FromArgb(250, 160, 60),   //         (laranja)
            Color.FromArgb(235, 80, 70)     // quente  (vermelho)
        };

        // Faixa de temperatura (°C) coberta pela escala. Fora dela a cor fica presa nas pontas.
        // O ponto quente inicial é 100 °C, então o máximo de 100 o mostra com a cor mais quente.
        private const double TempMinimaGradiente = 0;
        private const double TempMaximaGradiente = 100;

        public Form1()
        {
            InitializeComponent();
            materiais = CriarMateriais();
            CarregarLista();
        }

        // Ao fechar a janela, encerra a gravação em andamento para que nada fique pendente
        // (importante quando o repositório usa lote ou fila).
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gravador.Finalizar();
            base.OnFormClosing(e);
        }

        private ListaDuplaEncade<Material> CriarMateriais()
        {
            ListaDuplaEncade<Material> lista = new ListaDuplaEncade<Material>();

            lista.Adicionar(new Material("Alumínio", 2700, 900, 237));
            lista.Adicionar(new Material("Aço", 8060, 480, 15.1));
            lista.Adicionar(new Material("Bronze", 8150, 435, 64));
            lista.Adicionar(new Material("Cobre", 8930, 385, 401));
            lista.Adicionar(new Material("Chumbo", 11300, 129, 35.3));
            lista.Adicionar(new Material("Ferro", 7870, 447, 80.2));
            lista.Adicionar(new Material("Latão", 8490, 343, 111));
            lista.Adicionar(new Material("Madeira", 610, 2386, 0.15));
            lista.Adicionar(new Material("Níquel", 8910, 448, 90));
            lista.Adicionar(new Material("Ouro", 19300, 129, 317));
            lista.Adicionar(new Material("Prata", 10500, 235, 429));
            lista.Adicionar(new Material("Vidro", 2500, 750, 1.4));
            lista.Adicionar(new Material("Zinco", 7150, 381, 112));

            return lista;
        }

        private void CarregarLista()
        {
            for (int i = 0; i < materiais.GetQuantidade(); i++)
            {
                boxMaterial.Items.Add(materiais.ObterViaProx(i));  //Carrega todos os materias a caixa de seleção.
            }
        }

        // Tempo simulado de cada passo, em segundos. Fica num método único porque tanto o
        // cálculo (timer) quanto a gravação (estado inicial) precisam do mesmo valor.
        private double ObterDeltaTempo()
        {
            return (timerSimulacao.Interval / 1000.0) * 50;
        }

        private void BotaoCriarGrade(object sender, EventArgs e)
        {
            try
            {
                if (boxMaterial.SelectedItem == null)
                {
                    throw new Exception("O campo material não pode ficar vazio.");
                }

                Material materialSelecionado = (Material)boxMaterial.SelectedItem;

                double lado = (double)LadoCubo.Value;
                double temperatura = (double)TemperaturaInicial.Value;

                int tamanho = (int)TamanhoDaGrade.Value;
                float escala = DeviceDpi / 96f;

                int larguraQuadrado = (int)(80 * escala);
                int alturaQuadrado = (int)(60 * escala);

                grade = new Grade(tamanho);
                labelsGrade = new Label[tamanho, tamanho];
                celulas = new CelulaGradiente[tamanho, tamanho];

                tempoDecorrido = 0;
                labelTempo.Text = "Tempo: 0,0 s";  //Reinicia o timer do tempo decorrido sempre que se cria nova grade
                timerSimulacao.Stop(); //Pausa o timer quando isso acontece

                simulador = new Simulador(grade);

                painelGrade.Controls.Clear();
                painelGrade.AutoScroll = true;

                for (int linha = 0; linha < tamanho; linha++)
                {
                    for (int coluna = 0; coluna < tamanho; coluna++)
                    {
                        Corpo corpo = new Corpo(materialSelecionado, lado, temperatura);

                        grade.SetCorpo(linha, coluna, corpo);

                        if (linha == 0 && coluna == 0)
                        {
                            corpo.SetTemp(100);
                        }

                        // O quadrado é um painel que desenha o próprio degradê de fundo.
                        CelulaGradiente quadrado = new CelulaGradiente();
                        celulas[linha, coluna] = quadrado;

                        quadrado.Width = larguraQuadrado;
                        quadrado.Height = alturaQuadrado;
                        quadrado.BorderStyle = BorderStyle.FixedSingle;

                        quadrado.Location = new Point(coluna * larguraQuadrado, linha * alturaQuadrado);

                        ////////////////////////////////////////////////////
                        Label informacoes = new Label();
                        labelsGrade[linha, coluna] = informacoes;

                        informacoes.AutoSize = false;
                        informacoes.Dock = DockStyle.Fill;
                        informacoes.TextAlign = ContentAlignment.MiddleCenter;
                        informacoes.BackColor = Color.Transparent;   // Transparente: deixa o degradê do painel aparecer por trás do texto

                        int linhaAtual = linha;
                        int colunaAtual = coluna;

                        informacoes.Click += (sender, e) =>
                        {
                            Corpo corpoEncontrado = grade.GetCorpo(linhaAtual, colunaAtual);

                            InformacoesCorpoForm janela = new InformacoesCorpoForm(corpoEncontrado, materiais);
                            janela.ShowDialog(this);
                            AtualizarGrade();
                            // Permite clicar no quadrado e mostra uma janela com as informações.
                        };

                        quadrado.Controls.Add(informacoes);
                        ////////////////////////////////////////////////////

                        painelGrade.Controls.Add(quadrado);
                    }
                }

                // Preenche textos e cores iniciais de todos os quadrados de uma vez.
                AtualizarGrade();

                // Começa a gravar esta nova simulação: registra os materiais, a simulação em si
                // e o estado inicial da grade (iteração 0). Se havia outra em andamento, ela é encerrada.
                gravador.Iniciar(grade, materiais, ObterDeltaTempo(), VersaoSimulador);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                boxMaterial.Focus();            //Check se o usuário deixou o campo Material vazio ao criar Grade,
                return;                         //Mostra mensagem de erro e volta para seleção.
            }
        }

        // Converte uma temperatura em cor usando uma escala contínua.
        private Color ObterCorTemp(double temperatura)
        {
            // Normaliza a temperatura para 0..1 dentro da faixa da escala
            double t = (temperatura - TempMinimaGradiente) / (TempMaximaGradiente - TempMinimaGradiente);
            t = Math.Clamp(t, 0, 1);

            // Descobre entre quais dois pontos de referência a temperatura está
            double posicao = t * (coresGradiente.Length - 1);
            int indice = (int)Math.Floor(posicao);

            // t == 1 cai exatamente no último ponto, que não tem "próximo" para interpolar
            if (indice >= coresGradiente.Length - 1)
            {
                return coresGradiente[coresGradiente.Length - 1];
            }

            // Fração (0..1) do caminho entre o ponto atual e o seguinte
            double fracao = posicao - indice;
            Color de = coresGradiente[indice];
            Color para = coresGradiente[indice + 1];

            return Color.FromArgb(
                Interpolar(de.R, para.R, fracao),
                Interpolar(de.G, para.G, fracao),
                Interpolar(de.B, para.B, fracao));
        }

        // Interpolação linear entre dois valores de canal de cor (0..255).
        private static int Interpolar(int a, int b, double fracao)
        {
            return (int)Math.Round(a + (b - a) * fracao);
        }

        // Cor intermediária (média canal a canal) entre duas cores.
        private static Color Misturar(Color a, Color b)
        {
            return Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);
        }

        private void AtualizarGrade()
        {
            int tamanho = labelsGrade.GetLength(0);

            // Primeiro calcula a cor de cada corpo, porque o degradê de um quadrado
            // depende da cor dos vizinhos.
            Color[,] cores = new Color[tamanho, tamanho];
            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    cores[linha, coluna] = ObterCorTemp(grade.GetCorpo(linha, coluna).GetTemp());
                }
            }

            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpo = grade.GetCorpo(linha, coluna);

                    labelsGrade[linha, coluna].Text = "[" + linha + "," + coluna + "]" +
                                                      "\n" + corpo.GetMaterial().GetNome() +
                                                      "\n" + corpo.GetTemp().ToString("F2") + " °C";

                    Color centro = cores[linha, coluna];

                    // Em cada borda: média entre este corpo e o vizinho (ou a própria cor, se não houver vizinho).
                    // Os dois quadrados que dividem uma borda calculam a mesma média, então o degradê é contínuo.
                    Color esquerda = coluna > 0 ? Misturar(centro, cores[linha, coluna - 1]) : centro;
                    Color direita = coluna < tamanho - 1 ? Misturar(centro, cores[linha, coluna + 1]) : centro;
                    Color cima = linha > 0 ? Misturar(centro, cores[linha - 1, coluna]) : centro;
                    Color baixo = linha < tamanho - 1 ? Misturar(centro, cores[linha + 1, coluna]) : centro;

                    celulas[linha, coluna].DefinirCores(esquerda, centro, direita, cima, baixo);
                }
            }
        }

        private void BotaoIniciar_Click(object sender, EventArgs e)
        {
            if (simulador == null)
            {
                MessageBox.Show("Crie a grade primeiro.");
                return;
            }
            timerSimulacao.Start();
        }

        private void BotaoPausar_Click(object sender, EventArgs e)
        {
            timerSimulacao.Stop();
        }

        private void timerSimulacao_Tick(object sender, EventArgs e)
        {
            double deltaTempo = ObterDeltaTempo();

            // Versão paralela (N2). Para voltar à versão da N1, troque por SimularPassoSequencial
            // (e ajuste a constante VersaoSimulador no topo da classe).
            simulador.SimularPassoParalelo(deltaTempo);

            tempoDecorrido += deltaTempo;
            labelTempo.Text = "Tempo decorrido: " + tempoDecorrido.ToString("F1") + "s";

            // Grava o estado de todos os corpos após este passo (iteração atual).
            gravador.RegistrarIteracao(grade, tempoDecorrido);

            AtualizarGrade();
        }
    }
}

using SimuladorTransferenciaDeCalor.Classes;

namespace SimuladorTransferenciaDeCalor
{
    public partial class Form1 : Form
    {
        private ListaDuplaEncade<Material> materiais;
        private Grade grade;
        private Label[,] labelsGrade;
        private Simulador simulador;
        private double tempoDecorrido = 0;
        public Form1()
        {
            InitializeComponent();
            materiais = CriarMateriais();
            CarregarLista();


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

                        Panel quadrado = new Panel();

                        quadrado.Width = larguraQuadrado;
                        quadrado.Height = alturaQuadrado;
                        quadrado.BorderStyle = BorderStyle.FixedSingle;

                        quadrado.Location = new Point(coluna * larguraQuadrado, linha * alturaQuadrado);

                        ////////////////////////////////////////////////////
                        Label informacoes = new Label();
                        labelsGrade[linha, coluna] = informacoes;

                        informacoes.Text = "[" + linha + "," + coluna + "]" +
                                           "\n" + corpo.GetMaterial().GetNome() + //Mostra o nome do corpo atual.
                                           "\n" + corpo.GetTemp() + " °C";      //Mostra a temperatura atual do corpo.

                        informacoes.AutoSize = false;
                        informacoes.Dock = DockStyle.Fill;
                        informacoes.TextAlign = ContentAlignment.MiddleCenter;


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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                boxMaterial.Focus();            //Check se o usuário deixou o campo Material vazio ao criar Grade,
                return;                         //Mostra mensagem de erro e volta para seleção.
            }
        }
        private Color ObterCorTemp(double temperatura)
        {
            if (temperatura < 10)
                return Color.LightBlue;

            if (temperatura < 25)
                return Color.LightYellow;         //Altera a cor da label de cada corpo dependendo da sua Temperatura.

            if (temperatura < 50)
                return Color.Orange;

            return Color.IndianRed;
        }
        private void AtualizarGrade()
        {
            int tamanho = labelsGrade.GetLength(0);

            for (int linha = 0; linha < tamanho; linha++)
            {
                for (int coluna = 0; coluna < tamanho; coluna++)
                {
                    Corpo corpo = grade.GetCorpo(linha, coluna);

                    labelsGrade[linha, coluna].Text = "[" + linha + "," + coluna + "]" +
                                                      "\n" + corpo.GetMaterial().GetNome() +
                                                      "\n" + corpo.GetTemp().ToString("F2") + " °C";

                    labelsGrade[linha, coluna].BackColor = ObterCorTemp(corpo.GetTemp());
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
            double deltaTempo = (timerSimulacao.Interval / 1000.0) * 50;
            simulador.SimularPassoSequencial(deltaTempo);   // antes: SimularPasso

            tempoDecorrido += deltaTempo;
            labelTempo.Text = "Tempo decorrido: " + tempoDecorrido.ToString("F1") + "s";

            AtualizarGrade();
        }
    }
}

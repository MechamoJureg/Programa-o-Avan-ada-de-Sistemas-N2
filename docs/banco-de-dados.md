# Guia da parte de banco de dados (MySQL) — Simulador de Troca de Calor N2

Este documento explica o que já está pronto no código e o que falta fazer na parte do banco de dados.
O simulador **não conhece o MySQL**: ele só conversa com uma interface (`IRepositorioSimulacao`).
O trabalho do banco é criar as tabelas e implementar essa interface.

## 1. O que o enunciado pede

1. Gravar os resultados de **cada iteração** em um banco MySQL.
   - Devem ser gravadas **todas as informações de cada corpo, para cada iteração**.
   - Os dados gravados precisam ser **suficientes para remontar a simulação**.
2. O processamento foi paralelizado (já feito, não é com você).
3. O relatório compara a versão N1 (sequencial) com a N2 (paralela). Para isso, o **tempo de gravação precisa ser medido separado do tempo de cálculo** (o código já cronometra, veja a seção 6).

## 2. Como a simulação funciona (o mínimo para modelar os dados)

- A simulação é uma grade **N x N** de corpos cúbicos. Cada corpo tem: material, lado (m) e temperatura (°C).
- A cada passo (iteração), o simulador avança um intervalo de tempo `deltaTempo` (segundos simulados) e recalcula a temperatura de todos os corpos pela Lei de Fourier.
- Todos os corpos de uma grade são criados com o mesmo material, lado e temperatura inicial, e o corpo `[0,0]` começa a 100 °C. **Durante a simulação, o usuário pode trocar o material ou a temperatura de um corpo** pela tela. Por isso o material precisa ser gravado **a cada iteração**, e não só no início.
- Massa e volume são derivados (volume = lado³; massa = densidade × volume), então não precisam ser gravados.

## 3. O que já está pronto no código

Tudo fica na pasta `SimuladorTransferenciaDeCalor/Persistencia/`:

| Arquivo | Para que serve |
|---|---|
| `IRepositorioSimulacao.cs` | O contrato. **É isto que você implementa.** |
| `RegistroCorpo.cs` | "Foto" de um corpo: `Linha`, `Coluna`, `NomeMaterial`, `Lado`, `Temperatura`. |
| `RepositorioNulo.cs` | Implementação que não grava nada (usada hoje, e no benchmark do cálculo puro). |
| `GravadorSimulacao.cs` | Coordena a gravação: guarda o id, conta as iterações e cronometra o tempo gasto. |

O `Form1.cs` já chama o gravador nos pontos certos:

- ao **criar a grade**: `gravador.Iniciar(...)`;
- a **cada passo**, depois do cálculo: `gravador.RegistrarIteracao(...)`;
- ao **fechar a janela** (e ao criar uma nova grade): `gravador.Finalizar()`.

### O contrato (`IRepositorioSimulacao`)

| Método | Quando é chamado | O que deve fazer |
|---|---|---|
| `GarantirMateriais(lista)` | A cada nova simulação | Garantir que os 13 materiais existem na tabela, **sem duplicar** se já existirem. |
| `IniciarSimulacao(tamanho, deltaTempo, versao)` | Ao criar a grade | Criar o registro da simulação e **devolver o id** (`long`). `versao` é `"paralela"` ou `"sequencial"`. |
| `GravarEstadoInicial(id, corpos)` | Logo após iniciar | Gravar o estado **antes** do primeiro passo (iteração 0). |
| `GravarIteracao(id, numero, tempoDecorrido, corpos)` | A cada passo | Gravar o estado de **todos** os corpos. `numero` começa em 1; `tempoDecorrido` é o tempo simulado acumulado, em segundos. |
| `FinalizarSimulacao(id)` | Ao encerrar | Marcar o fim. **Se você usar fila ou lote, descarregue (grave) tudo que estiver pendente antes de retornar.** |

Pontos importantes:

- O vetor `RegistroCorpo[]` recebido em `GravarIteracao` é uma **cópia exclusiva**: você pode guardá-lo e gravar depois, em outra thread, sem risco de ele mudar.
- Os registros vêm ordenados **linha a linha** (linha 0 inteira, depois linha 1, etc.).
- `NomeMaterial` identifica o material (chave para a tabela de materiais).
- A chamada de `GravarIteracao` acontece **na thread da tela**. Se ela demorar, a interface trava. Por isso vale gravar em lote e/ou usar uma fila com uma thread gravadora (seção 5).

## 4. Modelo de tabelas (sugestão, ajuste como achar melhor)

```sql
CREATE TABLE Material (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nome            VARCHAR(50) NOT NULL UNIQUE,
    densidade       DOUBLE NOT NULL,   -- kg/m3
    calor_especifico DOUBLE NOT NULL,  -- J/(kg.K)
    condutividade   DOUBLE NOT NULL    -- W/(m.K)
);

CREATE TABLE Simulacao (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    data_hora       DATETIME NOT NULL,
    tamanho         INT NOT NULL,      -- N da grade N x N
    delta_tempo     DOUBLE NOT NULL,   -- segundos simulados por passo
    versao          VARCHAR(20) NOT NULL,  -- 'paralela' ou 'sequencial'
    finalizada      BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE Iteracao (
    id_simulacao    BIGINT NOT NULL,
    numero          INT NOT NULL,      -- 0 = estado inicial
    tempo_decorrido DOUBLE NOT NULL,
    PRIMARY KEY (id_simulacao, numero),
    FOREIGN KEY (id_simulacao) REFERENCES Simulacao(id)
);

CREATE TABLE IteracaoCorpo (
    id_simulacao    BIGINT NOT NULL,
    numero_iteracao INT NOT NULL,
    linha           INT NOT NULL,
    coluna          INT NOT NULL,
    id_material     INT NOT NULL,
    lado            DOUBLE NOT NULL,
    temperatura     DOUBLE NOT NULL,   -- °C
    PRIMARY KEY (id_simulacao, numero_iteracao, linha, coluna),
    FOREIGN KEY (id_simulacao, numero_iteracao) REFERENCES Iteracao(id_simulacao, numero),
    FOREIGN KEY (id_material) REFERENCES Material(id)
);
```

Observações:

- Esta é uma proposta. O que importa é cumprir o requisito: **todos os dados de cada corpo em cada iteração**, suficientes para remontar.
- O estado inicial pode ser guardado como iteração 0 (tempo 0), como acima.
- Se achar que o `lado` repetido em todas as linhas é desperdício, pode guardá-lo uma vez por corpo em outra tabela. Mas lembre que o enunciado pede *todas* as informações de cada corpo por iteração.
- Pense em índices e na chave primária com cuidado: eles aceleram a leitura, mas **deixam a gravação mais lenta**.

## 5. Volume de dados e desempenho

Cada iteração grava **N² linhas**. Exemplos:

| Grade | Linhas por iteração | 1.000 iterações |
|---|---|---|
| 20 x 20 (máximo da tela) | 400 | 400 mil |
| 100 x 100 | 10 mil | 10 milhões |
| 500 x 500 (benchmark) | 250 mil | 250 milhões |

Por isso:

1. **Nunca grave uma linha por vez.** Use inserção em lote (`INSERT` com várias linhas em um comando, ou `MySqlBulkCopy` se usar o driver MySqlConnector) **dentro de uma transação**. Isso costuma ser a maior diferença de desempenho.
2. Considere uma **fila com uma thread gravadora** (por exemplo, `System.Threading.Channels` ou `BlockingCollection`): `GravarIteracao` só enfileira o vetor e retorna rápido; a outra thread grava. Nesse caso, `FinalizarSimulacao` precisa **esperar a fila esvaziar**.
3. Deixe um limite razoável na fila para não consumir toda a memória se o banco ficar mais lento que a simulação.
4. Se uma gravação falhar, decida o comportamento (mostrar erro? parar a simulação?). Não deixe a exceção passar despercebida.

## 6. Medição de tempo (para o relatório)

O `GravadorSimulacao` já cronometra o tempo gasto nas chamadas ao repositório:

- `gravador.TempoTotalGravacao`: soma do tempo gasto gravando;
- `gravador.IteracoesGravadas`: quantas iterações foram gravadas.

Atenção: com fila em outra thread, esse tempo mede só o custo de **enfileirar**, não o de gravar de fato. Se usarem fila, meçam também o tempo total até `FinalizarSimulacao` terminar, para o relatório mostrar o custo real.

## 7. Como ligar o seu repositório ao programa

1. Crie a classe `RepositorioMySql : IRepositorioSimulacao` (pode ser na pasta `Persistencia`).
2. No `Form1.cs`, troque esta linha:

   ```csharp
   private GravadorSimulacao gravador = new GravadorSimulacao(new RepositorioNulo());
   ```

   pela versão com o seu repositório, por exemplo:

   ```csharp
   private GravadorSimulacao gravador = new GravadorSimulacao(new RepositorioMySql(stringDeConexao));
   ```

3. Instale o pacote NuGet do driver MySQL (por exemplo, `MySqlConnector`).

### Segurança: não suba a senha para o Git

A string de conexão tem usuário e senha. **Não coloque isso no código nem no repositório.** Opções:

- ler de uma **variável de ambiente**; ou
- ler de um arquivo de configuração local (por exemplo, `appsettings.local.json`) **adicionado ao `.gitignore`**.

Deixe no repositório só um arquivo de exemplo sem a senha (por exemplo, `appsettings.exemplo.json`).

## 8. Como validar

1. Rode uma simulação pequena (por exemplo, 5 x 5, 20 iterações) com o repositório MySQL.
2. Confira no banco: `Simulacao` com 1 registro; `Iteracao` com 21 (iteração 0 + 20); `IteracaoCorpo` com 21 x 25 registros.
3. Leia de volta os dados de uma iteração e compare as temperaturas com as da tela (ou com `RegistroCorpo.CapturarDe(grade)` no mesmo instante).
4. Teste o caso da **troca de material no meio da simulação**: clique em um corpo, altere o material e confira que as iterações seguintes gravam o material novo.
5. Teste fechar a janela no meio de uma simulação e confira que nada ficou pendente na fila.

Dica para "remontar a simulação": com o estado de uma iteração gravado, dá para recriar a grade (corpos com material, lado e temperatura) e redesenhar a tela. Vale deixar um método de leitura no repositório (por exemplo, `LerIteracao(id, numero)`), mesmo que não esteja na interface, para provar que os dados são suficientes.

## 9. Os 13 materiais

Estes são os materiais que o simulador usa (nome, densidade em kg/m³, calor específico em J/(kg·K), condutividade em W/(m·K)):

| Nome | Densidade | Calor específico | Condutividade |
|---|---|---|---|
| Alumínio | 2700 | 900 | 237 |
| Aço | 8060 | 480 | 15,1 |
| Bronze | 8150 | 435 | 64 |
| Cobre | 8930 | 385 | 401 |
| Chumbo | 11300 | 129 | 35,3 |
| Ferro | 7870 | 447 | 80,2 |
| Latão | 8490 | 343 | 111 |
| Madeira | 610 | 2386 | 0,15 |
| Níquel | 8910 | 448 | 90 |
| Ouro | 19300 | 129 | 317 |
| Prata | 10500 | 235 | 429 |
| Vidro | 2500 | 750 | 1,4 |
| Zinco | 7150 | 381 | 112 |

A fonte oficial é o método `CriarMateriais()` no `Form1.cs`. Se ele mudar, esta tabela deve mudar junto.

## 10. Como trabalhar com Git

- Crie a sua branch a partir da `main` (ou da `feature/paralelismo`, se ainda não tiver sido mesclada): `feature/banco-mysql`.
- Mexa só nos arquivos do banco (`Persistencia/` e o script SQL) e na **uma linha** do `Form1.cs` descrita na seção 7. Quem cuida do simulador e da tela é o outro integrante; assim não há conflito.
- Coloque o script SQL no repositório (por exemplo, `docs/banco.sql`) para qualquer pessoa conseguir criar as tabelas.
- Abra Pull Requests pequenos e avise o grupo.
- Se um arquivo novo criado fora do Visual Studio causar erro de "tipo não encontrado", clique com o botão direito nele no Gerenciador de Soluções e escolha **Incluir no Projeto**.
- Lembre que o trabalho é avaliado também pelos **comentários no código**: comente o porquê das decisões (por que lote, por que fila, por que essas chaves).

## 11. Checklist

- [ ] Script SQL das tabelas no repositório
- [ ] `RepositorioMySql` implementando as cinco operações do contrato
- [ ] Gravação em lote dentro de transação
- [ ] (Opcional) fila com thread gravadora e limite de tamanho
- [ ] Senha fora do Git (variável de ambiente ou arquivo no `.gitignore`)
- [ ] `FinalizarSimulacao` descarrega tudo que estiver pendente
- [ ] Teste de gravar, ler de volta e comparar
- [ ] Teste de troca de material no meio da simulação
- [ ] Tempos de gravação medidos para o relatório
- [ ] Linha do `Form1.cs` trocada para usar o `RepositorioMySql`

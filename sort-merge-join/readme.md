A coleta de métricas não está correta, mas a separação dos registros, geração de arquivos intermediários,
e resultado final estão corretos. Por gentileza, considere os arquivos intermediários gerados e o arquivo final, além das métricas do output.
# Projeto de Implementação do Sort-Merge Join em C#

Este projeto é uma implementação acadêmica do clássico algoritmo de junção de banco de dados conhecido como **Sort-Merge Join**. Ele foi projetado para realizar a junção (JOIN) entre duas tabelas (representadas por arquivos `.csv`) que, potencialmente, são grandes demais para serem carregadas inteiramente na memória.

## Visão Geral

O programa recebe dois arquivos CSV, `Tabela1` e `Tabela2`, e as respectivas colunas-chave para a junção. O objetivo é produzir um terceiro arquivo CSV contendo o resultado da junção de `Tabela1` e `Tabela2`, onde os valores das colunas-chave são iguais (um equi-join).

Para lidar com a restrição de memória, o programa não lê os arquivos inteiros de uma vez. Em vez disso, ele implementa uma estratégia de **ordenação externa**, processando os arquivos em pedaços, ordenando-os em etapas e, finalmente, juntando os resultados pré-ordenados.

## Como o Algoritmo Funciona

O processo é dividido em três fases principais, executadas pela classe `Operator`. Pense nela como o maestro dessa orquestra de dados.

### Fase 1: Geração de "Runs" Ordenadas (A Fase de Ordenação)

O ditado "dividir para conquistar" é levado a sério aqui. Como não podemos ordenar o arquivo todo de uma vez, nós o ordenamos em partes.

1.  **Leitura em Pedaços**: O programa lê um bloco de dados da `Tabela1` (atualmente configurado para 4 "páginas" de 10 "tuplas" cada, totalizando 40 tuplas) para a memória usando o `DiskIterator`.
2.  **Ordenação em Memória**: Este bloco de 40 tuplas, agora em memória, é ordenado com base na coluna-chave especificada. A ordenação usa o método `List.Sort()` com uma função de comparação customizada (`AuxClass.SortTuples`).
3.  **Escrita da "Run"**: O bloco ordenado, chamado de "run", é escrito em um arquivo temporário no disco (ex: `run_0_1.txt`).
4.  **Repetição**: O processo é repetido. O programa libera a memória (`Table.FreeFromMemo()`), lê as próximas 40 tuplas do arquivo original, ordena-as e as salva em um novo arquivo de run (ex: `run_0_2.txt`).

Isso continua até que toda a `Tabela1` tenha sido lida e transformada em uma série de pequenos arquivos de "runs", cada um deles internamente ordenado. O mesmo processo é então repetido para a `Tabela2`, criando um conjunto separado de runs ordenadas para ela.

### Fase 2: Intercalação das Runs (A Fase de Merge)

Agora temos um monte de arquivos pequenos e ordenados. O próximo passo é juntá-los em um único arquivo grande e perfeitamente ordenado para cada tabela original.

1.  **Merge em K-vias**: O algoritmo realiza um "merge" (intercalação) em múltiplos passos. Ele pega um número fixo de runs (atualmente 3, um "3-way merge") e as intercala em uma nova run, maior e ainda ordenada.
2.  **Lógica da Intercalação**: Para intercalar os arquivos, o programa lê a primeira linha de cada um, seleciona a linha com a menor chave, escreve-a no arquivo de saída e avança a leitura *apenas* no arquivo de onde a linha foi retirada. Esse processo se repete até que todos os arquivos de entrada tenham sido consumidos.
3.  **Níveis de Merge**: Esse processo cria um novo "nível" de runs (ex: `run_1_1.txt`). Se ainda houver mais de uma run, o processo se repete, intercalando as runs do nível 1 para criar as do nível 2, e assim por diante. É como um torneio onde os vencedores (arquivos maiores e ordenados) continuam avançando.
4.  **Resultado Final da Fase**: Ao final, para cada tabela original, restará apenas um único arquivo (ex: `T1_sorted.txt` e `T2_sorted.txt`), contendo todas as tuplas da tabela original, agora completamente ordenadas pela chave de junção.

### Fase 3: Junção dos Arquivos Ordenados (A Fase de Join)

Com os dois arquivos gigantes e ordenados em mãos, a junção final se torna surpreendentemente eficiente.

1.  **Ponteiros de Leitura**: O algoritmo mantém um "ponteiro" (lendo linha a linha) para `T1_sorted.txt` e outro para `T2_sorted.txt`.
2.  **Comparação das Chaves**:
    * Se `chave_t1 < chave_t2`, significa que a tupla atual de T1 não terá correspondente em T2 (pois T2 já está em uma chave maior). O programa avança o ponteiro de T1.
    * Se `chave_t2 < chave_t1`, o inverso acontece. O programa avança o ponteiro de T2.
    * Se `chave_t1 == chave_t2`, **temos uma correspondência!**
3.  **Tratamento de Duplicatas**: Quando uma correspondência é encontrada, o algoritmo é inteligente o suficiente para lidar com chaves duplicadas. Ele lê e armazena *todas* as tuplas consecutivas com a mesma chave de `T1` em uma lista e faz o mesmo para `T2`. Em seguida, ele realiza o produto cartesiano entre essas duas listas, juntando cada tupla de uma lista com cada tupla da outra e escrevendo o resultado no arquivo de saída final (`join_result.txt`).
4.  **Fim do Processo**: A junção continua até que um dos dois arquivos seja lido por completo.

## Estrutura do Projeto

* `Program.cs`: O ponto de entrada da aplicação. É aqui que você define quais tabelas (`uva.csv`, `pais.csv`, etc.) e quais colunas-chave serão usadas na operação.
* `Operator.cs`: O cérebro da operação. Orquestra as três fases (Sort, Merge, Join) e gerencia os diretórios temporários e de saída.
* `Table.cs`: Uma representação em memória de um pedaço da tabela (uma "run"). Contém a lista de tuplas e métodos para ordená-las e limpá-las da memória.
* `DiskIterator.cs`: Responsável pela leitura dos arquivos `.csv` do disco em blocos (páginas).
* `AuxClass.cs`: Uma classe de utilitários estáticos. Contém a lógica de comparação para a ordenação (`SortTuples`) e, crucialmente, o método `SetComparisonIndex` que mapeia nomes de colunas para seus índices numéricos.
* `PageLoadReference.cs`: Um objeto simples para transportar os dados lidos do disco (as tuplas, contadores e um sinalizador de fim de arquivo) entre o `DiskIterator` e o `Operator`.
* `StringfiedTable.cs`: Outro objeto simples para transportar o resultado da formatação das tuplas em string, junto com contadores de páginas/tuplas escritas.
* `SortMergeAlgorithm.cs`: **Este arquivo parece não ser utilizado no fluxo atual do programa.** Sua classe é instanciada, mas seu método `Merge` está vazio e nunca é chamado.

## Como Usar

1.  **Configurar Diretórios**: Na classe `Operator.cs`, a variável `rootDirectory` está hardcoded. Ajuste o caminho para um diretório válido no seu sistema (`C:\temp\sort_merge_join\` para Windows ou `/home/vitor/tmp/sort_merge_join/` para Linux). O programa criará subpastas para cada execução.
2.  **Posicionar os Arquivos**: Coloque seus arquivos `.csv` (ex: `uva.csv`, `pais.csv`) no diretório de execução do projeto (geralmente `bin/Debug/netX.X`). O `DiskIterator` procura os arquivos usando `Directory.GetCurrentDirectory()`.
3.  **Definir a Operação**: Em `Program.cs`, modifique a linha `new Operator(...)` para especificar os nomes dos arquivos e das colunas-chave que você deseja juntar.
4.  **Executar**: Compile e execute o projeto. O progresso será exibido no console e o resultado final estará no arquivo `join_result.txt` dentro da pasta de execução criada.
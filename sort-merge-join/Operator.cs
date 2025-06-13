using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace sort_merge_join;

class Operator
{
    static bool isLinux = false;
    private string rootDirectory { get; set; } = isLinux ? @"/home/vitor/tmp/sort_merge_join/" : @"C:\temp\sort_merge_join\"; //Windows
    private string RunT1Directory { get; set; }
    private string RunT2Directory { get; set; }
    private string SortedDirectory { get; set; }
    
    private int keyIndexT1;
    private int keyIndexT2;

    public Table Table1 { get; set; }
    public Table Table2 { get; set; }
    public string KeyOnTable1 { get; set; }
    public string KeyOnTable2 { get; set; }


    public Operator(Table table1, Table table2, string keyOnTable1, string keyOnTable2)
    {
        //Cria a pasta da execução atual -> Isso será movido para o operador.
        string executionDataDirectory = DateTime.Now.ToString(new CultureInfo("de-DE")).Replace(" ", "-").Replace(":", ".");
        RunT1Directory = Path.Combine(rootDirectory, executionDataDirectory, "runsT1");
        RunT2Directory = Path.Combine(rootDirectory, executionDataDirectory, "runsT2");
        SortedDirectory = Path.Combine(rootDirectory, executionDataDirectory, "sorted-files");

        try
        {
            Console.WriteLine("Criando pastas para a operação...");
            Directory.CreateDirectory(Path.Combine(rootDirectory + executionDataDirectory));
            Directory.CreateDirectory(RunT1Directory);
            Directory.CreateDirectory(RunT2Directory);
            Directory.CreateDirectory(SortedDirectory);
        }
        catch (Exception e)
        {
            throw new Exception("Erro ao criar o diretório com dados de execução.");
            return;
        }

        Table1 = table1;
        Table2 = table2;
        KeyOnTable1 = keyOnTable1;
        KeyOnTable2 = keyOnTable2;
        
        AuxClass.SetComparisonIndex(table1.Name, keyOnTable1);
        this.keyIndexT1 = AuxClass.ComparisonIndex;

        AuxClass.SetComparisonIndex(table2.Name, keyOnTable2);
        this.keyIndexT2 = AuxClass.ComparisonIndex;
    }

    public void DoOperation()
    {
        //Chamar a leitura da primeira tabela,
        //ordená-la e armazenar em disco
        //repetir o processo para a segunda tabela
        //mesclar as duas ordenadas em um arquivo de saida

        string table_name = Table1.Name;
        string column_name = KeyOnTable1;
        string result = "";
        string lineAux = "";
        int writtenTupleCounter = 0;
        int writtenPageCounter = 0;
        int readTupleCounter = 0;
        int readPageCounter = 0;
        int run_counter = 0;
        StringfiedTable AuxStructure;

        Console.WriteLine($"*---Ordenando tabela 1: {table_name}---*");

        //*Lê tabela 1 para a memória
        DiskIterator iterator = new DiskIterator();
        PageLoadReference reference = iterator.ReadToMemo(this.Table1, 0);

        Table1 = reference.Table;
        readPageCounter += reference.PageCounter; //Atualiza a quantidade de paginas lidas do disco
        readTupleCounter = reference.TupleCounter;
        Console.WriteLine($"[PARCIAL] Quantidade de páginas LIDAS na última ida ao disco: {readPageCounter}");
        Console.WriteLine($"[PARCIAL] Quantidade de tuplas LIDAS na última ida ao disco {1}: {readPageCounter * 10 + readTupleCounter}");


        //Se vazio, interrompe.
        if (Table1.Tuples.Count == 0)
        {
            Console.WriteLine("Tabela vazia para a seleção");
            return;
        }

        //Lê e ordena iterativamente a tabela.
        while (!reference.IsEndOfFile)
        {
            run_counter++;

            //Ordena as p´aginas em memoria
            Table1.Sort(column_name);

            //Monta a string com o conjunto de tuplas ordenadas
            AuxStructure = AuxClass.GetSortedString(Table1.Tuples);
            result = AuxStructure.Result;
            writtenPageCounter += AuxStructure.WrittenPageCounter;
            writtenTupleCounter = AuxStructure.WrittenTupleCounter;

            Console.WriteLine(
                $"[PARCIAL] Quantidade de tuplas ESCRITAS na última run {run_counter}: {writtenPageCounter * 10 + writtenTupleCounter}");

            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(Path.Combine(RunT1Directory, $"run_0_{run_counter}.txt"), result);
            result = "";

            Table1.FreeFromMemo(); //Garante que não se contamine com tuplas da run passada
            reference = iterator.ReadToMemo(Table1, writtenPageCounter);

            //Atualiza as metricas de leitura
            Table1 = reference.Table;
            readPageCounter += reference.PageCounter;
            readTupleCounter = reference.TupleCounter;
            Console.WriteLine($"[PARCIAL] Quantidade acumulada de páginas LIDAS com a última run {run_counter}: {readPageCounter}");
            Console.WriteLine($"[PARCIAL] Quantidade de tuplas LIDAS na última página da run {run_counter}: {readTupleCounter}");
        }

        //Realiza a ordenação do último bloco lido até o fim do arquivo - codigo repetido, corrigir
        if (Table1.Tuples.Count != 0)
        {
            run_counter++;

            //Ordena as p´aginas em memoria
            Table1.Sort(column_name);

            //Monta a string com o conjunto de tuplas ordenadas
            AuxStructure = AuxClass.GetSortedString(Table1.Tuples);
            result = AuxStructure.Result;
            writtenPageCounter += AuxStructure.WrittenPageCounter;
            writtenTupleCounter = AuxStructure.WrittenTupleCounter;
            Console.WriteLine($"[PARCIAL] Quantidade de tuplas escritas na última página da run{run_counter}: {writtenTupleCounter}");


            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(Path.Combine(RunT1Directory, $"run_0_{run_counter}.txt"), result);
            result = "";
            Table1.FreeFromMemo();
        }

        Console.WriteLine("\n|---Resumo da ordenação da tabela 1---|");
        Console.WriteLine($"[FINAL] Quantidade de páginas(cheias) ESCRITAS na operação: {writtenPageCounter}");
        Console.WriteLine($"[FINAL] Quantidade de tuplas ESCRITAS na última página(não cheia): {writtenTupleCounter}");
        Console.WriteLine($"[FINAL] Quantidade total tuplas ESCRITAS na operação: {writtenPageCounter * 10 + writtenTupleCounter}");
        Console.WriteLine($"[FINAL] Quantidade total páginas LIDAS na operação: {readPageCounter}"); //Confusão de nomenclatura e gerou essa nóia de ser sempre a quantidade de páginas cheia + o quebrado das tuplas. Aqui tem que ter mais 1.
        Console.WriteLine($"[FINAL] Quantidade total tuplas LIDAS na operação: {readPageCounter * 10 + readTupleCounter}");
        Console.WriteLine($"*---Final das runs da tabela 1---*");

        //Merge
        Console.WriteLine("\n*---Iniciando merge para a Tabela 1---*");

        string finalOutputFile = Path.Combine(SortedDirectory, "T1_sorted.txt");
        MergeRuns(true);

        Console.WriteLine($"*---Merge da Tabela 1 finalizado.---*");
        
        
        /*--- tabela 2 ---*/
        
        table_name = Table2.Name;
        column_name = KeyOnTable2;
        result = "";
        lineAux = "";
        writtenTupleCounter = 0;
        writtenPageCounter = 0;
        readTupleCounter = 0;
        readPageCounter = 0;
        run_counter = 0;

        Console.WriteLine($"*---Ordenando tabela 2: {table_name}---*");

        //*Lê tabela 1 para a memória
        iterator = new DiskIterator();
        reference = iterator.ReadToMemo(this.Table2, 0);

        Table2 = reference.Table;
        readPageCounter += reference.PageCounter; //Atualiza a quantidade de paginas lidas do disco
        readTupleCounter = reference.TupleCounter;
        Console.WriteLine($"[PARCIAL] Quantidade de páginas LIDAS na última ida ao disco: {readPageCounter}");
        Console.WriteLine($"[PARCIAL] Quantidade de tuplas LIDAS na última ida ao disco {1}: {readPageCounter * 10 + readTupleCounter}");


        //Se vazio, interrompe.
        if (Table2.Tuples.Count == 0)
        {
            Console.WriteLine("Tabela vazia para a seleção");
            return;
        }

        //Lê e ordena iterativamente a tabela.
        while (!reference.IsEndOfFile)
        {
            run_counter++;

            //Ordena as p´aginas em memoria
            Table2.Sort(column_name);

            //Monta a string com o conjunto de tuplas ordenadas
            AuxStructure = AuxClass.GetSortedString(Table2.Tuples);
            result = AuxStructure.Result;
            writtenPageCounter += AuxStructure.WrittenPageCounter;
            writtenTupleCounter = AuxStructure.WrittenTupleCounter;

            Console.WriteLine(
                $"[PARCIAL] Quantidade de tuplas ESCRITAS na última run {run_counter}: {writtenPageCounter * 10 + writtenTupleCounter}");

            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(Path.Combine(RunT2Directory, $"run_0_{run_counter}.txt"), result);
            result = "";

            Table2.FreeFromMemo(); //Garante que não se contamine com tuplas da run passada
            reference = iterator.ReadToMemo(Table2, writtenPageCounter);

            //Atualiza as metricas de leitura
            Table2 = reference.Table;
            readPageCounter += reference.PageCounter;
            readTupleCounter = reference.TupleCounter;
            Console.WriteLine($"[PARCIAL] Quantidade acumulada de páginas LIDAS com a última run {run_counter}: {readPageCounter}");
            Console.WriteLine($"[PARCIAL] Quantidade de tuplas LIDAS na última página da run {run_counter}: {readTupleCounter}");
        }

        //Realiza a ordenação do último bloco lido até o fim do arquivo - codigo repetido, corrigir
        if (Table2.Tuples.Count != 0)
        {
            run_counter++;

            //Ordena as p´aginas em memoria
            Table2.Sort(column_name);

            //Monta a string com o conjunto de tuplas ordenadas
            AuxStructure = AuxClass.GetSortedString(Table2.Tuples);
            result = AuxStructure.Result;
            writtenPageCounter += AuxStructure.WrittenPageCounter;
            writtenTupleCounter = AuxStructure.WrittenTupleCounter;
            Console.WriteLine($"[PARCIAL] Quantidade de tuplas escritas na última página da run{run_counter}: {writtenTupleCounter}");


            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(Path.Combine(RunT2Directory, $"run_0_{run_counter}.txt"), result);
            result = "";
            Table1.FreeFromMemo();
        }

        Console.WriteLine("\n|---Resumo da ordenação da tabela 2---|");
        Console.WriteLine($"[FINAL] Quantidade de páginas(cheias) ESCRITAS na operação: {writtenPageCounter}");
        Console.WriteLine($"[FINAL] Quantidade de tuplas ESCRITAS na última página(não cheia): {writtenTupleCounter}");
        Console.WriteLine($"[FINAL] Quantidade total tuplas ESCRITAS na operação: {writtenPageCounter * 10 + writtenTupleCounter}");
        Console.WriteLine($"[FINAL] Quantidade total páginas LIDAS na operação: {readPageCounter}"); //Confusão de nomenclatura e gerou essa nóia de ser sempre a quantidade de páginas cheia + o quebrado das tuplas. Aqui tem que ter mais 1.
        Console.WriteLine($"[FINAL] Quantidade total tuplas LIDAS na operação: {readPageCounter * 10 + readTupleCounter}");
        Console.WriteLine($"*---Final das runs da tabela 2---*");

        //Merge
        Console.WriteLine("\n*---Iniciando merge para a Tabela 2 ---*");

        finalOutputFile = Path.Combine(SortedDirectory, "T2_sorted.txt");
        MergeRuns(false);

        Console.WriteLine($"*---Merge da Tabela 2 finalizado.---*");

        JoinSortedFiles();
    }

    public void MergeRuns(bool isTable1)
    {
        string runDirectory = isTable1 ? RunT1Directory : RunT2Directory;
        string finalOutputFile = Path.Combine(SortedDirectory, isTable1 ? "T1_sorted.txt" : "T2_sorted.txt");

        //Define o índice de comparação global ANTES de iniciar o merge.
        if (isTable1)
        {
            AuxClass.SetComparisonIndex(this.Table1.Name, this.KeyOnTable1);
        }
        else
        {
            AuxClass.SetComparisonIndex(this.Table2.Name, this.KeyOnTable2);
        }

        int level = 0;
        var currentLevelFiles = Directory.GetFiles(runDirectory, "run_0_*.txt").ToList(); //quantidade de runs

        while (currentLevelFiles.Count > 1)
        {
            Console.WriteLine($"\n--- INICIANDO MERGE - NÍVEL {level} ---");
            Console.WriteLine($"Arquivos a processar: {currentLevelFiles.Count}");

            var nextLevelFiles = new List<string>();
            int runCounterNextLevel = 1;
            level++;

            for (int i = 0; i < currentLevelFiles.Count; i += 3)
            {
                string[] filesToMerge = currentLevelFiles.Skip(i).Take(3).ToArray();
                string outputFile = Path.Combine(runDirectory, $"run_{level}_{runCounterNextLevel}.txt");

                Console.WriteLine($" -> Mesclando {filesToMerge.Length} arquivos para -> run_{level}_{runCounterNextLevel}.txt");

                // A chamada para a função interna agora não passa mais o índice.
                MergeKRunsInternal(filesToMerge, outputFile);

                nextLevelFiles.Add(outputFile);
                runCounterNextLevel++;
            }

            currentLevelFiles = nextLevelFiles;
        }

        if (currentLevelFiles.Any())
        {
            File.Move(currentLevelFiles.First(), finalOutputFile, true);
            Console.WriteLine($"\n*--- Merge finalizado! Arquivo de saída: {finalOutputFile} ---*");
        }
    }

    /// Função interna que intercala k arquivos.
    /// <param name="inputFiles">Caminhos dos arquivos de entrada.</param>
    /// <param name="outputFile">Caminho do arquivo de saída.</param>
    private void MergeKRunsInternal(string[] inputFiles, string outputFile)
    {
        int k = inputFiles.Length;
        var readers = new StreamReader[k];
        var currentTuples = new string[k][];
        var outputBuffer = new List<string[]>(10);

        try
        {
            for (int i = 0; i < k; i++)
            {
                readers[i] = new StreamReader(inputFiles[i]);
                string line = readers[i].ReadLine();
                currentTuples[i] = line?.Split(',');
            }

            using (var writer = new StreamWriter(outputFile))
            {
                while (currentTuples.Any(tuple => tuple != null))
                {
                    int smallestTupleIndex = -1;
                    int smallestKey = int.MaxValue;

                    for (int i = 0; i < k; i++)
                    {
                        if (currentTuples[i] != null)
                        {
                            // A MUDANÇA ESTÁ AQUI: Usa AuxClass.ComparisonIndex em vez do parâmetro 'keyIndex'
                            int currentKey = int.Parse(currentTuples[i][AuxClass.ComparisonIndex]);
                            if (currentKey < smallestKey)
                            {
                                smallestKey = currentKey;
                                smallestTupleIndex = i;
                            }
                        }
                    }

                    outputBuffer.Add(currentTuples[smallestTupleIndex]);

                    if (outputBuffer.Count == 10)
                    {
                        foreach (var tuple in outputBuffer)
                        {
                            // Usando string.Join por ser mais direto para esta tarefa específica.
                            // A formatação final deve ser idêntica à do seu método GetSortedString.
                            writer.WriteLine(string.Join(",", tuple));
                        }

                        outputBuffer.Clear();
                    }

                    string nextLine = readers[smallestTupleIndex].ReadLine();
                    // O Split agora usa ',' como delimitador único, o que parece mais correto
                    // que a combinação de ", " usada em GetSortedString. Ajuste se necessário.
                    currentTuples[smallestTupleIndex] = nextLine?.Split(',');
                }

                if (outputBuffer.Any())
                {
                    foreach (var tuple in outputBuffer)
                    {
                        writer.WriteLine(string.Join(",", tuple));
                    }
                }
            }
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader?.Close();
            }
        }
    }
    

    /// Executa a junção final. 
    /// pois o construtor define keyIndexT1 e keyIndexT2 corretamente.
    public void JoinSortedFiles()
    {
        Console.WriteLine("\n*--- Iniciando a junção final dos arquivos ordenados ---*");
        string sortedFile1 = Path.Combine(SortedDirectory, "T1_sorted.txt");
        string sortedFile2 = Path.Combine(SortedDirectory, "T2_sorted.txt");
        string outputFile = Path.Combine(SortedDirectory, "join_result.txt");

        if (!File.Exists(sortedFile1) || !File.Exists(sortedFile2))
        {
            Console.WriteLine("ERRO: Um ou ambos os arquivos ordenados não foram encontrados. Execute o merge primeiro.");
            return;
        }

        long joinCount = 0;

        using (StreamReader reader1 = new StreamReader(sortedFile1))
        using (StreamReader reader2 = new StreamReader(sortedFile2))
        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            string line1 = reader1.ReadLine();
            string line2 = reader2.ReadLine();

            while (line1 != null && line2 != null)
            {
                var tuple1 = line1.Split(',');
                var tuple2 = line2.Split(',');

                // Usa as variáveis de instância que foram corretamente inicializadas
                int key1 = int.Parse(tuple1[this.keyIndexT1]);
                int key2 = int.Parse(tuple2[this.keyIndexT2]);

                if (key1 < key2)
                {
                    line1 = reader1.ReadLine();
                }
                else if (key2 < key1)
                {
                    line2 = reader2.ReadLine();
                }
                else
                {
                    int matchingKey = key1;
                    var groupT1 = new List<string[]>();
                    var groupT2 = new List<string[]>();
                    
                    while (line1 != null && int.Parse(line1.Split(',')[this.keyIndexT1]) == matchingKey)
                    {
                        groupT1.Add(line1.Split(','));
                        line1 = reader1.ReadLine();
                    }
                    
                    while (line2 != null && int.Parse(line2.Split(',')[this.keyIndexT2]) == matchingKey)
                    {
                        groupT2.Add(line2.Split(','));
                        line2 = reader2.ReadLine();
                    }

                    foreach (var t1 in groupT1)
                    {
                        foreach (var t2 in groupT2)
                        {
                            writer.WriteLine(string.Join(",", t1) + "," + string.Join(",", t2));
                            joinCount++;
                        }
                    }
                }
            }
        }
        
        Console.WriteLine($"\n*--- Junção finalizada! ---*");
        Console.WriteLine($"Total de tuplas unidas: {joinCount}");
        Console.WriteLine($"Resultado salvo em: {outputFile}");
    }
}
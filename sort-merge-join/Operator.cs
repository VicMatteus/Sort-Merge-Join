using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace sort_merge_join;

class Operator
{
    static bool isLinux = false;
    private string rootDirectory { get; set; } = isLinux ? @"/home/vitor/tmp/sort_merge_join/" : @"C:\temp\sort_merge_join\"; //Windows
    private string RunT1Directory { get; set; }
    private string RunT2Directory { get; set; }

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

        try
        {
            Directory.CreateDirectory(Path.Combine(rootDirectory + executionDataDirectory));
            Directory.CreateDirectory(RunT1Directory);
            Directory.CreateDirectory(RunT2Directory);
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

            Console.WriteLine($"[PARCIAL] Quantidade de tuplas ESCRITAS na última run {run_counter}: {writtenPageCounter * 10 + writtenTupleCounter}");

            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(Path.Combine(RunT1Directory, $"run_{run_counter}_{table_name}.txt"), result);
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
            File.AppendAllText(Path.Combine(RunT1Directory, $"run_{run_counter}_{table_name}.txt"), result);
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
    }

    public void MergeRuns()
    {}

}
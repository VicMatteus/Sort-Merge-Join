using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace sort_merge_join;

class Operator
{
    static bool isLinux = true;
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
        string executionDataDirectory = DateTime.Now.ToString(new CultureInfo("de-DE")).Replace(" ", "-")+"/";
        RunT1Directory = rootDirectory + executionDataDirectory + "runsT1/";
        RunT2Directory = rootDirectory + executionDataDirectory + "runsT2/";

        try
        {
            Directory.CreateDirectory(rootDirectory + executionDataDirectory);
            Directory.CreateDirectory(RunT1Directory);
            Directory.CreateDirectory(RunT2Directory);
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro ao criar o diretório com dados de execução.");
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
        int run_counter = 0;

        //*Lê tabela 1 para a memória
        DiskIterator iterator = new DiskIterator();
        PageLoadReference reference = iterator.ReadToMemo(this.Table1, 0);
        Table1 = reference.Table;

        //Se vazio, interrompe.
        if (Table1.Tuples.Count == 0)
        {
            Console.WriteLine("Tabela vazia para a seleção");
            return;
        }

        //Lê e ordena iterativamente a tabela.
        while (!reference.IsEndOfFile)
        {

            //Ordena as paginas em memória e devolve em Table1.Tuples
            AuxClass.SetComparisonIndex(table_name, column_name);
            Table1.Tuples.Sort(AuxClass.SortTuples);

            run_counter++;
            //Escreve a run atual no arquivo temporário
            foreach (Array tuple in Table1.Tuples)
            {
                //Concatena as colunas
                for (int i = 0; i < tuple.Length - 1; i++)
                {
                    lineAux += tuple.GetValue(i) + ", ";
                }
                lineAux += tuple.GetValue(tuple.Length - 1);
                result += lineAux + "\n"; // salva a tupla pronta em uma só string
                lineAux = "";

                writtenTupleCounter++;
                if (writtenTupleCounter >= 10)
                {
                    writtenPageCounter++;
                    writtenTupleCounter = 0;
                }
            }
            // Console.WriteLine(result);
            Console.WriteLine($"Quantidade de paginas escritas nesta run: {writtenPageCounter}");
            Console.WriteLine($"Quantidade de tuplas escritas nesta run: {writtenPageCounter * 10 + writtenTupleCounter}");

            //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
            File.AppendAllText(RunT1Directory + $"run_{run_counter}_{table_name}.txt", result);
            result = "";

            reference = iterator.ReadToMemo(Table1, writtenPageCounter);
        }

        //Realiza a ordenação do último bloco lido até o fim do arquivo
        // if (reference.Tuples.Count != 0)
        // {
        //     run_counter++;

        //     //Passo a tabela e a coluna da junção para realizar a ordenação baseado nela.
        //     AuxClass.SetComparisonIndex(table_name, column_name);
        //     reference.Tuples.Sort(AuxClass.SortTuples);
        //     foreach (Array tuple in reference.Tuples)
        //     {
        //         //Concatena as colunas
        //         for (int i = 0; i < tuple.Length - 1; i++)
        //         {
        //             lineAux += tuple.GetValue(i) + ", ";
        //         }
        //         lineAux += tuple.GetValue(tuple.Length - 1);
        //         result += lineAux + "\n"; // salva a tupla pronta em uma só string
        //         lineAux = "";

        //         writtenTupleCounter++;
        //         if (writtenTupleCounter >= 10)
        //         {
        //             writtenPageCounter++;
        //             writtenTupleCounter = 0;
        //         }
        //     }
        //     Console.WriteLine(result);

        //     //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado, separado em blocos de tuplas, onde cada bloco = 1 página.
        //     File.AppendAllText(RunT2Directory + $"run_{run_counter}.txt", result);
        // }
    }

    // public Array GetSortedString(Array tuples)
    // {
    //     string lineAux = "";
    //     string result = "";
    //     int writtenPageCounter = 0;
    //     int writtenTupleCounter = 0;

    //     foreach (Array tuple in Table1.Tuples)
    //     {
    //         //Concatena as colunas
    //         for (int i = 0; i < tuple.Length - 1; i++)
    //         {
    //             lineAux += tuple.GetValue(i) + ", ";
    //         }
    //         lineAux += tuple.GetValue(tuple.Length - 1);
    //         result += lineAux + "\n"; // salva a tupla pronta em uma só string
    //         lineAux = "";

    //         writtenTupleCounter++;
    //         if (writtenTupleCounter >= 10)
    //         {
    //             writtenPageCounter++;
    //             writtenTupleCounter = 0;
    //         }
    //     }
    //     return [result, writtenPageCounter, writtenTupleCounter];
    // }
}
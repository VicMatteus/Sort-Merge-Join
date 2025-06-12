// See https://aka.ms/new-console-template for more information

using System.Globalization;

namespace sort_merge_join
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Comentario com coisas usadas entre o dev no win e no linux
            //File.Exists(Directory.GetCurrentDirectory() + "/uva.csv")

            bool isLinux = true;
            Operator op = new Operator(new Table("vinho.csv"), new Table("uva.csv"), "uva_id", "uva_id");
            op.DoOperation();

            /*
            string table_name = "vinho.csv";
            string column_name = "pais_producao_id";
            string run_file_path = isLinux ? @"/home/vitor/tmp/sort_merge_join/" : @"C:\temp\sort_merge_join\"; //Windows
            string result = "";
            string lineAux = "";
            int writtenTupleCounter = 0;
            int writtenPageCounter = 0;
            int run_counter = 0;

            //*Lê uma tabela para a memória
            DiskIterator iterator = new DiskIterator();
            PageLoadReference reference = iterator.ReadToMemo(table_name, 0);

            //Se vazio, interrompe.
            if (reference.Tuples.Count == 0)
            {
                Console.WriteLine("Tabela vazia para a seleção");
                return;
            }

            //Lê e ordena iterativamente a tabela.
            while (!reference.IsEndOfFile)
            {
                run_counter++;

                //Passo a tabela e a coluna da junção para realizar a ordenação baseado nela.
                AuxClass.SetComparisonIndex(table_name, column_name);
                reference.Tuples.Sort(AuxClass.SortTuples);
                foreach (Array tuple in reference.Tuples)
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
                Console.WriteLine(result);

                //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado(até 40 tuplas) - run_N_tabela.txt
                File.AppendAllText(run_file_path + $"run_{run_counter}_{table_name}.txt", result);
                result = "";

                reference = iterator.ReadToMemo(table_name, writtenPageCounter);
            }

            //Realiza a ordenação do último bloco lido até o fim do arquivo
            if (reference.Tuples.Count != 0)
            {
                run_counter++;

                //Passo a tabela e a coluna da junção para realizar a ordenação baseado nela.
                AuxClass.SetComparisonIndex(table_name, column_name);
                reference.Tuples.Sort(AuxClass.SortTuples);
                foreach (Array tuple in reference.Tuples)
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
                Console.WriteLine(result);

                //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado, separado em blocos de tuplas, onde cada bloco = 1 página.
                File.AppendAllText(run_file_path + $"run_{run_counter}.txt", result);
            }
            */
        }
    }
}
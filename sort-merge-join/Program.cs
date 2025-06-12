// See https://aka.ms/new-console-template for more information

namespace sort_merge_join
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Comentario com coisas usadas entre o dev no win e no linux
            //File.Exists(Directory.GetCurrentDirectory() + "/uva.csv")

            bool isLinux = true;

            string table_name = "uva.csv";
            string column_name = "pais_origem_id";
            string run_file_path    = isLinux ? @"/home/vitor/tmp/sort_merge_join/" : @"C:\temp\sort_merge_join\"; //Windows
            string result = "";
            string lineAux = "";
            int writtenTupleCounter = 0;
            int writtenPageCounter = 0;
            int run_counter = 0;

            //*Lê uma tabela para a memória
            DiskIterator iterator = new DiskIterator();

            //Lê um máximo de tuplas para a memória
            PageLoadReference reference = iterator.ReadToMemo(table_name, 0);

            // Problema: não irá montar o resultado caso acabe na primeira leitura
            // e não irá mostrar o último resultado caso acabe dentro do laço

            //Quando ler um end of file, é um indictivo para ler realizar mais uma iteração 
            if (reference.Tuples.Count == 0)
            {
                Console.WriteLine("Tabela vazia para a seleção");
                return;
            }


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

                //Escreve as páginas que leu(4, no máximo) em um arquivo temporário ordenado, separado em blocos de tuplas, onde cada bloco = 1 página.
                File.AppendAllText(run_file_path + $"run_{run_counter}.txt", result);
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
            
        }
    }
}
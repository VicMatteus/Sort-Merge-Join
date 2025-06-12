// See https://aka.ms/new-console-template for more information

namespace sort_merge_join
{
    public class Sort
    {
        public static int ComparisonIndex { get; set; } = 0;
        public static int SortTuples(Array x, Array y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int retval = Int32.Parse(x.GetValue(ComparisonIndex).ToString()).CompareTo(Int32.Parse(y.GetValue(ComparisonIndex).ToString()));

                    if (retval != 0)
                    {
                        return retval;
                    }
                    else
                    {

                        return 0;
                    }
                }
            }
        }
    }
    
    
    internal class Program
    {
        static void Main(string[] args)
        {
            //*Lê uma tabela para a memória
            DiskIterator iterator = new DiskIterator();
            
            //Lê um máximo de tuplas para a memória
            PageLoadReference reference = iterator.ReadToMemo("uva.csv");

            
            //Ordena esse conjunto - apenas por ora, deixarei a ordenação a cargo da função padrão
            Console.WriteLine("Pré ordenação!");
            foreach (Array tuple in reference.Tuples)
            {
                Console.WriteLine($"valor: {tuple.GetValue(4)}, {tuple.GetValue(1)}, {tuple.GetValue(2)}, {tuple.GetValue(3)}, {tuple.GetValue(0)} "); //uva
                // Console.WriteLine($"valor: {tuple.GetValue(0)}, {tuple.GetValue(1)}, {tuple.GetValue(2)} "); //Pais
            }
            Console.WriteLine("Pós ordenação!");
            Sort.ComparisonIndex = 4;
            reference.Tuples.Sort(Sort.SortTuples);
            foreach (Array tuple in reference.Tuples)
            {
                Console.WriteLine($"valor: {tuple.GetValue(4)}, {tuple.GetValue(1)}, {tuple.GetValue(2)}, {tuple.GetValue(3)}, {tuple.GetValue(0)} "); //uva
                // Console.WriteLine($"valor: {tuple.GetValue(0)}, {tuple.GetValue(1)}, {tuple.GetValue(2)} "); //Pais
            }
            
            //Escreve as páginas que leu em um arquivo temporário ordenado, separado em blocos de tuplas, onde cada bloco = 1 página.
            // File.AppendAllLines();

            //Repete processo de leitura da tabela

            //Acaba processo de leitura da tabela e começa a fazer o merge do arquivo 
        }
    }
}
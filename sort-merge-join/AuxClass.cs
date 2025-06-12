namespace sort_merge_join;

//Classe contendo apenas métodos gerais de auxilio
public class AuxClass
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
    public static void SetComparisonIndex(string table, string column)
    {
        if (table == "uva.csv")
        {
            if (column == "uva_id")
            {
                ComparisonIndex = 0;
            }
            else
                ComparisonIndex = 4;
        }
        else if (table == "vinho.csv")
        {
            if (column == "vinho_id")
            {
                ComparisonIndex = 0;
            }
            else if (column == "uva_id")
            {
                ComparisonIndex = 3;
            }
            else
            {
                ComparisonIndex = 4;
            }
        }
        else
        {
            ComparisonIndex = 0;
        }
    }

    //Dado um conjunto de tuplas (uma tabela em memoria), ele retorna a string formatada para ser salva em disco
    //a quantidade de tuplas escritas e a quantidade de páginas escritas.
    public static StringfiedTable GetSortedString(List<Array> tuples)
    {
        string lineAux = "";
        string result = "";
        int writtenPageCounter = 0;
        int writtenTupleCounter = 0;

        foreach (Array tuple in tuples)
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
        return new StringfiedTable(result, writtenPageCounter, writtenTupleCounter);
    }
}
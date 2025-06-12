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
            if (table == "uva.csv" )
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
}
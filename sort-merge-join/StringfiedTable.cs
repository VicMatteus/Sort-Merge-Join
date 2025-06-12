namespace sort_merge_join;

public class StringfiedTable
{
    public string Result { get; set; }
    public int WrittenPageCounter { get; set; }
    public int WrittenTupleCounter { get; set; }

    public StringfiedTable(string result, int pageCounter, int tupleCounter)
    {
        Result = result;
        WrittenPageCounter = pageCounter;
        WrittenTupleCounter = tupleCounter;
    }
}
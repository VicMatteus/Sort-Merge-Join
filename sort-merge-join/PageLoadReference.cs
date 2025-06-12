namespace sort_merge_join;

//Represents the abstraction of a disk reading reference. Contains the tuples, the number of pages read and the number of tuples on the last page.
public class PageLoadReference
{
    public List<Array> Tuples { get; set; } = new List<Array>();
    public int TupleCounter { get; set; }
    public int PageCounter { get; set; }
    public bool IsEndOfFile { get; set; } = false;

    public PageLoadReference(List<Array> tuples, int pageCounter, int tupleCounter, bool isEndOfFile)
    {
        TupleCounter = tupleCounter;
        PageCounter = pageCounter;
        Tuples = tuples;
        IsEndOfFile = isEndOfFile;
    }
}
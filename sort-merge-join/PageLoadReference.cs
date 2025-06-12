namespace sort_merge_join;

//Represents the abstraction of a disk reading reference. Contains the tuples, the number of pages read and the number of tuples on the last page.
public class PageLoadReference
{
    public List<Array> Tuples { get; set; } = new List<Array>();
    public int TupleCounter { get; set; }
    public int PageCounter { get; set; }

    public PageLoadReference(List<Array> tuples, int tupleCounter, int pageCounter)
    {
        TupleCounter = tupleCounter;
        PageCounter = pageCounter;
        Tuples = tuples;
    }
}
namespace sort_merge_join;

public class Page
{
    // public List<Tuple> Tuples { get; set; } = new List<Tuple>();
    public List<Array> Tuples { get; set; } = new List<Array>();
    public bool Sorted { get; set; } = false;
    public bool IsFull { get; set; } = false;

    //Ordena as tuplas atuais
    public void Sort()
    {

    }

    //Verifica se tem espaço na página e adiciona a tupla
    public bool AddTupple(Array tuple)
    {
        if (IsFull)
        {
            return false;
        }
        Tuples.Add(tuple);
        if (Tuples.Count >= 10)
        {
            IsFull = true;
        }
        return true;
    }
}
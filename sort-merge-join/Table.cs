namespace sort_merge_join;

public class Table
{
    public string Name { get; set; }
    public List<Array> Tuples { get; set; } = new List<Array>();
    public bool Sorted { get; set; } = false;
    public bool IsFull { get; set; } = false;
    public int PageCounter { get; set; } = 0;
    public int TupleCounter { get; set; } = 0;

    public Table(string tableName)
    {
        Name = tableName;
    }

    //Ordena as tuplas atuais
    public void Sort(string keyColumnName)
    {
        //Ordena as paginas em memória
        AuxClass.SetComparisonIndex(Name, keyColumnName);
        Tuples.Sort(AuxClass.SortTuples);
    }

    //Verifica se tem espaço na página e adiciona a tupla
    public bool AddTupple(Array tuple)
    {
        if (IsFull)
        {
            Console.WriteLine($"Estouro de memória: tentativa de carregar mais tuplas do que o limite em {Name}!");
            return false;
        }

        Tuples.Add(tuple);
        TupleCounter++;
        if (TupleCounter >= 10)
        {
            PageCounter++;
            TupleCounter = 0;
            if (PageCounter >= 4)
            {
                IsFull = true;
            }
        }
        return true;
    }

    public Table FreeFromMemo()
    {
        Tuples = new List<Array>();
        Sorted = false;
        IsFull = false;
        PageCounter = 0;
        TupleCounter = 0;
        return this;
    }

}
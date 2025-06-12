namespace sort_merge_join;

class Page
{
    public List<Tuple> Tuples { get; set; } = new List<Tuple>();
    public bool Sorted { get; set; } = false;

    //Ordena as tuplas atuais
    public void Sort()
    {
        
    }
}
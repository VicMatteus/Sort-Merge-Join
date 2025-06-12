namespace sort_merge_join;

class Table
{
    public string Name { get; set; }
    public List<Page> Pages{ get; set; } = new List<Page>();

    public Table(string tableName)
    {
        Name = tableName;
    }

}
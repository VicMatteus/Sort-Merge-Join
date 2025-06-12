namespace sort_merge_join;

public class DiskIterator
{
    public PageLoadReference ReadToMemo(string tableName)
    {
        string table_path  = Path.Combine(Directory.GetCurrentDirectory(), tableName);
            
        if (!File.Exists(table_path))
            throw new FileNotFoundException($"Arquivo de dados {tableName} não encontrado em {table_path}");

        //Nome da coluna que será a chave da seleção
        string keyColumnName = "pais_id";
            
        int tupleCounter = 0;
        int pageCounter = 0;
        string line = "";
        string[] partLine;
        List<Array> helper = new List<Array>();
            
        using StreamReader reader = new StreamReader(table_path);
            
        //Read the header
        line = reader.ReadLine(); 
        partLine = line.Split(",");
        int keyColumnIndex = Array.IndexOf(partLine, keyColumnName);
        //Look up for the key column index
            
        while ((line = reader.ReadLine()) != null)
        {
            if (tupleCounter < 10)
            {
                partLine = line.Split(',');
                helper.Add(partLine); //Adiciona a tupla na memória

                tupleCounter++;
            }
            else
            {
                //Leu uma página toda; Incrementa uma página e reseta o contador de tuplas;
                pageCounter++;
                tupleCounter = 0;

                if (pageCounter >= 4)
                {
                    break;
                }
            }
        }    
        //Ao chegar aqui, helper terá lido até 4 páginas, com 40 tuplas em si, ou estará vazio.
        return new PageLoadReference(helper, pageCounter, tupleCounter);
    }
}
﻿namespace sort_merge_join;

public class DiskIterator
{
    public PageLoadReference ReadToMemo(Table table, int offSet)
    {
        string tableName = table.Name;

        string table_path  = Path.Combine(Directory.GetCurrentDirectory(), tableName);
            
        if (!File.Exists(table_path))
            throw new FileNotFoundException($"Arquivo de dados {tableName} não encontrado em {table_path}");
            
        int tupleCounter = 0;
        int readPageCounter = 0;
        int actualPage = 0;
        string line = "";
        string[] partLine;
        bool isEndOfFile = false;
        List<Array> helper = new List<Array>();
            
        using StreamReader reader = new StreamReader(table_path);
            
        //Read the header
        line = reader.ReadLine(); 
            
        while ((line = reader.ReadLine()) != null)
        {
            if (offSet != 0 && actualPage < offSet)
            {
                //Avança para a primeira página após a página offset
                for (int i = 0; i < offSet * 10 ; i++)
                {
                    line = reader.ReadLine();
                }
                actualPage = offSet;
            }
            if (tupleCounter < 10)
            {
                partLine = line.Split(',');
                table.AddTupple(partLine); //Adiciona a tupla na memória

                tupleCounter++;
            }
            else
            {
                //Leu uma página toda; Incrementa uma página e reseta o contador de tuplas;
                readPageCounter++;  //Paginas lidas na iteração atual 0 - 4
                actualPage++;       //Paginas lidas ao todo no arquivo (offset)
                tupleCounter = 0;

                Console.WriteLine($"Pagina lida para {tableName}!");

                if (readPageCounter >= 4)
                {
                    break;
                }
                partLine = line.Split(',');
                table.AddTupple(partLine); //Adiciona a tupla na memória
                tupleCounter++;
            }
        }
        isEndOfFile = (line == null);
        
        //Ao chegar aqui, helper terá lido até 4 páginas, com 40 tuplas em si, ou estará vazio.
        // return new PageLoadReference(helper, readPageCounter, tupleCounter, isEndOfFile);
        return new PageLoadReference(table, readPageCounter, tupleCounter, isEndOfFile);
    }
}
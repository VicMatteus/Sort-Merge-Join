// See https://aka.ms/new-console-template for more information

using System.Globalization;

namespace sort_merge_join
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Comentario com coisas usadas entre o dev no win e no linux
            //File.Exists(Directory.GetCurrentDirectory() + "/uva.csv")

            bool isLinux = true;
            // Operator op = new Operator(new Table("vinho.csv"), new Table("uva.csv"), "uva_id", "uva_id");
            Operator op = new Operator(new Table("uva.csv"), new Table("pais.csv"), "pais_origem_id", "pais_id");
            op.DoOperation();
        }
    }
}
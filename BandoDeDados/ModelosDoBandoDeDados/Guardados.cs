namespace Perfect_OCR.BandoDeDados.ModelosDoBandoDeDados
{
    class Guardados
    {
        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int ID { get; set; } 
        public string NomeFoto { get; set; }
        public string DataFoto { get; set; }
        public string TextFoto { get; set; }
        public string SymbolFoto { get; set; }
    }
}
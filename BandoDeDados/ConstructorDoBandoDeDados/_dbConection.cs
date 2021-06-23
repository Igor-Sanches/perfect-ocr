using System; 
using SQLite.Net.Platform.WinRT;
using SQLite.Net;
using System.IO;  
using Windows.Storage;
using Perfect_OCR.BandoDeDados.ModelosDoBandoDeDados;

namespace Perfect_OCR.BandoDeDados.ConstructorDoBandoDeDados
{
    public class _dbConection : IDisposable
    {
        public SQLiteConnection _conexao; 
        public _dbConection()
        {
            //if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _conexao = new SQLiteConnection(new SQLitePlatformWinRT(), Path.Combine(ApplicationData.Current.LocalFolder.Path, "Faby.db"));
      
            _conexao.CreateTable<Guardados>();

        }

         

        public void Dispose()
        {
            _conexao.Dispose();
        }
    }
}

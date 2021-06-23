using Perfect_OCR.BandoDeDados.ConstructorDoBandoDeDados;
using Perfect_OCR.BandoDeDados.InterfacesDoBandoDeDados;
using System.Collections.Generic;
using System.Linq;

namespace Perfect_OCR.BandoDeDados.ModelosDoBandoDeDados
{
    class GuardadosController : IRepositorioTXT<Guardados>
    {
        _dbConection _db;

        public GuardadosController()
        {
            _db = new _dbConection();
        }

        public bool Apagar(Guardados txt)
        {
            if (txt.ID > 0)
            {
                _db._conexao.Delete(txt);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Guardados inpID(Guardados ID)
        {
            _db._conexao.Table<Guardados>().Where(i => i.ID > 0);
            return null;
        }

        public List<Guardados> Lista()
        {
            return _db._conexao.Table<Guardados>().ToList();
        }

        public bool Salvar(Guardados txt)
        {
            _db._conexao.Insert(txt);
            return true;
        }
    }
}

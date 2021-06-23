using System.Collections.Generic;

namespace Perfect_OCR.BandoDeDados.InterfacesDoBandoDeDados
{ 
    public interface IRepositorioTXT <T> where T : class
    {
        T inpID(T ID);
        bool Salvar(T txt);
        bool Apagar(T txt);
        List<T> Lista();
    }
}

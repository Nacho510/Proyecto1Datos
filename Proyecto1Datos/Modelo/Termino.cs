using PruebaRider.Estructura;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo;

public class Termino
{
    private string palabra;
    private ListaDobleEnlazada<Documento> listaDocumentos;
    private int frecuencias; //DF (document frecuency)
    private double idf;
    
    public Termino(string palabra)
    {
        this.palabra = palabra;
        listaDocumentos = new ListaDobleEnlazada<Documento>();
        frecuencias = 0;
    }
    public string Palabra
    {
        get => palabra;
        set => palabra = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ListaDobleEnlazada<Documento> ListaDocumentos
    {
        get => listaDocumentos;
        set => listaDocumentos = value ?? throw new ArgumentNullException(nameof(value));
    }

    public double Idf
    {
        get => idf;
        set => idf = value;
    }

    public void AgregarDocumento(Documento documento) // O(n) - verificar si no existe
    {
        if (!listaDocumentos.Existe(documento))
        {
            listaDocumentos.Agregar(documento);
        }
    }

    public void CalcularIdf(int totalDocuments) // O(1)
    {
        if (listaDocumentos.Count > 0)
        {
            idf = Math.Log10((double)totalDocuments / listaDocumentos.Count);
        }
    }

   public double GetTfIdf(Documento documento) // O(1)
   {
        int tf = documento.GetFrecuencia(palabra);
        return tf * idf;
    }
}
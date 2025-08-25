using PruebaRider.Estructura;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo;

public class Documento
{
    private int id;
    private string textoOriginal;
    private string tokens;
    private string ruta;
    private ListaDobleEnlazada<TerminoFrecuencia> frecuencias; //Adios dictionary brr brr

    public Documento()
    {
        this.id = 0;
        this.textoOriginal = "";
        this.tokens = "";
        this.ruta = "";
        this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
    }
    
    public Documento(int id, string textoOriginal, string ruta)
    {
        this.id = id;
        this.textoOriginal = textoOriginal ?? throw new ArgumentNullException(nameof(textoOriginal));
        this.ruta = ruta ?? throw new ArgumentNullException(nameof(ruta));
        this.frecuencias = new ListaDobleEnlazada<TerminoFrecuencia>();
    }

    public int Id
    {
        get => id;
        set => id = value;
    }

    public ListaDobleEnlazada<TerminoFrecuencia> Frecuencias
    {
        get => frecuencias;
        set => frecuencias = value ?? throw new ArgumentNullException(nameof(value));
    }

    ///Tambien se puede hacer de esta forma mas sencilla si no hace falta incluir verificaciones
     //public int Id { get; set; }
    // public string TextoOriginal { get; set; } = "";
    
    public string TextoOriginal
    {
        get => textoOriginal; // => se llama lambda
        set => textoOriginal = value ?? throw new ArgumentNullException(nameof(value)); // evita errores
    }

    public string Ruta
    {
        get => ruta;
        set => ruta = value;
    }
    public string Tokens
    {
        get => tokens;
        set => tokens = value ?? throw new ArgumentNullException(nameof(value));
    }

    
    public void CalcularFrecuencias(List<string> tokens) // O(n*m)
    {
        frecuencias.Limpiar();

        foreach (var token in tokens)
        {
            bool encontrado = false;

            var current = frecuencias.ObtenerInicio();
            for (int i = 0; i < frecuencias.Count; i++)
            {
                if (current.Data.Token == token)
                {
                    current.Data.Frecuencia++;
                    encontrado = true;
                    break;
                }
                current = current.Sig;
            }

            if (!encontrado)
                frecuencias.Agregar(new TerminoFrecuencia(token));
        }
    }

    public int GetFrecuencia(string termino) // O(n)
    {
        var current = frecuencias.ObtenerInicio();
        for (int i = 0; i < frecuencias.Count; i++)
        {
            if (current.Data.Token == termino)
                return current.Data.Frecuencia;
            current = current.Sig;
        }
        return 0;
    }
}
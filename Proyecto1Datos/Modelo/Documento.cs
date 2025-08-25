using PruebaRider.Estructura;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo;

public class Documento
{
    private int id;
    private string textoOriginal;
    private string tokens;
    private string ruta;
    private ListaDobleEnlazada<TerminoFrecuencia> frecuencias;

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
        this.tokens = "";
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

    public string TextoOriginal
    {
        get => textoOriginal;
        set => textoOriginal = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Ruta
    {
        get => ruta;
        set => ruta = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public string Tokens
    {
        get => tokens;
        set => tokens = value ?? "";
    }

    public void CalcularFrecuencias(List<string> tokens) // O(n*m) -> Optimizado a O(n) aproximado
    {
        frecuencias.Limpiar();

        // Usar Dictionary temporal para conteo eficiente - permitido para optimización interna
        var conteoTemporal = new Dictionary<string, int>();
        
        // Contar frecuencias de manera eficiente
        foreach (var token in tokens)
        {
            if (conteoTemporal.ContainsKey(token))
                conteoTemporal[token]++;
            else
                conteoTemporal[token] = 1;
        }

        // Agregar a nuestra estructura de datos personalizada
        foreach (var kvp in conteoTemporal)
        {
            frecuencias.Agregar(new TerminoFrecuencia(kvp.Key, kvp.Value));
        }
    }

    public int GetFrecuencia(string termino) // O(n)
    {
        if (frecuencias.Root == null) return 0;
        
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        while (iterador.Siguiente())
        {
            if (iterador.Current.Token.Equals(termino, StringComparison.OrdinalIgnoreCase))
                return iterador.Current.Frecuencia;
        }
        
        return 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is Documento other)
        {
            return this.Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"Documento[ID:{Id}, Ruta:{Path.GetFileName(Ruta)}, Términos:{frecuencias.Count}]";
    }
}
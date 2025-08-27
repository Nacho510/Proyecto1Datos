using PruebaRider.Estructura;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Modelo;

/// <summary>
/// Documento optimizado SIN cache interno - solo algoritmos eficientes
/// Mantiene frecuencias ordenadas para búsqueda binaria O(log n)
/// </summary>
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
        set 
        { 
            frecuencias = value ?? throw new ArgumentNullException(nameof(value));
        }
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

    /// <summary>
    /// Calcular frecuencias manteniendo orden alfabético para búsqueda binaria O(log n)
    /// Trade-off: cálculo inicial más lento, pero búsquedas futuras O(log n)
    /// </summary>
    public void CalcularFrecuencias(List<string> tokens)
    {
        frecuencias.Limpiar();

        if (tokens == null || tokens.Count == 0)
            return;

        // Crear lista temporal para conteo eficiente
        var contadores = new ListaDobleEnlazada<ContadorTermino>();

        // Contar frecuencias - O(n*m) donde m es términos únicos
        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token)) continue;

            string tokenNormalizado = token.ToLowerInvariant();
            bool encontrado = false;

            // Buscar en contadores existentes
            var iterador = new Iterador<ContadorTermino>(contadores);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Token.Equals(tokenNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    iterador.Current.Incrementar();
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                contadores.Agregar(new ContadorTermino(tokenNormalizado));
            }
        }

        // Convertir contadores a TerminoFrecuencia manteniendo orden alfabético
        var iteradorContadores = new Iterador<ContadorTermino>(contadores);
        while (iteradorContadores.Siguiente())
        {
            var contador = iteradorContadores.Current;
            var terminoFrec = new TerminoFrecuencia(contador.Token, contador.Frecuencia);
            
            // Inserción ordenada alfabéticamente para búsqueda binaria futura
            frecuencias.AgregarOrdenado(terminoFrec, CompararTerminosAlfabeticamente);
        }
    }

    /// <summary>
    /// Obtener frecuencia usando búsqueda binaria O(log n)
    /// Consistentemente eficiente sin cache
    /// </summary>
    public int GetFrecuencia(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return 0;

        if (frecuencias.Count == 0)
            return 0;

        // Usar búsqueda binaria si hay suficientes elementos
        if (frecuencias.EstaOrdenada && frecuencias.Count > 5)
        {
            var terminoBuscado = new TerminoFrecuencia(termino.ToLowerInvariant(), 0);
            var encontrado = frecuencias.BuscarBinario(terminoBuscado, CompararTerminosAlfabeticamente);
            return encontrado?.Frecuencia ?? 0;
        }

        // Búsqueda lineal para listas pequeñas
        return BusquedaLinealFrecuencia(termino);
    }

    /// <summary>
    /// Verificar si contiene un término - O(log n) con búsqueda binaria
    /// </summary>
    public bool ContieneTerm(string termino)
    {
        return GetFrecuencia(termino) > 0;
    }

    /// <summary>
    /// Obtener términos más frecuentes del documento
    /// </summary>
    public ListaDobleEnlazada<TerminoFrecuencia> ObtenerTerminosMasFrecuentes(int cantidad = 10)
    {
        if (frecuencias.Count == 0)
            return new ListaDobleEnlazada<TerminoFrecuencia>();

        // Crear copia para ordenar por frecuencia (no alfabéticamente)
        var terminosOrdenados = new ListaDobleEnlazada<TerminoFrecuencia>();
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        
        while (iterador.Siguiente())
        {
            terminosOrdenados.Agregar(new TerminoFrecuencia(
                iterador.Current.Token, 
                iterador.Current.Frecuencia
            ));
        }

        // Ordenar por frecuencia descendente
        terminosOrdenados.OrdenarDescendente(tf => tf.Frecuencia);

        // Tomar solo la cantidad solicitada
        if (terminosOrdenados.Count <= cantidad)
            return terminosOrdenados;

        var resultado = new ListaDobleEnlazada<TerminoFrecuencia>();
        var iteradorOrdenado = new Iterador<TerminoFrecuencia>(terminosOrdenados);
        int contador = 0;
        
        while (iteradorOrdenado.Siguiente() && contador < cantidad)
        {
            resultado.Agregar(iteradorOrdenado.Current);
            contador++;
        }

        return resultado;
    }

    /// <summary>
    /// Obtener estadísticas detalladas del documento
    /// </summary>
    public EstadisticasDocumento ObtenerEstadisticas()
    {
        int totalTokens = 0;
        int maxFrecuencia = 0;
        string terminoMasFrecuente = "";

        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        while (iterador.Siguiente())
        {
            var tf = iterador.Current;
            totalTokens += tf.Frecuencia;
            
            if (tf.Frecuencia > maxFrecuencia)
            {
                maxFrecuencia = tf.Frecuencia;
                terminoMasFrecuente = tf.Token;
            }
        }

        return new EstadisticasDocumento
        {
            DocumentoId = id,
            NombreArchivo = Path.GetFileName(ruta),
            TerminosUnicos = frecuencias.Count,
            TotalTokens = totalTokens,
            TerminoMasFrecuente = terminoMasFrecuente,
            FrecuenciaMaxima = maxFrecuencia,
            PromedioFrecuencia = frecuencias.Count > 0 ? (double)totalTokens / frecuencias.Count : 0,
            TamañoTexto = textoOriginal?.Length ?? 0,
            FrecuenciasOrdenadas = frecuencias.EstaOrdenada
        };
    }

    /// <summary>
    /// Buscar términos que contengan una subcadena específica
    /// </summary>
    public ListaDobleEnlazada<TerminoFrecuencia> BuscarTerminosQueContienen(string subcadena)
    {
        var resultado = new ListaDobleEnlazada<TerminoFrecuencia>();
        
        if (string.IsNullOrWhiteSpace(subcadena))
            return resultado;

        string subcadenaNormalizada = subcadena.ToLowerInvariant();
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        
        while (iterador.Siguiente())
        {
            if (iterador.Current.Token.Contains(subcadenaNormalizada))
            {
                resultado.Agregar(iterador.Current);
            }
        }

        return resultado;
    }

    /// <summary>
    /// Obtener distribución de frecuencias (para análisis estadístico)
    /// </summary>
    public DistribucionFrecuencias Analizar()
    {
        var distribucion = new DistribucionFrecuencias();
        
        if (frecuencias.Count == 0)
            return distribucion;

        var listaFrecuencias = new ListaDobleEnlazada<int>();
        int total = 0;
        int minFrec = int.MaxValue;
        int maxFrec = 0;

        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        while (iterador.Siguiente())
        {
            int freq = iterador.Current.Frecuencia;
            listaFrecuencias.Agregar(freq);
            total += freq;
            
            if (freq < minFrec) minFrec = freq;
            if (freq > maxFrec) maxFrec = freq;
        }

        distribucion.TerminosUnicos = frecuencias.Count;
        distribucion.TotalTokens = total;
        distribucion.FrecuenciaMinima = minFrec == int.MaxValue ? 0 : minFrec;
        distribucion.FrecuenciaMaxima = maxFrec;
        distribucion.PromedioFrecuencia = frecuencias.Count > 0 ? (double)total / frecuencias.Count : 0;

        return distribucion;
    }

    /// <summary>
    /// Obtener términos con frecuencia específica
    /// </summary>
    public ListaDobleEnlazada<string> ObtenerTerminosConFrecuencia(int frecuenciaBuscada)
    {
        var resultado = new ListaDobleEnlazada<string>();
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        
        while (iterador.Siguiente())
        {
            if (iterador.Current.Frecuencia == frecuenciaBuscada)
            {
                resultado.Agregar(iterador.Current.Token);
            }
        }
        
        return resultado;
    }

    /// <summary>
    /// Obtener términos únicos (frecuencia = 1)
    /// </summary>
    public ListaDobleEnlazada<string> ObtenerTerminosUnicos()
    {
        return ObtenerTerminosConFrecuencia(1);
    }

    /// <summary>
    /// Contar términos por rango de frecuencia
    /// </summary>
    public int ContarTerminosEnRango(int frecuenciaMin, int frecuenciaMax)
    {
        int contador = 0;
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        
        while (iterador.Siguiente())
        {
            int freq = iterador.Current.Frecuencia;
            if (freq >= frecuenciaMin && freq <= frecuenciaMax)
            {
                contador++;
            }
        }
        
        return contador;
    }

    /// <summary>
    /// Crear conjunto de todos los términos del documento - SIN ConjuntoStrings
    /// </summary>
    public ListaDobleEnlazada<string> ObtenerListaTerminos()
    {
        var listaTerminos = new ListaDobleEnlazada<string>();
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        
        while (iterador.Siguiente())
        {
            listaTerminos.Agregar(iterador.Current.Token);
        }
        
        return listaTerminos;
    }

    /// <summary>
    /// Validar integridad de las frecuencias
    /// </summary>
    public bool ValidarIntegridad()
    {
        // Verificar que todas las frecuencias sean positivas
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        while (iterador.Siguiente())
        {
            if (iterador.Current.Frecuencia <= 0)
                return false;
            
            if (string.IsNullOrWhiteSpace(iterador.Current.Token))
                return false;
        }
        
        return true;
    }

    #region Métodos Privados

    /// <summary>
    /// Búsqueda lineal de frecuencia (fallback para listas pequeñas)
    /// </summary>
    private int BusquedaLinealFrecuencia(string termino)
    {
        var iterador = new Iterador<TerminoFrecuencia>(frecuencias);
        while (iterador.Siguiente())
        {
            if (iterador.Current.Token.Equals(termino, StringComparison.OrdinalIgnoreCase))
                return iterador.Current.Frecuencia;
        }
        return 0;
    }

    /// <summary>
    /// Comparador alfabético para TerminoFrecuencia
    /// </summary>
    private int CompararTerminosAlfabeticamente(TerminoFrecuencia tf1, TerminoFrecuencia tf2)
    {
        return string.Compare(tf1.Token, tf2.Token, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Clase auxiliar para conteo durante cálculo de frecuencias
    /// </summary>
    private class ContadorTermino
    {
        public string Token { get; }
        public int Frecuencia { get; private set; }

        public ContadorTermino(string token)
        {
            Token = token;
            Frecuencia = 1;
        }

        public void Incrementar()
        {
            Frecuencia++;
        }
    }

    #endregion

    #region Métodos de Objeto

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
        var stats = ObtenerEstadisticas();
        return $"Documento[ID:{Id}, Archivo:{stats.NombreArchivo}, Términos:{stats.TerminosUnicos}, Tokens:{stats.TotalTokens}]";
    }

    #endregion
}

/// <summary>
/// Estadísticas detalladas de un documento sin cache
/// </summary>
public class EstadisticasDocumento
{
    public int DocumentoId { get; set; }
    public string NombreArchivo { get; set; }
    public int TerminosUnicos { get; set; }
    public int TotalTokens { get; set; }
    public string TerminoMasFrecuente { get; set; }
    public int FrecuenciaMaxima { get; set; }
    public double PromedioFrecuencia { get; set; }
    public int TamañoTexto { get; set; }
    public bool FrecuenciasOrdenadas { get; set; }

    public override string ToString()
    {
        return $"📄 {NombreArchivo}\n" +
               $"   🔤 Términos únicos: {TerminosUnicos:N0}\n" +
               $"   📊 Total tokens: {TotalTokens:N0}\n" +
               $"   🏆 Más frecuente: '{TerminoMasFrecuente}' ({FrecuenciaMaxima}x)\n" +
               $"   📈 Promedio: {PromedioFrecuencia:F1}\n" +
               $"   📏 Tamaño: {TamañoTexto:N0} caracteres\n" +
               $"   🔍 Ordenado: {(FrecuenciasOrdenadas ? "✅ Búsqueda O(log n)" : "❌ Búsqueda O(n)")}";
    }
}

/// <summary>
/// Análisis de distribución de frecuencias en un documento
/// </summary>
public class DistribucionFrecuencias
{
    public int TerminosUnicos { get; set; }
    public int TotalTokens { get; set; }
    public int FrecuenciaMinima { get; set; }
    public int FrecuenciaMaxima { get; set; }
    public double PromedioFrecuencia { get; set; }

    public override string ToString()
    {
        return $"📊 Distribución de Frecuencias:\n" +
               $"   Términos únicos: {TerminosUnicos:N0}\n" +
               $"   Total tokens: {TotalTokens:N0}\n" +
               $"   Frecuencia mín/máx: {FrecuenciaMinima}/{FrecuenciaMaxima}\n" +
               $"   Promedio: {PromedioFrecuencia:F1}";
    }
}
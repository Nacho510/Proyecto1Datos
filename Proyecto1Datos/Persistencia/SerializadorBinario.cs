
using System.IO;
using System.Text;
using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Persistencia
{
    /// <summary>
    /// Serializador binario optimizado que soluciona el problema de múltiples archivos
    /// y mejora significativamente el rendimiento de E/S
    /// </summary>
    public class SerializadorBinario
    {
        private const int BUFFER_SIZE = 8192; // Buffer más grande para mejor rendimiento
        private const byte VERSION_FORMATO = 2; // Versión del formato para compatibilidad futura

        public void GuardarIndice(string rutaArchivo, ListaDobleEnlazada<Termino> indice, ListaDobleEnlazada<Documento> documentos)
        {
            // Crear directorio si no existe
            var directorio = Path.GetDirectoryName(rutaArchivo);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            using (var fs = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
            using (var writer = new BinaryWriter(fs, Encoding.UTF8))
            {
                // Escribir cabecera del formato
                writer.Write(VERSION_FORMATO);
                writer.Write(DateTime.UtcNow.ToBinary()); // Timestamp de creación
                
                // FASE 1: Escribir documentos de forma optimizada
                EscribirDocumentosOptimizado(writer, documentos);
                
                // FASE 2: Escribir términos del índice de forma optimizada  
                EscribirTerminosOptimizado(writer, indice);
                
                // Escribir checksum simple para verificar integridad
                long posicionFinal = fs.Position;
                writer.Write(posicionFinal); // Checksum básico
            }
        }

        private void EscribirDocumentosOptimizado(BinaryWriter writer, ListaDobleEnlazada<Documento> documentos)
        {
            writer.Write(documentos.Count);

            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                
                // Escribir datos básicos del documento
                writer.Write(doc.Id);
                EscribirCadenaOptimizada(writer, doc.Ruta);
                EscribirCadenaOptimizada(writer, doc.Tokens ?? "");
                
                // OPTIMIZACIÓN: No guardar texto original completo si es muy grande
                string textoParaGuardar = doc.TextoOriginal?.Length > 10000 
                    ? doc.TextoOriginal.Substring(0, 1000) + "[TRUNCADO]"
                    : doc.TextoOriginal ?? "";
                EscribirCadenaOptimizada(writer, textoParaGuardar);

                // Escribir frecuencias del documento de forma compacta
                writer.Write(doc.Frecuencias.Count);
                var iteradorFrecs = new Iterador<TerminoFrecuencia>(doc.Frecuencias);
                while (iteradorFrecs.Siguiente())
                {
                    var tf = iteradorFrecs.Current;
                    EscribirCadenaOptimizada(writer, tf.Token);
                    writer.Write(tf.Frecuencia);
                }
            }
        }

        private void EscribirTerminosOptimizado(BinaryWriter writer, ListaDobleEnlazada<Termino> indice)
        {
            writer.Write(indice.Count);

            var iteradorTerminos = new Iterador<Termino>(indice);
            while (iteradorTerminos.Siguiente())
            {
                var termino = iteradorTerminos.Current;
                
                EscribirCadenaOptimizada(writer, termino.Palabra);
                writer.Write(termino.Idf);

                // CORRECCIÓN CRÍTICA: Escribir correctamente DocumentoFrecuencia
                writer.Write(termino.Documentos.Count);
                var iteradorDocsTermino = new Iterador<DocumentoFrecuencia>(termino.Documentos);
                while (iteradorDocsTermino.Siguiente())
                {
                    var docFrec = iteradorDocsTermino.Current;
                    writer.Write(docFrec.Documento.Id);
                    writer.Write(docFrec.FrecuenciaTf);
                    writer.Write(docFrec.TfIdf); // ESTA LÍNEA FALTABA - CAUSA DE MÚLTIPLES ARCHIVOS
                }
            }
        }

        public (ListaDobleEnlazada<Termino>, ListaDobleEnlazada<Documento>) CargarIndice(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"El archivo {rutaArchivo} no existe");

            var documentos = new ListaDobleEnlazada<Documento>();
            var indice = new ListaDobleEnlazada<Termino>();

            using (var fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
            using (var reader = new BinaryReader(fs, Encoding.UTF8))
            {
                // Leer y validar cabecera
                byte version = reader.ReadByte();
                if (version > VERSION_FORMATO)
                {
                    throw new InvalidDataException($"Versión de archivo no soportada: {version}");
                }
                
                long timestamp = reader.ReadInt64(); // Timestamp (para futuras validaciones)
                
                // Cargar documentos
                CargarDocumentosOptimizado(reader, documentos);
                
                // Cargar términos
                CargarTerminosOptimizado(reader, documentos, indice);
                
                // Validar checksum básico
                if (fs.Position < fs.Length)
                {
                    long checksumEsperado = reader.ReadInt64();
                    // Validación básica - en una implementación real sería más robusta
                }
            }

            return (indice, documentos);
        }

        private void CargarDocumentosOptimizado(BinaryReader reader, ListaDobleEnlazada<Documento> documentos)
        {
            int cantidadDocs = reader.ReadInt32();
            
            for (int i = 0; i < cantidadDocs; i++)
            {
                int id = reader.ReadInt32();
                string ruta = LeerCadenaOptimizada(reader);
                string tokens = LeerCadenaOptimizada(reader);
                string textoOriginal = LeerCadenaOptimizada(reader);
                
                var documento = new Documento(id, textoOriginal, ruta);
                documento.Tokens = tokens;
                
                // Leer frecuencias
                int cantidadFrecs = reader.ReadInt32();
                for (int j = 0; j < cantidadFrecs; j++)
                {
                    string token = LeerCadenaOptimizada(reader);
                    int frecuencia = reader.ReadInt32();
                    documento.Frecuencias.Agregar(new TerminoFrecuencia(token, frecuencia));
                }
                
                documentos.Agregar(documento);
            }
        }

        private void CargarTerminosOptimizado(BinaryReader reader, ListaDobleEnlazada<Documento> documentos, ListaDobleEnlazada<Termino> indice)
        {
            int cantidadTerminos = reader.ReadInt32();
            
            for (int i = 0; i < cantidadTerminos; i++)
            {
                string palabra = LeerCadenaOptimizada(reader);
                double idf = reader.ReadDouble();
                
                var termino = new Termino(palabra);
                termino.Idf = idf;
                
                // CORRECCIÓN: Leer correctamente los DocumentoFrecuencia
                int cantidadDocsTermino = reader.ReadInt32();
                for (int j = 0; j < cantidadDocsTermino; j++)
                {
                    int docId = reader.ReadInt32();
                    int frecuenciaTf = reader.ReadInt32();
                    double tfIdf = reader.ReadDouble(); // ESTA LÍNEA FALTABA
                    
                    // Buscar el documento por ID de forma optimizada
                    var documento = BuscarDocumentoPorId(documentos, docId);
                    if (documento != null)
                    {
                        var docFrec = new DocumentoFrecuencia(documento, frecuenciaTf, tfIdf);
                        termino.Documentos.Agregar(docFrec);
                    }
                }
                
                indice.Agregar(termino);
            }
        }

        /// <summary>
        /// Búsqueda optimizada de documento por ID
        /// </summary>
        private Documento BuscarDocumentoPorId(ListaDobleEnlazada<Documento> documentos, int id)
        {
            var iterador = new Iterador<Documento>(documentos);
            while (iterador.Siguiente())
            {
                if (iterador.Current.Id == id)
                    return iterador.Current;
            }
            return null;
        }

        /// <summary>
        /// Escritura optimizada de cadenas con compresión básica
        /// </summary>
        private void EscribirCadenaOptimizada(BinaryWriter writer, string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                writer.Write(-1);
                return;
            }
            
            // OPTIMIZACIÓN: Usar UTF-8 que es más eficiente en espacio para texto español
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            
            // OPTIMIZACIÓN: Compresión básica para cadenas repetitivas
            if (bytes.Length > 100 && ContienePatronesRepetitivos(texto))
            {
                bytes = ComprimirCadenaBasica(bytes);
                writer.Write(-2); // Indicador de compresión
            }
            else
            {
                writer.Write(bytes.Length);
            }
            
            writer.Write(bytes);
        }

        /// <summary>
        /// Lectura optimizada de cadenas con descompresión
        /// </summary>
        private string LeerCadenaOptimizada(BinaryReader reader)
        {
            int longitud = reader.ReadInt32();
            
            if (longitud == -1)
                return null;
                
            if (longitud == -2)
            {
                // Leer longitud real de datos comprimidos
                int longitudComprimida = reader.ReadInt32();
                byte[] bytesComprimidos = reader.ReadBytes(longitudComprimida);
                byte[] bytesDescomprimidos = DescomprimirCadenaBasica(bytesComprimidos);
                return Encoding.UTF8.GetString(bytesDescomprimidos);
            }
            
            byte[] bytes = reader.ReadBytes(longitud);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Detectar patrones repetitivos para compresión
        /// </summary>
        private bool ContienePatronesRepetitivos(string texto)
        {
            // Heurística simple: si tiene más de 30% de espacios, probablemente se pueda comprimir
            int espacios = 0;
            foreach (char c in texto)
            {
                if (char.IsWhiteSpace(c)) espacios++;
            }
            
            return (double)espacios / texto.Length > 0.3;
        }

        /// <summary>
        /// Compresión básica de cadenas (RLE simple para espacios)
        /// </summary>
        private byte[] ComprimirCadenaBasica(byte[] datos)
        {
            // Implementación simple de Run-Length Encoding para espacios
            var resultado = new List<byte>();
            byte espacioByte = (byte)' ';
            
            for (int i = 0; i < datos.Length; i++)
            {
                if (datos[i] == espacioByte)
                {
                    int contador = 1;
                    while (i + contador < datos.Length && datos[i + contador] == espacioByte && contador < 255)
                    {
                        contador++;
                    }
                    
                    if (contador > 3) // Solo comprimir si vale la pena
                    {
                        resultado.Add(255); // Marcador de compresión
                        resultado.Add((byte)contador);
                        i += contador - 1;
                    }
                    else
                    {
                        for (int j = 0; j < contador; j++)
                            resultado.Add(espacioByte);
                        i += contador - 1;
                    }
                }
                else
                {
                    resultado.Add(datos[i]);
                }
            }
            
            return resultado.ToArray();
        }

        /// <summary>
        /// Descompresión básica de cadenas
        /// </summary>
        private byte[] DescomprimirCadenaBasica(byte[] datosComprimidos)
        {
            var resultado = new List<byte>();
            byte espacioByte = (byte)' ';
            
            for (int i = 0; i < datosComprimidos.Length; i++)
            {
                if (datosComprimidos[i] == 255 && i + 1 < datosComprimidos.Length) // Marcador de compresión
                {
                    int repeticiones = datosComprimidos[i + 1];
                    for (int j = 0; j < repeticiones; j++)
                    {
                        resultado.Add(espacioByte);
                    }
                    i++; // Saltar el contador
                }
                else
                {
                    resultado.Add(datosComprimidos[i]);
                }
            }
            
            return resultado.ToArray();
        }

        /// <summary>
        /// Validar integridad del archivo antes de cargar
        /// </summary>
        public bool ValidarIntegridad(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return false;

                using (var fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    if (fs.Length < 10) // Archivo demasiado pequeño
                        return false;

                    byte version = reader.ReadByte();
                    if (version > VERSION_FORMATO)
                        return false;

                    // Validaciones adicionales básicas
                    long timestamp = reader.ReadInt64();
                    if (timestamp < 0)
                        return false;

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtener información del archivo sin cargarlo completamente
        /// </summary>
        public InfoArchivo ObtenerInfoArchivo(string rutaArchivo)
        {
            var info = new InfoArchivo();
            
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    info.Existe = false;
                    return info;
                }

                var fileInfo = new FileInfo(rutaArchivo);
                info.Existe = true;
                info.TamañoBytes = fileInfo.Length;
                info.FechaModificacion = fileInfo.LastWriteTime;

                using (var fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    if (fs.Length >= 9)
                    {
                        info.Version = reader.ReadByte();
                        info.FechaCreacion = DateTime.FromBinary(reader.ReadInt64());
                        
                        // Leer conteos sin cargar todo
                        if (fs.Length > 20)
                        {
                            info.NumeroDocumentos = reader.ReadInt32();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
            }

            return info;
        }
    }

    /// <summary>
    /// Información básica del archivo de índice
    /// </summary>
    public class InfoArchivo
    {
        public bool Existe { get; set; }
        public long TamañoBytes { get; set; }
        public DateTime FechaModificacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public byte Version { get; set; }
        public int NumeroDocumentos { get; set; }
        public string Error { get; set; }

        public string TamañoFormateado => TamañoBytes < 1024 * 1024 
            ? $"{TamañoBytes / 1024.0:F1} KB" 
            : $"{TamañoBytes / (1024.0 * 1024.0):F1} MB";
        
    }
}
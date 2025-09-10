
using System.IO;
using System.Text;
using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Persistencia
{
    /// <summary>
    /// Serializador binario para varios archivos
    /// </summary>
    public class SerializadorBinario
    {
        private const int BUFFER_SIZE = 8192; 
        private const byte VERSION_FORMATO = 2; 

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
                writer.Write(VERSION_FORMATO);
                writer.Write(DateTime.UtcNow.ToBinary());
                
                EscribirDocumentosOptimizado(writer, documentos);
                
                EscribirTerminosOptimizado(writer, indice);
                
                long posicionFinal = fs.Position;
                writer.Write(posicionFinal); 
            }
        }

        private void EscribirDocumentosOptimizado(BinaryWriter writer, ListaDobleEnlazada<Documento> documentos)
        {
            writer.Write(documentos.Count);

            var iteradorDocs = new Iterador<Documento>(documentos);
            while (iteradorDocs.Siguiente())
            {
                var doc = iteradorDocs.Current;
                
                writer.Write(doc.Id);
                EscribirCadenaOptimizada(writer, doc.Ruta);
                EscribirCadenaOptimizada(writer, doc.Tokens ?? "");
                
                string textoParaGuardar = doc.TextoOriginal?.Length > 10000 
                    ? doc.TextoOriginal.Substring(0, 1000) + "[TRUNCADO]"
                    : doc.TextoOriginal ?? "";
                EscribirCadenaOptimizada(writer, textoParaGuardar);

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
                byte version = reader.ReadByte();
                if (version > VERSION_FORMATO)
                {
                    throw new InvalidDataException($"Versión de archivo no soportada: {version}");
                }
                
                long timestamp = reader.ReadInt64(); // Timestamp (para futuras validaciones)
                
                CargarDocumentosOptimizado(reader, documentos);
                
                CargarTerminosOptimizado(reader, documentos, indice);
                
                if (fs.Position < fs.Length)
                {
                    long checksumEsperado = reader.ReadInt64();
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
                
                int cantidadDocsTermino = reader.ReadInt32();
                for (int j = 0; j < cantidadDocsTermino; j++)
                {
                    int docId = reader.ReadInt32();
                    int frecuenciaTf = reader.ReadInt32();
                    double tfIdf = reader.ReadDouble(); 
                    
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
        
        private void EscribirCadenaOptimizada(BinaryWriter writer, string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                writer.Write(-1);
                return;
            }
            
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            
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
        
        
    }
    
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
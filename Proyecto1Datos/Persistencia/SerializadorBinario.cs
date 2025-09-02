
using System.Text;
using PruebaRider.Modelo;
using PruebaRider.Estructura.Nodo;

namespace PruebaRider.Persistencia
{
    public class SerializadorBinario
    {
        public void GuardarIndice(string rutaArchivo, ListaDobleEnlazada<Termino> indice, ListaDobleEnlazada<Documento> documentos)
        {
            using (FileStream fs = new FileStream(rutaArchivo, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Escribir documentos primero
                writer.Write(documentos.Count);
        
                var iteradorDocs = new Iterador<Documento>(documentos);
                while (iteradorDocs.Siguiente())
                {
                    var doc = iteradorDocs.Current;
                    writer.Write(doc.Id);
                    EscribirCadena(writer, doc.TextoOriginal);
                    EscribirCadena(writer, doc.Ruta);
                    EscribirCadena(writer, doc.Tokens ?? "");
            
                    // Escribir frecuencias del documento
                    writer.Write(doc.Frecuencias.Count);
                    var iteradorFrecs = new Iterador<TerminoFrecuencia>(doc.Frecuencias);
                    while (iteradorFrecs.Siguiente())
                    {
                        var tf = iteradorFrecs.Current;
                        EscribirCadena(writer, tf.Token);
                        writer.Write(tf.Frecuencia);
                    }
                }
        
                // Escribir términos del índice
                writer.Write(indice.Count);
        
                var iteradorTerminos = new Iterador<Termino>(indice);
                while (iteradorTerminos.Siguiente())
                {
                    var termino = iteradorTerminos.Current;
                    EscribirCadena(writer, termino.Palabra);
                    writer.Write(termino.Idf);
            
                    // Escribir documentos asociados al término
                    writer.Write(termino.Documentos.Count);
                    var iteradorDocsTermino = new Iterador<DocumentoFrecuencia>(termino.Documentos);
                    while (iteradorDocsTermino.Siguiente())
                    {
                        writer.Write(iteradorDocsTermino.Current.Documento.Id);
                        writer.Write(iteradorDocsTermino.Current.FrecuenciaTf);
                        writer.Write(iteradorDocsTermino.Current.TfIdf);
                    }
                }
            }
        }

        public (ListaDobleEnlazada<Termino>, ListaDobleEnlazada<Documento>) CargarIndice(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"El archivo {rutaArchivo} no existe");

            var documentos = new ListaDobleEnlazada<Documento>();
            var indice = new ListaDobleEnlazada<Termino>();

            using (FileStream fs = new FileStream(rutaArchivo, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Leer documentos
                int cantidadDocs = reader.ReadInt32();
                
                for (int i = 0; i < cantidadDocs; i++)
                {
                    int id = reader.ReadInt32();
                    string textoOriginal = LeerCadena(reader);
                    string ruta = LeerCadena(reader);
                    string tokens = LeerCadena(reader);
                    
                    var documento = new Documento(id, textoOriginal, ruta);
                    documento.Tokens = tokens;
                    
                    // Leer frecuencias
                    int cantidadFrecs = reader.ReadInt32();
                    for (int j = 0; j < cantidadFrecs; j++)
                    {
                        string token = LeerCadena(reader);
                        int frecuencia = reader.ReadInt32();
                        documento.Frecuencias.Agregar(new TerminoFrecuencia(token, frecuencia));
                    }
                    
                    documentos.Agregar(documento);
                }
                
                // Leer términos
                int cantidadTerminos = reader.ReadInt32();
                
                for (int i = 0; i < cantidadTerminos; i++)
                {
                    string palabra = LeerCadena(reader);
                    double idf = reader.ReadDouble();
                    
                    var termino = new Termino(palabra);
                    termino.Idf = idf;
                    
                    // Leer IDs de documentos y asociarlos
                    int cantidadDocsTermino = reader.ReadInt32();
                    for (int j = 0; j < cantidadDocsTermino; j++)
                    {
                        int docId = reader.ReadInt32();
                        
                        // Buscar el documento por ID
                        var iteradorDocs = new Iterador<Documento>(documentos);
                        while (iteradorDocs.Siguiente())
                        {
                            if (iteradorDocs.Current.Id == docId)
                            {
                                termino.AgregarDocumento(iteradorDocs.Current, frecuenciaTf: reader.ReadInt32());
                                break;
                            }
                        }
                    }
                    
                    indice.Agregar(termino);
                }
            }

            return (indice, documentos);
        }

        private void EscribirCadena(BinaryWriter writer, string texto)
        {
            if (texto == null)
            {
                writer.Write(-1);
                return;
            }
            
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private string LeerCadena(BinaryReader reader)
        {
            int longitud = reader.ReadInt32();
            if (longitud == -1)
                return null;
            
            byte[] bytes = reader.ReadBytes(longitud);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
using System.IO;
using PruebaRider.Modelo;

namespace PruebaRider.Servicios
{
    /// <summary>
    /// Almacena el resultado de una búsqueda vectorial
    /// Incluye el documento, la similitud coseno y métodos para generar URL y vista
    /// </summary>
    public class ResultadoBusquedaVectorial
    {
        public Documento Documento { get; set; }
        public double SimilitudCoseno { get; set; }
        
        // URL funcional (se genera solo cuando se solicita)
        private string _urlHtml = null;

        public ResultadoBusquedaVectorial(Documento documento, double similitudCoseno)
        {
            Documento = documento ?? throw new ArgumentNullException(nameof(documento));
            SimilitudCoseno = Math.Max(0.0, Math.Min(1.0, similitudCoseno));
        }
        
        public string GenerarUrlHtml()
        {
            if (_urlHtml != null) return _urlHtml;

            try
            {
                string contenido = File.Exists(Documento.Ruta) 
                    ? File.ReadAllText(Documento.Ruta)
                    : Documento.TextoOriginal ?? "Contenido no disponible";
                
                string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{Path.GetFileName(Documento.Ruta)}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 30px; background: #f5f5f5; }}
        .container {{ background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: #007acc; color: white; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ line-height: 1.6; white-space: pre-wrap; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📄 {Path.GetFileName(Documento.Ruta)}</h1>
            <p>Encontrado por Motor de Búsqueda - Similitud: {SimilitudCoseno * 100:F1}%</p>
        </div>
        <div class='content'>{contenido}</div>
    </div>
</body>
</html>";

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(html);
                _urlHtml = $"data:text/html;charset=utf-8;base64,{Convert.ToBase64String(bytes)}";
                return _urlHtml;
            }
            catch
            {
                return "Error generando URL";
            }
        }
        
        public string ObtenerVistaPrevia()
        {
            try
            {
                string contenido = File.Exists(Documento.Ruta) 
                    ? File.ReadAllText(Documento.Ruta)
                    : Documento.TextoOriginal ?? "";

                if (string.IsNullOrWhiteSpace(contenido))
                    return "Sin contenido disponible";

                // Tomar solo las primeras líneas
                var lineas = contenido.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var lineasValidas = lineas
                    .Where(l => !string.IsNullOrWhiteSpace(l.Trim()) && l.Trim().Length > 5)
                    .Take(2)
                    .ToArray();

                string preview = string.Join(" ", lineasValidas);
                
                // Limitar a 150 caracteres
                if (preview.Length > 150)
                    preview = preview.Substring(0, 150) + "...";

                return preview;
            }
            catch
            {
                return "Error obteniendo vista previa";
            }
        }

        public override string ToString()
        {
            return $"📄 {Path.GetFileName(Documento.Ruta)} | {SimilitudCoseno * 100:F1}%";
        }
    }
}


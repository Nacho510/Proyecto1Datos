namespace PruebaRider.Modelo;

    public class TerminoFrecuencia
    {
        public string Token { get; set; }
        public int Frecuencia { get; set; }

        public TerminoFrecuencia(string token, int frecuencia = 1)
        {
            Token = token;
            Frecuencia = frecuencia;
        }
    }
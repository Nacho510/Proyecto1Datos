namespace PruebaRider.Estructura.Vector;

public class Vector
{
    private double[] valores;
    public int Dimension => valores.Length;

    public Vector(int dimension)
    {
        valores = new double[dimension];
    }

    public Vector(double[] valores)
    {
        this.valores = new double[valores.Length];
        Array.Copy(valores, this.valores, valores.Length);
    }

    public double this[int index]
    {
        get => valores[index];
        set => valores[index] = value;
    }

    // Operador * sobrecargado para producto punto O(n)
    public static double operator *(Vector v1, Vector v2)
    {
        if (v1.Dimension != v2.Dimension)
            throw new ArgumentException("Los vectores deben ser de las mismas dimensiones. ");

        double result = 0;
        for (int i = 0; i < v1.Dimension; i++)
        {
            result += v1.valores[i] * v2.valores[i];
        }
        return result;
    }

    public double Magnitud() // O(n)
    {
        double sum = 0;
        for (int i = 0; i < valores.Length; i++)
        {
            sum += valores[i] * valores[i];
        }
        return Math.Sqrt(sum);
    }

    public double SimilitudCoseno(Vector other) 
    {
        if (this.Dimension != other.Dimension)
            throw new ArgumentException("Vectores de diferentes dimensiones");
        
        double dotProduct = this * other;
        double magnitude1 = this.Magnitud();
        double magnitude2 = other.Magnitud();

        if (magnitude1 == 0 || magnitude2 == 0) return 0;
        return dotProduct / (magnitude1 * magnitude2);
    }

    public double[] ToArray()
    {
        var result = new double[valores.Length];
        Array.Copy(valores, result, valores.Length);
        return result;
    }
}
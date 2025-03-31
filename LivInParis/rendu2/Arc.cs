using System;

namespace GraphProject
{
    /// <summary>
    /// Représente un arc entre deux noeuds dans un graphe.
    /// </summary>
    public class Arc<T>
    {
        public Noeud<T> Cible { get; set; }
        public double Poids { get; set; }

        public Arc(Noeud<T> cible, double poids)
        {
            Cible = cible;
            Poids = poids;
        }
    }
}

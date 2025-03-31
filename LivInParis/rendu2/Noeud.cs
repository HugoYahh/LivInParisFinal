using System.Collections.Generic;

namespace GraphProject
{
    /// <summary>
    /// Représente un noeud (sommet) dans un graphe.
    /// </summary>
    public class Noeud<T>
    {
        public T Valeur { get; set; }
        public List<Arc<T>> Adjacents { get; set; }

        public Noeud(T valeur)
        {
            Valeur = valeur;
            Adjacents = new List<Arc<T>>();
        }
    }
}

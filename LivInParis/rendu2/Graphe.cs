using System;
using System.Collections.Generic;

namespace GraphProject
{
    /// <summary>
    /// Graphe générique pouvant être orienté ou non, avec des poids (distances, temps, etc.).
    /// </summary>
    public class Graphe<T>
    {
        public bool Oriente { get; set; }
        public List<Noeud<T>> Noeuds { get; private set; }

        public Graphe(bool oriente = false)
        {
            Noeuds = new List<Noeud<T>>();
            Oriente = oriente;
        }

        /// <summary>
        /// Ajoute un nœud au graphe et le retourne.
        /// </summary>
        public Noeud<T> AjouterNoeud(T valeur)
        {
            var noeud = new Noeud<T>(valeur);
            Noeuds.Add(noeud);
            return noeud;
        }

        /// <summary>
        /// Retrouve le nœud ayant la valeur donnée (ou null si inexistant).
        /// </summary>
        public Noeud<T> TrouverNoeud(T valeur)
        {
            return Noeuds.Find(n => n.Valeur.Equals(valeur));
        }

        /// <summary>
        /// Ajoute un arc (liaison) entre deux nœuds (source -> cible) avec un poids.
        /// </summary>
        public void AjouterArc(T source, T cible, double poids)
        {
            var noeudSource = TrouverNoeud(source);
            var noeudCible = TrouverNoeud(cible);

            if (noeudSource == null)
                noeudSource = AjouterNoeud(source);
            if (noeudCible == null)
                noeudCible = AjouterNoeud(cible);

            noeudSource.Adjacents.Add(new Arc<T>(noeudCible, poids));

            // Si le graphe n'est pas orienté, ajouter l'arc dans l'autre sens
            if (!Oriente)
            {
                noeudCible.Adjacents.Add(new Arc<T>(noeudSource, poids));
            }
        }

        #region Algorithme de Dijkstra
        /// <summary>
        /// Calcule le plus court chemin à partir d'une source via l'algorithme de Dijkstra.
        /// Retourne un dictionnaire des distances et un dictionnaire des prédécesseurs.
        /// </summary>
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> predecesseurs)
            Dijkstra(Noeud<T> source)
        {
            var dist = new Dictionary<Noeud<T>, double>();
            var pred = new Dictionary<Noeud<T>, Noeud<T>>();
            var nonVisites = new List<Noeud<T>>(Noeuds);

            foreach (var noeud in Noeuds)
            {
                dist[noeud] = double.PositiveInfinity;
                pred[noeud] = null;
            }
            dist[source] = 0;

            while (nonVisites.Count > 0)
            {
                Noeud<T> u = null;
                double minDist = double.PositiveInfinity;
                foreach (var n in nonVisites)
                {
                    if (dist[n] < minDist)
                    {
                        minDist = dist[n];
                        u = n;
                    }
                }
                if (u == null)
                    break;

                nonVisites.Remove(u);

                foreach (var arc in u.Adjacents)
                {
                    double alt = dist[u] + arc.Poids;
                    if (alt < dist[arc.Cible])
                    {
                        dist[arc.Cible] = alt;
                        pred[arc.Cible] = u;
                    }
                }
            }
            return (dist, pred);
        }
        #endregion

        #region Algorithme de Bellman-Ford
        /// <summary>
        /// Calcule le plus court chemin à partir d'une source via l'algorithme de Bellman-Ford.
        /// </summary>
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> predecesseurs)
            BellmanFord(Noeud<T> source)
        {
            var dist = new Dictionary<Noeud<T>, double>();
            var pred = new Dictionary<Noeud<T>, Noeud<T>>();

            foreach (var noeud in Noeuds)
            {
                dist[noeud] = double.PositiveInfinity;
                pred[noeud] = null;
            }
            dist[source] = 0;

            for (int i = 0; i < Noeuds.Count - 1; i++)
            {
                foreach (var noeud in Noeuds)
                {
                    foreach (var arc in noeud.Adjacents)
                    {
                        double alt = dist[noeud] + arc.Poids;
                        if (alt < dist[arc.Cible])
                        {
                            dist[arc.Cible] = alt;
                            pred[arc.Cible] = noeud;
                        }
                    }
                }
            }

            // Détection de cycle de poids négatif (optionnel)
            foreach (var noeud in Noeuds)
            {
                foreach (var arc in noeud.Adjacents)
                {
                    double alt = dist[noeud] + arc.Poids;
                    if (alt < dist[arc.Cible])
                    {
                        Console.WriteLine("Cycle de poids négatif détecté !");
                    }
                }
            }
            return (dist, pred);
        }
        #endregion

        #region Algorithme de Floyd-Warshall
        /// <summary>
        /// Calcule tous les plus courts chemins entre tous les couples de nœuds.
        /// Retourne une matrice de distances et un dictionnaire pour la reconstruction des chemins.
        /// </summary>
        public (Dictionary<(Noeud<T>, Noeud<T>), double> dist, Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>> next)
            FloydWarshall()
        {
            var dist = new Dictionary<(Noeud<T>, Noeud<T>), double>();
            var next = new Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>>();

            foreach (var i in Noeuds)
            {
                foreach (var j in Noeuds)
                {
                    var tuple = (i, j);
                    if (i.Equals(j))
                    {
                        dist[tuple] = 0;
                        next[tuple] = null;
                    }
                    else
                    {
                        double poids = double.PositiveInfinity;
                        foreach (var arc in i.Adjacents)
                        {
                            if (arc.Cible.Equals(j))
                            {
                                poids = arc.Poids;
                                break;
                            }
                        }
                        dist[tuple] = poids;
                        next[tuple] = (poids < double.PositiveInfinity) ? j : null;
                    }
                }
            }

            foreach (var k in Noeuds)
            {
                foreach (var i in Noeuds)
                {
                    foreach (var j in Noeuds)
                    {
                        var ij = (i, j);
                        var ik = (i, k);
                        var kj = (k, j);

                        if (dist[ik] + dist[kj] < dist[ij])
                        {
                            dist[ij] = dist[ik] + dist[kj];
                            next[ij] = next[ik];
                        }
                    }
                }
            }
            return (dist, next);
        }

        /// <summary>
        /// Reconstruit le chemin entre deux nœuds après Floyd-Warshall.
        /// </summary>
        public List<Noeud<T>> ReconstituerCheminFloyd(Noeud<T> i, Noeud<T> j,
            Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>> next)
        {
            var chemin = new List<Noeud<T>>();
            if (next[(i, j)] == null)
            {
                return chemin; // aucun chemin
            }
            var courant = i;
            while (!courant.Equals(j))
            {
                chemin.Add(courant);
                courant = next[(courant, j)];
            }
            chemin.Add(j);
            return chemin;
        }
        #endregion
    }
}

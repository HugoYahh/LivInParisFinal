using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace GraphProject
{
    /// <summary>
    /// Graphe générique pouvant être orienté ou non, avec des poids (distances, temps, etc.).
    /// Inclut méthodes de plus court chemin et coloration Welsh-Powell + export JSON/XML.
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

        /// <summary>Ajoute un nœud au graphe.</summary>
        public Noeud<T> AjouterNoeud(T valeur)
        {
            var noeud = new Noeud<T>(valeur);
            Noeuds.Add(noeud);
            return noeud;
        }

        /// <summary>Retrouve le nœud ayant la valeur donnée.</summary>
        public Noeud<T> TrouverNoeud(T valeur)
        {
            return Noeuds.Find(n => n.Valeur.Equals(valeur));
        }

        /// <summary>Ajoute un arc (liaison) entre deux nœuds (source -> cible) avec un poids.</summary>
        public void AjouterArc(T source, T cible, double poids)
        {
            var noeudSource = TrouverNoeud(source) ?? AjouterNoeud(source);
            var noeudCible = TrouverNoeud(cible) ?? AjouterNoeud(cible);

            noeudSource.Adjacents.Add(new Arc<T>(noeudCible, poids));
            if (!Oriente)
                noeudCible.Adjacents.Add(new Arc<T>(noeudSource, poids));
        }

        #region Algorithme de Dijkstra
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> predecesseurs)
            Dijkstra(Noeud<T> source)
        {
            var dist = Noeuds.ToDictionary(n => n, n => double.PositiveInfinity);
            var pred = Noeuds.ToDictionary(n => n, n => (Noeud<T>)null);
            var nonVisites = new List<Noeud<T>>(Noeuds);

            dist[source] = 0;
            while (nonVisites.Count > 0)
            {
                var u = nonVisites.OrderBy(n => dist[n]).FirstOrDefault();
                if (u == null || double.IsInfinity(dist[u])) break;
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
        public (Dictionary<Noeud<T>, double> distances, Dictionary<Noeud<T>, Noeud<T>> predecesseurs)
            BellmanFord(Noeud<T> source)
        {
            var dist = Noeuds.ToDictionary(n => n, n => double.PositiveInfinity);
            var pred = Noeuds.ToDictionary(n => n, n => (Noeud<T>)null);
            dist[source] = 0;

            for (int i = 0; i < Noeuds.Count - 1; i++)
                foreach (var noeud in Noeuds)
                    foreach (var arc in noeud.Adjacents)
                        if (dist[noeud] + arc.Poids < dist[arc.Cible])
                        {
                            dist[arc.Cible] = dist[noeud] + arc.Poids;
                            pred[arc.Cible] = noeud;
                        }

            foreach (var noeud in Noeuds)
                foreach (var arc in noeud.Adjacents)
                    if (dist[noeud] + arc.Poids < dist[arc.Cible])
                        Console.WriteLine("Cycle de poids négatif détecté !");

            return (dist, pred);
        }
        #endregion

        #region Algorithme de Floyd-Warshall
        public (Dictionary<(Noeud<T>, Noeud<T>), double> dist,
                Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>> next)
            FloydWarshall()
        {
            var dist = new Dictionary<(Noeud<T>, Noeud<T>), double>();
            var next = new Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>>();

            foreach (var i in Noeuds)
            foreach (var j in Noeuds)
            {
                var key = (i, j);
                if (i.Equals(j))
                {
                    dist[key] = 0;
                    next[key] = null;
                }
                else
                {
                    var arc = i.Adjacents.FirstOrDefault(a => a.Cible.Equals(j));
                    dist[key] = arc?.Poids ?? double.PositiveInfinity;
                    next[key] = arc != null ? j : null;
                }
            }

            foreach (var k in Noeuds)
            foreach (var i in Noeuds)
            foreach (var j in Noeuds)
            {
                var ik = (i, k);
                var kj = (k, j);
                var ij = (i, j);
                if (dist[ik] + dist[kj] < dist[ij])
                {
                    dist[ij] = dist[ik] + dist[kj];
                    next[ij] = next[ik];
                }
            }
            return (dist, next);
        }

        public List<Noeud<T>> ReconstituerCheminFloyd(Noeud<T> i, Noeud<T> j,
            Dictionary<(Noeud<T>, Noeud<T>), Noeud<T>> next)
        {
            var chemin = new List<Noeud<T>>();
            if (next[(i, j)] == null) return chemin;
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

        #region Coloration Welsh-Powell & Export

        /// <summary>
        /// Applique l'algorithme de Welsh-Powell et retourne un dictionnaire mapping chaque nœud à une couleur (1,2,...).
        /// </summary>
        public Dictionary<Noeud<T>, int> WelshPowellColoring()
        {
            // 1) Trier les nœuds par degré décroissant
            var ordre = Noeuds.OrderByDescending(n => n.Adjacents.Count).ToList();
            var coloring = new Dictionary<Noeud<T>, int>();
            int couleur = 0;

            // 2) Tant que des nœuds non coloriés restent
            while (ordre.Any(n => !coloring.ContainsKey(n)))
            {
                couleur++;
                foreach (var noeud in ordre)
                {
                    if (coloring.ContainsKey(noeud)) continue;
                    // Vérifier si un voisin a déjà cette couleur
                    bool voisinDejaColorie = noeud.Adjacents
                        .Any(a => coloring.TryGetValue(a.Cible, out int c) && c == couleur);
                    if (!voisinDejaColorie)
                        coloring[noeud] = couleur;
                }
            }
            return coloring;
        }

        /// <summary>
        /// Exporte les résultats de la coloration en JSON (node->color) dans le fichier spécifié.
        /// </summary>
        public void ExportColoringJson(string filePath)
        {
            var coloring = WelshPowellColoring();
            var exportDict = coloring.ToDictionary(
                kvp => kvp.Key.Valeur.ToString(),
                kvp => kvp.Value);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(exportDict, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Exporte les résultats de la coloration en XML (liste NodeColor) dans le fichier spécifié.
        /// </summary>
        public void ExportColoringXml(string filePath)
        {
            var coloring = WelshPowellColoring();
            var list = coloring.Select(kvp => new NodeColor
            {
                Node = kvp.Key.Valeur.ToString(),
                Color = kvp.Value
            }).ToList();
            var serializer = new XmlSerializer(typeof(List<NodeColor>));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, list);
        }

        #endregion
    }

    /// <summary>
    /// Classe auxiliaire pour la sérialisation XML de la coloration.
    /// </summary>
    public class NodeColor
    {
        public string Node { get; set; }
        public int Color { get; set; }
    }
}

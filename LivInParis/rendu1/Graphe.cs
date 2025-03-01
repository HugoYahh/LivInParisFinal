using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
//using NUnit.Framework;

namespace GraphProject
{
    public class Graphe
    {
        List< List<int>> ListAdjacence{get;}
        

#region Constructeur       
        public Graphe(string[] chemin){
            this.ListAdjacence=ListAdjacenceGraphe(chemin);
        }
        public Graphe(){
            this.ListAdjacence=new List< List<int>>();
        }
#endregion
#region Methode D'instance

        public List< List<int>> ListAdjacenceGraphe(string[] fichierAdj)
        {
            List<List<int>> adjacences = new List<List<int>>();
            List<string> Header = new List<string>(fichierAdj[0].Split(' ', StringSplitOptions.RemoveEmptyEntries));
            int taille = Convert.ToInt32(Header[0]);

            for (int i = 1; i <= taille;i++){

                List<int> AdjacenceNoeud= new List<int>();

                for(int j = 1; j < fichierAdj.Length;j++){
                    List<string> words = new List<string>(fichierAdj[j].Split(' ', StringSplitOptions.RemoveEmptyEntries));
                    int noeud1 = Convert.ToInt32(words[0]);
                    int noeud2 =  Convert.ToInt32(words[1]);
                    if(noeud1==i)
                    {
                        AdjacenceNoeud.Add(noeud2-1);
                    } 
                    else if(noeud2==i){
                        AdjacenceNoeud.Add(noeud1-1) ;
                    }
                    
                }
                
                adjacences.Add(AdjacenceNoeud);
                
            }
            for (int i = 0; i < taille; i++)
            {
                adjacences[i] = adjacences[i].Distinct().OrderBy(n => n).ToList();
            }
            

            return adjacences;
        }
    

        public int[,] MatriceAdjacence(List<List<int>> ListAdjacence)
        {
            int taille = ListAdjacence.Count;
            int[,] matrice = new int[taille, taille];

            // Parcours des nœuds
            for (int i = 0; i < taille; i++)
            {
                foreach (int voisin in ListAdjacence[i])
                {
                    matrice[i, voisin] = 1; 
                }
            }

            return matrice;
        }

     
        public void DFS(int startNode)
        {
            
            // Conversion 1-based → 0-based
            startNode = startNode - 1;
            int taille = this.ListAdjacence.Count;

            // Validation
            if (startNode < 0 || startNode >= this.ListAdjacence.Count)
            {
                throw new ArgumentException("Le nœud de départ doit être entre 1 et " + taille + " (1-based).");
            }
            
            string[] couleur = new string[taille];
            int[] dateDec = new int[taille];
            int[] dateFin = new int[taille];
            int date = 0;

            for (int i = 0; i < taille; i++)
            {
                couleur[i] = "blanc";
                dateDec[i] = -1;
                dateFin[i] = -1;
            }

            Stack<int> pile = new Stack<int>();
            pile.Push(startNode);
            couleur[startNode] = "jaune";
            dateDec[startNode] = date; // Date de découverte initiale

            Console.WriteLine("Parcours en Profondeur (DFS) :"); // indique l'ordre dans laquelle les noeuds sont decouverts

            while (pile.Count > 0)
            {
                int sommet = pile.Peek(); //Returns the top object on the stack without removing it.
                bool voisinTrouve = false; //initialise le fait que pour l'instant on ne sait pas si le noeud en court d'exploration a des voisins

                foreach (int voisin in ListAdjacence[sommet])
                {
                    if (couleur[voisin] == "blanc")
                    {
                        date++;                     // Incrémenter avant de découvrir
                        couleur[voisin] = "jaune";
                        dateDec[voisin] = date;
                        pile.Push(voisin);
                        voisinTrouve = true;
                        break;
                    }
                }

                if (!voisinTrouve) //quand tout les voisins sont explorer ou qu'il n'y en a pas/plus on passe au sommet de la pile
                {
                    pile.Pop();
                    couleur[sommet] = "rouge";
                    dateFin[sommet] = date;
                    Console.Write(sommet + " ");
                    date++; // Incrémenter après avoir terminé
                }
            }

            Console.WriteLine();
            Console.WriteLine("Dates de découverte : " + string.Join(", ", dateDec)); // affiche le temps a laquelle chaque noeud a ete decouverte. exp : si la sortie est 0,1,5 alors le premier noeud est decouvert au temps 0 et le noeud numero 3 au temps 5
            Console.WriteLine("Dates de fin : " + string.Join(", ", dateFin)); //meme principe que pour la decouverte
        }
    

        public void BFS(int startNode)
        {
            // Conversion 1-based (entrée utilisateur) → 0-based (interne)
            startNode = startNode - 1;
            int taille = this.ListAdjacence.Count;

            // Validation du nœud de départ
            if (startNode < 0 || startNode >= taille)
            {
                throw new ArgumentException("Le nœud de départ doit être entre 1 et " + taille + " (1-based).");
            }

            string[] couleur = new string[taille]; // "blanc", "jaune", "rouge" (optionnel)
            int[] distance = new int[taille];      // Distance depuis le nœud de départ
            int[] parent = new int[taille];        // Parent dans le parcours BFS. Il represente le noeud grace auquel le nom actuel a ete decouvert, grace a ca on peut trouver le plus court chemin en remontant de parent en parent jusqu'au starNode

            // Initialisation
            for (int i = 0; i < taille; i++)
            {
                couleur[i] = "blanc";
                distance[i] = -1;
                parent[i] = -1;
            }

            Queue<int> file = new Queue<int>();
            file.Enqueue(startNode); // Adds item to the tail of the queue.
            couleur[startNode] = "jaune"; // Marqué comme découvert
            distance[startNode] = 0;

            Console.WriteLine("\nParcours en Largeur (BFS) :");

            while (file.Count > 0)
            {
                int u = file.Dequeue(); //Removes the object at the head of the queue and returns it.
                Console.Write((u + 1) + " "); // Affichage en 1-based

                // Explorer tous les voisins de u
                foreach (int v in ListAdjacence[u])
                {
                    if (couleur[v] == "blanc")
                    {
                        couleur[v] = "jaune";
                        distance[v] = distance[u] + 1;
                        parent[v] = u;
                        file.Enqueue(v);
                    }
                }
                couleur[u] = "rouge"; // Marqué comme traité (optionnel)
            }

            // Affichage des résultats
            Console.WriteLine("\n\nDistances depuis le nœud de départ :");
            for (int i = 0; i < taille; i++)
            {
                Console.WriteLine($"Nœud {i + 1} : {distance[i]}");
            }

            Console.WriteLine("\nParents dans le parcours BFS :");
            for (int i = 0; i < taille; i++)
            {
                Console.WriteLine($"Nœud {i + 1} : Parent {parent[i] + 1}");
            }
        }    
    

        public bool EstConnexe()  // on teste voir si a partir d'un noeud random on peut atteindre les 33 autres noms, si c'est oui par definition le graphe est connexe sinon il ne l'est pas
        {
            if (ListAdjacence.Count == 0)
            {
                return false; // Graphe vide
            }

            bool[] visite = new bool[ListAdjacence.Count];
            Queue<int> file = new Queue<int>();
            
            // Commencer par le premier nœud (0-based)
            file.Enqueue(0);
            visite[0] = true;

            // BFS
            while (file.Count > 0)
            {
                int u = file.Dequeue();
                foreach (int v in ListAdjacence[u])
                {
                    if (!visite[v])
                    {
                        visite[v] = true;
                        file.Enqueue(v);
                    }
                }
            }

            // Vérifier si tous les nœuds sont visités
            return visite.All(v => v);
        }    
    

        public bool ACycle()
    {
        int taille = ListAdjacence.Count;
        bool[] visited = new bool[taille];

        // Vérifier chaque composante connexe du graphe
        for (int i = 0; i < taille; i++)
        {
            if (!visited[i])
            {
                if (DFSDetectCycle(i, visited, -1))
                {
                    return true; // Cycle détecté
                }
            }
        }
        return false;
    }


        private bool DFSDetectCycle(int node, bool[] visited, int parent)
    {
        visited[node] = true;

        foreach (int neighbor in ListAdjacence[node])
        {
            if (!visited[neighbor])
            {
                if (DFSDetectCycle(neighbor, visited, node))
                    return true;
            }
            else if (neighbor != parent)
            {
                return true; // Cycle détecté
            }
        }
        return false;
    }
    

        [SupportedOSPlatform("windows")] //pour empecher les erreurs car systeme.drawing ne fonctionne que sur windows
        public void VisualiserGraphe(string fileName)
        {
            // 1) On récupère le nombre de nœuds dans le graphe
            int n = ListAdjacence.Count;

            // 2) On définit la largeur et la hauteur de l'image en pixels
            int width = 800;
            int height = 800;

            // 3) On prévoit une marge autour du dessin (pour éviter que les nœuds soient collés aux bords)
            int margin = 50;

            // 4) On crée un nouvel objet Bitmap (une image en mémoire) de taille width x height
            //    Puis on récupère un objet Graphics qui permet de dessiner sur ce Bitmap.
            using (Bitmap bmp = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 5) On active l'anti-aliasing pour que les cercles et lignes soient plus lisses (moins "pixellisés")
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 6) On remplit tout l'arrière-plan de l'image en blanc
                g.Clear(Color.White);

                // --------------------------------------------------------------------
                // PHASE A : Calculer les positions des nœuds sur un cercle
                // --------------------------------------------------------------------

                // 7) On prépare un tableau de positions (x,y) pour chaque nœud
                PointF[] positions = new PointF[n];

                // 8) On calcule le centre du futur cercle (le centre de l'image)
                float centerX = width / 2f;
                float centerY = height / 2f;

                // 9) On calcule le rayon du cercle : on prend la plus petite dimension (largeur/hauteur),
                //    on divise par 2 pour avoir un rayon, puis on retranche la marge pour ne pas toucher les bords.
                float radius = Math.Min(width, height) / 2f - margin;

                // 10) On boucle sur chaque nœud i pour lui attribuer une position sur le cercle
                for (int i = 0; i < n; i++)
                {
                    // 11) On calcule l'angle en radians pour ce nœud i
                    //     (2 * PI) représente 360°, qu’on répartit sur n nœuds
                    double angle = 2 * Math.PI * i / n;

                    // 12) On calcule les coordonnées cartésiennes (x, y) sur le cercle :
                    //     x = centreX + rayon * cos(angle)
                    //     y = centreY + rayon * sin(angle)
                    float x = centerX + radius * (float)Math.Cos(angle);
                    float y = centerY + radius * (float)Math.Sin(angle);

                    // 13) On enregistre ces coordonnées dans le tableau positions
                    positions[i] = new PointF(x, y);
                }

                // --------------------------------------------------------------------
                // PHASE B : Dessiner les arêtes (lignes) entre les nœuds
                // --------------------------------------------------------------------

                // 14) On crée un stylo (Pen) de couleur noire, épaisseur 2
                using (Pen edgePen = new Pen(Color.Black, 2))
                {
                    // 15) On parcourt chaque nœud i
                    for (int i = 0; i < n; i++)
                    {
                        // 16) On parcourt la liste d'adjacence du nœud i,
                        //     c’est-à-dire tous les voisins j de i
                        foreach (int j in ListAdjacence[i])
                        {
                            // 17) Pour éviter de dessiner la même arête deux fois,
                            //     on ne la trace que si j > i (c’est un choix simple)
                            if (j > i)
                            {
                                // 18) On dessine une ligne entre la position du nœud i
                                //     et la position du nœud j
                                g.DrawLine(edgePen, positions[i], positions[j]);
                            }
                        }
                    }
                }

                // --------------------------------------------------------------------
                // PHASE C : Dessiner les nœuds (cercles colorés)
                // --------------------------------------------------------------------

                // 19) On définit la taille (diamètre) de chaque nœud en pixels
                int nodeSize = 30;

                // 20) On crée un pinceau (Brush) bleu clair pour le remplissage des cercles
                //     et un stylo (Pen) bleu foncé pour le contour
                using (Brush nodeBrush = new SolidBrush(Color.LightBlue))
                using (Pen nodePen = new Pen(Color.DarkBlue, 2))
                {
                    // 21) On prépare une police de caractères (Arial, taille 10)
                    Font font = new Font("Arial", 10);

                    // 22) On crée un pinceau noir pour dessiner le texte
                    using (Brush textBrush = new SolidBrush(Color.Black))
                    {
                        // 23) On parcourt à nouveau tous les nœuds i
                        for (int i = 0; i < n; i++)
                        {
                            // 24) On calcule le coin supérieur-gauche du rectangle
                            //     qui contiendra le cercle de diamètre nodeSize
                            float x = positions[i].X - nodeSize / 2f;
                            float y = positions[i].Y - nodeSize / 2f;

                            // 25) On remplit un cercle bleu clair
                            g.FillEllipse(nodeBrush, x, y, nodeSize, nodeSize);

                            // 26) On dessine ensuite le contour du cercle en bleu foncé
                            g.DrawEllipse(nodePen, x, y, nodeSize, nodeSize);

                            // 27) On construit le label du nœud (ex: "1", "2", etc. en 1-based)
                            string label = (i + 1).ToString();

                            // 28) On mesure la place que prendra le texte (pour bien le centrer)
                            SizeF labelSize = g.MeasureString(label, font);

                            // 29) On dessine le texte (numéro du nœud) au centre du cercle
                            g.DrawString(
                                label,                 // Le texte
                                font,                  // La police
                                textBrush,             // La couleur (noir)
                                positions[i].X - labelSize.Width / 2f,   // Coordonnée X
                                positions[i].Y - labelSize.Height / 2f   // Coordonnée Y
                            );
                        }
                    }
                }

                // --------------------------------------------------------------------
                // PHASE D : Sauvegarder l'image au format PNG
                // --------------------------------------------------------------------

                // 30) On enregistre le bitmap dans un fichier .png
                bmp.Save(fileName, ImageFormat.Png);
            }
        }


        public int GetOrder()
        {
            return ListAdjacence.Count;
        }

        
        public int GetSize()
        {
            int total = 0;
            foreach (var voisins in ListAdjacence)
            {
                total += voisins.Count;
            }
            return total / 2;
        }

        
        public bool EstOriente() // On parcourt chaque arête et on vérifie que pour tout arc (i -> j), l'arc inverse (j -> i) existe. S'il manque au moins un arc inverse,  on considère que le graphe est orienté.
        {
            for (int i = 0; i < ListAdjacence.Count; i++)
            {
                foreach (int j in ListAdjacence[i])
                {
                    if (!ListAdjacence[j].Contains(i))
                    {
                        return true; // Une arête n'est pas réciproque
                    }
                }
            }
            return false;
        }


        public bool EstPondere()
        {
            int n = ListAdjacence.Count;
            // On génère la matrice d'adjacence à partir de la liste d'adjacence
            int[,] matrice = MatriceAdjacence(ListAdjacence);

            // Parcourt de chaque élément de la matrice
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Si une valeur n'est ni 0 ni 1, alors le graphe est pondéré
                    if (matrice[i, j] != 0 && matrice[i, j] != 1)
                    {
                        return true;
                    }
                }
            }
            // Si toutes les valeurs sont 0 ou 1, le graphe n'est pas pondéré
            return false;
        }

       
        public void AfficherProprietes()
        {
            Console.WriteLine("Propriétés du graphe :");
            Console.WriteLine("Ordre (nombre de nœuds) : " + GetOrder());
            Console.WriteLine("Taille (nombre d'arêtes) : " + GetSize());
            Console.WriteLine("Type : " + (EstOriente() ? "Orienté" : "Non orienté"));
            Console.WriteLine("Pondéré : " + (EstPondere() ? "Oui" : "Non"));
            if(EstConnexe()){
                Console.WriteLine("Le graphe est connexe");
            }

            if(ACycle()){
                Console.WriteLine("Le graphe a des cycles");
            }
        }
    
    }

#endregion    
}

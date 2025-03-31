using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using GraphProject;

namespace GraphProject
{
    class Program
    {
        // Simuler un catalogue de plats (pour la commande)
        static Dictionary<string, decimal> CataloguePlats = new Dictionary<string, decimal>
        {
            { "Salade Niçoise", 8.50m },
            { "Poulet Rôti", 12.00m },
            { "Pizza Margherita", 10.00m },
            { "Burger", 9.00m },
            { "Pâtes Carbonara", 11.00m }
        };

        static void Main(string[] args)
        {
            Console.WriteLine("=== Projet LivInParis : Etape 2 ===");

            // Chemin complet vers l'Excel (contenant les feuilles Arcs et Noeuds)
            string cheminExcel = @"C:\Users\hugo3\Documents\LivInParis\MetroParis.xlsx";

            // Création du graphe en mode orienté (pour respecter les liaisons unidirectionnelles)
            var metroGraphe = new Graphe<string>(oriente: true);

            // Charger les arcs depuis la feuille "Arcs"
            ExcelImporter.ChargerArcs(metroGraphe, cheminExcel);

            // Charger les coordonnées des stations depuis la feuille "Noeuds"
            var stationCoordinates = ExcelNoeudsImporter.ChargerNoeuds(cheminExcel);

            // Affichage de vérification
            Console.WriteLine("Stations chargées dans le graphe :");
            foreach (var noeud in metroGraphe.Noeuds)
            {
                Console.WriteLine($"- {noeud.Valeur}");
            }

            // Menu principal
            bool quit = false;
            while (!quit)
            {
                Console.WriteLine("\n--- MENU PRINCIPAL ---");
                Console.WriteLine("1. Tester la connexion à la base de données");
                Console.WriteLine("2. Lister les clients");
                Console.WriteLine("3. Calculer un plus court chemin (Dijkstra)");
                Console.WriteLine("4. Calculer un plus court chemin (Bellman-Ford)");
                Console.WriteLine("5. Calculer tous les plus courts chemins (Floyd-Warshall)");
                Console.WriteLine("6. Visualiser le chemin graphique (Windows Forms)");
                Console.WriteLine("7. S'inscrire (Client/Cuisinier)");
                Console.WriteLine("8. Simulation complète (Connexion/Inscription & Prise de commande)");
                Console.WriteLine("9. Quitter");
                Console.Write("Choix : ");
                var choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        TesterBD();
                        break;
                    case "2":
                        ListerClientsBD();
                        break;
                    case "3":
                        PlusCourtCheminDijkstra(metroGraphe, stationCoordinates);
                        break;
                    case "4":
                        PlusCourtCheminBellman(metroGraphe, stationCoordinates);
                        break;
                    case "5":
                        FloydWarshallDemo(metroGraphe, stationCoordinates);
                        break;
                    case "6":
                        VisualiserCheminGraphique(metroGraphe, stationCoordinates);
                        break;
                    case "7":
                        Inscription();
                        break;
                    case "8":
                        SimulationCommandeComplete(metroGraphe, stationCoordinates);
                        break;
                    case "9":
                        quit = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
            Console.WriteLine("Fin du programme.");
        }

        // --- Méthodes BD ---
        static void TesterBD()
        {
            var dbManager = new DatabaseManager("localhost", "livinparis", "root", "1234", 3306);
            dbManager.TestConnexion();
        }

        static void ListerClientsBD()
        {
            var dbManager = new DatabaseManager("localhost", "livinparis", "root", "1234", 3306);
            dbManager.ListerClients();
        }

        // --- Méthodes de calcul de plus court chemin ---
        static void PlusCourtCheminDijkstra(Graphe<string> graphe, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();

            var noeudDepart = graphe.TrouverNoeud(depart);
            var noeudArrivee = graphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = graphe.Dijkstra(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                double distanceParcourue = CalculerDistanceChemin(chemin, coords);
                Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
                Console.WriteLine($"Distance parcourue : {distanceParcourue:F2} km");
            }
        }

        static void PlusCourtCheminBellman(Graphe<string> graphe, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();

            var noeudDepart = graphe.TrouverNoeud(depart);
            var noeudArrivee = graphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = graphe.BellmanFord(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                double distanceParcourue = CalculerDistanceChemin(chemin, coords);
                Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
                Console.WriteLine($"Distance parcourue : {distanceParcourue:F2} km");
            }
        }

        static void FloydWarshallDemo(Graphe<string> graphe, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            var (dist, next) = graphe.FloydWarshall();
            Console.WriteLine("=== Matrice des temps de trajet ===");
            foreach (var i in graphe.Noeuds)
            {
                foreach (var j in graphe.Noeuds)
                {
                    double d = dist[(i, j)];
                    if (double.IsInfinity(d))
                        Console.WriteLine($"Pas de chemin de {i.Valeur} à {j.Valeur}");
                    else
                        Console.WriteLine($"Temps de trajet de {i.Valeur} à {j.Valeur} = {d}");
                }
            }
            Console.Write("Station de départ pour reconstruction : ");
            string dep = Console.ReadLine();
            Console.Write("Station d'arrivée pour reconstruction : ");
            string arr = Console.ReadLine();
            var noeudDep = graphe.TrouverNoeud(dep);
            var noeudArr = graphe.TrouverNoeud(arr);
            if (noeudDep == null || noeudArr == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var cheminReconstitue = graphe.ReconstituerCheminFloyd(noeudDep, noeudArr, next);
            if (cheminReconstitue.Count == 0)
                Console.WriteLine("Aucun chemin trouvé.");
            else
            {
                double distanceParcourue = CalculerDistanceChemin(cheminReconstitue.ConvertAll(n => n.Valeur), coords);
                Console.WriteLine("Chemin (Floyd-Warshall) : " +
                    string.Join(" -> ", cheminReconstitue.ConvertAll(n => n.Valeur)));
                Console.WriteLine($"Distance parcourue : {distanceParcourue:F2} km");
            }
        }

        // --- Option graphique ---
        static void VisualiserCheminGraphique(Graphe<string> graphe, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();
            var noeudDepart = graphe.TrouverNoeud(depart);
            var noeudArrivee = graphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = graphe.Dijkstra(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
                return;
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                double distanceParcourue = CalculerDistanceChemin(chemin, coords);
                Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
                Console.WriteLine($"Distance parcourue : {distanceParcourue:F2} km");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GraphForm(chemin, coords));
            }
        }

        // --- Gestion de la connexion / inscription ---
        static bool ConnexionUtilisateur(string email, string motDePasse, string type)
        {
            // Dans une implémentation réelle, on exécuterait une requête SELECT sur Client ou Cuisinier.
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(motDePasse) || !email.Contains("@"))
                return false;
            // Simulation simple : si l'email contient "test", connexion réussie.
            return email.ToLower().Contains("test");
        }

        static void Inscription()
        {
            Console.WriteLine("Inscription");
            Console.Write("Êtes-vous Client ou Cuisinier ? (C/U) : ");
            string type = Console.ReadLine().Trim().ToUpper();
            Console.Write("Nom : ");
            string nom = Console.ReadLine();
            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();
            Console.Write("Adresse email : ");
            string email = Console.ReadLine().Trim();
            Console.Write("Mot de passe : ");
            string motDePasse = Console.ReadLine().Trim();
            Console.Write("Rue : ");
            string rue = Console.ReadLine();
            Console.Write("Numéro de rue : ");
            string numRue = Console.ReadLine();
            Console.Write("Code postal : ");
            int codePostal;
            while (!int.TryParse(Console.ReadLine(), out codePostal))
            {
                Console.Write("Veuillez entrer un code postal valide : ");
            }
            Console.Write("Ville : ");
            string ville = Console.ReadLine();
            Console.Write("Téléphone : ");
            string tel = Console.ReadLine();
            Console.Write("Station de métro la plus proche : ");
            string metro = Console.ReadLine();
            var dbManager = new DatabaseManager("localhost", "livinparis", "root", "1234", 3306);
            if (type == "C")
                dbManager.InsererClient(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro);
            else if (type == "U")
                dbManager.InsererCuisinier(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro);
            else
                Console.WriteLine("Type d'utilisateur non reconnu.");
        }

        // --- Simulation complète de commande avec authentification réelle ---
        static void SimulationCommandeComplete(Graphe<string> graphe, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            Console.WriteLine("=== Simulation de Commande Complète ===");
            Console.Write("Avez-vous déjà un compte ? (O/N) : ");
            string reponse = Console.ReadLine().Trim().ToUpper();

            string email = "";
            string motDePasse = "";
            string typeUtilisateur = ""; // "C" pour Client, "U" pour Cuisinier

            if (reponse == "O")
            {
                Console.Write("Vous connectez-vous en tant que Client ou Cuisinier ? (C/U) : ");
                typeUtilisateur = Console.ReadLine().Trim().ToUpper();
                Console.Write("Adresse email : ");
                email = Console.ReadLine().Trim();
                Console.Write("Mot de passe : ");
                motDePasse = Console.ReadLine().Trim();
                if (!ConnexionUtilisateur(email, motDePasse, typeUtilisateur))
                {
                    Console.WriteLine("Échec de la connexion. Fin de la simulation.");
                    return;
                }
                Console.WriteLine("Connexion réussie !");
            }
            else
            {
                Console.WriteLine("Création d'un nouveau compte.");
                Console.Write("Êtes-vous Client ou Cuisinier ? (C/U) : ");
                typeUtilisateur = Console.ReadLine().Trim().ToUpper();
                Console.Write("Nom : ");
                string nom = Console.ReadLine();
                Console.Write("Prénom : ");
                string prenom = Console.ReadLine();
                Console.Write("Adresse email : ");
                email = Console.ReadLine().Trim();
                Console.Write("Mot de passe : ");
                motDePasse = Console.ReadLine().Trim();
                Console.Write("Rue : ");
                string rue = Console.ReadLine();
                Console.Write("Numéro de rue : ");
                string numRue = Console.ReadLine();
                Console.Write("Code postal : ");
                int codePostal;
                while (!int.TryParse(Console.ReadLine(), out codePostal))
                    Console.Write("Veuillez entrer un code postal valide : ");
                Console.Write("Ville : ");
                string ville = Console.ReadLine();
                Console.Write("Téléphone : ");
                string tel = Console.ReadLine();
                Console.Write("Station de métro la plus proche : ");
                string metro = Console.ReadLine();
                var dbManager = new DatabaseManager("localhost", "livinparis", "root", "1234", 3306);
                if (typeUtilisateur == "C")
                    dbManager.InsererClient(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro);
                else if (typeUtilisateur == "U")
                    dbManager.InsererCuisinier(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro);
                else
                {
                    Console.WriteLine("Type d'utilisateur non reconnu.");
                    return;
                }
                Console.WriteLine("Inscription réussie !");
            }

            // Simulation de la prise de commande réelle
            if (typeUtilisateur == "C")
            {
                Console.WriteLine("=== Interface Client ===");
                Console.Write("Voulez-vous passer une commande ? (O/N) : ");
                string passerCommande = Console.ReadLine().Trim().ToUpper();
                if (passerCommande != "O")
                {
                    Console.WriteLine("Simulation terminée.");
                    return;
                }
                // Saisie du nombre de lignes de commande
                Console.Write("Combien de plats voulez-vous commander ? ");
                int nbLignes;
                while (!int.TryParse(Console.ReadLine(), out nbLignes))
                    Console.Write("Veuillez entrer un nombre valide : ");

                decimal montantTotal = 0;
                List<OrderLine> commande = new List<OrderLine>();
                for (int i = 0; i < nbLignes; i++)
                {
                    Console.WriteLine($"--- Ligne de commande #{i + 1} ---");
                    Console.Write("Nom du plat (choisissez parmi : ");
                    foreach (var plat in CataloguePlats.Keys)
                        Console.Write(plat + ", ");
                    Console.Write(") : ");
                    string nomPlat = Console.ReadLine().Trim();
                    if (!CataloguePlats.ContainsKey(nomPlat))
                    {
                        Console.WriteLine("Plat non reconnu, commande annulée.");
                        return;
                    }
                    Console.Write("Quantité : ");
                    int quantite;
                    while (!int.TryParse(Console.ReadLine(), out quantite))
                        Console.Write("Veuillez entrer une quantité valide : ");
                    Console.Write("Date de livraison (jj/mm/aaaa) : ");
                    string dateLivraisonStr = Console.ReadLine().Trim();
                    DateTime dateLivraison;
                    if (!DateTime.TryParseExact(dateLivraisonStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateLivraison))
                    {
                        Console.WriteLine("Date invalide, commande annulée.");
                        return;
                    }
                    Console.Write("Adresse de livraison : ");
                    string adresseLivraison = Console.ReadLine().Trim();

                    decimal prix = CataloguePlats[nomPlat];
                    montantTotal += prix * quantite;

                    commande.Add(new OrderLine
                    {
                        Plat = nomPlat,
                        Quantite = quantite,
                        DateLivraison = dateLivraison,
                        AdresseLivraison = adresseLivraison,
                        PrixUnitaire = prix
                    });
                }

                Console.WriteLine($"Montant total de la commande : {montantTotal:C}");
                // Simuler l'insertion de la commande dans la base de données
                Console.WriteLine("Commande enregistrée dans la base.");

                // Simulation de calcul du chemin pour la livraison :
                // On demande la station de métro du client et celle du cuisinier qui prendra en charge la livraison.
                Console.Write("Entrez votre station de métro (client) : ");
                string stationClient = Console.ReadLine().Trim();
                Console.Write("Entrez la station de métro du cuisinier pour la livraison : ");
                string stationCuisinier = Console.ReadLine().Trim();

                var noeudClient = graphe.TrouverNoeud(stationClient);
                var noeudCuisinier = graphe.TrouverNoeud(stationCuisinier);
                if (noeudClient == null || noeudCuisinier == null)
                {
                    Console.WriteLine("Erreur : l'une des stations saisies est introuvable dans le graphe.");
                    return;
                }

                var (distances, predecesseurs) = graphe.Dijkstra(noeudCuisinier);
                double tempsTrajet = distances[noeudClient];
                if (double.IsInfinity(tempsTrajet))
                {
                    Console.WriteLine("Aucun chemin trouvé entre les stations indiquées.");
                }
                else
                {
                    var chemin = ReconstruireChemin(noeudCuisinier, noeudClient, predecesseurs);
                    double distanceParcourue = CalculerDistanceChemin(chemin, coords);
                    Console.WriteLine("=== Résultat de la commande ===");
                    Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                    Console.WriteLine("Chemin parcouru : " + string.Join(" -> ", chemin));
                    Console.WriteLine($"Distance totale parcourue : {distanceParcourue:F2} km");
                    Console.WriteLine("Appuyez sur une touche pour visualiser le chemin graphique.");
                    Console.ReadKey();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new GraphForm(chemin, coords));
                }
            }
            else if (typeUtilisateur == "U")
            {
                Console.WriteLine("=== Interface Cuisinier ===");
                Console.Write("Entrez votre station de métro : ");
                string stationCuisinier = Console.ReadLine().Trim();
                Console.Write("Entrez la station de métro du client pour la livraison : ");
                string stationClient = Console.ReadLine().Trim();

                var noeudCuisinier = graphe.TrouverNoeud(stationCuisinier);
                var noeudClient = graphe.TrouverNoeud(stationClient);
                if (noeudCuisinier == null || noeudClient == null)
                {
                    Console.WriteLine("Erreur : l'une des stations saisies est introuvable dans le graphe.");
                    return;
                }

                var (distances, predecesseurs) = graphe.Dijkstra(noeudCuisinier);
                double tempsTrajet = distances[noeudClient];
                if (double.IsInfinity(tempsTrajet))
                {
                    Console.WriteLine("Aucun chemin trouvé entre les stations indiquées.");
                }
                else
                {
                    var chemin = ReconstruireChemin(noeudCuisinier, noeudClient, predecesseurs);
                    double distanceParcourue = CalculerDistanceChemin(chemin, coords);
                    Console.WriteLine("=== Résultat de la livraison ===");
                    Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                    Console.WriteLine("Chemin parcouru : " + string.Join(" -> ", chemin));
                    Console.WriteLine($"Distance totale parcourue : {distanceParcourue:F2} km");
                    Console.WriteLine("Appuyez sur une touche pour visualiser le chemin graphique.");
                    Console.ReadKey();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new GraphForm(chemin, coords));
                }
            }
            else
            {
                Console.WriteLine("Type d'utilisateur non reconnu pour la simulation.");
            }
        }

        // --- Méthodes utilitaires pour le graphe ---
        static List<string> ReconstruireChemin(Noeud<string> depart, Noeud<string> arrivee, Dictionary<Noeud<string>, Noeud<string>> predecesseurs)
        {
            List<string> chemin = new List<string>();
            var courant = arrivee;
            while (courant != null && !courant.Valeur.Equals(depart.Valeur))
            {
                chemin.Insert(0, courant.Valeur);
                courant = predecesseurs[courant];
            }
            if (courant == null)
                return new List<string>(); // aucun chemin trouvé
            chemin.Insert(0, depart.Valeur);
            return chemin;
        }

        static double CalculerDistanceChemin(List<string> chemin, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            double distanceTotale = 0;
            for (int i = 0; i < chemin.Count - 1; i++)
            {
                string stationA = chemin[i];
                string stationB = chemin[i + 1];
                if (coords.TryGetValue(stationA, out var coordA) && coords.TryGetValue(stationB, out var coordB))
                    distanceTotale += CalculerDistanceHaversine(coordA.Latitude, coordA.Longitude, coordB.Latitude, coordB.Longitude);
            }
            return distanceTotale;
        }

        static double CalculerDistanceHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Rayon de la Terre en km
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        static double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }
    }

    // Classe pour simuler une ligne de commande
    public class OrderLine
    {
        public string Plat { get; set; }
        public int Quantite { get; set; }
        public DateTime DateLivraison { get; set; }
        public string AdresseLivraison { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}

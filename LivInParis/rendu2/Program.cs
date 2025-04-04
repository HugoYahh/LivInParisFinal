using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using GraphProject;

namespace GraphProject
{
    class Program
    {
        // Utilisation d'un chemin relatif pour accéder au fichier Excel
        static string cheminExcel = Path.Combine(AppContext.BaseDirectory, "MetroParis.xlsx");
        static Graphe<string> metroGraphe;
        static Dictionary<string, (double Latitude, double Longitude)> stationCoordinates;
        static DatabaseManager dbManager = new DatabaseManager("localhost", "LivInParis", "root", "1234", 3306);

        // Listes simulant les plats partagés et les commandes
        static List<Plat> listePlats = new List<Plat>();
        static List<Commande> listeCommandesClient = new List<Commande>();
        static Dictionary<int, List<Commande>> commandeParCuisinier = new Dictionary<int, List<Commande>>();
        static int orderCounter = 1;

        // Utilisateur connecté (peut être Client et/ou Cuisinier)
        static Utilisateur utilisateurConnecte = null;

        static void Main(string[] args)
        {
            // Chargement du graphe et des coordonnées depuis l'Excel (à partir du répertoire de sortie)
            metroGraphe = new Graphe<string>(oriente: true);
            ExcelImporter.ChargerArcs(metroGraphe, cheminExcel);
            stationCoordinates = ExcelNoeudsImporter.ChargerNoeuds(cheminExcel);

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n=== Bienvenue sur LivInParis ===");
                Console.WriteLine("1. Inscription");
                Console.WriteLine("2. Connexion");
                Console.WriteLine("3. Admin");
                Console.WriteLine("4. Quitter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        Inscription();
                        break;
                    case "2":
                        Connexion();
                        break;
                    case "3":
                        AdminMenu();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
            Console.WriteLine("Merci d'avoir utilisé LivInParis.");
        }

        static void Inscription()
        {
            Console.WriteLine("\n=== Inscription ===");
            Console.Write("Êtes-vous Client, Cuisinier ou les 2 ? (C/U/B pour Both) : ");
            string choixRole = Console.ReadLine().Trim().ToUpper();

            Console.Write("Nom : ");
            string nom = Console.ReadLine().Trim();
            Console.Write("Prénom : ");
            string prenom = Console.ReadLine().Trim();
            Console.Write("Adresse email : ");
            string email = Console.ReadLine().Trim();
            Console.Write("Mot de passe : ");
            string motDePasse = Console.ReadLine().Trim();
            Console.Write("Rue : ");
            string rue = Console.ReadLine().Trim();
            Console.Write("Numéro de rue : ");
            string numRue = Console.ReadLine().Trim();
            Console.Write("Code postal : ");
            int codePostal;
            while (!int.TryParse(Console.ReadLine(), out codePostal))
            {
                Console.Write("Veuillez entrer un code postal valide : ");
            }
            Console.Write("Ville : ");
            string ville = Console.ReadLine().Trim();
            Console.Write("Téléphone : ");
            string tel = Console.ReadLine().Trim();
            // Vérification de la station de métro saisie via les données Excel
            Console.Write("Station de métro (votre station) : ");
            string metro = Console.ReadLine().Trim();
            while (!stationCoordinates.ContainsKey(metro))
            {
                Console.Write("La station de métro n'existe pas. Veuillez entrer une station valide : ");
                metro = Console.ReadLine().Trim();
            }

            int idClient = -1, idCuisinier = -1;
            if (choixRole == "C" || choixRole == "B")
            {
                idClient = dbManager.InsererClient(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro, motDePasse);
            }
            if (choixRole == "U" || choixRole == "B")
            {
                Console.Write("Êtes-vous un cuisinier Particulier ou une Entreprise locale ? (P/E) : ");
                string sousType = Console.ReadLine().Trim().ToUpper();
                if (sousType == "P")
                {
                    Console.Write("Informations complémentaires (optionnel) : ");
                    string infosComplementaires = Console.ReadLine().Trim();
                    idCuisinier = dbManager.InsererCuisinierParticulier(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro, motDePasse, infosComplementaires);
                }
                else if (sousType == "E")
                {
                    Console.Write("Nom de l'entreprise : ");
                    string nomEntreprise = Console.ReadLine().Trim();
                    Console.Write("Nom du référent communication (optionnel) : ");
                    string referent = Console.ReadLine().Trim();
                    idCuisinier = dbManager.InsererCuisinierEntreprise(nom, prenom, rue, numRue, codePostal, ville, tel, email, metro, motDePasse, nomEntreprise, referent);
                }
                else
                {
                    Console.WriteLine("Sous-type de cuisinier non reconnu.");
                }
            }
            if (idClient != -1)
                Console.WriteLine($"Inscription Client réussie ! Votre ID Client est : {idClient}");
            if (idCuisinier != -1)
                Console.WriteLine($"Inscription Cuisinier réussie ! Votre ID Cuisinier est : {idCuisinier}");
        }

        static void Connexion()
        {
            Console.WriteLine("\n=== Connexion ===");
            Console.Write("Adresse email : ");
            string email = Console.ReadLine().Trim();
            Console.Write("Mot de passe : ");
            string motDePasse = Console.ReadLine().Trim();
            Console.Write("Se connecter en tant que Client ou Cuisinier ? (C/U) : ");
            string role = Console.ReadLine().Trim().ToUpper();

            if (role == "C")
            {
                Utilisateur user = dbManager.ObtenirClientByEmail(email, motDePasse);
                if (user != null)
                {
                    Console.WriteLine("Connexion Client réussie !");
                    utilisateurConnecte = user;
                    MenuClient();
                }
                else
                {
                    Console.WriteLine("Échec de la connexion pour le Client.");
                }
            }
            else if (role == "U")
            {
                Utilisateur user = dbManager.ObtenirCuisinierByEmail(email, motDePasse);
                if (user != null)
                {
                    Console.WriteLine("Connexion Cuisinier réussie !");
                    utilisateurConnecte = user;
                    MenuCuisinier();
                }
                else
                {
                    Console.WriteLine("Échec de la connexion pour le Cuisinier.");
                }
            }
            else
            {
                Console.WriteLine("Rôle non reconnu.");
            }
        }

        static void MenuClient()
        {
            bool logout = false;
            while (!logout)
            {
                Console.WriteLine("\n=== Interface Client ===");
                Console.WriteLine("1. Voir les plats disponibles");
                Console.WriteLine("2. Commander un plat");
                Console.WriteLine("3. Voir mes commandes (avec état)");
                Console.WriteLine("4. Se déconnecter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        AfficherPlatsDisponibles();
                        break;
                    case "2":
                        CommanderPlat();
                        break;
                    case "3":
                        VoirMesCommandesClient();
                        break;
                    case "4":
                        logout = true;
                        utilisateurConnecte = null;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        static void MenuCuisinier()
        {
            bool logout = false;
            while (!logout)
            {
                Console.WriteLine("\n=== Interface Cuisinier ===");
                Console.WriteLine("1. Partager un plat");
                Console.WriteLine("2. Voir mes plats partagés");
                Console.WriteLine("3. Voir les commandes reçues");
                Console.WriteLine("4. Mettre à jour l'état d'une commande");
                Console.WriteLine("5. Se déconnecter");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        PartagerPlat();
                        break;
                    case "2":
                        VoirMesPlatsPartages();
                        break;
                    case "3":
                        VoirCommandesCuisinier();
                        break;
                    case "4":
                        MettreAJourEtatCommande();
                        break;
                    case "5":
                        logout = true;
                        utilisateurConnecte = null;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        static void AdminMenu()
        {
            Console.WriteLine("\n=== Interface Admin ===");
            bool exitAdmin = false;
            while (!exitAdmin)
            {
                Console.WriteLine("1. Calculer plus court chemin (Dijkstra)");
                Console.WriteLine("2. Calculer plus court chemin (Bellman-Ford)");
                Console.WriteLine("3. Calculer tous les plus courts chemins (Floyd-Warshall)");
                Console.WriteLine("4. Visualiser le chemin graphique (Windows Forms)");
                Console.WriteLine("5. Afficher classement des cuisiniers");
                Console.WriteLine("6. Retour au menu principal");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();
                switch (choix)
                {
                    case "1":
                        CalculerDijkstra();
                        break;
                    case "2":
                        CalculerBellmanFord();
                        break;
                    case "3":
                        CalculerFloydWarshall();
                        break;
                    case "4":
                        VisualiserCheminGraphiqueAdmin();
                        break;
                    case "5":
                        AfficherClassementCuisiniers();
                        break;
                    case "6":
                        exitAdmin = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        static void AfficherPlatsDisponibles()
        {
            Console.WriteLine("\n=== Liste des plats disponibles ===");
            if (listePlats.Count == 0)
            {
                Console.WriteLine("Aucun plat n'a encore été partagé.");
            }
            else
            {
                foreach (var plat in listePlats)
                {
                    Console.WriteLine($"ID: {plat.ID} - {plat.Nom} ({plat.Type}) - Prix: {plat.PrixParPersonne:C}");
                }
            }
        }

        // Lorsqu'un client commande, la station du cuisinier est prise depuis le plat partagé
        static void CommanderPlat()
        {
            AfficherPlatsDisponibles();
            if (listePlats.Count == 0)
                return;
            Console.Write("Entrez l'ID du plat à commander : ");
            int idPlat;
            if (!int.TryParse(Console.ReadLine(), out idPlat))
            {
                Console.WriteLine("ID invalide.");
                return;
            }
            var plat = listePlats.Find(p => p.ID == idPlat);
            if (plat == null)
            {
                Console.WriteLine("Plat non trouvé.");
                return;
            }
            Console.Write("Quantité : ");
            int quantite;
            while (!int.TryParse(Console.ReadLine(), out quantite))
            {
                Console.Write("Veuillez entrer une quantité valide : ");
            }
            decimal montantTotal = plat.PrixParPersonne * quantite;
            Console.WriteLine($"Montant total de la commande : {montantTotal:C}");

            // Le client indique sa station de métro
            Console.Write("Entrez votre station de métro : ");
            string stationClient = Console.ReadLine().Trim();
            var noeudClient = metroGraphe.TrouverNoeud(stationClient);
            var noeudCuisinier = metroGraphe.TrouverNoeud(plat.StationCuisinier);
            if (noeudClient == null || noeudCuisinier == null)
            {
                Console.WriteLine("Erreur : station(s) introuvable(s) dans le graphe.");
                return;
            }
            var (distances, predecesseurs) = metroGraphe.Dijkstra(noeudCuisinier);
            double tempsTrajet = distances[noeudClient];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé entre les stations indiquées.");
                return;
            }
            else
            {
                var chemin = ReconstruireChemin(noeudCuisinier, noeudClient, predecesseurs);
                double distanceParcourue = CalculerDistanceChemin(chemin, stationCoordinates);
                Console.WriteLine("=== Détails de la commande ===");
                Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                Console.WriteLine("Chemin parcouru : " + string.Join(" -> ", chemin));
                Console.WriteLine($"Distance totale parcourue : {distanceParcourue:F2} km");

                // Enregistrement de la commande avec état initial "En attente"
                Commande commande = new Commande
                {
                    ID = orderCounter++,
                    Plat = plat.Nom,
                    Quantite = quantite,
                    DateLivraison = DateTime.Now,
                    AdresseLivraison = stationClient,
                    TempsTrajet = tempsTrajet,
                    Chemin = chemin,
                    Distance = distanceParcourue,
                    Etat = "En attente"
                };
                listeCommandesClient.Add(commande);
                if (!commandeParCuisinier.ContainsKey(plat.IDCuisinier))
                    commandeParCuisinier[plat.IDCuisinier] = new List<Commande>();
                commandeParCuisinier[plat.IDCuisinier].Add(commande);

                Console.WriteLine("Commande enregistrée.");
                Console.WriteLine("Appuyez sur une touche pour visualiser le trajet.");
                Console.ReadKey();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GraphForm(chemin, stationCoordinates));
            }
        }

        static void PartagerPlat()
        {
            Console.WriteLine("\n=== Partager un nouveau plat ===");
            Console.Write("Nom du plat : ");
            string nomPlat = Console.ReadLine().Trim();
            Console.Write("Type de plat (Entrée / Plat principal / Dessert) : ");
            string typePlat = Console.ReadLine().Trim();
            Console.Write("Pour combien de personnes ? ");
            int nbPortions;
            while (!int.TryParse(Console.ReadLine(), out nbPortions))
            {
                Console.Write("Veuillez entrer un nombre valide : ");
            }
            Console.Write("Date de fabrication (jj/mm/aaaa) : ");
            DateTime dateFabrication;
            if (!DateTime.TryParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFabrication))
            {
                Console.WriteLine("Date invalide.");
                return;
            }
            Console.Write("Date de péremption (jj/mm/aaaa) : ");
            DateTime datePeremption;
            if (!DateTime.TryParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out datePeremption))
            {
                Console.WriteLine("Date invalide.");
                return;
            }
            Console.Write("Prix par personne : ");
            decimal prix;
            while (!decimal.TryParse(Console.ReadLine(), out prix))
            {
                Console.Write("Veuillez entrer un prix valide : ");
            }
            Console.Write("Nationalité de la cuisine (ex. : chinoise, mexicaine) : ");
            string nationalite = Console.ReadLine().Trim();
            Console.Write("Régime alimentaire (végétarien, sans gluten, halal, etc.) : ");
            string regime = Console.ReadLine().Trim();
            Console.Write("Ingrédients principaux : ");
            string ingredients = Console.ReadLine().Trim();

            int idCuisinier = utilisateurConnecte.ID;
            dbManager.InsererPlat(nomPlat, typePlat, nbPortions, dateFabrication, datePeremption, prix, nationalite, regime, ingredients, idCuisinier);

            int newID = (listePlats.Count > 0) ? listePlats[listePlats.Count - 1].ID + 1 : 1;
            listePlats.Add(new Plat
            {
                ID = newID,
                Nom = nomPlat,
                Type = typePlat,
                NbPortions = nbPortions,
                DateFabrication = dateFabrication,
                DatePeremption = datePeremption,
                PrixParPersonne = prix,
                NationaliteCuisine = nationalite,
                RegimeAlimentaire = regime,
                Ingredients = ingredients,
                IDCuisinier = idCuisinier,
                StationCuisinier = utilisateurConnecte.MetroProche
            });
            Console.WriteLine("Plat partagé avec succès !");
        }

        static void VoirMesPlatsPartages()
        {
            Console.WriteLine("\n=== Mes plats partagés ===");
            int idCuisinier = utilisateurConnecte.ID;
            var mesPlats = listePlats.FindAll(p => p.IDCuisinier == idCuisinier);
            if (mesPlats.Count == 0)
            {
                Console.WriteLine("Aucun plat partagé.");
            }
            else
            {
                foreach (var plat in mesPlats)
                {
                    Console.WriteLine($"ID: {plat.ID} - {plat.Nom} ({plat.Type}) - Prix: {plat.PrixParPersonne:C}");
                }
            }
        }

        static void VoirMesCommandesClient()
        {
            Console.WriteLine("\n=== Mes commandes ===");
            if (listeCommandesClient.Count == 0)
            {
                Console.WriteLine("Aucune commande enregistrée.");
            }
            else
            {
                foreach (var commande in listeCommandesClient)
                {
                    Console.WriteLine($"Commande #{commande.ID}: Plat: {commande.Plat}, Quantité: {commande.Quantite}, Etat: {commande.Etat}, Temps trajet: {commande.TempsTrajet}, Distance: {commande.Distance:F2} km");
                }
            }
        }

        static void VoirCommandesCuisinier()
        {
            Console.WriteLine("\n=== Commandes reçues ===");
            int idCuisinier = utilisateurConnecte.ID;
            if (!commandeParCuisinier.ContainsKey(idCuisinier) || commandeParCuisinier[idCuisinier].Count == 0)
            {
                Console.WriteLine("Aucune commande reçue.");
            }
            else
            {
                foreach (var commande in commandeParCuisinier[idCuisinier])
                {
                    Console.WriteLine($"Commande #{commande.ID}: Plat: {commande.Plat}, Quantité: {commande.Quantite}, Etat: {commande.Etat}, Temps trajet: {commande.TempsTrajet}, Distance: {commande.Distance:F2} km");
                }
                Console.WriteLine("Souhaitez-vous visualiser le trajet d'une commande ? (O/N)");
                string rep = Console.ReadLine().Trim().ToUpper();
                if (rep == "O")
                {
                    Console.Write("Entrez l'ID de la commande : ");
                    int idCommande;
                    if (int.TryParse(Console.ReadLine(), out idCommande))
                    {
                        var commande = commandeParCuisinier[idCuisinier].Find(c => c.ID == idCommande);
                        if (commande != null)
                        {
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new GraphForm(commande.Chemin, stationCoordinates));
                        }
                        else
                        {
                            Console.WriteLine("Commande non trouvée.");
                        }
                    }
                }
            }
        }

        // Méthode pour que le cuisinier mette à jour l'état d'une commande
        static void MettreAJourEtatCommande()
        {
            Console.WriteLine("\n=== Mise à jour de l'état d'une commande ===");
            int idCuisinier = utilisateurConnecte.ID;
            if (!commandeParCuisinier.ContainsKey(idCuisinier) || commandeParCuisinier[idCuisinier].Count == 0)
            {
                Console.WriteLine("Aucune commande reçue.");
                return;
            }
            foreach (var commande in commandeParCuisinier[idCuisinier])
            {
                Console.WriteLine($"Commande #{commande.ID}: Plat: {commande.Plat}, Quantité: {commande.Quantite}, Etat actuel: {commande.Etat}");
            }
            Console.Write("Entrez l'ID de la commande à mettre à jour : ");
            int idCmd;
            if (!int.TryParse(Console.ReadLine(), out idCmd))
            {
                Console.WriteLine("ID invalide.");
                return;
            }
            var cmdToUpdate = commandeParCuisinier[idCuisinier].Find(c => c.ID == idCmd);
            if (cmdToUpdate == null)
            {
                Console.WriteLine("Commande non trouvée.");
                return;
            }
            Console.Write("Nouvel état (ex. 'En cours de livraison', 'Livrée') : ");
            string nouvelEtat = Console.ReadLine().Trim();
            cmdToUpdate.Etat = nouvelEtat;
            Console.WriteLine("L'état de la commande a été mis à jour.");
        }

        // Fonctions Admin (démonstration des algorithmes de plus court chemin)
        static void CalculerDijkstra()
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();
            var noeudDepart = metroGraphe.TrouverNoeud(depart);
            var noeudArrivee = metroGraphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = metroGraphe.Dijkstra(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                Console.WriteLine($"Temps de trajet (Dijkstra) : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
            }
        }

        static void CalculerBellmanFord()
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();
            var noeudDepart = metroGraphe.TrouverNoeud(depart);
            var noeudArrivee = metroGraphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = metroGraphe.BellmanFord(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                Console.WriteLine($"Temps de trajet (Bellman-Ford) : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
            }
        }

        static void CalculerFloydWarshall()
        {
            var (dist, next) = metroGraphe.FloydWarshall();
            Console.WriteLine("=== Matrice des temps de trajet ===");
            foreach (var i in metroGraphe.Noeuds)
            {
                foreach (var j in metroGraphe.Noeuds)
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
            var noeudDep = metroGraphe.TrouverNoeud(dep);
            var noeudArr = metroGraphe.TrouverNoeud(arr);
            if (noeudDep == null || noeudArr == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var cheminReconstitue = metroGraphe.ReconstituerCheminFloyd(noeudDep, noeudArr, next);
            if (cheminReconstitue.Count == 0)
                Console.WriteLine("Aucun chemin trouvé.");
            else
            {
                Console.WriteLine("Chemin (Floyd-Warshall) : " + string.Join(" -> ", cheminReconstitue.ConvertAll(n => n.Valeur)));
            }
        }

        static void VisualiserCheminGraphiqueAdmin()
        {
            Console.Write("Station de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Station d'arrivée : ");
            string arrivee = Console.ReadLine();
            var noeudDepart = metroGraphe.TrouverNoeud(depart);
            var noeudArrivee = metroGraphe.TrouverNoeud(arrivee);
            if (noeudDepart == null || noeudArrivee == null)
            {
                Console.WriteLine("Station(s) inconnue(s).");
                return;
            }
            var (distances, predecesseurs) = metroGraphe.Dijkstra(noeudDepart);
            double tempsTrajet = distances[noeudArrivee];
            if (double.IsInfinity(tempsTrajet))
            {
                Console.WriteLine("Aucun chemin trouvé.");
            }
            else
            {
                var chemin = ReconstruireChemin(noeudDepart, noeudArrivee, predecesseurs);
                Console.WriteLine($"Temps de trajet : {tempsTrajet}");
                Console.WriteLine("Chemin : " + string.Join(" -> ", chemin));
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GraphForm(chemin, stationCoordinates));
            }
        }

        // Affichage du classement des cuisiniers (basé sur le nombre de commandes dont l'état est "Livrée")
        static void AfficherClassementCuisiniers()
        {
            Console.WriteLine("\n=== Classement des cuisiniers ===");
            Dictionary<int, int> classement = new Dictionary<int, int>();
            foreach (var kvp in commandeParCuisinier)
            {
                int idCuisinier = kvp.Key;
                int deliveredCount = 0;
                foreach (var commande in kvp.Value)
                {
                    if (commande.Etat.Equals("Livrée", StringComparison.OrdinalIgnoreCase))
                        deliveredCount++;
                }
                classement[idCuisinier] = deliveredCount;
            }
            // Tri décroissant par nombre de commandes livrées
            foreach (var item in SortedByValueDescending(classement))
            {
                Console.WriteLine($"Cuisinier ID {item.Key} : {item.Value} commande(s) livrée(s)");
            }
        }

        // Méthode utilitaire pour trier un dictionnaire par valeur en ordre décroissant
        static IEnumerable<KeyValuePair<int, int>> SortedByValueDescending(Dictionary<int, int> dict)
        {
            var list = new List<KeyValuePair<int, int>>(dict);
            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            return list;
        }

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

    // Classe représentant un plat partagé (ajout de la propriété StationCuisinier)
    public class Plat
    {
        public int ID { get; set; }
        public string Nom { get; set; }
        public string Type { get; set; }
        public int NbPortions { get; set; }
        public DateTime DateFabrication { get; set; }
        public DateTime DatePeremption { get; set; }
        public decimal PrixParPersonne { get; set; }
        public string NationaliteCuisine { get; set; }
        public string RegimeAlimentaire { get; set; }
        public string Ingredients { get; set; }
        public int IDCuisinier { get; set; }
        public string StationCuisinier { get; set; }
    }

    // Classe représentant une commande (ajout de la propriété Etat)
    public class Commande
    {
        public int ID { get; set; }
        public string Plat { get; set; }
        public int Quantite { get; set; }
        public DateTime DateLivraison { get; set; }
        public string AdresseLivraison { get; set; }
        public double TempsTrajet { get; set; }
        public List<string> Chemin { get; set; }
        public double Distance { get; set; }
        public string Etat { get; set; } // ex. "En attente", "En cours de livraison", "Livrée"
    }

    // Classe représentant un utilisateur
    public class Utilisateur
    {
        public int ID { get; set; }
        public string Type { get; set; } // "Client", "Cuisinier" ou "Admin"
        public string SousType { get; set; } // Pour cuisinier : "Particulier" ou "Entreprise"
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string MetroProche { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GraphProject
{
    class Program
    {
        /// <summary>
        /// Chemin relatif vers l’Excel (Build Action = Content, Copy to Output Directory)
        static string cheminExcel = Path.Combine(AppContext.BaseDirectory, "MetroParis.xlsx");
        static Graphe<string> metroGraphe;
        static Dictionary<string, (double Latitude, double Longitude)> stationCoordinates;
        static DatabaseManager dbManager = new DatabaseManager("localhost", "LivInParis", "root", "1234", 3306);

        static List<Plat> listePlats = new List<Plat>();
        static List<Commande> listeCommandesClient = new List<Commande>();
        static Dictionary<int, List<Commande>> commandeParCuisinier = new Dictionary<int, List<Commande>>();
        static int orderCounter = 1;

        static Utilisateur utilisateurConnecte = null;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            metroGraphe = new Graphe<string>(oriente: true);
            ExcelImporter.ChargerArcs(metroGraphe, cheminExcel);
            stationCoordinates = ExcelNoeudsImporter.ChargerNoeuds(cheminExcel);

            listeCommandesClient = dbManager.ObtenirToutesCommandes();
            /// <summary>
            /// Reconstruire commandeParCuisinier EN MEMOIRE à partir de la seule propriété IDCuisinier
            commandeParCuisinier = listeCommandesClient
                .GroupBy(c => c.IDCuisinier)
                .ToDictionary(g => g.Key, g => g.ToList());


            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n=== Bienvenue sur LivInParis ===");
                Console.WriteLine("1. Inscription");
                Console.WriteLine("2. Connexion");
                Console.WriteLine("3. Admin");
                Console.WriteLine("4. Quitter");
                Console.Write("Choix : ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": Inscription(); break;
                    case "2": Connexion();    break;
                    case "3": AdminMenu();    break;
                    case "4": exit = true;    break;
                    default:  Console.WriteLine("Choix invalide."); break;
                }
            }
            Console.WriteLine("Merci d'avoir utilisé LivInParis.");
        }

        static void Inscription()
        {
            Console.WriteLine("\n=== Inscription ===");
            Console.Write("Rôle (C=Client, U=Cuisinier, B=Both) : ");
            var role = Console.ReadLine().Trim().ToUpper();

            Console.Write("Nom : ");
            var nom = Console.ReadLine().Trim();
            Console.Write("Prénom : ");
            var prenom = Console.ReadLine().Trim();
            Console.Write("Email : ");
            var email = Console.ReadLine().Trim();
            Console.Write("Mot de passe : ");
            var mdp = Console.ReadLine().Trim();
            Console.Write("Rue : ");
            var rue = Console.ReadLine().Trim();
            Console.Write("Numéro de rue : ");
            var numRue = Console.ReadLine().Trim();

            Console.Write("Code postal : ");
            int cp;
            while (!int.TryParse(Console.ReadLine(), out cp))
                Console.Write("Code postal invalide, réessayez : ");

            Console.Write("Ville : ");
            var ville = Console.ReadLine().Trim();
            Console.Write("Téléphone : ");
            var tel = Console.ReadLine().Trim();

            Console.Write("Station métro : ");
            string metro;
            while (!stationCoordinates.ContainsKey(metro = Console.ReadLine().Trim()))
                Console.Write("Station inconnue, réessayez : ");

            int idClient = -1, idCuisinier = -1;
            if (role == "C" || role == "B")
                idClient = dbManager.InsererClient(nom, prenom, rue, numRue, cp, ville, tel, email, metro, mdp);

            if (role == "U" || role == "B")
            {
                Console.Write("Particulier (P) ou Entreprise (E) ? ");
                var st = Console.ReadLine().Trim().ToUpper();
                if (st == "P")
                {
                    Console.Write("Infos complémentaires : ");
                    var infos = Console.ReadLine().Trim();
                    idCuisinier = dbManager.InsererCuisinierParticulier(nom, prenom, rue, numRue, cp, ville, tel, email, metro, mdp, infos);
                }
                else if (st == "E")
                {
                    Console.Write("Nom entreprise : ");
                    var ne = Console.ReadLine().Trim();
                    Console.Write("Référent comm. : ");
                    var rc = Console.ReadLine().Trim();
                    idCuisinier = dbManager.InsererCuisinierEntreprise(nom, prenom, rue, numRue, cp, ville, tel, email, metro, mdp, ne, rc);
                }
                else
                    Console.WriteLine("Sous-type non reconnu.");
            }

            if (idClient > 0)    Console.WriteLine($"→ ID Client : {idClient}");
            if (idCuisinier > 0) Console.WriteLine($"→ ID Cuisinier : {idCuisinier}");
        }

        static void Connexion()
        {
            Console.WriteLine("\n=== Connexion ===");
            Console.Write("Email : ");
            var email = Console.ReadLine().Trim();
            Console.Write("Mot de passe : ");
            var mdp   = Console.ReadLine().Trim();
            Console.Write("Client (C) ou Cuisinier (U) ? ");
            var role  = Console.ReadLine().Trim().ToUpper();

            if (role == "C")
            {
                var u = dbManager.ObtenirClientByEmail(email, mdp);
                if (u != null) { utilisateurConnecte = u; MenuClient(); }
                else          Console.WriteLine("Échec connexion Client.");
            }
            else if (role == "U")
            {
                var u = dbManager.ObtenirCuisinierByEmail(email, mdp);
                if (u != null) { utilisateurConnecte = u; MenuCuisinier(); }
                else          Console.WriteLine("Échec connexion Cuisinier.");
            }
            else
                Console.WriteLine("Rôle non reconnu.");
        }

        static void MenuClient()
        {
            bool logout = false;
            while (!logout)
            {
                Console.WriteLine("\n--- Menu Client ---");
                Console.WriteLine("1. Voir plats");
                Console.WriteLine("2. Commander");
                Console.WriteLine("3. Historique & état");
                Console.WriteLine("4. Modifier mon compte");
                Console.WriteLine("5. Supprimer mon compte");
                Console.WriteLine("6. Déconnexion");
                Console.Write("Choix : ");
                switch (Console.ReadLine().Trim())
                {
                    case "1": AfficherPlatsDisponibles();  break;
                    case "2": CommanderPlat();             break;
                    case "3": VoirMesCommandesClient();    break;
                    case "4": ModifierCompte();            break;
                    case "5": SupprimerCompte(); logout = true; break;
                    case "6": logout = true; utilisateurConnecte = null; break;
                    default:  Console.WriteLine("Choix invalide."); break;
                }
            }
        }

        static void MenuCuisinier()
        {
            bool logout = false;
            while (!logout)
            {
                Console.WriteLine("\n--- Menu Cuisinier ---");
                Console.WriteLine("1. Partager un plat");
                Console.WriteLine("2. Voir mes plats partagés");
                Console.WriteLine("3. Voir commandes reçues");
                Console.WriteLine("4. Mettre à jour état");
                Console.WriteLine("5. Modifier mon compte");
                Console.WriteLine("6. Supprimer mon compte");
                Console.WriteLine("7. Déconnexion");
                Console.Write("Choix : ");
                switch (Console.ReadLine().Trim())
                {
                    case "1": PartagerPlat();            break;
                    case "2": VoirMesPlatsPartages();    break;
                    case "3": VoirCommandesCuisinier();  break;
                    case "4": MettreAJourEtatCommande(); break;
                    case "5": ModifierCompte();          break;
                    case "6": SupprimerCompte(); logout = true; break;
                    case "7": logout = true; utilisateurConnecte = null; break;
                    default:  Console.WriteLine("Choix invalide."); break;
                }
            }
        }

        static void AdminMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n--- Menu Admin ---");
                Console.WriteLine("1. Dijkstra");
                Console.WriteLine("2. Bellman-Ford");
                Console.WriteLine("3. Floyd-Warshall");
                Console.WriteLine("4. Visualiser trajet");
                Console.WriteLine("5. Classement cuisiniers");
                Console.WriteLine("6. Coloration graphe RATP");
                Console.WriteLine("7. Coloration graphe Clients↔Cuisiniers");
                Console.WriteLine("8. Statistiques");
                Console.WriteLine("9. Retour");
                Console.Write("Choix : ");
                switch (Console.ReadLine().Trim())
                {
                    case "1": CalculerDijkstra();            break;
                    case "2": CalculerBellmanFord();         break;
                    case "3": CalculerFloydWarshall();       break;
                    case "4": VisualiserCheminGraphiqueAdmin(); break;
                    case "5": AfficherClassementCuisiniers();   break;
                    case "6": PerformGraphColoring();            break; //// existant : stations
                    case "7": ColorerGrapheClientsCuisiniers();  break; //// nouveau
                    case "8": ModuleStatistiques();               break;
                    case "9": exit = true;                        break;
                    default:  Console.WriteLine("Choix invalide."); break;
                
                }
            }
        }

        static void ModifierCompte()
        {
            Console.WriteLine("\n=== Modifier mon compte ===");
            Console.Write($"Nom    (actuel: {utilisateurConnecte.Nom})    : ");
            var nom = Console.ReadLine().Trim();
            Console.Write($"Prénom (actuel: {utilisateurConnecte.Prenom}) : ");
            var prenom = Console.ReadLine().Trim();
            Console.Write($"Téléphone (actuel: {utilisateurConnecte.MetroProche}) : ");
            var tel = Console.ReadLine().Trim();

            if (utilisateurConnecte.Type == "Client" || utilisateurConnecte.Type == "Both")
                dbManager.MettreAJourClient(utilisateurConnecte.ID, nom, prenom, tel);

            if (utilisateurConnecte.Type == "Cuisinier" || utilisateurConnecte.Type == "Both")
                dbManager.MettreAJourCuisinier(utilisateurConnecte.ID, nom, prenom, tel);

            if (!string.IsNullOrEmpty(nom))    utilisateurConnecte.Nom = nom;
            if (!string.IsNullOrEmpty(prenom)) utilisateurConnecte.Prenom = prenom;
            if (!string.IsNullOrEmpty(tel))    utilisateurConnecte.MetroProche = tel;

            Console.WriteLine("Compte mis à jour avec succès.");
        }

        static void SupprimerCompte()
        {
            Console.Write("Confirmez-vous la suppression de votre compte ? (O/N) : ");
            if (Console.ReadLine().Trim().ToUpper() == "O")
            {
                if (utilisateurConnecte.Type == "Client" || utilisateurConnecte.Type == "Both")
                    dbManager.SupprimerClient(utilisateurConnecte.ID);
                if (utilisateurConnecte.Type == "Cuisinier" || utilisateurConnecte.Type == "Both")
                    dbManager.SupprimerCuisinier(utilisateurConnecte.ID);

                Console.WriteLine("Compte supprimé.");
                utilisateurConnecte = null;
            }
            else
            {
                Console.WriteLine("Suppression annulée.");
            }
        }

        static void ModuleStatistiques()
        {
            Console.WriteLine("\n=== Module Statistiques ===");
            Console.WriteLine("1. Nombre de livraisons par cuisinier");
            Console.WriteLine("2. Commandes par période");
            Console.WriteLine("3. Moyenne des montants des commandes");
            Console.WriteLine("4. Moyenne des commandes par client");
            Console.WriteLine("5. Commandes par nationalité et période");
            Console.WriteLine("6. Retour");
            Console.Write("Choix : ");
            switch (Console.ReadLine().Trim())
            {
                case "1": Stat_LivraisonsParCuisinier();      break;
                case "2": Stat_CommandesParPeriode();         break;
                case "3": Stat_MoyenneMontantsCommandes();    break;
                case "4": Stat_MoyenneCommandesParClient();   break;
                case "5": Stat_CommandesParClientEtNat();     break;
                case "6": return;
                default: Console.WriteLine("Choix invalide."); break;
            }
        }

        static void Stat_LivraisonsParCuisinier()
        {
            Console.WriteLine("\n--- Nombre de livraisons par cuisinier ---");
            foreach (var kv in commandeParCuisinier)
            {
                int cid = kv.Key;
                int count = kv.Value.Count(c => c.Etat.Equals("Livrée", StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"Cuisinier {cid} : {count} livraison(s) livrée(s)");
            }
        }

        static void Stat_CommandesParPeriode()
        {
            Console.Write("Date début (dd/MM/yyyy) : ");
            var d1 = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            Console.Write("Date fin   (dd/MM/yyyy) : ");
            var d2 = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var cmds = listeCommandesClient
                .Where(c => c.DateLivraison.Date >= d1.Date && c.DateLivraison.Date <= d2.Date)
                .ToList();
            Console.WriteLine($"\nCommandes entre {d1:dd/MM/yyyy} et {d2:dd/MM/yyyy} : {cmds.Count}");
            foreach (var c in cmds)
                Console.WriteLine($"#{c.ID} {c.Plat}×{c.Quantite} Montant:{c.MontantTotal:C} Station:{c.AdresseLivraison}");
        }

        static void Stat_MoyenneMontantsCommandes()
        {
            if (!listeCommandesClient.Any())
            {
                Console.WriteLine("Aucune commande.");
                return;
            }
            var avg = listeCommandesClient.Average(c => (double)c.MontantTotal);
            Console.WriteLine($"\nMoyenne des montants : {avg:C2}");
        }

        static void Stat_MoyenneCommandesParClient()
        {
            var groups = listeCommandesClient.GroupBy(c => c.AdresseLivraison);
            var avg = groups.Any() ? groups.Average(g => g.Count()) : 0;
            Console.WriteLine($"\nMoyenne des commandes par client (station) : {avg:F2}");
        }

        static void Stat_CommandesParClientEtNat()
        {
            Console.Write("Nationalité cuisine : ");
            var nat = Console.ReadLine().Trim();
            Console.Write("Date début (dd/MM/yyyy) : ");
            var d1 = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            Console.Write("Date fin   (dd/MM/yyyy) : ");
            var d2 = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var cmds = listeCommandesClient
                .Where(c =>
                    c.DateLivraison.Date >= d1.Date &&
                    c.DateLivraison.Date <= d2.Date &&
                    listePlats.Any(p => p.Nom == c.Plat && p.NationaliteCuisine.Equals(nat, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            Console.WriteLine($"\nCommandes ({nat}) du {d1:dd/MM/yyyy} au {d2:dd/MM/yyyy} : {cmds.Count}");
            foreach (var c in cmds)
                Console.WriteLine($"#{c.ID} {c.Plat}×{c.Quantite} Montant:{c.MontantTotal:C}");
        }

        static void AfficherPlatsDisponibles()
        {
            Console.WriteLine("\n--- Plats disponibles ---");
            if (!listePlats.Any())
                Console.WriteLine("Aucun plat.");
            else
                listePlats.ForEach(p => Console.WriteLine($"ID {p.ID}: {p.Nom} ({p.Type}) — {p.PrixParPersonne:C}"));
        }

        static void CommanderPlat()
        {
            AfficherPlatsDisponibles();
            if (!listePlats.Any()) return;

            Console.Write("ID du plat : ");
            if (!int.TryParse(Console.ReadLine(), out int idPlat) || listePlats.All(p => p.ID != idPlat))
            {
                Console.WriteLine("ID invalide."); return;
            }
            var plat = listePlats.First(p => p.ID == idPlat);

            Console.Write("Quantité : ");
            if (!int.TryParse(Console.ReadLine(), out int quantite))
            {
                Console.WriteLine("Quantité invalide."); return;
            }

            Console.Write("Votre station métro : ");
            string stationClient = Console.ReadLine().Trim();
            var nc = metroGraphe.TrouverNoeud(stationClient);
            var nu = metroGraphe.TrouverNoeud(plat.StationCuisinier);
            if (nc == null || nu == null)
            {
                Console.WriteLine("Station introuvable."); return;
            }

            var (dist, pred) = metroGraphe.Dijkstra(nu);
            double temps = dist[nc];
            if (double.IsInfinity(temps))
            {
                Console.WriteLine("Pas de chemin."); return;
            }
            var chemin = ReconstruireChemin(nu, nc, pred);
            double distance = CalculerDistanceChemin(chemin, stationCoordinates);
            decimal montantTotal = plat.PrixParPersonne * quantite;

            Console.WriteLine($"\nTemps trajet : {temps}");
            Console.WriteLine($"Distance totale : {distance:F2} km");
            Console.WriteLine($"Montant total : {montantTotal:C}");
            Console.WriteLine("Trajet : " + string.Join(" → ", chemin));

            var cmd = new Commande
            {
                ID = orderCounter++,
                Plat = plat.Nom,
                Quantite = quantite,
                DateLivraison = DateTime.Now,
                AdresseLivraison = stationClient,
                TempsTrajet = temps,
                Chemin = chemin,
                Distance = distance,
                Etat = "En attente",
                MontantTotal = montantTotal
            };
            listeCommandesClient.Add(cmd);
            if (!commandeParCuisinier.ContainsKey(plat.IDCuisinier))
                commandeParCuisinier[plat.IDCuisinier] = new List<Commande>();
            commandeParCuisinier[plat.IDCuisinier].Add(cmd);

            Console.WriteLine("\nAppuyez sur une touche pour visualiser le trajet...");
            Console.ReadKey();
            Application.Run(new GraphForm(chemin, stationCoordinates));
        }

        static void PartagerPlat()
        {
            Console.WriteLine("\n--- Partager un plat ---");
            Console.Write("Nom : ");
            var nom = Console.ReadLine().Trim();
            Console.Write("Type (Entrée/Plat principal/Dessert) : ");
            var type = Console.ReadLine().Trim();
            Console.Write("Pour combien de personnes ? ");
            var nb = int.Parse(Console.ReadLine().Trim());
            Console.Write("Date fabrication (dd/MM/yyyy) : ");
            var df = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            Console.Write("Date péremption    (dd/MM/yyyy) : ");
            var dp = DateTime.ParseExact(Console.ReadLine().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            Console.Write("Prix par personne : ");
            var pr = decimal.Parse(Console.ReadLine().Trim());
            Console.Write("Nationalité cuisine : ");
            var nat = Console.ReadLine().Trim();
            Console.Write("Régime alimentaire  : ");
            var reg = Console.ReadLine().Trim();
            Console.Write("Ingrédients        : ");
            var ing = Console.ReadLine().Trim();

            int idc = utilisateurConnecte.ID;
            dbManager.InsererPlat(nom, type, nb, df, dp, pr, nat, reg, ing, idc);

            int newID = listePlats.Any() ? listePlats.Max(p => p.ID) + 1 : 1;
            listePlats.Add(new Plat
            {
                ID = newID,
                Nom = nom,
                Type = type,
                NbPortions = nb,
                DateFabrication = df,
                DatePeremption = dp,
                PrixParPersonne = pr,
                NationaliteCuisine = nat,
                RegimeAlimentaire = reg,
                Ingredients = ing,
                IDCuisinier = idc,
                StationCuisinier = utilisateurConnecte.MetroProche
            });
            Console.WriteLine("Plat partagé avec succès !");
        }

        static void VoirMesPlatsPartages()
        {
            Console.WriteLine("\n--- Mes plats partagés ---");
            var mes = listePlats.Where(p => p.IDCuisinier == utilisateurConnecte.ID).ToList();
            if (!mes.Any())
                Console.WriteLine("Aucun plat partagé.");
            else
                mes.ForEach(p => Console.WriteLine($"ID {p.ID}: {p.Nom} — {p.PrixParPersonne:C}"));
        }

        static void VoirMesCommandesClient()
        {
            Console.WriteLine("\n--- Mes commandes ---");
            if (!listeCommandesClient.Any())
                Console.WriteLine("Aucune commande.");
            else
                listeCommandesClient.ForEach(c =>
                    Console.WriteLine($"#{c.ID} {c.Plat}×{c.Quantite} — État:{c.Etat} — Montant:{c.MontantTotal:C}"));
        }

        static void VoirCommandesCuisinier()
        {
            Console.WriteLine("\n--- Commandes reçues ---");
            if (!commandeParCuisinier.TryGetValue(utilisateurConnecte.ID, out var list) || !list.Any())
            {
                Console.WriteLine("Aucune commande reçue.");
                return;
            }
            list.ForEach(c => Console.WriteLine($"#{c.ID} {c.Plat}×{c.Quantite} — État:{c.Etat} — Montant:{c.MontantTotal:C}"));
            Console.Write("\nVisualiser trajet d'une commande ? (O/N) ");
            if (Console.ReadLine().Trim().ToUpper() == "O")
            {
                Console.Write("ID commande : ");
                if (int.TryParse(Console.ReadLine(), out int id) && list.Any(c => c.ID == id))
                {
                    var cmd = list.First(c => c.ID == id);
                    Application.Run(new GraphForm(cmd.Chemin, stationCoordinates));
                }
            }
        }

        static void MettreAJourEtatCommande()
        {
            Console.WriteLine("\n--- Mettre à jour état ---");
            if (!commandeParCuisinier.TryGetValue(utilisateurConnecte.ID, out var list) || !list.Any())
            {
                Console.WriteLine("Aucune commande reçue.");
                return;
            }
            list.ForEach(c => Console.WriteLine($"#{c.ID} — État actuel : {c.Etat}"));
            Console.Write("ID à modifier : ");
            if (int.TryParse(Console.ReadLine(), out int id) && list.Any(c => c.ID == id))
            {
                var cmd = list.First(c => c.ID == id);
                Console.Write("Nouvel état : ");
                cmd.Etat = Console.ReadLine().Trim();
                Console.WriteLine("État mis à jour.");
            }
        }

        static void CalculerDijkstra()
        {
            Console.Write("Station départ : ");
            var dep = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            Console.Write("Station arrivée : ");
            var arr = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            if (dep == null || arr == null)
            {
                Console.WriteLine("Station inconnue.");
                return;
            }
            var (dist, pred) = metroGraphe.Dijkstra(dep);
            double t = dist[arr];
            if (double.IsInfinity(t))
            {
                Console.WriteLine("Pas de chemin.");
                return;
            }
            var ch = ReconstruireChemin(dep, arr, pred);
            Console.WriteLine($"\nDijkstra — Temps : {t} — Chemin : {string.Join(" → ", ch)}");
        }

        static void CalculerBellmanFord()
        {
            Console.Write("Station départ : ");
            var dep = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            Console.Write("Station arrivée : ");
            var arr = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            if (dep == null || arr == null)
            {
                Console.WriteLine("Station inconnue.");
                return;
            }
            var (dist, pred) = metroGraphe.BellmanFord(dep);
            double t = dist[arr];
            if (double.IsInfinity(t))
            {
                Console.WriteLine("Pas de chemin.");
                return;
            }
            var ch = ReconstruireChemin(dep, arr, pred);
            Console.WriteLine($"\nBellman-Ford — Temps : {t} — Chemin : {string.Join(" → ", ch)}");
        }

        static void CalculerFloydWarshall()
        {
            var (dist, next) = metroGraphe.FloydWarshall();
            Console.Write("Station départ : ");
            var dep = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            Console.Write("Station arrivée : ");
            var arr = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            if (dep == null || arr == null)
            {
                Console.WriteLine("Station inconnue.");
                return;
            }
            var ch = metroGraphe.ReconstituerCheminFloyd(dep, arr, next);
            double t = dist[(dep, arr)];
            if (!ch.Any() || double.IsInfinity(t))
                Console.WriteLine("Aucun chemin trouvé.");
            else
                Console.WriteLine($"\nFloyd-Warshall — Temps : {t} — Chemin : {string.Join(" → ", ch.Select(n => n.Valeur))}");
        }

        static void VisualiserCheminGraphiqueAdmin()
        {
            Console.Write("Station départ : ");
            var dep = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            Console.Write("Station arrivée : ");
            var arr = metroGraphe.TrouverNoeud(Console.ReadLine().Trim());
            if (dep == null || arr == null)
            {
                Console.WriteLine("Station inconnue.");
                return;
            }
            var (dist, pred) = metroGraphe.Dijkstra(dep);
            double t = dist[arr];
            if (double.IsInfinity(t))
            {
                Console.WriteLine("Pas de chemin.");
                return;
            }
            var ch = ReconstruireChemin(dep, arr, pred);
            Console.WriteLine($"\nVisuel — Temps : {t} — Chemin : {string.Join(" → ", ch)}");
            Application.Run(new GraphForm(ch, stationCoordinates));
        }

        static void AfficherClassementCuisiniers()
        {
            Console.WriteLine("\n--- Classement des cuisiniers (Livrées) ---");
            var classement = commandeParCuisinier
                .ToDictionary(kv => kv.Key,
                              kv => kv.Value.Count(c => c.Etat.Equals("Livrée", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(kv => kv.Value);
            foreach (var kv in classement)
                Console.WriteLine($"Cuisinier {kv.Key} → {kv.Value} livraison(s)");
        }

        static void PerformGraphColoring()
        {
            Console.WriteLine("\n--- Coloration Welsh-Powell ---");
            var coloring = metroGraphe.WelshPowellColoring();
            int maxColor = coloring.Values.Max();
            Console.WriteLine($"Couleurs utilisées : {maxColor}");
            foreach (var kvp in coloring)
                Console.WriteLine($"Station {kvp.Key.Valeur} → Couleur {kvp.Value}");
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "coloring.json");
            metroGraphe.ExportColoringJson(jsonPath);
            Console.WriteLine($"JSON exporté : {jsonPath}");
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "coloring.xml");
            metroGraphe.ExportColoringXml(xmlPath);
            Console.WriteLine($"XML exporté  : {xmlPath}");
        }

        static void ColorerGrapheClientsCuisiniers()
        {
            //// 1) On récupère les relations
            var relations = dbManager.ChargerRelationsClientCuisinier();

            //// 2) On crée un nouveau graphe générique orienté = false
            var g = new Graphe<string>(oriente: false);

            //// 3) On ajoute tous les clients et cuisiniers en tant que nœuds,
            ////    en les préfixant pour distinguer les deux ensembles
            var clients = relations.Select(r => r.clientId).Distinct();
            var cuisiniers = relations.Select(r => r.cuisinierId).Distinct();
            foreach (var cid in clients)    g.AjouterNoeud("C:" + cid);
            foreach (var pid in cuisiniers) g.AjouterNoeud("U:" + pid);

            //// 4) On ajoute une arête pour chaque relation
            foreach (var (cid, pid) in relations)
                g.AjouterArc("C:" + cid, "U:" + pid, 1);  //// poids 1

            //// 5) On colore avec Welsh-Powell
            var coloring = g.WelshPowellColoring();

            //// 6) On affiche le résultat à la console
            Console.WriteLine("Coloration du graphe client↔cuisinier :");
            foreach (var kv in coloring)
                Console.WriteLine($"{kv.Key} → couleur {kv.Value}");

            //// 7) On exporte en JSON / XML
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "coloring_users.json");
            g.ExportColoringJson(jsonPath);
            Console.WriteLine($"JSON exporté : {jsonPath}");
            var xmlPath  = Path.Combine(AppContext.BaseDirectory, "coloring_users.xml");
            g.ExportColoringXml(xmlPath);
            Console.WriteLine($"XML exporté  : {xmlPath}");
        }


        static List<string> ReconstruireChemin(Noeud<string> start, Noeud<string> end, Dictionary<Noeud<string>, Noeud<string>> pred)
        {
            var path = new List<string>();
            var cur = end;
            while (cur != null && !cur.Equals(start))
            {
                path.Insert(0, cur.Valeur);
                cur = pred[cur];
            }
            if (cur != null)
                path.Insert(0, start.Valeur);
            return path;
        }

        static double CalculerDistanceChemin(List<string> chemin, Dictionary<string, (double Latitude, double Longitude)> coords)
        {
            double total = 0;
            for (int i = 0; i < chemin.Count - 1; i++)
            {
                var a = coords[chemin[i]];
                var b = coords[chemin[i + 1]];
                total += CalculerDistanceHaversine(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            }
            return total;
        }

        static double CalculerDistanceHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = ToRadians(lat2 - lat1), dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        static double ToRadians(double deg) => deg * Math.PI / 180;
    }

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
        public string Etat { get; set; }
        public decimal MontantTotal { get; set; }
        public int IDCuisinier { get; set; }
    }

    public class Utilisateur
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string SousType { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string MetroProche { get; set; }
    }
}

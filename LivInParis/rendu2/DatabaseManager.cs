using System;
using MySql.Data.MySqlClient;

namespace GraphProject
{
    public class DatabaseManager
    {
        private readonly string connectionString;

        /// <summary>
        /// Initialise un nouveau DatabaseManager avec les paramètres de connexion.
        /// </summary>

        public DatabaseManager(string server, string database, string user, string password, int port = 3306)
        {
            connectionString = $"SERVER={server};PORT={port};DATABASE={database};UID={user};PASSWORD={password};";
        }
        
        /// <summary>
        /// Charge toutes les commandes et leurs lignes associées depuis la base de données.
        /// </summary>
        public List<Commande> ObtenirToutesCommandes()
        {
            var liste = new List<Commande>();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = @"
            SELECT
                cmd.ID_commande,
                ldc.ID_plat,
                p.Nom_plat,
                ldc.Quantité_plat,
                ldc.Date_livraison,
                ldc.Adresse_livraison,
                cmd.Statut_Commande,
                cmd.Prix_tot,
                liv.ID_cuisinier
            FROM Commande cmd
            JOIN Ligne_de_commande ldc ON ldc.ID_commande = cmd.ID_commande
            JOIN Plat p               ON p.ID_plat        = ldc.ID_plat
            JOIN Livraison liv        ON liv.ID_commande  = cmd.ID_commande;
            ";
            using var cmd = new MySqlCommand(sql, conn);
            using var rd  = cmd.ExecuteReader();
            while (rd.Read())
            {
                liste.Add(new Commande
                {
                    ID               = rd.GetInt32("ID_commande"),
                    Plat             = rd.GetString("Nom_plat"),
                    Quantite         = rd.GetInt32("Quantité_plat"),
                    DateLivraison    = rd.GetDateTime("Date_livraison"),
                    AdresseLivraison = rd.GetString("Adresse_livraison"),
                    Etat             = rd.GetString("Statut_Commande"),
                    MontantTotal     = rd.GetDecimal("Prix_tot"),
                    IDCuisinier      = rd.GetInt32("ID_cuisinier")  // <--- on renseigne ici
                });
            }
            return liste;
        }

         /// <summary>
        /// Teste la connexion MySQL en ouvrant et fermant une connexion.
        /// </summary>
        public bool TestConnexion()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Connexion MySQL réussie.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de connexion : " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Affiche dans la console la liste de tous les clients.
        /// </summary>
        public void ListerClients()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Client;";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("ID_client");
                            string nom = reader.GetString("Nom_client");
                            string prenom = reader.GetString("Prenom_client");
                            Console.WriteLine($"{id} : {nom} {prenom}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la lecture des clients : " + ex.Message);
            }
        }

        
        /// <summary>
        /// Insertion d'un nouveau client et retourne son ID généré.
        /// </summary>²
        public int InsererClient(string nom, string prenom, string rue, string numRue, int codePostal, string ville, string tel, string email, string metro, string motDePasse)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Client (Nom_client, Prenom_client, Rue_client, Numero_rue_client, Code_postal_client, Ville_client, Num_client, Adresse_mail_client, Metro_proche, Mot_de_passe) " +
                                   "VALUES (@nom, @prenom, @rue, @numRue, @codePostal, @ville, @tel, @email, @metro, @motDePasse); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@prenom", prenom);
                    cmd.Parameters.AddWithValue("@rue", rue);
                    cmd.Parameters.AddWithValue("@numRue", numRue);
                    cmd.Parameters.AddWithValue("@codePostal", codePostal);
                    cmd.Parameters.AddWithValue("@ville", ville);
                    cmd.Parameters.AddWithValue("@tel", tel);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@metro", metro);
                    cmd.Parameters.AddWithValue("@motDePasse", motDePasse);
                    object result = cmd.ExecuteScalar();
                    int newId = Convert.ToInt32(result);
                    Console.WriteLine($"Client inséré avec l'ID {newId}.");
                    return newId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du client : " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Insertion d'un cuisinier de type particulier et retourne son ID généré.
        /// </summary>
        public int InsererCuisinierParticulier(string nom, string prenom, string rue, string numRue, int codePostal, string ville, string tel, string email, string metro, string motDePasse, string infosComplementaires)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Cuisinier (Nom_cuisinier, Prenom_cuisinier, Rue_cuisinier, Numero_rue_cuisinier, Code_postal_cuisinier, Ville_cuisinier, Num_cuisinier, Adresse_mail_cuisinier, Metro_proche_cuisinier, Mot_de_passe, Type_cuisinier, Infos_complementaires) " +
                                   "VALUES (@nom, @prenom, @rue, @numRue, @codePostal, @ville, @tel, @email, @metro, @motDePasse, 'Particulier', @infos); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@prenom", prenom);
                    cmd.Parameters.AddWithValue("@rue", rue);
                    cmd.Parameters.AddWithValue("@numRue", numRue);
                    cmd.Parameters.AddWithValue("@codePostal", codePostal);
                    cmd.Parameters.AddWithValue("@ville", ville);
                    cmd.Parameters.AddWithValue("@tel", tel);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@metro", metro);
                    cmd.Parameters.AddWithValue("@motDePasse", motDePasse);
                    cmd.Parameters.AddWithValue("@infos", infosComplementaires);
                    object result = cmd.ExecuteScalar();
                    int newId = Convert.ToInt32(result);
                    Console.WriteLine($"Cuisinier Particulier inséré avec l'ID {newId}.");
                    return newId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du cuisinier particulier : " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Insertion d'un cuisinier de type entreprise et retourne son ID généré.
        /// </summary>
        public int InsererCuisinierEntreprise(string nom, string prenom, string rue, string numRue, int codePostal, string ville, string tel, string email, string metro, string motDePasse, string nomEntreprise, string referentCommunication)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Cuisinier (Nom_cuisinier, Prenom_cuisinier, Rue_cuisinier, Numero_rue_cuisinier, Code_postal_cuisinier, Ville_cuisinier, Num_cuisinier, Adresse_mail_cuisinier, Metro_proche_cuisinier, Mot_de_passe, Type_cuisinier, Nom_entreprise, Referent_communication) " +
                                   "VALUES (@nom, @prenom, @rue, @numRue, @codePostal, @ville, @tel, @email, @metro, @motDePasse, 'Entreprise', @nomEntreprise, @referent); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@prenom", prenom);
                    cmd.Parameters.AddWithValue("@rue", rue);
                    cmd.Parameters.AddWithValue("@numRue", numRue);
                    cmd.Parameters.AddWithValue("@codePostal", codePostal);
                    cmd.Parameters.AddWithValue("@ville", ville);
                    cmd.Parameters.AddWithValue("@tel", tel);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@metro", metro);
                    cmd.Parameters.AddWithValue("@motDePasse", motDePasse);
                    cmd.Parameters.AddWithValue("@nomEntreprise", nomEntreprise);
                    cmd.Parameters.AddWithValue("@referent", referentCommunication);
                    object result = cmd.ExecuteScalar();
                    int newId = Convert.ToInt32(result);
                    Console.WriteLine($"Cuisinier Entreprise inséré avec l'ID {newId}.");
                    return newId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du cuisinier entreprise : " + ex.Message);
                return -1;
            }
        }

         /// <summary>
        /// Insertion d'un plat partagé par un cuisinier.
        /// </summary>
        public void InsererPlat(string nomPlat, string typePlat, int nbPortions, DateTime dateFabrication, DateTime datePeremption, decimal prixParPersonne, string nationaliteCuisine, string regimeAlimentaire, string ingredients, int idCuisinier)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Plat (Nom_plat, Type_plat, Nb_portions, Date_fabrication, Date_peremption, Prix_plat, Nationalité_cuisine, Regime_alimentaire, Ingredients_principaux, ID_cuisinier) " +
                                   "VALUES (@nomPlat, @typePlat, @nbPortions, @dateFab, @datePeremption, @prix, @nationalite, @regime, @ingredients, @idCuisinier);";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nomPlat", nomPlat);
                    cmd.Parameters.AddWithValue("@typePlat", typePlat);
                    cmd.Parameters.AddWithValue("@nbPortions", nbPortions);
                    cmd.Parameters.AddWithValue("@dateFab", dateFabrication);
                    cmd.Parameters.AddWithValue("@datePeremption", datePeremption);
                    cmd.Parameters.AddWithValue("@prix", prixParPersonne);
                    cmd.Parameters.AddWithValue("@nationalite", nationaliteCuisine);
                    cmd.Parameters.AddWithValue("@regime", regimeAlimentaire);
                    cmd.Parameters.AddWithValue("@ingredients", ingredients);
                    cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rows} plat(s) inséré(s).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du plat : " + ex.Message);
            }
        }

         /// <summary>
        /// Charge les relations client–cuisinier issues des commandes existantes.
        /// </summary>
        public List<(int clientId, int cuisinierId)> ChargerRelationsClientCuisinier()
        {
            var liste = new List<(int, int)>();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = @"
            SELECT DISTINCT c.ID_client, p.ID_cuisinier
            FROM Commande c
            JOIN Livraison l ON l.ID_commande = c.ID_commande
            JOIN Cuisinier p ON p.ID_cuisinier = l.ID_cuisinier
            ";
            using var cmd = new MySqlCommand(sql, conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                liste.Add(( rd.GetInt32(0), rd.GetInt32(1) ));
            return liste;
        }


        /// <summary>
        /// Obtient un client à partir de son email et mot de passe.
        /// </summary>
        public Utilisateur ObtenirClientByEmail(string email, string mdp)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "SELECT * FROM Client WHERE Adresse_mail_client=@mail AND Mot_de_passe=@mdp";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mail", email);
            cmd.Parameters.AddWithValue("@mdp", mdp);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Utilisateur
                {
                    ID = reader.GetInt32("ID_client"),
                    Type = "Client",
                    Nom = reader.GetString("Nom_client"),
                    Prenom = reader.GetString("Prenom_client"),
                    Email = reader.GetString("Adresse_mail_client"),
                    MotDePasse = mdp,
                    MetroProche = reader.GetString("Metro_proche")
                };
            }
            return null;
        }

        /// <summary>
        /// Obtient un cuisinier à partir de son email et mot de passe.
        /// </summary>
        public Utilisateur ObtenirCuisinierByEmail(string email, string mdp)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "SELECT * FROM Cuisinier WHERE Adresse_mail_cuisinier=@mail AND Mot_de_passe=@mdp";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mail", email);
            cmd.Parameters.AddWithValue("@mdp", mdp);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Utilisateur
                {
                    ID = reader.GetInt32("ID_cuisinier"),
                    Type = "Cuisinier",
                    SousType = reader.GetString("Type_cuisinier"),
                    Nom = reader.GetString("Nom_cuisinier"),
                    Prenom = reader.GetString("Prenom_cuisinier"),
                    Email = reader.GetString("Adresse_mail_cuisinier"),
                    MotDePasse = mdp,
                    MetroProche = reader.GetString("Metro_proche_cuisinier")
                };
            }
            return null;
        }
        /// <summary>
        /// Met à jour les informations de base d'un client existant.
        /// </summary>
        public void MettreAJourClient(int id, string nom, string prenom, string tel)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "UPDATE Client SET Nom_client=@nom, Prenom_client=@prenom, Num_client=@tel WHERE ID_client=@id";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", nom);
            cmd.Parameters.AddWithValue("@prenom", prenom);
            cmd.Parameters.AddWithValue("@tel", tel);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void MettreAJourCuisinier(int id, string nom, string prenom, string tel)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "UPDATE Cuisinier SET Nom_cuisinier=@nom, Prenom_cuisinier=@prenom, Num_cuisinier=@tel WHERE ID_cuisinier=@id";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", nom);
            cmd.Parameters.AddWithValue("@prenom", prenom);
            cmd.Parameters.AddWithValue("@tel", tel);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void SupprimerClient(int id)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "DELETE FROM Client WHERE ID_client=@id";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void SupprimerCuisinier(int id)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "DELETE FROM Cuisinier WHERE ID_cuisinier=@id";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}

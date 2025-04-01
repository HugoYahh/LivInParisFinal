using System;
using MySql.Data.MySqlClient;

namespace GraphProject
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string server, string database, string user, string password, int port = 3306)
        {
            connectionString = $"SERVER={server};PORT={port};DATABASE={database};UID={user};PASSWORD={password};";
        }

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

        // Insertion Client : retourne l'ID généré
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

        // Insertion Cuisinier Particulier : retourne l'ID généré
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

        // Insertion Cuisinier Entreprise : retourne l'ID généré
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

        // Insertion d'un plat partagé par un cuisinier
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

        // Obtention d'un Client via Email et Mot de Passe
        public Utilisateur ObtenirClientByEmail(string email, string motDePasse)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Client WHERE Adresse_mail_client = @email AND Mot_de_passe = @motDePasse";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@motDePasse", motDePasse);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Utilisateur u = new Utilisateur();
                            u.ID = reader.GetInt32("ID_client");
                            u.Type = "Client";
                            u.Nom = reader.GetString("Nom_client");
                            u.Prenom = reader.GetString("Prenom_client");
                            u.Email = reader.GetString("Adresse_mail_client");
                            u.MotDePasse = reader.GetString("Mot_de_passe");
                            u.MetroProche = reader.GetString("Metro_proche");
                            return u;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la récupération du client : " + ex.Message);
            }
            return null;
        }

        // Obtention d'un Cuisinier via Email et Mot de Passe
        public Utilisateur ObtenirCuisinierByEmail(string email, string motDePasse)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Cuisinier WHERE Adresse_mail_cuisinier = @email AND Mot_de_passe = @motDePasse";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@motDePasse", motDePasse);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Utilisateur u = new Utilisateur();
                            u.ID = reader.GetInt32("ID_cuisinier");
                            u.Type = "Cuisinier";
                            u.Nom = reader.GetString("Nom_cuisinier");
                            u.Prenom = reader.GetString("Prenom_cuisinier");
                            u.Email = reader.GetString("Adresse_mail_cuisinier");
                            u.MotDePasse = reader.GetString("Mot_de_passe");
                            u.MetroProche = reader.GetString("Metro_proche_cuisinier");
                            u.SousType = reader.GetString("Type_cuisinier");
                            return u;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la récupération du cuisinier : " + ex.Message);
            }
            return null;
        }
    }
}

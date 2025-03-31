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

        public void InsererClient(string nom, string prenom, string rue, string numRue, int codePostal, string ville, string tel, string email, string metro)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Client (Nom_client, Prenom_client, Rue_client, Numero_rue_client, Code_postal_client, Ville_client, Num_client, Adresse_mail_client, Metro_proche) " +
                                   "VALUES (@nom, @prenom, @rue, @numRue, @codePostal, @ville, @tel, @email, @metro);";
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
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rows} client(s) inséré(s).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du client : " + ex.Message);
            }
        }

        public void InsererCuisinier(string nom, string prenom, string rue, string numRue, int codePostal, string ville, string tel, string email, string metro)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Cuisinier (Nom_cuisinier, Prenom_cuisinier, Rue_cuisinier, Numero_rue_cuisinier, Code_postal_cuisinier, Ville_cuisinier, Num_cuisinier, Adresse_mail_cuisinier, Metro_proche_cuisinier) " +
                                   "VALUES (@nom, @prenom, @rue, @numRue, @codePostal, @ville, @tel, @email, @metro);";
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
                    int rows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rows} cuisinier(s) inséré(s).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion du cuisinier : " + ex.Message);
            }
        }
    }
}

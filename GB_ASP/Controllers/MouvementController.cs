using GB_ASP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace GB_ASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MouvementController : ControllerBase
    {
        private readonly IConfiguration _connection;

        public MouvementController(IConfiguration connection)
        {
            _connection = connection;
        }
        private SqlConnection GetSqlConnection()
        {
            SqlConnection con = new SqlConnection(_connection.GetConnectionString("GBancaire").ToString());

            return con;
        }
        private Compte GetCompte(SqlConnection connection, int compteId)
        {
            Compte compte = null;

            string query = "SELECT Id, solde, nom FROM Compte WHERE Id = @compteId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@compteId", compteId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        compte = new Compte
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            solde = (float)reader.GetDouble(reader.GetOrdinal("solde")),
                            nom = reader.GetString(reader.GetOrdinal("nom"))
                        };
                    }
                }
            }

            return compte;
        }

        private void UpdateCompte(SqlConnection connection, Compte compte)
        {
            string query = "UPDATE Compte SET Solde = @solde WHERE Id = @compteId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@solde", compte.solde);
                command.Parameters.AddWithValue("@compteId", compte.Id);

                command.ExecuteNonQuery();
            }
        }

        [HttpGet("compte/{compteId}")]
        public string GetMouvementsByCompteId(int compteId)
        {
            List<Mouvement> mouvements = new List<Mouvement>();
            Console.WriteLine(compteId);
            using (SqlConnection connection = GetSqlConnection())
            {
                // Ouvrir la connexion à la base de données
                connection.Open();
                
                    // Ouvrir la connexion à la base de données


                    // Créer une commande SQL pour récupérer les mouvements pour le compte spécifié
                    SqlCommand command = new SqlCommand("SELECT * FROM mouvement WHERE compte_id = @CompteId", connection);
                    command.Parameters.AddWithValue("@CompteId", compteId);

                    // Exécuter la commande et lire les résultats
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Créer un objet Mouvement à partir des données lues
                        Mouvement mouvement = new Mouvement
                        {

                            id = (int)reader["id"],
                            compte_id = (int)reader["compte_id"],
                            montant = (float)(double)reader["montant"],
                            date_mvt = (DateTime)reader["date_mvt"]
                        };
                        Console.WriteLine(mouvement.date_mvt);
                        Console.WriteLine("ssssssssssssss");



                        // Ajouter le mouvement à la liste
                        mouvements.Add(mouvement);
                    }

                    // Fermer le lecteur
                    reader.Close();
                
                    // Fermer la connexion à la base de données
                    connection.Close();
                

                return  JsonConvert.SerializeObject(mouvements) ;
            }
        }

        // Autres actions et méthodes du contrôleur...
        [HttpPost("crediter")]
        public IActionResult Crediter(int compteId, float montant)
        {
            Console.WriteLine(montant);
            Console.WriteLine(compteId);


            using (SqlConnection connection = GetSqlConnection())
          {
                // Ouvrir la connexion à la base de données
                connection.Open();

                // Récupérer le compte à créditer
                Compte compte = GetCompte(connection, compteId);

                if (compte == null)
                {
                    return NotFound(); // Compte non trouvé
                }

                // Ajouter le montant au solde du compte
                compte.solde += montant;
                Mouvement mouvement = new Mouvement
                {
                    compte_id = compte.Id,
                    montant = montant,
                    date_mvt = DateTime.Now
                };
                AjouterMouvement(connection, mouvement);
                // Enregistrer les modifications dans la base de données
                UpdateCompte(connection, compte);

                // Fermer la connexion à la base de données
                connection.Close();
            }

            return Ok(); // Opération réussie
        }

        [HttpPost("debiter")]
        public IActionResult Debiter(int compteId, float montant)
        {
            using (SqlConnection connection = GetSqlConnection())
            {
                // Ouvrir la connexion à la base de données
                connection.Open();

                // Récupérer le compte à débiter
                Compte compte = GetCompte(connection, compteId);

                if (compte == null)
                {
                    return NotFound(); // Compte non trouvé
                }

                if (compte.solde < montant)
                {
                    return BadRequest("Solde insuffisant"); // Solde insuffisant pour effectuer le débit
                }

                // Soustraire le montant du solde du compte
                compte.solde -= montant;

                Mouvement mouvement = new Mouvement
                {
                    compte_id = compte.Id,
                    montant = - montant,
                    date_mvt = DateTime.Now
                };
                AjouterMouvement(connection, mouvement);
                // Enregistrer les modifications dans la base de données
                UpdateCompte(connection, compte);

                // Fermer la connexion à la base de données
                connection.Close();
            }

            return Ok(); // Opération réussie
        }
        private void AjouterMouvement(SqlConnection connection, Mouvement mouvement)
        {
            string query = "INSERT INTO Mouvement (compte_id, montant, date_mvt) VALUES (@idCompte, @montant, @dateMvt)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idCompte", mouvement.compte_id);
                command.Parameters.AddWithValue("@montant", mouvement.montant);
                command.Parameters.AddWithValue("@dateMvt", mouvement.date_mvt);

                command.ExecuteNonQuery();
            }
        }




    }


}


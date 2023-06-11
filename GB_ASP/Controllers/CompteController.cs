using GB_ASP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace GB_ASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompteController : ControllerBase
    {
        private readonly IConfiguration  _connection;

        public CompteController(IConfiguration connection)
        {
            _connection = connection;
        }
         [HttpGet("comptes")]
        public string Getcompte() {
            Console.WriteLine("ssssssssssssss");

            SqlConnection con = new SqlConnection(_connection.GetConnectionString("GBancaire").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Compte",con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Compte> cmt = new List<Compte>();
            response re = new response();
            if(dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                { Compte compte = new Compte();
                    compte.Id = Convert.ToInt32(dt.Rows[i]["id"]);
                    compte.nom = Convert.ToString(dt.Rows[i]["nom"]);
                    compte.solde = Convert.ToSingle(dt.Rows[i]["solde"]);

                    cmt.Add(compte);
                }
            }
            if (dt.Rows.Count > 0)
                return JsonConvert.SerializeObject(cmt);
            else
            {
                re.ErrorMessage ="no data found";
                re.Statuscode = 100;
                return JsonConvert.SerializeObject(re);

            }
        }
       

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;


namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ComputerController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // ----------Get all----------
        [HttpGet]
        public async Task<IActionResult> Get(
          )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Model
                        FROM Computer
                        WHERE 1 = 1 
                        ";
                    //if (firstName != null)
                    //{
                    //    cmd.CommandText += " AND FirstName LIKE @firstName";
                    //    cmd.Parameters.Add(new SqlParameter("@firstName", "%" + firstName + "%"));
                    //};

                    //if (lastName != null)
                    //{
                    //    cmd.CommandText += " AND LastName LIKE @lastName";
                    //    cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                    //};


                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> allComputers = new List<Computer>();

                    while (reader.Read())
                    {
                        var computer = new Computer()
                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),

                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }

                        allComputers.Add(computer);
                    }
                    reader.Close();

                    return Ok(allComputers);
                }
            }
        }

        //----------GET by Id----------
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Model
                        FROM Computer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer()
                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),

                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                       
                        reader.Close();

                        return Ok(computer);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }

        //----------POST----------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Computer (PurchaseDate, Make, Model)
                        OUTPUT INSERTED.Id
                        VALUES (@PurchaseDate,  @Make, @Model)";

                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")));
                    cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@Model", computer.Model));
                  
                    int id = (int)cmd.ExecuteScalar();

                    computer.Id = id;
                    return CreatedAtRoute("GetComputer", new { id = id }, computer);
                }
            }
        }


    }
}
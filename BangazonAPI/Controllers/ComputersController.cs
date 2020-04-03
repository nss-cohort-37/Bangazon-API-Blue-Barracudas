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
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ComputersController(IConfiguration config)
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

        // this turinary checks if the 'available' query param is being used and if it is 'true' or 'false'
        public async Task<IActionResult> Get(
            [FromQuery] string available
          )
        {
            if (available != "true" && available != "false")
            {
                // if there are neither true or false being passed as a param, envoke the private method to get all computers
                var computers = GetAllComputers(available);
                return Ok(computers);
            } else if (available == "true")
            {
                //if true - envoke the private method that gets only the unnassigned computers
                var computers = GetAvailableComputers(available);
                return Ok(computers);
            } else
            {
                // if false - envoke the private method that gets only the assigned computers
                var computers = GetUnavailableComputers(available);
                return Ok(computers);
            }
        }

        private List<Computer> GetAvailableComputers ([FromQuery] string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model, e.ComputerId
                        FROM Computer c
                        LEFT JOIN Employee e
                        ON e.ComputerId = c.Id";
                    // left join on Employee where employee.computerId = the computer Id
                    // so we have a reference to all computers that have an associated computer

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> allComputers = new List<Computer>();

                    while (reader.Read())
                    {
                        // checking if the column 'computerId' has no assosiated computer and then building a list of computers that have no employee
                        if (reader.IsDBNull(reader.GetOrdinal("ComputerId")))
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
                        else
                        {
                            computer.DecomissionDate = null;
                        }

                            allComputers.Add(computer);
                        }
                    }
                    reader.Close();

                    return allComputers;
                }
            }
        }

        private List<Computer> GetUnavailableComputers([FromQuery] string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model
                        FROM Computer c
                        INNER JOIN Employee e
                        ON e.ComputerId = c.Id";



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
                        else
                        {
                            computer.DecomissionDate = null;
                        }

                        allComputers.Add(computer);
                    }
                    reader.Close();

                    return allComputers;
                }
            }
        }
        private List<Computer> GetAllComputers([FromQuery] string available)
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
                        else
                        {
                            computer.DecomissionDate = null;
                        }


                        allComputers.Add(computer);
                    }
                    reader.Close();

                    return allComputers;
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
                        else
                        {
                            computer.DecomissionDate = null;
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

        ////////----------PUT----------
       [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                            SET PurchaseDate = @purchaseDate, DecomissionDate = @decomissionDate, Make = @make, Model = @model
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));

                        if (computer.DecomissionDate == null)
                        {

                            cmd.Parameters.Add(new SqlParameter("@decomissionDate", DBNull.Value));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@decomissionDate", computer.DecomissionDate));
                        }

                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ////////----------DELETE----------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {

            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        //if (!ComputerInUse(id) ){

                        cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return new StatusCodeResult(StatusCodes.Status204NoContent);
                            }
                            throw new Exception("No rows affected");

                            
                        //} 
                        //else
                        //{
                        //    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                        //}

                            
                               

                        


                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        


        private bool ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id
                        FROM Computer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        private bool ComputerInUse(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model, e.ComputerId
                        FROM Computer c
                        LEFT JOIN Employee e
                        ON e.ComputerId = c.Id
                        WHERE c.Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                    {

                        return true;
                    } else
                    {
                        return false;
                    }
                     //reader.Read();
                }
            }
        }
    }
}
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
    public class UserPaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserPaymentTypesController(IConfiguration config)
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

        
        //////----------GET----------
        
        [HttpGet]
        public async Task<IActionResult> Get(
               [FromQuery] string customerId
               )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // We dont want to return a full list of all the users payment types so we only want to get one at a type 
                    //based off of their customerId so we have to add the customerId param

                    if (customerId != null)
                    {
                        //because we only want active payment types we only get back where active = 1 (true)
                    cmd.CommandText = @"
                        SELECT u.Id, u.AcctNumber, u.Active, u.CustomerId, u.PaymentTypeid 
                        FROM UserPaymentType u
                        WHERE u.Active = 1
                        AND u.CustomerId = @id";

                        cmd.Parameters.Add(new SqlParameter("@Id", customerId));
                    }
                    else
                    {
                        //if you try to get all the users payment types you will get a forbidden error 

                        return new StatusCodeResult(StatusCodes.Status403Forbidden);
                    };

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<UserPaymentType> allUserPaymentTypes = new List<UserPaymentType>();

                    while (reader.Read())
                    {
                        var userPaymentType = new UserPaymentType()
                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
  
                        };

                        allUserPaymentTypes.Add(userPaymentType);
                    }
                    reader.Close();

                    return Ok(allUserPaymentTypes);
                }
            }
        }

        //----------POST----------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPaymentType userPaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO UserPaymentType (AcctNumber, Active, CustomerId, PaymentTypeId)
                        OUTPUT INSERTED.Id
                        VALUES (@AcctNumber, @Active, @CustomerId, @PaymentTypeId)";

                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", userPaymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@Active", userPaymentType.Active));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", userPaymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", userPaymentType.PaymentTypeId));
                    

                    int id = (int)cmd.ExecuteScalar();

                    userPaymentType.Id = id;
                    return CreatedAtRoute("GetUserPaymentType", new { id = id }, userPaymentType);
                }
            }
        }


        ////////----------PUT----------
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserPaymentType userPaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                     SET AcctNumber = @AcctNumber, Active = @Active, CustomerId = @CustomerId, PaymentTypeId = @PaymentTypeId
                                     WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", userPaymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@Active", userPaymentType.Active));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", userPaymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", userPaymentType.PaymentTypeId));
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
                if (!UserPaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        private bool UserPaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id
                        FROM UserPaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }

}
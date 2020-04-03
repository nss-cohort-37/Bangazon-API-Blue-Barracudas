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
                    if (customerId != null)
                    {
                    cmd.CommandText = @"
                        SELECT u.Id, u.AcctNumber, u.Active, u.CustomerId, u.PaymentTypeid 
                        FROM UserPaymentType u
                        WHERE u.Active = 1
                        AND u.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Id", customerId));
                    }
                    else
                    {
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
    }
}
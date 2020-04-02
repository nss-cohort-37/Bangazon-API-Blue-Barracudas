using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

using BangazonAPI.Models;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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

        // All payment types from the DB
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Active FROM PaymentType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();

                    while (reader.Read())
                    {
                        PaymentType paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };

                        paymentTypes.Add(paymentType);
                    }
                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }

        // Get single payment type by ID
        [HttpGet("{id}", Name = "GetPaymentType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT
                                Id, Name, Active
                            FROM PaymentType
                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType paymentType = null;

                    if (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };
                        reader.Close();

                        return Ok(paymentType);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }
    }
}
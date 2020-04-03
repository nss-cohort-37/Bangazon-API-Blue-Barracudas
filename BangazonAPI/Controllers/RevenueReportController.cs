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
    public class RevenueReportController : ControllerBase
    {
        private readonly IConfiguration _config;
        public RevenueReportController(IConfiguration config)
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
                    // this query returns a table with the total revnue of each product that has orders
                    cmd.CommandText = @"
                       SELECT pt.[Name], pt.Id, SUM(p.Price) AS Revenue
                       FROM OrderProduct op 
                       LEFT JOIN Product p ON p.Id = op.ProductId
                       LEFT JOIN ProductType pt on p.ProductTypeId = pt.Id group by pt.[Name], pt.Id 
                        ";
                   

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<RevenueReport> allReports = new List<RevenueReport>();

                    while (reader.Read())
                    {
                        var report = new RevenueReport()
                        {

                            ProductType = reader.GetString(reader.GetOrdinal("Name")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("Id")),
                            TotalRevenue = reader.GetDecimal(reader.GetOrdinal("Revenue"))

                        };

                        allReports.Add(report);
                    }
                    reader.Close();

                    return Ok(allReports);
                }
            }
        }


    }
}
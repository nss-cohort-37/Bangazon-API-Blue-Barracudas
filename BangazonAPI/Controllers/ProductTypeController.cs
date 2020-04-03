using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

using BangazonAPI.Models;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

        // Get all product types
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM ProductType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> productTypes = new List<ProductType>();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        productTypes.Add(productType);
                    }
                    reader.Close();

                    return Ok(productTypes);
                }
            }
        }


        // Determines whether to get product types or product types with products included!
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string includes)
        {
            if (includes == "products")
            {
                var productTypeWithProducts = GetProductTypeWithProducts(id);
                return Ok(productTypeWithProducts);
            }
            else
            {
                var productType = GetProductType(id);
                return Ok(productType);
            }
        }

        private ProductType GetProductType(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType productType = null;

                    if (reader.Read())
                    {
                        productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };
                    }

                    reader.Close();

                    return productType;
                }
            }
        }

        private ProductType GetProductTypeWithProducts(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT pt.Id, pt.Name, p.Id AS ProductId, p.DateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description
                                        FROM ProductType pt
                                        LEFT JOIN Product p ON pt.Id = p.ProductTypeId
                                        WHERE pt.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType productType = null;

                    while (reader.Read())
                    {
                        if (productType == null)
                        {
                            productType = new ProductType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Products = new List<Product>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            productType.Products.Add(new Product()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),

                            });
                        }
                    }
                    
                    reader.Close();

                    return productType;
                }
            }
        }
    }
}
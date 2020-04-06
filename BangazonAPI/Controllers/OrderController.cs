using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers

{
    [Route("api/[controller]")]

    [ApiController]

    public class OrderController : ControllerBase

    {
        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)

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

        //Get by customer
        [HttpGet]
        public async Task<IActionResult> GetOrdersByCustomerId([FromQuery] int customerId)

        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText =

                     @"Select o.Id, o.CustomerId, o.UserPaymentTypeId
                     FROM [Order] o
                     Where 1 = 1";

                    if (customerId != null)

                    {
                        cmd.CommandText += " AND o.CustomerId = @customerId";

                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Order> orders = new List<Order>();

                    while (reader.Read())

                    {

              //          if (reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")) != 0)

              
                            Order order = new Order

                            {

                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),

                            };

                            orders.Add(order);
               

                    }

                    reader.Close();

                    return Ok(orders);

                }
            }

        }

        //Get order by ID
        [HttpGet("{id}", Name = "GetOrder")]

        public async Task<IActionResult> Get([FromRoute] int Id)

        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = @"SELECT o.Id AS OrderId, o.CustomerId, o.UserPaymentTypeId, ordProd.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description, p.Id AS ProductId
                    FROM OrderProduct ordProd
                    LEFT JOIN [Order] o 
                    ON o.Id = ordProd.OrderId
                    LEFT JOIN  Product p 
                    ON p.Id= ordProd.ProductId
                    WHERE o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", Id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    List<Product> products = new List<Product>();

                    while (reader.Read())

                    {

                        if (order == null)

                        {
                            order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),

                                products = new List<Product>()
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))

                            {
                                order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }

                        }

                        order.products.Add(new Product()

                        {

                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))

                        });
                    }

                    reader.Close();

                    return Ok(order);
                }
            }
        }

        //Get customer's shopping cart





        //Add a product to shopping cart
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)

        {

            using (SqlConnection conn = Connection)

            {

                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = @"INSERT INTO Order (FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @CreatedDate, 
                                        @Active, @Address, @Email, @City, @State, @Phone)";

                    cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@CreatedDate", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                    cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                    cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                    cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                    cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                    cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));

                    int newId = (int)cmd.ExecuteScalar();

                    customer.Id = newId;

                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }


        //Purchase an order
        [HttpPut("{id}")]

        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order order)

        {
            try
            {
                using (SqlConnection conn = Connection)

                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())

                    {

                        cmd.CommandText = "UPDATE [Order] SET UserPaymentTypeId = @userPaymentTypeId WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@userPaymentTypeId", order.UserPaymentTypeId));

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
                if (!OrderExists(id))
                {
                    return NotFound();
                }

                else

                {
                    throw;
                }
            }
        }

        //Remove product from cart
        [HttpDelete("{id}")]
        [Route("{orderId}/products/{productId}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromRoute] int productId)

        {
            try

            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())

                    {
                        cmd.CommandText = @"DELETE FROM OrderProduct 
                                            WHERE Id = @id AND ProductId = @productId";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@productId", productId));

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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OrderExists(int id)

        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Select Id, CustomerId, UserPaymentTypeId  
                     FROM Order 
                     Where Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
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

    public class OrdersController : ControllerBase

    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)

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
        public async Task<IActionResult> GetOrdersByCustomerId([FromQuery] int customerId, [FromQuery] bool cart)

        {

            if (cart == true)
            {
                var CustomerCart = getCustomerCart(customerId);
                return Ok(CustomerCart);
            }
            else
            {

                var AllOrders = getAllCustomerOrders(customerId);
                return Ok(AllOrders);
            }

        }

           private Order getCustomerCart(int customerId) {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                    
                {

                    cmd.CommandText = @"SELECT o.Id AS OrderId, o.CustomerId, o.UserPaymentTypeId, ordProd.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description, p.Id AS ProductId
                                        FROM [Order] o
                                        LEFT JOIN OrderProduct ordProd
                                        ON o.Id = ordProd.OrderId
                                        LEFT JOIN  Product p 
                                        ON ordProd.ProductId = p.Id
                                        WHERE o.CustomerId = @customerId";

                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    Order order = null;

                    while (reader.Read())

                    {
                            if (reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))

                            {

                                if (order == null)

                                {

                                    order = new Order()
                               
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),

                                        products = new List<Product>()
                            };

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
                        }

                    reader.Close();

                    return order;
                    }

                }
           }

    private List<Order> getAllCustomerOrders([FromQuery] int customerId) {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                    
                {

                    cmd.CommandText = @"SELECT o.Id, o.CustomerId, o.UserPaymentTypeId, ordProd.Id, ordProd.OrderId, ordProd.ProductId, p.Id, p.DateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.Description
                                        FROM [Order] o 
                                        LEFT JOIN OrderProduct ordProd ON o.Id = ordProd.OrderId
                                        LEFT JOIN Product p ON ordProd.ProductId = p.Id
                                        WHERE o.CustomerId = @customerId";

                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    
                        List<Order> orders = new List<Order>();

                        while (reader.Read())

                        {

                            var existingOrder = orders.FirstOrDefault(order => order.Id == reader.GetInt32(reader.GetOrdinal("Id")));

                            if (existingOrder == null)

                            {

                                Order order = new Order

                                {

                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    products = new List<Product>()

                                };

                                if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))

                                {

                                    order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));

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

                                orders.Add(order);

                            }

                            else

                            {

                                existingOrder.products.Add(new Product()

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

                        }

                        reader.Close();

                        return (orders);
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


                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))

                            {
                                order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }

                        }

                    }

                    reader.Close();

                    return Ok(order);
                }
            }
        }


        //Add a product to shopping cart
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerProduct customerProduct)

        {

            using (SqlConnection conn = Connection)

            {

                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {

                        cmd.CommandText = @"INSERT INTO [Order] (CustomerId, UserPaymentTypeId)
                                            OUTPUT INSERTED.Id
                                            VALUES ( @CustomerId, @UserPaymentTypeId)";

                        cmd.Parameters.Add(new SqlParameter("@CustomerId", customerProduct.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@UserPaymentTypeId", DBNull.Value));

                        int orderId = (int)cmd.ExecuteScalar();

                        var cart = new Order()

                        {
                            Id = orderId, 
                            CustomerId= customerProduct.CustomerId

                        };

                        cmd.CommandText = @"INSERT INTO OrderProduct (OrderId, ProductId)
                                            OUTPUT INSERTED.Id
                                            VALUES ( @OrderId, @ProductId)";

                        cmd.Parameters.Add(new SqlParameter("@OrderId", cart.Id ));
                        cmd.Parameters.Add(new SqlParameter("@ProductId", customerProduct.ProductId));

                        int customersId = (int)cmd.ExecuteScalar();

                        customerProduct.Id = customersId;

                return CreatedAtRoute(new { id = cart.Id }, new Order { CustomerId = customerProduct.CustomerId, Id = cart.Id });
                    
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

                        cmd.CommandText = @"UPDATE [Order] SET UserPaymentTypeId = @userPaymentTypeId
                                            WHERE Id = @id";

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
        [Route("{orderId}/products{productId}")]
        public async Task<IActionResult> Delete([FromRoute] int orderId, [FromRoute] int productId)

        {
            try

            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())

                    {
                        cmd.CommandText = @"DELETE FROM OrderProduct 
                                            WHERE OrderId = @id 
                                            AND ProductId = @productId";


                        cmd.Parameters.Add(new SqlParameter("@id", orderId));
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
                if (!OrderExists(orderId))
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
                    cmd.CommandText = @"SELECT Id, CustomerId, UserPaymentTypeId  
                                        FROM [Order] 
                                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
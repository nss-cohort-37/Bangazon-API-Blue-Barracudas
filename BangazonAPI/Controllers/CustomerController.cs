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



    public class CustomersController : ControllerBase



    {

        private readonly IConfiguration _config;



        public CustomersController(IConfiguration config)



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



        //Get Customers

        [HttpGet]

        public async Task<IActionResult> Get([FromQuery] string q)

        {

            using (SqlConnection conn = Connection)



            {

                conn.Open();



                using (SqlCommand cmd = conn.CreateCommand())



                {

                    cmd.CommandText =



                     @"Select c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Address, c.City, c.State, c.Email, c.Phone  

                     FROM Customer c

                     Where 1 = 1";



                    if (q != null)

                    {

                        cmd.CommandText += " AND FirstName Like @q OR LastName Like @q";



                        cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));

                    }



                    SqlDataReader reader = cmd.ExecuteReader();



                    List<Customer> customers = new List<Customer>();



                    while (reader.Read())

                    {

                        if (reader.GetBoolean(reader.GetOrdinal("Active")) == true)



                        {

                            Customer customer = new Customer



                            {



                                Id = reader.GetInt32(reader.GetOrdinal("Id")),

                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),

                                LastName = reader.GetString(reader.GetOrdinal("LastName")),

                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),

                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),

                                Email = reader.GetString(reader.GetOrdinal("Email")),

                                Address = reader.GetString(reader.GetOrdinal("Address")),

                                City = reader.GetString(reader.GetOrdinal("City")),

                                State = reader.GetString(reader.GetOrdinal("State")),

                                Phone = reader.GetString(reader.GetOrdinal("Phone"))

                            };



                            customers.Add(customer);

                        }

                    }



                    reader.Close();



                    return Ok(customers);



                }



            }



        }



        //Get customer by ID

        [HttpGet("{id}", Name = "GetCustomer")]



        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string include)



        {

            using (SqlConnection conn = Connection)



            {

                conn.Open();



                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = @"



                        SELECT c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Email, c.Address, c.City, c.State, c.Phone";



                    if (include == "products")

                    {

                        cmd.CommandText += ", p.Id as ProductId, p.Title, p.Description, p.Price, p.CustomerId, p.DateAdded, p.ProductTypeId";

                    }



                    cmd.CommandText += " FROM Customer c ";



                    if (include == "products")

                    {

                        cmd.CommandText += "LEFT JOIN Product p on p.CustomerId = c.Id ";

                    }



                    cmd.CommandText += "WHERE c.Id = @id AND c.Active = 1";

                    cmd.Parameters.Add(new SqlParameter("@id", id));



                    SqlDataReader reader = cmd.ExecuteReader();



                    Customer customer = null;



                    while (reader.Read())



                    {

                            if (customer == null)

                            {

                                customer = new Customer



                                {



                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),

                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),

                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),

                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),

                                    Active = reader.GetBoolean(reader.GetOrdinal("Active")),

                                    Email = reader.GetString(reader.GetOrdinal("Email")),

                                    Address = reader.GetString(reader.GetOrdinal("Address")),

                                    City = reader.GetString(reader.GetOrdinal("City")),

                                    State = reader.GetString(reader.GetOrdinal("State")),

                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),



                                    products = new List<Product>()



                                };



                            if (include == "products")



                            {

                                if (!reader.IsDBNull(reader.GetOrdinal("CustomerId")))



                                {

                                    customer.products.Add(new Product()



                                    {



                                        Id = reader.GetInt32(reader.GetOrdinal("ProductId")),

                                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),

                                        Description = reader.GetString(reader.GetOrdinal("Description")),

                                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),

                                        Title = reader.GetString(reader.GetOrdinal("Title")),

                                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),

                                        DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"))



                                    });;



                                }

                            }

                        }

                    }



                    reader.Close();



                    return Ok(customer);

                }

            }

        }



        //Add a customer

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Customer customer)



        {



            using (SqlConnection conn = Connection)



            {



                conn.Open();



                using (SqlCommand cmd = conn.CreateCommand())



                {



                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone)

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





        //Update a customer

        [HttpPut("{id}")]



        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)



        {

            try

            {

                using (SqlConnection conn = Connection)



                {

                    conn.Open();



                    using (SqlCommand cmd = conn.CreateCommand())



                    {



                        cmd.CommandText = @"UPDATE Customer

                                            SET FirstName = @firstName, LastName = @lastName, Address = @Address, City = @City, Email = @email, State = @State, Phone = @Phone

                                            WHERE Id = @id";



                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));

                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));

                        cmd.Parameters.Add(new SqlParameter("@email", customer.Email));

                        cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));

                        cmd.Parameters.Add(new SqlParameter("@City", customer.City));

                        cmd.Parameters.Add(new SqlParameter("@State", customer.State));

                        cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));



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

                if (!CustomerExists(id))

                {

                    return NotFound();

                }



                else



                {

                    throw;

                }

            }

        }





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

                        cmd.CommandText = @"UPDATE Customer 

                                            SET Active = @active

                                            WHERE Id = @id";



                        cmd.Parameters.Add(new SqlParameter("@id", id));



                        cmd.Parameters.Add(new SqlParameter("@active", false));



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

                if (!CustomerExists(id))

                {

                    return NotFound();

                }

                else

                {

                    throw;

                }

            }

        }



        private bool CustomerExists(int id)



        {

            using (SqlConnection conn = Connection)



            {

                conn.Open();



                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = @"Select Id, FirstName, LastName, Email, Address , City, State, Phone, Active, CreatedDate  

                     FROM Customer 

                     Where Id = @id";



                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();



                    return reader.Read();

                }

            }

        }

    }

}
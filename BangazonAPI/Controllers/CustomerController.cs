
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

    public class CustomerController : ControllerBase

    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)

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

        //Get all customers
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string include, [FromRoute] int Id)
        {
            if (include != "product")

            {
                var customers = GetAllCustomers();

                return Ok(customers);
            }
            else

            {
                var customers = GetCustomerWithProducts(Id);

                return Ok(customers);
            };
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute] int Id)

        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText = @"SELECT c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Address, c.City, c.State, c.Email, Phone FROM Customer c Where c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", Id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;

                    if (reader.Read())

                    {
                        if (customer == null) 
                             
                        {
                            customer = new Customer()

                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    City = reader.GetString(reader.GetOrdinal("City")),
                                    State = reader.GetString(reader.GetOrdinal("State")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone"))

                                };

                            reader.Close();

                            return Ok(customer);

                        }

                        else
                        {
                            return NotFound();
                        }
                    }

                    else



                    {



                        return NotFound();



                    }
                }
            }
        }

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
                                        VALUES (@FirstName, @LastName, @CreatedDate, @Active, @Address, @City, @State, @Email, @Phone)";

                    cmd.Parameters.Add(new SqlParameter("@Name", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@Name", customer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@CreatedDate", customer.CreatedDate));
                    cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                    cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                    cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                    cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                    cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                    cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));

                    int newId = (int)cmd.ExecuteScalar();
                    customer.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

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
                                            SET FirstName = @FirstName, LastName=@LastName, CreatedDate=@CreatedDate, Active=@Active, Address=@Address,
                                            City=@City, State=@State, Email=@Email, Phone=@Phone
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Name", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@Name", customer.LastName));
                        cmd.Parameters.Add(new SqlParameter("@CreatedDate", customer.CreatedDate));
                        cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                        cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                        cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                        cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                        cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                        cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));

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
                                            SET Active=@Active
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Active", false));

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



        private List<Customer> GetAllCustomers()

        {

            using (SqlConnection conn = Connection)

            {

                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = "SELECT c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Address, c.City, c.State, c.Email, c.Phone FROM Customer c WHERE c.Id = id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())

                    {
                        Customer customer = new Customer

                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            City = reader.GetString(reader.GetOrdinal("City")),
                            State = reader.GetString(reader.GetOrdinal("State")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };

                        customers.Add(customer);
                    }

                    reader.Close();

                    return customers;
                }
            }
        }

        private List<Customer> GetCustomerWithProducts([FromRoute] int id)

        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())

                {

                    cmd.CommandText = @"SELECT c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Address, c.City, c.State, c.Email, c.Phone
                        FROM Customer c
                        Where c.Id = @id";


                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())

                    {
                        var customer = new Customer

                        {

                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            City = reader.GetString(reader.GetOrdinal("City")),
                            State = reader.GetString(reader.GetOrdinal("State")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),

                                products = new List<Product>()
                        };

                    customer.products.Add(new Product
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        Price = reader.GetDouble(reader.GetOrdinal("Price")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Description = reader.GetString(reader.GetOrdinal("Description"))

                    });
                }

                reader.Close();

                return customers;
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
                cmd.CommandText = @"
                        SELECT Id, FirstName, LastName
                        FROM Customer
                        WHERE Id = @id";
                cmd.Parameters.Add(new SqlParameter("@id", id));

                SqlDataReader reader = cmd.ExecuteReader();

                return reader.Read();
            }
        }
    }
}
}
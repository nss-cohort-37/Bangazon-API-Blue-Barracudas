using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;

//PROPS
//publiC int Id { get; set; }
//public string Name { get; set; }
//public int Budget { get; set; }

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {

        private readonly IConfiguration _config;

        public DepartmentsController(IConfiguration config)
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


        //GET all departments               url: "api/departments"    method: GET     result: Department Array

        //DEPARTMENT PROPS
        //publiC int Id     public string Name          public int Budget       public List<Employee> EmployeesByDept

        //when running this will show a null value for the "<list>employees" on the department model 

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Budget FROM Department";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Department> departments = new List<Department>();

                    while (reader.Read())
                    {
                        Department department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = new List<Employee>()
                        };

                        departments.Add(department);
                    }
                    reader.Close();

                    return Ok(departments);

                }
            }
        }

      

        //this get method with throw an exception and will run either of these GETs. These methods are a private method listed below. 

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] string includes)
        {

            //if the department includes employees this method  will run
            if (includes == "employees")
            {
                var department = GetDepartmentWithEmployees(id);
                return Ok(department);
            }
            //if the department does not it will still render with an empty employee list 
            else
            {
                var department = GetDepartment(id);
                return Ok(department);
            }


        }


        //Add a department          url: "api/departments"         method: POST             result: Department Object   
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));

                    int newId = (int)cmd.ExecuteScalar();
                    department.Id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }

        //Update a department              url: "api/departments/{id}"	           method: PUT     result: Department Object    
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                            SET Name = @Name,
                                                Budget = @Budget
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //GET department by Id              url: "api/departments/{id}"	        method: GET     result: Department Object
        private Department GetDepartment(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Budget FROM Department WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Department department = new Department();

                    if (reader.Read())
                    {
                        department = new Department()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = new List<Employee>()
                        };
                    }
                    reader.Close();

                    return department;
                }
            }
        }
        //GET department with employees    url: "api/departments/{id}?include = employees"      method: GET         result: Department Array w/ employees

        private Department GetDepartmentWithEmployees(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id, d.Name, d.Budget, e.Id as EmployeeId, e.LastName, e.FirstName, e.Email, e.IsSupervisor, e.DepartmentId, e.ComputerId
                        FROM Department d 
                        INNER JOIN Employee e ON d.Id = e.DepartmentId
                        WHERE d.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Department department = null;

                    while (reader.Read())
                    {
                        if (department == null)
                        {
                            department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                Employees = new List<Employee>()
                            };
                        }

                        department.Employees.Add(new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                    

                        });
                    }
                    reader.Close();

                    return department;
                }
            }
        }




       

        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Budget
                        FROM Department
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
              
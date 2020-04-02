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




//Add training program                     url: api/trainingPrograms                                POST                TrainingProgram Object  Training Program Object
//Add employee to training program           api/trainingPrograms/{id}/employees                POST                Employee Object TrainingProgram Object w/ Employees
//Update training program                   api/trainingPrograms/{id}	                        PUT                 TrainingProgram Object TrainingProgram Object
//Remove training program                   api/trainingPrograms/{id}	                        DELETE
//Remove employee from program              api/trainingPrograms/{id}/employees/{employeeId}	DELETE

//TRAINING PROGRAM PROPS
//public int        Id
//public string     Name 
//public DateTime   StartDate
//public DateTime   EndDate
//public int        MaxAttendees

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramsController : ControllerBase
    {

        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
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



        //Get upcoming training programs           url: api/trainingPrograms            method: GET                       result: TrainingProgram Array

        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT tp.Id, tp.Name, tp.MaxAttendees, tp.StartDate, tp.EndDate
                                        FROM TrainingProgram tp 
                                        WHERE tp.id LIKE @q";
                    cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        var trainingProgramId = reader.GetInt32(reader.GetOrdinal("Id"));
                        var trainingProgramAlreadyAdded = trainingPrograms.FirstOrDefault(tprog => tprog.Id == trainingProgramId);

                        if (trainingProgramAlreadyAdded == null)
                        { 

                            TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            Employees = new List<Employee>()
                          
                        };

                        trainingPrograms.Add(trainingProgram);
                        
                        var hasEmployees = !reader.IsDBNull(reader.GetOrdinal("Id"));
                        if (hasEmployees)


                            trainingProgram.Employees.Add(new Employee()
                        {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                                //ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                //DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                //computer = new Computer(),
                                //department = new Department()
                            }

                        );


                    }
                    else

                    {
                        var hasEmployees = !reader.IsDBNull(reader.GetOrdinal("Id"));

                        if (hasEmployees)
                        {
                            trainingProgramAlreadyAdded.Employees.Add(new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                            });
                        }

                    }
                }
                reader.Close();

                    return Ok(trainingPrograms);

                }
            }
        }


        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT tp.Id, tp.Name, tp.StartDate, tp.MaxAttendees, tp.EndDate, 
                                          e.Id, e.FirstName, e.LastName, e.DepartmentId, 
                                              e.Email, e.IsSupervisor, e.ComputerId 
                                        FROM TrainingProgram tp 
                                        INNER JOIN EmployeeTraining et ON tp.Id = et.TrainingProgramId 
                                        INNER JOIN Employee e ON e.Id = et.EmployeeId
                                        WHERE tp.id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    TrainingProgram trainingProgram = null;
                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                                Employees = new List<Employee>()
                            };
                        }

                        trainingProgram.Employees.Add(new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                        });
                    }
                    reader.Close();
                    return Ok(trainingProgram);
                }
            }
        }
    }
}






//        //Add a trainingProgram          url: "api/trainingPrograms"         method: POST             result: TrainingProgram Object   
//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, Budget)
//                                        OUTPUT INSERTED.Id
//                                        VALUES (@Name, @Budget)";
//                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
//                    cmd.Parameters.Add(new SqlParameter("@Budget", trainingProgram.Budget));

//                    int newId = (int)cmd.ExecuteScalar();
//                    trainingProgram.Id = newId;
//                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
//                }
//            }
//        }

//        //Update a trainingProgram              url: "api/trainingPrograms/{id}"	           method: PUT     result: TrainingProgram Object    
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] TrainingProgram trainingProgram)
//        {
//            try
//            {
//                using (SqlConnection conn = Connection)
//                {
//                    conn.Open();
//                    using (SqlCommand cmd = conn.CreateCommand())
//                    {
//                        cmd.CommandText = @"UPDATE TrainingProgram
//                                            SET Name = @Name,
//                                                Budget = @Budget
//                                            WHERE Id = @id";
//                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
//                        cmd.Parameters.Add(new SqlParameter("@Budget", trainingProgram.Budget));
//                        cmd.Parameters.Add(new SqlParameter("@id", id));

//                        int rowsAffected = cmd.ExecuteNonQuery();
//                        if (rowsAffected > 0)
//                        {
//                            return new StatusCodeResult(StatusCodes.Status204NoContent);
//                        }
//                        throw new Exception("No rows affected");
//                        throw new Exception("No rows affected");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                if (!TrainingProgramExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//        }






//private bool TrainingProgramExists(int id)
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = @"
//                        SELECT Id, Name, Budget
//                        FROM TrainingProgram
//                        WHERE Id = @id";
//            cmd.Parameters.Add(new SqlParameter("@id", id));

//            SqlDataReader reader = cmd.ExecuteReader();
//            return reader.Read();
//        }
//    }
//        }
//    }
//}

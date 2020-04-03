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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
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
                                          e.Id AS EmployeeId , e.FirstName, e.LastName,
                                              e.Email, e.IsSupervisor, e.DepartmentId, e.ComputerId
                                        FROM TrainingProgram tp 
                                        LEFT JOIN EmployeeTraining et ON tp.Id = et.TrainingProgramId 
                                        LEFT JOIN Employee e ON e.Id = et.EmployeeId
                                        WHERE tp.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

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

                        var hasEmployees = !reader.IsDBNull(reader.GetOrdinal("EmployeeId"));

                        if (hasEmployees)
                        {

                            trainingProgram.Employees.Add(new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId"))
                            });
                        }

                    }
                    reader.Close();
                    return Ok(trainingProgram);
                }
            }
        }






        //Add a trainingProgram          url: "api/trainingPrograms"         method: POST             result: TrainingProgram Object   
        //When POSTing StartDate & EndDate you must use format year month date (i.e. 2018-09-06)

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.Id
                                        VALUES ( @Name, @StartDate, @EndDate, @MaxAttendees)";
                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));

                    int newId = (int)cmd.ExecuteScalar();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
                }
            }
        }


        //Add employee to training program              url: api/trainingPrograms/{id}/employees                method: POST                Employee Object      TrainingProgram Object w/ Employees

        [HttpPost]
        [Route("{id}/employees")]
        public async Task<IActionResult> Post([FromBody] Employee employee, 
                                               [FromRoute] int id)
                                                
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@EmployeeId, @TrainingProgramId)";
                    cmd.Parameters.Add(new SqlParameter("@EmployeeId", employee.Id));
                    cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", id));

                    int newId = (int)cmd.ExecuteScalar();
                    return RedirectToRoute("GetTrainingProgram", new {id = id} );
                   
                }
            }
        }

        //Remove employee from program                  url: api/trainingPrograms/{id}/employees/{employeeId}	DELETE

        [HttpDelete("{id}")]
        [Route("{trainingProgramId}/employees/{employeeId}")]
  
        public async Task<IActionResult> Delete([FromRoute] int employeeId,
                                                [FromRoute] int trainingProgramId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"  DELETE FROM EmployeeTraining WHERE EmployeeId = @EmployeeId AND TrainingProgramId = @TrainingProgramId";
                        cmd.Parameters.Add(new SqlParameter("@EmployeeId", employeeId));
                        cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", trainingProgramId));

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
                if (!TrainingProgramExists(employeeId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //Update training program                       url: api/trainingPrograms/{id}	                        PUT                 TrainingProgram Object TrainingProgram Object

        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] TrainingProgram trainingProgram)
{
    try
    {
        using (SqlConnection conn = Connection)
        {
            conn.Open();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE TrainingProgram
                                            SET Name = @Name,
                                                StartDate = @StartDate,
                                                EndDate = @EndDate,
                                                MaxAttendees = @MaxAttendees
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));
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
        if (!TrainingProgramExists(id))
        {
            return NotFound();
        }
        else
        {
            throw;
        }
    }
}

        //Remove training program                       url: api/trainingPrograms/{id}	                        DELETE

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
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TrainingProgramExists(int id)
{
    using (SqlConnection conn = Connection)
    {
        conn.Open();
        using (SqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                        SELECT Id, Name, StartDate, EndDate, MaxAttendees
                        FROM TrainingProgram
                        WHERE Id = @id";
            cmd.Parameters.Add(new SqlParameter("@id", id));

            SqlDataReader reader = cmd.ExecuteReader();
            return reader.Read();
        }
    }
}
    }
}

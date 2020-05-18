using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apbd_example_tutorial_10.Entities;
using Apbd_example_tutorial_10.DTOs.Requests;
using Apbd_example_tutorial_10.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Apbd_example_tutorial_10.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly StudentContext _studentContext;

        public StudentsController(StudentContext studentContext)
        {
            _studentContext = studentContext;
        }

        // Note: I used to Task<IActionResult> because i wanted to use async 

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                var students = await _studentContext.Student
                                              .Include(s => s.IdEnrollmentNavigation).ThenInclude(e => e.IdStudyNavigation)
                                              .Select(s => new GetStudentsResponse
                                              {
                                                  IndexNumber = s.IndexNumber,
                                                  FirstName = s.FirstName,
                                                  LastName = s.LastName,
                                                  BirthDate = s.BirthDate.ToShortDateString(),
                                                  Semester = s.IdEnrollmentNavigation.Semester,
                                                  Studies = s.IdEnrollmentNavigation.IdStudyNavigation.Name
                                              })
                                              .ToListAsync();
                return Ok(students);

            }catch(Exception e)
            {
                return BadRequest(e);
            }
        }


        
        [HttpPost]
        public async Task<IActionResult> InsertStudent(Student student)
        {

            try
            {
                await _studentContext.Student.AddAsync(student);

                await _studentContext.SaveChangesAsync();

                CRUDStudentResponse resp = new CRUDStudentResponse
                {
                    message = "The Following Student was inserted successfully!",
                    firstName = student.FirstName,
                    lastName = student.LastName,
                    index = student.IndexNumber
                };

                return Ok(resp);

            }catch(Exception e)
            {
                return BadRequest(e);
            }
}

        [HttpPut]
        public async Task<IActionResult> UpdateStudent(Student student)
        {
            //  var res = _studentContext.Student.Where(s => s.IndexNumber == student.IndexNumber).FirstOrDefaultAsync();

            try { 
                _studentContext.Attach(student);
                _studentContext.Entry(student).State = EntityState.Modified;
                await _studentContext.SaveChangesAsync();
           

                CRUDStudentResponse resp = new CRUDStudentResponse
                {
                    message = "The Following Student was updated successfully!",
                    firstName = student.FirstName,
                    lastName = student.LastName,
                    index = student.IndexNumber
                };

                return Ok(resp);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpDelete("{indexNumber}")]
        public async Task<IActionResult> DeleteStudent(string index)
        {
            // var student = await _studentContext.Student.FindAsync(index);
            //Instead of first selecting the student and then deleting him/her we can immediately delete a student that we create and use change tracking

            try
            {
                var student = new Student { IndexNumber = index };
                _studentContext.Attach(student);
                _studentContext.Entry(student).State = EntityState.Deleted;
                await _studentContext.SaveChangesAsync();

                CRUDStudentResponse resp = new CRUDStudentResponse
                {
                    message = "The Following Student was deleted successfully!",
                    firstName = student.FirstName,
                    lastName = student.LastName,
                    index = student.IndexNumber
                };

                return Ok(resp);

            }catch(Exception e)
            {
                return BadRequest(e);
             }
        }

        [HttpPost("EnrollStudent")]
        public async Task<IActionResult> EnrollStudent(EnrollStudentRequest request)
        {
                var resp = new EnrollStudentResponse();
                var idStudy = await _studentContext.Studies.Where(s => s.Name == request.Studies).Select(s => s.IdStudy ).FirstOrDefaultAsync();
               
                try
                {
                    // or i could get idStudy as a list and compare count
                    if (idStudy.Equals(null))
                    {
                        return BadRequest("No study was found");
                    }

                    int enrollId = 0;
                    var idEnrollment = await _studentContext.Enrollment.Where(e => (e.Semester == 1) && (e.IdStudy ==  idStudy) ).Select(e =>  e.IdEnrollment ).FirstOrDefaultAsync();

                    //check if enrollment already exists , else insert one 

                    if (idEnrollment.Equals(null))
                    {

                        //Select Top 1 IdEnrollment as id from Enrollment Order By IdEnrollment DESC
                        var id = await _studentContext.Enrollment.OrderByDescending(e => e.IdEnrollment).Select(e => e.IdEnrollment).FirstOrDefaultAsync();

                        var enrollment = new Enrollment
                        {
                            IdEnrollment = ++id,
                            IdStudy = idStudy,
                            Semester = 1,
                            StartDate = DateTime.Now.Date
                        };

                        await _studentContext.Enrollment.AddAsync(enrollment);

                        await _studentContext.SaveChangesAsync();

                    }
                    else
                    {
                        enrollId = idEnrollment;
                    }

                //check if index number was assigned to any other student , if not insert student 
                var studentCount = await _studentContext.Student.Where(s => s.IndexNumber == request.IndexNumber).Select(s => new { countNum = s.IndexNumber.Count()}).FirstOrDefaultAsync();
                
                    if (studentCount.countNum > 0)
                    {
                        return BadRequest("Student Already Exists");
                    }

                var student = new Student
                {
                    IndexNumber = request.IndexNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate,
                    IdEnrollment  = enrollId
                };

                resp.IdEnrollment = enrollId;
                resp.IndexNumber = request.IndexNumber;
                resp.Semester = 1;

                    
                }
                catch (Exception e)
                {
                    return BadRequest(e);
                }

                return Ok(resp);
            }

        }

    }

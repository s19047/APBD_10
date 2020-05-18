using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apbd_example_tutorial_10.Entities;
using Apbd_example_tutorial_10.Models;
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

        }

        [HttpPut]
        public async Task<IActionResult> UpdateStudent(Student student)
        {
            //  var res = _studentContext.Student.Where(s => s.IndexNumber == student.IndexNumber).FirstOrDefaultAsync();

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

        [HttpDelete("{indexNumber}")]
        public async Task<IActionResult> DeleteStudent(string index)
        {
            // var student = await _studentContext.Student.FindAsync(index);
            //Instead of first selecting the student and then deleting him/her we can immediately delete a student that we create and use change tracking

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

        }

    }
}
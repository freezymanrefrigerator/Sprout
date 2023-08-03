using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.WebApp.Models;
using Sprout.Exam.WebApp.Data;
using Microsoft.EntityFrameworkCore;
using Sprout.Exam.WebApp.Services;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeFactory _employeeFactory;

        public EmployeesController(ApplicationDbContext context, IEmployeeFactory employeeFactory)
        {
            _context = context;
            _employeeFactory = employeeFactory;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _context.Employee
                                       .Where(e => !e.IsDeleted) 
                                       .ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.Employee.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            var item = await _context.Employee.FirstOrDefaultAsync(e => e.Id == input.Id && !e.IsDeleted);
            if (item == null) return NotFound();

            item.FullName = input.FullName;
            item.Tin = input.Tin;
            item.Birthdate = input.Birthdate;
            item.EmployeeTypeId = input.EmployeeTypeId;

            _context.Employee.Update(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        private async Task<bool> TinExists(string tin)
        {
            var existingEmployee = await _context.Employee.FirstOrDefaultAsync(e => e.Tin == tin && !e.IsDeleted);
            return existingEmployee != null;
        }

        [HttpGet("CheckTin/{tin}")]
        public async Task<IActionResult> CheckTin(string tin)
        {
            return Ok(await TinExists(tin));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateEmployeeDto input)
        {
            if (await TinExists(input.Tin))
            {
                return Conflict("An employee with this TIN already exists.");
            }

            var employee = new EmployeeDto
            {
                Birthdate = input.Birthdate,
                FullName = input.FullName,
                Tin = input.Tin,
                EmployeeTypeId = input.EmployeeTypeId,
                IsDeleted = false 
            };

            await _context.Employee.AddAsync(employee);
            await _context.SaveChangesAsync();

            return Created($"/api/employees/{employee.Id}", employee.Id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();


            employee.IsDeleted = true;

            _context.Employee.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(id);
        }

        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculatePayLoad payload)
        {
            var result = await _context.Employee
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(e => e.Id == payload.Id && !e.IsDeleted);

            if (result == null) return NotFound();

            var type = (EmployeeType)result.EmployeeTypeId;

            var employee = _employeeFactory.CreateEmployee(type);

            var computedSalary = employee.CalculateSalary(payload.AbsentDays, payload.WorkedDays);

            return Ok(Math.Round(computedSalary, 2));
        }
    }
}

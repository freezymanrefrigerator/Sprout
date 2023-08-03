using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sprout.Exam.Business.DataTransferObjects;

namespace Sprout.Exam.WebApp
{
    public static class StaticEmployees
    {
        public static List<EmployeeDto> ResultList = new()
        {
            new EmployeeDto
            {
                Birthdate = DateTime.ParseExact("1993-03-25", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                FullName = "Jane Doe",
                Id = 1,
                Tin = "123215413",
                EmployeeTypeId = 1
            },
            new EmployeeDto
            {
                Birthdate = DateTime.ParseExact("1993-05-28", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                FullName = "John Doe",
                Id = 2,
                Tin = "957125412",
                EmployeeTypeId = 2
            }
        };
    }
}

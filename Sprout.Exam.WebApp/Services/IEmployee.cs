using System;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;

namespace Sprout.Exam.WebApp.Services
{
    public interface IEmployee
    {
        decimal CalculateSalary(decimal absentDays, decimal workedDays);
    }

    public class RegularEmployee : IEmployee
    {
        private const decimal MonthlySalary = 20000m;
        private const decimal TaxPercentage = 0.12m;

        public decimal CalculateSalary(decimal absentDays, decimal workedDays)
        {
            return MonthlySalary - (MonthlySalary / 22m * absentDays) - (MonthlySalary * TaxPercentage);
        }
    }

    public class ContractualEmployee : IEmployee
    {
        private const decimal DailySalary = 500m;

        public decimal CalculateSalary(decimal absentDays, decimal workedDays)
        {
            return DailySalary * workedDays;
        }
    }

    public interface IEmployeeFactory
    {
        IEmployee CreateEmployee(EmployeeType type);
    }

    public class EmployeeFactory : IEmployeeFactory
    {
        public IEmployee CreateEmployee(EmployeeType type)
        {
            switch (type)
            {
                case EmployeeType.Regular:
                    return new RegularEmployee();
                case EmployeeType.Contractual:
                    return new ContractualEmployee();
                // Add more cases here for other employee types if needed
                default:
                    throw new ArgumentException("Invalid employee type", nameof(type));
            }
        }
    }
}

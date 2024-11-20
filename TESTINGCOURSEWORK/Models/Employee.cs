using System;
using System.Collections.Generic;

namespace TESTINGCOURSEWORK.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? Position { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    public decimal? Salary { get; set; }
}

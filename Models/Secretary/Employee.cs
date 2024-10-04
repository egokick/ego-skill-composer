namespace skill_composer.Models
{
    /// <summary>
    /// Represents an Employee in the organization.
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Gets or sets the unique identifier for the Employee.
        /// </summary>
        public int EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the BusinessId associated with the Employee.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the Employee.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the Employee.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the Employee.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the Specialisation of the employee.
        /// eg., Hair Dressing, Hair Colour, Landscape Design.
        /// </summary>
        public string Specialisation { get; set; }

        /// <summary>
        /// Gets or sets the position/title of the Employee within the organization.
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the Employee.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the Employee.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets the employee as a comma separated list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var employee = string.Empty;

            if (!string.IsNullOrEmpty(FirstName)) { employee += $"{FirstName}"; }
            if (!string.IsNullOrEmpty(MiddleName)) { employee += $" {MiddleName}"; }
            if (!string.IsNullOrEmpty(LastName)) { employee += $" {LastName}"; }
            if (!string.IsNullOrEmpty(Specialisation)) { employee += $" {Specialisation}"; }
            if (!string.IsNullOrEmpty(Position)) { employee += $", {Position}"; }
            if (!string.IsNullOrEmpty(Phone)) { employee += $", phone {Phone}"; }
            if (!string.IsNullOrEmpty(Email)) { employee += $", email {Email}"; }

            return employee;
        }
    }
}

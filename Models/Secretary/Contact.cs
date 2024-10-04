namespace skill_composer.Models
{
    /// <summary>
    /// Represents a contact entity.
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Gets or sets the unique identifier for the contact.
        /// </summary>
        public int ContactId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the business.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the contact.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the contact.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the contact.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the position/title of the Employee within the organization.
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the contact.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the email address of the contact.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Is the main business contact.
        /// </summary>
        public bool IsMainContact { get; set; } = false;

        /// <summary>
        /// Gets the contact as a comma separated list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var contact = string.Empty;

            if (!string.IsNullOrEmpty(FirstName)) { contact += $"{FirstName}"; }
            if (!string.IsNullOrEmpty(MiddleName)) { contact += $" {MiddleName}"; }
            if (!string.IsNullOrEmpty(LastName)) { contact += $" {LastName}"; }
            if (!string.IsNullOrEmpty(Position)) { contact += $", {Position}"; }
            if (!string.IsNullOrEmpty(Phone)) { contact += $", phone {Phone}"; }
            if (!string.IsNullOrEmpty(Email)) { contact += $", email {Email}"; }

            if (IsMainContact) { contact += " this is the main business Contact."; }

            return contact;
        }
    }
}

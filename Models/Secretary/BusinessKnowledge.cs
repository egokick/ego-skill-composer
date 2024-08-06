namespace skill_composer.Models
{
    /// <summary>
    /// Business Knowledge.
    /// </summary>
    public class BusinessKnowledge
    {
  
        /// <summary>
        /// Gets or sets the unique identifier for the business.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SubDomain { get; set; }

        /// <summary>
        /// Business Addresses.
        /// </summary>
        public IList<Address> Addresses { get; set; }

        /// <summary>
        /// Business Contacts.
        /// </summary>
        public IList<Contact> Contacts { get; set; }

        /// <summary>
        /// Business Employees.
        /// </summary>
        public IList<Employee> Employees { get; set; }

        /// <summary>
        /// Business Hours.
        /// </summary>
        public IList<BasicBusinessHour> BusinessHours { get; set; }

        /// <summary>
        /// Question and Answers.
        /// </summary>
        public IList<QuestionAnswer> QuestionAnswers { get; set; }

        /// <summary>
        /// Business Phone Number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// CustomerPhoneNumber.
        /// </summary>
        public BasicCustomer Customer { get; set; }

        /// <summary>
        /// List of services provided by the company.
        /// </summary>
        public IList<Service> Services { get; set; }

        /// <summary>
        /// List of resources that facilitate the services.
        /// </summary>
        public IList<Resource> Resources { get; set; }

        /// <summary>
        /// List of Service Resources.
        /// </summary>
        public IList<ServiceResource> ServiceResources { get; set; }
         
    }
}

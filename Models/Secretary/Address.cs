namespace skill_composer.Models
{   
    /// <summary>
    /// Represents an address entity.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the unique identifier for the address.
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the business associated with this address.
        /// </summary>
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the first line of the address.
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the address.
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the town or city of the address.
        /// </summary>
        public string Town { get; set; }

        /// <summary>
        /// Gets or sets the county or region of the address.
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the address.
        /// </summary>
        public string Postcode { get; set; }

        /// <summary>
        /// Is the main business address
        /// </summary>
        public bool IsMainAddress { get; set; } = false;

        /// <summary>
        /// Gets the address as a comma separated list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var address = string.Empty;
            if(!string.IsNullOrEmpty(LocationName)) { address = LocationName; }

            if (!string.IsNullOrEmpty(Address1)) { address += $", {Address1}"; }
            if (!string.IsNullOrEmpty(Address2)) { address += $", {Address2}"; }
            if (!string.IsNullOrEmpty(Town)) { address += $", {Town}"; }
            if (!string.IsNullOrEmpty(County)) { address += $", {County}"; }
            if (!string.IsNullOrEmpty(Postcode)) { address += $", {Postcode}"; }

            if (IsMainAddress) { address += " this is the main business Address."; }

            return address;
        }
    }
}

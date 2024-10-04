namespace skill_composer.Models
{
    /// <summary>
    /// Represents a phone number associated with a business.
    /// </summary>
    public class BusinessPhoneNumber
    {
        /// <summary>
        /// Gets or sets the unique identifier for the BusinessPhoneNumber.
        /// </summary>
        public int BusinessPhoneNumberId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the Business associated with this phone number.
        /// </summary> 
        public int BusinessId { get; set; }

        /// <summary>
        /// Gets or sets the phone number string.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the monthly cost of the phone number.
        /// </summary>
        public decimal MonthlyCost { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the phone number was purchased.
        /// </summary>
        public DateTime PurchasedOn { get; set; }

        /// <summary>
        /// Supported text to speech Language eg.,
        /// English en-US, en-GB, en-AU, en-CA, en-IN
        /// Spanish es-MX, es-ES, es-US
        /// French fr-FR, fr-CA
        /// German de-DE, de-AT
        /// Italian it-IT
        /// Japanese ja-JP
        /// Chinese zh-CN, zh-TW
        /// Korean ko-KR
        /// Arabic ar-SA
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Text to speech voice, either an Amazon Polly Neural or Google (WaveNet, Neural2) voice.
        /// </summary>
        public string Voice { get; set; }

        /// <summary>
        /// Spoken introduction of the company on the phone line eg.,
        /// {Greeting}, you've reached Digital Dreams. My name is Tracy, and I'm here to assist you. How may I be of service today?
        /// Where {Greeting} would be replaced with either Good morning/afternoon depending on the time of day.
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// The Twilio sid for this phone number.
        /// </summary>
        public string TwilioSid { get; set; }

        /// <summary>
        /// Customers Phone Number.
        /// </summary>
        public string CustomerPhoneNumber { get; set; }

        /// <summary>
        /// Company Name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// If true this is a warm transfer.
        /// </summary>
      
        public bool IsWarmTransfer { get; set; }

        /// <summary>
        /// Used for transferring a call into a conference.
        /// </summary>
 
        public string ConnectingCallSid { get; set; }

        /// <summary>
        /// Used for transferring a call into a conference.
        /// </summary>
      
        public string ConferenceName { get; set; }

        public string SkillName { get; set; }


        public int ConversationMetaId { get; set; }
    }
}

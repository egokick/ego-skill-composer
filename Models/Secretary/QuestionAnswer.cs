namespace skill_composer.Models
{
    /// <summary>
    /// Question Answer.
    /// </summary>
    public class QuestionAnswer
    {
        /// <summary>
        /// Secretary Task Identity.
        /// </summary>
        public int SecretaryTaskId { get; set; }

        /// <summary>
        /// Task Name.
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Question Identity.
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Question Text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Answer Identity.
        /// </summary>
        public int AnswerId { get; set; }

        /// <summary>
        /// Answer Text.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets the question and answers as a comma separated list.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var questionAndAnswer = string.Empty;

            if (!string.IsNullOrEmpty(QuestionText)) { questionAndAnswer += $"{QuestionText}"; }
            if (!string.IsNullOrEmpty(AnswerText)) { questionAndAnswer += $": {AnswerText}"; }

            return questionAndAnswer;
        }
    }
}

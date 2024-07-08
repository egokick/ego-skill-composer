using skill_composer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.SpecialActionsTestData
{
    public class QRCodeCreate : ISpecialActionTestData
    {
        public (skill_composer.Models.Task, Settings?) GetTestData()
        {
            var task = new skill_composer.Models.Task()
            {
                Name = "Create the QR code from the previous input. Saves the QR code as a PNG and sets the FilePath.",
                Input = "mailto:egokick@gmail.com?subject=testsubject&body=testbody",
                Mode = "Internal",
                SpecialAction = "QRCodeCreate"
            };

            return (task, null, null);
        }
    }
}

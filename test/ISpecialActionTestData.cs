using skill_composer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    public interface ISpecialActionTestData
    {
        public (skill_composer.Models.Task, Settings?) GetTestData();
    }
}

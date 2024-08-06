using skill_composer.Models;
using skill_composer.Helper;
using skill_composer.Services;
using Newtonsoft.Json;

namespace skill_composer.SpecialActions
{
    public class DatabaseGetBusinessKnowledgeFromBusinessIdAndCustomerPhoneNumber : ISpecialAction
    {  
        public async Task<Models.Task> Execute(Models.Task task, Skill selectedSkill)
        {
            var request = JsonConvert.DeserializeObject<DatabaseGetBusinessKnowledgeFromBusinessIdAndCustomerPhoneNumberRequest>(task.Input);
            
            if (request is null) 
            {
                return task;
            }

            var businessKnowledge = await GetBusinessKnowledge(request.businessId, request.customerPhoneNumber); 
            
            task.Output = businessKnowledge;
           
            return task;
        }

        private async Task<string> GetBusinessKnowledge(int businessId, string customerPhoneNumber = "")
        {
            var databaseService = new DatabaseService(Program._settings);

            var parameters = new Dictionary<string, string> { { "p_BusinessId", businessId.ToString() }, { "p_CustomerPhoneNumber", customerPhoneNumber } };
            var ds = await databaseService.Execute("secretary.Business_GetBusinessKnowledge", parameters);

            var businessTable = ds.Tables[0];
            var addressTable = ds.Tables[1];
            var contactTable = ds.Tables[2];
            var employeeTable = ds.Tables[3];
            var businessHourTable = ds.Tables[4];
            var questionAnswerTable = ds.Tables[5];
            var serviceTable = ds.Tables[6];
            var resourceTable = ds.Tables[7];
            var serviceResourceTable = ds.Tables[8];
            var customerTable = ds.Tables[9];
            var promptTable = ds.Tables[10];

            var businessKnowledge = PropertyMapper.CreateItem<BusinessKnowledge>(businessTable.Rows[0]);
            businessKnowledge.Addresses = PropertyMapper.ConvertTo<Address>(addressTable.Rows);
            businessKnowledge.Contacts = PropertyMapper.ConvertTo<Contact>(contactTable.Rows);
            businessKnowledge.Employees = PropertyMapper.ConvertTo<Employee>(employeeTable.Rows);
            businessKnowledge.BusinessHours = PropertyMapper.ConvertTo<BasicBusinessHour>(businessHourTable.Rows);
            businessKnowledge.QuestionAnswers = PropertyMapper.ConvertTo<QuestionAnswer>(questionAnswerTable.Rows);
            businessKnowledge.Services = PropertyMapper.ConvertTo<Service>(serviceTable.Rows);
            businessKnowledge.Resources = PropertyMapper.ConvertTo<Models.Resource>(resourceTable.Rows);
            businessKnowledge.ServiceResources = PropertyMapper.ConvertTo<ServiceResource>(serviceResourceTable.Rows);

            if (!string.IsNullOrEmpty(customerPhoneNumber) && customerTable.Rows is { Count: > 0 })
            {
                businessKnowledge.Customer = PropertyMapper.CreateItem<BasicCustomer>(customerTable.Rows[0]);
            }
            
            var businessKnowledgeString = JsonConvert.SerializeObject(businessKnowledge);

            return businessKnowledgeString; 
        } 
    }
}

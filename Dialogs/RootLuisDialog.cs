namespace LuisBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;

    [Serializable]
    [LuisModel("7951d94c-f2ae-4823-ac4e-991cbaa7d256", "4b2f4de202f54faca5852e864cbde6ba")]
    public class RootLuisDialog : LuisDialog<object>
    {
        public const string EntityUserName = "UserName";
        public const string EntityAccountNumber = "AccountNumber";
        public const string EntityEmployeeName = "EmployeeName";

        public const string UserAuthenticatedKey = "UserAuthenticated";
        public const string LastFourSsnKey = "LastFourSsn";
        public const string CompanyNameKey = "CompanyName";
        private bool isUserAuthenticated(IDialogContext context)
        {
            bool authenticated = false;
            context.ConversationData.TryGetValue(UserAuthenticatedKey, out authenticated);
            return authenticated;
        }

        private const string EntityGeographyCity = "builtin.geography.city";

        private const string EntityHotelName = "Hotel";

        private const string EntityAirportCode = "AirportCode";

        private IList<string> titleOptions = new List<string> { "“Very stylish, great stay, great staff”", "“good hotel awful meals”", "“Need more attention to little things”", "“Lovely small hotel ideally situated to explore the area.”", "“Positive surprise”", "“Beautiful suite and resort”" };

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' to see the kind of tasks I can help you with.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Farewell")]
        public async Task Farewell(IDialogContext context, LuisResult result)
        {
            string userName;
            string message;
            if (context.ConversationData.TryGetValue(EntityUserName, out userName))
            {
                message = $"OK, {userName}, thank for contacting UltiPro support. Have a great day!";
            }
            else
            {
                message = $"OK, thank for contacting UltiPro support. Have a great day!";
            }
            context.ConversationData.Clear();
            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greet(IDialogContext context, LuisResult result)
        {

            string message = isUserAuthenticated(context) ?
                "Hi, thank you for contacting UltiPro support. What can I help you with today?" : 
                "Hi, thank you for contacting UltiPro support. Can I please have your name and account number?";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Identification")]
        public async Task Identification(IDialogContext context, LuisResult result)
        {
            EntityRecommendation userNameEntity;
            EntityRecommendation accountNumberEntity;

            string userNameValue;
            string accountNumberValue;

            if (!result.TryFindEntity(EntityUserName, out userNameEntity))
            {
                PromptDialog.Text(
                context: context,
                resume: HandleUserNamePrompt,
                prompt: "I didin't catch your name. What is your name please?",
                retry: "I didn't understand. Please try again.");
            }
            else
            {
                context.ConversationData.SetValue(EntityUserName, userNameEntity.Entity);
                if (!result.TryFindEntity(EntityAccountNumber, out accountNumberEntity))
                {
                    PromptDialog.Text(
                        context: context,
                        resume: HandleAccountNumberPrompt,
                        prompt: $"OK {userNameEntity.Entity}, I didin't catch your account number. What is your account number please?",
                        retry: "I didn't understand. Please try again.");
                }
                else
                {
                    context.ConversationData.SetValue(EntityAccountNumber, accountNumberEntity.Entity);
                    await GreetByName(context);
                }
            }
        }

        public async Task HandleUserNamePrompt(IDialogContext context, IAwaitable<string> argument)
        {
            string userName = await argument;
            context.ConversationData.SetValue(EntityUserName, userName);
            PromptDialog.Text(
                context: context,
                resume: HandleAccountNumberPrompt,
                prompt: $"OK {userName}, I didin't catch your account number. What is your account number please?",
                retry: "I didn't understand. Please try again.");
        }

        public async Task HandleAccountNumberPrompt(IDialogContext context, IAwaitable<string> argument)
        {
            string accountNumber = await argument;
            context.ConversationData.SetValue(EntityAccountNumber, accountNumber);
            await GreetByName(context);
        }

        public async Task GreetByName(IDialogContext context)
        {
            context.ConversationData.SetValue(UserAuthenticatedKey, true);
            string userName = context.ConversationData.Get<string>(EntityUserName);
            string message = $"Thank you, {userName}. What can I help you with today?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task ShowOptions(IDialogContext context, LuisResult result)
        {
            string message = "I am able to help you with a few tasks such as:\n" +
                "- deleting an employee \t\n" +
                "- checking your W2 print status \t\n" + 
                "Can I help you with any of those? If not, let me know if you'd like to speak to a human representative";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("TransferToRepresentative")]
        public async Task TransferToRepresentative(IDialogContext context, LuisResult result)
        {
            string message = "OK, I will transfer you to an UltiPro human representative. Thank you!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("DeleteEmployee")]
        public async Task DeleteEmployee(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityemployeeName;
            if (result.TryFindEntity(EntityEmployeeName, out entityemployeeName))
            {
                context.ConversationData.SetValue(EntityEmployeeName, entityemployeeName.Entity);
                PromptDialog.Text(
                context: context,
                resume: HandleEmployeeLastFourOfSocialSecurityPrompt,
                prompt: $"OK, please enter the last 4 digits of {entityemployeeName.Entity}'s SSN to confirm",
                retry: "I didn't understand. Please try again.");
            }
            else
            {
                PromptDialog.Text(
                context: context,
                resume: HandleEmployeeNamePrompt,
                prompt: $"What is the name of the employee you would like me to delete from the system?",
                retry: "I didn't understand. Please try again.");
            }
        }

        public async Task HandleEmployeeNamePrompt(IDialogContext context, IAwaitable<string> argument)
        {
            string employeeName = await argument;
            context.ConversationData.SetValue(EntityEmployeeName, employeeName);
            PromptDialog.Text(
                context: context,
                resume: HandleEmployeeLastFourOfSocialSecurityPrompt,
                prompt: $"OK, please enter the last 4 digits of {employeeName}'s SSN to confirm",
                retry: "I didn't understand. Please try again.");
        }

        public async Task HandleEmployeeLastFourOfSocialSecurityPrompt(IDialogContext context, IAwaitable<string> argument)
        {
            string lastFourOfSsn = await argument;
            context.ConversationData.SetValue(LastFourSsnKey, lastFourOfSsn);
            await DeleteEmployee(context);
        }

        public async Task DeleteEmployee(IDialogContext context)
        {
            string employeeName = context.ConversationData.Get<string>(EntityEmployeeName);
            string lastFourSsn = context.ConversationData.Get<string>(LastFourSsnKey);
            string message = $"OK, I have deleted the employee {employeeName} with SSN XXX-XX-{lastFourSsn} from the system. " +
                "Is there anything else can I help you with?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("CheckW2PrintJobStatus")]
        public async Task CheckW2PrintJobStatus(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: HandleCompanyNamePrompt,
            prompt: "Sure, I can check on the status of that W2 print job for you. Can I have your company name please?",
            retry: "I didn't understand. Please try again.");
        }

        public async Task HandleCompanyNamePrompt(IDialogContext context, IAwaitable<string> argument)
        {
            string companyName = await argument;
            context.ConversationData.SetValue(CompanyNameKey, companyName);
            await ShowStatusOfW2PrintJob(context);
        }

        public async Task ShowStatusOfW2PrintJob(IDialogContext context)
        {
            string companyName = context.ConversationData.Get<string>(CompanyNameKey);
            string message = $"OK, I see in the system that the W2s for {companyName} have been printed and were shipped on 10/12.\t\n" +
                "The Fedex tracking number for the package is 7273 2836 2832 3942 \t\n" + 
                "Is there anything else can I help you with?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}



using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.InteropServices;

namespace ProjectBuilder.ViewModels
{
    public class ConfigurationMessage
    {
        public bool IsWaiting { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }
        public bool HasCancelButton { get; set; } = true;
        public bool HasSaveButton { get; set; } = false;
        public string Foreground { get; set; } = Constants.SUCCESSBRUSH;
        public string CancelContent { get; set; } = "Cancel";
        public string ButtonContent { get; set; }
        public bool HasNeverShowMessage { get; set; } = false;
        public string SenderToken { get; set; }
        public ConfigurationMessage()
        {
        }
        public ConfigurationMessage(string senderToken)
        {
            SenderToken = senderToken;
        }
        public ConfigurationMessage SaveConfiguration(bool isAutoSave)
        {
            HasNeverShowMessage = false;
            Icon = "SaveContent";
            HasCancelButton = true;
            HasNeverShowMessage = false;
            Foreground = Constants.SUCCESSBRUSH;
            if (isAutoSave)
            {
                IsWaiting = true;
                Message = "Saving your changes please wait.";
                HasSaveButton = false;
                Title = "Saving Changes";
            }
            else
            {
                IsWaiting = true;
                ButtonContent = "Save";
                HasSaveButton= true;
                Title = "Unsaved changes";
                Message = "There are unsaved changes. would you like to save them?";
                IsWaiting = false;
            }
            return this;
        }
        public ConfigurationMessage ErrorConfiguration(string error)
        {
            Foreground = Constants.ERRORBRUSH;
            Icon = "Error";
            Title = "Error";
            Message = error;
            IsWaiting = false;
            CancelContent = "Close";
            HasCancelButton = true;
            HasNeverShowMessage = false;
            return this;
        }
        public ConfigurationMessage SuccessConfiguration(string successMessagre)
        {
            Foreground = Constants.SUCCESSBRUSH;
            Icon = "CheckboxMarkedCircle";
            Title = "Success";
            Message = successMessagre;
            IsWaiting = false;
            CancelContent = "Close";
            HasCancelButton = true;
            HasNeverShowMessage = false;
            return this;
        }
        public ConfigurationMessage DeleteConfiguration()
        {
            Foreground = Constants.ERRORBRUSH;
            Icon = "Delete";
            Title = "Delete Recod";
            Message = "Do you want to delete the selected Record?";
            IsWaiting =false;
            CancelContent = "Cancel";
            HasSaveButton = true;
            HasNeverShowMessage = true;
            ButtonContent = "Delete";
            HasCancelButton = true;
            return this;
        }
        public ConfigurationMessage RunScenarioConfiguration(string scenarioName)
        {
            HasCancelButton = false;
            HasSaveButton = false;
            HasNeverShowMessage = false;
            Icon = "Console";
            IsWaiting = true;
            Title = "Running Scenario";
            Message = $"Please wait while the scenario: \"{scenarioName}\" is running.";
            Foreground = Constants.SECONDARYBRUSH;
            return this;
        }
    }  
}

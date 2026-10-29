using Volo.Abp.Emailing;
using Volo.Abp.Settings;

namespace ProjectBuilder.Settings;

public class ProjectBuilderSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ProjectBuilderSettings.MySetting1));

        // ABP marks the SMTP password as encrypted, so it is run through
        // ISettingEncryptionService.Decrypt on read. That only makes sense for values written by
        // the Setting Management UI. This project supplies SMTP settings from the "Settings"
        // section of appsettings.json in plain text, so decryption must be turned off or the
        // password comes back as garbage and authentication fails.
        var smtpPassword = context.GetOrNull(EmailSettingNames.Smtp.Password);
        if (smtpPassword != null)
        {
            smtpPassword.IsEncrypted = false;
        }
    }
}

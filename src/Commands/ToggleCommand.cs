namespace CopyNice
{
    [Command(PackageIds.Toggle)]
    internal sealed class ToggleCommand : BaseCommand<ToggleCommand>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            Command.Checked = GeneralOptions.Instance.Enabled;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            GeneralOptions settings = await GeneralOptions.GetLiveInstanceAsync();
            settings.Enabled = !settings.Enabled;
            await settings.SaveAsync();
        }
    }
}

using Spectre.Console;
using BadBuilder.Models;
using BadBuilder.Helpers;

namespace BadBuilder
{
    internal partial class Program
    {
        static string PromptDiskSelection(List<DiskInfo> disks)
        {
            var choices = new List<string>();
            foreach (var disk in disks)
                choices.Add($"{disk.MountPoint} ({disk.SizeFormatted}) - {disk.Type}");

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a disk to format:")
                    .HighlightStyle(GreenStyle)
                    .AddChoices(choices)
            );
        }

        static bool PromptFormatConfirmation(string selectedDisk)
        {
            string diskName = selectedDisk.Contains(" (") 
                ? selectedDisk.Substring(0, selectedDisk.IndexOf(" ("))
                : selectedDisk;

            return AnsiConsole.Prompt(
                new TextPrompt<bool>($"[#FF7200 bold]WARNING: [/]Are you sure you would like to format [bold]{diskName}[/]? All data on this drive will be lost.")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .ChoicesStyle(GreenStyle)
                    .DefaultValueStyle(OrangeStyle)
                    .WithConverter(choice => choice ? "y" : "n")
            );
        }

        static bool FormatDisk(DiskInfo disk)
        {
            bool ret = true;
            string output = string.Empty;

            AnsiConsole.Status().SpinnerStyle(LightOrangeStyle).Start($"[#76B900]Formatting disk[/] {disk.MountPoint} ({disk.SizeFormatted}) - {disk.Type}", async ctx =>
            {
                output = DiskHelper.FormatDisk(disk);
                if (output != string.Empty) ret = false;
            });

            if (!ret)
            {
                AnsiConsole.Clear();
                ShowWelcomeMessage();
                Console.Write("\n" + output + "\n");
            }

            return ret;
        }
    }
}

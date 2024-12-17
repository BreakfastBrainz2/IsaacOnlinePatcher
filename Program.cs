using Reloaded.Memory.Sigscan;

namespace IsaacOnlinePatcher;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("ERROR: No arguments provided! Path to isaac-ng.exe must be provided. (You can drag isaac-ng.exe onto the program)");
            Console.ReadLine();
            return;
        }

        string pegglePath = args[0];
        if (!File.Exists(pegglePath) ||
            !Path.GetFileName(pegglePath).StartsWith("isaac-ng", StringComparison.InvariantCultureIgnoreCase) ||
            !Path.GetExtension(pegglePath).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
        {
            Console.WriteLine($"ERROR: {pegglePath} is not the isaac executable.");
            Console.ReadLine();
            return;
        }

        var exeCopy = File.ReadAllBytes(pegglePath);
        var scanner = new Scanner(exeCopy);

        // Sig for IsUsingMods() function
        var offset1Result = scanner.FindPattern("55 8B EC 6A FF 68 ?? ?? ?? ?? 64 A1 00 00 00 00 50 81 EC 84 00 00 00 56 A1 ?? ?? ?? ?? 33 C5 50 8D 45 F4 64 A3 00 00 00 00 8B 0D");
        if(offset1Result.Found)
        {
            // NOP'ing some jmps inside the function
            exeCopy[offset1Result.Offset + 56] = 0x90;
            exeCopy[offset1Result.Offset + 57] = 0x90;
            exeCopy[offset1Result.Offset + 61] = 0x90;
            exeCopy[offset1Result.Offset + 62] = 0x90;

            string backupFileName = Path.GetDirectoryName(pegglePath) + "\\isaac-ng-backup.exe";
            if (File.Exists(backupFileName))
            {
                File.Delete(backupFileName);
            }
            File.Move(pegglePath, Path.GetDirectoryName(pegglePath) + "\\isaac-ng-backup.exe");

            string newExeFilename = Path.GetDirectoryName(pegglePath) + "\\isaac-ng.exe";
            File.WriteAllBytes(newExeFilename, exeCopy);

            Console.WriteLine("Finished patching executable.");
            Console.WriteLine($"Patched executable location: {newExeFilename}");
            Console.WriteLine("Press enter to exit.");

            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Could not find signature for IsUsingMods()!");
            Console.ReadLine();
            return;
        }
    }
}

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
//using static System.Net.WebRequestMethods;

class Program
{
    static void Main()
    {
        bool mod = modifyAssemblies();

        if (mod)
        {
            return;
        }

        PrintAsciiArt();
        PrintColoredText();

        try
        {
            string gamePath = AskForGamePath();
            if (!string.IsNullOrEmpty(gamePath))
            {
                ModifyAssembly(gamePath);
            }
            else
            {
                Console.WriteLine("Invalid game path. Exiting...");
            }
        }
        catch (Exception ex)
        {
            LogException(ex, GetExecutableDirectory());
        }
    }

    static void PrintAsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Red; // Set color to red
        Console.WriteLine(
            @"
db    db d888888b d888888b  .o88b.        d8888b.  .d8b.  d888888b  .o88b. db   db d88888b d8888b. 
88    88   `88'   `~~88~~' d8P  Y8        88  `8D d8' `8b `~~88~~' d8P  Y8 88   88 88'     88  `8D 
88    88    88       88    8P             88oodD' 88ooo88    88    8P      88ooo88 88ooooo 88oobY' 
88    88    88       88    8b      C8888D 88~~~   88~~~88    88    8b      88~~~88 88~~~~~ 88`8b   
88b  d88   .88.      88    Y8b  d8        88      88   88    88    Y8b  d8 88   88 88.     88 `88. 
~Y8888P' Y888888P    YP     `Y88P'        88      YP   YP    YP     `Y88P' YP   YP Y88888P 88   YD"
        );
        Console.ResetColor(); // Reset color to default
    }

    static void PrintColoredText()
    {
        Console.ForegroundColor = ConsoleColor.Cyan; // Set color to cyan
        Console.Write("                                                                         v1.0.1");
        Console.ResetColor(); // Reset color to default
        Console.Write(" | by ");
        Console.ForegroundColor = ConsoleColor.Yellow; // Set color to yellow
        Console.WriteLine("Official-Husko");
        Console.ResetColor(); // Reset color to default
        Console.WriteLine();
    }

    static string GetParentDir(string path)
    {
        try
        {
            string parentDir = Directory.GetParent(path).FullName;
            return parentDir;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    static string[] SearchDirectories(string parentDir, string search)
    {
        string[] directories = Directory.GetDirectories(parentDir, search, SearchOption.AllDirectories);
        return directories;
    }

    static string[] getGamePaths()
    {
        string currentDir = Directory.GetCurrentDirectory();

        string parentDir = GetParentDir(currentDir);


        string[] directories = SearchDirectories(parentDir, "UiTC_Data");
        int trys = 0;

        while (directories.Length <= 0 && trys < 7)
        {
            parentDir = GetParentDir(currentDir);
            currentDir = parentDir;
            directories = SearchDirectories(parentDir, "UiTC_Data");
            trys++;
        }
        return directories;
    }

    static bool modifyAssemblies()
    {
        string[] directories = getGamePaths();
        bool modify = false;
        for (int i = 0; i < directories.Length; i++)
        {
            modify = true;
            try
            {
                string gamePath = directories[i];
                if (!string.IsNullOrEmpty(gamePath))
                {
                    ModifyAssembly(gamePath, true);
                }
                else
                {
                    Console.WriteLine("Invalid game path. Exiting...");
                }
            }
            catch (Exception ex)
            {
                LogException(ex, GetExecutableDirectory());
            }
        }
        return modify;
    }

    static string AskForGamePath(bool dontAsk = false)
    {
        if (dontAsk)
        {
            return null;
        }

        Console.WriteLine("Make sure to point it to your data folder! (e.g. C:\\Users\\paw_beans\\Downloads\\UiTC_v33b_EX_Win_64_Bit\\UiTC_v33b_EX_Win_64_Bit_Data)");
        Console.Write("Enter the path to the game data folder: ");
        
        string gamePath = Console.ReadLine();

        // Remove any trailing slashes or backslashes
        gamePath = gamePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        Console.WriteLine("Your gamepath: " + gamePath);
        if (Directory.Exists(gamePath))
        {
            return gamePath;
        }
        else
        {
            Console.WriteLine("Invalid game path. Please make sure the directory exists.");
            return null;
        }
    }

    static void ModifyAssembly(string gamePath, bool dontAsk = false)
    {
        // Construct paths for the DLL and its backup
        string dllPath = Path.Combine(gamePath, @"Managed\Assembly-CSharp.dll");

        if (!File.Exists(dllPath))
        {
            Console.WriteLine(dllPath + " not exists!");
            return;
        }

        string backupFilePath = Path.Combine(gamePath, @"Managed\Assembly-CSharp-Backup.dll");

        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }

        // Load the assembly
        ModuleDefMD module = ModuleDefMD.Load(dllPath);

        // Find the type and the fields to modify
        TypeDef globalObjectsType = module.Types.SingleOrDefault(t => t.Name == "GlobalObjects_UW");

        if (globalObjectsType != null)
        {
            Dictionary<string, string> fieldsWithTypes = new Dictionary<string, string>
            {
                    {"showDevDebug", "bool"},
                    {"isDevBuild", "bool"},
                    {"isDebugMode", "bool"},
                    {"showMovePoints", "bool"},
                    {"ignoreChoiceReqs", "bool"},
                    {"overpowerStars", "bool"},
                    {"accessAllClothes", "bool"},
                    {"maxLuck", "bool"},
                    {"pregnancyInfoCheat", "bool"},
                    {"cheatsOn", "bool"},
                    {"isVersionCheats", "bool"},
                    {"isVersionExtended", "bool"},
                    {"enc", "int"},
                    {"showInternalDebug", "bool"},
                    {"isFurries", "bool"},
                    {"isSuperDebugging", "bool"},
                    {"allowCumShake", "bool"},
                    {"stopTimeFlow", "bool"},
                    {"dontCleanSperm", "bool"},
                    {"isRandomNPCSize", "bool"},
                    {"disableScreenFade", "bool"},
                    {"eyesOverHairs", "bool"},
                    {"pregnantBelly", "bool"},
                    {"disablePregnancy", "bool"},
                    {"hidePregnantBelly", "bool"},
            };

            Dictionary<string, object> fieldsWithValues = new Dictionary<string, object>
            {
            };

            Dictionary<string, FieldDef> fieldsWithDefs = new Dictionary<string, FieldDef>
            {
            };

            Dictionary<FieldDef, string> fieldsWithDefs2 = new Dictionary<FieldDef, string>
            {
            };

            Dictionary<string, string> fieldsWithTypesNew = new Dictionary<string, string>
            {
            };

            // Find the static constructor (.cctor) method
            MethodDef cctorMethod = globalObjectsType.Methods.SingleOrDefault(m => m.Name == ".cctor");

            if (cctorMethod == null)
            {
                Console.WriteLine("Static constructor (.cctor) not found.");
                return;
            }

            FieldDef encField = globalObjectsType.Fields.SingleOrDefault(f => f.Name == "enc");

            foreach (var kvp in fieldsWithTypes)
            {
                string fieldName = kvp.Key;
                string fieldType = kvp.Value;
                //Console.WriteLine(fieldName);
                //Console.WriteLine(fieldType);

                object fieldValue = AskForInput(fieldName, fieldType, dontAsk);

                FieldDef field = globalObjectsType.Fields.SingleOrDefault(f => f.Name == fieldName);

                if (field != null)
                {
                    fieldsWithValues.Add(fieldName, fieldValue);
                    fieldsWithDefs.Add(fieldName, field);
                    fieldsWithTypesNew.Add(fieldName, fieldType);
                    fieldsWithDefs2.Add(field, fieldName);
                } else {
                    Console.WriteLine($"Field '{fieldName}' not found. The Game Dev removed it.");
                }
            }

            List<string> youtubeLinks = new List<string>
            {
                    "https://www.youtube.com/watch?v=o9l4EiYFZjg",
                    "https://www.youtube.com/watch?v=egYUfUo3__k"
            };

            ModifyMethod(globalObjectsType, ".cctor", fieldsWithTypesNew, fieldsWithValues, fieldsWithDefs, fieldsWithDefs2, youtubeLinks);
            ModifyMethod(globalObjectsType, "CheckVersionEXCHBA", fieldsWithTypesNew, fieldsWithValues, fieldsWithDefs, fieldsWithDefs2, youtubeLinks);
            File.Move(dllPath, backupFilePath);
            module.Write(dllPath);
        }
        else
        {
            Console.WriteLine("Type not found. Make sure the structure matches.");
        }
    }

    static void ModifyMethod(TypeDef globalObjectsType, string methodName, Dictionary<string, string> fieldsWithTypes,  Dictionary<string, object> fieldsWithValues, Dictionary<string, FieldDef> fieldsWithDefs, Dictionary<FieldDef, string> fieldsWithDefs2, List<string> youtubeLinks)
    {
        MethodDef cctorMethod = globalObjectsType.Methods.SingleOrDefault(m => m.Name == methodName);
        if (cctorMethod != null)
        {
            for (int i = 0; i < cctorMethod.Body.Instructions.Count; i++)
            {
                Instruction instr = cctorMethod.Body.Instructions[i];
                if (instr.OpCode != OpCodes.Stsfld)
                {
                    continue;
                }

                if (i == 0)
                {
                    continue;
                }

                FieldDef field = (FieldDef)instr.Operand;
                
                if (!fieldsWithDefs2.ContainsKey(field))
                {
                    continue;
                }

                string fieldName = fieldsWithDefs2[field];
                
                if (fieldName == "enc")
                {
                    continue;
                }

                object fieldValue = fieldsWithValues[fieldName];
                //Console.WriteLine($"{fieldName} {fieldValue}");
                Instruction next = cctorMethod.Body.Instructions[i - 1];
                bool is_i40_or_i41 = next.OpCode == OpCodes.Ldc_I4_0 || next.OpCode == OpCodes.Ldc_I4_1;
                bool is_i4 = next.OpCode == OpCodes.Ldc_I4;

                if (is_i40_or_i41)
                {
                    bool boolValue = fieldValue is bool;
                    if (boolValue)
                    {
                        boolValue = (bool)fieldValue;
                        next.OpCode = boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                        cctorMethod.Body.Instructions[i - 1] = next;
                    }
                    continue;
                } else if (is_i4)
                {
                    if (field.Constant is { } constantValue)
                    {
                        next.Operand = fieldValue;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid constant value for '{fieldName}'. Defaulting to false.");
                        next.Operand = 0;
                    }
                    cctorMethod.Body.Instructions[i - 1] = next;
                    continue;
                }

                if (fieldName == "isVersionExtended" && fieldValue is bool isVersionExtended && isVersionExtended)
                {
                    FieldDef encField = globalObjectsType.Fields.SingleOrDefault(f => f.Name == "enc");
                    if (encField != null)
                    {
                        int encValue = isVersionExtended ? 76 : 77;

                        for (int j = 0; j < cctorMethod.Body.Instructions.Count; j++)
                        {
                            Instruction currentInstr = cctorMethod.Body.Instructions[j];
                            if (currentInstr.OpCode == OpCodes.Ldc_I4 && j + 1 < cctorMethod.Body.Instructions.Count &&
                                cctorMethod.Body.Instructions[j + 1].OpCode == OpCodes.Stsfld &&
                                cctorMethod.Body.Instructions[j + 1].Operand == encField)
                            {
                                currentInstr.Operand = encValue;
                            }
                            cctorMethod.Body.Instructions[j] = currentInstr;
                        }
                    }
                }

                if (fieldName == "isFurries" && fieldValue is bool isFurries && isFurries)
                {
                    OpenRandomYouTubeLink(youtubeLinks);
                }
            }

            for (int i = 0; i < cctorMethod.Body.Instructions.Count; i++)
            {
                globalObjectsType.Methods.SingleOrDefault(m => m.Name == methodName).Body.Instructions[i] = cctorMethod.Body.Instructions[i];
            }
        }
        else
        {
            Console.WriteLine("Static constructor not found.");
        }


        return;
    }

static object AskForInput(string fieldName, string fieldType, bool dontAsk = false)
{
    if (fieldName == "enc")
    {
        return 0;
    }

    try
    {
        if (!dontAsk)
        {
            Console.Write($"Enable {fieldName}? (y/n): ");
        }

        if (fieldType == "bool")
        {
            if (dontAsk)
            {
                return true;
            }

            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key == ConsoleKey.Y)
            {
                return true;
            }
            else if (key.Key == ConsoleKey.N)
            {
                return false;
            }
            else
            {
                Console.WriteLine("Invalid input. Defaulting to false.");
                return false;
            }
        }
        else if (fieldType == "int")
        {
            int number = 0;

            if (dontAsk)
            {
                return 0;
            }

            try
            {
                number = int.Parse(Console.ReadLine());
                Console.WriteLine($"the number is: {number}");
            }
            catch (FormatException)
            {

                Console.WriteLine("Invalid input. Defaulting to 0.");
                number = 0;
            }
            return number;
        }
        else
        {
            Console.WriteLine($"Unsupported field type: {fieldType}");
            return null;
        }
    }
    catch (Exception ex)
    {
        LogException(ex, GetExecutableDirectory());
        return null;
    }
}

static void OpenRandomYouTubeLink(List<string> links)
{
    try
    {
        if (links.Count > 0)
        {
            Random random = new Random();
            int index = random.Next(links.Count);
            string randomLink = links[index];

            Process.Start(new ProcessStartInfo(randomLink) { UseShellExecute = true });
        }
        else
        {
            Console.WriteLine("No YouTube links available. :(");
        }
    }
    catch (Exception ex)
    {
        LogException(ex, GetExecutableDirectory());
    }
}

static void LogException(Exception ex, string directory)
{
    try
    {
        string logFilePath = Path.Combine(directory, "runtime.log");
        File.WriteAllText(logFilePath, $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
        Console.WriteLine($"An error occurred. Details logged in {logFilePath}. Please Report this!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    catch
    {
        Console.WriteLine("An error occurred, and failed to log details. Please Report this!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}

static string GetExecutableDirectory()
{
    return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
}
}

/*
This code is probably the biggest dog shit out there but i don't care. it is what it is
*/
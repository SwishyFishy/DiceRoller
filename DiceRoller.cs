using System.Text.RegularExpressions;

namespace DiceRoller_v2
{
    public class DiceRoller
    {
        // Properties
        private UserObjectsList ObjectsList { get; set; }
        private UserObjectsFile ObjectsFile { get; set; }
        private PointerNode SelectedObject { get; set; }
        private bool Continue { get; set; }
        private bool SaveOnExit { get; set; }

        // Constructor
        public DiceRoller()
        {
            // Stage runtime
            Start();
            while (true)
            {
                Continue = true;
                SaveOnExit = true;

                Run();
                if (Exit())
                {
                    Output("Exiting");
                    break;
                }
            }
        }

        // Methods
        private void Start()
        {
            // Initialize properties
            ObjectsFile = new UserObjectsFile("newfile");
            SelectedObject = new PointerNode();

            // Load initial objects
            ObjectsList = null;
            bool run = true;
            while (run)
            {
                ObjectsFile.SetFileName(Query("Enter file name to load saved file, or no input to proceed without loading >>> ", ObjectsFile.AcceptableNames.ToString(), "Invalid file name"));

                if (ObjectsFile.FileName == "" || File.Exists(ObjectsFile.FilePath))
                {
                    ObjectsList = ObjectsFile.Load();
                    run = false;
                }
                else
                    Output("File does not exist", ConsoleColor.DarkRed, ConsoleColor.Black);
            }

            // User greeting
            Output("DiceRoller ver2.0 (c) 2024 Jonah Galloway-Fenwick. Enter 'help' for help menu. It is highly recommended that you run this application in fullscreen.");
        }

        private void Run()
        {
            while (Continue)
            {
                // Generate comamnd prompt "filename [SelectedObject.Name] >>> "
                string query = (ObjectsFile.FileName != "" ? ObjectsFile.FileName : "[No File Loaded]") + "[";
                if (SelectedObject.Object != null)
                {
                    if (SelectedObject.Object.Die != null)
                        query += SelectedObject.Object.Die.Name;
                    if (SelectedObject.Object.Collection != null)
                        query += SelectedObject.Object.Collection.Name;
                }
                else
                    query += "No Object Selected";

                query += "] >>> ";

                // Parse user command
                Parse(Query(query, @"^[|\w\-\+ ]+$")); 
            }
        }

        private bool Exit()
        {
            // Attempt to save
            if (SaveOnExit && ObjectsFile.FileName != "" && SaveFile(new string[] { "f_save" }) != "")
                return false;

            return true;
        }

        private void Parse(string command)
        {
            string[] cmds = command.Split('|');
            string[] cmd;
            string msg;

            // Loop through sequenced commands
            for (int index = 0; index < cmds.Length; index++)
            {
                cmd = cmds[index].Split(' ');

                // Remove excess whitespace from cmd
                List<string> args = new List<string>();
                for (int i = 0; i < cmd.Length; i++)
                {
                    if (!string.IsNullOrEmpty(cmd[i]))
                        args.Add(cmd[i]);
                }
                cmd = args.ToArray();

                // Evaluate command and call requisite handling method
                // If the returned string has length >0, an error was encountered; print the returned error message to console
                // If the returned string has length 0, the method executed successfully
                switch (cmd[0])
                {
                    case "add":
                        msg = Add(cmd);
                        break;
                    case "create":
                        msg = Create(cmd);
                        break;
                    case "delete":
                        msg = Delete(cmd);
                        break;
                    case "exit":
                        msg = Exit(cmd);
                        break;
                    case "f_delete":
                        msg = DeleteFile(cmd);
                        break;
                    case "f_list":
                        msg = ListFiles(cmd);
                        break;
                    case "f_load":
                        msg = LoadFile(cmd);
                        break;
                    case "f_rename":
                        msg = RenameFile(cmd);
                        break;
                    case "f_save":
                        msg = SaveFile(cmd);
                        break;
                    case "help":
                        msg = Help(cmd);
                        break;
                    case "list":
                        msg = List(cmd);
                        break;
                    case "modifier":
                        msg = Modifier(cmd);
                        break;
                    case "remove":
                        msg = Remove(cmd);
                        break;
                    case "rename":
                        msg = Rename(cmd);
                        break;
                    case "roll":
                        msg = Roll(cmd);
                        break;
                    case "select":
                        msg = Select(cmd);
                        break;
                    default:
                        msg = "Command not recognized";
                        break;
                }

                // Error catching and sequence abortion
                if (msg != "")
                {
                    Output("Error - " + msg, ConsoleColor.DarkRed, ConsoleColor.Black);
                    if (index < cmds.Length - 1)
                    {
                        Output("Remaining command sequence aborted - '" + cmds[index] + "' execution failed");
                        index = cmds.Length;
                    }
                }
            }
        }

        private void Output(string output, ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            Console.Write(output);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private string Query(string query, string acceptable = "^.*$", string errormsg = "Invalid input")
        {
            string entry = "";
            Regex AcceptableEntry = new Regex(acceptable);

            // Request and read user input
            Console.Write(query);
            entry = Console.ReadLine(); 
            
            // Verify user input is acceptable. Loop until acceptable input given
            while (!AcceptableEntry.IsMatch(entry))
            {
                Output(errormsg, ConsoleColor.DarkRed, ConsoleColor.Black);
                Console.Write(query);
                entry = Console.ReadLine();
            }

            return entry;
        }



        /////////////////////////////////////////////////////////////////////////////////////////
        // User Command Methods ////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////
        // Each method handles 1 user command
        // If a method encounters an error while executing, it returns an error message
        // Upon successful execution, the method returns an empty string

        // Error Messages:
        // "Invalid parameter count"                                                    -> cmd.Length is wrong
        // "Unrecognized parameter"                                                     -> cmd[i] did not find a specific string it was looking for
        // "Wrong parameter type"                                                       -> A parameter failed to parse to a char or int primitive
        // "Parameter object does not exist (in this context)"                          -> An object passed by the user does not exist (or might, but not here)
        // "Name already in use"                                                        -> User attempts to use one name for two distinct objects
        // "DiceCollections cannot contain themselves"                                  -> User attmepts to store a DiceCollection in itself
        // "Can't remove last face of a die"                                            -> User attempts to remove a face from a single-face die
        // "No object selected"                                                         -> SelectedObject(.Object) is null when it needs an object

        // "Aborted by user"                                                            -> User declined query to continue execution after failed action
        // "File does not exist"                                                        -> A file the user entered does not exist
        // "Invalid file name"                                                          -> User attempts to supply an invalid file name
        // "Mismatched parentheses"                                                     -> User supplied parentheses are missing open/close or are nonsensical



        // Active DiceRoller
        public string Create(string[] cmd)
        {
            // Command Format:
            // create <name> <>|<'+'|'-'int>|<int>|<int, int, int, ...> <>|<int>|<int, int, int, ...>
            // Creates a collection with name <name>. +/- adds a modifier. Adding other integers make it a die with (one int) that many faces, or (more than one int) those face values

            // Error Handling
            if (cmd.Length < 2)
                return "Invalid parameter count";

            /////////////
            // Collection
            // create <name>
            if (cmd.Length == 2)
            {
                if (ObjectsList.Add(new DiceCollection(cmd[1], 0)) == 0)
                {
                    Select(new string[] { "select", cmd[1] });
                    return "";
                }

                return "Name already in use";
            }
            // create <name> <+/-int>
            else if (cmd.Length == 3 && (cmd[2][0] == '-' || cmd[2][0] == '+'))
            {
                try
                {
                    if (ObjectsList.Add(new DiceCollection(cmd[1], int.Parse(cmd[2]))) == 0)
                    {
                        Select(new string[] { "select", cmd[1] });
                        return "";
                    }

                    return "Name already in use";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            ///////////////////
            // Die with n faces
            // create <name> <int>
            else if (cmd.Length == 3)
            {
                try
                {
                    if (ObjectsList.Add(new Die(cmd[1], 0, int.Parse(cmd[2]))) == 0)
                    {
                        Select(new string[] { "select", cmd[1] });
                        return "";
                    }

                    return "Name already in use";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            // create <name> <+/-int> <int>
            else if (cmd.Length == 4 && (cmd[2][0] == '-' || cmd[2][0] == '+'))
            {
                try
                {
                    if (ObjectsList.Add(new Die(cmd[1], int.Parse(cmd[2]), int.Parse(cmd[3]))) == 0)
                    {
                        Select(new string[] { "select", cmd[1] });
                        return "";
                    }

                    return "Name already in use";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            ///////////////////////////////////////
            // Die with faces of value a, b, c, ...
            // create <name> <int> <int> ...
            else if (cmd.Length >= 4 && !(cmd[2][0] == '-' || cmd[2][0] == '+'))
            {
                try
                {
                    int[] faces = new int[cmd.Length - 2];
                    for (int i = 0; i < cmd.Length - 2; i++)
                    {
                        faces[i] = int.Parse(cmd[i + 2]);
                    }

                    if (ObjectsList.Add(new Die(cmd[1], 0, faces)) == 0)
                    {
                        Select(new string[] { "select", cmd[1] });
                        return "";
                    }

                    return "Name already in use";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            // create <name> <+/-int> <int> <int> ...
            else if (cmd.Length >= 5 && (cmd[2][0] == '-' || cmd[2][0] == '+'))
            {
                try
                {
                    int[] faces = new int[cmd.Length - 3];
                    for (int i = 0; i < cmd.Length - 3; i++)
                    {
                        faces[i] = int.Parse(cmd[i + 3]);
                    }

                    if (ObjectsList.Add(new Die(cmd[1], int.Parse(cmd[2]), faces)) == 0)
                    {
                        Select(new string[] { "select", cmd[1] });
                        return "";
                    }

                    return "Name already in use";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }

            return "Unrecognized parameter";
        }

        public string Delete(string[] cmd)
        {
            // Command Format:
            // delete
            // Deletes SelectedObject

            // Error Handling
            if (cmd.Length != 1)
                return "Invalid parameter count";

            if (ObjectsList.Contains(SelectedObject.Object.Die != null ? SelectedObject.Object.Die.Name : SelectedObject.Object.Collection.Name) == null)
            {
                return "Paramter object does not exist";
            }

            // Delete
            ObjectsList.Remove(SelectedObject.Object.Die != null ? SelectedObject.Object.Die : SelectedObject.Object.Collection);
            SelectedObject = new PointerNode();
            return "";

        }

        public string Rename(string[] cmd)
        {
            // Command Format:
            // rename <name>
            // Renames SelectedObject to name <name>

            // Error Handling
            if (cmd.Length != 2)
                return "Invalid parameter count";

            // Duplicate name handling
            if (ObjectsList.Contains(cmd[1]) != null)
                return "Name already in use";

            // Rename
            if (SelectedObject.Object.Die != null)
                SelectedObject.Object.Die.Rename(cmd[1]);
            else if (SelectedObject.Object.Collection != null)
                SelectedObject.Object.Collection.Rename(cmd[1]);

            return "";
        }

        public string Modifier(string[] cmd)
        {
            // Command Format:
            // modifier <'+'|'-'int>
            // Changes modifier of SelectedObject

            // Error Handling
            if (cmd.Length != 2)
                return "Invalid parameter count";

            if (!(cmd[1][0] == '+' || cmd[1][0] == '-'))
                return "Unrecognized parameter";

            try
            {
                if (SelectedObject.Object.Die != null)
                    SelectedObject.Object.Die.SetModifier(int.Parse(cmd[1]));
                if (SelectedObject.Object.Collection != null)
                    SelectedObject.Object.Collection.SetModifier(int.Parse(cmd[1]));
            }
            catch (Exception)
            {
                return "Wrong parameter type";
            }

            return "";
        }

        public string Add(string[] cmd)
        {
            // Command Format:
            // add <name|value>
            // Adds object of name <name> to SelectedObject if it is a DiceCollection, or face with value <value> to SelectedObject if it is a Die

            // Error Handling
            if (cmd.Length != 2)
                return "Invalid parameter count";

            // Add face to Die
            if (SelectedObject.Object.Die != null)
            {
                try
                {
                    SelectedObject.Object.Die.AddFace(int.Parse(cmd[1]));
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            // Add object to collection
            else if (SelectedObject.Object.Collection != null)
            {
                // Add to DiceCollection
                int code = SelectedObject.Object.Collection.Add(ObjectsList.Contains(cmd[1]));
                if (code == -1)
                    return "Parameter object does not exist";
                else if (code == -2 || code == -3)
                    return "DiceCollections cannot contain themselves";
                else
                    return "";
            }

            return "";
        }

        public string Remove(string[] cmd)
        {
            // Command Format:
            // remove <name|value>
            // Removes object of name <name> from SelectedObject if it is a DiceCollection, or face with value <value> from SelectedObject if it is a Die

            // Error Handling
            if (cmd.Length != 2)
                return "Invalid parameter count";

            int attempt;

            // Remove face from Die
            if (SelectedObject.Object.Die != null)
            {
                try
                {
                    attempt = int.Parse(cmd[1]);
                    attempt = SelectedObject.Object.Die.RemoveFace(attempt);
                    if (attempt == -1)
                        return "Can't remove last face of a die";
                    if (attempt == -2)
                        return "Parameter object does not exist";
                }
                catch (Exception)
                {
                    return "Wrong parameter type";
                }
            }
            // Remove object from collection
            else if (SelectedObject.Object.Collection != null)
            {
                // Remove from DiceCollection
                bool code = SelectedObject.Object.Collection.Remove(cmd[1]);
                if (code == false)
                    return "Parameter object does not exist in this context";
                else
                    return "";
            }

            return "";
        }

        public string Select(string[] cmd)
        {
            // Command Format:
            // select <>|<name>
            // Sets SelectedObject to object of name <name>

            // Error Handling
            if (cmd.Length > 2)
                return "Invalid parameter count";

            if (cmd.Length == 1)
            {
                SelectedObject = new PointerNode();
                return "";
            }

            // Select the given object
            PointerNode found = ObjectsList.Contains(cmd[1]);

            if (found != null)
            {
                SelectedObject = found;
                return "";
            }
            else
            {
                return "Parameter object does not exist";
            }
        }

        public string List(string[] cmd)
        {
            // Command Format:
            // list
            // Lists all objects if SelectedObject is null. If not, lists all contents of SelectedObject if it is a DiceCollection, or all faces if it is a Die

            // Error Handling
            if (cmd.Length != 1)
                return "Invalid parameter count";

            // List
            if (SelectedObject.Object == null)
                Output(ObjectsList.ToString());
            else if (SelectedObject.Object.Die != null)
                Output(SelectedObject.Object.Die.ToString());
            else if (SelectedObject.Object.Collection != null)
                Output(SelectedObject.Object.Collection.ToString());
            return "";
            
        }

        public string Roll(string[] cmd)
        {
            // Command Format:
            // roll <>|<'quick'>
            // Displays detailed results of rolling SelectedObject, or just final tally if <'quick'>

            // Error handling
            if (cmd.Length > 2)
                return "Invalid parameter count";

            if (cmd.Length == 2 && cmd[1] != "quick")
                return "Unrecognized parameter";

            if (SelectedObject == null || SelectedObject.Object == null)
                return "No object selected";

            // Roll
            // Roll Die
            if (SelectedObject.Object.Die != null)
                Output(SelectedObject.Object.Die.Roll().ToString());
            // Roll DiceCollection
            else if (SelectedObject.Object.Collection != null && cmd.Length == 1)
            {
                (string, int, bool)[] results = SelectedObject.Object.Collection.RollPrecisely();
                for (int i = 0; i < results.Length; i++)
                {
                    // Subtotals
                    // Ternary puts darker colour on final subtotal (proper total)
                    if (results[i].Item3 == true)
                        Output(results[i].Item1 + ": " + results[i].Item2.ToString(), (i == results.Length - 1 ? ConsoleColor.DarkCyan : ConsoleColor.Cyan), ConsoleColor.Black);
                    // Non-Subtotals
                    else
                        Output("\t" + results[i].Item1 + ": " + results[i].Item2.ToString());
                }
            }
            // Roll DiceCollection fast
            else if (SelectedObject.Object.Collection != null && cmd.Length == 2)
            {
                Output(SelectedObject.Object.Collection.Roll().ToString());
            }

            return "";
        }

        // File Management
        public string LoadFile(string[] cmd)
        {
            // Command Format:
            // loadfile <name>
            // Loads file of name <name>, or creates t if it does not exist

            // Error Handling
            if (cmd.Length != 2)
                return "Invalid parameter count";

            // Save current file, then load new file
            if (ObjectsFile.FileName != "" && SaveFile(new string[] { "f_save" }) != "")
            {
                // Handle failed save
                if (Query("[[WARNING]] File save failed, load new file anyway (y/n)? >>> ", "^[y|n]$") == "n")
                {
                    return "Aborted by user";
                }
            }

            UserObjectsFile newFile = new UserObjectsFile(cmd[1]);
            ObjectsFile = newFile;
            ObjectsList = newFile.Load();
            if (ObjectsList == null)
                ObjectsList = new UserObjectsList();
            else
                SelectedObject = new PointerNode();

            return "";
        }

        public string SaveFile(string[] cmd)
        {
            // Command Format:
            // savefile <name>|<>
            // Saves ObjectList to file with name <name>, or current file if no <name> given

            // Error Handling
            if (cmd.Length > 2)
                return "Invalid parameter count";

            // Save
            bool saved = false;

            // Save to loaded file
            if (cmd.Length == 1)
                saved = ObjectsFile.Save(ObjectsList);
            // Save to new file
            else if (cmd.Length == 2)
            {
                if (!ObjectsFile.AcceptableNames.IsMatch(cmd[1]))
                    return "Invalid file name";

                UserObjectsFile newFile = new UserObjectsFile(cmd[1]);
                UserObjectsFile temp = ObjectsFile;
                ObjectsFile = newFile;
                if (!(saved = ObjectsFile.Save(ObjectsList)))
                    ObjectsFile = temp;
            }

            if (!saved)
                return "File save failed";

            return "";
        }

        public string DeleteFile(string[] cmd)
        {
            // Command Format:
            // deletefile <name>|<>
            // Deletes file with name <name>, or current file if no <name> given

            // Error Handling
            if (cmd.Length > 2)
                return "Invalid parameter count"; 
            
            // Delete loaded file
            if (cmd.Length == 1)
            {
                File.Delete(ObjectsFile.FilePath);
                ObjectsFile.SetFileName("");
            }
            // Delete supplied file
            else if (cmd.Length == 2)
            {
                string path = ObjectsFile.ConvertToPath(cmd[1]);
                if (!File.Exists(path))
                    return "File does not exist";

                File.Delete(path);
            }

            return "";
        }

        public string RenameFile(string[] cmd)
        {
            // Command Format:
            // renamefile <name> <>|<name>
            // Renames file with name <name>, or current file if no <name> given

            // Error Handling
            if (cmd.Length == 1 || cmd.Length > 3)
                return "Invalid parameter count";

            // Rename loaded file
            if (cmd.Length == 2)
            {
                // Check new name validity
                if (!ObjectsFile.AcceptableNames.IsMatch(cmd[1]))
                    return "Invalid file name";

                // Move contents of loaded file into new file, then delete old file
                if (File.Exists(ObjectsFile.FilePath))
                {
                    File.Move(ObjectsFile.FilePath, ObjectsFile.ConvertToPath(cmd[1]));
                    File.Delete(ObjectsFile.FilePath);
                }
                ObjectsFile.SetFileName(cmd[1]);
            }
            // Rename supplied file
            else if (cmd.Length == 3)
            {
                // Get path of file to be renamed
                string path = ObjectsFile.ConvertToPath(cmd[1]);
                if (!File.Exists(path))
                    return "File does not exist";

                // Check new name validity
                if (!ObjectsFile.AcceptableNames.IsMatch(cmd[2]))
                    return "Invalid file name";

                // Move contents of given file to new file, then delete given file
                if (File.Exists(path))
                {
                    File.Move(path, ObjectsFile.ConvertToPath(cmd[2]));
                    File.Delete(path);
                }

                // Catch case where user uses this functionality to rename loaded file
                if (cmd[1] == ObjectsFile.FileName)
                    ObjectsFile.SetFileName(cmd[2]);
            }

            return "";
        }

        public string ListFiles(string[] cmd)
        {
            // Command Format:
            // listfiles
            // Lists all saved files

            // Error Handling
            if (cmd.Length > 1)
                return "Invalid parameter count";

            // List all UserPresets files
            string[] files = Directory.GetFiles("UserPresets/");
            for (int i = 0; i < files.Length; i++)
            {
                // Split off directory name and file extension
                Output(files[i].Split('/')[1].Split('.')[0]);
            }

            return "";
        }

        // Misc

        public string Help(string[] cmd)
        {
            // Command Format:
            // help
            // Displays help menu

            // Error Handling
            if (cmd.Length != 1)
                return "Invalid parameter count";

            string helpMenu =
                "The loaded file is displayed on the command line. When you quit DiceRoller, your dice will be saved to that file.\n" +
                "The selected item is displayed on the command line in [square brackets]. Commands apply primarily to the selected item.\n" +
                "Separate each command, parameter, and sequence indicator with whitespace. Multiple sequential whitespace characters are redundant.\n" +
                "'|' is a sequence indicator. Commands separated by a sequence indicator will run in the order entered when the command line is submitted.\n" +
                "Parameters in this menu are constructed of two parts: the parameter in <carats>, and the type in (brackets).\n" +
                "The parameter type indicates the value type accepted. (strings) are sequences of characters. (ints) are integers.\n" +
                "Optional parameters are supplied in this menu in [square brackets]. These parameters may be omitted from commands.\n" +
                "Strict paramters are supplied in this menu inside 'single quotations'. These parameters take the value listed. If there are multiple possible values, they are separated by a /.\n" +
                "Repeat parameters are supplied in this menu proceeded by '...'. These parameters can be supplied in any amount no less than 1.\n" +
                "\n\n" +
                "DICE MANAGEMENT\n" +
                "add        <(string)name/(int)value>                                               -> Depending on whether the selected item is a Collection or a Die, adds the item or face value.\n" +
                "create     <(string)name> <['+'/'-'(int)modifier]>                                 -> Creates a Collection with the given name and modifier (defaults to +0).\n" +
                "           <(string)name> <['+'/'-'(int)modifier]> <(int)faces>                    -> Creates a Die with the given name, modifier (defaults to +0), and number of faces valued 1, 2, ...\n" +
                "           <(string)name> <['+'/'-'(int)modifier]> <(int)value> <(int)value> ...   -> Creates a Die with the given name, modifier (defaults to +0), and faces with the given values.\n" +
                "delete                                                                             -> Deletes the selected item.\n" +
                "list                                                                               -> Depending on whether the selected item is a Collection, a Die, or null, lists all contents, face values, or all items.\n" +
                "modifier   <'+'/'-'(int)modifier>                                                  -> Sets the selected item's modifier to the given value.\n" +
                "remove     <(string)name/(int)value>                                               -> Depending on whether the selected item is a Collection or a Die, removes the item or face value.\n" +
                "rename     <(string)name>                                                          -> Sets the selected item's name to the given name.\n" +
                "roll       <['quick']>                                                             -> Rolls the selected item. If 'quick' parameter supplied, prints only the final result.\n" +
                "select     <(string)name>                                                          -> Sets the selected item to the item with the given name.\n" +
                "\n\n" +
                "FILE MANAGEMENT\n" +
                "f_delete   <[(string)name]>                                                        -> Deletes the loaded file, or the file with the given name if supplied.\n" +
                "f_list                                                                             -> Lists all files.\n" +
                "f_load     <(string)name>                                                          -> Saves the current file if one is loaded, then loads the file with the given name.\n" +
                "f_rename   <(string)name>                                                          -> Sets the loaded file's name to the given name.\n" +
                "           <(string)oldname> <(string)newname>                                     -> Sets the name of the file with given oldname to newname.\n" +
                "f_save     <[(string)name]>                                                        -> Saves the loaded file, or saves to a file with the given name if supplied.\n" +
                "\n\n" +
                "MISCELLANEOUS\n" +
                "exit       <['nosave']>                                                            -> Exit DiceRoller. Saves automatically to loaded file unless 'nosave' parameter supplied.\n" +
                "help                                                                               -> Display this menu.\n" +
                "";

            Output(helpMenu);
            return "";
        }

        public string Exit(string[] cmd)
        {
            // Command Format:
            // exit <>|<'nosave'>
            // Saves to current file and exits, or exits without saving if <'nosave'>

            // Error Handling
            if (cmd.Length > 2)
                return "Invalid parameter count";

            if (cmd.Length == 1)
            {
                Continue = false;
                return "";
            }
            else if (cmd.Length == 2 && cmd[1] == "nosave")
            {
                Continue = false;
                SaveOnExit = false;
                return "";
            }

            return "Unrecognized parameter";
        }

    }
}

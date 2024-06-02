using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiceRoller_v2
{
    public class UserObjectsFile
    {
        // Properties
        public Regex AcceptableNames { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        // Constructor
        public UserObjectsFile(string filename)
        {
            AcceptableNames = new Regex(@"^\w*$");
            FileName = "newfile";
            SetFileName(filename);
        }

        // Methods
        public bool SetFileName(string name)
        {
            if (!AcceptableNames.IsMatch(name))
                return false;

            FileName = name;
            FilePath = ConvertToPath(FileName);
            return true;
        }

        public string ConvertToPath(string name)
        {
            return "UserPresets/" + name + ".json";
        }

        public UserObjectsList Load()
        {
            // Create UserPresets Directory if necessary
            Directory.CreateDirectory("UserPresets");

            // Handle no input
            if (FileName == "")
                return new UserObjectsList();

            // Handle nonexistent file
            if (!File.Exists(FilePath))
                return null;

            // Load file
            string jsonified = File.ReadAllText(FilePath);
            SavedUserObjectsListFormat SerializedObjectsList = JsonSerializer.Deserialize<SavedUserObjectsListFormat>(jsonified);

            return SerializedObjectsList.ConstructObjectList();
        }

        public bool Save(UserObjectsList objects)
        {
            if (FileName == "")
                return false;

            // Write objects to file
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            SavedUserObjectsListFormat SerializableObjectsList = objects.PreSerialize();

            string jsonified = JsonSerializer.Serialize(SerializableObjectsList, options);

            File.WriteAllText(FilePath, jsonified);

            return true;
        }
    }
}


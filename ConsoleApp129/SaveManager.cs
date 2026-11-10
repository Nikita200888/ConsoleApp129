using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConsoleApp129
{
    // Сделали internal вместо public
    internal class SaveData
    {
        public int Level { get; set; }
        public int HeroX { get; set; }
        public int HeroY { get; set; }
        public List<string> MapRows { get; set; }
    }

    // Сделали internal вместо public
    internal static class SaveManager
    {
        private static string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

        // Метод тоже internal
        internal static void SaveGame(int level, MapObject[,] mapArray, int heroX, int heroY)
        {
            try
            {
                SaveData data = new SaveData
                {
                    Level = level,
                    HeroX = heroX,
                    HeroY = heroY,
                    MapRows = new List<string>()
                };

                for (int i = 0; i < mapArray.GetLength(0); i++)
                {
                    string row = "";
                    for (int j = 0; j < mapArray.GetLength(1); j++)
                    {
                        MapObject obj = mapArray[i, j];
                        if (obj is Hero) row += "H";
                        else if (obj is Enemy) row += "E";
                        else if (obj is Wall) row += "W";
                        else if (obj is Tree) row += "T";
                        else row += ".";
                    }
                    data.MapRows.Add(row);
                }

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(savePath, json);

                Console.WriteLine($"\n[УСПЕХ] Игра сохранена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ОШИБКА] Не удалось сохранить: {ex.Message}");
            }
        }

        internal static SaveData LoadGame()
        {
            if (!File.Exists(savePath)) return null;

            try
            {
                string json = File.ReadAllText(savePath);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch { return null; }
        }

        internal static bool HasSave() => File.Exists(savePath);
        internal static void DeleteSave() { if (File.Exists(savePath)) File.Delete(savePath); }
    }
}
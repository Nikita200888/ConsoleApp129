using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConsoleApp129
{
   

    internal class SaveData
    {
        public int Level { get; set; }
        public int HeroX { get; set; }
        public int HeroY { get; set; }
        public List<string> MapRows { get; set; }
    }

    internal static class SaveManager
    {
        private static string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ОШИБКА] Не удалось сохранить: {ex.Message}");
            }
        }

        internal static SaveData LoadGame()
        {
            // ВЫБРАСЫВАЕМ ИСКЛЮЧЕНИЕ, ЕСЛИ ФАЙЛА НЕТ
            if (!File.Exists(savePath))
            {
                throw new GameActionException("Сохранений пока нет! Сначала начните новую игру и сохранитесь.");
            }

            try
            {
                string json = File.ReadAllText(savePath);
                SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

                if (data == null || data.MapRows == null)
                {
                    throw new GameActionException("Файл сохранения поврежден! Начните новую игру.");
                }
                return data;
            }
            catch (GameActionException)
            {
                throw; // Пробрасываем наше исключение наверх в Program.cs
            }
            catch (Exception ex)
            {
                throw new GameActionException($"Ошибка при чтении файла: {ex.Message}");
            }
        }

        internal static bool HasSave() => File.Exists(savePath);
        internal static void DeleteSave() { if (File.Exists(savePath)) File.Delete(savePath); }
    }
}
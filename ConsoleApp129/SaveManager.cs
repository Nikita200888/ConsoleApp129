using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConsoleApp129
{
    /// <summary>
    /// Класс для хранения данных сохранения игры.
    /// </summary>
    internal class SaveData
    {
        /// <summary>Номер уровня.</summary>
        public int Level { get; set; }
        /// <summary>Координата X героя.</summary>
        public int HeroX { get; set; }
        /// <summary>Координата Y героя.</summary>
        public int HeroY { get; set; }
        /// <summary>Строки карты.</summary>
        public List<string> MapRows { get; set; }
    }

    /// <summary>
    /// Статический класс для управления сохранениями игры.
    /// </summary>
    internal static class SaveManager
    {
        private static string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

        /// <summary>
        /// Сохраняет текущее состояние игры в JSON-файл.
        /// </summary>
        /// <param name="level">Номер уровня.</param>
        /// <param name="mapArray">Массив карты.</param>
        /// <param name="heroX">Координата X героя.</param>
        /// <param name="heroY">Координата Y героя.</param>
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

        /// <summary>
        /// Загружает игру из JSON-файла.
        /// </summary>
        /// <returns>Данные сохранения.</returns>
        /// <exception cref="GameActionException">Выбрасывается, если файл отсутствует или поврежден.</exception>
        internal static SaveData LoadGame()
        {
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
                throw;
            }
            catch (Exception ex)
            {
                throw new GameActionException($"Ошибка при чтении файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет наличие файла сохранения.
        /// </summary>
        /// <returns>True, если файл существует.</returns>
        internal static bool HasSave() => File.Exists(savePath);

        /// <summary>
        /// Удаляет файл сохранения.
        /// </summary>
        internal static void DeleteSave() { if (File.Exists(savePath)) File.Delete(savePath); }
    }
}
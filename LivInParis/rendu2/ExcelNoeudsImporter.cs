using System;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;

namespace GraphProject
{
    public static class ExcelNoeudsImporter
    {
        public static Dictionary<string, (double Latitude, double Longitude)> ChargerNoeuds(string cheminExcel)
        {
            var dict = new Dictionary<string, (double, double)>();
            using (var workbook = new XLWorkbook(cheminExcel))
            {
                var ws = workbook.Worksheet("Noeuds");
                int row = 2; /// <summary> Supposons que la première ligne est l'en-tête</summary>
                while (!ws.Row(row).IsEmpty())
                {
                    ///<summary> La colonne 3 contient le nom de la station, la 4 la longitude et la 5 la latitude</summary>
                    string stationName = ws.Cell(row, 3).Value.ToString().Trim();
                    
                    ///<summary> Utiliser .Value.ToString() au lieu de GetValue<string>()</summary>
                    string lonStr = ws.Cell(row, 4).Value.ToString().Trim();
                    string latStr = ws.Cell(row, 5).Value.ToString().Trim();
                    
                    double longitude, latitude;
                    
                    if (!double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
                    {
                        if (!double.TryParse(lonStr, NumberStyles.Any, CultureInfo.CurrentCulture, out longitude))
                        {
                            throw new Exception($"Impossible de convertir la valeur de la cellule D{row} en double: {lonStr}");
                        }
                    }
                    
                    if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out latitude))
                    {
                        if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.CurrentCulture, out latitude))
                        {
                            throw new Exception($"Impossible de convertir la valeur de la cellule E{row} en double: {latStr}");
                        }
                    }

                    if (!dict.ContainsKey(stationName))
                    {
                        dict.Add(stationName, (latitude, longitude));
                    }
                    row++;
                }
            }
            return dict;
        }
    }
}

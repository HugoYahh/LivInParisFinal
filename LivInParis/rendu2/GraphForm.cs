using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GraphProject
{
    public class GraphForm : Form
    {
        private List<string> bestPath;
        private Dictionary<string, (double Latitude, double Longitude)> stationCoordinates;
        /// <summary>
        /// Variables pour recalculer les bornes (min/max) des coordonnées pour adapter à la zone d'affichage.
        /// </summary>
        private double minLat, maxLat, minLon, maxLon;

        public GraphForm(List<string> bestPath, Dictionary<string, (double Latitude, double Longitude)> stationCoordinates)
        {
            this.bestPath = bestPath;
            this.stationCoordinates = stationCoordinates;
            this.Text = "Visualisation du Meilleur Chemin";
            this.Width = 800;
            this.Height = 600;
            this.DoubleBuffered = true;
            CalculerBornes();
        }

        /// <summary>
        /// Calcule les bornes (min/max) de latitude et longitude pour adapter le dessin.
        /// </summary>
        private void CalculerBornes()
        {
            bool first = true;
            foreach (var coord in stationCoordinates.Values)
            {
                if (first)
                {
                    minLat = maxLat = coord.Latitude;
                    minLon = maxLon = coord.Longitude;
                    first = false;
                }
                else
                {
                    if (coord.Latitude < minLat) minLat = coord.Latitude;
                    if (coord.Latitude > maxLat) maxLat = coord.Latitude;
                    if (coord.Longitude < minLon) minLon = coord.Longitude;
                    if (coord.Longitude > maxLon) maxLon = coord.Longitude;
                }
            }
        }

        /// <summary>
        /// Transforme les coordonnées géographiques en coordonnées d'écran (client).
        /// </summary>
        
        private PointF ConvertirCoordonnees(double lat, double lon)
        {
            ///<summary> On laisse une marge de 20 pixels tout autour.</summary>
            float margin = 20f;
            float width = ClientSize.Width - 2 * margin;
            float height = ClientSize.Height - 2 * margin;
            
            ///<summary> Normalisation des valeurs entre 0 et 1.</summary>
            float x = (float)((lon - minLon) / (maxLon - minLon));
            float y = (float)((maxLat - lat) / (maxLat - minLat)); ///<summary> inversion de y pour que le nord soit en haut</summary>

            return new PointF(margin + x * width, margin + y * height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            ///<summary> Dessiner toutes les stations en bleu.</summary>
            foreach (var kvp in stationCoordinates)
            {
                string station = kvp.Key;
                var coord = kvp.Value;
                PointF pt = ConvertirCoordonnees(coord.Latitude, coord.Longitude);
                g.FillEllipse(Brushes.Blue, pt.X - 5, pt.Y - 5, 10, 10);
                g.DrawString(station, this.Font, Brushes.Black, pt.X + 6, pt.Y);
            }

            ///<summary> Dessiner le meilleur chemin en rouge si la liste est valide.</summary>
            if (bestPath != null && bestPath.Count >= 2)
            {
                Pen cheminPen = new Pen(Color.Red, 3);
                for (int i = 0; i < bestPath.Count - 1; i++)
                {
                    if (stationCoordinates.TryGetValue(bestPath[i], out var coordA) &&
                        stationCoordinates.TryGetValue(bestPath[i + 1], out var coordB))
                    {
                        PointF ptA = ConvertirCoordonnees(coordA.Latitude, coordA.Longitude);
                        PointF ptB = ConvertirCoordonnees(coordB.Latitude, coordB.Longitude);
                        g.DrawLine(cheminPen, ptA, ptB);
                    }
                }
            }
        }
    }
}

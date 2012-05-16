using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace BingMapToDeepZoom
{
    class Program
    {
        public Program(string bingMapDir, string deepZoomDir)
        {
            // Determine whether the bing map directory exists.
            if (!Directory.Exists(bingMapDir))
            {
                Debug.WriteLine("Bing Map directory: " + bingMapDir + " doesn't exist.");
                return;
            }
            // Determine whether the deepzoom directory exists.
            if (Directory.Exists(deepZoomDir))
            {
                Debug.WriteLine("Deep Zoom directory: " + deepZoomDir + " exists already.");
                return;
            }

            // Try to create the directory.
            DirectoryInfo di = Directory.CreateDirectory(deepZoomDir);
            Debug.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(deepZoomDir));

            string[] filePaths = Directory.GetFiles(bingMapDir, "*.png");
            foreach (string path in filePaths)
            {
                string file = path.Replace(bingMapDir, "");
                file = file.Replace(".png", "");
                int tileX, tileY, levelOfDetail;
                QuadKeyToTileXY(file, out tileX, out tileY, out levelOfDetail);
                string fileName = tileX + "_" + tileY + ".png";
                string levelDir = deepZoomDir + levelOfDetail.ToString();
                if (!Directory.Exists(levelDir))
                {
                    Directory.CreateDirectory(levelDir);
                }
                File.Move(path, levelDir + "\\" + fileName);
                /*if (levelOfDetail < 4)
                {
                    Debug.WriteLine("X=" + tileX + " Y=" + tileY + " level=" + levelOfDetail);
                }*/
                //Debug.WriteLine("X=" + tileX + " Y=" + tileY + " level=" + levelOfDetail);
            }

            Debug.WriteLine("Success!");
        }



        /// <summary>
        /// Converts a QuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">QuadKey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
        {
            tileX = tileY = 0;
            levelOfDetail = quadKey.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[levelOfDetail - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }

        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        static void Main(string[] args)
        {
            //Debug.WriteLine(TileXYToQuadKey(0, 0, 1));
            //Debug.WriteLine(TileXYToQuadKey(0, 0, 2));
            if (args.Length == 2)
            {
                new Program(args[0], args[1]);
            }
            else
            {
                Debug.WriteLine("Two arguments needed.");
            }
        }
    }
}

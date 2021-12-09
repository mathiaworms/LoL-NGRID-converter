using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLNGRIDConverter_Editer
{
    public class NavGridEditing
    {
        public static void ApplyEdits(List<NavGridCell> cells, int cellCountX, int cellCountZ)
        {
            // Use SetCellFlagsByRow here as needed.
            Console.WriteLine("Applied initial edits.");
        }
        public static void SetCellFlagsByRow(List<NavGridCell> cells, VisionPathingFlags checkFlag, VisionPathingFlags newFlag, int gridwidth, int gridheight, int x, int z, int width, int height)
        {
            // Assumes starting position is a bottom left corner.
            int startindex = x * z;
            int maxdrawtries = width * height;
            int drawtries = 0;
            int realdraws = 0;
            int firstdrawx = 0, firstdrawz = 0;
            int lastdrawx = 0, lastdrawz = 0;
            for (int searches = startindex; searches < cells.Count; searches++)
            {
                if ((cells[searches].x <= (x + width) && cells[searches].x >= x) && (cells[searches].z <= (z + height) && cells[searches].z >= z))
                {
                    if (HasCellVisionFlag(cells[searches], checkFlag))
                    {
                        SetCellVisionFlag(cells[searches], newFlag);

                        if (realdraws == 0)
                        {
                            firstdrawx = cells[searches].x;
                            firstdrawz = cells[searches].z;
                        }
                        lastdrawx = cells[searches].x;
                        lastdrawz = cells[searches].z;
                        realdraws++;
                    }
                    drawtries++;
                }
                if (realdraws == maxdrawtries || searches == cells.Count)
                {
                    break;
                }
            }

            //Console.WriteLine("startindex: " + startindex);
            //Console.WriteLine("maxdrawtries: " + maxdrawstries);
            //Console.WriteLine("drawtries: " + drawtries);
            Console.WriteLine("draws: " + realdraws);
            if (realdraws > 0)
            {
                Console.WriteLine("Set cells (by row): " + "(" + firstdrawx + ", " + firstdrawz + ") =>" + " (" + lastdrawx + ", " + lastdrawz + ")" + " to have flag " + newFlag + ".");
            }
            else
            {
                Console.WriteLine("No cells were changed.");
            }
        }

        public static void SetCellVisionFlag(NavGridCell cell, VisionPathingFlags flag)
        {
            cell.visionPathingFlags = flag;
        }
        public static bool HasCellVisionFlag(NavGridCell cell, VisionPathingFlags flag)
        {
            return cell.visionPathingFlags == flag;
        }
    }
}

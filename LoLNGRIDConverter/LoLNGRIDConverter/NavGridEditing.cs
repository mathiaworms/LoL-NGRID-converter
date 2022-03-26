using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LoLNGRIDConverter_Editer
{
    public class NavGridEditing
    {
        public static void ApplyEdits(List<NavGridCell> cells, int cellCountX, int cellCountZ)
        {
          SetCellFlagsByRow(cells, VisionPathingFlags.Wall, VisionPathingFlags.Walkable, cellCountX, cellCountZ, 0, 0, cellCountX, cellCountZ);
           SetCellFlagsByRow(cells, VisionPathingFlags.TransparentWall, VisionPathingFlags.Walkable, cellCountX, cellCountZ, 0, 0, cellCountX, cellCountZ); 
          SetCellFlagsByRow(cells, VisionPathingFlags.Brush, VisionPathingFlags.Walkable, cellCountX, cellCountZ, 0, 0, cellCountX, cellCountZ);
           SetCellFlagsByRow(cells, VisionPathingFlags.AlwaysVisible, VisionPathingFlags.Walkable, cellCountX, cellCountZ, 0, 0, cellCountX, cellCountZ);
           int counter = 0;  
           
             Console.WriteLine("/////////////////////////////////////////////////////////////////////////////////");
            
             Console.ReadLine();
             // Read the file and display it line by line.  
            foreach (string line in System.IO.File.ReadLines(@"D:\a model 3d\lolngridconverter\LoLNGRIDConverter\LoL-NGRID-converter\LoLNGRIDConverter\LoLNGRIDConverter\AIPath.visionPathing.txt"))
                {  
               
                 string[] numbers = Regex.Split(line, @"\D+");
               // Console.WriteLine("reussis");
              //   Console.ReadLine();
                if (numbers != null)
                     {
                foreach (string value in numbers)
                    {
                      if (!string.IsNullOrEmpty(value))
                     {
                              int i = int.Parse(value);
                       }
                    }
                     }
               //   string[] numbers = Regex.Split(line, @"\D+");
                if (numbers != null)
                     {
                    string verifflag = numbers[3] + numbers[4] + numbers[5];
                     Console.WriteLine(verifflag);
                     
              /*  
                if (verifflag == "255255255") {
                     
                     SetCellFlagsByRow(cells, VisionPathingFlags.Wall, VisionPathingFlags.Walkable, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                   
                     SetCellFlagsByRow(cells, VisionPathingFlags.Brush, VisionPathingFlags.Walkable , 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);

                     }
                     if(verifflag == "012214") {
                        Console.WriteLine("reussis");
                        Console.ReadLine();
                     SetCellFlagsByRow(cells, VisionPathingFlags.Wall, VisionPathingFlags.Brush, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                     SetCellFlagsByRow(cells, VisionPathingFlags.Walkable, VisionPathingFlags.Brush, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                     }
                     if(verifflag == "065122") {

                     SetCellFlagsByRow(cells, VisionPathingFlags.Walkable, VisionPathingFlags.Wall, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);

                     SetCellFlagsByRow(cells, VisionPathingFlags.Brush, VisionPathingFlags.Wall, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                    
                     }
                     else {
                         Console.WriteLine(verifflag);
                        Console.WriteLine("echec");
                        Console.WriteLine(verifflag);
                        Console.ReadLine();
                    }*/
                      
                      
                      switch(verifflag) 
                            {
                            case "255255255":
                           SetCellFlagsByRow(cells, VisionPathingFlags.Wall, VisionPathingFlags.Walkable, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                   
                            SetCellFlagsByRow(cells, VisionPathingFlags.Brush, VisionPathingFlags.Walkable , 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                            break;
                            case "012214":
                             SetCellFlagsByRow(cells, VisionPathingFlags.Wall, VisionPathingFlags.Brush, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                            SetCellFlagsByRow(cells, VisionPathingFlags.Walkable, VisionPathingFlags.Brush, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                            
                            break;
                            case "065122":
                            SetCellFlagsByRow(cells, VisionPathingFlags.Walkable, VisionPathingFlags.Wall, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);

                            SetCellFlagsByRow(cells, VisionPathingFlags.Brush, VisionPathingFlags.Wall, 1, 1, Int32.Parse(numbers[1]), Int32.Parse(numbers[2]), 1,1);
                            break;
                            default:
                            Console.WriteLine(verifflag);
                            Console.WriteLine("echec");
                            Console.WriteLine(verifflag);
                            Console.ReadLine();
                            break;
                            }
                 
              }  
               counter++;  
            } 
        Console.ReadLine(); 
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

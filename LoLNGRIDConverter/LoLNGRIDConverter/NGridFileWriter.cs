using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLNGRIDConverter_Editer
{
    public class NGridFileWriter
    {

        #region Color definitions

        private static Color walkableColor = new Color(255, 255, 255);
        private static Color brushColor = new Color(0, 122, 14);
        private static Color wallColor = new Color(64, 64, 64);
        private static Color brushWallColor = new Color(0, 216, 111);
        private static Color transparentWallColor = new Color(0, 210, 214);
        private static Color alwaysVisibleColor = new Color(192, 192, 0); // not seen from Riot
        private static Color blueTeamOnlyColor = new Color(87, 79, 255);
        private static Color redTeamOnlyColor = new Color(255, 124, 124);
        private static Color neutralZoneVisibilityColor = new Color(255, 165, 0);  // HTML orange (not seen from Riot)
        private static Color blueTeamNeutralZoneVisibilityColor = new Color(12, 0, 255);
        private static Color redTeamNeutralZoneVisibilityColor = new Color(255, 0, 0);

        // all other flags will get indexed into this array
        private static Color[] flagColors = new Color[] { new Color(64, 0, 0),
                                                          new Color(140, 0, 0),
                                                          new Color(240, 0, 0),
                                                          new Color(0, 100, 0),
                                                          new Color(0, 240, 0),
                                                          new Color(0, 0, 100),
                                                          new Color(0, 0, 240),
                                                          new Color(100, 0, 100),
                                                          new Color(240, 0, 240),
                                                          new Color(140, 140, 0),
                                                          new Color(240, 240, 0),
                                                          new Color(0, 140, 140),
                                                          new Color(0, 240, 240),
                                                          new Color(64, 64, 64),
                                                          new Color(160, 160, 160),
                                                          new Color(240, 240, 240)
                                                        };

        // height samples are a gradient based on whatever this color is
        // highest height sample = pure base color
        // lowest height sample = pure black
        // 
        // black and white looks nicer than red as a base, but if there's only a single height
        // sample value for the entire map then the default image becomes pure white, which
        // blends in with the background when the image viewer is opened (and even with SR height
        // samples, the corners of spawn are still not very visible against the white background),
        // so we'll stick with the red base color for now
        // 
        // red also seems to be the easiest to differentiate between different shades
        private static Color heightSampleBaseColor = new Color(255, 0, 0);

        #endregion

        private FileWrapper file;
        private FileWrapper fileCopy;
        private NGridFileReader NGridCopy;

        public NGridFileWriter(FileWrapper file, FileWrapper fileCopy, NGridFileReader NGridCopy)
        {
            this.file = file;
            this.fileCopy = fileCopy;
            this.NGridCopy = NGridCopy;

            file.WriteByte(NGridCopy.ngridMajorVersion);
            file.WriteShort(NGridCopy.ngridMinorVersion);

            Console.WriteLine("Wrote majorVersion: " + NGridCopy.ngridMajorVersion + ", minorVersion: " + NGridCopy.ngridMinorVersion);

            file.WriteVector3(NGridCopy.minBounds);
            file.WriteVector3(NGridCopy.maxBounds);

            Console.WriteLine("Wrote min bounds: " + NGridCopy.minBounds + ", max bounds: " + NGridCopy.maxBounds);

            file.WriteFloat(NGridCopy.cellSize);
            file.WriteInt(NGridCopy.cellCountX);
            file.WriteInt(NGridCopy.cellCountZ);

            Console.WriteLine("Wrote cell size: " + NGridCopy.cellSize + ", cell count X: " + NGridCopy.cellCountX + ", cell count Z: " + NGridCopy.cellCountZ);

            Console.WriteLine("\nWriting cells: " + file.GetFilePosition());

            if (NGridCopy.ngridMajorVersion == 7)
            {
                WriteCellsVersion7();
            }
            else if (NGridCopy.ngridMajorVersion == 3 || NGridCopy.ngridMajorVersion == 5)
            {
                WriteCellsVersion5();
            }
            else
            {
                throw new System.Exception("Error: unsupported version number " + NGridCopy.ngridMajorVersion);
            }

            Console.WriteLine("Writing height samples:  " + file.GetFilePosition());

            WriteHeightSamples();


            Console.WriteLine("\nWriting hint nodes:  " + file.GetFilePosition());

            WriteHintNodes();

            Console.WriteLine("\nLast written location: " + file.GetFilePosition());
            Console.WriteLine("Missed bytes:  " + (fileCopy.GetLength() - file.GetFilePosition()));

            Program.TryReadFile(file.GetFullFilePath());
        }

        #region WriteCellsVersion7()

        private void WriteCellsVersion7()
        {
            int totalCellCount = NGridCopy.cellCountX * NGridCopy.cellCountZ;

            for (int i = 0; i < totalCellCount; i++)
            {
                //file.WriteInt(i); // cell index

                file.WriteFloat(NGridCopy.cells[i].centerHeight); // (overridden by height samples)
                file.WriteInt(NGridCopy.cells[i].sessionID);
                file.WriteFloat(NGridCopy.cells[i].arrivalCost);
                file.WriteInt(Convert.ToInt32(NGridCopy.cells[i].isOpen));
                file.WriteFloat(NGridCopy.cells[i].Heuristic);

                file.WriteShort(NGridCopy.cells[i].z);
                file.WriteShort(NGridCopy.cells[i].x);

                file.WriteInt(NGridCopy.cells[i].actorList);
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition())); // unknown 1
                file.WriteInt(NGridCopy.cells[i].goodCellSessionID);
                file.WriteFloat(NGridCopy.cells[i].hintHeight);
                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition())); // unknown 2
                file.WriteShort(NGridCopy.cells[i].arrivalDirection);
                file.WriteShort(NGridCopy.cells[i].hintNode1);
                file.WriteShort(NGridCopy.cells[i].hintNode2);
            }


            for (int i = 0; i < totalCellCount; i++)
            {
                file.WriteShort((short)NGridCopy.cells[i].visionPathingFlags); // vision pathing flags
            }

            for (int i = 0; i < totalCellCount; i++)
            {
                file.WriteByte((byte)NGridCopy.cells[i].riverRegionFlags); // river region flags

                // seems to be impossible to write these bytes by using their NGridCopy counterparts due to bitwise operations that occur during their assignment
                // consequentially, this means it is impossible to edit these flags and expect to backconvert correctly
                file.WriteByte(fileCopy.ReadByte(file.GetFilePosition())); // jungle quadrant and main region flags

                file.WriteByte(fileCopy.ReadByte(file.GetFilePosition())); // nearest lane and point of interest flags

                file.WriteByte((byte)NGridCopy.cells[i].ringFlags);
            }


            // appears to be 8 blocks of 132 bytes each, but in practice only 7 are used and the 8th is all zeros
            // 
            // roughly appears to be 8 bytes of maybe some sort of hash followed by alternating between four bytes of zero and four bytes
            // of garbage (a couple make valid floats, most are invalid floats, maybe more hashes?)
            // 
            // at a certain point, each block becomes all zero for the rest of the block, but this varies by block (appears to be around
            // 40-48 bytes after the first 8 bytes until the rest is all zero)

            Console.WriteLine("Writing unknown block: " + file.GetFilePosition());
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 132; j++)
                {
                    file.WriteByte(fileCopy.ReadByte(file.GetFilePosition()));
                }
            }
        }

        #endregion

        #region WriteCellsVersion5()

        private void WriteCellsVersion5()
        {
            int totalCellCount = NGridCopy.cellCountX * NGridCopy.cellCountZ;

            for (int i = 0; i < totalCellCount; i++)
            {
                //file.WriteInt(i); // cell index

                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition())); // center height (overridden by height samples)
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition())); // session ID
                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition()));  // arrival cost
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition()));  // is open
                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition()));  // heuristic
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition()));  // actor list

                file.WriteShort(NGridCopy.cells[i].x);
                file.WriteShort(NGridCopy.cells[i].z);

                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition())); // additional cost
                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition()));  // hint as good cell
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition()));  // additional cost count
                file.WriteInt(fileCopy.ReadInt(file.GetFilePosition()));  // good cell session ID
                file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition()));  // hint weight

                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition()));  // arrival direction
                file.WriteShort((int)NGridCopy.cells[i].visionPathingFlags);  // vision pathing flags
                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition()));  // hint node 1
                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition()));  // hint node 2
            }


            if (NGridCopy.ngridMajorVersion == 5)
            {
                // version 5 only has 2 bytes per cell instead of version 7's 4 bytes per cell, meaning that some flag layers are missing in version 5
                Console.WriteLine("Writing flag block:  " + file.GetFilePosition());
                for (int i = 0; i < totalCellCount; i++)
                {
                    file.WriteByte((byte)NGridCopy.cells[i].riverRegionFlags); // river region flags

                    file.WriteByte(fileCopy.ReadByte(file.GetFilePosition())); // jungle quadrant and main region flags
                }

                // version 5 only has 4 blocks of 132 bytes each instead of version 7's 8 blocks of 132 bytes each
                Console.WriteLine("Writing unknown block:  " + file.GetFilePosition());
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 132; j++)
                    {
                        file.WriteByte(fileCopy.ReadByte(file.GetFilePosition()));
                    }
                }
            }
            else
            {
                // version 2 and version 3 lack an extra flag block and jump straight into height samples after the last cell
            }
        }

        #endregion

        #region WriteHeightSamples()

        private void WriteHeightSamples()
        {
            // these are what's actually used for the height mesh
            // unwalkable cells define a center height value of 0, but this (accidentally?) includes base gates
            // these height sample values accomadate for base gates, so they should be used instead of the center height values
            // 
            // there appears to be one sample per cell corner
            // total sample count = (total cell count * 4) + (cell count X * 2) + (cell count Z * 2) + 1

            file.WriteInt(NGridCopy.heightSampleCountX);
            file.WriteInt(NGridCopy.heightSampleCountZ);
            file.WriteFloat(NGridCopy.heightSampleOffsetX);
            file.WriteFloat(NGridCopy.heightSampleOffsetZ);

            // write each sample so any re-conversions can get an accurate gradient when recording min and max height samples
            int totalCount = NGridCopy.heightSampleCountX * NGridCopy.heightSampleCountZ;
            for (int i = 0; i < totalCount; i++)
            {
                file.WriteFloat(NGridCopy.heightSamples[i]);
            }
        }

        #endregion

        #region WriteHintNodes()

        private void WriteHintNodes()
        {
            // not really sure how this data is used (the 'hint nodes' in NavGridCell appear to refer to these)
            for (int i = 0; i < 900; i++) // this 900 value *might* be variable, in practice it's constant though
            {
                for (int j = 0; j < 900; j++) // same as above
                {
                    // this appears to be a distance to another cell, but not sure how cells are indexed here, also seems to be mostly whole numbers
                    file.WriteFloat(fileCopy.ReadFloat(file.GetFilePosition()));
                }

                // are these what is referred to by 'hint nodes' in NavGridCell?
                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition()));
                file.WriteShort(fileCopy.ReadShort(file.GetFilePosition()));
            }
        }

        #endregion
    }
}

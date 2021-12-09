using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace LoLNGRIDConverter_Editer
{
    public class Program
    {
        public static FileWrapper lastFile;
        public static NGridFileReader lastNGridFile;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine("LoL NGRID Converter and Editor by FrankTheBoxMonster and Lizardy");

            if(args.Length < 1)
            {
                Console.WriteLine("Error: must provide a file (you can drag-and-drop one or more files onto the .exe)");
                Pause(true);
                System.Environment.Exit(1);
            }
            Console.WriteLine("FilePath of NavGrid that will be converted: " + args[0]);

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    Console.WriteLine("\nConverting file " + (i + 1) + "/" + args.Length + ":  " + args[i].Substring(args[i].LastIndexOf('\\') + 1));
                    Console.WriteLine("Input additional conversion arguments into the popup dialogs to continue...");
                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault(false);
                    //Application.Run(new Form1());
                    string input1 = Interaction.InputBox("Should edits to the NavGrid be applied when exporting? (occurs for each converted file)", "Apply Edits?", "false", -1, -1);
                    bool applyEdits = false;
                    string input2 = Interaction.InputBox("Should we convert back to aimesh_ngrid after exporting? (only works for the last converted file)", "Back Convert?", "false", -1, -1);
                    bool backConvert = false;
                    if (!input1.Equals("") && !input2.Equals(""))
                    {
                        if (!input1.Equals("true", StringComparison.CurrentCultureIgnoreCase) && !input1.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("Error:  Bad input for argument 2 " + "(" + input1 + "), applying edits turned off.");
                            applyEdits = false;
                        }
                        else
                        {
                            if (input1.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                applyEdits = true;
                            if (input1.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                                applyEdits = false;
                        }
                        if (!input2.Equals("true", StringComparison.CurrentCultureIgnoreCase) && !input2.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("Error: Bad input for argument 3 " + "(" + input2 + "), back converting turned off.");
                            backConvert = false;
                        }
                        else
                        {
                            if (input2.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                backConvert = true;
                            if (input2.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                                backConvert = false;
                        }
                        Console.WriteLine("Applying Edits: " + applyEdits.ToString());
                        Console.WriteLine("Back Converting: " + backConvert.ToString());
                        TryReadFile(args[i], applyEdits, backConvert);
                    }
                    else
                    {
                        Console.WriteLine("Error: Bad input for argument 2 and 3 " + "(" + input1 + ", " + input2 + "), applying edits and back converting turned off.");
                        TryReadFile(args[i]);
                    }
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("\n\nError: " + e.ToString());
                }
            }

            Console.WriteLine("\n\nDone");

            //Pause(true);
        }


        public static void Pause(bool intercept)
        {
            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey(intercept);
        }

        public static void TryReadFile(string filePath, bool applyEdits = false, bool backConvert = false)
        {
            FileWrapper input = new FileWrapper(filePath);
            lastFile = input;

            if (filePath.ToLower().EndsWith(".aimesh_ngrid") == false)
            {
                Console.WriteLine("Error: not an .AIMESH_NGRID file");
                return;
            }


            int majorVersion = input.ReadByte();
            int minorVersion = 0;
            if(majorVersion != 2)
            {
                // not sure why this is short but the other is byte, might just be padding
                // note that the only known non-zero minor version to exist (other than unofficial 2.1) is 3.1, with no know difference from 3.0
                minorVersion = input.ReadShort();
            }
            else
            {
                // version 2 lacked a minor version value (although it clearly needed one since there's an unofficial version 2.0 and 2.1 split)
            }

            Console.WriteLine("\nmajor version = " + majorVersion + " minor version = " + minorVersion);

            if (majorVersion != 7 && majorVersion != 5 && majorVersion != 3 && majorVersion != 2)
            {
                Console.WriteLine("Error:  unsupported version number " + majorVersion);
                return;
            }

            NGridFileReader file = new NGridFileReader(input, majorVersion, minorVersion);
            lastNGridFile = file;
            file.ConvertFiles(applyEdits, backConvert);
        }

        public static void TryWriteFile(string filePath)
        {
            FileWrapper input = new FileWrapper(filePath);

            if (filePath.ToLower().EndsWith(".aimesh_ngrid") == false)
            {
                Console.WriteLine("Error: not an .AIMESH_NGRID file");
                return;
            }


            int majorVersion = lastNGridFile.ngridMajorVersion;
            int minorVersion = 0;
            if (majorVersion != 2)
            {
                // not sure why this is short but the other is byte, might just be padding
                // note that the only known non-zero minor version to exist (other than unofficial 2.1) is 3.1, with no know difference from 3.0
                minorVersion = lastNGridFile.ngridMinorVersion;
            }
            else
            {
                // version 2 lacked a minor version value (although it clearly needed one since there's an unofficial version 2.0 and 2.1 split)
            }

            Console.WriteLine("\nmajor version: " + majorVersion + " minor version: " + minorVersion);

            if (majorVersion != 7 && majorVersion != 5 && majorVersion != 3)
            {
                Console.WriteLine("Error: unsupported version number " + majorVersion);
                return;
            }

            Console.WriteLine("\nWriting Back Converted NavGrid...");
            NGridFileWriter file = new NGridFileWriter(input, lastFile, lastNGridFile);
        }
    }
}

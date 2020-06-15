using System;
using System.IO;
using System.Collections.Generic;

namespace assembler
{
    public class Content
    {
        public static List<Instruction> Instructions = new List<Instruction>();
        public static List<Label> Labels = new List<Label>();
        public static List<string> Instruction_Content = new List<string>();
        public static bool Assemble_Error;
        public static int Average_CPI;
        public static int Num_Of_Instructions;
    }
    public class Instruction
    {
        public string Opcode { get; set; }
        public string Destination { get; set; }
        public string Source_One { get; set; }
        public string Source_Two { get; set; }
        public int Position { get; set; }
        public int CPI { get; set; }
        public string Binary_Opcode { get; set; }
        public string Binary_Destination { get; set; }
        public string Binary_Source_One { get; set; }
        public string Binary_Source_Two { get; set; }
        public string Binary_Function { get; set; }
        public string Binary_Constant { get; set; }
        public string Binary_MachineCode { get; set; }
        public string Type { get; set; }

        public void Print()
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine($"Opcode: {Opcode}");

            if(this.Type == "R")
            {
                Console.WriteLine($"Destination: {Destination}");
                Console.WriteLine($"Source One: {Source_One}");
                Console.WriteLine($"Source Two: {Source_Two}");
            }
            if(this.Type == "I")
            {
                Console.WriteLine($"Destination: {Destination}");
                Console.WriteLine($"Source: {Source_One}");
                Console.WriteLine($"Constant: {Source_Two}");
            }
            if(this.Type == "J")
            {
                if(this.Opcode == "JUMP")
                {
                    Console.WriteLine($"Label: {Source_Two}");
                } else
                {
                    Console.WriteLine($"Constant: {Source_Two}");
                }
                
            }

            Console.WriteLine($"CPI: {CPI}");
            Console.WriteLine($"Word Address: {Position}");
            Console.WriteLine($"Machine Code: {Binary_MachineCode}");
            Console.WriteLine("-------------------------------------------");
        }

        public void Encode()
        {
            try
            {
                var Types = new Dictionary<string, string> {
                { "ADD", "R"},
                { "ADDI", "I"},
                { "SUB", "R" },
                { "AND", "R" },
                { "ANDI", "I" },
                { "OR", "R" },
                { "DMADDR", "J" },
                { "XOR", "R" },
                { "SLT", "R" },
                { "MUL", "R" },
                { "DIV", "R" },
                { "SLL", "I" },
                { "SRL", "I" },
                { "SRA", "I" },
                { "LW", "I" },
                { "LWU", "I" },
                { "LWL", "I" },
                { "SW", "I" },
                { "SWU", "I" },
                { "SWL", "I" },
                { "BREQ", "I" },
                { "BRNE", "I" },
                { "JUMP", "J" }
                };

                var CPI = new Dictionary<string, int> {
                { "ADD", 4},
                { "ADDI", 4},
                { "SUB", 4},
                { "AND", 4},
                { "ANDI", 4},
                { "OR", 4},
                { "DMADDR", 4},
                { "XOR", 4},
                { "SLT", 4},
                { "MUL", 14},
                { "DIV", 16},
                { "SLL", 3},
                { "SRL", 3},
                { "SRA", 3},
                { "LW", 5},
                { "LWU", 4},
                { "LWL", 4},
                { "SW", 5},
                { "SWU", 4},
                { "SWL", 4},
                { "BREQ", 4},
                { "BRNE", 4},
                { "JUMP", 4}
                };

                var BinaryOpcodes = new Dictionary<string, string> {
                { "ADD", "0000"},
                { "ADDI", "0001"},
                { "SUB", "0000" },
                { "AND", "0000" },
                { "ANDI", "0010" },
                { "OR", "0000" },
                { "DMADDR", "0011" },
                { "XOR", "0000" },
                { "SLT", "0000"},
                { "MUL", "0000" },
                { "DIV", "0000" },
                { "SLL", "0100" },
                { "SRL", "0101" },
                { "SRA", "0110" },
                { "LW", "0111" },
                { "LWU", "1000" },
                { "LWL", "1001" },
                { "SW", "1010" },
                { "SWU", "1011" },
                { "SWL", "1100" },
                { "BREQ", "1101" },
                { "BRNE", "1110" },
                { "JUMP", "1111" }
                };

                var BinaryFunctions = new Dictionary<string, string> {
                { "ADD", "000"},
                { "SUB", "001" },
                { "AND", "100" },
                { "OR", "011" },
                { "XOR", "110" },
                { "SLT", "101"},
                { "MUL", "010" },
                { "DIV", "111" },
                };

                var BinaryRegisters = new Dictionary<string, string>
                {
                { "$R0", "000"},
                { "$R1", "001"},
                { "$R2", "010"},
                { "$R3", "011"},
                { "$R4", "100"},
                { "$R5", "101"},
                { "$R6", "110"},
                { "$R7", "111"}
                };

                this.Type = Types[Opcode];
                this.Binary_Opcode = BinaryOpcodes[Opcode];
                this.CPI = CPI[Opcode];

                if (this.Type == "R")
                {
                    this.Binary_Function = BinaryFunctions[Opcode];
                    this.Binary_Destination = BinaryRegisters[this.Destination];
                    this.Binary_Source_One = BinaryRegisters[this.Source_One];
                    this.Binary_Source_Two = BinaryRegisters[this.Source_Two];

                    this.Binary_MachineCode = this.Binary_Opcode + this.Binary_Destination + this.Binary_Source_One + this.Binary_Source_Two + this.Binary_Function;
                }
                if (this.Type == "I")
                {
                    if (this.Opcode == "BREQ" || this.Opcode == "BRNE")
                    {
                        this.Binary_Destination = BinaryRegisters[this.Destination];
                        this.Binary_Source_One = BinaryRegisters[this.Source_One];

                        foreach (Label label in Content.Labels)
                        {

                            if (label.Name == this.Source_Two)
                            {
                                int Offset = label.Instruction_Position - this.Position - 2;
                                this.Binary_Constant = Convert.ToString(Offset, 2).PadLeft(6, '0');
                            }
                        }

                    }
                    else
                    {
                        this.Binary_Destination = BinaryRegisters[this.Destination];
                        this.Binary_Source_One = BinaryRegisters[this.Source_One];
                        this.Binary_Constant = Convert.ToString(Convert.ToInt32(this.Source_Two), 2).PadLeft(6, '0');

                    }



                    if (this.Binary_Constant.Length > 6)
                    {
                        this.Binary_Constant = this.Binary_Constant.Substring(this.Binary_Constant.Length - 6);
                    }

                    this.Binary_MachineCode = this.Binary_Opcode + this.Binary_Destination + this.Binary_Source_One + this.Binary_Constant;
                }

                if (this.Type == "J")
                {
                    if (this.Opcode == "JUMP")
                    {
                        foreach (Label label in Content.Labels)
                        {

                            if (label.Name == this.Source_Two)
                            {
                                this.Binary_Constant = Convert.ToString(label.Instruction_Position, 2).PadLeft(12, '0');
                            }
                        }

                        this.Binary_MachineCode = this.Binary_Opcode + this.Binary_Constant;
                    }
                    else
                    {
                        this.Binary_Constant = Convert.ToString(Convert.ToInt32(this.Source_Two), 2).PadLeft(12, '0');

                        if (this.Binary_Constant.Length > 12)
                        {
                            this.Binary_Constant = this.Binary_Constant.Substring(this.Binary_Constant.Length - 12);
                        }

                        this.Binary_MachineCode = this.Binary_Opcode + this.Binary_Constant;

                    }


                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured. {0} Exception caught.", e);
                Content.Assemble_Error = true;
            }
            
        }

        public string Get_MachineCode()
        {
            return this.Binary_MachineCode;
        }
    }

    public class Label
    {
        public string Name { get; set; }
        public int Instruction_Position { get; set; }

        public void Print()
        {
            Console.WriteLine($"{Name} @ {Instruction_Position}");
        }
    }

    public class Datapath
    {
        Dictionary<string, string>BinaryRegisters = new Dictionary<string, string>
            {
                { "$R0", "000"},
                { "$R1", "001"},
                { "$R2", "010"},
                { "$R3", "011"},
                { "$R4", "100"},
                { "$R5", "101"},
                { "$R6", "110"},
                { "$R7", "111"}
            };

        public int Program_Counter { get; set; }

        public void Next()
        {
            
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            string command = "None";

            

            while (command != "exit")
            {
                Console.Write(">> ");
                command = Console.ReadLine();

                if(command == "read")
                {
                    string[] lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\code.txt"));
                    int instruction_position = 0;

                    foreach (string line in lines)
                    {
                        string label = "";
                        string opcode = "";
                        string destination = "";
                        string source_one = "";
                        string source_two = "";
                        string temp = "";

                        for (int counter = 0; counter<line.Length; counter++)
                        {
                            if(line[counter].Equals(':'))
                            {
                                label = temp;
                                temp = "";
                            }

                            if(Char.IsWhiteSpace(line[counter]) & !(opcode.Length>0))
                            {
                                opcode = temp;
                                temp = "";
                            }
                            if(line[counter].Equals(',') & (opcode.Length > 0) & !(destination.Length > 0))
                            {
                                destination = temp;
                                temp = "";
                            }

                            if(line[counter].Equals(',') & (destination.Length > 0) & !(source_one.Length > 0))
                            {
                                source_one = temp;
                                temp = "";
                            }

                            temp = $"{temp}{line[counter]}";

                            if(counter == line.Length - 1 || line[counter+1].Equals('#'))
                            {
                                source_two = temp;
                                temp = "";
                                break;
                            }

                        }

                        label = label.Trim(' ', ',', '#', ':').ToUpper();
                        opcode = opcode.Trim(' ', ',', '#', ':').ToUpper();
                        destination = destination.Trim(' ', ',', '#', ':').ToUpper();
                        source_one = source_one.Trim(' ', ',', '#', ':').ToUpper();
                        source_two = source_two.Trim(' ', ',', '#', ':').ToUpper();

                        if (label.Length > 0)
                            Content.Labels.Add(new Label { 
                                Name = label, 
                                Instruction_Position = instruction_position 
                            });

                        if( opcode.Length > 0 )
                        {
                            Content.Instructions.Add(new Instruction { 
                                Opcode = opcode,
                                Destination = destination, 
                                Source_One = source_one, 
                                Source_Two = source_two, 
                                Position = instruction_position 
                            });
                            instruction_position++;
                        }
                            
                        // End of the line
                    }

                    foreach (Instruction instruction in Content.Instructions)
                    {
                        instruction.Encode();
                        instruction.Print();
                        Content.Average_CPI += instruction.CPI;
                        Content.Instruction_Content.Add(instruction.Get_MachineCode());
                    }

                    Content.Num_Of_Instructions = Content.Instructions.Count;
                    Content.Average_CPI /= Content.Num_Of_Instructions;

                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("Assemble Report:");
                    Console.WriteLine("Error Occured: {0}", Content.Assemble_Error);
                    Console.WriteLine("Number of Instruction: {0}", Content.Num_Of_Instructions);
                    Console.WriteLine("Average CPI: {0}", Content.Average_CPI);

                }

                if(command == "convert")
                {
                    String[] MachineCodeLines = Content.Instruction_Content.ToArray();
                    System.IO.File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\machinecode.txt"), MachineCodeLines);
                }
            }

        }
    }
}

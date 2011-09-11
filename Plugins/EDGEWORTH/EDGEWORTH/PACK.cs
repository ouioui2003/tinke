﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PluginInterface;

namespace EDGEWORTH
{
    public static class PACK
    {
        public static void Read(string file, IPluginHost pluginHost)
        {
            String romFile = pluginHost.Get_TempFolder() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
            File.Copy(file, romFile, true);
            BinaryReader br = new BinaryReader(File.OpenRead(file));
            Carpeta unpacked = new Carpeta();
            unpacked.files = new List<Archivo>();

            uint num_files = (br.ReadUInt32() / 0x04) - 1;
            br.ReadUInt32(); // Pointer table
            for (int i = 0; i < num_files; i++)
            {
                uint startOffset = br.ReadUInt32();
                long currPos = br.BaseStream.Position;
                br.BaseStream.Position = startOffset;

                Archivo newFile = new Archivo();
                newFile.name = "File " + i.ToString();
                newFile.offset = startOffset + 4;
                newFile.packFile = romFile;
                newFile.size = br.ReadUInt32();

                br.BaseStream.Position = currPos;
                unpacked.files.Add(newFile);
            }

            br.Close();
            pluginHost.Set_Files(unpacked);
        }
        public static void Write(string output, Carpeta unpackedFiles)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(output));
            
            // Write pointers
            bw.Write((uint)((unpackedFiles.files.Count + 1) * 4)); // Pointer table size
            uint currOffset = 0x00; // Pointer table offset
            bw.Write(currOffset);
            currOffset += (uint)(unpackedFiles.files.Count + 1) * 4 + 4;
            for (int i = 0; i < unpackedFiles.files.Count; i++)
            {
                bw.Write(currOffset);
                currOffset += unpackedFiles.files[i].size + 4;
            }

            // Write files
            for (int i = 0; i < unpackedFiles.files.Count; i++)
            {
                BinaryReader br;
                if (unpackedFiles.files[i].packFile is String && unpackedFiles.files[i].packFile != "")
                {
                    br = new BinaryReader(File.OpenRead(unpackedFiles.files[i].packFile));
                    br.BaseStream.Position = unpackedFiles.files[i].offset;
                }
                else
                    br = new BinaryReader(File.OpenRead(unpackedFiles.files[i].path));

                bw.Write((uint)unpackedFiles.files[i].size);
                bw.Write(br.ReadBytes((int)unpackedFiles.files[i].size));
            }

            bw.Flush();
            bw.Close();
        }
    }

}
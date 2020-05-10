﻿#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Threading.Tasks;

using ClassicUO.Game;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.IO.Resources
{
    internal class TexmapsLoader : UOFileLoader<UOTexture>
    {
        private readonly ushort[] _textmapPixels128 = new ushort[128 * 128];
        private readonly ushort[] _textmapPixels64 = new ushort[64 * 64];
        private UOFile _file;

        private TexmapsLoader(int count) : base(count) 
        {

        }

        public static TexmapsLoader _instance;
        public static TexmapsLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TexmapsLoader(Constants.MAX_LAND_TEXTURES_DATA_INDEX_COUNT);
                }

                return _instance;
            }
        }


        public override Task Load()
        {
            return Task.Run(() =>
            {
                string path = UOFileManager.GetUOFilePath("texmaps.mul");
                string pathidx = UOFileManager.GetUOFilePath("texidx.mul");

                FileSystemHelper.EnsureFileExists(path);
                FileSystemHelper.EnsureFileExists(pathidx);

                _file = new UOFileMul(path, pathidx, Constants.MAX_LAND_TEXTURES_DATA_INDEX_COUNT, 10);
                _file.FillEntries(ref Entries);
                string pathdef = UOFileManager.GetUOFilePath("TexTerr.def");

                if (!File.Exists(pathdef))
                    return;

                using (DefReader defReader = new DefReader(pathdef))
                {
                    while (defReader.Next())
                    {
                        int index = defReader.ReadInt();

                        if (index < 0 || index >= Constants.MAX_LAND_TEXTURES_DATA_INDEX_COUNT)
                            continue;

                        int[] group = defReader.ReadGroup();

                        if (group == null)
                            continue;

                        for (int i = 0; i < group.Length; i++)
                        {
                            int checkindex = group[i];

                            if (checkindex < 0 || checkindex >= Constants.MAX_LAND_TEXTURES_DATA_INDEX_COUNT)
                                continue;

                            Entries[index] = Entries[checkindex];
                        }
                    }
                }

                //using (StreamReader reader = new StreamReader(File.OpenRead(pathdef)))
                //{
                //    string line;

                //    while ((line = reader.ReadLine()) != null)
                //    {
                //        line = line.Trim();

                //        if (line.Length <= 0 || line[0] == '#')
                //            continue;

                //        string[] defs = line.Split(new[]
                //        {
                //            '\t', ' ', '#'
                //        }, StringSplitOptions.RemoveEmptyEntries);

                //        if (defs.Length < 2)
                //            continue;
                //        int index = int.Parse(defs[0]);

                //        if (index < 0 || index >= TEXTMAP_COUNT)
                //            continue;
                //        int first = defs[1].IndexOf("{");
                //        int last = defs[1].IndexOf("}");

                //        string[] newdef = defs[1].Substring(first + 1, last - 1).Split(new[]
                //        {
                //            ' ', ','
                //        }, StringSplitOptions.RemoveEmptyEntries);

                //        foreach (string s in newdef)
                //        {
                //            int checkindex = int.Parse(s);

                //            if (checkindex < 0 || checkindex >= TEXTMAP_COUNT)
                //                continue;
                //            _file.Entries[index] = _file.Entries[checkindex];
                //        }
                //    }
                //}
            });

        }

        public override UOTexture GetTexture(uint g, bool keepData = false)
        {
            if (g >= Resources.Length)
                return null;

            ref var texture = ref Resources[g];

            if (texture == null || texture.IsDisposed)
            {
                ushort[] pixels = GetTextmapTexture((ushort) g, out int size);

                if (pixels == null || pixels.Length == 0)
                    return null;

                texture = new UOTexture16(size, size);
                texture.SetData(pixels);

                SaveID(g);
            }
 
            return texture;
        }

        public override void CleanResources()
        {
            base.CleanResources();
            
            _file?.Dispose();
            _file = null;
            _instance = null;
        }

        private ushort[] GetTextmapTexture(ushort index, out int size)
        {
            ref readonly var entry = ref GetValidRefEntry(index);

            if (entry.Length <= 0)
            {
                size = 0;

                return null;
            }

            ushort[] pixels;

            if (entry.Extra == 0)
            {
                size = 64;
                pixels = _textmapPixels64;
            }
            else
            {
                size = 128;
                pixels = _textmapPixels128;
            }

            _file.Seek(entry.Offset);

            for (int i = 0; i < size; i++)
            {
                int pos = i * size;

                for (int j = 0; j < size; j++)
                    pixels[pos + j] = (ushort) (0x8000 | _file.ReadUShort());
            }

            return pixels;
        }
    }
}
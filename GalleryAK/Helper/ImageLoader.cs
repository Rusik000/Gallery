﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace GalleryAK
{
    public partial class MainWindow : Window
    {
        FileInfo[] ImageList;

        List<FileInfo> NewImageList = new List<FileInfo>();

        int[] ImageIdxList;
        Boolean ImageListReady = false;
        int ImageIdxListPtr = 0;
        int ImageIdxListDeletePtr = -1;       
        
        private void CreateIdxListCode()
        {
            if (ImageList != null && ImageList.Length > 0)
            {
                ImageIdxListDeletePtr = -1;
                ImageIdxList = null;
                ImageIdxList = new int[ImageList.Length];
                for (int i = 0; i < ImageList.Length; i++)
                {
                    ImageIdxList[i] = i;
                }
                ImageIdxListPtr = 0;
                if (RandomizeImages == true)
                {
                    InitRNGKeys();
                    EncryptIdxListCode();
                    InitRNGKeys();
                }
            }
            else
            {
                MessageBox.Show("No images found in: " + SlideShowDirectory);
                return;
            }
        }        
        private void GetFilesCode()
        {
            NewImageList = null;
            NewImageList = new List<FileInfo>();            
            if (string.IsNullOrEmpty(SlideShowDirectory) || !Directory.Exists(SlideShowDirectory))
            {
                return;
            }
            topTextBoxClass.messageDisplayStart("Finding images...", -1, true, false);            
            GetFiles(SlideShowDirectory, "*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.webp");
            topTextBoxClass.messageDisplayStart(NewImageList.Count + " images found.", 5);            
        }
        
        
        public void GetFiles(string path, string searchPattern)
        {
            string[] patterns = searchPattern.Split(';');
            Stack<string> dirs = new Stack<string>();
            if (!Directory.Exists(path))
            {
                return;
            }
            dirs.Push(path);
            int NextListUpdate = 0;
            do
            {
                string currentDir = dirs.Pop();
                try
                {
                    string[] subDirs = Directory.GetDirectories(currentDir);
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
                catch { }
                try
                {
                    foreach (string filter in patterns)
                    {
                        if (StartGetFiles_Cancel)
                        {
                            NewImageList.Clear();
                            return;
                        }                        
                        DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
                        FileInfo[] fs = dirInfo.GetFiles(filter);
                        NewImageList.AddRange(fs);
                        NextListUpdate += fs.Length;
                        if (NextListUpdate > 100)
                        {
                            NextListUpdate = 0;
                            topTextBoxClass.messageDisplayStart("Scanning directories... Images found: " + NewImageList.Count, -1, true, false);                            
                        }
                    }
                }
                catch { }
            } while (dirs.Count > 0);
        }
    }
}
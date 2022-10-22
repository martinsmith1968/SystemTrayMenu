﻿// <copyright file="RowData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.DataClasses
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using SystemTrayMenu.Utilities;
    using static SystemTrayMenu.Utilities.IconReader;
    using Menu = SystemTrayMenu.UserInterface.Menu;

    internal class RowData
    {
        private static DateTime contextMenuClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowData"/> class.
        /// empty dummy.
        /// </summary>
        internal RowData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowData"/> class.
        /// (Related replace "\x00" see #171.)
        /// </summary>
        /// <param name="isFolder">Flag if file or folder.</param>
        /// <param name="isAddionalItem">Flag if addional item, from other folder than root folder.</param>
        /// <param name="isNetworkRoot">Flag if resolved from network root folder.</param>
        /// <param name="level">The number of the menu level.</param>
        /// <param name="path">Path to item.</param>
        internal RowData(bool isFolder, bool isAddionalItem, bool isNetworkRoot, int level, string path)
        {
            IsFolder = isFolder;
            IsAddionalItem = isAddionalItem;
            IsNetworkRoot = isNetworkRoot;
            Level = level;

            try
            {
                FileInfo = new FileInfo(path.Replace("\x00", string.Empty));
                Path = IsFolder ? $@"{FileInfo.FullName}\" : FileInfo.FullName;
                FileExtension = System.IO.Path.GetExtension(Path);
                IsLink = FileExtension.Equals(".lnk", StringComparison.InvariantCultureIgnoreCase);
                if (IsLink)
                {
                    ResolvedPath = FileLnk.GetResolvedFileName(Path, out bool isLinkToFolder);
                    IsLinkToFolder = isLinkToFolder || FileLnk.IsNetworkRoot(ResolvedPath);
                    ShowOverlay = Properties.Settings.Default.ShowLinkOverlay;
                    Text = System.IO.Path.GetFileNameWithoutExtension(Path);
                    if (string.IsNullOrEmpty(ResolvedPath))
                    {
                        Log.Info($"Resolved path is empty: '{Path}'");
                        ResolvedPath = Path;
                    }
                }
                else
                {
                    ResolvedPath = Path;
                    if (string.IsNullOrEmpty(FileInfo.Name))
                    {
                        int nameBegin = FileInfo.FullName.LastIndexOf(@"\", StringComparison.InvariantCulture) + 1;
                        Text = FileInfo.FullName[nameBegin..];
                    }
                    else if (FileExtension.Equals(".url", StringComparison.InvariantCultureIgnoreCase) ||
                        FileExtension.Equals(".appref-ms", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ShowOverlay = Properties.Settings.Default.ShowLinkOverlay;
                        Text = System.IO.Path.GetFileNameWithoutExtension(FileInfo.Name);
                    }
                    else if (!IsFolder && Config.IsHideFileExtension())
                    {
                        Text = System.IO.Path.GetFileNameWithoutExtension(FileInfo.Name);
                    }
                    else
                    {
                        Text = FileInfo.Name;
                    }
                }

                ContainsMenu = IsFolder;
                if (Properties.Settings.Default.ResolveLinksToFolders)
                {
                    ContainsMenu |= IsLinkToFolder;
                }

                IsMainMenu = Level == 0;
            }
            catch (Exception ex)
            {
                Log.Warn($"path:'{path}'", ex);
            }
        }

        internal Icon Icon { get; private set; }

        internal FileInfo FileInfo { get; }

        internal string Path { get; }

        internal bool IsFolder { get; }

        internal bool IsAddionalItem { get; }

        internal bool IsNetworkRoot { get; }

        internal int Level { get; set; }

        internal string FileExtension { get; }

        internal bool IsLink { get; }

        internal string ResolvedPath { get; }

        internal bool IsLinkToFolder { get; }

        internal bool ShowOverlay { get; }

        internal string Text { get; }

        internal bool ContainsMenu { get; }

        internal bool IsMainMenu { get; }

        internal Menu SubMenu { get; set; }

        internal bool IsMenuOpen { get; set; }

        internal bool IsClicking { get; set; }

        internal bool IsSelected { get; set; }

        internal bool IsContextMenuOpen { get; set; }

        internal bool HiddenEntry { get; set; }

        internal int RowIndex { get; set; }

        internal bool IconLoading { get; set; }

        internal bool ProcessStarted { get; set; }

#if TODO  // WPF REMOVE?
        internal void SetData(RowData data, DataTable dataTable)
        {
            DataRow row = dataTable.Rows.Add();
            data.RowIndex = dataTable.Rows.IndexOf(row);

            if (HiddenEntry)
            {
                row[0] = AddIconOverlay(data.Icon, Properties.Resources.White50Percentage);
            }
            else
            {
                row[0] = data.Icon;
            }

            row[1] = data.Text;
            row[2] = data;
        }
#endif

        internal Icon ReadIcon(bool updateIconInBackground)
        {
            if (IsFolder || IsLinkToFolder)
            {
                Icon = GetFolderIconWithCache(Path, ShowOverlay, updateIconInBackground, IsMainMenu, out bool loading);
                IconLoading = loading;
            }
            else
            {
                Icon = GetFileIconWithCache(Path, ResolvedPath, ShowOverlay, updateIconInBackground, IsMainMenu, out bool loading);
                IconLoading = loading;
            }

            if (!IconLoading)
            {
                if (Icon == null)
                {
                    Icon = Properties.Resources.NotFound;
                }
                else if (HiddenEntry)
                {
                    Icon = AddIconOverlay(Icon, Properties.Resources.White50Percentage);
                }
            }

            return Icon;
        }

        internal void MouseDown(ListView dgv, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsClicking = true;
            }

            if (e != null &&
                e.RightButton == MouseButtonState.Pressed &&
                FileInfo != null &&
                dgv != null &&
                dgv.Items.Count > RowIndex &&
                (DateTime.Now - contextMenuClosed).TotalMilliseconds > 200)
            {
                IsContextMenuOpen = true;
                ShellContextMenu ctxMnu = new();
                Window window = dgv.GetParentWindow();
                var position = Mouse.GetPosition(window);
                position.Offset(window.Left, window.Top);
                if (ContainsMenu)
                {
                    DirectoryInfo[] dir = new DirectoryInfo[1];
                    dir[0] = new DirectoryInfo(Path);
                    ctxMnu.ShowContextMenu(dir, position);
                    TriggerFileWatcherChangeWorkaround();
                }
                else
                {
                    FileInfo[] arrFI = new FileInfo[1];
                    arrFI[0] = FileInfo;
                    ctxMnu.ShowContextMenu(arrFI, position);
                    TriggerFileWatcherChangeWorkaround();
                }

                IsContextMenuOpen = false;
                contextMenuClosed = DateTime.Now;
            }

            void TriggerFileWatcherChangeWorkaround()
            {
                try
                {
                    string parentFolder = System.IO.Path.GetDirectoryName(Path);
                    Directory.GetFiles(parentFolder);
                }
                catch (Exception ex)
                {
                    Log.Warn($"{nameof(TriggerFileWatcherChangeWorkaround)} '{Path}'", ex);
                }
            }
        }

        internal void MouseClick(MouseEventArgs e, out bool toCloseByDoubleClick)
        {
            IsClicking = false;
            toCloseByDoubleClick = false;
            if (Properties.Settings.Default.OpenItemWithOneClick)
            {
                OpenItem(e, ref toCloseByDoubleClick);
            }

            if (Properties.Settings.Default.OpenDirectoryWithOneClick &&
                ContainsMenu && (e == null || e.LeftButton == MouseButtonState.Pressed))
            {
                Log.ProcessStart(Path);
                if (!Properties.Settings.Default.StaysOpenWhenItemClicked)
                {
                    toCloseByDoubleClick = true;
                }
            }
        }

        internal void DoubleClick(MouseButtonEventArgs e, out bool toCloseByDoubleClick)
        {
            IsClicking = false;
            toCloseByDoubleClick = false;
            if (!Properties.Settings.Default.OpenItemWithOneClick)
            {
                OpenItem(e, ref toCloseByDoubleClick);
            }

            if (!Properties.Settings.Default.OpenDirectoryWithOneClick &&
                ContainsMenu && (e == null || e.LeftButton == MouseButtonState.Pressed))
            {
                Log.ProcessStart(Path);
                if (!Properties.Settings.Default.StaysOpenWhenItemClicked)
                {
                    toCloseByDoubleClick = true;
                }
            }
        }

        private void OpenItem(MouseEventArgs e, ref bool toCloseByOpenItem)
        {
            if (!ContainsMenu &&
                (e == null || e.LeftButton == MouseButtonState.Pressed))
            {
                ProcessStarted = true;
                string workingDirectory = System.IO.Path.GetDirectoryName(ResolvedPath);
                Log.ProcessStart(Path, string.Empty, false, workingDirectory, true, ResolvedPath);
                if (!Properties.Settings.Default.StaysOpenWhenItemClicked)
                {
                    toCloseByOpenItem = true;
                }
            }
        }
    }
}

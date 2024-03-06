using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace VisionAnalysis
{
    public class Tools
    {
        // P/Invoke 函數，用於釋放 GDI 資源
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }

    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private event Action<T> onAddItem, onRemoveItem;

        public ObservableRangeCollection() : base() { }
        public ObservableRangeCollection(
            Action<T> onAddItem, Action<T> onRemoveItem = null
            ) : base()
        {
            this.onAddItem += onAddItem;
            this.onRemoveItem += onRemoveItem;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var i in collection) Add(i);
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (index > Items.Count)
                throw new OverflowException(
                    "index: " + index + " >= Items.Count:" + Items.Count);
            else if (index == Items.Count)
                AddRange(collection);
            else
                for (int i = 0; i < collection.Count(); i++)
                    Insert(index + i, collection.ElementAt(i));
        }
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var i in collection)
                Items.Remove(i);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }
        public void RemoveRange(int index, int length)
        {
            if (index + length - 1 >= Items.Count)
                throw new OverflowException("length over");
            for (int i = length; i > 0; i--)
                Items.RemoveAt(index);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }
        public void Replace(T item)
        {
            ReplaceRange(new T[] { item });
        }
        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }
        public void MoveRange(int oldIndex, int length, int newIndex)
        {
            if (newIndex - oldIndex >= 0)
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    Move(oldIndex + i, newIndex + i);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    Move(oldIndex + i, newIndex + i);
                }

            }
        }
        public List<T> GetRange(int index, int length)
        {
            List<T> list = new List<T>();
            for (int i = index; i < index + length; i++)
            {
                list.Add(Items.ElementAt(i));
            }
            return list;
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            onAddItem?.Invoke(item);
        }
        protected override void RemoveItem(int index)
        {
            onRemoveItem?.Invoke(Items.ElementAt(index));
            base.RemoveItem(index);
        }
    }

    public class TVImoveByMouse
    {
        private bool isMoving = false;
        private object cut;
        private Action<object, object> onMove;

        public TVImoveByMouse(System.Windows.Window w, TreeView tv, Action<object, object> onUpdate)
        {
            w.MouseUp += Window_MouseUp;
            tv.MouseLeave += tv_MouseLeave;
            onMove += onUpdate;
        }

        public void tvi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                cut = element.DataContext;
                isMoving = true;
            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e) { isMoving = false; }
        private void tv_MouseLeave(object sender, MouseEventArgs e) { isMoving = false; }
        public void tvi_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;

            if (isMoving && element.DataContext != cut)
            {
                onMove?.Invoke(cut, element.DataContext);
            }
        }
    }

    public class PathSelector
    {
        #region File read/write...
        public enum PathRequest { SaveFile, ReadFile, SelectFolder }
        //filter format: myTxt|searchName 
        public static readonly string AllFile = "All Files|*.*";

        public static string getFilterStr(string[] filters)
        {
            string str = "";
            foreach (string s in filters)
            {
                str += str == string.Empty ? "" : "|";
                str += s;
            }
            return str;
        }
        public static string getUserSelectPath(PathRequest request)
        {
            return getUserSelectPath(request, new string[] { AllFile });
        }
        public static string getUserSelectPath(PathRequest request, string defualtPath)
        {
            return getUserSelectPath(request, defualtPath, new string[] { AllFile });
        }
        public static string getUserSelectPath(PathRequest request, string[] filterStr)
        {
            return getUserSelectPath(request, "", filterStr);
        }
        public static string getUserSelectPath(
            PathRequest request, string defualtPath, string[] filterStr)
        {
            string fstr = getFilterStr(filterStr);
            if (request == PathRequest.ReadFile)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                initialPath(openFileDialog, defualtPath);
                openFileDialog.Filter = fstr;
                if (openFileDialog.ShowDialog() == false)
                {
                    return null;
                }
                return openFileDialog.FileName;
            }

            if (request == PathRequest.SaveFile)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                initialPath(saveFileDialog, defualtPath);
                saveFileDialog.Filter = fstr;
                if (saveFileDialog.ShowDialog() == false)
                {
                    return null;
                }
                return saveFileDialog.FileName;
            }

            if (request == PathRequest.SelectFolder)
            {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog
                    = new System.Windows.Forms.FolderBrowserDialog();
                folderBrowserDialog.SelectedPath = defualtPath;
                if (folderBrowserDialog.ShowDialog()
                    == System.Windows.Forms.DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }

            }
            return null;
        }
        private static void initialPath(FileDialog fileDialog, string defualtPath)
        {
            if (File.Exists(defualtPath))
            {
                int index = defualtPath.LastIndexOf("\\");
                fileDialog.InitialDirectory = defualtPath.Substring(0, index);
                fileDialog.FileName = defualtPath.Substring(index + 1);
            }
            else if (Directory.Exists(defualtPath))
            {
                fileDialog.InitialDirectory = defualtPath;
            }
        }

        #endregion

    }

    public class WindowControlHelper
    {
        #region Window control
        public enum WindowLocation { NoneSet, Top, Bottom, Left, Right, OwnCenter }
        private class ClosingListenerBuilder
        {
            private System.Windows.Window window;
            public ClosingListenerBuilder(System.Windows.Window window)
            {
                this.window = window;
            }

            public void closing(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                window.Visibility = Visibility.Hidden;
            }
        }

        public static void WindowInitialShow(System.Windows.Window newW, System.Windows.Window owner, WindowLocation location)
        {
            if (newW.IsLoaded)
            {
                newW.Visibility = Visibility.Visible;
                setWindowLocation(newW, owner, location);
                newW.Focus();
            }
            else
            {
                WindowInitial(newW, owner, location);
                newW.Closing += new ClosingListenerBuilder(newW).closing;
                newW.Show();
            }
        }
        public static void WindowInitial(System.Windows.Window newW, System.Windows.Window owner, WindowLocation location)
        {
            newW.FontSize = WindowPreference.getCfgValue<int>(
                    WindowPreference.fontSize);
            setWindowLocation(newW, owner, location);
        }
        private static void setWindowLocation(System.Windows.Window newW, System.Windows.Window owner, WindowLocation location)
        {
            if (location == WindowLocation.NoneSet)
            {

            }
            else if (location == WindowLocation.Top && owner.Top - owner.Height > 0)
            {
                newW.Left = owner.Left;
                newW.Top = owner.Top - owner.Height;
            }
            else if (location == WindowLocation.Bottom && owner.Top + owner.Height < SystemParameters.WorkArea.Height)
            {
                newW.Left = owner.Left;
                newW.Top = owner.Top + owner.Height;
            }
            else if (location == WindowLocation.Left && owner.Left - newW.Width > 0)
            {
                newW.Left = owner.Left - newW.Width;
                newW.Top = owner.Top;
            }
            else if (location == WindowLocation.Right && owner.Left + owner.Width < SystemParameters.WorkArea.Width)
            {
                newW.Left = owner.Left + owner.Width;
                newW.Top = owner.Top;
            }
            else if (location == WindowLocation.OwnCenter)
            {
                newW.Owner = owner;
                newW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }
        #endregion
    }
}

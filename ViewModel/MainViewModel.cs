using GalaSoft.MvvmLight;
using BeckOff_Parser.Model;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BeckOff_Parser.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
        }
        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string text = "";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text == value)
                {
                    return;
                }

                text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }

        /// <summary>
            /// The <see cref="Lines" /> property's name.
            /// </summary>
        public const string LinesPropertyName = "Lines";

        private string[] lines ;

        /// <summary>
        /// Sets and gets the Lines property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string[] Lines
        {
            get
            {
                return lines;
            }

            set
            {
                if (lines == value)
                {
                    return;
                }

                lines = value;
                RaisePropertyChanged(LinesPropertyName);
            }
        }
        bool drop = false;
        public ICommand ConvertCommand
        {
            get
            {
                return new RelayCommand<DragEventArgs>(
                    (e) =>
                    {

                        lines = createLinesFromString();
                        if (lines == null)
                        {
                            MessageBox.Show("nothing in textBox");
                            return;
                        }
                        string ret = "[StructLayout(LayoutKind.Sequential, Pack = 0)]\n";
                        ret += "public class ";
                        try
                        {
                            bool start = false;
                            foreach (string line in lines)
                            {
                                if (line.Contains("END_TYPE"))
                                {
                                    ret += "\n" + "}";
                                }
                                else if (line.Contains("TYPE"))
                                {
                                    int st = line.IndexOf("TYPE") + 4;
                                    int en = line.IndexOf(":");
                                    ret += line.Substring(st, en - st) + "\n" + "{" + "\n";
                                }
                                else if (line.Contains("STRUCT"))
                                {
                                    start = true;
                                }
                                else if (start)
                                {
                                    ret += parseLine(line);
                                }
                                else if (line.Contains("END_STRUCT"))
                                {
                                    break;
                                }
                            }
                            Text = ret;

                        }

                        catch (Exception ex)
                        {

                            MessageBox.Show("somthing went wrong" + ex.Message);
                            return;
                        }
                            // Note that you can have more than one file.
                            ;

                    });
            }
        }

        private string[] createLinesFromString()
        {
            string[] ret = Text.Split('\n');
            return ret;
        }

        private string parseLine(string line)
        {
            string ret= "";
            int startIndex = line.IndexOf(": ");
            int endIndex = line.IndexOf(";");
            if (startIndex > -1 || endIndex > 0)
            {
                string type = line.Substring(startIndex + 1, endIndex - startIndex-1);
                type = deleteSpaces(type);
                string var = line.Substring(0, startIndex) + ";";
                var = deleteSpaces(var);
                if (type.Contains("ARRAY"))
                {
                    int startIndex2 = line.IndexOf("OF ");
                    int endIndex2 = line.IndexOf(";");
                    int startSize = line.IndexOf("[0..");
                    int endSize = line.IndexOf("]");
                    string size = line.Substring(startSize + 4, endSize - (startSize + 4));
                    string type2 = line.Substring(startIndex2 + 3, endIndex2 - (startIndex2 + 3));
                    type2 = deleteSpaces(type2);
                    string marshel = "\t[MarshalAs(UnmanagedType.ByValArray, SizeConst =" + size + ")]\n";
                    switch (type2)
                    {
                        case "BOOL":
                            ret = marshel + "\tpublic byte[] " + " " + var;
                            break;
                        case "WORD":
                            ret = marshel + "\tpublic ushort[] " + var;
                            break;
                        case "DWORD":
                            ret = marshel + "\tpublic uint[] " + var;
                            break;
                        case "SINT":
                            ret = marshel + "\tpublic sbyte[] " + var;
                            break;
                        case "INT":
                            ret = marshel + "\tpublic short[] " + " " + var;
                            break;
                        case "DINT":
                            ret = marshel + "\tpublic int[] " + var;
                            break;
                        case "LINT":
                            ret = marshel + "\tpublic long[] " + var;
                            break;
                        case "USINT":
                            ret = marshel + "\tpublic byte[] " + var;
                            break;
                        case "UINT":
                            ret = marshel + "\tpublic ushort[] " + var;
                            break;
                        case "UDINT":
                            ret = marshel + "\tpublic uint[] " + var;
                            break;
                        case "ULINT":
                            ret = marshel + "\tpublic ulong[] " + var;
                            break;
                        case "REAL":
                            ret = marshel + "\tpublic real[] " + var;
                            break;
                        case "DREAL":
                            ret = marshel + "\tdouble[] " + var;
                            break;

                    }
                }
                else
                {

                    switch (type)
                    {
                        case "BOOL":
                            ret = "\tpublic byte" + " " + var;
                            break;
                        case "WORD":
                            ret = "\tpublic ushort " + var;
                            break;
                        case "DWORD":
                            ret = "\tpublic uint " + var;
                            break;
                        case "SINT":
                            ret = "\tpublic sbyte " + var;
                            break;
                        case "INT":
                            ret = "\tpublic short" + " " + var;
                            break;
                        case "DINT":
                            ret = "\tpublic int " + var;
                            break;
                        case "LINT":
                            ret = "\tpublic long " + var;
                            break;
                        case "USINT":
                            ret = "\tpublic byte " + var;
                            break;
                        case "UINT":
                            ret = "\tpublic ushort " + var;
                            break;
                        case "UDINT":
                            ret = "\tpublic uint " + var;
                            break;
                        case "ULINT":
                            ret = "\tpublic ulong " + var;
                            break;
                        case "REAL":
                            ret = "\tpublic real " + var;
                            break;
                        case "DREAL":
                            ret = "\tdouble " + var;
                            break;

                    }
                }
            }
            return ret + "\n";
           
        }

        private string deleteSpaces(string var)
        {
            int i = 0;
            while(true)
            {
                if (var[i].Equals(' '))
                {
                    var = var.Substring(1);
                    i++;
                }
                else if (var[i] ==('\t'))
                {
                    i++;
                    var = var.Substring(1);
                }
                else break;
            }
            return var;
        }

        public ICommand DropFileCommand
        {
            get
            {
                return new RelayCommand<DragEventArgs>(

                    (e) =>
                    {
                        drop = true;
                        if (e.Data.GetDataPresent(DataFormats.FileDrop))
                        {
                            try
                            {
                                string createText ="";
                                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                                FileInfo f = new FileInfo(files[0]);

                                List<string> allowableFileTypes = new List<string>();
                                allowableFileTypes.AddRange(new string[] { ".txt" });
                                if(allowableFileTypes.Contains(f.Extension.ToLower()))
                                {
                                    if (File.Exists(f.FullName))
                                    {
                                        Text = "";
                                        // Create a file to write to.
                                        lines = System.IO.File.ReadAllLines(f.FullName);
                                        foreach(string line in lines)
                                        {
                                            Text += line;
                                            Text += "\n";
                                        }

                                    }
                                }
                             
                                
                            }


                            catch (Exception ex)
                            {

                                MessageBox.Show("Can't load");
                                return;
                            }
                            // Note that you can have more than one file.
                        }


                    });
            }
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}
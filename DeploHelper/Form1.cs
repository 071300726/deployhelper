using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DeployHelper
{
    public partial class Form1 : Form
    {
        private string basePath
        {
            get { return this.textBox3.Text.Trim(); }
        }
        private string targetBasePath = string.Empty;
        public Form1()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
            this.textBox3.Text = GetBasePath();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            targetBasePath = this.textBox2.Text.Trim();
            DeleteAll(targetBasePath);

            string[] lines = this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string line in lines)
            {
                string[] items = line.Trim(new char[] { '\t', ' ' }).Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                string sourceFilePath = Path.Combine(basePath, items[1], items[0]);

                if(items[1].StartsWith(@"src\Lib\"))
                {
                    //拷贝对应DLL
                    string dll = GetDllPath(sourceFilePath);
                    string targetPath = Path.Combine(targetBasePath, GetBinPath());

                    CopyFile(dll + ".dll", targetPath);
                    CopyFile(dll + ".pdb", targetPath);
                }
                else
                {
                    //直接拷贝文件
                    string targetPath = Path.Combine(targetBasePath, items[1]);

                    CopyFile(sourceFilePath, targetPath);
                }

            }
            MessageBox.Show("完成");
            System.Diagnostics.Process.Start("explorer.exe", targetBasePath);


        }

        private void DeleteAll(string path)
        {
            if(Directory.Exists(path)==false)
            {
                return;
            }

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(path, true);

        }
        private string GetBinPath()
        {
            switch (this.comboBox1.SelectedItem.ToString())
            {
                case "class.hujiang.com":return @"src\web\bin";
                case "mc.hujiang.com":return @"src\mc.hujiang.com\bin";
            }
            return string.Empty;
        }

        private string GetDllPath(string file)
        {
            string csproj = FindCSProj(new FileInfo(file).Directory.FullName);
            if(string.IsNullOrEmpty(csproj))
            {
                return null;
            }
            string projBasePath = new FileInfo(csproj).DirectoryName;

            XmlDocument csprojConfig = new XmlDocument();
            csprojConfig.Load(csproj); 
            XmlNamespaceManager manager = new XmlNamespaceManager(csprojConfig.NameTable);
            manager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            //<OutputPath>bin\Debug\</OutputPath>
            XmlNode outputNode = csprojConfig.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:OutputPath", manager);
            if(outputNode==null || string.IsNullOrEmpty(outputNode.InnerText))
            {
                return null;
            }

            //<AssemblyName>YesHJ.Class</AssemblyName>
            XmlNode assemblyNode = csprojConfig.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:AssemblyName", manager);
            if (assemblyNode == null || string.IsNullOrEmpty(assemblyNode.InnerText))
            {
                return null;
            }

            string dllPath = Path.Combine(projBasePath, outputNode.InnerText, assemblyNode.InnerText);

            return dllPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string FindCSProj(string path)
        {
            string[] files = Directory.GetFiles(path, "*.csproj");
            if(files.Length>0)
            {
                return files[0];
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                return dirInfo.Parent == null ? null : FindCSProj(dirInfo.Parent.FullName);
            }

        }


        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="targetPath"></param>
        private void CopyFile(string file,string targetPath)
        {
            if(Directory.Exists(targetPath)==false)
            {
                Directory.CreateDirectory(targetPath);
            }

            string fileName = file.Substring(file.LastIndexOf(@"\") + 1);
            string targetFile = Path.Combine(targetPath, fileName);

            File.Copy(file, targetFile, true);
            
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            SaveBasePath(this.textBox3.Text.Trim());
        }



        private void SaveBasePath(string str)
        {
            File.WriteAllText("config.txt", str);
        }

        private string GetBasePath()
        {
            try
            {
                return File.ReadAllText("config.txt");
            }
            catch(Exception ex)
            {
                return string.Empty;"
            }
        }

    }
}

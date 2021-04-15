using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectCoordinateMapping
{
    public class DataSave
    {
        public DataSave()
        {
            CreateFolder();
        }
        private void CreateFolder()
        {
            //Proje dizininde Data adında klasör oluşturur.
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Data");
        }
        public void dataSave(int stepCount,string tip, double x, double y, float z, string dosyaAdi1)
        {
            try
            {
                string _data = "Step Count:" + stepCount + "Type:" + tip + " x:" + x + " | y:" + y + " | z:" + z;
                string dosyaAdi;
                string yolEx;
                dosyaAdi = dosyaAdi1+".csv";
                StreamWriter sw = null;
                yolEx = System.IO.Directory.GetCurrentDirectory() + @"\Data\\" + dosyaAdi;
                sw = new StreamWriter(System.IO.Directory.GetCurrentDirectory() + @"\Data\\" + dosyaAdi, true, Encoding.Default);
                //Veriyi texte yazar.
                sw.WriteLine(_data);
                //sw.Flush();
                //sw.Write(gonderim);
                sw.Close();
            }
            catch (Exception hata)
            {
            }
        }
    }
}

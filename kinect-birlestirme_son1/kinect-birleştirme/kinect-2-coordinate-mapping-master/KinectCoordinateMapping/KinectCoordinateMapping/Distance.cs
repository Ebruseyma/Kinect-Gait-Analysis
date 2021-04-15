using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KinectCoordinateMapping
{
   public  class Distance

    {
        public Distance()
        {
            CreateFolder();
        }
        private void CreateFolder()
        {
            //Proje dizininde Data adında klasör oluşturur.
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\");
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Data");
        }

        public void dataSave(double distance,string time)
        {
            try
            {
                string _data = "Time:" + time+" | Distance:" + distance ;
                string dosyaAdi;
                string yolEx;
                dosyaAdi = "distance.txt";
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;
using OpenCvSharp.Extensions;

namespace WindowsFormsApplication4
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Bitmap imgObject;
        Bitmap imgScene;
        SettingForm form = new SettingForm();
        ImageForm imageForm = new ImageForm();

        private void стартToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                labelstrip.Text = "Получение изображения";


                // берем изображение из PictureBox и переводим bitmap в Mat
                Bitmap image1b = (Bitmap)objectPictureBox.Image;
                Bitmap image2b = (Bitmap)scenePictureBox.Image;
                Mat image1 = BitmapConverter.ToMat(image1b);
                Mat image2 = BitmapConverter.ToMat(image2b);

                //создаем SURF переменную
                SURF surf = new SURF(form.getThreshold(), form.getOctaves(), form.getOctavesLayer(), form.getDecriptors(), false);

                labelstrip.Text = "Нахождение особых точек и их дескриптеров";

                //создаем по 2 переменных для записи ключевых точек и дескриптеров
                Mat descriptors1 = new Mat();
                Mat descriptors2 = new Mat();
                KeyPoint[] points1, points2;

                //находим особые точки
                points1 = surf.Detect(image1);
                points2 = surf.Detect(image2);


                //нахождение дескрипторов точек
                surf.Compute(image1, ref points1, descriptors1);
                surf.Compute(image2, ref points2, descriptors2);

                //матчим массивы дескрипторов
                FlannBasedMatcher matcher = new FlannBasedMatcher();
                DMatch[] matches;
                matches = matcher.Match(descriptors1, descriptors2);



                //Вычисление максимального и минимального расстояния среди всех дескрипторов
                double max_dist = 0; double min_dist = 100;

                for (int i = 0; i < descriptors1.Rows; i++)
                {
                    double dist = matches[i].Distance;
                    if (dist < min_dist) min_dist = dist;
                    if (dist > max_dist) max_dist = dist;
                }

                labelstrip.Text = "Отбор точек";

                // Отобрать только хорошие матчи, расстояние меньше чем 3 * min_dist

                List<DMatch> good_matches = new List<DMatch>();

                for (int i = 0; i < matches.Length; i++)
                {
                    if (matches[i].Distance < form.getMinDinst() * min_dist)
                    {
                        good_matches.Add(matches[i]);
                    }
                }



                Mat image3 = new Mat();
                Cv2.DrawMatches(image1, points1, image2, points2, good_matches, image3, Scalar.RandomColor(), Scalar.RandomColor(), null, DrawMatchesFlags.NotDrawSinglePoints);

                labelstrip.Text = "Локализация объекта";

                //Использование гомографии
                // Локализация объектов

                Point2f[] vector = new Point2f[good_matches.Count];
                Point2d[] vector1 = new Point2d[good_matches.Count];
                Point2d[] vector2 = new Point2d[good_matches.Count];
                for (int i = 0; i < good_matches.Count; i++)
                {
                    vector[i] = points1[good_matches[i].QueryIdx].Pt;
                    vector1[i].X = vector[i].X;
                    vector1[i].Y = vector[i].Y;
                    vector[i] = points2[good_matches[i].TrainIdx].Pt;
                    vector2[i].X = vector[i].X;
                    vector2[i].Y = vector[i].Y;
                }

                Mat H = Cv2.FindHomography(vector1, vector2, HomographyMethod.Ransac);

                //Получить "углы" изображения с целевым объектом
                Point2d[] vector3 = new Point2d[4];
                vector3[0].X = 0; vector3[0].Y = 0;
                vector3[1].X = image1.Cols; vector3[1].Y = 0;
                vector3[2].X = image1.Cols; vector3[2].Y = image1.Rows;
                vector3[3].X = 0; vector3[3].Y = image1.Rows;
                Point2d pointtest; pointtest.X = 0; pointtest.Y = 0;


                List<Point2d> vector4 = new List<Point2d>() { pointtest, pointtest, pointtest, pointtest };

                //Отобразить углы целевого объекта, используя найденное преобразование, на сцену
                Cv2.PerspectiveTransform(InputArray.Create(vector3), OutputArray.Create(vector4), H); //?

                Point2d point1;
                Point2d point2;
                int k;
                for (int i = 0; i < 4; i++)
                {
                    if (i == 3) { k = 0; } else { k = i + 1; }
                    point1.X = vector4[i].X + image1.Cols;
                    point1.Y = vector4[i].Y + 0;
                    point2.X = vector4[k].X + image1.Cols;
                    point2.Y = vector4[k].Y + 0;
                    Cv2.Line(image3, point1, point2, Scalar.RandomColor(), 4);
                }

                labelstrip.Text = "Объект локализован";

                Bitmap image3b = BitmapConverter.ToBitmap(image3);
                imageForm.getresultimage(image3b);

                imageForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
                labelstrip.Text = "Произошла ошибка";
            }
        }

        private void объектToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelstrip.Text = "Загрузка изображения";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.JPEG)|*.BMP;*.JPG;*.GIF;*.JPEG|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap test = new Bitmap(dialog.FileName);
                    imgObject = test;
                    objectPictureBox.Image = imgObject;
                    objectPictureBox.Update();
                    labelstrip.Text = "Изображение загружено";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    labelstrip.Text = "Произошла ошибка";
                }
            }
            else { labelstrip.Text = "Отмена загрузки изображения"; }
        }

        private void сценаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelstrip.Text = "Загрузка изображения";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.JPEG)|*.BMP;*.JPG;*.GIF;*.JPEG|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap test = new Bitmap(dialog.FileName);
                    imgScene = test;
                    scenePictureBox.Image = imgScene;
                    scenePictureBox.Update();
                    labelstrip.Text = "Изображение загружено";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    labelstrip.Text = "Произошла ошибка";
                }
            }
            else { labelstrip.Text = "Отмена загрузки изображения"; }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form.ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

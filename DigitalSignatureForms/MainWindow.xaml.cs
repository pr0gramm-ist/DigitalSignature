using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using DigitalSignature.Curves;
using DigitalSignature;
using Microsoft.Win32;
using System.Collections;

namespace DigitalSignatureForms
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EdwardsCurve edwardsCurve = null;
        public EdwardsCurvePoint edwardsCurvePoint = null;
        public VeryBigInteger key = null;
        public BitArray[] sign = null;
        public byte[] data = null;
        public MainWindow()
        {
            InitializeComponent();
            curveSelection.ItemsSource = new EdwardsCurve[]
            {
                EdwardsCurve.id_tc26_gost_3410_2012_256_paramSetA(),
                EdwardsCurve.id_tc26_gost_3410_2012_512_paramSetC()
            };
        }

        private void generatePoint_Click(object sender, RoutedEventArgs e)
        {
            if(edwardsCurve is null)
            {
                MessageBox.Show("Сначала выберите кривую из возможных");
                return;
            }

            loading.Visibility = Visibility.Visible;

            edwardsCurvePoint = edwardsCurve.GeneratePoint();
            pointX.Text = edwardsCurvePoint.x.ToString();
            pointY.Text = edwardsCurvePoint.y.ToString();

            loading.Visibility = Visibility.Hidden;

        }

        private void curveSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            edwardsCurve = (EdwardsCurve)curveSelection.SelectedItem;
        }

        private void pointFromGOST_Click(object sender, RoutedEventArgs e)
        {
            if (edwardsCurve is null)
            {
                MessageBox.Show("Сначала выберите кривую из возможных");
                return;
            }

            if(edwardsCurve == EdwardsCurve.id_tc26_gost_3410_2012_256_paramSetA())
            {
                edwardsCurvePoint = EdwardsCurvePoint.id_tc26_gost_3410_2012_256_paramSetA();
                pointX.Text = edwardsCurvePoint.x.ToString();
                pointY.Text = edwardsCurvePoint.y.ToString();
            }
            else if(edwardsCurve == EdwardsCurve.id_tc26_gost_3410_2012_512_paramSetC())
            {
                edwardsCurvePoint = EdwardsCurvePoint.id_tc26_gost_3410_2012_512_paramSetC();
                pointX.Text = edwardsCurvePoint.x.ToString();
                pointY.Text = edwardsCurvePoint.y.ToString();
            }
            else
            {
                edwardsCurvePoint = null;
                pointX.Text = "";
                pointY.Text = "";
            }
        }

        private async void signMessage_Click(object sender, RoutedEventArgs e)
        {
            if(key is null)
            {
                MessageBox.Show("Сначала выберите или сгенерируйте ключ");
                return;
            }

            if (edwardsCurve is null)
            {
                MessageBox.Show("Сначала выберите кривую");
                return;
            }

            if (edwardsCurvePoint is null)
            {
                MessageBox.Show("Сначала выберите или сгенерируйте точку");
                return;
            }

            if (data is null)
            {
                MessageBox.Show("Выберите файл, который хотите подписать");
                return;
            }

            loading.Visibility = Visibility.Visible;

            var sign = await GOST3410_2018.SignAMessageAsync(data, edwardsCurvePoint, key);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "sign files (*.sign)|*.sign";
            var answer = saveFileDialog.ShowDialog();
            if (answer == true)
            {
                string filename = saveFileDialog.FileName;
                using (var sw = new StreamWriter(filename))
                {
                    sw.WriteLine($"{edwardsCurve.e.Value} {edwardsCurve.d.Value} {edwardsCurve.p}");

                    sw.WriteLine($"{edwardsCurvePoint.x.Value} {edwardsCurvePoint.y.Value}");

                    var Q = edwardsCurvePoint * key;
                    sw.WriteLine($"{Q.x.Value} {Q.y.Value}");

                    for (int i = 0; i < sign[0].Length; i++)
                    {
                        var bit = 0;
                        if(sign[0][i])
                        {
                            bit = 1;
                        }
                        sw.Write(bit);
                    }
                    sw.WriteLine();
                    for (int i = 0; i < sign[1].Length; i++)
                    {
                        var bit = 0;
                        if (sign[1][i])
                        {
                            bit = 1;
                        }
                        sw.Write(bit);
                    }
                }
                signFile.Text = filename;
            }

            loading.Visibility = Visibility.Hidden;
        }

        private void generateKey_Click(object sender, RoutedEventArgs e)
        {
            if (edwardsCurve is null)
            {
                MessageBox.Show("Сначала выберите кривую");
                return;
            }

            if (edwardsCurvePoint is null)
            {
                MessageBox.Show("Сначала выберите или сгенерируйте точку");
                return;
            }

            key = VeryBigInteger.NextRandomNumber() % edwardsCurvePoint.Order;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "key files (*.key)|*.key";
            var answer = saveFileDialog.ShowDialog();
            if (answer == true)
            {
                string filename = saveFileDialog.FileName;
                using(var sw = new StreamWriter(filename))
                {
                    sw.WriteLine(key);
                }
                keyFile.Text = filename;
            }
        }

        private void keyFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "key files (*.key)|*.key";
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                using (var sr = new StreamReader(filename))
                {
                    key = new VeryBigInteger(sr.ReadLine());
                }

                keyFile.Text = filename;
            }
        }

        private void downloadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                using (var sr = new StreamReader(filename))
                {
                    data = File.ReadAllBytes(filename);
                }

                dataFile.Text = filename;
            }
        }

        private async void checkSign_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (data is null)
            {
                MessageBox.Show("Выберите файл, подпись которого хотите проверить");
                return;
            }

            EdwardsCurvePoint Q;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                loading.Visibility = Visibility.Visible;
                string filename = openFileDialog.FileName;
                using (var sr = new StreamReader(filename))
                {
                    var curveNumbers = sr.ReadLine().Split(' ');
                    var curve = new EdwardsCurve(new VeryBigInteger(curveNumbers[0]), new VeryBigInteger(curveNumbers[1]), new VeryBigInteger(curveNumbers[2]));

                    if (curve == EdwardsCurve.id_tc26_gost_3410_2012_256_paramSetA())
                    {
                        edwardsCurve = EdwardsCurve.id_tc26_gost_3410_2012_256_paramSetA();
                    }
                    else if (curve == EdwardsCurve.id_tc26_gost_3410_2012_512_paramSetC())
                    {
                        edwardsCurve = EdwardsCurve.id_tc26_gost_3410_2012_512_paramSetC();
                    }
                    else
                    {
                        edwardsCurve = curve;
                    }

                    var selectedIndex = 0;
                    foreach (var curveElem in curveSelection.ItemsSource)
                    {
                        if ((EdwardsCurve)curveElem == edwardsCurve)
                        {
                            curveSelection.SelectedIndex = selectedIndex;
                        }
                        selectedIndex++;
                    }

                    var pointNumbers = sr.ReadLine().Split(' ');
                    var x = new FmodElement(new VeryBigInteger(pointNumbers[0]), edwardsCurve.p);
                    var y = new FmodElement(new VeryBigInteger(pointNumbers[1]), edwardsCurve.p);
                    var P = new EdwardsCurvePoint(x, y, edwardsCurve);
                    if (P == EdwardsCurvePoint.id_tc26_gost_3410_2012_256_paramSetA())
                    {
                        edwardsCurvePoint = EdwardsCurvePoint.id_tc26_gost_3410_2012_256_paramSetA();
                    }
                    else if (P == EdwardsCurvePoint.id_tc26_gost_3410_2012_512_paramSetC())
                    {
                        edwardsCurvePoint = EdwardsCurvePoint.id_tc26_gost_3410_2012_512_paramSetC();
                    }
                    else
                    {
                        edwardsCurvePoint = P;
                    }

                    pointX.Text = x.ToString();
                    pointY.Text = y.ToString();

                    pointNumbers = sr.ReadLine().Split(' ');
                    x = new FmodElement(new VeryBigInteger(pointNumbers[0]), edwardsCurve.p);
                    y = new FmodElement(new VeryBigInteger(pointNumbers[1]), edwardsCurve.p);
                    Q = new EdwardsCurvePoint(x, y, edwardsCurve);

                    sign = new BitArray[2];
                    var numbers = sr.ReadLine();
                    sign[0] = new BitArray(numbers.Length);
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        sign[0][i] = numbers[i] == '1';
                    }
                    numbers = sr.ReadLine();
                    sign[1] = new BitArray(numbers.Length);
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        sign[1][i] = numbers[i] == '1';
                    }
                }

                var result = await GOST3410_2018.CheckSignAsync(data, sign, edwardsCurvePoint, Q);

                if (result)
                {
                    checkResult.Text = "Подпись валидна";
                }
                else
                {
                    checkResult.Text = "Подпись не валидна";
                }

                signFile.Text = filename;

                loading.Visibility = Visibility.Hidden;

            }
        }
    }
}

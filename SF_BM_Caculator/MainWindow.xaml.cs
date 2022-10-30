using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.ObjectModel;
using System.Windows.Data;
using LiveCharts.Wpf;


namespace SF_BM_Caculator
{
    public partial class MainWindow : Window
    {
        // We will create a 'Dictionary Collection' to dynamically
        // create a 'Textbox Control' and a 'Label Control'.
        Dictionary<string, NumericUpDown> hole_NumObject = new Dictionary<string, NumericUpDown>();     // Hole Textbox Collection
        Dictionary<string, Label> hole_LblObject = new Dictionary<string, Label>();                     // Hole Label Collection
        Dictionary<string, NumericUpDown> self_NumObject = new Dictionary<string, NumericUpDown>();     // Self Textbox Collection
        Dictionary<string, Label> self_LblObject = new Dictionary<string, Label>();                     // Hole Label Collection

        // This is necessary to put it in the chart.
        List<double> HoleSF_List = new List<double>();      // Hole SF List
        List<double> HoleBM_List = new List<double>();      // Hole BM List
        List<double> SelfSF_List = new List<double>();      // Self SF List
        List<double> SelfBM_List = new List<double>();      // Self SF List
        List<double> ResultSF_List = new List<double>();    // Result SF List
        List<double> ResultBM_List = new List<double>();    // Result SF List

        // This is a grid defined in xaml for placing dynamically generated controls.
        Grid[] grid = new Grid[2];  // Hole Grid and Self Grid.

        //This is observableCollection variable for dynamic binding data to the DataGrid.
        ObservableCollection<CalcValue> gridHoleContent = new ObservableCollection<CalcValue>();    
        ObservableCollection<CalcValue> gridSelfContent = new ObservableCollection<CalcValue>();
        ObservableCollection<CalcValue> gridResultContent = new ObservableCollection<CalcValue>();

        // This is boolean variable that decide whether caculation progress performed successfully or not.
        bool holeFlag, selfFlag;    // Hole caculation and Self caculation


        string[] stHole = { "Interval", "oa_Span", "Breadth", "Thickness", "BearingLeft", "BearingRight",
                "LenHole", "HeightHole", "DistLeftHole", "DenistyHole" };

        string[] stSelf = { "Interval", "oa_Span", "Breadth", "Thickness", "BearingLeft", "BearingRight", "Self" };

        public enum State
        {
            Hole,  Self
        }

        public MainWindow()
        {
            InitializeComponent();

            //window position, window size
            double width = SystemParameters.FullPrimaryScreenWidth;
            double height = SystemParameters.FullPrimaryScreenHeight;
            this.Width = width;
            this.Height = height;
            this.Top = (height - this.Height) / 2;
            this.Left = (width - this.Width) / 2;
                        ResizeMode = ResizeMode.NoResize;
             
            grid[0] = gridHole;
            grid[1] = gridSelf;

            //Creating Label and Textbox control
            for (int i = 0; i < stHole.Length; i++)
            {
                hole_NumObject[stHole[i]] = NumObjectGenerator((int)State.Hole, i);
                hole_LblObject[stHole[i]] = LabelObjectGenerator(stHole[i], (int)State.Hole, i);
            }

            for (int i = 0; i < stSelf.Length; i++)
            {
                self_NumObject[stSelf[i]] = NumObjectGenerator((int)State.Self, i);
                self_LblObject[stSelf[i]] = LabelObjectGenerator(stSelf[i], (int)State.Self, i);
            }

            //Referencing ObservableCollection
            holeDataGrid.ItemsSource = gridHoleContent;
            selfDataGrid.ItemsSource = gridSelfContent;
            resultDataGrid.ItemsSource = gridResultContent;

        }

        //Textbox generator
        public NumericUpDown NumObjectGenerator(int gridNum, int row)
        {
            NumericUpDown tmpObject = new NumericUpDown();

            tmpObject.VerticalAlignment = VerticalAlignment.Center;
            tmpObject.HideUpDownButtons = true;
            tmpObject.Background = new SolidColorBrush(Colors.White);
            tmpObject.FontSize = 16;
            tmpObject.Margin = new Thickness(0, 2, 0, 2);

            DropShadowBitmapEffect dropShadowEffect = new DropShadowBitmapEffect();
            Color myShadowColor = new Color();
            myShadowColor.ScA = 1;
            myShadowColor.ScB = 0;
            myShadowColor.ScG = 0;
            myShadowColor.ScR = 0;
            dropShadowEffect.Color = new Color();
            // Set the direction of where the shadow is cast to 320 degrees.
            dropShadowEffect.Direction = 320;
            // Set the depth of the shadow being cast.
            dropShadowEffect.ShadowDepth = 3;
            // Set the shadow softness to the maximum (range of 0-1).
            dropShadowEffect.Softness = 0.4;
            // Set the shadow opacity to half opaque or in other words - half transparent.
            // The range is 0-1.
            dropShadowEffect.Opacity = 0.5;
            // Apply the bitmap effect to the Button.
            tmpObject.BitmapEffect = dropShadowEffect;

            grid[gridNum].Children.Add(tmpObject);
            Grid.SetRow(tmpObject, row);
            Grid.SetColumn(tmpObject, 1);

            return tmpObject;
        }

        //Label control generator
        public Label LabelObjectGenerator(string lblText, int gridNum, int row)
        {
            Label tmpObject = new Label();

            tmpObject.Content = lblText;
            tmpObject.FontSize = 18;
            tmpObject.Foreground = Brushes.White;

            tmpObject.VerticalAlignment = VerticalAlignment.Center;
            tmpObject.HorizontalContentAlignment = HorizontalAlignment.Right;

            grid[gridNum].Children.Add(tmpObject);
            Grid.SetRow(tmpObject, row);
            Grid.SetColumn(tmpObject, 0);

            return tmpObject;
        }

        //Checking textbox value
        private bool InputCheck(int stateNum)
        {
            if (stateNum == 0 )
            {
                for(int i = 0; i < hole_NumObject.Count; i++)
                {
                    if (hole_NumObject[stHole[i]].Value == null) return false;
                }
            }
            else
            {
                for (int i = 0; i < self_NumObject.Count; i++)
                {
                    if (self_NumObject[stSelf[i]].Value == null) return false;
                }
            }
            return true;
        }

        private void Hole_CalcBtn_Click(object sender, RoutedEventArgs e)
        {
            HoleCalc();     
        }

        private void HoleCalc()
        {
            holeFlag = false;
            gridHoleContent.Clear();

            // initial variable
            int n_Interval;
            double d_oa_Span;
            double d_Breadth;
            double d_Thickness;
            double d_Bearing_Left;
            double d_Bearing_Right;
            double d_Len_Hole;
            double d_Height_Hole;
            double d_Dist_Left_Hole;
            double d_Density_Hole;

            //middle variable
            double d_Volume_Hole;
            double d_UDL_Hole_Total;
            double d_UDL_Hole;
            double d_Total_Hole;
            double d_dx;
            double d_Dist_cc;
            double d_OTM_Rt_Sup;
            double d_R_left;
            double d_R_right;
            double d_R_left_pres;
            double d_R_right_pres;

            //result variable
            double d_ix;
            double d_Rlx;
            double d_HoleSF_x;
            double d_Rrx;
            double d_SF;
            double d_Rlx_m;
            double d_Hole_BM_x;
            double d_Rrx_m;
            double d_BM;

            //Erasing SF,BM array
            HoleSF_List.Clear();
            HoleBM_List.Clear();

            //checking basic datas
            if (!InputCheck((int)State.Hole))
            {
                MessageBox.Show("Please enter the \"Hole\" basic data correctly", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //input basic datas
            n_Interval = (int)hole_NumObject["Interval"].Value;
            d_oa_Span = (double)hole_NumObject["oa_Span"].Value;
            d_Breadth = (double)hole_NumObject["Breadth"].Value;
            d_Thickness = (double)hole_NumObject["Thickness"].Value;
            d_Bearing_Left = (double)hole_NumObject["BearingLeft"].Value;
            d_Bearing_Right = (double)hole_NumObject["BearingRight"].Value;
            d_Len_Hole = (double)hole_NumObject["LenHole"].Value;
            d_Height_Hole = (double)hole_NumObject["HeightHole"].Value;
            d_Dist_Left_Hole = (double)hole_NumObject["DistLeftHole"].Value;
            d_Density_Hole = (double)hole_NumObject["DenistyHole"].Value;

            //Middle values caculation
            d_Volume_Hole = d_Thickness * d_Len_Hole * d_Height_Hole / 1000000000;
            d_UDL_Hole_Total = d_Density_Hole * d_Volume_Hole;
            d_UDL_Hole = 1000 * d_UDL_Hole_Total / d_Len_Hole;
            d_Total_Hole = d_UDL_Hole * d_Len_Hole / 1000;
            d_dx = d_oa_Span / n_Interval;
            d_Dist_cc = d_oa_Span - (d_Bearing_Left + d_Bearing_Right) / 2;
            d_OTM_Rt_Sup = d_Total_Hole * (d_oa_Span - d_Dist_Left_Hole - d_Len_Hole / 2 - d_Bearing_Right / 2);
            d_R_left = d_OTM_Rt_Sup / d_Dist_cc;
            d_R_right = d_Total_Hole - d_R_left;
            d_R_left_pres = d_R_left / d_Bearing_Left;
            d_R_right_pres = d_R_right / d_Bearing_Right;

            //Result values caculation
            for (int i = 0; i <= n_Interval; i++)
            {
                d_ix = i * d_oa_Span / n_Interval;                          // ix

                if (d_ix < d_Bearing_Left)
                {
                    d_Rlx = d_R_left_pres * d_ix;                           // Rlx
                }
                else
                {
                    d_Rlx = d_R_left;
                }

                if (d_ix < d_Dist_Left_Hole)
                {
                    d_HoleSF_x = 0;                                         // HostSF_x
                }
                else
                {
                    if (d_ix < (d_Dist_Left_Hole + d_Len_Hole))
                    {
                        d_HoleSF_x = -d_UDL_Hole * (d_ix - d_Dist_Left_Hole) / 1000;
                    }
                    else
                    {
                        d_HoleSF_x = -d_Total_Hole;
                    }
                }

                if (d_ix < (d_oa_Span - d_Bearing_Right))
                {
                    d_Rrx = 0;                                              // Rrx
                }
                else
                {
                    d_Rrx = d_R_right_pres * (d_ix - d_oa_Span + d_Bearing_Right);
                }

                d_SF = d_Rlx + d_HoleSF_x + d_Rrx;                          // SF

                if (d_ix < d_Bearing_Left)
                {
                    d_Rlx_m = d_R_left_pres * Math.Pow(d_ix, 2) / 2000;     // Rlx_m
                }
                else
                {
                    d_Rlx_m = d_R_left * (d_ix / 1000 - d_Bearing_Left / 2000);
                }

                if (d_ix < d_Dist_Left_Hole)
                {
                    d_Hole_BM_x = 0;                                        // Hole_BM_x
                }
                else
                {
                    if (d_ix < (d_Dist_Left_Hole + d_Len_Hole))
                    {
                        d_Hole_BM_x = -d_UDL_Hole * Math.Pow((d_ix - d_Dist_Left_Hole) / 1000, 2) / 2;
                    }
                    else
                    {
                        d_Hole_BM_x = -d_Total_Hole * (d_ix - d_Dist_Left_Hole - d_Len_Hole / 2) / 1000;
                    }
                }

                //
                if (d_ix < (d_oa_Span - d_Bearing_Right))
                {
                    d_Rrx_m = 0;                                            // Rfx_m
                }
                else
                {
                    d_Rrx_m = d_R_right_pres * Math.Pow(d_ix - d_oa_Span + d_Bearing_Right, 2) / 2000;
                }

                d_BM = d_Rlx_m + d_Hole_BM_x + d_Rrx_m;                     // BM
                    
                HoleSF_List.Add(d_SF);
                HoleBM_List.Add(d_BM);
                gridHoleContent.Add(new CalcValue() { Interval_Value = i, SF_Value = d_SF, BM_Value = d_BM });
            }

            var series1 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "SF Value",
                Values = new LiveCharts.ChartValues<double>(HoleSF_List)
            };

            var series2 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "BM Value",
                Values = new LiveCharts.ChartValues<double>(HoleBM_List)
            };

            // display the series in the chart control

            holeChart.Series.Clear();
            holeChart.Series.Add(series1);      // Put value of SF into array
            holeChart.Series.Add(series2);      // Put value of BM into array
            holeFlag = true;
        }

        private void Hole_ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < hole_NumObject.Count; i++)
            {
                hole_NumObject[stHole[i]].Value = null;
            }
            gridHoleContent.Clear();
            
        }

        private void Self_CalcBtn_Click(object sender, RoutedEventArgs e)
        {
            SelfCalc();
        }

        private void SelfCalc()
        {   
            selfFlag = false;           //Calculation progress judgment variable

            gridSelfContent.Clear();

            // basic variable declaration
            int n_Interval;
            double d_oa_Span;
            double d_Breadth;
            double d_Thickness;
            double d_Bearing_Left;
            double d_Bearing_Right;
            double d_Self;

            //middle variable declaration
            double d_Volume_Self;
            double d_UDL_Self_Total;
            double d_UDL_Self_m;
            double d_dx_Self;
            double d_Dist_cc_Self;
            double d_OTM_Rt_Sup_Self;
            double d_R_left_Self;
            double d_R_right_Self;
            double d_R_left_pres_Self;
            double d_R_right_pres_Self;

            //result variable declaration
            double d_ix;
            double d_RlxSelf;
            double d_SelfSF_x;
            double d_RrxSelf;
            double d_SFSelf;
            double d_RlxSelf_m;
            double d_Self_BM_x;
            double d_RrxSelf_m;
            double d_BMSelf;

            //Erasing SF,BM array
            SelfSF_List.Clear();
            SelfBM_List.Clear();

            //checking basic datas
            if (!InputCheck((int)State.Self))
            {
                MessageBox.Show("Please enter the \"Self\" basic data correctly", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //input basic datas
            n_Interval = (int)self_NumObject["Interval"].Value;
            d_oa_Span = (double)self_NumObject["oa_Span"].Value;
            d_Breadth = (double)self_NumObject["Breadth"].Value;
            d_Thickness = (double)self_NumObject["Thickness"].Value;
            d_Bearing_Left = (double)self_NumObject["BearingLeft"].Value;
            d_Bearing_Right = (double)self_NumObject["BearingRight"].Value;
            d_Self = (double)self_NumObject["Self"].Value;

            //Middle values caculation
            d_Volume_Self = d_Thickness * d_oa_Span * d_Breadth / 1000000000;
            d_UDL_Self_Total = d_Volume_Self * d_Self;
            d_UDL_Self_m = d_UDL_Self_Total / d_oa_Span;
            d_dx_Self = d_oa_Span / n_Interval;
            d_Dist_cc_Self = d_oa_Span - (d_Bearing_Left + d_Bearing_Right) / 2;
            d_OTM_Rt_Sup_Self = d_UDL_Self_Total * (d_oa_Span - d_Bearing_Right) / 2;
            d_R_left_Self = d_OTM_Rt_Sup_Self / d_Dist_cc_Self;
            d_R_right_Self = d_UDL_Self_Total - d_R_left_Self;
            d_R_left_pres_Self = d_R_left_Self / d_Bearing_Left;
            d_R_right_pres_Self = d_R_right_Self / d_Bearing_Left;

            //Result values caculation
            for (int i = 0; i <= n_Interval; i++)
            {
                d_ix = i * d_oa_Span / n_Interval;                              // ix

                if (d_ix < d_Bearing_Left)
                {
                    d_RlxSelf = d_R_left_pres_Self * d_ix;                      // RlxSelf
                }
                else
                {
                    d_RlxSelf = d_R_left_Self;
                }

                d_SelfSF_x = -d_UDL_Self_Total * i / n_Interval;                // SelfSF_x

                if (d_ix < (d_oa_Span - d_Bearing_Right))
                {
                    d_RrxSelf = 0;                                              // RrxSelf
                }
                else
                {
                    d_RrxSelf = d_R_right_pres_Self * (d_ix - d_oa_Span + d_Bearing_Right);
                }

                d_SFSelf = d_RlxSelf + d_SelfSF_x + d_RrxSelf;                  // SFSelf

                if (d_ix < d_Bearing_Left)
                {
                    d_RlxSelf_m = d_R_left_pres_Self * Math.Pow(d_ix, 2) / 2000;    // RlxSelf_m
                }
                else
                {
                    d_RlxSelf_m = d_R_left_Self * (d_ix / 1000 - d_Bearing_Left / 2000);
                }

                d_Self_BM_x = -d_UDL_Self_m * Math.Pow(d_ix, 2) / 2000;         // Self_BM_x

                if (d_ix < (d_oa_Span - d_Bearing_Right))
                {
                    d_RrxSelf_m = 0;                                            // RrxSelf_m
                }
                else
                {
                    d_RrxSelf_m = d_R_right_pres_Self * Math.Pow(d_ix - d_oa_Span + d_Bearing_Left, 2) / 2000;
                }

                d_BMSelf = d_RlxSelf_m + d_Self_BM_x + d_RrxSelf_m;             // BMSelf_m

                SelfSF_List.Add(d_SFSelf);              // Put value of SF into array
                SelfBM_List.Add(d_BMSelf);              // Put value of BM into array
                gridSelfContent.Add(new CalcValue() { Interval_Value = i, SF_Value = d_SFSelf, BM_Value = d_BMSelf });

            }

            //Put value of SF and BM in series1 variable and series2 variable 
            var series1 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "SF Value",
                Values = new LiveCharts.ChartValues<double>(SelfSF_List)
            };

            var series2 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "BM Value",
                Values = new LiveCharts.ChartValues<double>(SelfBM_List)
            };

            // display the series in the chart control
            selfChart.Series.Clear();
            selfChart.Series.Add(series1);      // Put value of SF into array
            selfChart.Series.Add(series2);      // Put value of BM into array

            selfFlag = true;              //Calculation progress judgment variable
        }

        private void Self_ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            // Erasing TextBox and DataGrid
            for (int i = 0; i < self_NumObject.Count; i++)
            {
                self_NumObject[stSelf[i]].Value = null;
            }
            gridSelfContent.Clear();

        }


        private void Result_CalcBtn_Click(object sender, RoutedEventArgs e)
        {
            gridResultContent.Clear();

            HoleCalc();
            SelfCalc();

            // If Bot of Hole and Self caculation is false, will return.
            if (holeFlag == false || selfFlag == false)
            {
                holeFlag = false;
                selfFlag = false;
                return;
            }

            // If If Interval value of Hole and Interval value of Self is different, will return.
            if (gridHoleContent.Count != gridSelfContent.Count)
            {
                MessageBox.Show("Please enter the same Interval Number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResultSF_List.Clear();
            ResultBM_List.Clear();

            for (int i = 0; i < gridHoleContent.Count; i++)
            {
                double resultSF_Value = gridHoleContent.ElementAt(i).SF_Value + gridSelfContent.ElementAt(i).SF_Value;
                double resultBM_Value = gridHoleContent.ElementAt(i).BM_Value + gridSelfContent.ElementAt(i).BM_Value;
                gridResultContent.Add(new CalcValue()
                {
                    Interval_Value = i,
                    SF_Value = resultSF_Value,
                    BM_Value = resultBM_Value
                });

                ResultSF_List.Add(resultSF_Value);
                ResultBM_List.Add(resultBM_Value);
            }

            // Put value of SF and BM in series1 variable and series2 variable
            var series1 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "SF Value",
                Values = new LiveCharts.ChartValues<double>(ResultSF_List)
            };

            var series2 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "BM Value",
                Values = new LiveCharts.ChartValues<double>(ResultBM_List)
            };

            resultChart.Series.Clear();
            resultChart.Series.Add(series1);
            resultChart.Series.Add(series2);
        }
    }

    public class CalcValue
    {
        public int Interval_Value { get; set; }
        public double SF_Value { get; set; }
        public double BM_Value { get; set; }
    }
}

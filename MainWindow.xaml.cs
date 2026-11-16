using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Globalization;

namespace ProdejVozu
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NacistXML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "XML soubory (*.xml)|*.xml";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Načtení XML dokumentu
                    XDocument dokument = XDocument.Load(dialog.FileName);

                    List<Vuz> vozidla = new List<Vuz>();

                    // Načtení jednotlivých vozidel
                    foreach (XElement prvek in dokument.Root.Elements("Vuz"))
                    {
                        Vuz v = new Vuz();

                        v.Model = (string)prvek.Element("Model");

                        v.DatumProdeje = DateTime.ParseExact(
                            (string)prvek.Element("DatumProdeje"),
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture
                        );

                        v.Cena = (double)prvek.Element("Cena");
                        v.DPH = (double)prvek.Element("DPH");

                        vozidla.Add(v);
                    }

                    // Zobrazení všech vozidel v tabulce
                    dgVozidla.ItemsSource = vozidla;

                    // Výběr pouze víkendových prodejů
                    var vikendoveProdeje = vozidla.Where(v =>
                        v.DatumProdeje.DayOfWeek == DayOfWeek.Saturday ||
                        v.DatumProdeje.DayOfWeek == DayOfWeek.Sunday
                    );

                    // Seskupení podle modelu a výpočet cen
                    var vysledky = vikendoveProdeje
                        .GroupBy(v => v.Model)
                        .Select(g => new Vysledek
                        {
                            NazevModelu = g.Key,

                            CenaBezDPH = g.Sum(v => v.Cena),

                            CenaSDPH = g.Sum(v =>
                                v.Cena * (1 + v.DPH / 100.0)
                            )
                        })
                        .OrderBy(v => v.NazevModelu)
                        .ToList();

                    // Zobrazení výsledků
                    dgVysledky.ItemsSource = vysledky;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Chyba při načítání XML souboru:\n" + ex.Message,
                        "Chyba",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }
    }
}

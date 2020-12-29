using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            try
        {
            MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");
             MySqlCommand cmd = new MySqlCommand("Select * from usuarios where empleado_id = '" + username.Text + "'  and password = '" + password.Password + "'", c);
              MySqlDataReader lectura;
              c.Open();
              lectura = cmd.ExecuteReader();
              int count = 0;
              bool isAdmin = false;
              while (lectura.Read()) {
                  count = count + 1;
                  isAdmin = (lectura[2].Equals( 1)) ? true : false;
              }
              if (count == 1)
              {
                  
                  if( isAdmin ) {
                       MainWindow win2 = new MainWindow();
                        win2.Show();
                    }
                    else {
                        Admin win3 = new Admin();
                        win3.Show();
                    }
                  c.Close();     
                  this.Close();
              }
              else
              {
                  c.Close();
                MessageBox.Show("Datos erróneos");
            }

            c.Close();
        }catch(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        }
    }
}

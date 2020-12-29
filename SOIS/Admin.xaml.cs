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
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Window
    {
        public Admin()
        {
            InitializeComponent();
            FillGrid();
            admin1.Visibility = Visibility.Hidden;
            empleado1.Visibility = Visibility.Hidden;
            try
            {
                MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");
                string command = "select * from info_usuarios";
                MySqlCommand cmd = new MySqlCommand(command, c);
                c.Open();
                MySqlDataReader mdr = cmd.ExecuteReader();
                while (mdr.Read())
                {
                    zonecb.Items.Add(mdr.GetString("empleado_ID"));
                    comboBox.Items.Add(mdr.GetString("empleado_ID"));
                }
                c.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
            int lvl=0;
            if(empleado.IsChecked == true)
            {
                lvl = 1;
            }else if(admin.IsChecked == true)
            {
                lvl = 2;
            }
            try
            {
                MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");
                string Query = "insert into usuarios values('" + this.id_empleado.Text + "','" + this.password.Text + "'," + lvl + ");";
                MySqlCommand cmd = new MySqlCommand(Query, c);
                MySqlDataReader lectura;
                c.Open();
                lectura = cmd.ExecuteReader();

                while (lectura.Read())
                {
                }
                c.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


            try
            {
                MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");
                string Query2 = "insert into info_usuarios values('" + this.id_empleado.Text + "','" + this.nombre.Text + "','" + this.direccion.Text + "','" + this.telefono.Text + "');";
                MySqlCommand cmd2 = new MySqlCommand(Query2, c);
                MySqlDataReader lectura2;
                c.Open();
                lectura2 = cmd2.ExecuteReader();
                MessageBox.Show("Datos almacenados");
                while (lectura2.Read())
                {
                }
                c.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void editar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(editar.SelectedIndex== 4)
            {
                admin1.Visibility = Visibility.Visible;
                empleado1.Visibility = Visibility.Visible;
                campo.Visibility = Visibility.Hidden;
            }
            else
            {
                admin1.Visibility = Visibility.Hidden;
                empleado1.Visibility = Visibility.Hidden;
                campo.Visibility = Visibility.Visible;
            }
        }




        public void FillGrid()
        {
          string sql = "SELECT empleado_ID, nombre, direccion ,telefono FROM info_usuarios";

            using (MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; "))
            {
                c.Open();
                using (MySqlCommand cmdSel = new MySqlCommand(sql, c))
                {
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmdSel);
                    da.Fill(dt);
                    dataGrid1.ItemsSource = dt.DefaultView;
                }
                c.Close();
            }
        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
            try
            {
                MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");
                if(editar.SelectedIndex == 0)
                {
                    string command = "update usuarios set password='" + this.campo.Text + "'"+ "where empleado_ID='" + zonecb.Text + "';";
                    MySqlCommand cmd = new MySqlCommand(command, c);
                    c.Open();
                    MySqlDataReader mdr = cmd.ExecuteReader();
                    while (mdr.Read())
                    {
                       
                    }
                    MessageBox.Show("Contraseña cambiada");
                    c.Close();
                }
                else if (editar.SelectedIndex == 1)
                {
                    string command = "update info_usuarios set nombre='" + this.campo.Text + "'" + "where empleado_ID='" + zonecb.Text + "';";
                    MySqlCommand cmd = new MySqlCommand(command, c);
                    c.Open();
                    MySqlDataReader mdr = cmd.ExecuteReader();
                    while (mdr.Read())
                    {

                    }
                    MessageBox.Show("Nombre cambiado");
                    c.Close();
                    FillGrid();
                }else if(editar.SelectedIndex == 2)
                {

                    string command = "update info_usuarios set direccion='" + this.campo.Text + "'" + "where empleado_ID='" + zonecb.Text + "';";
                    MySqlCommand cmd = new MySqlCommand(command, c);
                    c.Open();
                    MySqlDataReader mdr = cmd.ExecuteReader();
                    while (mdr.Read())
                    {

                    }
                    MessageBox.Show("Dirección cambiada");
                    c.Close();
                    FillGrid();
                }else if(editar.SelectedIndex == 3)
                {
                    string command = "update info_usuarios set telefono='" + this.campo.Text + "'" + "where empleado_ID='" + zonecb.Text + "';";
                    MySqlCommand cmd = new MySqlCommand(command, c);
                    c.Open();
                    MySqlDataReader mdr = cmd.ExecuteReader();
                    while (mdr.Read())
                    {

                    }
                    MessageBox.Show("Teléfono cambiado");
                    c.Close();
                    FillGrid();
                }else if(editar.SelectedIndex == 4)
                {
                    int lvl2 = 0;
                    if (empleado1.IsChecked == true)
                    {
                        lvl2 = 1;
                    }
                    else if (admin1.IsChecked == true)
                    {
                        lvl2 = 2;
                    }
                    string command = "update usuarios set nivel=" + lvl2 + " where empleado_ID='" + zonecb.Text + "';";
                    MySqlCommand cmd = new MySqlCommand(command, c);
                    c.Open();
                    MySqlDataReader mdr = cmd.ExecuteReader();
                    while (mdr.Read())
                    {

                    }
                    MessageBox.Show("Permisos cambiados");
                    c.Close();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
            comboBox.Items.Clear();
            try {
                MySqlConnection c = new MySqlConnection("Server=localhost; database=SOIS; UID=root; Pwd=; ");

                string command = "delete from usuarios where usuarios.empleado_ID='" + comboBox.Text + "';";
                MySqlCommand cmd = new MySqlCommand(command, c);
                c.Open();
                MySqlDataReader mdr = cmd.ExecuteReader();
                while (mdr.Read())
                {

                }
                c.Close();

                string command2 = "delete from info_usuarios where info_usuarios.empleado_ID='" + comboBox.Text + "';";
                MySqlCommand cmd2 = new MySqlCommand(command2, c);
                c.Open();
                MySqlDataReader mdr2 = cmd2.ExecuteReader();
                while (mdr2.Read())
                {

                }
                

                MessageBox.Show("Usuario eliminado");
                comboBox.Items.Clear();

                c.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            FillGrid();
        }
    }
}

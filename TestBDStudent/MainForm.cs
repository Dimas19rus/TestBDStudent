using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace TestBDStudent
{
    public partial class MainForm : Form
    {
        string connectionStr = "Host=localhost; Username=Dmitriy; Password=12345678; Port=5432; Database=testbase";
        public List<Student> Students { get; set; }

        public Student SelectedStudent { get; set; }


        public MainForm()
        {
            InitializeComponent();
            Students = new List<Student>();
        }

        private void buttonAddStudent_Click(object sender, EventArgs e)
        {
            Student student = new Student
            {
                Name = textBoxName.Text, Surname = textBoxSurname.Text, Patronomic = textBoxPatronomic.Text,
                Group = textBoxGroup.Text
            };

            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();
                string insertQuery =
                    $"INSERT INTO student( name, patronomic, surname, \"group\") VALUES (\'{student.Name}\', \'{student.Patronomic}\', \'{student.Surname}\', \'{student.Group}\');";

                var cmd = new NpgsqlCommand(insertQuery, conn);
                cmd.ExecuteNonQuery();

                Students.Clear();
                GetListStudent(conn);
                UpdateListView();

                cmd.Connection.Close();
            }
        }

        private void buttonContinentList_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();
                Students.Clear();
                GetListStudent(conn);
                UpdateListView();
            }
        }

        private void GetListStudent(NpgsqlConnection conn)
        {
            using (var command = new NpgsqlCommand("SELECT * FROM student", conn))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Students.Add(new Student
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Patronomic = reader.GetString(2),
                        Surname = reader.GetString(3),
                        Group = reader.GetString(4)
                    });
                }

                reader.Close();
            }
        }

        private void UpdateListView()
        {
            listView1.Items.Clear();
            foreach (Student student in Students)
            {
                listView1.Items.Add(new ListViewItem(new string[]
                    {student.Name, student.Patronomic, student.Surname, student.Group}));
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 1)
            {
                FillDataUpdate();
            }
            else
            {
                CleaningDataUpdate();
            }

            if (listView1.SelectedIndices.Count == 0)
            {
                buttonDelete.Enabled = false;
            }
            else
            {
                buttonDelete.Enabled = true;
            }
        }

        private void CleaningDataUpdate()
        {
            textBoxUpdateName.Text = "";
            textBoxUpdatePatronomic.Text = "";
            textBoxUpdateSurname.Text = "";
            textBoxUpdateGroup.Text = "";
            button1.Enabled = false;
            SelectedStudent = null;
        }

        private void FillDataUpdate()
        {
            int idList = listView1.SelectedIndices[0];
            SelectedStudent = Students[idList];
            textBoxUpdateName.Text = Students[idList].Name;
            textBoxUpdatePatronomic.Text = Students[idList].Patronomic;
            textBoxUpdateSurname.Text = Students[idList].Surname;
            textBoxUpdateGroup.Text = Students[idList].Group;
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand(
                    $"UPDATE student SET name=\'{textBoxUpdateName.Text}\', patronomic=\'{textBoxUpdatePatronomic.Text}\', surname=\'{textBoxUpdateSurname.Text}\', \"group\"=\'{textBoxUpdateGroup.Text}\' WHERE id={SelectedStudent.ID}",
                    conn))
                {
                    command.ExecuteNonQuery();
                    listView1.SelectedIndices.Clear();
                }

                Students.Clear();
                GetListStudent(conn);
                UpdateListView();
                
            }

        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connectionStr))
            {
                conn.Open();
                string str = "";
                foreach (int idList in listView1.SelectedIndices)
                {
                    str += Students[idList].ID + ", ";
                }

                str = "(" + str.Remove(str.Length - 2) + ")";
                using (var command = new NpgsqlCommand($"DELETE FROM student WHERE id IN " + str, conn))
                {
                    command.ExecuteNonQuery();
                }

                Students.Clear();
                GetListStudent(conn);
                UpdateListView();
            }
        }
    }
}